using System;
using System.Collections.Generic;
using System.Threading;
using automacro.config;
using automacro.models;

namespace automacro.modules
{
    public class MacroEngine
    {
        private Automacro.Models.ProcessTarget _processTarget;

        public void SetProcessTarget(Automacro.Models.ProcessTarget target)
        {
            _processTarget = target;
            _keyPressService.TargetWindowHandle = target?.WindowHandle ?? IntPtr.Zero;
        }

        private IntPtr GetTargetWindowHandle()
        {
            if (_processTarget != null && _processTarget.IsValid())
                return _processTarget.WindowHandle;
            // fallback to previous logic, e.g., find window by name
            return FindDefaultWindowHandle();
        }

        // TODO: Implement actual fallback logic for default window targeting
        private IntPtr FindDefaultWindowHandle()
        {
            return IntPtr.Zero;
        }

        private readonly AppConfig _config;
        private readonly KeyPressService _keyPressService;
        private readonly HotkeyScheduler _hotkeyScheduler;
        private readonly GameWindowManager _windowManager;
        
        private Thread _executionThread;
        private bool _isRunning;
        private bool _isPaused;
        private CancellationTokenSource _cancellationToken;
        
        // Timing and state management
        private DateTime _lastMainLoopTime = DateTime.MinValue;
        private DateTime _lastHotkeyQueueTime = DateTime.MinValue;
        private const int MAIN_LOOP_CHECK_INTERVAL = 50;
        
        // State flags for coordination
        private bool _mainLoopBlocked = false;
        private DateTime _mainLoopBlockedUntil = DateTime.MinValue;
        private int _consecutiveHotkeysQueued = 0;
        private const int MAX_CONSECUTIVE_HOTKEYS = 4; // Process max 4 hotkeys at once
        
        // Hotkey processing settings
        private const int BASE_BLOCK_DURATION = 750; // Base block for 1 hotkey
        private const int EXTRA_BLOCK_PER_HOTKEY = 350; // Additional block per extra hotkey
        private const int MIN_HOTKEY_GAP = 450; // Minimum gap between consecutive hotkeys
        
        public event Action<string> OnLog;
        public event Action<string> OnPauseStateChanged;
        
        public bool IsRunning => _isRunning;
        public bool IsPaused => _isPaused;

public MacroEngine(AppConfig config)
        {
            _config = config;
            
            // Initialize services
            _keyPressService = new KeyPressService();
            _hotkeyScheduler = new HotkeyScheduler();
            _windowManager = new GameWindowManager(config);
            
            // Wire up logging
            _keyPressService.OnLog += (msg) => Log($"[KEYPRESS] {msg}");
            _hotkeyScheduler.OnLog += (msg) => Log($"[SCHEDULER] {msg}");
            _windowManager.OnLog += (msg) => Log($"[WINDOW] {msg}");
            
            // Load initial hotkeys
            _hotkeyScheduler.LoadHotkeys(config);
            
            Log("🟢 MacroEngine initialized with multi-hotkey support");
        }

public MacroEngine(AppConfig config, Automacro.Models.ProcessTarget target)
            : this(config)
        {
            _processTarget = target;
            _keyPressService.TargetWindowHandle = target?.WindowHandle ?? IntPtr.Zero;
            _windowManager = new GameWindowManager(config, target);
            Log($"🟢 MacroEngine initialized with process target: {target?.ProcessName} (PID: {target?.ProcessId})");
        }

        public void Start()
        {
            if (_isRunning) return;

            _isRunning = true;
            _isPaused = false;
            _cancellationToken = new CancellationTokenSource();
            
            // Reset state
            _mainLoopBlocked = false;
            _mainLoopBlockedUntil = DateTime.MinValue;
            _lastMainLoopTime = DateTime.Now;
            _lastHotkeyQueueTime = DateTime.Now;
            _consecutiveHotkeysQueued = 0;
            
            // Start services
            _keyPressService.Start();
            _windowManager.RefreshGameWindow();
            
            // Reset hotkey timers
            _hotkeyScheduler.ResetAllTimers();
            
            // Start execution thread
            _executionThread = new Thread(MainExecutionLoop) 
            { 
                Name = "MacroExecution", 
                IsBackground = true 
            };
            
            _executionThread.Start();
            Log("🚀 MacroEngine started");
        }

