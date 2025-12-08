using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Runtime.InteropServices;

namespace automacro.modules
{
    public class KeyPressService : IDisposable
    {
        private readonly ConcurrentQueue<KeyPressAction> _keyPressQueue = new ConcurrentQueue<KeyPressAction>();
        private Thread _keyPressThread;
        private bool _isRunning;
        private CancellationTokenSource _cancellationToken;
        private Random _random = new Random();
        
        private DateTime _lastKeyPress = DateTime.MinValue;
        
        // MapleStory timing (based on testing)
        private const int MIN_KEY_GAP_MS = 200;  // Minimum gap between key presses
        private const int PRESS_DURATION_VARIATION = 10; // Variation in press duration
        
        // Windows API - using the simple method that was working
        [DllImport("user32.dll")]
        private static extern uint SendInput(uint nInputs, [MarshalAs(UnmanagedType.LPArray), In] INPUT[] pInputs, int cbSize);

        private const uint KEYEVENTF_KEYUP = 0x0002;

        // Struct definitions
        [StructLayout(LayoutKind.Sequential)]
        private struct INPUT
        {
            public uint type;
            public InputUnion u;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct InputUnion
        {
            [FieldOffset(0)]
            public MOUSEINPUT mi;
            [FieldOffset(0)]
            public KEYBDINPUT ki;
            [FieldOffset(0)]
            public HARDWAREINPUT hi;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct MOUSEINPUT
        {
            public int dx;
            public int dy;
            public uint mouseData;
            public uint dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct KEYBDINPUT
        {
            public ushort wVk;
            public ushort wScan;
            public uint dwFlags;
            public uint time;
            public UIntPtr dwExtraInfo;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct HARDWAREINPUT
        {
            public uint uMsg;
            public ushort wParamL;
            public ushort wParamH;
        }

        public event Action<string> OnLog;

        public KeyPressService()
        {
            Log("⌨️ KeyPressService initialized (simple method)");
        }

        public void Start()
        {
            if (_isRunning) return;
            
            _isRunning = true;
            _cancellationToken = new CancellationTokenSource();
            
            _keyPressQueue.Clear();
            _lastKeyPress = DateTime.MinValue;
            
            _keyPressThread = new Thread(KeyPressLoop) 
            { 
                Name = "KeyPressLoop", 
                IsBackground = true
            };
            
            _keyPressThread.Start();
            Log("⌨️ KeyPressService started");
        }

        public void Stop()
        {
            if (!_isRunning) return;
            
            _isRunning = false;
            _cancellationToken?.Cancel();
            
            _keyPressQueue.Clear();
            Thread.Sleep(50);
            
            Log("⌨️ KeyPressService stopped");
        }

        public IntPtr TargetWindowHandle { get; set; } = IntPtr.Zero;

        public void QueueKeyPress(KeyPressAction action)
        {
            if (!_isRunning) return;
            action.PressType = KeyPressType.Press;
            _keyPressQueue.Enqueue(action);
            Log($"📥 Queued: {action.Id}='{action.Key}' for {action.Duration}ms");
        }

        public void QueueKeyDown(string key)
        {
            if (!_isRunning) return;
            _keyPressQueue.Enqueue(new KeyPressAction
            {
                Id = "TELEGRAM",
                Key = key,
                Duration = 0,
                DueTime = DateTime.Now,
                PressType = KeyPressType.Down
            });
            Log($"⬇️ KeyDown: {key}");
        }

        public void QueueKeyUp(string key)
        {
            if (!_isRunning) return;
            _keyPressQueue.Enqueue(new KeyPressAction
            {
                Id = "TELEGRAM",
                Key = key,
                Duration = 0,
                DueTime = DateTime.Now,
                PressType = KeyPressType.Up
            });
            Log($"⬆️ KeyUp: {key}");
        }

        public void ClearQueue()
        {
            while (_keyPressQueue.TryDequeue(out _)) { }
            Log("⚠️ Cleared key press queue");
        }

        public int GetQueueSize()
        {
            return _keyPressQueue.Count;
        }

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        private void KeyPressLoop()
        {
            try
            {
                while (_isRunning && !_cancellationToken.Token.IsCancellationRequested && !automacro.AppState.IsShuttingDown)
                {
                    if (!_isRunning || _cancellationToken.Token.IsCancellationRequested)
                    {
                        break;
                    }
                    if (!_keyPressQueue.TryDequeue(out var action))
                    {
                        Thread.Sleep(10);
                        continue;
                    }

                    // Double-check running state before sending key
                    if (!_isRunning || _cancellationToken.Token.IsCancellationRequested)
                        break;

                    // Enforce minimum gap between key presses
                    var currentTime = DateTime.Now;
                    var timeSinceLastKey = (currentTime - _lastKeyPress).TotalMilliseconds;

                    if (timeSinceLastKey < MIN_KEY_GAP_MS)
                    {
                        // Wait for required gap
                        int waitTime = MIN_KEY_GAP_MS - (int)timeSinceLastKey;
                        Thread.Sleep(waitTime);
                    }

                    // Double-check again before sending key
                    if (!_isRunning || _cancellationToken.Token.IsCancellationRequested)
                        break;

                    // Execute key press
                    bool success = true;
                    if (action.PressType == KeyPressType.Press)
                    {
                        Log($"⌨️ Pressing {action.Id}: '{action.Key}'");
                        success = PressKeySimple(action.Key, action.Duration);
                    }
                    else if (action.PressType == KeyPressType.Down)
                    {
                        Log($"⬇️ KeyDown: {action.Key}");
                        PressKeyDown(action.Key);
                    }
                    else if (action.PressType == KeyPressType.Up)
                    {
                        Log($"⬆️ KeyUp: {action.Key}");
                        PressKeyUp(action.Key);
                    }

                    if (success)
                    {
                        _lastKeyPress = DateTime.Now;
                        Log($"✅ {action.Id} pressed");
                    }
                    else
                    {
                        Log($"⚠️ {action.Id} failed");
                    }
                }
            }
            catch (Exception ex)
            {
                Log($"❌ Key press loop error: {ex.Message}");
            }
        }

        private bool PressKeySimple(string key, int duration)
        {
            if (string.IsNullOrWhiteSpace(key)) return false;
            
            try
            {
                // Simple reliable method that was working before
                PressKeySendInput(key);
                
                // Hold for duration with slight variation
                int variedDuration = duration + _random.Next(-PRESS_DURATION_VARIATION, PRESS_DURATION_VARIATION);
                if (variedDuration > 0)
                {
                    Thread.Sleep(variedDuration);
                }
                
                return true;
            }
            catch (Exception ex)
            {
                Log($"❌ Key press error ({key}): {ex.Message}");
                return false;
            }
        }

        private void PressKeySendInput(string key)
        {
            ushort scanCode = GetScanCode(key);
            ushort virtualKey = GetVirtualKey(key);

            INPUT[] inputs = new INPUT[2];
            
            // Key down
            inputs[0] = CreateKeyboardInput(virtualKey, scanCode, 0);
            // Key up  
            inputs[1] = CreateKeyboardInput(virtualKey, scanCode, KEYEVENTF_KEYUP);
            
            SendInput(2, inputs, Marshal.SizeOf(typeof(INPUT)));
        }

        private void PressKeyDown(string key)
        {
            ushort scanCode = GetScanCode(key);
            ushort virtualKey = GetVirtualKey(key);

            INPUT input = CreateKeyboardInput(virtualKey, scanCode, 0);
            SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        private void PressKeyUp(string key)
        {
            ushort scanCode = GetScanCode(key);
            ushort virtualKey = GetVirtualKey(key);

            INPUT input = CreateKeyboardInput(virtualKey, scanCode, KEYEVENTF_KEYUP);
            SendInput(1, new INPUT[] { input }, Marshal.SizeOf(typeof(INPUT)));
        }

        private INPUT CreateKeyboardInput(ushort virtualKey, ushort scanCode, uint flags)
        {
            return new INPUT
            {
                type = 1,
                u = new InputUnion
                {
                    ki = new KEYBDINPUT
                    {
                        wVk = virtualKey,
                        wScan = scanCode,
                        dwFlags = flags,
                        time = 0,
                        dwExtraInfo = UIntPtr.Zero
                    }
                }
            };
        }

        private ushort GetScanCode(string key)
        {
            var keyLower = key.ToLower();
            
            var scanCodes = new Dictionary<string, ushort>
            {
                // Letters
                {"a", 0x1E}, {"b", 0x30}, {"c", 0x2E}, {"d", 0x20}, {"e", 0x12},
                {"f", 0x21}, {"g", 0x22}, {"h", 0x23}, {"i", 0x17}, {"j", 0x24},
                {"k", 0x25}, {"l", 0x26}, {"m", 0x32}, {"n", 0x31}, {"o", 0x18},
                {"p", 0x19}, {"q", 0x10}, {"r", 0x13}, {"s", 0x1F}, {"t", 0x14},
                {"u", 0x16}, {"v", 0x2F}, {"w", 0x11}, {"x", 0x2D}, {"y", 0x15}, {"z", 0x2C},
                
                // Numbers
                {"0", 0x0B}, {"1", 0x02}, {"2", 0x03}, {"3", 0x04}, {"4", 0x05},
                {"5", 0x06}, {"6", 0x07}, {"7", 0x08}, {"8", 0x09}, {"9", 0x0A},

                // Special keys
                {"esc", 0x01}, {"escape", 0x01},
                {"enter", 0x1C}, {"return", 0x1C},
                {"tab", 0x0F},
                {"space", 0x39},
                {"backspace", 0x0E},
                {"delete", 0x53},
                {"up", 0x48},
                {"down", 0x50},
                {"left", 0x4B},
                {"right", 0x4D},
                {"shift", 0x2A}
            };
            
            return scanCodes.ContainsKey(keyLower) ? scanCodes[keyLower] : (ushort)0x1F; // Default to 's'
        }

        private ushort GetVirtualKey(string key)
        {
            var keyLower = key.ToLower();
            
            var virtualKeys = new Dictionary<string, ushort>
            {
                // Letters
                {"a", 0x41}, {"b", 0x42}, {"c", 0x43}, {"d", 0x44}, {"e", 0x45},
                {"f", 0x46}, {"g", 0x47}, {"h", 0x48}, {"i", 0x49}, {"j", 0x4A},
                {"k", 0x4B}, {"l", 0x4C}, {"m", 0x4D}, {"n", 0x4E}, {"o", 0x4F},
                {"p", 0x50}, {"q", 0x51}, {"r", 0x52}, {"s", 0x53}, {"t", 0x54},
                {"u", 0x55}, {"v", 0x56}, {"w", 0x57}, {"x", 0x58}, {"y", 0x59}, {"z", 0x5A},
                
                // Numbers
                {"0", 0x30}, {"1", 0x31}, {"2", 0x32}, {"3", 0x33}, {"4", 0x34},
                {"5", 0x35}, {"6", 0x36}, {"7", 0x37}, {"8", 0x38}, {"9", 0x39},

                // Special keys
                {"esc", 0x1B}, {"escape", 0x1B},
                {"enter", 0x0D}, {"return", 0x0D},
                {"tab", 0x09},
                {"space", 0x20},
                {"backspace", 0x08},
                {"delete", 0x2E},
                {"up", 0x26},
                {"down", 0x28},
                {"left", 0x25},
                {"right", 0x27},
                {"shift", 0x10}
            };
            
            return virtualKeys.ContainsKey(keyLower) ? virtualKeys[keyLower] : (ushort)0x53; // Default to 's'
        }

        public int GetEstimatedQueueProcessingTime()
        {
            // Estimate time to process all queued keys
            // Each key takes at least MIN_KEY_GAP_MS
            return _keyPressQueue.Count * MIN_KEY_GAP_MS;
        }

        private void Log(string message)
        {
            OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss.fff}] {message}");
        }

        public void Dispose()
        {
            Stop();
        }
    }

    public enum KeyPressType
    {
        Press,
        Down,
        Up
    }

    public class KeyPressAction
    {
        public string Id { get; set; } = "";
        public string Key { get; set; } = "";
        public int Duration { get; set; } = 100;
        public DateTime DueTime { get; set; }
        public KeyPressType PressType { get; set; } = KeyPressType.Press;
    }
}
