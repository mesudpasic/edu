#include <intrin.h>
#include "windows.h"
#include <stdio.h>
#include <tchar.h>
#include <iostream>
#include <signal.h>
#include <intrin.h>
//collection of methods for detecting VMs
DWORD __forceinline IsInsideVPC_exceptionFilter(LPEXCEPTION_POINTERS ep)
{
	PCONTEXT ctx = ep->ContextRecord;

	ctx->Ebx = -1; // Not running VPC
	ctx->Eip += 4; // skip past the "call VPC" opcodes
	return EXCEPTION_CONTINUE_EXECUTION;
	// we can safely resume execution since we skipped faulty instruction
}
string GetCpuID()
{
	//Initialize used variables
	char SysType[13]; //Array consisting of 13 single bytes/characters
	string CpuID; //The string that will be used to add all the characters to
				  //Starting coding in assembly language
	_asm
	{
		//Execute CPUID with EAX = 0 to get the CPU producer
		XOR EAX, EAX
		CPUID
			//MOV EBX to EAX and get the characters one by one by using shift out right bitwise operation.
			MOV EAX, EBX
			MOV SysType[0], al
			MOV SysType[1], ah
			SHR EAX, 16
			MOV SysType[2], al
			MOV SysType[3], ah
			//Get the second part the same way but these values are stored in EDX
			MOV EAX, EDX
			MOV SysType[4], al
			MOV SysType[5], ah
			SHR EAX, 16
			MOV SysType[6], al
			MOV SysType[7], ah
			//Get the third part
			MOV EAX, ECX
			MOV SysType[8], al
			MOV SysType[9], ah
			SHR EAX, 16
			MOV SysType[10], al
			MOV SysType[11], ah
			MOV SysType[12], 00
	}
	CpuID.assign(SysType, 12);
	return CpuID;
}
// High level language friendly version of IsInsideVPC()
bool IsInsideVPC()
{
	bool rc = false;

	__try
	{
		_asm push ebx
		_asm mov  ebx, 0 // It will stay ZERO if VPC is running
		_asm mov  eax, 1 // VPC function number

						 // call VPC 
		_asm __emit 0Fh
		_asm __emit 3Fh
		_asm __emit 07h
		_asm __emit 0Bh

		_asm test ebx, ebx
		_asm setz[rc]
			_asm pop ebx
	}
	// The except block shouldn't get triggered if VPC is running!!
	__except (IsInsideVPC_exceptionFilter(GetExceptionInformation()))
	{
	}

	return rc;
}
bool IsInsideVMWare()
{
	bool rc = true;

	__try
	{
		__asm
		{
			push   edx
			push   ecx
				push   ebx

				mov    eax, 'VMXh'
				mov    ebx, 0 // any value but not the MAGIC VALUE
				mov    ecx, 10 // get VMWare version
				mov    edx, 'VX' // port number

				in     eax, dx // read port
							   // on return EAX returns the VERSION
				cmp    ebx, 'VMXh' // is it a reply from VMWare?
				setz[rc] // set return value

				pop    ebx
				pop    ecx
				pop    edx
		}
	}
	__except (EXCEPTION_EXECUTE_HANDLER)
	{
		rc = false;
	}

	return rc;
}
bool IsInsideHyperV()
{
	bool x = 0;
	__asm
	{
		pushad
		pushfd
			pop eax
			or eax, 0x00200000
			push eax
			popfd
			pushfd
			pop eax
			and eax, 0x00200000
			jz CPUID_NOT_SUPPORTED;Are you still alive ?
			xor eax, eax
			xor edx, edx
			xor ecx, ecx
			xor ebx, ebx
			inc eax;processor info and feature bits
			cpuid
			test ecx, 0x80000000;Hypervisor present
			jnz Hypervisor
			mov x, 0
			jmp bye
			Hypervisor :
		mov x, 1
			jmp bye
			CPUID_NOT_SUPPORTED :
		mov x, 2
			bye :
			popad
	}	
	string CpuID = GetCpuID();
	return (x == 1 && (CpuID.compare("Microsoft Hv")==0 || CpuID.compare("Hyper-V")==0));
}
bool IsInsideVirtualBox()
{
	bool result1 = false;
	bool result2 = false;
	if (LoadLibrary(L"VBoxHook.dll") != NULL)
	{
		result1 = true;
	}
	else
	{
		result1 = false;
	}
	if (CreateFile(L"\\\\.\\VBoxMiniRdrDN", GENERIC_READ, FILE_SHARE_READ, NULL, OPEN_EXISTING, FILE_ATTRIBUTE_NORMAL, NULL) != INVALID_HANDLE_VALUE)
	{
		result2 = false;
	}
	else
	{
		result2 = false;
	}
	return (result1 || result2);
}