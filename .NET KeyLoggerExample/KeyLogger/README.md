# Key Logger Example

> **Educational purpose only**
>
> This project is intended for learning how Windows keyboard input can be observed at the API level. Use it only on systems you own or control, with explicit permission, and in a controlled lab or classroom environment.
>
> Do **not** deploy this software to monitor other people without their knowledge and consent. Unauthorized keylogging may violate law and policy. The authors provide this code for study only and accept no responsibility for misuse.

## Overview

This solution demonstrates two common techniques for detecting keyboard activity on Windows:

1. **Low-level keyboard hook** (`WH_KEYBOARD_LL`)
2. **Polling with `GetAsyncKeyState`**

Both approaches are implemented as separate class libraries. A small WinForms application shows their output side by side.

## Solution structure

```
KeyLogger/
├── KeyLogger.sln              # Visual Studio solution
├── KeyLogger.csproj           # WinForms demo application
├── frmMain.cs                 # UI that displays captured input
├── KeyLogger.Hook/            # Hook-based library
└── KeyLogger.Poll/            # Polling-based library
```

| Project | Type | Description |
|---------|------|-------------|
| **KeyLogger** | WinExe | Demo UI with two text panels |
| **KeyLogger.Hook** | Class library | Global keyboard hook monitor |
| **KeyLogger.Poll** | Class library | Timer-based key state polling |

## KeyLogger (WinForms app)

The main form runs both libraries when it opens and stops them when it closes.

- **Top panel (`txtKeys`)** — output from `KeyLogger.Hook`, one line per key event
- **Bottom panel (`txtPollKeys`)** — output from `KeyLogger.Poll`, typed text built character by character

Example hook output:

```text
DOWN A scan=30 alt=false
UP   A scan=30 alt=false
```

Example poll output:

```text
hello world
```

## KeyLogger.Hook

Uses the Windows low-level keyboard hook API (`SetWindowsHookEx` with `WH_KEYBOARD_LL`).

### How it works

1. Installs a global hook in the current process.
2. Windows calls the hook procedure when a key is pressed or released anywhere on the desktop (same user session).
3. The library reads virtual-key code, scan code, and modifier state.
4. It raises a `KeyDetected` event with `KeyDetectedEventArgs`.

### Main types

| Type | Purpose |
|------|---------|
| `GlobalKeyboardHook` | Installs/removes the hook; exposes `Start()`, `Stop()`, `Dispose()` |
| `KeyDetectedEventArgs` | Event data and `ToDisplayLine()` formatting |
| `NativeMethods` | P/Invoke declarations for `user32.dll` |

### Notes

- Must run on a thread with a Windows message loop (for example the WinForms UI thread).
- Antivirus and EDR tools often flag keyboard hooks because they are also used by malware.
- Does not require polling; events are delivered by the OS.

This approach is similar to the Lazarus/Delphi low-level hook example discussed in class.

## KeyLogger.Poll

Uses repeated calls to `GetAsyncKeyState`, similar to classic Delphi polling examples that loop through virtual keys 0–255.

### How it works

1. A timer fires every 10 ms (configurable via `PollIntervalMilliseconds`).
2. For each virtual key from `0` to `255`, the library checks `(GetAsyncKeyState(vk) & 1)`.
3. Bit 0 indicates the key transitioned since the last call.
4. Left mouse button (`VK_LBUTTON`, code 1) is skipped, as in the original Delphi sample.
5. The virtual key is mapped to display text through `VirtualKeyMap` (like a Delphi `ListView` lookup table).
6. A `KeyDetected` event is raised with `KeyPollEventArgs`.

### Main types

| Type | Purpose |
|------|---------|
| `AsyncKeyStateMonitor` | Timer-based polling; exposes `Start()`, `Stop()`, `Dispose()` |
| `VirtualKeyMap` | Maps VK codes to characters; supports custom mappings via `SetMapping()` |
| `KeyPollEventArgs` | Event data (`VirtualKeyCode`, `Text`, `IsBackspace`) |
| `NativeMethods` | P/Invoke for `GetAsyncKeyState` and `GetKeyState` |

### Notes

- Does not install a system hook; it polls key state instead.
- Uses more CPU than a hook because of the timer loop.
- Can miss very fast key presses if they occur between poll intervals.
- Shift, Caps Lock, and common punctuation are handled in `VirtualKeyMap`.

## Comparison

| | KeyLogger.Hook | KeyLogger.Poll |
|--|----------------|----------------|
| API | `SetWindowsHookEx` | `GetAsyncKeyState` |
| Detection | Event-driven | Timer polling |
| Output style | Raw key events (up/down) | Typed text |
| CPU usage | Lower | Higher |
| Typical AV attention | Higher | Lower |

## Requirements

- Windows
- .NET Framework 4.7.2
- Visual Studio 2019 or later (or MSBuild)

## Build and run

1. Open `KeyLogger.sln` in Visual Studio.
2. Build the solution (**Build → Build Solution**).
3. Run the **KeyLogger** project.
4. Type in Notepad or another application while the demo window is open.
5. Watch both panels update.

If build fails with “Access denied” on `KeyLogger.exe`, close any running instance of the app and rebuild.

## Customizing poll key mappings

```csharp
var monitor = new AsyncKeyStateMonitor();
monitor.KeyMap.SetMapping(0x41, "custom-a");
monitor.Start();
```

## Legal and ethical reminder

This sample is for **education only** — for example courses on Windows programming, security awareness, or reverse engineering labs.

- Do not use it to capture passwords, personal messages, or credentials.
- Do not install it on shared or production machines without authorization.
- Always follow your institution’s policies and applicable laws.
