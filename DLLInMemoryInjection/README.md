# DLL In-Memory Injection (Educational Example)

A small Windows demonstration of **loading and executing a DLL entirely from memory**, without writing it to disk. The sample is built with [Free Pascal](https://www.freepascal.org/) and [Lazarus](https://www.lazarus-ide.org/).

> **Disclaimer — educational use only**  
> This project is provided **solely for learning** about PE loading, Windows internals, and defensive security research in controlled environments. Do not use it to bypass security controls, inject code into processes you do not own, or for any unauthorized or malicious purpose. The authors assume no liability for misuse.

## Overview

The repository contains two related projects:

| Project | Role |
|---------|------|
| **DLLDemo** | A minimal DLL that exports `ExecuteDLL` and shows a message box when called. |
| **DLLLoader** | A GUI application that embeds the DLL as an executable resource, loads it from a memory buffer, resolves `ExecuteDLL`, and runs it—without calling `LoadLibrary` on a file path. |

### How it works

1. **DLLDemo** is compiled to `dlldemo.dll`.
2. **DLLLoader** embeds that DLL as an `RCDATA` resource named `DLLDEMO` (see project settings in `dllloader.lpi`).
3. When you click **Load DLL from Memory**, the loader:
   - Reads the embedded bytes into a `TMemoryStream`
   - Passes the buffer to **BTMemoryModule** (`btmemorymodule.pas`), which manually maps the PE image in memory (based on [Joachim Bauch’s memory-module approach](http://www.joachim-bauch.de/tutorials/loading-a-dll-from-memory/))
   - Resolves `ExecuteDLL` via `BTMemoryGetProcAddress` and invokes it
   - Frees the in-memory module with `BTMemoryFreeLibrary`

The loader also includes a helper (`DownloadToMemoryStream`) showing how the same bytes could be fetched from a URL instead of from embedded resources—left as an alternative code path in comments.

## Requirements

- Windows (32-bit target in project files; adjust CPU/OS in Lazarus if you build for Win64)
- [Lazarus IDE](https://www.lazarus-ide.org/) with LCL (tested concepts align with FPC 2.x / Lazarus 4.x style projects)
- Matching bitness: the demo DLL and loader should be built for the same architecture (e.g. both `i386-win32` or both `x86_64-win64`)

## Build instructions

Build in this order:

### 1. Build DLLDemo

1. Open `DLLDemo/dlldemo.lpi` in Lazarus.
2. **Run → Build** (or compile). Output: `DLLDemo/dlldemo.dll` (next to the project or under `lib/<cpu>-<os>/` depending on your Lazarus output settings).

### 2. Embed the DLL in DLLLoader

The loader project references the DLL as a resource:

- Resource name: `DLLDEMO`
- Type: `RCDATA`
- File: configured in `DLLLoader/dllloader.lpi` (currently `..\DLLDemo\dlldemo.dll` relative to the project file)

After building **DLLDemo**, ensure the path in **Project → Project Options → Resources** points to your built `dlldemo.dll`, then rebuild **DLLLoader** so the resource is embedded.

### 3. Build DLLLoader

1. Open `DLLLoader/dllloader.lpi` in Lazarus.
2. **Run → Build**. Output: `dllloader.exe`.

## Usage

1. Run `dllloader.exe`.
2. Click **Load DLL from Memory**.
3. If loading and export resolution succeed, you should see: **Hello from injected DLL!**

If nothing happens, verify that the `DLLDEMO` resource is present (rebuild after fixing the DLL path) and that DLL and EXE use the same platform/bitness.

## Project layout

```
DLLInMemoryInjection/
├── README.md                 (this file)
├── DLLDemo/
│   ├── dlldemo.lpr           # Demo DLL source
│   └── dlldemo.lpi
└── DLLLoader/
    ├── dllloader.lpr         # Program entry
    ├── dllloader.lpi
    ├── loader.pas            # UI and in-memory load logic
    ├── loader.lfm
    └── btmemorymodule.pas    # In-memory PE loader (BTMemoryModule / MPL)
```

## Key source references

- **Load from resource and execute:** `DLLLoader/loader.pas` — `TfrmMain.Button1Click`
- **Exported entry point:** `DLLDemo/dlldemo.lpr` — `ExecuteDLL`
- **Memory mapping:** `DLLLoader/btmemorymodule.pas` — `BTMemoryLoadLibary`, `BTMemoryGetProcAddress`, `BTMemoryFreeLibrary`

## Third-party code

`btmemorymodule.pas` is a Pascal port of the memory-module technique (Joachim Bauch, Martin Offenwanger) and is distributed under the **Mozilla Public License 1.1**. See the file header for copyright and license details.

## Further reading

- [Loading a DLL from memory (Joachim Bauch)](http://www.joachim-bauch.de/tutorials/loading-a-dll-from-memory/)
- Windows PE format and `LoadLibrary` vs. manual mapping (for understanding what this sample omits from the normal loader path)