        public void Stop()
        {
            if (!_isRunning) return;
            
            Log("🛑 Stopping MacroEngine...");
            _isRunning = false;
            _cancellationToken.Cancel();
            
            // Stop services
            _keyPressService.Stop();
            
            // Wait for thread
            Thread.Sleep(100);
            
            Log("✅ MacroEngine stopped");
        }

        public void Pause(string reason = "Manual pause")
        {
            if (!_isRunning || _isPaused) return;
            
            _isPaused = true;
            Log($"⏸️ Macro paused: {reason}");
            OnPauseStateChanged?.Invoke($"Paused: {reason}");
        }

        public void Resume()
        {
            if (!_isRunning || !_isPaused) return;
            
            _isPaused = false;
            Log("▶️ Macro resumed");
            OnPauseStateChanged?.Invoke("Resumed");
        }

        public void TogglePause()
        {
            if (_isPaused)
                Resume();
            else
                Pause("Manual toggle");
        }

        private void MainExecutionLoop()
        {
            Log("🔁 Main execution loop started");
            
            try
            {
                while (_isRunning && !_cancellationToken.Token.IsCancellationRequested)
                {
                    if (_isPaused)
                    {
                        Thread.Sleep(100);
                        continue;
                    }
                    
                    // Check window focus, but do not bring to front if not focused
                    if (!_windowManager.IsGameFocused())
                    {
                        _keyPressService.ClearQueue();
                        _mainLoopBlocked = false;
                        _consecutiveHotkeysQueued = 0;
                        Thread.Sleep(200);
                        continue;
                    }
                    
                    DateTime currentTime = DateTime.Now;
                    
                    // Check if main loop is blocked (by hotkeys)
                    if (_mainLoopBlocked)
                    {
                        if (currentTime < _mainLoopBlockedUntil)
                        {
                            // Still blocked, wait
                            int waitMs = (int)(_mainLoopBlockedUntil - currentTime).TotalMilliseconds;
                            if (waitMs > 0)
                            {
                                Thread.Sleep(Math.Min(waitMs, 100));
                            }
                            continue;
                        }
                        else
                        {
                            // Block expired
                            _mainLoopBlocked = false;
                            _consecutiveHotkeysQueued = 0;
                            Log("🔓 Main loop unblocked");
                        }
                    }
                    
                    // Process hotkeys FIRST
                    ProcessHotkeys(currentTime);
                    
                    // If hotkeys were queued, block main loop
                    if (_mainLoopBlocked)
                    {
                        continue;
                    }
                    
                    // Process main loop (only if not blocked and not too many hotkeys recently)
                    if (_consecutiveHotkeysQueued == 0 || (currentTime - _lastHotkeyQueueTime).TotalMilliseconds > 1000)
                    {
                        ProcessMainLoop(currentTime);
                    }
                    
                    Thread.Sleep(MAIN_LOOP_CHECK_INTERVAL);
                }
            }
            catch (Exception ex)
            {
                Log($"❌ Execution loop error: {ex.Message}");
            }
            finally
            {
                Log("🔁 Main execution loop ended");
            }
        }

        private void ProcessHotkeys(DateTime currentTime)
        {
            try
            {
                var dueHotkeys = _hotkeyScheduler.GetDueHotkeys(currentTime);
                
                if (dueHotkeys.Count > 0)
                {
                    // Calculate block duration based on number of hotkeys
                    int blockDuration = CalculateBlockDuration(dueHotkeys.Count);
                    
                    // Block main loop
                    _mainLoopBlocked = true;
                    _mainLoopBlockedUntil = currentTime.AddMilliseconds(blockDuration);
                    
                    Log($"🎯 Found {dueHotkeys.Count} hotkey(s) - blocking main loop for {blockDuration}ms");
                    
                    // Queue hotkeys with proper spacing
                    QueueHotkeysWithSpacing(dueHotkeys, currentTime);
                    
                    _consecutiveHotkeysQueued += dueHotkeys.Count;
                    _lastHotkeyQueueTime = currentTime;
                }
            }
            catch (Exception ex)
            {
                Log($"❌ Error processing hotkeys: {ex.Message}");
            }
        }

