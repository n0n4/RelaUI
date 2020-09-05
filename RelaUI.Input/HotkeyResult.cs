using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Input
{
    public class HotkeyResult
    {
        public string Name;
        public string Source;

        public HotkeyResult(string name, string source)
        {
            Name = name;
            Source = source;
        }
    }
}
