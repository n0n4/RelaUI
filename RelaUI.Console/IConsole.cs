using System;

namespace RelaUI.Console
{
    public interface IConsole
    {
        void Log(string s);
        void Error(string s);
        void Save(string path);
        void Input(string input);

        int LogCount();
        string GetLog(int index);
    }
}
