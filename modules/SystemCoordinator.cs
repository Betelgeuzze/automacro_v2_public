using System;
using System.Threading;
using automacro.config;
using automacro.models;
using automacro.utils;

namespace automacro.modules
{
    public class SystemCoordinator
    {
        private readonly AppConfig _config;
    private readonly MacroEngine _macroEngine;
    public MacroEngine MacroEngine => _macroEngine;
        private readonly MonitoringService _monitoringService;
        
        private bool _isRunning = false;

        public event Action<string> OnStatusUpdate;
        public event Action<string> OnSystemAlert;

        public SystemCoordinator(AppConfig config)
        {
            _config = config;
            _macroEngine = new MacroEngine(config);
            _monitoringService = new MonitoringService(config);

            _macroEngine.OnLog += (message) => Log($"[MACRO] {message}");
            _macroEngine.OnPauseStateChanged += (state) => Log($"[MACRO] {state}");

            _monitoringService.OnStatusUpdate += (message) => Log($"[MONITOR] {message}");
            _monitoringService.OnAlert += (alert) => HandleMonitoringAlert(alert);
        }

        public SystemCoordinator(AppConfig config, Automacro.Models.ProcessTarget target)
        {
            _config = config;
            _macroEngine = new MacroEngine(config, target);
            _monitoringService = new MonitoringService(config, target);

            _macroEngine.OnLog += (message) => Log($"[MACRO] {message}");
            _macroEngine.OnPauseStateChanged += (state) => Log($"[MACRO] {state}");

            _monitoringService.OnStatusUpdate += (message) => Log($"[MONITOR] {message}");
            _monitoringService.OnAlert += (alert) => HandleMonitoringAlert(alert);
        }

        public void StartAllSystems()
        {
            if (_isRunning) return;

            _isRunning = true;

            // Reset UI detection state for double-check logic
            var wasUiDetectedField = _monitoringService.GetType().GetField("_wasUiDetected", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            if (wasUiDetectedField != null)
                wasUiDetectedField.SetValue(_monitoringService, true);

            // Start monitoring first
            _monitoringService.StartMonitoring();
            
            // Small delay before starting macro
            Thread.Sleep(500);
            
            // Start macro
            _macroEngine.Start();

            Log("🟢 All systems started");
        }

        public void StopAllSystems()
        {
            if (!_isRunning) return;

            _isRunning = false;

            // Stop macro and monitoring asynchronously to avoid UI freeze
            System.Threading.Tasks.Task.Run(() =>
            {
                _macroEngine.Stop();
                Thread.Sleep(500);
                _monitoringService.StopMonitoring();
                Log("🔴 All systems stopped");
            });
        }

        public void PauseMacro()
        {
            _macroEngine.Pause("Manual pause");
        }

        public void ResumeMacro()
        {
            _macroEngine.Resume();
        }

        public void ToggleMacroPause()
        {
            if (_macroEngine.IsPaused)
                _macroEngine.Resume();
            else
                _macroEngine.Pause("Manual toggle");
        }
        
        public bool IsMacroRunning => _macroEngine.IsRunning;
        
        public string GetGameExeName()
        {
            return _config.TargetGame.Exe;
        }

        public void ReloadHotkeys()
        {
            _macroEngine.ReloadHotkeys();
        }

        public Automacro.Models.ProcessTarget GetProcessTarget()
        {
            // Expose the process target if MacroEngine has it
            var field = typeof(MacroEngine).GetField("_processTarget", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
            return field?.GetValue(_macroEngine) as Automacro.Models.ProcessTarget;
        }

        private void HandleMonitoringAlert(string alert)
        {
            if (alert.Contains("Popup alert sent") || 
                alert.Contains("UI check failed alert sent") || 
                alert.Contains("Game not running alert sent"))
            {
                string pauseReason;
                if (alert.Contains("Popup"))
                    pauseReason = _config.Messages.PopupDetected;
                else if (alert.Contains("UI check failed"))
                    pauseReason = _config.Messages.UiCheckFailed;
                else if (alert.Contains("Game not running"))
                    pauseReason = _config.Messages.GameNotRunning;
                else
                    pauseReason = "Monitoring alert";
                
                _macroEngine.Stop();
                _isRunning = false; // Allow restart after auto-stop
                OnSystemAlert?.Invoke($"Macro auto-stopped: {pauseReason}");
                
                Log($"🚨 Monitoring alert handled: {pauseReason}");
            }
        }

        private void Log(string message)
        {
            OnStatusUpdate?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        public void FocusGameWindow()
        {
            // We'll need to add this method to MacroEngine first
            // For now, we'll keep the old logic
            try
            {
                string gameExe = GetGameExeName();
                if (string.IsNullOrEmpty(gameExe)) return;
                
                var processes = System.Diagnostics.Process.GetProcessesByName(gameExe.Replace(".exe", ""));
                
                if (processes.Length > 0)
                {
                    foreach (var process in processes)
                    {
                        try
                        {
                            if (process.MainWindowHandle != IntPtr.Zero)
                            {
                                bool success = WindowUtils.BringWindowToFront(process.MainWindowHandle);
                                if (success)
                                {
                                    Log($"Focused game window: {process.ProcessName}");
                                    break;
                                }
                            }
                        }
catch (Exception)
                        {
                            // Removed debug log
                        }
                    }
                }
            }
catch (Exception)
            {
                Log("Warning: Could not focus game window");
            }
        }

    }
}
