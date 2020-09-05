using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Input.Clipboards
{
    public class ClipboardLocal : IClipboard
    {
        private string Text = string.Empty;

        public string GetClipboard()
        {
            return Text;
        }

        public void SetClipboard(string s)
        {
            Text = s;
        }
    }
}