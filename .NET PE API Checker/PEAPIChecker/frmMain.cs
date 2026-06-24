using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows.Forms;

namespace PEEXEAPIChecker
{
    public partial class frmMain : Form
    {
        private readonly ApiDescriptionService _apiDescriptions = new ApiDescriptionService();

        public frmMain()
        {
            InitializeComponent();
        }

        private void btnSelectApp_Click(object sender, EventArgs e)
        {
            if (oDlg.ShowDialog() != DialogResult.OK)
                return;

            LoadImports(oDlg.FileName);
        }

        private void LoadImports(string filePath)
        {
            try
            {
                lvResults.Items.Clear();

                IList<PeImportEntry> imports = PeImportReader.GetImportedApis(filePath);

                lblSummary.Text = string.Format(
                    "File: {0}   |   Imported APIs: {1}   |   Double-click a row to open Microsoft documentation",
                    filePath,
                    imports.Count);

                foreach (PeImportEntry import in imports)
                {
                    var item = new ListViewItem(import.ImportName);
                    item.SubItems.Add(_apiDescriptions.GetDescription(import));
                    item.Tag = import;
                    lvResults.Items.Add(item);
                }

                if (lvResults.Items.Count > 0)
                    lvResults.Items[0].Selected = true;
            }
            catch (Exception ex)
            {
                lblSummary.Text = "No file loaded.";
                lvResults.Items.Clear();

                MessageBox.Show(
                    this,
                    ex.Message,
                    "PE Import Error",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private void lvResults_DoubleClick(object sender, EventArgs e)
        {
            if (lvResults.SelectedItems.Count == 0)
                return;

            var import = lvResults.SelectedItems[0].Tag as PeImportEntry;
            if (import == null)
                return;

            string url = _apiDescriptions.GetDocumentationUrl(import);

            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = url,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    this,
                    string.Format("Could not open documentation link.{0}{0}{1}", Environment.NewLine, ex.Message),
                    "Open Documentation",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
            }
        }
    }
}
