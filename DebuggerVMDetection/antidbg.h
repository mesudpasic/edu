#include "windows.h"
#include <TlHelp32.h>
#include <tchar.h>
#include <stdio.h>
#include <iostream>
using namespace std;

// collected methods for detecting most famous debuggers

bool DbgFound = false;
// CheckProcessDebugFlags will return true if 
// the EPROCESS->NoDebugInherit is == FALSE, 
// the reason we check for false is because 
// the NtQueryProcessInformation function returns the
// inverse of EPROCESS->NoDebugInherit so (!TRUE == FALSE)
 bool CheckProcessDebugFlags()
{
	// Much easier in ASM but C/C++ looks so much better
	typedef NTSTATUS(WINAPI *pNtQueryInformationProcess)
		(HANDLE, UINT, PVOID, ULONG, PULONG);

	DWORD NoDebugInherit = 0;
	NTSTATUS Status;

	// Get NtQueryInformationProcess
	pNtQueryInformationProcess NtQIP = (pNtQueryInformationProcess)
		GetProcAddress(GetModuleHandle(TEXT("ntdll.dll")),
			"NtQueryInformationProcess");

	Status = NtQIP(GetCurrentProcess(),
		0x1f, // ProcessDebugFlags
		&NoDebugInherit, 4, NULL);

	if (Status != 0x00000000)
		return false;

	if (NoDebugInherit == FALSE)
		return true;
	else
		return false;
}
std::wstring StringToWString(std::string& s)
{
	int len;
	int slength = (int)s.length() + 1;
	len = MultiByteToWideChar(CP_ACP, 0, s.c_str(), slength, 0, 0);
	wchar_t* buf = new wchar_t[len];
	MultiByteToWideChar(CP_ACP, 0, s.c_str(), slength, buf, len);
	std::wstring r(buf);
	delete[] buf;
	return r;
}
wstring xor_resolve(string input) {
	 /* XOR encrypt decrypt */
	/* 596e3fdaad1c42b78e34b413597021db */
	 char key[32] = { 0x35, 0x39, 0x36, 0x65, 0x33, 0x66, 0x64, 0x61, 0x61, 0x64, 0x31, 0x63, 0x34, 0x32, 0x62, 0x37, 0x38, 0x65, 0x33, 0x34, 0x62, 0x34, 0x31, 0x33, 0x35, 0x39, 0x37, 0x30, 0x32, 0x31, 0x64, 0x62 };
	 string r = input;
	 for (int i = 0; i < input.size(); i++)
		 r[i] = input[i] ^ key[i % (sizeof(key) / sizeof(char))];
	 return StringToWString(r);
 }
