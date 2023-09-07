using RelaUI.Components;
using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;
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
        public List<UIComponent> InnerComponents;
        public List<int> ComponentIndices; // note: these need to equal one of the style switch indices

        public TextStyles(List<TextSettings> settingsStyles, List<int> styleSwitchIndices, List<int> styleSwitchStyles, List<RelaFont> fonts, List<UIComponent> components = null, List<int> componentIndices = null)
        {
            Styles = settingsStyles;
            StyleSwitchIndices = styleSwitchIndices;
            StyleSwitchStyles = styleSwitchStyles;
            Fonts = fonts;
            InnerComponents = components;
            ComponentIndices = componentIndices;
        }

        // TODO: should really make the consumer functions (e.g. in draw / texthelper, drawmultistyle)
        //       take in an offset rather than cloning everything and making garbage over and over
        public TextStyles Adjust(int offset)
        {
            List<int> newindices = new List<int>();
            List<int> newstyles = new List<int>();
            List<UIComponent> newcomponents = InnerComponents != null ? new List<UIComponent>() : null;
            List<int> newcompIndices = ComponentIndices != null ? new List<int>() : null;

            if (InnerComponents != null && ComponentIndices != null)
            {
                for (int i = 0; i < InnerComponents.Count; i++)
                {
                    int newindex = ComponentIndices[i] - offset;
                    if (newindex >= 0)
                    {
                        newcompIndices.Add(newindex);
                        newcomponents.Add(InnerComponents[i]);
                    }
                }
            }

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
