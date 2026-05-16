using System;
using System.Runtime.InteropServices;

internal static class NativeInterop
{
    public const uint CREATE_SUSPENDED = 0x00000004;

    public const uint MEM_COMMIT = 0x00001000;
    public const uint MEM_RESERVE = 0x00002000;
    public const uint PAGE_EXECUTE_READWRITE = 0x00000040;

    public const int STATUS_SUCCESS = 0;
    public const int INVALID_RESUME_THREAD = -1;

    public const ushort IMAGE_FILE_MACHINE_I386 = 0x014C;
    public const ushort IMAGE_FILE_MACHINE_AMD64 = 0x8664;

    public const ushort IMAGE_NT_OPTIONAL_HDR32_MAGIC = 0x10B;
    public const ushort IMAGE_NT_OPTIONAL_HDR64_MAGIC = 0x20B;

    public const uint CONTEXT_i386 = 0x00010000;
    public const uint CONTEXT_AMD64 = 0x00100000;
    public const uint CONTEXT_CONTROL = 0x00000001;
    public const uint CONTEXT_INTEGER = 0x00000002;
    public const uint CONTEXT_SEGMENTS = 0x00000004;

    public static readonly uint ContextFull32 = CONTEXT_i386 | CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_SEGMENTS;
    public static readonly uint ContextFull64 = CONTEXT_AMD64 | CONTEXT_CONTROL | CONTEXT_INTEGER | CONTEXT_SEGMENTS;

    public const int PebImageBaseOffset32 = 0x08;
    public const int PebImageBaseOffset64 = 0x10;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct STARTUPINFO
    {
        public int cb;
        public string lpReserved;
        public string lpDesktop;
        public string lpTitle;
        public int dwX;
        public int dwY;
        public int dwXSize;
        public int dwYSize;
        public int dwXCountChars;
        public int dwYCountChars;
        public int dwFillAttribute;
        public int dwFlags;
        public short wShowWindow;
        public short cbReserved2;
        public IntPtr lpReserved2;
        public IntPtr hStdInput;
        public IntPtr hStdOutput;
        public IntPtr hStdError;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct PROCESS_INFORMATION
    {
        public IntPtr hProcess;
        public IntPtr hThread;
        public uint dwProcessId;
        public uint dwThreadId;
    }

    /// <summary>32-bit / WoW64 thread context (0x2CC bytes).</summary>
    [StructLayout(LayoutKind.Explicit, Size = 0x2CC)]
    public struct CONTEXT32
    {
        [FieldOffset(0x00)] public uint ContextFlags;
        [FieldOffset(0xAC)] public uint Ebx;
        [FieldOffset(0xC0)] public uint Eip;
    }

    /// <summary>64-bit native thread context (0x4D0 bytes).</summary>
    [StructLayout(LayoutKind.Explicit, Size = 0x4D0)]
    public struct CONTEXT64
    {
        [FieldOffset(0x30)] public uint ContextFlags;
        [FieldOffset(0x88)] public ulong Rdx;
        [FieldOffset(0x128)] public ulong Rip;
    }

    [DllImport("kernel32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
    public static extern bool CreateProcess(
        string lpApplicationName,
        string lpCommandLine,
        IntPtr lpProcessAttributes,
        IntPtr lpThreadAttributes,
        bool bInheritHandles,
        uint dwCreationFlags,
        IntPtr lpEnvironment,
        string lpCurrentDirectory,
        ref STARTUPINFO lpStartupInfo,
        out PROCESS_INFORMATION lpProcessInformation);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetThreadContext(IntPtr hThread, ref CONTEXT32 lpContext);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetThreadContext(IntPtr hThread, ref CONTEXT32 lpContext);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool GetThreadContext(IntPtr hThread, ref CONTEXT64 lpContext);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool SetThreadContext(IntPtr hThread, ref CONTEXT64 lpContext);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool Wow64GetThreadContext(IntPtr hThread, ref CONTEXT32 lpContext);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool Wow64SetThreadContext(IntPtr hThread, ref CONTEXT32 lpContext);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool ReadProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        byte[] lpBuffer,
        int nSize,
        out IntPtr lpNumberOfBytesRead);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern bool WriteProcessMemory(
        IntPtr hProcess,
        IntPtr lpBaseAddress,
        byte[] lpBuffer,
        int nSize,
        out IntPtr lpNumberOfBytesWritten);

    [DllImport("ntdll.dll", SetLastError = true)]
    public static extern int NtUnmapViewOfSection(IntPtr process, IntPtr baseAddress);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern IntPtr VirtualAllocEx(
        IntPtr hProcess,
        IntPtr lpAddress,
        IntPtr dwSize,
        uint flAllocationType,
        uint flProtect);

    [DllImport("kernel32.dll", SetLastError = true)]
    public static extern uint ResumeThread(IntPtr hThread);

    public static void ThrowWin32(string operation)
    {
        throw new InvalidOperationException($"{operation} failed. Win32 error {Marshal.GetLastWin32Error()}.");
    }

    public static IntPtr ReadPointer(IntPtr hProcess, IntPtr address)
    {
        int size = IntPtr.Size;
        byte[] buffer = new byte[size];
        IntPtr read;
        if (!ReadProcessMemory(hProcess, address, buffer, size, out read) || read.ToInt64() != size)
            ThrowWin32("ReadProcessMemory");
        if (size == 4)
            return new IntPtr(BitConverter.ToInt32(buffer, 0));
        return new IntPtr(BitConverter.ToInt64(buffer, 0));
    }

    public static void WritePointer(IntPtr hProcess, IntPtr address, IntPtr value)
    {
        byte[] buffer = IntPtr.Size == 4
            ? BitConverter.GetBytes(value.ToInt32())
            : BitConverter.GetBytes(value.ToInt64());
        IntPtr written;
        if (!WriteProcessMemory(hProcess, address, buffer, buffer.Length, out written))
            ThrowWin32("WriteProcessMemory");
    }
}
