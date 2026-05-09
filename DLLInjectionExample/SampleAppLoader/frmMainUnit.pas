unit frmMainUnit;

{$mode objfpc}{$H+}

interface

uses
  Classes, SysUtils, Forms, Controls, Graphics, Dialogs, StdCtrls, Windows;

type

  { TfrmMain }

  TfrmMain = class(TForm)
    Button1: TButton;
    procedure Button1Click(Sender: TObject);
    procedure FormCreate(Sender: TObject);
  private

  public

  end;

var
  frmMain: TfrmMain;

implementation

{$R *.lfm}

{ TfrmMain }

procedure TfrmMain.FormCreate(Sender: TObject);
begin

end;

procedure TfrmMain.Button1Click(Sender: TObject);
var
  dll_in: PAnsiChar = 'dllinjection.dll';
  dll_length: UINT;

  process_handle: THandle;
  remote_thread: THandle;
  remote_buffer: Pointer;

  kernel32_handle: HMODULE;
  lbuffer: Pointer;

  TargetPID: DWORD;
  SI: TStartupInfo;
  PI: TProcessInformation;
begin
    ZeroMemory(@SI, SizeOf(SI));
    SI.cb := SizeOf(SI);

    ZeroMemory(@PI, SizeOf(PI));

    if CreateProcess(
         nil,
         PChar('mspaint.exe'),
         nil,
         nil,
         False,
         0,
         nil,
         nil,
         SI,
         PI
       ) then
    begin
      ShowMessage('MSPaint started!' + sLineBreak +
                  'Process ID: ' + IntToStr(PI.dwProcessId));
      TargetPID:=PI.dwProcessId;
      dll_length := Length(string(dll_in)) + 1;
      // Get handle to Kernel32.dll
      kernel32_handle := GetModuleHandle('Kernel32.dll');

      // Get address of LoadLibraryA
      lbuffer := GetProcAddress(kernel32_handle, 'LoadLibraryA');

      // Open target process
      process_handle := OpenProcess(
        PROCESS_ALL_ACCESS,
        False,
        TargetPID
      );
      if process_handle = 0 then
    begin
      Writeln('Failed to open process.');
      Exit;
    end;

    // Allocate memory inside target process
    remote_buffer := VirtualAllocEx(
      process_handle,
      nil,
      dll_length,
      MEM_RESERVE or MEM_COMMIT,
      PAGE_EXECUTE_READWRITE
    );

    if remote_buffer = nil then
    begin
      ShowMessage('VirtualAllocEx failed.');
      CloseHandle(process_handle);
      Exit;
    end;

    // Write DLL path into target process memory
    if not WriteProcessMemory(
             process_handle,
             remote_buffer,
             dll_in,
             dll_length,
             nil
           ) then
    begin
      ShowMessage('WriteProcessMemory failed.');
      CloseHandle(process_handle);
      Exit;
    end;

    // Create remote thread calling LoadLibraryA
    remote_thread := CreateRemoteThread(
      process_handle,
      nil,
      0,
      LPTHREAD_START_ROUTINE(lbuffer),
      remote_buffer,
      0,
      nil
    );

    if remote_thread = 0 then
    begin
      ShowMessage('CreateRemoteThread failed.');
      CloseHandle(process_handle);
      Exit;
    end;

    ShowMessage('DLL injected successfully.');

    // Cleanup
    CloseHandle(remote_thread);
    CloseHandle(process_handle);

    // Always close handles when done
    CloseHandle(PI.hThread);
    CloseHandle(PI.hProcess);
  end
  else
  begin
    ShowMessage('Failed to start MSPaint. Error: ' +
                IntToStr(GetLastError));
  end;
end;

end.

