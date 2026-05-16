# Process Hollowing (Educational)

## Educational purpose only

This repository demonstrates **process hollowing** (sometimes called **RunPE**) on Windows for **learning and research** only.

- Use only on systems and software you **own** or are **explicitly authorized** to test.
- Do **not** use this code to evade security controls, distribute malware, or attack third parties.
- Techniques shown here are **flagged by antivirus/EDR** and may violate organizational policy or law if misused.
- The authors and contributors assume **no liability** for misuse.

By building or running this project, you agree to use it responsibly and in compliance with applicable laws and policies.

---

## What this project does (high level)

The loader:

1. Starts a **benign host executable** (`source`) in a **suspended** state.
2. Reads a **second PE file** (`target`) from disk into memory.
3. **Unmaps** the host’s original image (when bases match), **allocates** memory in the remote process, and **writes** the target PE (headers + sections).
4. Updates the **thread context** and **PEB** so execution continues in the injected image.
5. **Resumes** the main thread.

The visible process name/path may still look like `source`, while the code that runs can come from `target`. That mismatch is why this pattern is studied in malware analysis and defensive security courses.

---

## Requirements

- Windows
- .NET Framework 4.7.2 (see `PELoader.csproj`)
- **Source and target must be the same architecture** (both x86 or both x64)
- **64-bit payloads** require a **64-bit PHLoader** build (`x64` or **Any CPU** with **Prefer 32-bit** unchecked)
- **32-bit payloads** can run from an **x86** or **x64** PHLoader (x64 uses `Wow64*` APIs for the child thread)

---

## Build and run

```text
PHLoader.exe <source.exe> <target.exe>
```

| Argument   | Role |
|-----------|------|
| `source`  | Legitimate executable path used to create the suspended process (e.g. `C:\Windows\System32\notepad.exe`) |
| `target`  | PE file on disk whose bytes are written into the remote process |

Examples (paths must exist on your machine):

**32-bit (x86) host + payload:**

```text
PHLoader.exe C:\Windows\SysWOW64\notepad.exe C:\path\to\payload-x86.exe
```

**64-bit host + payload:**

```text
PHLoader.exe C:\Windows\System32\notepad.exe C:\path\to\payload-x64.exe
```

Build **x64** or **Any CPU** (64-bit) for 64-bit targets. Do not use `System32\notepad.exe` with a 32-bit-only PHLoader build against a 64-bit payload.

On failure, the loader prints the exception and attempts to **terminate** the created process.

---

## Architecture overview

```mermaid
flowchart LR
  A[Read target PE bytes] --> B[CreateProcess suspended]
  B --> C[Parse PE headers]
  C --> D[GetThreadContext]
  D --> E[Read PEB ImageBase]
  E --> F[NtUnmapViewOfSection optional]
  F --> G[VirtualAllocEx]
  G --> H[WriteProcessMemory headers and sections]
  H --> I[Patch PEB and EIP]
  I --> J[SetThreadContext]
  J --> K[ResumeThread]
```

---

## Windows API declarations (`PELoader.cs`)

These are **P/Invoke** imports: managed C# calls into native DLLs. `[SuppressUnmanagedCodeSecurity]` skips a legacy CAS check (common in older samples; modern code often omits it).

### `kernel32.dll`

| Function | Purpose in this project |
|----------|-------------------------|
| **`CreateProcess`** (`CreateProcessW`) | Launches `source` with flag `CREATE_SUSPENDED` so the primary thread does not run user code until resumed. Fills `PROCESS_INFORMATION` with process/thread handles and IDs. |
| **`GetThreadContext`** | On a **32-bit** loader, reads the suspended thread’s CPU context (registers) into a buffer. Used to obtain **EBX** (points near the PEB) and later set **EIP**. |
| **`Wow64GetThreadContext`** | On a **64-bit** loader, reads context for a **32-bit (WoW64)** thread in the child process. |
| **`SetThreadContext`** / **`Wow64SetThreadContext`** | Writes the modified context back (notably **EIP** → new entry point). |
| **`ReadProcessMemory`** | Reads 4 bytes from the child at `EBX + 8`, the **PEB `ImageBaseAddress`** field on 32-bit Windows. |
| **`WriteProcessMemory`** | Copies PE headers, section bytes, and the new image base into the child address space. |
| **`VirtualAllocEx`** | Reserves/commits memory in the remote process (`MEM_COMMIT \| MEM_RESERVE`) with **`PAGE_EXECUTE_READWRITE`** for the size `SizeOfImage`. |
| **`ResumeThread`** | Starts the suspended thread after injection. Return value `-1` indicates failure. |

### `ntdll.dll`

| Function | Purpose in this project |
|----------|-------------------------|
| **`NtUnmapViewOfSection`** | Low-level unmap of the **original** executable mapping in the child when the loaded base matches the payload’s preferred **`ImageBase`**. Success is **`STATUS_SUCCESS` (0)**. |

### Supporting structures

| Type | Role |
|------|------|
| **`StartupInformation`** | Marshalled `STARTUPINFO`-like block; `Size` must be set before `CreateProcess`. |
| **`ProcessInformation`** | Receives `hProcess`, `hThread`, and process/thread IDs after creation. |

> **Note:** This sample marshals thread context as `int[]` with hard-coded indices (`ContextIndexEbx`, `ContextIndexEip`). Production or portable code should use the Win32 **`CONTEXT`** / **`WOW64_CONTEXT`** structures.

