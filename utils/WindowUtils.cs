using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace automacro.utils
{
    public static class WindowUtils
    {
        // Windows API imports
        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();

        [DllImport("user32.dll")]
        private static extern int GetWindowRect(IntPtr hwnd, ref Rect rectangle);

        [DllImport("user32.dll")]
        private static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint processId);

        [DllImport("user32.dll")]
        private static extern int GetWindowText(IntPtr hWnd, StringBuilder text, int count);

        [DllImport("user32.dll")]
        private static extern bool IsWindowVisible(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

        private const int SW_RESTORE = 9;
        private const int SW_SHOW = 5;

        [StructLayout(LayoutKind.Sequential)]
        public struct Rect
        {
            public int Left;
            public int Top;
            public int Right;
            public int Bottom;
        }

        public static bool BringWindowToFront(IntPtr hwnd)
        {
            try
            {
                // First, try to restore if minimized
                ShowWindow(hwnd, SW_RESTORE);
                
                // Then set as foreground
                return SetForegroundWindow(hwnd);
            }
            catch
            {
                return false;
            }
        }

public static Process GetGameProcess(string processName)
        {
            try
            {
                var processes = Process.GetProcessesByName(processName.Replace(".exe", ""));
                
                if (processes.Length == 0) return null;
                
                // If multiple processes, try to find the main game window
                foreach (var process in processes)
                {
                    try
                    {
                        if (process.MainWindowHandle != IntPtr.Zero && IsWindowVisible(process.MainWindowHandle))
                        {
                            var windowTitle = GetWindowTitle(process.MainWindowHandle);
                            if (!string.IsNullOrEmpty(windowTitle) && 
                                !windowTitle.ToLower().Contains("chat") && 
                                !windowTitle.ToLower().Contains("external"))
                            {
                                return process;
                            }
                        }
                    }
                    catch
                    {
                        continue;
                    }
                }
                
                return processes[0];
            }
            catch
            {
                return null;
            }
        }

        public static Process GetGameProcess(Automacro.Models.ProcessTarget target)
        {
            try
            {
                if (target == null || target.ProcessId == 0)
                    return null;
                return Process.GetProcesses().FirstOrDefault(p => p.Id == target.ProcessId);
            }
            catch
            {
                return null;
            }
        }

        private static string GetWindowTitle(IntPtr hwnd)
        {
            try
            {
                StringBuilder buffer = new StringBuilder(256);
                GetWindowText(hwnd, buffer, buffer.Capacity);
                return buffer.ToString();
            }
            catch
            {
                return string.Empty;
            }
        }

        public static Rectangle GetWindowRect(IntPtr hwnd)
        {
            try
            {
                Rect rect = new Rect();
                GetWindowRect(hwnd, ref rect);
                return new Rectangle(rect.Left, rect.Top, rect.Right - rect.Left, rect.Bottom - rect.Top);
            }
            catch
            {
                return Rectangle.Empty;
            }
        }

        public static bool IsProcessRunning(string processName)
        {
            return GetGameProcess(processName) != null;
        }

        public static Bitmap CaptureWindow(IntPtr hwnd)
        {
            try
            {
                var rect = GetWindowRect(hwnd);
                if (rect.Width <= 0 || rect.Height <= 0) return null;

                var bitmap = new Bitmap(rect.Width, rect.Height);

                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.CopyFromScreen(rect.Left, rect.Top, 0, 0, bitmap.Size);
                }

                return bitmap;
            }
            catch
            {
                return null;
            }
        }

public static Bitmap CaptureRegion(IntPtr hwnd, Rectangle region, automacro.gui.Controls.ConsolePanel consolePanel = null)
{
    try
    {
var windowRect = GetWindowRect(hwnd);
if (windowRect.Width <= 0 || windowRect.Height <= 0)
{
        if (consolePanel != null)
            consolePanel.AppendToLog("❌ WindowUtils.CaptureRegion: Invalid window rect (returning null)");
        return null;
}

if (consolePanel != null)
{
    consolePanel.AppendToLog($"WindowUtils.CaptureRegion: windowRect=({windowRect.Left},{windowRect.Top},{windowRect.Width},{windowRect.Height}), region=({region.Left},{region.Top},{region.Width},{region.Height})");
}

var absoluteRegion = new Rectangle(
    windowRect.Left + region.Left,
    windowRect.Top + region.Top,
    region.Width,
    region.Height
);

if (consolePanel != null)
{
    consolePanel.AppendToLog($"WindowUtils.CaptureRegion: absoluteRegion=({absoluteRegion.Left},{absoluteRegion.Top},{absoluteRegion.Width},{absoluteRegion.Height})");
}

var bitmap = new Bitmap(region.Width, region.Height);

using (var graphics = Graphics.FromImage(bitmap))
{
    graphics.CopyFromScreen(absoluteRegion.Left, absoluteRegion.Top, 0, 0, bitmap.Size);
}

        return bitmap;
    }
    catch (Exception ex)
    {
        if (consolePanel != null)
            consolePanel.AppendToLog($"❌ WindowUtils.CaptureRegion error: {ex.Message} (returning null)");
        return null;
    }
}

        public static bool IsWindowFocused(IntPtr hwnd)
        {
            try
            {
                IntPtr foregroundWindow = GetForegroundWindow();
                return foregroundWindow == hwnd;
            }
            catch
            {
                return false;
            }
        }

        [DllImport("user32.dll")]
        private static extern bool PostMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        private const uint WM_LBUTTONDOWN = 0x0201;
        private const uint WM_LBUTTONUP = 0x0202;

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint dwFlags, uint dx, uint dy, uint dwData, UIntPtr dwExtraInfo);

        private const uint MOUSEEVENTF_LEFTDOWN = 0x0002;
        private const uint MOUSEEVENTF_LEFTUP = 0x0004;
        private const uint MOUSEEVENTF_MOVE = 0x0001;
        private const uint MOUSEEVENTF_ABSOLUTE = 0x8000;

        public static void SendMouseClick(IntPtr hWnd, int x, int y)
        {
            // Convert client coordinates to screen coordinates
            var windowRect = GetWindowRect(hWnd);
            int screenX = windowRect.Left + x;
            int screenY = windowRect.Top + y;

            // Move mouse to target position
            Cursor.Position = new Point(screenX, screenY);

            mouse_event(MOUSEEVENTF_LEFTDOWN, 0, 0, 0, UIntPtr.Zero);
            System.Threading.Thread.Sleep(80);
            mouse_event(MOUSEEVENTF_LEFTUP, 0, 0, 0, UIntPtr.Zero);
        }
    }
}
