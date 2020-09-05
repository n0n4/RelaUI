using SpriteFontPlus;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Text
{
    public static class CharacterRangeExtensions
    {
        public static readonly CharacterRange HangulSyllables =
            new CharacterRange((char)0xAC00, (char)0xD7AF);
    }
}
