using System;
using System.Runtime.InteropServices;
using automacro.config;
using automacro.utils;
using automacro.models;

namespace automacro.modules
{
    public class GameWindowManager
    {
        private readonly string _gameExe;
        private IntPtr _gameWindowHandle = IntPtr.Zero;
        private uint _gameProcessId = 0;
        
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);
        
        public event Action<string> OnLog;
        
public GameWindowManager(AppConfig config)
        {
            _gameExe = config.TargetGame.Exe;
            RefreshGameWindow();
            Log("🖥️ GameWindowManager initialized");
        }

        public GameWindowManager(AppConfig config, Automacro.Models.ProcessTarget target)
        {
            _gameExe = config.TargetGame.Exe;
            if (target != null && target.IsValid())
            {
                _gameWindowHandle = target.WindowHandle;
                _gameProcessId = (uint)target.ProcessId;
                Log($"🖥️ GameWindowManager initialized with selected process: Handle={_gameWindowHandle}, PID={_gameProcessId}");
            }
            else
            {
                _gameWindowHandle = IntPtr.Zero;
                _gameProcessId = 0;
                Log("⚠️ No valid process target provided to GameWindowManager");
            }
        }
        
        public bool IsGameFocused()
        {
            try
            {
                if (_gameWindowHandle == IntPtr.Zero || _gameProcessId == 0) 
                {
                    RefreshGameWindow();
                    return false;
                }

                IntPtr foregroundWindow = GetForegroundWindow();
                if (foregroundWindow == IntPtr.Zero) return false;

                GetWindowThreadProcessId(foregroundWindow, out uint foregroundProcessId);
                return foregroundProcessId == _gameProcessId;
            }
            catch
            {
                return false;
            }
        }
        
        public void RefreshGameWindow()
        {
            // No-op if using ProcessTarget; do not update _gameWindowHandle or _gameProcessId
        }
        
        [DllImport("user32.dll")]
        private static extern bool IsIconic(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;

        public bool BringGameToFront()
        {
            if (_gameWindowHandle == IntPtr.Zero)
            {
                RefreshGameWindow();
                return false;
            }

            return WindowUtils.BringWindowToFront(_gameWindowHandle);
        }
        
        private void Log(string message)
        {
            OnLog?.Invoke($"[{DateTime.Now:HH:mm:ss}] {message}");
        }
    }
}