bool CALLBACK EnumWindowsProc(HWND hwnd, LPARAM lParam)
{
	if (IsWindowVisible(hwnd))
	{
		char olly[8] = { 0x7A, 0x75, 0x7A, 0x3C, 0x77, 0x24, 0x23 };
		wstring olly_s = xor_resolve(olly);//OLLYDBG
		char win_ddb[17] = { 0x62,0x50,0x58,0x21,0x51,0x1,0x22,0x13,0x0,0x9,0x54,0x20,0x58,0x53,0x11,0x44 };
		wstring win_ddb_s = xor_resolve(win_ddb);//WinDbgFrameClass
		char z_dbg[14] = { 0x6F, 0x5C, 0x42, 0x4, 0x13, 0x22, 0x1, 0x3, 0x14, 0x3, 0x56, 0x6, 0x46 };
		wstring z_dbg_s = xor_resolve(z_dbg);//Zeta Debugger
		char r_dbg[14] = { 0x67, 0x56, 0x55, 0xE, 0x13, 0x22, 0x1, 0x3, 0x14, 0x3, 0x56, 0x6, 0x46 };
		wstring r_dbg_s = xor_resolve(r_dbg);//Rock Debugger
		char ob_gui[12] = { 0x7A, 0x5B, 0x45, 0xC, 0x57, 0xF, 0x5, 0xF, 0x26, 0x31, 0x78 };
		wstring ob_gui_s = xor_resolve(ob_gui);//ObsidianGUI	
		int length = ::GetWindowTextLength(hwnd);
		TCHAR* buffer;
		wstring windowTitle;
		wstring windowClass;
		if (length > 0)
		{
			buffer = new TCHAR[length + 1];
			memset(buffer, 0, (length + 1) * sizeof(TCHAR));
			GetWindowText(hwnd, buffer, length + 1);
			windowTitle = buffer;
			delete[] buffer;
		}
		buffer = new TCHAR[255];
		memset(buffer, 0, (length + 1) * sizeof(TCHAR));
		GetClassName(hwnd, buffer, 256);
		windowClass = buffer;
		delete[] buffer;
		if ((windowTitle.compare(olly_s) == 0 || windowClass.compare(olly_s) == 0) ||
			(windowTitle.compare(win_ddb_s) == 0 || windowClass.compare(win_ddb_s) == 0) ||
			(windowTitle.compare(z_dbg_s) == 0 || windowClass.compare(z_dbg_s) == 0) ||
			(windowTitle.compare(r_dbg_s) == 0 || windowClass.compare(r_dbg_s) == 0) ||
			(windowTitle.compare(ob_gui_s) == 0 || windowClass.compare(ob_gui_s) == 0))
			DbgFound = true;
		if (DbgFound)
			return false;
		else
			return true;
	}
	return true;
}
 bool DetectFamousDebuggers()
{	
	
	/*HANDLE ollydbg_handle = FindWindow(olly_s.c_str(), NULL); //for olly debugger
	HANDLE windbg_handle = FindWindow(win_ddb_s.c_str(), NULL); //for WinDbg debugger
	HANDLE zetadbg_handle = FindWindow(z_dbg_s.c_str(), NULL); //for Zeta Debugger
	HANDLE rockdbg_handle = FindWindow(r_dbg_s.c_str(), NULL); //for Rock Debugger
	HANDLE obsidian_handle = FindWindow(ob_gui_s.c_str(), NULL); //for ObsidianGUI Debugger*/
	EnumWindows((WNDENUMPROC)EnumWindowsProc, NULL);
	/* this line is used to crash ollydbg only, not to detect anything */
	OutputDebugString(TEXT("%s%s%s%s%s%s%s%s%s%s%s")TEXT("%s%s%s%s%s%s%s%s%s%s%s%s%s")TEXT("%s%s%s%s%s%s%s%s%s%s%s%s%s")TEXT("%s%s%s%s%s%s%s%s%s%s%s%s%s"));
	//return (ollydbg_handle || windbg_handle || zetadbg_handle || rockdbg_handle || obsidian_handle);
	return DbgFound;
}

/*
Memory breakpoints are implemented by a debugger using guard pages, and they act like "a one-shot alarm for memory page access" 
(Creating Guard Pages). In a nutshell, when a page of memory is marked as PAGE_GUARD and is accessed, 
a STATUS_GUARD_PAGE_VIOLATION exception is raised, which can then be handled by the current program.
At the moment, there's no accurate way to check for memory breakpoints. However, we can use the techniques 
a debugger uses to implement memory breakpoints to discover if our program is currently running under a debugger. 
In essence, what occurs is that we allocate a dynamic buffer and write a RET to the buffer. We then mark the page 
as a guard page and push a potential return address onto the stack. Next, we jump to our page, and if we're under 
a debugger, specifically OllyDBG, then we will hit the RET instruction and return to the address we pushed onto 
the stack before we jumped to our page. Otherwise, a STATUS_GUARD_PAGE_VIOLATION exception will occur, 
and we know we're not being debugged by OllyDBG. 
*/
 bool MemoryBreakpointDebuggerCheck()
{
	unsigned char *pMem = NULL;
	SYSTEM_INFO sysinfo = { 0 };
	DWORD OldProtect = 0;
	void *pAllocation = NULL; // Get the page size for the system 

	GetSystemInfo(&sysinfo); // Allocate memory 

	pAllocation = VirtualAlloc(NULL, sysinfo.dwPageSize,
		MEM_COMMIT | MEM_RESERVE,
		PAGE_EXECUTE_READWRITE);

	if (pAllocation == NULL)
		return false;

	// Write a ret to the buffer (opcode 0xc3)
	pMem = (unsigned char*)pAllocation;
	*pMem = 0xc3;

	// Make the page a guard page         
	if (VirtualProtect(pAllocation, sysinfo.dwPageSize,
		PAGE_EXECUTE_READWRITE | PAGE_GUARD,
		&OldProtect) == 0)
	{
		return false;
	}

	__try
	{
		__asm
		{
			mov eax, pAllocation
			// This is the address we'll return to if we're under a debugger
				push MemBpBeingDebugged
				jmp eax // Exception or execution, which shall it be :D?
		}
	}
	__except (EXCEPTION_EXECUTE_HANDLER)
	{
		// The exception occured and no debugger was detected
		VirtualFree(pAllocation, NULL, MEM_RELEASE);
		return false;
	}

	__asm {MemBpBeingDebugged:}
	VirtualFree(pAllocation, NULL, MEM_RELEASE);
	return true;
}

