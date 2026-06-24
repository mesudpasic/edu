Set WshShell = CreateObject("WScript.Shell")

' Desktop path
DesktopPath = WshShell.SpecialFolders("Desktop")

' Create shortcut
Set Shortcut = WshShell.CreateShortcut(DesktopPath & "\Google.lnk")

' Chrome executable
ExecPath = "powershell.exe"
' Chrome executable
ChromePath = "C:\Program Files\Google\Chrome\Application\chrome.exe"
' Open website in Chrome
Shortcut.TargetPath = ExecPath
Shortcut.Arguments = "-Command ""Add-Type -AssemblyName System.Windows.Forms; [System.Windows.Forms.MessageBox]::Show('Hi there','Message')"""

' Chrome icon
Shortcut.IconLocation = ChromePath & ",0"

' Working directory = Desktop
Shortcut.WorkingDirectory = DesktopPath

' Save shortcut
Shortcut.Save