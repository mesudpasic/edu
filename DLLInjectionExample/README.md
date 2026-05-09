# DLL Injection Example (Lazarus / Free Pascal)

> **EDUCATIONAL USE ONLY — FOR ETHICAL HACKERS**
>
> This repository is intended **strictly for educational purposes** and is aimed
> at security students, ethical hackers, malware analysts, and
> defensive engineers who want to understand how classic Windows DLL injection
> works so they can better detect, prevent, and respond to it.
>
> **Do not** use this code, or any derivative of it, against systems, processes,
> or users without **explicit, written authorization**.

---

## What this project demonstrates

This is a minimal, easy-to-read demonstration of the textbook
`CreateRemoteThread` + `LoadLibraryA` Windows DLL injection technique,
implemented in Object Pascal using the [Lazarus IDE](https://www.lazarus-ide.org/)
and the Free Pascal Compiler (FPC).

It is intentionally small and uncomplicated so that the steps of the technique
are easy to follow line-by-line. It is **not** stealthy, **not** hardened, and
**not** obfuscated — it is a teaching artifact.

The repository contains two sub-projects:

| Folder                | Output                | Purpose                                                                 |
| --------------------- | --------------------- | ----------------------------------------------------------------------- |
| `DLLInjection/`       | `dllinjection.dll`    | The payload DLL. Pops a `MessageBox` from `DllMain` on `PROCESS_ATTACH`. |
| `SampleAppLoader/`    | `LoaderApp.exe`       | A small GUI loader that spawns `mspaint.exe` and injects the DLL into it. |

### The injected payload — `DLLInjection`

`dllinjection.dll` is a tiny library whose `DllMain` simply calls `MessageBox`
with the text *"Hello from injected dll!"* when it is loaded into a process.
This makes successful injection visually obvious (the message box appears
**inside the target process**, e.g. MSPaint).

See `DLLInjection/dllinjection.lpr`.

### The loader — `SampleAppLoader`

`LoaderApp.exe` is a Lazarus LCL form with a single button. When the button is
clicked, it performs the following classic injection sequence:

1. `CreateProcess` — launches `mspaint.exe` as the target process.
2. `GetModuleHandle("Kernel32.dll")` + `GetProcAddress("LoadLibraryA")` —
   resolves the address of `LoadLibraryA` (which is at the same address in the
   target process because `kernel32.dll` is mapped at the same base in every
   process on a given session).
3. `OpenProcess(PROCESS_ALL_ACCESS, ...)` — obtains a handle to the target.
4. `VirtualAllocEx` — allocates memory **inside the target process**.
5. `WriteProcessMemory` — writes the string `"dllinjection.dll"` into that
   remote allocation.
6. `CreateRemoteThread` — starts a new thread inside the target process whose
   start routine is `LoadLibraryA` and whose argument is the remote string.
   `LoadLibraryA` then loads our DLL inside the target, which triggers
   `DllMain` and the message box.

See `SampleAppLoader/frmMainUnit.pas` for the full, commented implementation.

## Why this matters for defenders

Understanding this technique is foundational because it (and close variants —
`NtCreateThreadEx`, reflective loading, APC injection, `SetWindowsHookEx`, etc.)
is widely used by real malware families. By building and running a benign
version yourself you can:

- See exactly which Win32 APIs are involved (`OpenProcess`,
  `VirtualAllocEx`, `WriteProcessMemory`, `CreateRemoteThread`, `LoadLibraryA`).
- Practice writing detections in tools like Sysmon, EDR rules, or YARA.
- Observe the technique in dynamic analysis tools (Process Monitor,
  Process Hacker / System Informer, API Monitor, x64dbg).
- Reason about mitigations such as PPL, CIG/ACG, ASR rules, and
  `PROCESS_MITIGATION_BINARY_SIGNATURE_POLICY`.

## Requirements

- **Windows** (x86_64). The technique used here is Windows-specific.
- **[Lazarus IDE](https://www.lazarus-ide.org/)** with **FPC** (Free Pascal
  Compiler). Tested with a current stable Lazarus install.
- Build target: **`x86_64-win64`**. The loader and DLL must match the bitness
  of the target process — `mspaint.exe` is 64-bit on 64-bit Windows, so build
  both projects as 64-bit. Mixing bitness will fail.

## Build

Open each `.lpi` project file in Lazarus and use **Run → Build** (Shift+F9):

1. Open `DLLInjection/dllinjection.lpi` and build → produces `dllinjection.dll`.
2. Open `SampleAppLoader/LoaderApp.lpi` and build → produces `LoaderApp.exe`.

Alternatively, from the command line with `lazbuild`:

```powershell
lazbuild DLLInjection\dllinjection.lpi
lazbuild SampleAppLoader\LoaderApp.lpi
```

## Run

1. Place `dllinjection.dll` in the **current working directory of
   `LoaderApp.exe`** (the loader passes only the file name `dllinjection.dll`
   to `LoadLibraryA`, so the standard DLL search order applies inside the
   target process).
2. Start `LoaderApp.exe`.
3. Click the button on the form.
4. You should see, in order:
   - A confirmation that MSPaint was started (with its PID).
   - A *"DLL injected successfully."* message from the loader.
   - A *"Hello from injected dll!"* `MessageBox` whose **owner is the MSPaint
     process** — confirming the DLL is running inside the target.

## Recommended safe lab environment

Even though the payload here is harmless (just a `MessageBox`), get into the
habit of doing this kind of work in an isolated environment:

- A dedicated Windows VM (Hyper-V, VMware, VirtualBox) with **no shared
  folders and no production credentials**.
- Network isolated or host-only.
- Snapshots before/after each experiment.

## Scope and limits — what this is **not**

- This is **not** a stealthy injector. It uses the most heavily-monitored APIs
  on the platform and will trip essentially every modern EDR.
- It does **not** bypass any mitigations (ACG, CIG, CFG, PPL, ASR, etc.).
- It does **not** target arbitrary processes — by design it spawns and injects
  into a process it created itself (`mspaint.exe`).
- The payload does **nothing harmful** — it only displays a message box.

If you extend this code, keep it that way. Do not weaponize it.

## License & responsible-use statement

This code is published for **education and defensive research only**. By
cloning, building, or running it you agree that:

- You will only use it on systems you own or are **explicitly authorized**
  to test.
- You will comply with all applicable local, national, and international laws.
- You will not use this material to harm, surveil, or gain unauthorized
  access to any system, user, or organization.