// HideThread will attempt to use
// NtSetInformationThread to hide a thread
// from the debugger, Passing NULL for
// hThread will cause the function to hide the thread
// the function is running in. Also, the function returns
// false on failure and true on success
 bool HideThread(HANDLE hThread)
{
	typedef NTSTATUS(NTAPI *pNtSetInformationThread)
		(HANDLE, UINT, PVOID, ULONG);
	NTSTATUS Status;

	// Get NtSetInformationThread
	pNtSetInformationThread NtSIT = (pNtSetInformationThread)
		GetProcAddress(GetModuleHandle(TEXT("ntdll.dll")),
			"NtSetInformationThread");

	// Shouldn't fail
	if (NtSIT == NULL)
		return false;

	// Set the thread info
	if (hThread == NULL)
		Status = NtSIT(GetCurrentThread(),
			0x11, // HideThreadFromDebugger
			0, 0);
	else
		Status = NtSIT(hThread, 0x11, 0, 0);

	if (Status != 0x00000000)
		return false;
	else
		return true;
}

// CheckOutputDebugString checks whether or 
// OutputDebugString causes an error to occur
// and if the error does occur then we know 
// there's no debugger, otherwise if there IS
// a debugger no error will occur
 bool CheckOutputDebugString(LPCTSTR String)
{
	OutputDebugString(String);
	if (GetLastError() == 0)
		return true;
	else
		return false;
}

// This function uses the toolhelp32 api to enumerate all running processes
// on the computer and does a comparison of the process name against the
// ProcessName parameter. The function will return 0 on failure.
 DWORD GetProcessIdFromName(LPCTSTR ProcessName)
{
	PROCESSENTRY32 pe32;
	HANDLE hSnapshot = NULL;
	ZeroMemory(&pe32, sizeof(PROCESSENTRY32));

	// We want a snapshot of processes
	hSnapshot = CreateToolhelp32Snapshot(TH32CS_SNAPPROCESS, 0);

	// Check for a valid handle, in this case we need to check for
	// INVALID_HANDLE_VALUE instead of NULL
	if (hSnapshot == INVALID_HANDLE_VALUE)
		return 0;

	// Now we can enumerate the running process, also
	// we can't forget to set the PROCESSENTRY32.dwSize member
	// otherwise the following functions will fail
	pe32.dwSize = sizeof(PROCESSENTRY32);

	if (Process32First(hSnapshot, &pe32) == FALSE)
	{
		// Cleanup the mess
		CloseHandle(hSnapshot);
		return 0;
	}

	// Do our first comparison
	if (_tcsicmp(pe32.szExeFile, ProcessName) == FALSE)
	{
		// Cleanup the mess
		CloseHandle(hSnapshot);
		return pe32.th32ProcessID;
	}

	// Most likely it won't match on the first try so
	// we loop through the rest of the entries until
	// we find the matching entry or not one at all
	while (Process32Next(hSnapshot, &pe32))
	{
		if (_tcsicmp(pe32.szExeFile, ProcessName) == 0)
		{
			// Cleanup the mess
			CloseHandle(hSnapshot);
			return pe32.th32ProcessID;
		}
	}

	// If we made it this far there wasn't a match
	// so we'll return 0
	CloseHandle(hSnapshot);
	return 0;
}

