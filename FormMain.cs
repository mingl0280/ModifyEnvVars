using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;

namespace ModifyEnvVars
{
    public partial class FormMain : Form
    {
        public FormMain()
        {
            InitializeComponent();
            _varsDt.Columns.Add("Values");
            dataGridViewMain.DataSource = _varsDt;
            dataGridViewMain.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }

        private List<string> _envValues = new List<string>();
        private string _envRawStr = "";
        RegistryKey env_key = null;
        private DataTable _varsDt = new DataTable("Paths");

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                env_key = Registry.LocalMachine.OpenSubKey("SYSTEM\\CurrentControlSet\\Control\\Session Manager\\Environment", true);
            }
            catch (Exception)
            {
                MessageBox.Show(Resource.FormMain_Form1_Load_Please_restart_the_app_in_Administrator_privilege_, Resource.FormMain_Form1_Load_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }

            if (env_key == null)
            {
                MessageBox.Show(Resource.FormMain_Form1_Load_Please_restart_the_app_in_Administrator_privilege_, Resource.FormMain_Form1_Load_Error, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Environment.Exit(-1);
            }
            _envRawStr = env_key.GetValue("Path") as string;
            _envValues = _envRawStr.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            
            foreach (var env_value in _envValues)
            {
                _varsDt.Rows.Add(env_value);
            }

            dataGridViewMain.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
        }

        private void buttonNew_Click(object sender, EventArgs e)
        {
            var idx = dataGridViewMain.CurrentCell.RowIndex;

            var data_row = _varsDt.NewRow();
            _varsDt.Rows.InsertAt(data_row, idx+1);
            
            dataGridViewMain.CurrentCell = dataGridViewMain[0, idx+1];

            dataGridViewMain.BeginEdit(false);
        }

        private void buttonSave_Click(object sender, EventArgs e)
        {
            Save();
        }

        private void Save()
        {
            UpdateVarsList();
            var results_str = string.Join(";", _envValues);
            env_key.SetValue("path", results_str, RegistryValueKind.String);
            MessageBox.Show(Resource.FormMain_buttonSave_Click_Save_complete_, Resource.FormMain_buttonSave_Click_Info, MessageBoxButtons.OK, MessageBoxIcon.Information);
        }


        private void dataGridViewMain_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            UpdateVarsList();
        }

        private void UpdateVarsList()
        {
            _envValues.Clear();
            foreach (DataRow vars_dt_row in _varsDt.Rows)
            {
                _envValues.Add(vars_dt_row[0] as string);
            }
        }

        private void buttonEdit_Click(object sender, EventArgs e)
        {
            var idx = dataGridViewMain.CurrentCell.RowIndex;
            dataGridViewMain.CurrentCell = dataGridViewMain[0, idx];
            dataGridViewMain.BeginEdit(true);
        }

        private void buttonBrowse_Click(object sender, EventArgs e)
        {
            var current_cell_text = dataGridViewMain.CurrentCell.Value as string;
            
            var dialog = new CommonOpenFileDialog();
            dialog.IsFolderPicker = true;
            if (!string.IsNullOrWhiteSpace(current_cell_text))
            {
                dialog.InitialDirectory = (new DirectoryInfo(current_cell_text)).FullName;
            }
            if (dialog.ShowDialog() == CommonFileDialogResult.Ok)
            {
                if (Directory.Exists(dialog.FileName))
                {
                    dataGridViewMain.CurrentCell.Value = dialog.FileName;
                }
            }
        }

        private void buttonDelete_Click(object sender, EventArgs e)
        {
            var idx = dataGridViewMain.CurrentCell.RowIndex;
            _varsDt.Rows.RemoveAt(idx);
        }

        private void buttonMoveUp_Click(object sender, EventArgs e)
        {
            var idx = dataGridViewMain.CurrentCell.RowIndex;
            if (idx == 0)
                return;
            var row_current = _varsDt.Rows[idx].Clone();
            _varsDt.Rows.RemoveAt(idx);
            _varsDt.Rows.InsertAt(row_current, idx-1);
            dataGridViewMain.ClearSelection();
            dataGridViewMain.CurrentCell = dataGridViewMain.Rows[idx - 1].Cells[0];
        }

        private void buttonMoveDown_Click(object sender, EventArgs e)
        {
            var idx = dataGridViewMain.CurrentCell.RowIndex;
            if (idx == _varsDt.Rows.Count - 1)
                return;
            var row_current = _varsDt.Rows[idx].Clone();
            _varsDt.Rows.RemoveAt(idx);
            _varsDt.Rows.InsertAt(row_current, idx + 1);
            dataGridViewMain.ClearSelection();
            dataGridViewMain.CurrentCell = dataGridViewMain.Rows[idx + 1].Cells[0];
        }

        private void buttonOK_Click(object sender, EventArgs e)
        {
            Save();
            Application.Exit();
        }

        private void buttonQuit_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void buttonEditText_Click(object sender, EventArgs e)
        {
            MessageBox.Show(Resource.FormMain_buttonEditText_Click_Not_implemented_, Resource.FormMain_buttonEditText_Click_Warning, MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
        }
    }

    public static class ExtendedMethods
    {
        public static DataRow Clone(this DataRow Row)
        {
            var new_row = Row.Table.NewRow();
            new_row.ItemArray = Row.ItemArray;
            return new_row;
        }
    }
}
