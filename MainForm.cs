using System;
using System.Drawing;
using System.Windows.Forms;
using automacro.config;
using automacro.models;
using automacro.modules;
using automacro.gui.Controls;
using automacro.utils;
using Gma.System.MouseKeyHook;
using OpenCvSharp;

namespace automacro.gui
{
    public partial class MainForm : Form
    {
        // UI setup and config management moved to MainForm.Config.cs
        // Autoclick logic moved to MainForm.AutoClick.cs
        private Automacro.Models.ProcessTarget _selectedProcessTarget;
        private automacro.modules.MacroEngine macroEngine;
        private automacro.modules.ImageDetector imageDetector;
        private automacro.modules.MonitoringService monitoringService;

private void btnSelectProcessTarget_Click(object sender, EventArgs e)
        {
            var target = Automacro.Utils.ProcessSelector.SelectProcess(this);
if (target != null)
            {
                _selectedProcessTarget = target;
                txtProcessTarget.Text = target.ToString();
                macroEngine?.SetProcessTarget(target);

                // Update config with selected process info
if (_config != null && target != null)
                {
                    _config.TargetGame.Exe = target.ProcessName;
                    _config.TargetGame.Name = target.WindowTitle;
                }

                // Re-initialize coordinator and macroEngine with updated config and process target
if (_config != null)
                {
_coordinator = new SystemCoordinator(_config, _selectedProcessTarget);
_coordinator.OnStatusUpdate += HandleStatusUpdate;
_coordinator.OnSystemAlert += HandleSystemAlert;

                macroEngine = new automacro.modules.MacroEngine(_config, _selectedProcessTarget);
                // MacroEngine should only be started/stopped via F9 hotkey, not GUI status
                if (regionMonitorModule != null)
                {
                    // Always update macroEngine reference in regionMonitorModule
                    regionMonitorModule.Configure(
                        imageDetector,
                        gameWindow: IntPtr.Zero,
                        region: _selectedRegion ?? new Rectangle(0, 0, 1920, 1080),
                        template: null,
                        threshold: 0.85f,
                        clickCoordinate: new System.Drawing.Point(_config.MouseX, _config.MouseY),
                        macroEngine: macroEngine
                    );
                }
                else if (chkAutoClick != null && chkAutoClick.Checked)
                {
                    // If autoclick is enabled but regionMonitorModule is null, create and configure it
                    IntPtr gameWindow = IntPtr.Zero;
                    var gameProcess = _selectedProcessTarget != null && _selectedProcessTarget.ProcessId != 0
                        ? WindowUtils.GetGameProcess(_selectedProcessTarget)
                        : null;
                    if (gameProcess != null)
                        gameWindow = gameProcess.MainWindowHandle;

                    Rectangle region = _selectedRegion ?? new Rectangle(0, 0, 1920, 1080);
                    Mat template = null;
                    string templatePath = "resources/template.png";
                    if (System.IO.File.Exists(templatePath))
                        template = new OpenCvSharp.Mat(templatePath, OpenCvSharp.ImreadModes.Color);

                    float threshold = 0.85f;
                    System.Drawing.Point clickCoordinate = new System.Drawing.Point(_config.MouseX, _config.MouseY);

                    regionMonitorModule = new RegionMonitorModule();
                    regionMonitorModule.Configure(
                        imageDetector,
                        gameWindow,
                        region,
                        template,
                        threshold,
                        clickCoordinate,
                        macroEngine
                    );
                    regionMonitorModule.StartMonitoring();
                }
if (_globalHotkeyManager != null)
    _globalHotkeyManager.Dispose();
_globalHotkeyManager = new GlobalHotkeyManager(this.Handle, _coordinator);
_globalHotkeyManager.OnHotkeyTriggered += HandleHotkeyTriggered;
if (consolePanel == null)
    consolePanel = new ConsolePanel();
imageDetector = new automacro.modules.ImageDetector(_config, consolePanel);
monitoringService = new automacro.modules.MonitoringService(_config, _selectedProcessTarget, consolePanel, imageDetector ?? new automacro.modules.ImageDetector(_config, consolePanel));
                }
            }
            else
            {
                txtProcessTarget.Text = "";
                _selectedProcessTarget = null;
                macroEngine?.SetProcessTarget(null);
            }
        }
        private SystemCoordinator _coordinator;
        private AppConfig _config;
        private ConfigManager _configManager;
        private GlobalHotkeyManager _globalHotkeyManager;
        
