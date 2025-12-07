// models/ProcessTarget.cs
using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Automacro.Models
{
[Serializable]
public class ProcessTarget
{
    public int ProcessId { get; set; }
    public IntPtr WindowHandle { get; set; }
    public string WindowTitle { get; set; }
    public string ProcessName { get; set; }

    public ProcessTarget() { }

    public ProcessTarget(int processId, IntPtr windowHandle, string windowTitle, string processName)
    {
        ProcessId = processId;
        WindowHandle = windowHandle;
        WindowTitle = windowTitle;
        ProcessName = processName;
    }

    public bool IsValid()
    {
        return WindowHandle != IntPtr.Zero && NativeMethods.IsWindow(WindowHandle);
    }

    public override string ToString()
    {
        return $"{ProcessName} (PID: {ProcessId}) - \"{WindowTitle}\"";
    }

    private static class NativeMethods
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        public static extern bool IsWindow(IntPtr hWnd);
    }
}
}
