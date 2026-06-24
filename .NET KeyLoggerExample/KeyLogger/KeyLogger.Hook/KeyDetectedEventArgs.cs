using System;

namespace KeyLogger.Hook
{
    public sealed class KeyDetectedEventArgs : EventArgs
    {
        public KeyDetectedEventArgs(int virtualKeyCode, int scanCode, bool isKeyDown, bool isAltDown)
        {
            VirtualKeyCode = virtualKeyCode;
            ScanCode = scanCode;
            IsKeyDown = isKeyDown;
            IsAltDown = isAltDown;
        }

        public int VirtualKeyCode { get; }

        public int ScanCode { get; }

        public bool IsKeyDown { get; }

        public bool IsAltDown { get; }

        public string ToDisplayLine()
        {
            string eventType = IsKeyDown ? "DOWN" : "UP  ";
            string keyName = VirtualKeyToText(VirtualKeyCode);
            string altText = IsAltDown ? "true" : "false";
            return string.Format("{0} {1} scan={2} alt={3}", eventType, keyName, ScanCode, altText);
        }

        private static string VirtualKeyToText(int vk)
        {
            switch (vk)
            {
                case NativeMethods.VK_RETURN: return "Enter";
                case NativeMethods.VK_ESCAPE: return "Esc";
                case NativeMethods.VK_SPACE: return "Space";
                case NativeMethods.VK_BACK: return "Backspace";
                case NativeMethods.VK_TAB: return "Tab";
                case NativeMethods.VK_SHIFT:
                case NativeMethods.VK_LSHIFT:
                case NativeMethods.VK_RSHIFT: return "Shift";
                case NativeMethods.VK_CONTROL:
                case NativeMethods.VK_LCONTROL:
                case NativeMethods.VK_RCONTROL: return "Ctrl";
                case NativeMethods.VK_MENU:
                case NativeMethods.VK_LMENU:
                case NativeMethods.VK_RMENU: return "Alt";
                default:
                    if (vk >= 'A' && vk <= 'Z')
                        return ((char)vk).ToString();
                    return "VK_" + vk;
            }
        }
    }
}
