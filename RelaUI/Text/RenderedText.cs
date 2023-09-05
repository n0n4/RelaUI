using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelaUI.Text
{
    public class RenderedText
    {
        private string TextField;
        public string Text 
        { 
            get { return TextField; } 
            set 
            { 
                if (TextField != value)
                {
                    TextField = value; 
                    Rendered = null;
                }
            } 
        }
        private string Rendered;
        private RelaFont RenderedFont;

        public string Render(RelaFont font)
        {
            if (Rendered != null && RenderedFont == font)
            {
                return Rendered;
            }
            RenderedFont = font;

            Rendered = font.SanitizeString(Text);

            return Rendered;
        }
    }
}
