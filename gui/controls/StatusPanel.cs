using System;
using System.Drawing;
using System.Windows.Forms;

namespace automacro.gui.Controls
{
    public partial class StatusPanel : UserControl
    {
        public Label lblMacroStatus { get; private set; } = null;
        public Label lblGameStatus { get; private set; } = null;
        public Label lblFocusStatus { get; private set; } = null;
        public Label lblDetectorStatus { get; private set; } = null;
        public Label lblAlertStatus { get; private set; } = null;
        
        public StatusPanel()
        {
            // Remove InitializeComponent();
            SetupControls();
        }
        
        private void SetupControls()
        {
            this.SuspendLayout();
            
            // Create GroupBox
            var grpStatus = new GroupBox
            {
                Text = "System Status",
                Location = new Point(8, 0),
                Size = new Size(430, 120),
                Anchor = AnchorStyles.Top | AnchorStyles.Left
            };
            
            // Create labels
            var label5 = new Label { Text = "Macro Status:", Location = new Point(20, 15) };
            lblMacroStatus = new Label 
            { 
                Text = "STOPPED", 
                Location = new Point(120, 15),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Red
            };
            
            var label7 = new Label { Text = "Game Status:", Location = new Point(20, 40) };
            lblGameStatus = new Label 
            { 
                Text = "UNKNOWN", 
                Location = new Point(120, 40),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            
            var label9 = new Label { Text = "Focus Status:", Location = new Point(20, 65) };
            lblFocusStatus = new Label 
            { 
                Text = "UNKNOWN", 
                Location = new Point(120, 65),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            
            var label11 = new Label { Text = "Detector Status:", Location = new Point(20, 90) };
            lblDetectorStatus = new Label 
            { 
                Text = "UNKNOWN", 
                Location = new Point(120, 90),
                Font = new Font("Segoe UI", 9, FontStyle.Bold)
            };
            
            lblAlertStatus = new Label 
            { 
                Text = "",
                Location = new Point(120, 120),
                Font = new Font("Segoe UI", 9, FontStyle.Bold),
                ForeColor = Color.Red,
                Visible = false
            };
            
            // Add controls to GroupBox
            grpStatus.Controls.AddRange(new Control[] 
            {
                label5, lblMacroStatus,
                label7, lblGameStatus,
                label9, lblFocusStatus,
                label11, lblDetectorStatus,
                lblAlertStatus
            });
            
            // Add GroupBox to panel
            this.Controls.Add(grpStatus);
            
            this.ResumeLayout(false);
        }
        
        public void UpdateMacroStatus(string status, Color color)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateMacroStatus(status, color)));
                return;
            }
            
            lblMacroStatus.Text = status;
            lblMacroStatus.ForeColor = color;
        }
        
        public void UpdateGameStatus(string status, Color color)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => UpdateGameStatus(status, color)));
                return;
            }
            
            lblGameStatus.Text = status;
            lblGameStatus.ForeColor = color;
        }
        
        public void ShowAlert(string message, bool show = true)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => ShowAlert(message, show)));
                return;
            }
            
            lblAlertStatus.Text = message;
            lblAlertStatus.Visible = show;
        }
    }
}
