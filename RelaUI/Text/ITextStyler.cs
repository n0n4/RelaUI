using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Text
{
    public interface ITextStyler
    {
        void PreStyling();
        TextStyles GetTextStyles(RelaFont font, string text, TextSettings baseStyle, int lineNumber);
        TextStyles GetTextStyles(RelaFont font, string text, TextSettings baseStyle, int lineNumber, TextStyles existing)
        {
            return GetTextStyles(font, text, baseStyle, lineNumber);
        }
        void PostStyling();
    }
}
