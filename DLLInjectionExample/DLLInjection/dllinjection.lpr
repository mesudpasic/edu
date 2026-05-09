library dllinjection;

{$mode objfpc}{$H+}

uses
  Windows;

function DllMain(hModule: HMODULE; nReason: DWORD; lpReserved: Pointer): BOOL; stdcall;
begin
  case nReason of
    DLL_PROCESS_ATTACH:
      begin
        MessageBox(
          0,
          'Hello from injected dll!',
          '=^..^=',
          MB_OK
        );
      end;

    DLL_PROCESS_DETACH:
      begin
      end;

    DLL_THREAD_ATTACH:
      begin
      end;

    DLL_THREAD_DETACH:
      begin
      end;
  end;

  Result := TRUE;
end;

exports
  DllMain;

begin
end.

