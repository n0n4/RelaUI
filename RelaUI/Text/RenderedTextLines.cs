using RelaUI.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelaUI.Text
{
    public class RenderedTextLines
    {
        private List<RenderedText> List = new List<RenderedText>();

        public RenderedText GetLine(int index)
        {
            return List[index];
        }
        public int Count()
        {
            return List.Count;
        }
        public void SetAll(string text)
        {
            List<string> lines = text.SplitByLines();
            for (int i = 0; i < lines.Count; i++)
            {
                if (i >= List.Count)
                    List.Add(new RenderedText());
                List[i].Text = lines[i];
            }

            if (List.Count > lines.Count)
            {
                List.RemoveRange(lines.Count, List.Count - lines.Count);
            }
        }

        public void Insert(int index, string text)
        {
            RenderedText rendered = new RenderedText();
            rendered.Text = text;
            List.Insert(index, rendered);
        }

        public void Set(int index, string text)
        {
            List[index].Text = text;
        }

        public void Add(string text)
        {
            RenderedText rendered = new RenderedText();
            rendered.Text = text;
            List.Add(rendered);
        }

        public void RemoveAt(int index)
        {
            List.RemoveAt(index);
        }
    }
}
