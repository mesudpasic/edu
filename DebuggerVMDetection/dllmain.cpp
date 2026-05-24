#include "windows.h"
#include "antidbg.h"
#include "vmcheck.h"
#include "detect.h"

bool DLL_EXPORT IsUnderAnyVM()
{
	return (IsInsideVPC() || IsInsideHyperV() || IsInsideVMWare() || IsInsideVirtualBox());
}

void DLL_EXPORT NotifyVMPresence()
{
	if (IsInsideHyperV())		
		MessageBox(NULL, (LPCWSTR)L"Hyper-V detected ->IsInsideHyperV", (LPCWSTR)L"VM Check", MB_ICONWARNING | MB_OK | MB_DEFBUTTON1);			
	else
		if (IsInsideVPC())
			MessageBox(NULL, (LPCWSTR)L"VPC detected->IsInsideVPC", (LPCWSTR)L"VM Check", MB_ICONWARNING | MB_OK | MB_DEFBUTTON1);
		else
			if (IsInsideVMWare())
				MessageBox(NULL, (LPCWSTR)L"VMWare detected->IsInsideVMWare", (LPCWSTR)L"VM Check", MB_ICONWARNING | MB_OK | MB_DEFBUTTON1);
			else
				if (IsInsideVirtualBox())
					MessageBox(NULL, (LPCWSTR)L"VirtualBox detected->IsInsideVirtualBox", (LPCWSTR)L"VM Check", MB_ICONWARNING | MB_OK | MB_DEFBUTTON1);
				else
					MessageBox(NULL, (LPCWSTR)L"No virtual machine detected", (LPCWSTR)L"VM Check", MB_ICONWARNING | MB_OK | MB_DEFBUTTON1);	
}

bool DLL_EXPORT IsAnyDebuggerFound()
{
	return (IsDbgPresentPrefixCheck() || Int2DCheck() || CanOpenCsrss() || MemoryBreakpointDebuggerCheck() || DetectFamousDebuggers() || CheckProcessDebugFlags());
}

void DLL_EXPORT CrashMe()
{
	//crashes the running process, can be used if you detect you're under VM od debugger, to crash execution ;)
	KillMe();
}


BOOL APIENTRY DllMain( HMODULE hModule,
                       DWORD  ul_reason_for_call,
                       LPVOID lpReserved
					 )
{
	switch (ul_reason_for_call)
	{
	case DLL_PROCESS_ATTACH:
	case DLL_THREAD_ATTACH:
	case DLL_THREAD_DETACH:
	case DLL_PROCESS_DETACH:
		break;
	}
	return TRUE;
}

