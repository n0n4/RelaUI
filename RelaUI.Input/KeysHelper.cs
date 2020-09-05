using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Input
{
    public static class KeysHelper
    {
        public static string CreateStrokeString(List<Keys> keys)
        {
            if (keys.Count == 0)
                return "";
            StringBuilder sb = new StringBuilder();
            sb.Append(keys[0].ToString());
            for (int i = 1; i < keys.Count; i++)
            {
                sb.Append("-").Append(keys[i].ToString());
            }
            return sb.ToString();
        }

        public static string CreateStrokeString(Keys[] keys, int count)
        {
            if (count == 0)
                return "";
            StringBuilder sb = new StringBuilder();
            sb.Append(keys[0].ToString());
            for (int i = 1; i < count; i++)
            {
                sb.Append("-").Append(keys[i].ToString());
            }
            return sb.ToString();
        }

        public static List<Keys> ReadStrokeString(string s)
        {
            List<Keys> outs = new List<Keys>();
            while (!string.IsNullOrWhiteSpace(s))
            {
                int dash = s.IndexOf('-');
                string read = "";
                if (dash == -1)
                {
                    read = s;
                    s = string.Empty;
                }
                else
                {
                    read = s.Substring(0, dash);
                    s = s.Substring(dash + 1);
                }

                Keys k;
                if (!Enum.TryParse<Keys>(read, out k))
                {
                    throw new Exception("Unrecognized Key '" + read + "'.");
                }
                outs.Add(k);
            }
            return outs;
        }
    }
}