using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.IO;

namespace RelaUI.Input
{
    public class CursorManager
    {
        public bool Enabled = true;
        public const int MaxCursorStates = 4;
        public eCursorState State = eCursorState.NORMAL;
        public Dictionary<string, Texture2D[]> CursorImages = new Dictionary<string, Texture2D[]>();
        public Texture2D[] CurrentCursorImages = new Texture2D[MaxCursorStates];
        public Color CursorColor = Color.White;

        public void Render(GraphicsDevice g, SpriteBatch sb, int x, int y)
        {
            if (!Enabled)
                return;
            sb.Draw(CurrentCursorImages[(int)State], new Vector2(x, y), CursorColor);
            /*switch (State)
            {
                case eCursorState.NORMAL:
                    Draw.DrawRectangle(g, sb, Color.Red,
                        x - 1, y - 1, 3, 3);
                    break;
                case eCursorState.WAITING:
                    break;
                case eCursorState.CLICKABLE:
                    Draw.DrawRectangle(g, sb, Color.Red,
                        x - 2, y - 2, 5, 5, outline: true, outlinewidth: 1);
                    break;
                case eCursorState.DOWN:
                    Draw.DrawRectangle(g, sb, Color.DarkRed,
                        x - 1, y - 1, 3, 3);
                    break;
                default:
                    break;
            }*/

            TrySet(eCursorState.NORMAL);
        }

        public void TrySet(eCursorState state)
        {
            if (State != eCursorState.WAITING)
            {
                State = state;
            }
        }


        public void Load(ContentManager content)
        {
            string cursorfolder = "Cursors";
            DirectoryInfo dir = new DirectoryInfo(Path.Combine(content.RootDirectory, cursorfolder));
            if (!dir.Exists)
            {
                throw new DirectoryNotFoundException("Cursors content folder not found!");
            }

            CursorImages = new Dictionary<string, Texture2D[]>();

            // find each directory in this folder
            DirectoryInfo[] dirs = dir.GetDirectories();
            foreach (DirectoryInfo d in dirs)
            {
                string dname = d.Name;
                CursorImages.Add(dname, new Texture2D[MaxCursorStates]);
                FileInfo[] files = d.GetFiles("*.*");
                foreach (FileInfo file in files)
                {
                    string fname = Path.GetFileNameWithoutExtension(file.Name);
                    string spt = fname.Substring(fname.LastIndexOf('_') + 1);
                    eCursorState cstate;
                    if (!Enum.TryParse<eCursorState>(spt, out cstate))
                    {
                        continue; // skip this file if not recognized
                    }

                    CursorImages[dname][(int)cstate] = content.Load<Texture2D>(Path.Combine(cursorfolder, dname, fname));
                }
            }
        }

        public void SetCursor(string name, Color color)
        {
            if (!CursorImages.ContainsKey(name))
            {
                throw new Exception("Cursor " + name + " does not exist!");
            }
            CurrentCursorImages = CursorImages[name];
            CursorColor = color;
        }
    }
}
