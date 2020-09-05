using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Input.Clipboards
{
    public interface IClipboard
    {
        void SetClipboard(string s);
        string GetClipboard();
    }
}
