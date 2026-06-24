# Portable Executable API Checker

> **Educational purpose only**
>
> This tool is for learning how Windows executables declare dependencies on system APIs through the PE import table. Use it only on files you are allowed to analyze (your own programs, lab samples, or files with explicit permission).

## Overview

**PE API Checker** is a Windows Forms application that reads a `.exe` file and lists every Windows API function imported by that executable. For each import, the app shows a short description and lets you open Microsoft documentation with a double-click.

Static import analysis shows which APIs a program *can* call. It does not show when or why those APIs are used at runtime.

## Features

- Open and analyze **32-bit and 64-bit PE executables** (`.exe` only)
- Parse the PE **import directory** and list imports as `DLL!FunctionName`
- Show a **description** for each API when available
- Fall back to a **DLL category** when a function is not in the local database
- **Double-click** a row to search [Microsoft Learn](https://learn.microsoft.com/) for that API

## How it works

1. You select an `.exe` file.
2. `PeImportReader` reads the PE headers and import table from disk.
3. Each imported function is matched against `ApiDescriptions.json`.
4. If no match exists, `DllCategoryResolver` provides a general category based on the DLL name (for example `USER32.dll` → user interface APIs).
5. Results appear in a two-column list: **Import** and **Description**.

## Project structure

```
PEAPIChecker/
├── PEAPIChecker.sln          # Visual Studio solution
├── PEAPIChecker.csproj       # WinForms application
├── Program.cs                # Application entry point
├── frmMain.cs                # Main UI
├── PeImportReader.cs         # PE import table parser
├── PeImportEntry.cs          # Import record (DLL + function name)
├── ApiDescriptionService.cs  # JSON lookup and documentation URLs
├── DllCategoryResolver.cs    # Fallback descriptions by DLL
├── ApiDescriptions.json      # Local API description database
└── Properties/               # Assembly and designer resources
```

## Example output

| Import | Description |
|--------|-------------|
| `KERNEL32.dll!CreateFileW` | Creates or opens a file, directory, device, or other I/O resource. |
| `USER32.dll!MessageBoxW` | Displays a modal dialog box with text, caption, and buttons. |
| `UNKNOWN.dll!SomeRareApi` | `[Imported from UNKNOWN.dll]` |

## Extending API descriptions

Edit `ApiDescriptions.json` and add entries by **function name**:

```json
{
  "MyFunctionW": "Short plain-language description of what the API does."
}
```

Rebuild the project so `ApiDescriptions.json` is copied to the output folder (`bin\Debug` or `bin\Release`).

## Requirements

- Windows
- .NET Framework 4.7.2
- Visual Studio 2019 or later (or MSBuild)

## Build and run

1. Open `PEAPIChecker.sln` in Visual Studio.
2. Build the solution (**Build → Build Solution**).
3. Run the **PEAPIChecker** project.
4. Click **Select EXE File** and choose a Windows executable.
5. Review imports in the list; double-click a row to open documentation in your browser.

## Limitations

- **Imports only** — delay-loaded imports and runtime `GetProcAddress` calls may not appear in the main import table.
- **Ordinal imports** — shown as `Ordinal_N`; documentation links use a generic search.
- **Description coverage** — `ApiDescriptions.json` covers common APIs; most executables will also show DLL category fallbacks.
- **Not a malware sandbox** — do not run unknown executables unless you understand the risk.

## Legal and ethical reminder

Use this project for **education and authorized analysis** only. Do not use it to inspect software or systems without permission.
