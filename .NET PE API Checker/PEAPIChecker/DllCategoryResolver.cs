using System;
using System.Collections.Generic;

namespace PEEXEAPIChecker
{
    internal static class DllCategoryResolver
    {
        private static readonly Dictionary<string, string> Categories =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                { "KERNEL32.dll", "Core OS services: files, memory, processes, and threads" },
                { "KERNELBASE.dll", "Base kernel services used by higher-level Windows APIs" },
                { "NTDLL.dll", "Low-level NT system calls and runtime support" },
                { "USER32.dll", "User interface: windows, input, dialogs, and messaging" },
                { "GDI32.dll", "Graphics drawing, fonts, device contexts, and printing" },
                { "GDIPLUS.dll", "Advanced 2D graphics and image processing" },
                { "ADVAPI32.dll", "Registry, security, services, and event logging" },
                { "SHELL32.dll", "Shell folders, shortcuts, file operations, and taskbar" },
                { "SHLWAPI.dll", "Lightweight shell helper utilities" },
                { "OLE32.dll", "COM component creation and marshaling" },
                { "OLEAUT32.dll", "Automation, VARIANTs, and BSTR string support" },
                { "COMDLG32.dll", "Common dialog boxes such as open/save file" },
                { "WS2_32.dll", "Winsock networking and socket operations" },
                { "WININET.dll", "HTTP/FTP and internet access through WinINet" },
                { "WINHTTP.dll", "HTTP client services" },
                { "CRYPT32.dll", "Certificates, cryptography, and SSL/TLS support" },
                { "BCrypt.dll", "Modern cryptographic primitives" },
                { "NCrypt.dll", "Next-generation cryptography and key storage" },
                { "SETUPAPI.dll", "Device installation and driver setup" },
                { "CFGMGR32.dll", "Plug and Play configuration manager" },
                { "VERSION.dll", "File version information queries" },
                { "PSAPI.dll", "Process and module information" },
                { "IPHLPAPI.dll", "Network adapter and IP helper functions" },
                { "DNSAPI.dll", "Domain Name System (DNS) queries" },
                { "WTSAPI32.dll", "Remote desktop and terminal services" },
                { "USERENV.dll", "User profiles and environment settings" },
                { "NETAPI32.dll", "Network management and domain services" },
                { "MPR.dll", "Multiple provider router for network resources" },
                { "WINMM.dll", "Multimedia audio, timers, and joysticks" },
                { "IMM32.dll", "Input Method Manager for keyboard layouts" },
                { "UXTHEME.dll", "Visual themes and styled controls" },
                { "DWMAPI.dll", "Desktop Window Manager composition effects" },
                { "PROPSYS.dll", "Property system for shell metadata" },
                { "PROFAPI.dll", "User profile API support" },
                { "MSVCRT.dll", "C runtime library functions" },
                { "VCRUNTIME140.dll", "Visual C++ runtime support" },
            };

        public static string GetCategory(string dllName)
        {
            if (string.IsNullOrWhiteSpace(dllName))
                return "Unknown Windows module";

            string category;
            if (Categories.TryGetValue(dllName, out category))
                return category;

            if (dllName.StartsWith("API-MS-", StringComparison.OrdinalIgnoreCase))
                return "Windows API set forwarder to core system functionality";

            if (dllName.StartsWith("EXT-MS-", StringComparison.OrdinalIgnoreCase))
                return "Extended Windows API set forwarder";

            return string.Format("Imported from {0}", dllName);
        }
    }
}
