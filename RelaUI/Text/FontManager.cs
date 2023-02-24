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
                throw new DirectoryNotFoundException("BMFonts content folder not found!");
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

                if (DefaultDynamicFont == string.Empty)
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
            RelaFont rfont = new RelaFont();
            rfont.Size = fontsize;

            if (DynamicFonts.ContainsKey(font) || !Fonts.ContainsKey(font))
            {
                rfont.IsDynamic = true;
                rfont.DFont = ResolveDynamicFont(font, fontsize);
                return rfont;
            }

            rfont.IsDynamic = false;
            rfont.SFont = ResolveFont(font, fontsize);
            return rfont;
        }
    }
}