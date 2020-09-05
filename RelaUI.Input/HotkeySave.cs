using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RelaUI.Input
{
    public class HotkeySave
    {
        public string Name;
        public string Source;
        public List<string> Stroke;

        public HotkeySave(string name, string source, List<string> stroke)
        {
            Name = name;
            Source = source;
            Stroke = stroke;
        }

        /*public HotkeySave(string name, string source, List<List<Keys>> stroke)
        {
            Name = name;
            Source = source;
            Stroke = stroke.Select(x => KeysHelper.CreateStrokeString(x)).ToList();
        }*/

        public List<List<Keys>> GetStroke()
        {
            if (Stroke.Count <= 0)
            {
                return new List<List<Keys>>() { new List<Keys>() };
            }
            return Stroke.Select(x => KeysHelper.ReadStrokeString(x)).ToList();
        }
    }
}