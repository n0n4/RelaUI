using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace RelaUI.Input.Clipboards
{
    public class ClipboardWindows : IClipboard
    {
        private const uint CFUnicodeText = 13;

        public string GetClipboard()
        {
            if (!IsClipboardFormatAvailable(CFUnicodeText))
                return string.Empty;

            IntPtr handle = IntPtr.Zero;
            IntPtr pointer = IntPtr.Zero;

            WaitOpen();

            try
            {
                handle = GetClipboardData(CFUnicodeText);
                if (handle == IntPtr.Zero)
                    return string.Empty;

                pointer = GlobalLock(handle);
                if (pointer == IntPtr.Zero)
                    return string.Empty;

                int len = GlobalSize(handle);
                byte[] buffer = new byte[len];

                Marshal.Copy(pointer, buffer, 0, len);
                return Encoding.Unicode.GetString(buffer).TrimEnd('\0');
            }
            finally
            {
                if (pointer != IntPtr.Zero)
                    GlobalUnlock(handle);
                CloseClipboard();
            }
        }

        public void SetClipboard(string s)
        {
            WaitOpen();

            EmptyClipboard();
            IntPtr handle = IntPtr.Zero;
            try
            {
                int bytes = (s.Length + 1) * 2;
                handle = Marshal.AllocHGlobal(bytes);

                if (handle == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                IntPtr pointer = GlobalLock(handle);
                if (pointer == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());

                try
                {
                    Marshal.Copy(s.ToCharArray(), 0, pointer, s.Length);
                }
                finally
                {
                    GlobalUnlock(pointer);
                }

                if (SetClipboardData(CFUnicodeText, handle) == IntPtr.Zero)
                    throw new Win32Exception(Marshal.GetLastWin32Error());
                handle = IntPtr.Zero;
            }
            finally
            {
                if (handle != IntPtr.Zero)
                    Marshal.FreeHGlobal(handle);
                CloseClipboard();
            }
        }

        private void WaitOpen()
        {
            // try to access the clipboard
            // return if it works
            // sleep and try again if it doesn't
            for (int i = 0; i < 10; i++)
            {
                if (OpenClipboard(IntPtr.Zero))
                    return;
                Thread.Sleep(100);
            }
            // if we don't succeed after a certain number 
            // of tries, throw an exception
            throw new Win32Exception(Marshal.GetLastWin32Error());
        }

        [DllImport("User32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool IsClipboardFormatAvailable(uint format);

        [DllImport("User32.dll", SetLastError = true)]
        private static extern IntPtr GetClipboardData(uint uFormat);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern IntPtr GlobalLock(IntPtr hMem);

        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool GlobalUnlock(IntPtr hMem);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool OpenClipboard(IntPtr hWndNewOwner);

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool CloseClipboard();

        [DllImport("user32.dll", SetLastError = true)]
        private static extern IntPtr SetClipboardData(uint uFormat, IntPtr data);

        [DllImport("user32.dll")]
        private static extern bool EmptyClipboard();

        [DllImport("Kernel32.dll", SetLastError = true)]
        private static extern int GlobalSize(IntPtr hMem);
    }
}
