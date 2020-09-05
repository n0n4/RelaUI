using RelaUI.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Input.Clipboards
{
    public static class Clipboard
    {
        public static IClipboard Driver = DetermineClipboard();

        private static IClipboard DetermineClipboard()
        {
            Platform.OS os = Platform.Current();

            if (os == Platform.OS.WINDOWS)
            {
                return new ClipboardWindows();
            }
            else if (os == Platform.OS.LINUX)
            {
                return new ClipboardLinux();
            }
            else if (os == Platform.OS.OSX)
            {
                return new ClipboardOsx();
            }
            else if (os == Platform.OS.UNKNOWN)
            {
                return new ClipboardLocal();
            }

            throw new Exception("Unrecognized OS for Clipboard determination");
        }

        public static string Get()
        {
            return Driver.GetClipboard();
        }

        public static void Set(string s)
        {
            Driver.SetClipboard(s);
        }
    }
}
