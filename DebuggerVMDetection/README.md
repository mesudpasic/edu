# Debugger & VM Detection (C++)

Educational demo that builds a Windows DLL (`detect.dll`) with classic techniques for detecting **debuggers** and **virtual machines**. 

## What it does

The DLL exposes a small C API. Your host application (native C/C++, .NET, etc.) loads `detect.dll` and calls:

| Export | Description |
|--------|-------------|
| `IsUnderAnyVM()` | Returns `true` if any supported VM/sandbox is detected. |
| `IsAnyDebuggerFound()` | Returns `true` if any debugger check fires. |
| `NotifyVMPresence()` | Shows a `MessageBox` naming the detected hypervisor (or “none”). |
| `CrashMe()` | Deliberately crashes the process via `KillMe()` (null write). |

Aggregated logic lives in `dllmain.cpp`:

- **VM:** `IsInsideVPC()` OR `IsInsideHyperV()` OR `IsInsideVMWare()` OR `IsInsideVirtualBox()`
- **Debugger:** `IsDbgPresentPrefixCheck()` OR `Int2DCheck()` OR `CanOpenCsrss()` OR `MemoryBreakpointDebuggerCheck()` OR `DetectFamousDebuggers()` OR `CheckProcessDebugFlags()`

## Project layout

```
DebuggerVMDetection/
├── SandboxVMDetection.sln   # Visual Studio solution
├── detect.vcxproj           # DLL project (output: detect.dll)
├── dllmain.cpp              # Exported API and DllMain
├── detect.h                 # Export declarations, KillMe()
├── antidbg.h                # Debugger detection methods
└── vmcheck.h                # VM / hypervisor detection methods
```

## Building

**Requirements:** Windows, Visual Studio 2017 or later (project uses toolset **v141**), Windows 10 SDK.

1. Open `SandboxVMDetection.sln` in Visual Studio.
2. Select configuration **Debug** or **Release**.
3. Prefer platform **Win32 (x86)** — most checks use **inline x86 assembly** (`__asm`) in `antidbg.h` and `vmcheck.h`. An **x64** build may fail or need refactoring to intrinsics / separate asm files.
4. Build the **detect** project. The DLL is written under `Debug/` or `Release/` (e.g. `Win32\Release\detect.dll`).

Copy `detect.dll` next to your test executable (or add its folder to `PATH`).

## Detection techniques

### Virtual machines (`vmcheck.h`)

| Function | Idea |
|----------|------|
| `IsInsideVPC()` | Microsoft Virtual PC backdoor port sequence (`0F 3F 07 0B`) with SEH. |
| `IsInsideVMWare()` | VMware I/O port `VX` / magic `VMXh`. |
| `IsInsideHyperV()` | CPUID hypervisor bit + vendor string (`Microsoft Hv`, `Hyper-V`). |
| `IsInsideVirtualBox()` | `VBoxHook.dll` load and/or `\\.\VBoxMiniRdrDN` device path. |

`GetCpuID()` reads the CPU vendor string via `CPUID` for Hyper-V identification.

### Debuggers (`antidbg.h`)

| Function | Idea |
|----------|------|
| `IsDbgPresentPrefixCheck()` | Prefix/`INT 1` sequence; debuggers often step over it, bare metal hits SEH. |
| `Int2DCheck()` | `INT 2D` behavior differs with a debugger attached. |
| `CanOpenCsrss()` | Opening `csrss.exe` with `PROCESS_ALL_ACCESS` (often possible when debug privileges are in play). |
| `MemoryBreakpointDebuggerCheck()` | Guard page + `RET` trick aimed at OllyDbg-style memory breakpoints. |
| `DetectFamousDebuggers()` | `EnumWindows` for known debugger window titles/classes (strings XOR-obfuscated). |
| `CheckProcessDebugFlags()` | `NtQueryInformationProcess` / `ProcessDebugFlags`. |

Additional helpers in the same header (not wired into `IsAnyDebuggerFound()` by default) include `HideThread`, `CheckOutputDebugString`, `GetProcessIdFromName`, and `ErasePEHeaderFromMemory`.

## Using from C/C++

```cpp
#include "detect.h"

// Link against the import lib if you use one, or LoadLibrary + GetProcAddress.

if (IsUnderAnyVM()) {
    // running in a VM
}

if (IsAnyDebuggerFound()) {
    // debugger likely present
}

NotifyVMPresence();  // UI demo: which VM?
```

## Using from .NET (P/Invoke)

Match the DLL bitness to your app (typically **x86** for this project). Place `detect.dll` beside the executable.

```csharp
using System.Runtime.InteropServices;

public static class DetectNative
{
    [DllImport("detect.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern bool IsUnderAnyVM();

    [DllImport("detect.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern bool IsAnyDebuggerFound();

    [DllImport("detect.dll", CallingConvention = CallingConvention.StdCall)]
    public static extern void NotifyVMPresence();
}

// Example:
// if (DetectNative.IsAnyDebuggerFound())
//     MessageBox.Show("Debugger found");
// else
//     MessageBox.Show("Debugger not found");

// DetectNative.NotifyVMPresence();
```

## Testing the demo

1. **Bare metal:** Build Release Win32, run a small host that calls the exports—expect “no VM” and usually “no debugger” when nothing is attached.
2. **VM:** Run inside VirtualBox, VMware, Hyper-V, or legacy VPC and call `NotifyVMPresence()` or `IsUnderAnyVM()`.
3. **Debugger:** Attach x64dbg, WinDbg, OllyDbg (x86), or Visual Studio debugger and call `IsAnyDebuggerFound()`.

Results vary by OS version, hypervisor settings, and whether checks are patched or bypassed.

## Limitations (important)

- **Educational only:** Modern malware analysts and reversers expect these patterns; many are trivial to patch, hook, or emulate.
- **False positives/negatives:** Hyper-V on Windows, nested virtualization, WSL, and current Windows security features can skew results.
- **Legacy APIs:** Some calls (e.g. `GetVersionEx`, opening `csrss`) reflect older research code.
- **Platform:** Win32-focused due to inline assembly; x64 and ARM are not supported without porting.
- **`CrashMe()`:** Intentionally undefined behavior—use only in controlled lab scenarios.
