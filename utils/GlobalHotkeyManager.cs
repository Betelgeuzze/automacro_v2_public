using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using automacro.modules;
using automacro.utils;

namespace automacro.utils
{
    public class GlobalHotkeyManager : IDisposable
    {
        [DllImport("user32.dll")]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll")]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        private readonly IntPtr _handle;
        private readonly SystemCoordinator _coordinator;
        private bool _disposed = false;

        private const int HOTKEY_ID_F9 = 1;
        private const int HOTKEY_ID_BACKSLASH = 2;
        
        private const uint MOD_NONE = 0x0000;
        private const uint VK_F9 = 0x78;
        private const uint VK_BACKSLASH = 0xDC;

        public event Action<string> OnHotkeyTriggered;

        public GlobalHotkeyManager(IntPtr windowHandle, SystemCoordinator coordinator)
        {
            _handle = windowHandle;
            _coordinator = coordinator;
            
            RegisterHotKeys();
        }

        private void RegisterHotKeys()
        {
            try
            {
                bool f9Registered = RegisterHotKey(_handle, HOTKEY_ID_F9, MOD_NONE, VK_F9);
                // Removed debug log

                bool backslashRegistered = RegisterHotKey(_handle, HOTKEY_ID_BACKSLASH, MOD_NONE, VK_BACKSLASH);
                // Removed debug log

                if (f9Registered && backslashRegistered)
                {
                    // Removed debug log
                }
                else
                {
                    // Removed debug log
                }
            }
            catch (Exception ex)
            {
                // Removed debug log
            }
        }

        public void ProcessHotkeyMessage(Message m)
        {
            if (m.Msg == 0x0312)
            {
                int hotkeyId = m.WParam.ToInt32();
                
                switch (hotkeyId)
                {
                    case HOTKEY_ID_F9:
                        HandleF9Hotkey();
                        break;
                    
                    case HOTKEY_ID_BACKSLASH:
                        HandleBackslashHotkey();
                        break;
                }
            }
        }

        private void HandleF9Hotkey()
        {
            try
            {
                bool isRunning = _coordinator.IsMacroRunning;
                
                if (!isRunning)
                {
                    OnHotkeyTriggered?.Invoke("F9: Starting macro...");
                    _coordinator.StartAllSystems();
                    FocusGameWindow();
                }
                else
                {
                    OnHotkeyTriggered?.Invoke("F9: Stopping macro...");
                    _coordinator.StopAllSystems();
                }
            }
            catch (Exception ex)
            {
                OnHotkeyTriggered?.Invoke($"F9 Error: {ex.Message}");
            }
        }

        private void HandleBackslashHotkey()
        {
            try
            {
                if (_coordinator.IsMacroRunning)
                {
                    OnHotkeyTriggered?.Invoke("Emergency Stop (Backslash): Stopping immediately!");
                    _coordinator.StopAllSystems();
                }
            }
            catch (Exception ex)
            {
                OnHotkeyTriggered?.Invoke($"Backslash Error: {ex.Message}");
            }
        }

private void FocusGameWindow()
        {
            try
            {
                var processTarget = _coordinator.GetProcessTarget();
                System.Diagnostics.Process gameProcess = null;

                if (processTarget != null && processTarget.ProcessId != 0)
                {
                    gameProcess = WindowUtils.GetGameProcess(processTarget);
                }
                else
                {
                    string gameExe = _coordinator.GetGameExeName();
                    if (!string.IsNullOrEmpty(gameExe))
                        gameProcess = WindowUtils.GetGameProcess(gameExe);
                }

                if (gameProcess != null && gameProcess.MainWindowHandle != IntPtr.Zero)
                {
                    bool success = WindowUtils.BringWindowToFront(gameProcess.MainWindowHandle);
                    if (success)
                        OnHotkeyTriggered?.Invoke($"Focused game window: {gameProcess.ProcessName}");
                    else
                        OnHotkeyTriggered?.Invoke($"Game process found but could not focus window");
                }
                else
                {
                    OnHotkeyTriggered?.Invoke($"Game process not found or has no visible window");
                }
            }
            catch (Exception ex)
            {
                OnHotkeyTriggered?.Invoke($"Warning: Could not focus game window: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (!_disposed)
            {
                try
                {
                    UnregisterHotKey(_handle, HOTKEY_ID_F9);
                    UnregisterHotKey(_handle, HOTKEY_ID_BACKSLASH);
                    // Removed debug log
                }
                catch (Exception ex)
                {
                    // Removed debug log
                }
                
                _disposed = true;
            }
        }
    }
}
