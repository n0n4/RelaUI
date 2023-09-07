using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Text
{
    public interface ITextStyler
    {
        void PreStyling();
        TextStyles GetTextStyles(RelaFont font, string text, TextSettings baseStyle, int lineNumber);
        void PostStyling();
    }
}