// This function will return the process id of csrss.exe
// and will do so in two different ways. If the OS is XP or
// greater NtDll has a CsrGetProcessId otherwise I'll use
// GetProcessIdFromName. Like other functions it will
// return 0 on failure.
 DWORD GetCsrssProcessId()
{
	// Don't forget to set dw.Size to the appropriate
	// size (either OSVERSIONINFO or OSVERSIONINFOEX)
	OSVERSIONINFO osinfo;
	ZeroMemory(&osinfo, sizeof(OSVERSIONINFO));
	osinfo.dwOSVersionInfoSize = sizeof(OSVERSIONINFO);

	// Shouldn't fail
	GetVersionEx(&osinfo);

	// Visit http://msdn.microsoft.com/en-us/library/ms724833(VS.85).aspx
	// for a full table of versions however what I have set will
	// trigger on anything XP and newer including Server 2003
	if (osinfo.dwMajorVersion >= 5 && osinfo.dwMinorVersion >= 1)
	{
		// Gotta love functions pointers
		typedef DWORD(__stdcall *pCsrGetId)();

		// Grab the export from NtDll
		pCsrGetId CsrGetProcessId = (pCsrGetId)GetProcAddress(GetModuleHandle(TEXT("ntdll.dll")), "CsrGetProcessId");

		if (CsrGetProcessId)
			return CsrGetProcessId();
		else
			return 0;
	}
	else
		return GetProcessIdFromName(TEXT("csrss.exe"));
}
// The function will attempt to open csrss.exe with 
// PROCESS_ALL_ACCESS rights if it fails we're 
// not being debugged however, if its successful we probably are
 bool CanOpenCsrss()
{
	HANDLE Csrss = 0;

	// If we're being debugged and the process has
	// SeDebugPrivileges privileges then this call
	// will be successful, note that this only works
	// with PROCESS_ALL_ACCESS.
	Csrss = OpenProcess(PROCESS_ALL_ACCESS,
		FALSE, GetCsrssProcessId());

	if (Csrss != NULL)
	{
		CloseHandle(Csrss);
		return true;
	}
	else
		return false;
}

// The IsDbgPresentPrefixCheck works in at least two debuggers
// OllyDBG and VS 2008, by utilizing the way the debuggers handle
// prefixes we can determine their presence. Specifically if this code
// is ran under a debugger it will simply be stepped over;
// however, if there is no debugger SEH will fire :D
 bool IsDbgPresentPrefixCheck()
{
	__try
	{
		__asm __emit 0xF3 // 0xF3 0x64 disassembles as PREFIX REP:
		__asm __emit 0x64
		__asm __emit 0xF1 // One byte INT 1
	}
	__except (EXCEPTION_EXECUTE_HANDLER)
	{
		return false;
	}

	return true;
}

// The Int2DCheck function will check to see if a debugger
// is attached to the current process. It does this by setting up
// SEH and using the Int 2D instruction which will only cause an
// exception if there is no debugger. Also when used in OllyDBG
// it will skip a byte in the disassembly and will create
// some havoc.
 bool Int2DCheck()
{
	__try
	{
		__asm
		{
			int 0x2d
			xor eax, eax
				add eax, 2
		}
	}
	__except (EXCEPTION_EXECUTE_HANDLER)
	{
		return false;
	}

	return true;
}

// This function will erase the current images
// PE header from memory preventing a successful image
// if dumped
 void ErasePEHeaderFromMemory()
{
	DWORD OldProtect = 0;

	// Get base address of module
	char *pBaseAddr = (char*)GetModuleHandle(NULL);

	// Change memory protection
	VirtualProtect(pBaseAddr, 4096, // Assume x86 page size
		PAGE_READWRITE, &OldProtect);

	// Erase the header
	ZeroMemory(pBaseAddr, 4096);
}