---

## `Execute` method — step by step

Implementation: `PELoader.Execute(string source, string target)`.

### 1. Load the payload file

```csharp
byte[] data = File.ReadAllBytes(target);
```

Reads the **entire** `target` PE into a byte array. All header parsing and section copies use this buffer.

### 2. Create the host process (suspended)

```csharp
CreateProcess(source, args, ..., CREATE_SUSPENDED, ..., ref si, ref pi);
```

- **`source`**: path to the decoy/host executable.
- **`CREATE_SUSPENDED` (`0x4`)**: process is created; initial thread is paused.
- **`pi.ProcessHandle` / `pi.ThreadHandle`**: used for all subsequent memory and thread operations.

### 3. Parse PE headers from `data`

| Step | Code concept | Meaning |
|------|----------------|---------|
| NT headers location | `BitConverter.ToInt32(data, 0x3C)` | DOS header **`e_lfanew`** → offset to `PE` signature |
| Preferred base | optional header **`ImageBase`** | Where the payload wants to be mapped |
| Entry point RVA | **`AddressOfEntryPoint`** | Added to mapped base later for **EIP** |
| **`SizeOfImage`** | Total virtual size for allocation |
| **`SizeOfHeaders`** | Bytes to copy starting at PE layout base |
| Section count | **`NumberOfSections`** | Loop bound for section headers |

Constants at the top of `PELoader.cs` map these to documented **`IMAGE_*`** offsets for **PE32**.

### 4. Capture thread context

```csharp
context[0] = CONTEXT_INTEGER;
GetThreadContext / Wow64GetThreadContext(pi.ThreadHandle, context);
int ebx = context[ContextIndexEbx];
```

- Requests integer registers for x86.
- **`ebx`** is used as a pointer into the child’s **PEB** (sample-specific index into the marshalled array).

Branching uses `IntPtr.Size == 4` (32-bit CLR) vs 64-bit CLR with WoW64 APIs.

### 5. Read the image base the loader actually used

```csharp
ReadProcessMemory(pi.ProcessHandle, ebx + PebImageBaseOffset, ref loadedImageBase, 4, ...);
```

Reads the **loaded image base** from the child PEB (`PebImageBaseOffset = 8` on 32-bit).

### 6. Unmap the original section (conditional)

```csharp
if (imageBase == loadedImageBase)
    NtUnmapViewOfSection(pi.ProcessHandle, loadedImageBase);
```

If the payload’s preferred base matches what the host already mapped, the original image view is unmapped before writing the new image.

### 7. Allocate remote memory

```csharp
VirtualAllocEx(pi.ProcessHandle, imageBase, sizeOfImage, MEM_COMMIT | MEM_RESERVE, PAGE_EXECUTE_READWRITE);
```

Allocates a region sized **`SizeOfImage`** at the preferred **`imageBase`**. Failure (`newImageBase == 0`) aborts.

### 8. Write headers and sections

```csharp
WriteProcessMemory(..., newImageBase, data, sizeOfHeaders, ...);
```

Copies PE headers to the remote base.

For each **section header**:

- **`VirtualAddress`**: RVA where the section is mapped relative to `newImageBase`.
- **`SizeOfRawData` / `PointerToRawData`**: raw bytes in `data` to copy.

```csharp
WriteProcessMemory(..., newImageBase + virtualAddress, sectionData, ...);
```

Skips sections with zero raw size.

### 9. Patch PEB image base

```csharp
WriteProcessMemory(pi.ProcessHandle, ebx + PebImageBaseOffset, BitConverter.GetBytes(newImageBase), 4, ...);
```

Updates **`PEB.ImageBaseAddress`** so the process sees the new module base.

### 10. Set instruction pointer to the new entry point

```csharp
addressOfEntryPoint = ... // RVA from optional header
context[ContextIndexEip] = newImageBase + addressOfEntryPoint;
SetThreadContext / Wow64SetThreadContext(...);
```

**EIP** (via the sample’s context slot) points to the payload’s entry point in the remote address space.

### 11. Resume execution

```csharp
ResumeThread(pi.ThreadHandle);
```

The primary thread runs the injected code.

### 12. Error handling

On any exception, the sample logs the error, resolves the child by **`ProcessId`**, and calls **`Kill()`**. Returns `false` on failure, `true` on success.

---

## Limitations of this sample

| Topic | Detail |
|-------|--------|
| Bitness | **32-bit loader** cannot inject **64-bit** PEs; 64-bit loader supports both via native / WoW64 context APIs |
| PEB register | x64 path assumes **RDX** points at the PEB for a suspended process (typical on current Windows) |
| Security | No authenticity checks, no ASLR handling beyond chosen base, broad **RWX** mapping |
| Detection | Behavior matches classic **process hollowing** signatures |

For production or defensive tooling, use documented structures, proper error codes, and authorized test environments.

---

## Further reading

- Microsoft PE format: [PE Format - Win32 apps](https://learn.microsoft.com/en-us/windows/win32/debug/pe-format)
- `CreateProcess`: [CreateProcessW function](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-createprocessw)
- Thread context: [GetThreadContext function](https://learn.microsoft.com/en-us/windows/win32/api/processthreadsapi/nf-processthreadsapi-getthreadcontext)
- Process hollowing (defensive perspective): search for “process hollowing MITRE” and malware analysis lab material

---