        private StatusPanel statusPanel;
        // configPanel is declared in Designer
        private TelegramConfigPanel telegramConfigPanel;
        private MessagesConfigPanel messagesConfigPanel;
        private ConsolePanel consolePanel;
        
        // Removed unused btnSaveConfig field
        
        private IKeyboardMouseEvents _globalHook;

        public MainForm()
        {
            InitializeComponent();
            this.btnSelectProcessTarget.Click += btnSelectProcessTarget_Click;

            // Set custom window title
            this.Text = "AutoMacro";

            // Make form compact and scalable
            this.MinimumSize = new System.Drawing.Size(510, 700);
            this.Size = new System.Drawing.Size(510, 700);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.AutoScaleMode = AutoScaleMode.Font;
            this.FormBorderStyle = FormBorderStyle.Sizable;

            InitializeCustomComponents();

            btnMouseCoordinate.Click += BtnMouseCoordinate_Click;
            btnRegionSelection.Click += btnRegionSelection_Click;
            _globalHook = null;

            // Defer config, coordinator, and hotkey manager setup until UI is shown
            this.Shown += (s, e) =>
            {
                _configManager = new ConfigManager();
                _config = _configManager.LoadConfig();

                configPanel.LoadConfig(_config);
                telegramConfigPanel.LoadTelegramConfig(_config.Telegram);
                messagesConfigPanel.LoadMessagesConfig(_config.Messages);

                // Require user to select process before running
                if (_selectedProcessTarget == null)
                {
                    MessageBox.Show("Please select a process target before using the application.", "Process Required", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                // Load region from config
                if (_config.RegionWidth > 0 && _config.RegionHeight > 0)
                {
                    _selectedRegion = new Rectangle(_config.RegionX, _config.RegionY, _config.RegionWidth, _config.RegionHeight);
                }

                _coordinator = new SystemCoordinator(_config);
                _coordinator.OnStatusUpdate += HandleStatusUpdate;
                _coordinator.OnSystemAlert += HandleSystemAlert;

                macroEngine = new automacro.modules.MacroEngine(_config);

                _globalHotkeyManager = new GlobalHotkeyManager(this.Handle, _coordinator);
                _globalHotkeyManager.OnHotkeyTriggered += HandleHotkeyTriggered;

                timerStatusUpdate.Start();
            };
        }
        
        // MainForm_Load removed; hotkey manager now initialized in Shown event
        
        protected override void WndProc(ref Message m)
        {
            // Check if hotkey manager is initialized before processing
            if (_globalHotkeyManager != null)
            {
                _globalHotkeyManager.ProcessHotkeyMessage(m);
            }
            base.WndProc(ref m);
        }
        
        private void InitializeCustomComponents()
        {
            statusPanel = new StatusPanel();
            // configPanel is initialized in Designer
            telegramConfigPanel = new TelegramConfigPanel();
            messagesConfigPanel = new MessagesConfigPanel();
            consolePanel = new ConsolePanel();

            // Add auto click checkbox to Mouse tab
            chkAutoClick = new CheckBox
            {
                Text = "Enable Auto Click",
                Dock = DockStyle.Top,
                Checked = false
            };
            tabMouse.Controls.Add(chkAutoClick);
            chkAutoClick.CheckedChanged += ChkAutoClick_CheckedChanged;

            // Ensure all tabs and panels scale with window
            tabStatus.Controls.Add(statusPanel);
            statusPanel.Dock = DockStyle.Top;
            tabStatus.Controls.Add(telegramConfigPanel);
            telegramConfigPanel.Dock = DockStyle.Top;
            tabStatus.Controls.Add(messagesConfigPanel);
            messagesConfigPanel.Dock = DockStyle.Top;

            // configPanel is now added in Designer; do not add again here

            tabConsole.Controls.Add(consolePanel);
            consolePanel.Dock = DockStyle.Fill;

            tabControl1.Dock = DockStyle.Fill;

            configPanel.ConfigChanged += OnConfigChanged;
            configPanel.SaveRequested += SaveConfigFromUI;
        }
        
        // Save Config button panel removed; button will be managed by ConfigPanel
        
        
        private void HandleHotkeyTriggered(string message)
        {
            consolePanel.AppendToLog($"[GLOBAL HOTKEY] {message}");
            if (macroEngine != null)
            {
                if (message.Contains("F9: Starting macro"))
                {
                    macroEngine.Start();
                    if (regionMonitorModule != null && chkAutoClick != null && chkAutoClick.Checked)
                    {
                        // Update macroEngine reference in regionMonitorModule after macro starts
                        regionMonitorModule.Configure(
                            imageDetector,
                            _selectedProcessTarget != null && _selectedProcessTarget.ProcessId != 0
                                ? WindowUtils.GetGameProcess(_selectedProcessTarget)?.MainWindowHandle ?? IntPtr.Zero
                                : IntPtr.Zero,
                            _selectedRegion ?? new Rectangle(0, 0, 1920, 1080),
                            System.IO.File.Exists("resources/template.png")
                                ? new OpenCvSharp.Mat("resources/template.png", OpenCvSharp.ImreadModes.Color)
                                : null,
                            0.85f,
                            new System.Drawing.Point(_config.MouseX, _config.MouseY),
                            macroEngine
                        );
                    }
                }
                else if (message.Contains("F9: Stopping macro"))
                    macroEngine.Stop();
            }
        }
        
        private void HandleStatusUpdate(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => HandleStatusUpdate(message)));
                return;
            }
            
