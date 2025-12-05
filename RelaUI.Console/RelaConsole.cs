using RelaUI.Utilities;
using System;
using System.Collections.Generic;

namespace RelaUI.Console
{
    public class RelaConsole : IConsole
    {
        public List<string> Logs = new List<string>();
        public Action<string> InputAction = null;

        public void Error(string s)
        {
            Logs.Add("!! " + s);
        }

        public string GetLog(int index)
        {
            return Logs[index];
        }

        public void Input(string input)
        {
            Logs.Add("<  " + input);
            if (InputAction != null)
                InputAction(input);
        }

        public void Log(string s)
        {
            Logs.Add("   " + s);
        }

        public int LogCount()
        {
            return Logs.Count;
        }

        public void Save(string path)
        {
            FileUtility.WriteLinesToFile(path, Logs);
            Logs.Add("Wrote save to " + path);
        }
    }
}
