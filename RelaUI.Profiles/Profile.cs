using RelaUI.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Profiles
{
    public class Profile<T>
    {
        public const string DefaultStyle = "Default.json";

        public string SaveName = "profile";
        public string UserName = "User";
        public string Style = DefaultStyle;

        public T Options;

        public List<HotkeySave> Hotkeys = new List<HotkeySave>();
    }
}
