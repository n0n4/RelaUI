using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Text
{
    // TODO: should this be a struct?
    public class TextStyles
    {
        public List<TextSettings> Styles;
        public List<int> StyleSwitchIndices;
        public List<int> StyleSwitchStyles;
        public List<RelaFont> Fonts;

        public TextStyles(List<TextSettings> settingsStyles, List<int> styleSwitchIndices, List<int> styleSwitchStyles, List<RelaFont> fonts)
        {
            Styles = settingsStyles;
            StyleSwitchIndices = styleSwitchIndices;
            StyleSwitchStyles = styleSwitchStyles;
            Fonts = fonts;
        }

        public TextStyles Adjust(int offset)
        {
            List<int> newindices = new List<int>();
            List<int> newstyles = new List<int>();

            for (int i = 0; i < StyleSwitchIndices.Count; i++)
            {
                int newindex = StyleSwitchIndices[i] - offset;
                if (newindex >= 0)
                {
                    newindices.Add(newindex);
                    newstyles.Add(StyleSwitchStyles[i]);
                }
                else
                {
                    // special cases
                    // if nothing has been added yet and this is the last one, add it anyways
                    if (i + 1 >= StyleSwitchIndices.Count)
                    {
                        newindices.Add(0);
                        newstyles.Add(StyleSwitchStyles[i]);
                    }
                    else if (StyleSwitchIndices[i + 1] - offset > 0)
                    {
                        // next index is valid, but starts after 0, so include this one until that one kicks in
                        newindices.Add(0);
                        newstyles.Add(StyleSwitchStyles[i]);
                    }
                }
            }

            return new TextStyles(Styles, newindices, newstyles, Fonts);
        }
    }
}
