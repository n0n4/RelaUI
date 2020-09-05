using System;
using System.Runtime.InteropServices;

namespace RelaUI.Utilities
{
    public static class Platform
    {
        private static bool HasDetermined = false;
        private static OS Determined = OS.UNKNOWN;

        public enum OS
        {
            UNKNOWN = 0,
            WINDOWS = 1,
            LINUX = 2,
            OSX = 3,
        }

        private static void Determine()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Determined = OS.WINDOWS;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Determined = OS.LINUX;
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
            {
                Determined = OS.OSX;
            }

            HasDetermined = true;
        }

        public static OS Current()
        {
            if (!HasDetermined)
                Determine();
            return Determined;
        }
    }
}