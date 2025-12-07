namespace automacro.gui
{
    partial class MainForm
    {
        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.Timer timerStatusUpdate;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabStatus;
        private System.Windows.Forms.TabPage tabAutomate;
        private System.Windows.Forms.TabPage tabMouse;

        private automacro.gui.Controls.ConfigPanel configPanel;
        private System.Windows.Forms.GroupBox groupBoxProcessTarget;
        private System.Windows.Forms.Label lblProcessTarget;
        private System.Windows.Forms.TextBox txtProcessTarget;
        private System.Windows.Forms.Button btnSelectProcessTarget;
        private System.Windows.Forms.Button btnMouseCoordinate;
        private System.Windows.Forms.Button btnRegionSelection;
        private System.Windows.Forms.TabPage tabConsole;
        private System.Windows.Forms.Timer timerAlertFlash;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabStatus = new System.Windows.Forms.TabPage();
            this.tabAutomate = new System.Windows.Forms.TabPage();
            this.tabMouse = new System.Windows.Forms.TabPage();
            this.btnMouseCoordinate = new System.Windows.Forms.Button();
            this.btnRegionSelection = new System.Windows.Forms.Button();
            this.tabConsole = new System.Windows.Forms.TabPage();
            this.timerAlertFlash = new System.Windows.Forms.Timer(this.components);
            this.timerStatusUpdate = new System.Windows.Forms.Timer(this.components);

            // Add these event handlers
            this.timerAlertFlash.Tick += new System.EventHandler(this.timerAlertFlash_Tick);
            this.timerStatusUpdate.Tick += new System.EventHandler(this.timerStatusUpdate_Tick);
            this.tabControl1.SelectedIndexChanged += new System.EventHandler(this.tabControl1_SelectedIndexChanged);
            
            this.SuspendLayout();
            
            // tabControl1
            this.tabControl1.Controls.Add(this.tabStatus);
            this.tabControl1.Controls.Add(this.tabAutomate);
            this.tabControl1.Controls.Add(this.tabMouse);
            this.tabControl1.Controls.Add(this.tabConsole);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(684, 561);
            this.tabControl1.TabIndex = 0;
            
            // tabStatus
            this.tabStatus.Location = new System.Drawing.Point(4, 24);
            this.tabStatus.Name = "tabStatus";
            this.tabStatus.Padding = new System.Windows.Forms.Padding(3);
            this.tabStatus.Size = new System.Drawing.Size(676, 533);
            this.tabStatus.TabIndex = 0;
            this.tabStatus.Text = "Status";
            this.tabStatus.UseVisualStyleBackColor = true;
            
            // configPanel
            this.configPanel = new automacro.gui.Controls.ConfigPanel();

            // groupBoxProcessTarget
            this.groupBoxProcessTarget = new System.Windows.Forms.GroupBox();
            this.groupBoxProcessTarget.Text = "Process Target";
            this.groupBoxProcessTarget.Size = new System.Drawing.Size(400, 70);
            this.groupBoxProcessTarget.Location = new System.Drawing.Point(10, 10);

            this.lblProcessTarget = new System.Windows.Forms.Label();
            this.lblProcessTarget.Text = "Selected Process:";
            this.lblProcessTarget.Location = new System.Drawing.Point(10, 30);
            this.lblProcessTarget.Size = new System.Drawing.Size(100, 23);

            this.txtProcessTarget = new System.Windows.Forms.TextBox();
            this.txtProcessTarget.ReadOnly = true;
            this.txtProcessTarget.Location = new System.Drawing.Point(115, 27);
            this.txtProcessTarget.Size = new System.Drawing.Size(180, 23);

            this.btnSelectProcessTarget = new System.Windows.Forms.Button();
            this.btnSelectProcessTarget.Text = "Select";
            this.btnSelectProcessTarget.Location = new System.Drawing.Point(305, 26);
            this.btnSelectProcessTarget.Size = new System.Drawing.Size(75, 25);

            this.groupBoxProcessTarget.Controls.Add(this.lblProcessTarget);
            this.groupBoxProcessTarget.Controls.Add(this.txtProcessTarget);
            this.groupBoxProcessTarget.Controls.Add(this.btnSelectProcessTarget);

            this.tabAutomate.Controls.Add(this.groupBoxProcessTarget);
            this.tabAutomate.Controls.Add(this.configPanel);
            this.configPanel.Dock = System.Windows.Forms.DockStyle.None;
            this.configPanel.Location = new System.Drawing.Point(10, 90);
            this.configPanel.Size = new System.Drawing.Size(650, 420);

            // tabAutomate
            this.tabAutomate.Location = new System.Drawing.Point(4, 24);
            this.tabAutomate.Name = "tabAutomate";
            this.tabAutomate.Padding = new System.Windows.Forms.Padding(3);
            this.tabAutomate.Size = new System.Drawing.Size(676, 533);
            this.tabAutomate.TabIndex = 1;
            this.tabAutomate.Text = "Automate";
            this.tabAutomate.UseVisualStyleBackColor = true;
            
            // tabMouse
            this.tabMouse.Location = new System.Drawing.Point(4, 24);
            this.tabMouse.Name = "tabMouse";
            this.tabMouse.Padding = new System.Windows.Forms.Padding(3);
            this.tabMouse.Size = new System.Drawing.Size(676, 533);
            this.tabMouse.TabIndex = 2;
            this.tabMouse.Text = "Mouse";
            this.tabMouse.UseVisualStyleBackColor = true;

            // btnMouseCoordinate
            this.btnMouseCoordinate.Size = new System.Drawing.Size(120, 60);
            this.btnMouseCoordinate.Location = new System.Drawing.Point(40, 40);
            this.btnMouseCoordinate.Name = "btnMouseCoordinate";
            this.btnMouseCoordinate.Text = "Mouse Coordinate";
            this.btnMouseCoordinate.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnMouseCoordinate.UseVisualStyleBackColor = true;

            // btnRegionSelection
            this.btnRegionSelection.Size = new System.Drawing.Size(120, 60);
            this.btnRegionSelection.Location = new System.Drawing.Point(220, 40);
            this.btnRegionSelection.Name = "btnRegionSelection";
            this.btnRegionSelection.Text = "Region Selection";
            this.btnRegionSelection.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.btnRegionSelection.UseVisualStyleBackColor = true;

            this.tabMouse.Controls.Add(this.btnMouseCoordinate);
            this.tabMouse.Controls.Add(this.btnRegionSelection);

            // tabConsole
            this.tabConsole.Location = new System.Drawing.Point(4, 24);
            this.tabConsole.Name = "tabConsole";
            this.tabConsole.Padding = new System.Windows.Forms.Padding(3);
            this.tabConsole.Size = new System.Drawing.Size(676, 533);
            this.tabConsole.TabIndex = 3;
            this.tabConsole.Text = "Console";
            this.tabConsole.UseVisualStyleBackColor = true;
            
            // timerAlertFlash
            this.timerAlertFlash.Interval = 500;
            
            // timerStatusUpdate
            this.timerStatusUpdate.Interval = 1000;
            
            // MainForm
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(684, 561);
            this.Controls.Add(this.tabControl1);
            this.MinimumSize = new System.Drawing.Size(700, 600);
            this.Name = "MainForm";
            this.Text = "AutoMacro - Use F9 to toggle, \\ to emergency stop";
            
            this.ResumeLayout(false);
        }
    }
}
