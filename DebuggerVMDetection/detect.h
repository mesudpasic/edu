#ifndef __AREDLL_H__
#define __AREDLL_H__

#include <windows.h>

void KillMe()
{
	*((unsigned int*)0) = 0xDEAD;
}

#ifdef AREDLL_EXPORTS
#define DLL_EXPORT __declspec(dllexport)
#else
#define DLL_EXPORT __declspec(dllimport)
#endif

#ifdef __cplusplus
extern "C"
{
#endif

	bool DLL_EXPORT IsUnderAnyVM();
	bool DLL_EXPORT IsAnyDebuggerFound();
	void DLL_EXPORT CrashMe();
	void DLL_EXPORT NotifyVMPresence();

#ifdef __cplusplus
}
#endif

#endif