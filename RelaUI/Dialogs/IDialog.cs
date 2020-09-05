using RelaUI.Components;
using RelaUI.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Dialogs
{
    public interface IDialog
    {
        void Embed(UIComponent component);
        void EmbedAuto(UIComponent component);
        void Popup(UISystem system);
        void Close(float elapsedms, InputManager input);
    }
}
