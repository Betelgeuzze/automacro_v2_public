using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace automacro.utils
{
    public static class FlashWindow
    {
        [DllImport("user32.dll")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool FlashWindowEx(ref FLASHWINFO pwfi);

        [StructLayout(LayoutKind.Sequential)]
        private struct FLASHWINFO
        {
            public uint cbSize;
            public IntPtr hwnd;
            public uint dwFlags;
            public uint uCount;
            public uint dwTimeout;
        }

        private const uint FLASHW_ALL = 0x00000003;
        private const uint FLASHW_TIMERNOFG = 0x0000000C;

        public static bool Flash(Form form)
        {
            try
            {
                FLASHWINFO fInfo = new FLASHWINFO();
                fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
                fInfo.hwnd = form.Handle;
                fInfo.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
                fInfo.uCount = 3; // Flash 3 times
                fInfo.dwTimeout = 0; // Use default cursor blink rate

                return FlashWindowEx(ref fInfo);
            }
            catch
            {
                return false;
            }
        }
    }
}