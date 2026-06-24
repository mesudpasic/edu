
namespace PEEXEAPIChecker
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
            this.btnSelectApp = new System.Windows.Forms.Button();
            this.lblSummary = new System.Windows.Forms.Label();
            this.lvResults = new System.Windows.Forms.ListView();
            this.colImport = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colDescription = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.oDlg = new System.Windows.Forms.OpenFileDialog();
            this.SuspendLayout();
            // 
            // btnSelectApp
            // 
            this.btnSelectApp.Location = new System.Drawing.Point(12, 12);
            this.btnSelectApp.Name = "btnSelectApp";
            this.btnSelectApp.Size = new System.Drawing.Size(177, 64);
            this.btnSelectApp.TabIndex = 0;
            this.btnSelectApp.Text = "Select EXE File";
            this.btnSelectApp.UseVisualStyleBackColor = true;
            this.btnSelectApp.Click += new System.EventHandler(this.btnSelectApp_Click);
            // 
            // lblSummary
            // 
            this.lblSummary.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblSummary.Location = new System.Drawing.Point(12, 79);
            this.lblSummary.Name = "lblSummary";
            this.lblSummary.Size = new System.Drawing.Size(776, 40);
            this.lblSummary.TabIndex = 1;
            this.lblSummary.Text = "Select an EXE file to list imported Windows APIs.";
            // 
            // lvResults
            // 
            this.lvResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lvResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colImport,
            this.colDescription});
            this.lvResults.FullRowSelect = true;
            this.lvResults.GridLines = true;
            this.lvResults.HideSelection = false;
            this.lvResults.Location = new System.Drawing.Point(12, 122);
            this.lvResults.MultiSelect = false;
            this.lvResults.Name = "lvResults";
            this.lvResults.Size = new System.Drawing.Size(776, 316);
            this.lvResults.TabIndex = 2;
            this.lvResults.UseCompatibleStateImageBehavior = false;
            this.lvResults.View = System.Windows.Forms.View.Details;
            this.lvResults.DoubleClick += new System.EventHandler(this.lvResults_DoubleClick);
            // 
            // colImport
            // 
            this.colImport.Text = "Import";
            this.colImport.Width = 280;
            // 
            // colDescription
            // 
            this.colDescription.Text = "Description";
            this.colDescription.Width = 470;
            // 
            // oDlg
            // 
            this.oDlg.DefaultExt = "exe";
            this.oDlg.Filter = "Executable files (*.exe)|*.exe";
            this.oDlg.FilterIndex = 1;
            this.oDlg.Title = "Select EXE file";
            // 
            // frmMain
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.lvResults);
            this.Controls.Add(this.lblSummary);
            this.Controls.Add(this.btnSelectApp);
            this.Name = "frmMain";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Portable Executable API Checker";
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Button btnSelectApp;
        private System.Windows.Forms.Label lblSummary;
        private System.Windows.Forms.ListView lvResults;
        private System.Windows.Forms.ColumnHeader colImport;
        private System.Windows.Forms.ColumnHeader colDescription;
        private System.Windows.Forms.OpenFileDialog oDlg;
    }
}
