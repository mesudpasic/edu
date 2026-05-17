library dlldemo;

{$SMARTLINK ON}
{$D-}

uses
   Dialogs, Interfaces;
  { you can add units after this }

procedure ExecuteDLL; stdcall;
begin
   ShowMessage('Hello from injected DLL!');
end;

exports
  ExecuteDLL;

begin
end.

