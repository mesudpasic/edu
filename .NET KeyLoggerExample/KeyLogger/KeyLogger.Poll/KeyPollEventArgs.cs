using System;

namespace KeyLogger.Poll
{
    public sealed class KeyPollEventArgs : EventArgs
    {
        public KeyPollEventArgs(int virtualKeyCode, string text, bool isBackspace)
        {
            VirtualKeyCode = virtualKeyCode;
            Text = text;
            IsBackspace = isBackspace;
        }

        public int VirtualKeyCode { get; }

        public string Text { get; }

        public bool IsBackspace { get; }
    }
}
