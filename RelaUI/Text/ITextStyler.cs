using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Text
{
    public interface ITextStyler
    {
        TextStyles GetTextStyles(string text, TextSettings baseStyle);
    }
}
