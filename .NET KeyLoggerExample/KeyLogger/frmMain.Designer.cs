
namespace KeyLogger
{
    partial class frmMain
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lblHook = new System.Windows.Forms.Label();
            this.txtKeys = new System.Windows.Forms.TextBox();
            this.lblPoll = new System.Windows.Forms.Label();
            this.txtPollKeys = new System.Windows.Forms.TextBox();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.txtKeys);
            this.splitContainer1.Panel1.Controls.Add(this.lblHook);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.txtPollKeys);
            this.splitContainer1.Panel2.Controls.Add(this.lblPoll);
            this.splitContainer1.Size = new System.Drawing.Size(800, 450);
            this.splitContainer1.SplitterDistance = 220;
            this.splitContainer1.TabIndex = 0;
            // 
            // lblHook
            // 
            this.lblHook.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblHook.Location = new System.Drawing.Point(0, 0);
            this.lblHook.Name = "lblHook";
            this.lblHook.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.lblHook.Size = new System.Drawing.Size(800, 23);
            this.lblHook.TabIndex = 0;
            this.lblHook.Text = "Hook monitor (WH_KEYBOARD_LL) - one line per key event";
            this.lblHook.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtKeys
            // 
            this.txtKeys.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtKeys.HideSelection = false;
            this.txtKeys.Location = new System.Drawing.Point(0, 23);
            this.txtKeys.Multiline = true;
            this.txtKeys.Name = "txtKeys";
            this.txtKeys.ReadOnly = true;
            this.txtKeys.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtKeys.Size = new System.Drawing.Size(800, 197);
            this.txtKeys.TabIndex = 1;
            // 
            // lblPoll
            // 
            this.lblPoll.Dock = System.Windows.Forms.DockStyle.Top;
            this.lblPoll.Location = new System.Drawing.Point(0, 0);
            this.lblPoll.Name = "lblPoll";
            this.lblPoll.Padding = new System.Windows.Forms.Padding(4, 0, 0, 0);
            this.lblPoll.Size = new System.Drawing.Size(800, 23);
            this.lblPoll.TabIndex = 0;
            this.lblPoll.Text = "Poll monitor (GetAsyncKeyState) - typed text like Delphi memo";
            this.lblPoll.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtPollKeys
            // 
            this.txtPollKeys.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtPollKeys.HideSelection = false;
            this.txtPollKeys.Location = new System.Drawing.Point(0, 23);
            this.txtPollKeys.Multiline = true;
            this.txtPollKeys.Name = "txtPollKeys";
            this.txtPollKeys.ReadOnly = true;
            this.txtPollKeys.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.txtPollKeys.Size = new System.Drawing.Size(800, 203);
            this.txtPollKeys.TabIndex = 1;
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.splitContainer1);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Key Logger Example";
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.Label lblHook;
        private System.Windows.Forms.TextBox txtKeys;
        private System.Windows.Forms.Label lblPoll;
        private System.Windows.Forms.TextBox txtPollKeys;
    }
}
