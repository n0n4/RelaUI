using RelaUI.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Profiles
{
    public class Profile<T>
    {
        public string SaveName = "profile";
        public string UserName = "User";

        public T Options;

        public List<HotkeySave> Hotkeys = new List<HotkeySave>();
    }
}
