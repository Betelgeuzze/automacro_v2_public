using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using automacro.models;

namespace automacro.gui.Controls
{
    public partial class ConfigPanel : UserControl
    {
        private AppConfig _config;
        
        // Events
        public event Action<AppConfig> ConfigChanged;
        public event Action SaveRequested;
        
        // Controls
        private TextBox txtMainKey;
        private NumericUpDown numMainInterval;
        private NumericUpDown numMainDuration;
        // Save button removed

        // (Telegram controls removed)
        
        private TextBox[] txtHotkeys;
        private NumericUpDown[] numIntervals;
        private NumericUpDown[] numDurations;
        private CheckBox[] chkEnabled;
        
        public ConfigPanel()
        {
            // Initialize with default config
            _config = new AppConfig();
            
            SetupControls();
            InitializeHotkeyArrays();
        }
        
        private void SetupControls()
        {
            this.SuspendLayout();
            
            // Create main container panel
            var mainPanel = new Panel
            {
                Dock = DockStyle.Fill,
                AutoScroll = true,
                MinimumSize = new Size(400, 300)
            };

            // Main Loop Group (compact, manual layout)
            var grpMainLoop = new GroupBox
            {
                Text = "Main Loop Configuration",
                Location = new Point(10, 10),
                Size = new Size(400, 80)
            };

            var lblMainKey = new Label { Text = "Main Key:", Location = new Point(15, 30), Size = new Size(70, 23) };
            txtMainKey = new TextBox { Location = new Point(90, 27), Size = new Size(80, 23) };
            var lblInterval = new Label { Text = "Interval:", Location = new Point(180, 30), Size = new Size(70, 23) };
            numMainInterval = new NumericUpDown { Location = new Point(265, 27), Size = new Size(100, 23), Maximum = 100000 };
            var lblDuration = new Label { Text = "Duration:", Location = new Point(15, 55), Size = new Size(70, 23) };
            numMainDuration = new NumericUpDown { Location = new Point(90, 52), Size = new Size(80, 23), Maximum = 10000 };

            grpMainLoop.Controls.AddRange(new Control[] { lblMainKey, txtMainKey, lblInterval, numMainInterval, lblDuration, numMainDuration });

            // Hotkeys Group (compact, manual layout)
            var grpHotkeys = new GroupBox
            {
                Text = "Hotkey Configuration",
                Location = new Point(10, 100),
                Size = new Size(400, 230)
            };

txtHotkeys = new TextBox[7];
numIntervals = new NumericUpDown[7];
numDurations = new NumericUpDown[7];
chkEnabled = new CheckBox[7];

for (int i = 0; i < 7; i++)
{
    int y = 30 + i * 26;
    var label = new Label { Text = $"Hotkey {i + 1}:", Location = new Point(15, y), Size = new Size(70, 23) };
    txtHotkeys[i] = new TextBox { Location = new Point(90, y), Size = new Size(60, 23) };
    numIntervals[i] = new NumericUpDown { Location = new Point(155, y), Size = new Size(80, 23), Maximum = 2000000 };
    numDurations[i] = new NumericUpDown { Location = new Point(240, y), Size = new Size(70, 23), Maximum = 10000 };
    chkEnabled[i] = new CheckBox { Text = "Enabled", Location = new Point(320, y), AutoSize = true };
    grpHotkeys.Controls.AddRange(new Control[] { label, txtHotkeys[i], numIntervals[i], numDurations[i], chkEnabled[i] });
}

            // Save Button
            mainPanel.Controls.Add(grpMainLoop);
            mainPanel.Controls.Add(grpHotkeys);

            var btnSave = new Button
            {
                Text = "Save Config",
                Size = new Size(120, 36),
                Location = new Point(280, 330)
            };
            btnSave.Click += (s, e) => { SaveToConfig(); SaveRequested?.Invoke(); };
            mainPanel.Controls.Add(btnSave);

            this.Controls.Add(mainPanel);

            this.ResumeLayout(false);
        }
        
        private void InitializeHotkeyArrays()
        {
            // Arrays are already initialized in SetupControls
        }
        
        public void LoadConfig(AppConfig config)
        {
            _config = config;
            UpdateControls();
        }
        
        public AppConfig GetConfig()
        {
            SaveToConfig();
            return _config;
        }
        
        private void UpdateControls()
        {
            if (_config == null) return;
            
            // Main Loop
            txtMainKey.Text = _config.MainLoop.Key;
            numMainInterval.Value = _config.MainLoop.Interval;
            numMainDuration.Value = _config.MainLoop.PressDuration;
            
            // Removed debug log
            
            // Hotkeys
            if (_config.Macro.Hotkeys != null)
            {
for (int i = 0; i < 7; i++)
{
    string key = (i + 1).ToString();
    if (_config.Macro.Hotkeys.ContainsKey(key))
    {
        var hk = _config.Macro.Hotkeys[key];
        txtHotkeys[i].Text = hk.Key;
        numIntervals[i].Value = hk.Interval;
        numDurations[i].Value = hk.PressDuration;
        chkEnabled[i].Checked = hk.Enabled;
        
        // Removed debug log
    }
}
            }
            else
            {
                // Removed debug log
            }

            // (Telegram section removed)
        }
        
private void SaveToConfig()
        {
            if (_config == null)
            {
                // Removed debug log
                return;
            }

            // Log all UI controls for null
            // Removed debug log
            // Removed debug log
            // Removed debug log
            // Removed debug log
            // Removed debug log
            // Removed debug log
            // Removed debug log

            // Ensure sub-objects are initialized
            if (_config.MainLoop == null)
            {
                // Removed debug log
                _config.MainLoop = new MainLoopConfig();
            }
            if (_config.Macro == null)
            {
                // Removed debug log
                _config.Macro = new MacroConfig();
            }
            if (_config.Macro.Hotkeys == null)
            {
                // Removed debug log
                _config.Macro.Hotkeys = new Dictionary<string, HotkeyConfig>();
            }

            // Main Loop
            _config.MainLoop.Key = txtMainKey?.Text ?? "";
            _config.MainLoop.Interval = (int)(numMainInterval?.Value ?? 1000);
            _config.MainLoop.PressDuration = (int)(numMainDuration?.Value ?? 100);

            // Hotkeys
for (int i = 0; i < 7; i++)
{
    if (txtHotkeys == null || numIntervals == null || numDurations == null || chkEnabled == null)
    {
        // Removed debug log
        continue;
    }
    string key = (i + 1).ToString();
    // Removed debug log
    // Removed debug log
    // Removed debug log
    // Removed debug log

    if (txtHotkeys[i] != null && numIntervals[i] != null && numDurations[i] != null && chkEnabled[i] != null)
    {
        _config.Macro.Hotkeys[key] = new HotkeyConfig
        {
            Key = txtHotkeys[i].Text ?? "",
            Interval = (int)numIntervals[i].Value,
            PressDuration = (int)numDurations[i].Value,
            Enabled = chkEnabled[i].Checked
        };
    }
}

            // For backward compatibility
            _config.Macro.MainLoopInterval = (int)(numMainInterval?.Value ?? 1000);
            _config.Macro.MainLoopKey = txtMainKey?.Text ?? "";
            _config.Macro.MainKeyPressDuration = (int)(numMainDuration?.Value ?? 100);

            // (Telegram section removed)
            ConfigChanged?.Invoke(_config);
        }
        
        // Save button handler removed
    }
}
