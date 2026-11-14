using RelaUI.Components;
using System.Collections.Generic;

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

        private string CachedText = null;
        private List<string> Chunks = new List<string>();
        private List<int> ChunkWidths = new List<int>();

        public TextStyles(List<TextSettings> settingsStyles, List<int> styleSwitchIndices, List<int> styleSwitchStyles, List<RelaFont> fonts, List<UIComponent> components = null, List<int> componentIndices = null)
        {
            Styles = settingsStyles;
            StyleSwitchIndices = styleSwitchIndices;
            StyleSwitchStyles = styleSwitchStyles;
            Fonts = fonts;
            InnerComponents = components;
            ComponentIndices = componentIndices;
        }

        public List<string> GetChunks(string text)
        {
            if (CachedText == text)
                return Chunks;

            CachedText = text;
            Chunks.Clear();
            ChunkWidths.Clear();

            for (int styleIndex = 0; styleIndex < StyleSwitchIndices.Count; styleIndex++)
            {
                int pos = StyleSwitchIndices[styleIndex];
                if (pos >= text.Length)
                {
                    Chunks.Add(null);
                    ChunkWidths.Add(0);
                    continue;
                }

                int nextpos = text.Length;
                if (styleIndex + 1 < StyleSwitchIndices.Count)
                {
                    nextpos = StyleSwitchIndices[styleIndex + 1];
                    if (nextpos >= text.Length)
                        nextpos = text.Length;
                }

                if (nextpos <= pos)
                {
                    Chunks.Add(null);
                    ChunkWidths.Add(0);
                    continue;
                }

                string chunk = text.Substring(pos, nextpos - pos);
                Chunks.Add(chunk);
                ChunkWidths.Add(chunk.IndexOf('\t') == -1 ? -1 : -2);
            }

            return Chunks;
        }

        public int GetChunkWidth(int index, RelaFont font, TextSettings settings, int totalWidth)
        {
            if (index < 0 || index >= Chunks.Count || Chunks[index] == null)
                return 0;

            int cached = ChunkWidths[index];
            if (cached >= 0)
                return cached;

            int width = TextHelper.GetWidthUnsafe(font, Chunks[index], settings, totalWidth);
            if (cached == -1)
                ChunkWidths[index] = width;
            return width;
        }

        public void InvalidateChunks()
        {
            CachedText = null;
            Chunks.Clear();
            ChunkWidths.Clear();
        }

        public TextStyles Adjust(int offset)
        {
            return Adjust(offset, null);
        }

        public TextStyles Adjust(int offset, TextStyles existing)
        {
            if (ReferenceEquals(existing, this))
                existing = null;

            TextStyles result = existing != null
                ? existing
                : new TextStyles(Styles, new List<int>(), new List<int>(), Fonts);

            result.Styles = Styles;
            result.Fonts = Fonts;
            result.InvalidateChunks();

            List<int> newindices = result.StyleSwitchIndices;
            List<int> newstyles = result.StyleSwitchStyles;
            newindices.Clear();
            newstyles.Clear();

            List<UIComponent> newcomponents = null;
            List<int> newcompIndices = null;
            if (InnerComponents != null && ComponentIndices != null)
            {
                if (result.InnerComponents != null && result.ComponentIndices != null)
                {
                    newcomponents = result.InnerComponents;
                    newcompIndices = result.ComponentIndices;
                    newcomponents.Clear();
                    newcompIndices.Clear();
                }
                else
                {
                    newcomponents = new List<UIComponent>();
                    newcompIndices = new List<int>();
                }

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

            result.InnerComponents = newcomponents;
            result.ComponentIndices = newcompIndices;
            return result;
        }
    }
}
