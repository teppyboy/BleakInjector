using System;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Windows.Forms;
using BleakInjector.Etc;

namespace BleakInjector
{
    public partial class BleakMain : Form
    {
        public string[] MethodItems;

        private readonly Config _config = new Config();

        private readonly DataTable _processTable = new DataTable();

        public BleakMain()
        {
            InitializeComponent();
            MethodComboBox.Items.Clear();
            MethodComboBox.Items.Add("[BLEAK] CreateThread");
            MethodComboBox.Items.Add("[BLEAK] HijackThread");
            MethodComboBox.Items.Add("[BLEAK] ManualMap");
            MethodComboBox.Items.Add("BasicInjector");
            MethodComboBox.Items.Add("[RI] Injector");
            MethodComboBox.Items.Add("[LUNAR] Injector");
            if (Program.isBypasserMode)
            {
                isBypassMode.Checked = true;
            }
        }
        private void Interface_Load(object sender, EventArgs e)
        {
            // Fill ProcessDataGrid

            _processTable.Columns.Add("Name", typeof(string));

            ProcessDataGrid.DataSource = _processTable;

            PopulateDataTable();

            // Sort the processes alphabetically

            ProcessDataGrid.Sort(ProcessDataGrid.Columns["Name"], ListSortDirection.Ascending);
        }

        private void PopulateDataTable()
        {
            var processes = Process.GetProcesses();
            foreach (var process in processes)
            {
                _processTable.Rows.Add(process.ProcessName);
            }
        }

        private void RefreshButton_Click(object sender, EventArgs e)
        {
            _processTable.Rows.Clear();

            // Refresh the process list

            PopulateDataTable();

            // Scroll to the top

            ProcessDataGrid.CurrentCell = ProcessDataGrid.Rows[0].Cells[0];
        }

        private void ChooseDLLButton_Click(object sender, EventArgs e)
        {
            FileDialog.ShowDialog();

            // Get the path to the dll

            var dll = _config.DllPath = FileDialog.FileName;

            DLLFileTextBox.Text = Path.GetFileNameWithoutExtension(dll);

        }

        private void InjectDllButton_Click(object sender, EventArgs e)
        {
            // Inject the DLL

            var status = Program.Inject(_config);

            // Update the status text

            StatusLabel.Text = "";

            if (status.InjectionOutcome)
            {
                StatusLabel.Text += "Successfully Injected DLL" + Environment.NewLine;
            }

            if (status.EraseHeadersOutcome)
            {
                StatusLabel.Text += "Successfully Erased PE Headers" + Environment.NewLine;
            }
        }

        private void ProcessDataGrid_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            var process = ProcessDataGrid.Rows[e.RowIndex].Cells[e.ColumnIndex];

            SelectedProcessTextBox.Text = _config.ProcessName = process.FormattedValue?.ToString();
        }

        private void MethodComboBox_TextChanged(object sender, EventArgs e)
        {
            _config.InjectionMethod = MethodComboBox.Text;
        }

        private void CloseAfterInjectCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _config.CloseAfterInject = CloseAfterInjectCheckBox.Checked;
        }

        private void EraseHeadersCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            _config.EraseHeaders = EraseHeadersCheckBox.Checked;
        }

        private void BleakMain_FormClosing(object sender, FormClosingEventArgs e)
        {
            var currappname = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
            if (currappname != Program.rootAppExe)
            {
                Process p = new Process();
                p.StartInfo.FileName = Program.rootAppExe;
                p.StartInfo.Arguments = "-clrtemp." + currappname;
                p.Start();
                Environment.Exit(0);
            }
        }

        private void isBypassMode_Click(object sender, EventArgs e)
        {
            if (isBypassMode.Checked)
            {
                DialogResult askusr = MessageBox.Show("BleakInjector has detected that Anti-Injector Bypass has been enabled.\nIn order to continue this will create a temp app and launch the patched app with bypass.\nPress OK to enable Anti-Injector Bypass or Cancel to cancel.", "BleakInjector", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (askusr == DialogResult.OK)
                {
                    var currappname = Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
                    var newappname = Program.GetUniqueKey(13) + ".exe";
                    File.Copy(currappname, newappname);
                    Process p = new Process();
                    p.StartInfo.FileName = newappname;
                    p.StartInfo.Arguments = "-rootAppName." + currappname;
                    p.Start();
                    p.Dispose();
                    Environment.Exit(0);
                }
                else
                {
                    isBypassMode.Checked = false;
                }
            }
            else if (!isBypassMode.Checked)
            {
                DialogResult askusr = MessageBox.Show("BleakInjector has detected that Anti-Injector Bypass has been disabled.\nIn order to continue this will delete itself and launch the original app.\nPress OK to disable Anti-Injector Bypass or Cancel to cancel.", "BleakInjector", MessageBoxButtons.OKCancel, MessageBoxIcon.Information);
                if (askusr == DialogResult.OK)
                {
                    Process p = new Process();
                    p.StartInfo.FileName = Program.rootAppExe;
                    p.StartInfo.Arguments = "-clrt3mp&lc." + Path.GetFileName(Process.GetCurrentProcess().MainModule.FileName);
                    p.Start();
                    p.Dispose();
                    Environment.Exit(0);
                }
                else
                {
                    isBypassMode.Checked = true;
                }
            }
        }
    }
}