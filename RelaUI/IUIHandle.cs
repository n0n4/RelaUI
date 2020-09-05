using RelaUI.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI
{
    public interface IUIHandle
    {
        UISystem GetUISystem();
        string GetName();
        void Update(float elapsedms, InputManager input);
        void RegisterHotkeys(HotkeyManager hotkeyManager);
    }
}
