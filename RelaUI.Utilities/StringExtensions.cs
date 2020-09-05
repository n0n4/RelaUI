using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelaUI.Utilities
{
    public static class StringExtensions
    {
        public static List<string> GetWords(this string s, bool ignoreQuoteBlocks = false)
        {
            List<string> outs = new List<string>();

            string curword = "";
            bool inquote = false;
            int i = 0;
            while (i < s.Length)
            {
                char c = s[i];
                if (c == '"' && !ignoreQuoteBlocks)
                {
                    inquote = !inquote;
                }
                else if (c == ' ' && !inquote)
                {
                    outs.Add(curword);
                    curword = "";
                }
                else
                {
                    curword += c;
                }
                i++;
            }
            outs.Add(curword);

            return outs;
        }

        public static List<string> SplitByLines(this string s)
        {
            List<string> outs = new List<string>();
            char c;
            int lastStart = 0;
            for (int i = 0; i < s.Length; i++)
            {
                c = s[i];
                if (c == '\r' || c == '\n')
                {
                    int len = i - lastStart;
                    if (len > 0)
                        outs.Add(s.Substring(lastStart, len));
                    else
                        outs.Add("");
                    
                    if (c == '\r' && i + 1 < s.Length && s[i + 1] == '\n')
                    {
                        i++;
                    }
                    lastStart = i + 1;
                    if (lastStart == s.Length)
                        outs.Add("");
                }
            }
            
            int finallen = s.Length - lastStart;
            if (finallen > 0)
                outs.Add(s.Substring(lastStart, finallen));

            return outs;
            //return s.Replace("\r\n", "\n").Replace('\r', '\n').Split('\n').ToList();
        }

        public static string CutOut(this string s, int startpos, int length)
        {
            if (length == 0)
                return s;

            if (startpos == 0)
            {
                if (length == s.Length)
                    return string.Empty;
                return s.Substring(length);
            }

            if (startpos + length >= s.Length)
                return s.Substring(0, startpos);

            return s.Substring(0, startpos) + s.Substring(startpos + length);
        }
    }
}
