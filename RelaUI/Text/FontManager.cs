using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using SpriteFontPlus;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RelaUI.Text
{
    public class FontManager
    {
        public string DefaultFont = string.Empty;
        public string DefaultDynamicFont = string.Empty;
        public int DefaultSize = 8;
        public Dictionary<string, Dictionary<int, SpriteFont>> Fonts = new Dictionary<string, Dictionary<int, SpriteFont>>();
        public Dictionary<string, DynamicSpriteFont> DynamicFonts = new Dictionary<string, DynamicSpriteFont>();
        public Dictionary<string, Dictionary<int, RelaFont>> RelaFonts = new Dictionary<string, Dictionary<int, RelaFont>>();

        public List<string> GlobalFontNames = new List<string>();
        public void Load(ContentManager content)
        {
            string fontfolder = "BMFonts";
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(content.RootDirectory, fontfolder));
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("BMFonts content folder not found!");
            }

            Fonts = new Dictionary<string, Dictionary<int, SpriteFont>>();

            // find each directory in this folder
            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo d in dirs)
            {
                string dname = d.Name;
                Fonts.Add(dname, new Dictionary<int, SpriteFont>());
                FileInfo[] dfiles = d.GetFiles("*.*");
                foreach (FileInfo file in dfiles)
                {
                    string fname = Path.GetFileNameWithoutExtension(file.Name);
                    string spt = fname.Substring(fname.LastIndexOf('_') + 1);
                    int pt = Int32.Parse(spt);

                    Fonts[dname][pt] = content.Load<SpriteFont>(Path.Combine(fontfolder, dname, fname));

                    if (DefaultFont == string.Empty)
                    {
                        DefaultFont = dname; // only check in here so empty font folders don't count
                    }
                }
            }

            // now load the dynamic fonts
            string dynamicfontfolder = "DFonts";
            dir = new DirectoryInfo(Path.Combine(content.RootDirectory, dynamicfontfolder));
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("DFonts content folder not found!");
            }

            DynamicFonts = new Dictionary<string, DynamicSpriteFont>();

            // find each directory in this folder
            FileInfo[] files = dir.GetFiles("*.ttf");
            foreach (FileInfo file in files)
            {
                string fname = Path.GetFileNameWithoutExtension(file.Name);

                using (var stream = File.OpenRead(file.FullName))
                {
                    using (var memoryStream = new MemoryStream())
                    {
                        stream.CopyTo(memoryStream);
                        DynamicFonts.Add(fname, DynamicSpriteFont.FromTtf(memoryStream.ToArray(), DefaultSize));
                    }
                }

                if (fname.StartsWith("Global"))
                {
                    GlobalFontNames.Add(fname);
                }
                else if (DefaultDynamicFont == string.Empty)
                {
                    DefaultDynamicFont = fname; // only check in here so empty font folders don't count
                }
            }
        }

        public SpriteFont ResolveFont(string font, int fontsize)
        {
            // tries to get the closest available font to what was requested
            if (Fonts.Keys.Count <= 0 || DefaultFont == string.Empty)
            {
                throw new Exception("No fonts loaded!");
            }
            if (!Fonts.ContainsKey(font))
            {
                font = DefaultFont;
            }
            if (Fonts[font].Keys.Count <= 0)
            {
                font = DefaultFont;
            }
            if (!Fonts[font].ContainsKey(fontsize))
            {
                int best = int.MaxValue;
                foreach (int k in Fonts[font].Keys)
                {
                    if (Math.Abs(k - fontsize) < Math.Abs(best - fontsize))
                    {
                        best = k;
                    }
                }
                fontsize = best;
            }
            return Fonts[font][fontsize];
        }

        public DynamicSpriteFont ResolveDynamicFont(string font, int fontsize)
        {
            // tries to get the closest available font to what was requested
            if (DynamicFonts.Keys.Count <= 0 || DefaultDynamicFont == string.Empty)
            {
                throw new Exception("No dynamic fonts loaded!");
            }
            if (!DynamicFonts.ContainsKey(font))
            {
                return DynamicFonts[DefaultDynamicFont];
            }
            return DynamicFonts[font];
        }

        public RelaFont ResolveRelaFont(string font, int fontsize)
        {
            // reuse font if already exists in size
            if (RelaFonts.TryGetValue(font, out var fontdict))
            {
                if (fontdict.TryGetValue(fontsize, out RelaFont outfont))
                    return outfont;
            }
            else
            {
                RelaFonts.Add(font, new Dictionary<int, RelaFont>());
            }

            // make new font if not found
            RelaFont rfont = new RelaFont();
            rfont.Size = fontsize;
            RelaFonts[font].Add(fontsize, rfont);

            if (DynamicFonts.ContainsKey(font) || !Fonts.ContainsKey(font))
            {
                rfont.IsDynamic = true;
                rfont.DFont = ResolveDynamicFont(font, fontsize);
                FillRelatedDynamicFonts(rfont, font, fontsize);
                return rfont;
            }

            rfont.IsDynamic = false;
            rfont.SFont = ResolveFont(font, fontsize);
            return rfont;
        }

        private void FillRelatedDynamicFonts(RelaFont rfont, string name, int size)
        {
            // add global links
            foreach (string globalfont in GlobalFontNames)
            {
                if (name != globalfont)
                {
                    rfont.RelatedFonts.Add(globalfont, ResolveRelaFont(globalfont, size));
                }
            }

            // find prefix
            int hyphenIndex = name.LastIndexOf('-');
            if (hyphenIndex == -1)
                return; // no prefix

            string prefix = name.Substring(0, hyphenIndex);
            string suffix = name.Substring(hyphenIndex + 1);

            // find all dynamic fonts that match the prefix but not the suffix
            foreach (var kvp in DynamicFonts)
            {
                int keyHyphenIndex = kvp.Key.LastIndexOf('-');
                if (keyHyphenIndex == -1)
                    continue;

                string keyPrefix = kvp.Key.Substring(0, keyHyphenIndex);
                string keySuffix = kvp.Key.Substring(keyHyphenIndex + 1);

                if (keyPrefix == prefix && keySuffix != suffix)
                {
                    rfont.RelatedFonts.Add(keySuffix, ResolveRelaFont(kvp.Key, size));
                }
            }
        }
    }
}