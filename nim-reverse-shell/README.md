# Nim reverse TCP client (`program.nim`)

Nim TCP client that connects to a configurable host/port, reads one line per message from the server, runs each line via `cmd.exe /c …` on Windows (hidden console), sends command output back on the socket, and closes when it receives **`exit`** (case-insensitive, stripped).

## Requirements

- [Nim](https://nim-lang.org/) toolchain (e.g. 2.x)
- Windows (uses `cmd.exe`)

## Configure

Edit the constants near the top of `program.nim`:

- **`SERVER_IP`** — address of your server
- **`SERVER_PORT`** — port (e.g. `Port(5000)`)

## Build & run

```bash
nim c program.nim
./program.exe
```

## Behaviour summary

| Input from server | Action |
|-------------------|--------|
| Non-empty line    | Execute as shell command; stream stdout/stderr to client and server |
| `exit`           | Disconnect and quit |
| Empty line       | Ignored |

## Disclaimer

**This project is strictly for educational purposes.**
