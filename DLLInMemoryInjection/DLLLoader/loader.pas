unit loader;

{$mode objfpc}{$H+}

interface

uses
  Classes, SysUtils,FileUtil,Windows, Forms, Controls, Graphics, Dialogs, StdCtrls, BTMemoryModule, fphttpclient;

type

  { TfrmMain }

  TfrmMain = class(TForm)
    Button1: TButton;
    procedure Button1Click(Sender: TObject);
  private

  public

  end;
type
  TExecuteDLL = procedure; stdcall;
var
  frmMain: TfrmMain;
implementation

{$R *.lfm}

{ TfrmMain }
// example how to download a binary file from net into a memory stream
procedure DownloadToMemoryStream(const URL: string; MS: TMemoryStream);
var
  HTTP: TFPHTTPClient;
begin
  HTTP := TFPHTTPClient.Create(nil);
  try
    MS.Clear;
    HTTP.Get(URL, MS);
    MS.Position := 0;
  finally
    HTTP.Free;
  end;
end;

procedure TfrmMain.Button1Click(Sender: TObject);
var
   ms: TMemoryStream;
   mp_DllData:Pointer;
   btMM: PBTMemoryModule;
   bm_ExecutePointer : TExecuteDLL=nil;
   rs : TResourceStream;
begin
  // can also use DownloadToMemoryStream to fetch it from net instead of from exe resources
 if 0 <> FindResource(hInstance, 'DLLDEMO', RT_RCDATA) then
 begin
  ms := TMemoryStream.Create;
  rs := TResourceStream.Create(hInstance, 'DLLDEMO', RT_RCDATA);
  ms.LoadFromStream(rs);
  ms.Position:=0;
  // load into memory from memory stream
  mp_DllData := GetMemory(ms.Size);
  ms.Read(mp_DllData^, ms.Size);
  // load into memory dll module
  btMM := BTMemoryLoadLibary(mp_DllData, ms.Size);
  if btMM <> nil then
    begin
        // get a pointer for DLL method
        Pointer(bm_ExecutePointer) := BTMemoryGetProcAddress(btMM, 'ExecuteDLL');
        if bm_ExecutePointer <> nil then
           bm_ExecutePointer();
        if Assigned(btMM) then
           BTMemoryFreeLibrary(btMM);
    end;
  ms.Free;
  rs.Free;
 end;
end;

end.

