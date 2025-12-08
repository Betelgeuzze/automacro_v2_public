using System;
using System.Threading;
using automacro.config;
using automacro.utils;
using automacro.models;
using Automacro.Models;

namespace automacro.modules
{
    public class MonitoringService : IDisposable
    {
        private ImageDetector _imageDetector;
        private readonly TelegramBot _telegramBot;
        private readonly AppConfig _config;
        private readonly ProcessTarget _processTarget;
        private automacro.gui.Controls.ConsolePanel _consolePanel;
        
        private Thread _monitorThread;
        private bool _isMonitoring;
        private CancellationTokenSource _cancellationToken;
        
        // Alert state tracking to prevent spam
        private DateTime _lastPopupAlert = DateTime.MinValue;
        private DateTime _lastUiAlert = DateTime.MinValue;
        private DateTime _lastGameAlert = DateTime.MinValue;
        private bool _wasGameRunning = true;
        private bool _wasPopupDetected = false;
        private bool _wasUiDetected = true;

        public event Action<string> OnStatusUpdate;
        public event Action<string> OnAlert;
        public bool IsRunning => _isMonitoring;

        public MonitoringService(AppConfig config, ProcessTarget processTarget = null, automacro.gui.Controls.ConsolePanel consolePanel = null, ImageDetector imageDetector = null)
        {
            
            _config = config;
            _processTarget = processTarget;
            _consolePanel = consolePanel ?? new automacro.gui.Controls.ConsolePanel();
            _imageDetector = imageDetector ?? new ImageDetector(config, consolePanel);
            _telegramBot = new TelegramBot(config.Telegram);
            
        }