            consolePanel.AppendToLog(message);
            UpdateStatusLabels(message);
        }
        
        private void HandleSystemAlert(string message)
        {
            if (InvokeRequired)
            {
                Invoke(new Action(() => HandleSystemAlert(message)));
                return;
            }
            
            consolePanel.AppendToLog($"[ALERT] {message}");
            statusPanel.ShowAlert(message, true);
        }
        
        private void UpdateStatusLabels(string message)
        {
            if (message.Contains("🟢") || message.Contains("started"))
            {
                statusPanel.UpdateMacroStatus("RUNNING", Color.Green);
            }
            else if (message.Contains("🔴") || message.Contains("stopped"))
            {
                statusPanel.UpdateMacroStatus("STOPPED", Color.Red);
                statusPanel.ShowAlert("", false);
            }
            else if (message.Contains("paused"))
            {
                statusPanel.UpdateMacroStatus("PAUSED", Color.Orange);
            }
        }

        private void OnTelegramConfigChanged(TelegramConfig telegramConfig)
        {
            if (_config != null)
            {
                _config.Telegram = telegramConfig;
            }
        }

        private void SaveTelegramConfigFromUI()
        {
            try
            {
                if (_config != null)
                {
                    _config.Telegram = telegramConfigPanel.GetTelegramConfig();
                    bool saved = _configManager.SaveConfig(_config);

                    if (saved)
                    {
                        MessageBox.Show("✅ Telegram configuration saved successfully!", "Success",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        consolePanel.AppendToLog("✅ Telegram configuration saved successfully");
                    }
                    else
                    {
                        MessageBox.Show("❌ Failed to save Telegram configuration!", "Error",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                        consolePanel.AppendToLog("❌ Failed to save Telegram configuration");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"❌ Error saving Telegram config: {ex.Message}", "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                consolePanel.AppendToLog($"❌ Error saving Telegram config: {ex.Message}");
            }
        }
        
        // private void btnSaveConfig_Click(object sender, EventArgs e)
        // {
        //     SaveConfigFromUI();
        // }
        
        
        private void timerStatusUpdate_Tick(object sender, EventArgs e)
        {
            UpdateGameStatus();
        }
        
        private void UpdateGameStatus()
        {
            try
            {
var gameProcess = _selectedProcessTarget != null && _selectedProcessTarget.ProcessId != 0
    ? WindowUtils.GetGameProcess(_selectedProcessTarget)
    : WindowUtils.GetGameProcess(_config.TargetGame.Exe);
                statusPanel.UpdateGameStatus(
                    gameProcess != null ? "RUNNING" : "NOT RUNNING",
                    gameProcess != null ? Color.Green : Color.Red
                );
            }
            catch
            {
                statusPanel.UpdateGameStatus("ERROR", Color.Red);
            }
        }
        
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            // Dispose background threads asynchronously to avoid UI hang
            System.Threading.Tasks.Task.Run(() =>
            {
                _coordinator.StopAllSystems();
                _globalHotkeyManager?.Dispose();
            });
            timerStatusUpdate.Stop();
            base.OnFormClosing(e);
        }
        
        private void tabControl1_SelectedIndexChanged(object sender, EventArgs e) { }
        private void timerAlertFlash_Tick(object sender, EventArgs e) { }

        private void BtnMouseCoordinate_Click(object sender, EventArgs e)
        {
var gameProcess = _selectedProcessTarget != null && _selectedProcessTarget.ProcessId != 0
    ? WindowUtils.GetGameProcess(_selectedProcessTarget)
    : null;
            if (gameProcess == null)
            {
                MessageBox.Show("Game window not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            WindowUtils.BringWindowToFront(gameProcess.MainWindowHandle);

            if (_globalHook != null)
            {
                _globalHook.MouseDownExt -= GlobalHook_MouseDownExt;
                _globalHook.Dispose();
                _globalHook = null;
            }

            _globalHook = Hook.GlobalEvents();
            _globalHook.MouseDownExt += GlobalHook_MouseDownExt;
        }

        // Region selection logic
        private Rectangle? _selectedRegion = null;

        private void btnRegionSelection_Click(object sender, EventArgs e)
        {
            var gameProcess = _selectedProcessTarget != null && _selectedProcessTarget.ProcessId != 0
                ? WindowUtils.GetGameProcess(_selectedProcessTarget)
                : null;
            if (gameProcess == null)
            {
                MessageBox.Show("Game window not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            WindowUtils.BringWindowToFront(gameProcess.MainWindowHandle);

            ShowNonModalPopup("Click and drag to select a region in the game window.");

            // Start region selection using global mouse hook
            if (_globalHook != null)
            {
                _globalHook.MouseDownExt -= GlobalHook_RegionMouseDown;
                _globalHook.MouseUpExt -= GlobalHook_RegionMouseUp;
                _globalHook.Dispose();
                _globalHook = null;
            }

            _regionStart = System.Drawing.Point.Empty;
            _regionEnd = System.Drawing.Point.Empty;
            _globalHook = Hook.GlobalEvents();
            _globalHook.MouseDownExt += GlobalHook_RegionMouseDown;
            _globalHook.MouseUpExt += GlobalHook_RegionMouseUp;
        }

        private System.Drawing.Point _regionStart;
        private System.Drawing.Point _regionEnd;

        private void GlobalHook_RegionMouseDown(object sender, MouseEventExtArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            _regionStart = new System.Drawing.Point(e.X, e.Y);
        }

        private void GlobalHook_RegionMouseUp(object sender, MouseEventExtArgs e)
        {
            if (e.Button != MouseButtons.Left) return;
            _regionEnd = new System.Drawing.Point(e.X, e.Y);

            var gameProcess = _selectedProcessTarget != null && _selectedProcessTarget.ProcessId != 0
                ? WindowUtils.GetGameProcess(_selectedProcessTarget)
                : null;
            if (gameProcess == null)
            {
                _globalHook.MouseDownExt -= GlobalHook_RegionMouseDown;
                _globalHook.MouseUpExt -= GlobalHook_RegionMouseUp;
                _globalHook.Dispose();
                _globalHook = null;
                ShowNonModalPopup("Game window not found.");
                return;
            }
            var rect = WindowUtils.GetWindowRect(gameProcess.MainWindowHandle);

            // Calculate region relative to game window
            int x = Math.Min(_regionStart.X, _regionEnd.X) - rect.Left;
            int y = Math.Min(_regionStart.Y, _regionEnd.Y) - rect.Top;
            int width = Math.Abs(_regionEnd.X - _regionStart.X);
            int height = Math.Abs(_regionEnd.Y - _regionStart.Y);

            _selectedRegion = new Rectangle(x, y, width, height);

            // Save region to config
            _config.RegionX = x;
            _config.RegionY = y;
            _config.RegionWidth = width;
            _config.RegionHeight = height;
            _configManager.SaveConfig(_config);

            // Unregister hook
            _globalHook.MouseDownExt -= GlobalHook_RegionMouseDown;
            _globalHook.MouseUpExt -= GlobalHook_RegionMouseUp;
            _globalHook.Dispose();
            _globalHook = null;

            ShowNonModalPopup($"Region selected: X={x}, Y={y}, W={width}, H={height}");

            // Capture region screenshot as template
var bmp = WindowUtils.CaptureRegion(gameProcess.MainWindowHandle, _selectedRegion.Value, consolePanel);
            if (bmp != null)
            {
                string templatePath = "resources/template.png";
                bmp.Save(templatePath);
                ShowNonModalPopup($"Template saved: {templatePath}");
            }
            else
            {
                ShowNonModalPopup("Failed to capture template screenshot.");
            }

            WindowUtils.BringWindowToFront(this.Handle);
        }

        // Auto click logic


private void AutoClickTimer_Tick(object sender, EventArgs e)
{
    var gameProcess = _selectedProcessTarget != null && _selectedProcessTarget.ProcessId != 0
        ? WindowUtils.GetGameProcess(_selectedProcessTarget)
        : null;
    if (gameProcess == null || _selectedRegion == null)
        return;

    var bmp = WindowUtils.CaptureRegion(gameProcess.MainWindowHandle, _selectedRegion.Value);
    if (bmp != null)
    {
        // Load template from disk
        string templatePath = "resources/template.png";
        if (System.IO.File.Exists(templatePath))
        {
            using var msRegion = new System.IO.MemoryStream();
            bmp.Save(msRegion, System.Drawing.Imaging.ImageFormat.Png);
            msRegion.Position = 0;
            using var srcMat = OpenCvSharp.Mat.FromStream(msRegion, OpenCvSharp.ImreadModes.Color);

            using var templMat = new OpenCvSharp.Mat(templatePath, OpenCvSharp.ImreadModes.Color);
            using var result = new OpenCvSharp.Mat();

            OpenCvSharp.Cv2.MatchTemplate(srcMat, templMat, result, OpenCvSharp.TemplateMatchModes.CCoeffNormed);
            OpenCvSharp.Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out OpenCvSharp.Point minLoc, out OpenCvSharp.Point maxLoc);

            if (maxVal > 0.85)
            {
                WindowUtils.SendMouseClick(gameProcess.MainWindowHandle, _config.MouseX, _config.MouseY);
            }
        }
    }
}

        private void GlobalHook_MouseDownExt(object sender, MouseEventExtArgs e)
        {
            // Only handle left mouse button and only once
            if (e.Button != MouseButtons.Left) return;

            var gameProcess = _selectedProcessTarget != null && _selectedProcessTarget.ProcessId != 0
                ? WindowUtils.GetGameProcess(_selectedProcessTarget)
                : null;
            if (gameProcess == null)
            {
                _globalHook.MouseDownExt -= GlobalHook_MouseDownExt;
                _globalHook.Dispose();
                _globalHook = null;
                MessageBox.Show("Game window not found.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            var rect = WindowUtils.GetWindowRect(gameProcess.MainWindowHandle);

            // Only handle if click is inside game window
            if (e.X < rect.Left || e.X > rect.Right || e.Y < rect.Top || e.Y > rect.Bottom)
                return;

            int relX = e.X - rect.Left;
            int relY = e.Y - rect.Top;

            // Unregister hook BEFORE showing popup to avoid repeated triggers
            _globalHook.MouseDownExt -= GlobalHook_MouseDownExt;
            _globalHook.Dispose();
            _globalHook = null;

            _config.MouseX = relX;
            _config.MouseY = relY;
            _configManager.SaveConfig(_config);

            ShowNonModalPopup($"Mouse coordinate relative to game window: {relX}, {relY} (saved)");
            WindowUtils.BringWindowToFront(this.Handle);
        }

        // Template matching with OpenCV
        private string MatchTemplateOpenCV(string screenshotPath, string templatePath, out double matchScore)
        {
            matchScore = 0.0;
            try
            {
                using var src = new OpenCvSharp.Mat(screenshotPath, OpenCvSharp.ImreadModes.Color);
                using var templ = new OpenCvSharp.Mat(templatePath, OpenCvSharp.ImreadModes.Color);
                using var result = new OpenCvSharp.Mat();

                OpenCvSharp.Cv2.MatchTemplate(src, templ, result, OpenCvSharp.TemplateMatchModes.CCoeffNormed);
                OpenCvSharp.Cv2.MinMaxLoc(result, out double minVal, out double maxVal, out OpenCvSharp.Point minLoc, out OpenCvSharp.Point maxLoc);

                matchScore = maxVal;
                return $"Template match score: {maxVal:F3} at ({maxLoc.X},{maxLoc.Y})";
            }
            catch (Exception ex)
            {
                return $"Template matching error: {ex.Message}";
            }
        }

        private void ShowNonModalPopup(string message)
        {
            Form popup = new Form
            {
            Size = new System.Drawing.Size(320, 100),
                StartPosition = FormStartPosition.CenterScreen,
                TopMost = true,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                ShowInTaskbar = false,
                Text = "Mouse Coordinate"
            };
            Label lbl = new Label
            {
                Text = message,
                Dock = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleCenter,
                Font = new Font("Segoe UI", 12)
            };
            popup.Controls.Add(lbl);

            var timer = new System.Windows.Forms.Timer { Interval = 2500 };
            timer.Tick += (s, e) =>
            {
                popup.Close();
                timer.Dispose();
            };
            timer.Start();

            popup.Show();
        }
    }
}
