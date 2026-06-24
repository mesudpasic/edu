using System;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace KeyLogger.Hook
{
    /// <summary>
    /// Global low-level keyboard hook (WH_KEYBOARD_LL), equivalent to the Lazarus example.
    /// Must be started from a thread that runs a Windows message loop (e.g. WinForms UI thread).
    /// </summary>
    public sealed class GlobalKeyboardHook : IDisposable
    {
        private NativeMethods.LowLevelKeyboardProc _hookProc;
        private IntPtr _hookHandle = IntPtr.Zero;
        private bool _disposed;

        public event EventHandler<KeyDetectedEventArgs> KeyDetected;

        public bool IsRunning
        {
            get { return _hookHandle != IntPtr.Zero; }
        }

        public void Start()
        {
            if (_hookHandle != IntPtr.Zero)
                return;

            _hookProc = HookCallback;
            _hookHandle = NativeMethods.SetWindowsHookEx(
                NativeMethods.WH_KEYBOARD_LL,
                _hookProc,
                NativeMethods.GetCurrentModuleHandle(),
                0);

            if (_hookHandle == IntPtr.Zero)
                throw new Win32Exception(Marshal.GetLastWin32Error(), "SetWindowsHookEx failed.");
        }

        public void Stop()
        {
            if (_hookHandle == IntPtr.Zero)
                return;

            if (!NativeMethods.UnhookWindowsHookEx(_hookHandle))
                throw new Win32Exception(Marshal.GetLastWin32Error(), "UnhookWindowsHookEx failed.");

            _hookHandle = IntPtr.Zero;
            _hookProc = null;
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Stop();
            _disposed = true;
        }

        private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode == NativeMethods.HC_ACTION)
            {
                int message = wParam.ToInt32();
                if (message == NativeMethods.WM_KEYDOWN ||
                    message == NativeMethods.WM_SYSKEYDOWN ||
                    message == NativeMethods.WM_KEYUP ||
                    message == NativeMethods.WM_SYSKEYUP)
                {
                    NativeMethods.KBDLLHOOKSTRUCT hookStruct =
                        (NativeMethods.KBDLLHOOKSTRUCT)Marshal.PtrToStructure(
                            lParam,
                            typeof(NativeMethods.KBDLLHOOKSTRUCT));

                    bool isKeyDown = message == NativeMethods.WM_KEYDOWN ||
                                     message == NativeMethods.WM_SYSKEYDOWN;
                    bool isAltDown = (hookStruct.flags & NativeMethods.LLKHF_ALTDOWN) != 0;

                    OnKeyDetected(new KeyDetectedEventArgs(
                        (int)hookStruct.vkCode,
                        (int)hookStruct.scanCode,
                        isKeyDown,
                        isAltDown));
                }
            }

            return NativeMethods.CallNextHookEx(_hookHandle, nCode, wParam, lParam);
        }

        private void OnKeyDetected(KeyDetectedEventArgs e)
        {
            EventHandler<KeyDetectedEventArgs> handler = KeyDetected;
            if (handler != null)
                handler(this, e);
        }
    }
}