        private int CalculateBlockDuration(int hotkeyCount)
        {
            // Base block for 1 hotkey, plus extra for each additional hotkey
            int blockDuration = BASE_BLOCK_DURATION + (Math.Min(hotkeyCount - 1, MAX_CONSECUTIVE_HOTKEYS - 1) * EXTRA_BLOCK_PER_HOTKEY);
            
            // Cap at reasonable maximum (e.g., 2500ms for many hotkeys)
            return Math.Min(blockDuration, 2500);
        }

        private void QueueHotkeysWithSpacing(List<KeyPressAction> hotkeys, DateTime currentTime)
        {
            DateTime nextAvailableSlot = currentTime;
            
            for (int i = 0; i < hotkeys.Count; i++)
            {
                var hotkey = hotkeys[i];
                
                // Calculate when this hotkey can be queued
                if (i > 0) // For hotkeys after the first one
                {
                    // Add minimum gap between hotkeys
                    nextAvailableSlot = nextAvailableSlot.AddMilliseconds(MIN_HOTKEY_GAP + hotkey.Duration);
                    
                    // If we need to wait, simulate the wait in the queue timing
                    DateTime now = DateTime.Now;
                    if (nextAvailableSlot > now)
                    {
                        int waitMs = (int)(nextAvailableSlot - now).TotalMilliseconds;
                        Log($"⏳ Hotkey {hotkey.Id} will wait {waitMs}ms before queuing");
                    }
                }
                
                _keyPressService.QueueKeyPress(hotkey);
                Log($"🔥 Queued hotkey {hotkey.Id}: '{hotkey.Key}' for {hotkey.Duration}ms");
            }
        }

        private void ProcessMainLoop(DateTime currentTime)
        {
            try
            {
                // Skip if main loop not configured
                if (string.IsNullOrWhiteSpace(_config.MainLoop.Key) || _config.MainLoop.Interval <= 0)
                    return;
                
                var timeSinceLastMain = (currentTime - _lastMainLoopTime).TotalMilliseconds;
                
                if (timeSinceLastMain >= _config.MainLoop.Interval)
                {
                    // Check if key press service has queued keys
                    if (_keyPressService.GetQueueSize() > 0)
                    {
                        Log($"⏸️ Main loop waiting - {_keyPressService.GetQueueSize()} key(s) in queue");
                        return;
                    }
                    
                    // Ensure minimum gap from last hotkey
                    var timeSinceLastHotkey = (currentTime - _lastHotkeyQueueTime).TotalMilliseconds;
                    if (timeSinceLastHotkey < 300) // 300ms gap after hotkeys
                    {
                        Log($"⏳ Main loop waiting {300 - (int)timeSinceLastHotkey}ms after hotkeys");
                        return;
                    }
                    
                    _keyPressService.QueueKeyPress(new KeyPressAction
                    {
                        Id = "MAIN",
                        Key = _config.MainLoop.Key,
                        Duration = _config.MainLoop.PressDuration,
                        DueTime = currentTime
                    });
                    
                    _lastMainLoopTime = currentTime;
                    Log($"🔁 Main loop key queued");
                    
                    // Reset consecutive hotkey counter since we processed a main loop key
                    _consecutiveHotkeysQueued = 0;
                }
            }
            catch (Exception ex)
            {
                Log($"❌ Error processing main loop: {ex.Message}");
            }
        }

        public void ReloadHotkeys()
        {
            _hotkeyScheduler.LoadHotkeys(_config);
            Log("✅ Hotkeys reloaded");
        }

        public void FocusGameWindow()
        {
            _windowManager.BringGameToFront();
        }

        private void Log(string message)
        {
            OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
        }
    }
}
