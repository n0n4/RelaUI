using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelaUI.Text
{
    public static class CharacterHelper
    {
        // ref: https://stackoverflow.com/questions/75145712/how-can-i-tell-a-string-starts-with-an-emoji-and-get-the-first-emoji-in-the-stri
        private static bool IsEmoticon(int charCode) =>
            0x1F600 <= charCode && charCode <= 0x1F64F;
        private static bool IsMiscPictograph(int charCode) =>
            0x1F680 <= charCode && charCode <= 0x1F5FF;
        private static bool IsTransport(int charCode) =>
            0x2600 <= charCode && charCode <= 0x1F6FF;
        private static bool IsMiscSymbol(int charCode) =>
            0x2700 <= charCode && charCode <= 0x26FF;
        private static bool IsDingbat(int charCode) =>
            0x2700 <= charCode && charCode <= 0x27BF;
        private static bool IsVariationSelector(int charCode) =>
            0xFE00 <= charCode && charCode <= 0xFE0F;
        private static bool IsSupplemental(int charCode) =>
            0x1F900 <= charCode && charCode <= 0x1F9FF;
        private static bool IsFlag(int charCode) =>
            0x1F1E6 <= charCode && charCode <= 0x1F1FF;

        public static bool IsEmoji(char hs, char ls)
        {
            if (!char.IsHighSurrogate(hs) || !char.IsLowSurrogate(ls))
                return false;
            var charCode = Char.ConvertToUtf32(hs, ls); 
            return IsEmoticon(charCode)
                || IsMiscPictograph(charCode)
                || IsTransport(charCode)
                || IsMiscSymbol(charCode)
                || IsDingbat(charCode)
                || IsVariationSelector(charCode)
                || IsSupplemental(charCode)
                || IsFlag(charCode);
        }
    }
}