        public void StartMonitoring()
        {
            _isMonitoring = false; // Always allow new monitoring session to start

            // Ensure previous monitor thread is stopped before starting new one
            if (_monitorThread != null && _monitorThread.IsAlive)
            {
                _isMonitoring = false;
                _cancellationToken?.Cancel();
                _monitorThread.Join(5000);
            }

            // Always use the shared ImageDetector instance

            // ADD THIS DEBUG

            _wasUiDetected = true; // Reset so UI check failure triggers alert after restart
            _lastUiAlert = DateTime.MinValue; // Reset cooldown so alert can fire after restart
            _isMonitoring = true;
            _cancellationToken = new CancellationTokenSource();
            _monitorThread = new Thread(() =>
            {
                // Log("DEBUG: Entered immediate UI check thread after restart");
                // Immediate first check for UI missing after restart
                try
                {
var gameProcess = _processTarget != null && _processTarget.ProcessId != 0
    ? WindowUtils.GetGameProcess(_processTarget)
    : null;
                    if (gameProcess != null)
                    {
                        var result = _imageDetector.CheckGameState(gameProcess.MainWindowHandle);
                        if (!result.UiDetected)
                        {
                            Log("⚠️ UI check failed (immediate after restart) - game might be crashed");
                            Log($"DEBUG: _lastUiAlert={_lastUiAlert}, now={DateTime.Now}, cooldown={_config.Timing.UiCheckCooldown}");
                            if (DateTime.Now - _lastUiAlert > TimeSpan.FromSeconds(_config.Timing.UiCheckCooldown))
                            {
                                // Log("🚨 [DEBUG] About to send UI check failed alert (immediate after restart)");
_ = _telegramBot.SendUiCheckFailedAsync(_config.Telegram, _config.Messages);
                                // Log("🚨 [DEBUG] Sent UI check failed alert (immediate after restart)");
                                _lastUiAlert = DateTime.Now;
                                // Log("DEBUG: Calling OnAlert for UI check failed alert");
                                OnAlert?.Invoke("UI check failed alert sent");
                            }
                            else
                            {
                                // Log("DEBUG: Cooldown prevented UI check alert after restart");
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Log($"❌ Immediate UI check error: {ex.Message}");
                }
                MonitorLoop();
            });
            _monitorThread.Start();

            Log("🟢 Monitoring service started");
        }

        // Helper to check if an object is disposed
        private bool IsDisposed(IDisposable obj)
        {
            try
            {
                // Try to call ToString or any method, will throw if disposed
                obj.ToString();
                return false;
            }
            catch (ObjectDisposedException)
            {
                return true;
            }
        }

        public void StopMonitoring()
        {
            _isMonitoring = false;
            _cancellationToken?.Cancel();
            if (_monitorThread != null && _monitorThread.IsAlive)
            {
                _monitorThread.Join(5000); // Wait up to 5 seconds
            }

            _wasUiDetected = true; // Ensure state is reset for next start
            _imageDetector.Dispose();

            Log("🔴 Monitoring service stopped");
            // Removed the shutdown Telegram message
        }

        private void MonitorLoop()
        {
            while (_isMonitoring && !(_cancellationToken.Token.IsCancellationRequested) && !automacro.AppState.IsShuttingDown)
            {
                try
                {
                    // Find the game window
                    var gameProcess = _processTarget != null && _processTarget.ProcessId != 0
                        ? WindowUtils.GetGameProcess(_processTarget)
                        : null;

                    if (gameProcess == null)
                    {
                        HandleGameNotRunning();
                    }
                    else
                    {
                        HandleGameRunning(gameProcess.MainWindowHandle);
                    }

                    // Wait before next check
                    Thread.Sleep(_config.Timing.DetectInterval);
                }
                catch (Exception ex)
                {
                    Log($"❌ Monitoring error: {ex.Message}");
                    Thread.Sleep(5000); // Wait longer on error
                }
            }
        }

        private void HandleGameNotRunning()
        {
            Log("🔴 Game not running");
            
            // Only send alert if game was previously running (state change)
            if (_wasGameRunning)
            {
                // Check cooldown
                if (DateTime.Now - _lastGameAlert > TimeSpan.FromSeconds(_config.Timing.UiCheckCooldown))
                {
                    Log("🚨 Sending game not running alert");
_ = _telegramBot.SendGameNotRunningAsync(_config.Telegram, _config.Messages);
                    _lastGameAlert = DateTime.Now;
                    OnAlert?.Invoke("Game not running alert sent");
                }
            }
            
            _wasGameRunning = false;
            _wasUiDetected = false; // Reset UI state
        }

        private void HandleGameRunning(IntPtr gameWindow)
        {
            Log($"[DEBUG] Entered HandleGameRunning");
            Log($"[DEBUG] _imageDetector is null: {_imageDetector == null}");
            Log($"[DEBUG] gameWindow: {gameWindow}");
            Log($"[DEBUG] _config is null: {_config == null}");
            Log($"[DEBUG] _consolePanel is null: {_consolePanel == null}");
            try
            {
                var result = _imageDetector.CheckGameState(gameWindow);
                
                if (!result.Success)
                {
                    Log("⚠️ Could not check game state");
                    return;
                }

                // Handle popup detection (only alert on state change)
                if (result.PopupDetected && !_wasPopupDetected)
                {
                    Log(_config.Messages.PopupDetected);
                    
                    if (DateTime.Now - _lastPopupAlert > TimeSpan.FromSeconds(_config.Timing.DetectCooldown))
                    {
                        Log("🚨 Sending popup alert");
_ = _telegramBot.SendPopupDetectedAsync(_config.Telegram, _config.Messages);
                        _lastPopupAlert = DateTime.Now;
                        OnAlert?.Invoke("Popup alert sent");
                        Log("✅ Popup alert event fired");
                        StopMonitoring(); // Stop detection system when popup triggers
                    }
                }
                
                _wasPopupDetected = result.PopupDetected;

                // Handle UI detection (only alert when UI disappears)
                if (!result.UiDetected && _wasUiDetected)
                {
                    Log("⚠️ UI check failed - game might be crashed");

                    // Double-check: wait and re-check before alerting
                    Thread.Sleep(5000); // 5 second delay before re-check
                    var retryResult = _imageDetector.CheckGameState(gameWindow);
                    if (!retryResult.UiDetected)
                    {
                        if (DateTime.Now - _lastUiAlert > TimeSpan.FromSeconds(_config.Timing.UiCheckCooldown))
                        {
                            // Log("🚨 [DEBUG] About to send UI check failed alert (double-checked)");
_ = _telegramBot.SendUiCheckFailedAsync(_config.Telegram, _config.Messages);
                            // Log("🚨 [DEBUG] Sent UI check failed alert (double-checked)");
                            _lastUiAlert = DateTime.Now;
                            OnAlert?.Invoke("UI check failed alert sent");
                        }
                    }
                    else
                    {
                        Log("✅ UI check false positive, no alert sent");
                    }
                }

                _wasUiDetected = result.UiDetected;
                _wasGameRunning = true; // Game is running

                if (result.UiDetected && !result.PopupDetected)
                {
                    Log("✅ Game running normally");
                }
            }
            catch (Exception ex)
            {
                Log($"❌ Error in HandleGameRunning: {ex.Message}");
            }
        }

        private void Log(string message)
        {
            OnStatusUpdate?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
            if (_consolePanel != null)
                _consolePanel.AppendToLog($"[{DateTime.Now:HH:mm:ss}] {message}");
        }

        // Dispose pattern for memory management
        public void Dispose()
        {
            StopMonitoring();
            _imageDetector?.Dispose();
            _cancellationToken?.Dispose();
            // Unsubscribe all events
            OnStatusUpdate = null;
            OnAlert = null;
        }
    }
}
