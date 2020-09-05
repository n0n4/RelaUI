using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace RelaUI.Utilities
{
    public static class Bash
    {
        public static string Run(string command, int timeout = 500)
        {
            Process process = new Process()
            {
                StartInfo = new ProcessStartInfo()
                {
                    FileName = "/bin/bash",
                    Arguments = "-c \"" + command + "\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true,
                }
            };

            process.Start();
            if (!process.WaitForExit(timeout))
                throw new Exception("Bash process timed out '" + command + "'");

            return process.StandardOutput.ReadToEnd();
        }
    }
}