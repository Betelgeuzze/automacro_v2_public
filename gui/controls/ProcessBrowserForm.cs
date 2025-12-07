// gui/controls/ProcessBrowserForm.cs
using System;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Automacro.Models;

namespace Automacro.Gui.Controls
{
public class ProcessBrowserForm : Form
{
    private ListView listView;
    private Button btnOk;
    private Button btnCancel;
    private Button btnRefresh;
    private TextBox txtFilter;
    private Label lblFilter;
    private ProcessTarget selectedTarget;

    public ProcessTarget SelectedTarget => selectedTarget;

    public ProcessBrowserForm()
    {
        InitializeComponent();
        LoadProcesses();
    }

    private void InitializeComponent()
    {
        this.Text = "Select Target Process";
        this.Size = new Size(600, 400);
        this.StartPosition = FormStartPosition.CenterParent;
        this.FormBorderStyle = FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;

        listView = new ListView
        {
            View = View.Details,
            FullRowSelect = true,
            MultiSelect = false,
            Dock = DockStyle.Top,
            Height = 270
        };
        listView.Columns.Add("Process Name", 150);
        listView.Columns.Add("PID", 70);
        listView.Columns.Add("Window Title", 340);

        btnOk = new Button { Text = "OK", DialogResult = DialogResult.OK, Left = 320, Width = 80, Top = 320 };
        btnCancel = new Button { Text = "Cancel", DialogResult = DialogResult.Cancel, Left = 410, Width = 80, Top = 320 };
        btnRefresh = new Button { Text = "Refresh", Left = 500, Width = 80, Top = 320 };

        lblFilter = new Label { Text = "Filter:", Left = 10, Top = 320, Width = 40 };
        txtFilter = new TextBox { Left = 55, Top = 320, Width = 250 };

        btnOk.Click += BtnOk_Click;
        btnCancel.Click += (s, e) => this.Close();
        btnRefresh.Click += (s, e) => LoadProcesses();
        txtFilter.TextChanged += (s, e) => LoadProcesses();

        this.Controls.Add(listView);
        this.Controls.Add(btnOk);
        this.Controls.Add(btnCancel);
        this.Controls.Add(btnRefresh);
        this.Controls.Add(lblFilter);
        this.Controls.Add(txtFilter);
    }

    private void LoadProcesses()
    {
        listView.Items.Clear();
        string filter = txtFilter.Text?.ToLowerInvariant();

        Process[] processes;
        try
        {
            processes = Process.GetProcesses();
        }
        catch (Exception ex)
        {
            MessageBox.Show("Unable to enumerate processes:\n" + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return;
        }

        foreach (var proc in processes)
        {
            try
            {
                if (proc.MainWindowHandle == IntPtr.Zero || string.IsNullOrWhiteSpace(proc.MainWindowTitle))
                    continue;

                if (!string.IsNullOrEmpty(filter) &&
                    !proc.ProcessName.ToLowerInvariant().Contains(filter) &&
                    !proc.MainWindowTitle.ToLowerInvariant().Contains(filter))
                    continue;

                var item = new ListViewItem(proc.ProcessName);
                item.SubItems.Add(proc.Id.ToString());
                item.SubItems.Add(proc.MainWindowTitle);
                item.Tag = new ProcessTarget(proc.Id, proc.MainWindowHandle, proc.MainWindowTitle, proc.ProcessName);
                listView.Items.Add(item);
            }
            catch
            {
                // Permission denied or process exited, skip gracefully
            }
        }
    }

    private void BtnOk_Click(object sender, EventArgs e)
    {
        if (listView.SelectedItems.Count == 1)
        {
            selectedTarget = listView.SelectedItems[0].Tag as ProcessTarget;
            this.DialogResult = DialogResult.OK;
            this.Close();
        }
        else
        {
            MessageBox.Show("Please select a process.", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }
}
}
