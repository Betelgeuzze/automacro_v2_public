using System;
using System.Drawing;
using System.Windows.Forms;

namespace automacro.gui.Controls
{
    public partial class ConsolePanel : UserControl
    {
        public TextBox txtConsole { get; private set; } = null;
        
        public ConsolePanel()
        {
            // Remove InitializeComponent();
            SetupControls();
        }
        
        private void SetupControls()
        {
            this.SuspendLayout();
            
            txtConsole = new TextBox
            {
                Dock = DockStyle.Fill,
                Multiline = true,
                ReadOnly = true,
                Font = new Font("Consolas", 9),
                ScrollBars = ScrollBars.Both,
                WordWrap = false
            };
            
            this.Controls.Add(txtConsole);
            this.ResumeLayout(false);
        }
        
        public void AppendToLog(string message)
        {
            if (this.IsDisposed || !this.IsHandleCreated || automacro.AppState.IsShuttingDown)
                return;

            if (txtConsole.InvokeRequired)
            {
                txtConsole.Invoke(new Action(() => AppendToLog(message)));
                return;
            }

            try
            {
                txtConsole.AppendText($"{message}{Environment.NewLine}");
                txtConsole.ScrollToCaret();
            }
            catch { }
        }
        
        public void ClearLog()
        {
            if (txtConsole.InvokeRequired)
            {
                txtConsole.Invoke(new Action(ClearLog));
                return;
            }
            
            txtConsole.Clear();
        }
    }
}
