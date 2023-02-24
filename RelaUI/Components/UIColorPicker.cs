using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json.Linq;
using RelaUI.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static RelaUI.Components.UINumberField;

namespace RelaUI.Components
{
    public class UIColorPicker : UIComponent
    {
        public UITexture Gradient;
        public UITexture LightGradient;
        public UITexture Sample;
        public UINumberField RField;
        public UINumberField GField;
        public UINumberField BField;

        private Color ActiveColorField;
        private float Modifier = 1;
        private Color BaseColor;
        private Color[] GradientColorData;
        private bool IgnoreTextUpdates = true;

        private int WidthField;
        public int Width
        {
            get { return WidthField; }
            set
            {
                WidthField = value;

                Resize();
            }
        }
        private int HeightField;
        public int Height
        {
            get { return HeightField; }
            set
            {
                HeightField = value;

                Resize();
            }
        }
        private int ButtonWidthField;
        public int ButtonWidth
        {
            get { return ButtonWidthField; }
            set
            {
                ButtonWidthField = value;

                Resize();
            }
        }
        private int ButtonHeightField;
        public int ButtonHeight
        {
            get { return ButtonHeightField; }
            set
            {
                ButtonHeightField = value;

                Resize();
            }
        }

        private int LightWidthField;
        public int LightWidth
        {
            get { return LightWidthField; }
            set
            {
                LightWidthField = value;

                Resize();
            }
        }

        public UIColorPicker(GraphicsDevice graphics, int x, int y, int width, int height, int lightWidth, int buttonWidth, int buttonHeight, string font = "", int fontSize = 16, int gradientResolution = 256)
        {
            this.x = x;
            this.y = y;
            Width = width;
            Height = height;
            ButtonWidth = buttonWidth;
            ButtonHeight = buttonHeight;
            LightWidth = lightWidth;

            int gradw = Width - LightWidth - ButtonWidth;
            Texture2D texture = GenerateGradientTexture(graphics, gradientResolution, gradientResolution);
            Gradient = new UITexture(0, 0, texture)
            {
                ScaleX = (float)gradw / (float)gradientResolution,
                ScaleY = (float)height / (float)gradientResolution,
            };
            Add(Gradient);
            Gradient.EventFocused += (sender, e) =>
            {
                EventFocusedHandlerArgs eargs = e as EventFocusedHandlerArgs;
                TestMouse(eargs.Input.State.MousePos.X, eargs.Input.State.MousePos.Y, eargs.Dx, eargs.Dy, eargs.ElapsedMS, eargs.Input);
            };

            Texture2D lightTexture = GenerateLightGradientTexture(graphics, gradientResolution);
            LightGradient = new UITexture(0, 0, lightTexture)
            {
                ScaleX = (float)LightWidth,
                ScaleY = (float)height / (float)gradientResolution,
            };
            Add(LightGradient);
            LightGradient.EventFocused += (sender, e) =>
            {
                EventFocusedHandlerArgs eargs = e as EventFocusedHandlerArgs;
                TestMouse(eargs.Input.State.MousePos.X, eargs.Input.State.MousePos.Y, eargs.Dx, eargs.Dy, eargs.ElapsedMS, eargs.Input);
            };

            int samph = height - (ButtonHeight * 3);
            Texture2D sampTexture = new Texture2D(graphics, 1, 1);
            sampTexture.SetData(new Color[] { new Color(1f, 1f, 1f) });
            Sample = new UITexture(gradw, 0, sampTexture)
            {
                ScaleX = ButtonWidth,
                ScaleY = samph
            };
            Add(Sample);

            RField = new UINumberField(gradw, samph, ButtonWidth, ButtonHeight, buttonWidth / 3, "0", true, font, fontSize);
            Add(RField);
            RField.EventValueChanged += (sender, e) =>
            {
                EventValueChangedHandlerArgs eargs = e as EventValueChangedHandlerArgs;
                UpdateColorFromText(eargs.ElapsedMS, eargs.Input);
            };

            GField = new UINumberField(gradw, samph + ButtonHeight, ButtonWidth, ButtonHeight, buttonWidth / 3, "0", true, font, fontSize);
            Add(GField);
            GField.EventValueChanged += (sender, e) =>
            {
                EventValueChangedHandlerArgs eargs = e as EventValueChangedHandlerArgs;
                UpdateColorFromText(eargs.ElapsedMS, eargs.Input);
            };

            BField = new UINumberField(gradw, samph + ButtonHeight * 2, ButtonWidth, ButtonHeight, buttonWidth / 3, "0", true, font, fontSize);
            Add(BField);
            BField.EventValueChanged += (sender, e) =>
            {
                EventValueChangedHandlerArgs eargs = e as EventValueChangedHandlerArgs;
                UpdateColorFromText(eargs.ElapsedMS, eargs.Input);
            };

            IgnoreTextUpdates = false;
        }

        public override int GetWidth()
        {
            return Width;
        }

        public override int GetHeight()
        {
            return Height;
        }

        private void UpdateColorFromText(float elapsedMs, InputManager input)
        {
            Color col = new Color(RField.AsInt(), GField.AsInt(), BField.AsInt());
            if (IgnoreTextUpdates || (col.R == ActiveColorField.R && col.G == ActiveColorField.G && col.B == ActiveColorField.B))
            {
                return;
            }
            Vector3 normal = col.ToVector3();
            normal.Normalize();
            BaseColor = new Color(normal);
            Modifier = (float)(Math.Max(Math.Max(col.R, col.G), col.B)) / 255f;

            SetActiveColor(col, elapsedMs, input);
        }

        public Color GetActiveColor()
        {
            return ActiveColorField;
        }

        public void SetActiveColor(Color col, float elapsedTime, InputManager input)
        {
            IgnoreTextUpdates = true;
            ActiveColorField = col;

            if (Sample != null)
            {
                Sample.Color = col;
            }

            if (LightGradient != null)
            {
                Vector3 normal = col.ToVector3();
                normal.Normalize();
                if (normal.X == 0 && normal.Y == 0 && normal.Z == 0)
                {
                    normal.X = 1; 
                    normal.Y = 1;
                    normal.Z = 1;
                }
                LightGradient.Color = new Color(normal);
            }

            if (RField != null)
            {
                RField.Set((int)col.R, elapsedTime, input);
            }

            if (GField != null)
            {
                GField.Set((int)col.G, elapsedTime, input);
            }

            if (BField != null)
            {
                BField.Set((int)col.B, elapsedTime, input);
            }

            IgnoreTextUpdates = false;
        }

        public void Resize()
        {
            int gradw = Width - ButtonWidth - LightWidth;
            int samph = Height - (ButtonHeight * 3);

            if (Gradient != null)
            {
                Gradient.y = 1;
                Gradient.ScaleX = (float)gradw / (float)Gradient.Texture.Width;
                Gradient.ScaleY = (float)(Height - 2) / (float)Gradient.Texture.Height;
            }

            if (LightGradient != null)
            {
                LightGradient.ScaleX = (float)LightWidth;
                LightGradient.ScaleY = (float)(Height - 2) / (float)LightGradient.Texture.Height;
                LightGradient.y = 1;
                LightGradient.x = gradw;
            }

            if (Sample != null)
            {
                Sample.ScaleX = ButtonWidth;
                Sample.ScaleY = samph;
                Sample.x = gradw + LightWidth;
            }

            if (RField != null)
            {
                RField.Height = ButtonHeight;
                RField.Width = ButtonWidth;
                RField.x = gradw + LightWidth;
                RField.y = samph;
            }

            if (GField != null)
            {
                GField.Height = ButtonHeight;
                GField.Width = ButtonWidth;
                GField.x = gradw + LightWidth;
                GField.y = samph + ButtonHeight;
            }

            if (BField != null)
            {
                BField.Height = ButtonHeight;
                BField.Width = ButtonWidth;
                BField.x = gradw + LightWidth;
                BField.y = samph + ButtonHeight * 2;
            }
        }

        protected override void SelfRender(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input, float dx, float dy)
        {
            base.SelfRender(elapsedms, g, sb, input, dx, dy);

            if (input.MouseDown(eMouseButtons.Left) && Hovering(dx, dy, input))
            {
                TestMouse(input.State.MousePos.X, input.State.MousePos.Y, dx, dy, elapsedms, input);
            }
        }

        protected override void SelfOverRender(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input, float dx, float dy)
        {
            base.SelfOverRender(elapsedms, g, sb, input, dx, dy);

            int modifierBar = (int)dy + (int)(Height - (Modifier * Height));
            Draw.DrawRectangleHandle(sb, (int)dx + Width - ButtonWidth - LightWidth, modifierBar, LightWidth, 3, new Color(1 - Modifier, 1 - Modifier, 1 - Modifier));
        }

        private void TestMouse(float mx, float my, float dx, float dy, float elapsedTime, InputManager input)
        {
            float x = mx - dx;
            float y = my - dy;

            float adjy = y - 1;
            if (adjy < 0)
                adjy = 0;
            if (adjy > Height)
                adjy = Height;

            int gradw = Width - LightWidth - ButtonWidth;
            if (x < gradw)
            {
                x = (float)(x / Gradient.ScaleX);
                y = (float)(adjy / Gradient.ScaleY);
                int xpos = (int)x;
                int ypos = (int)y;
                if (xpos > 0 && xpos < Gradient.Texture.Width && ypos > 0 && ypos < Gradient.Texture.Height)
                {
                    BaseColor = GradientColorData[ypos * Gradient.Texture.Width + xpos];
                    Vector3 moddedColor = BaseColor.ToVector3();
                    moddedColor *= Modifier;
                    SetActiveColor(new Color(moddedColor), elapsedTime, input);
                }
            }
            else if (x < gradw + LightWidth)
            {
                float mod = 1f - (float)(adjy / Height);
                Modifier = mod;
                Vector3 normal = BaseColor.ToVector3();
                normal.Normalize();
                normal *= mod;
                SetActiveColor(new Color(normal), elapsedTime, input);
            }
        }

        public Texture2D GenerateLightGradientTexture(GraphicsDevice graphics, int height)
        {
            Texture2D gradientTexture = new Texture2D(graphics, 1, height);
            Color[] col = new Color[1 * height];

            float v = 1f;
            float vstep = 1f / height;
            for (int i = 0; i < height; i++)
            {
                col[i] = new Color(v, v, v);
                v -= vstep;
                if (v <= 0)
                    v = 0;
            }

            gradientTexture.SetData(col);
            return gradientTexture;
        }

        public Texture2D GenerateGradientTexture(GraphicsDevice graphics, int width, int height)
        {
            Texture2D gradientTexture = new Texture2D(graphics, width, height);
            Color[] col = new Color[width * height];

            float r = 1f;
            float g = 0;
            float b = 0;
            float wstep = 6f / (float)width;
            bool incRed = false;
            bool decRed = false;
            bool incGreen = true;
            bool decGreen = false;
            bool incBlue = false;
            bool decBlue = false;
            for (int i = 0; i < width; i++)
            {
                for (int o = 0; o < height; o++)
                {
                    float mod = (float)o / height;
                    col[o * width + i] = new Color(
                        mod * (1-r) + r, 
                        mod * (1-g) + g,
                        mod * (1-b) + b);
                }

                if (incRed)
                {
                    r += wstep;
                    if (r >= 1)
                    {
                        r = 1;
                        incRed = false;
                        decBlue = true;
                    }
                }
                if (decRed)
                {
                    r -= wstep;
                    if (r <= 0)
                    {
                        r = 0;
                        decRed = false;
                        incBlue = true;
                    }
                }

                if (incGreen)
                {
                    g += wstep;
                    if (g >= 1)
                    {
                        g = 1;
                        incGreen = false;
                        decRed = true;
                    }
                }
                if (decGreen)
                {
                    g -= wstep;
                    if (g <= 0)
                    {
                        g = 0;
                        decGreen = false;
                        incRed = true;
                    }
                }

                if (incBlue)
                {
                    b += wstep;
                    if (b >= 1)
                    {
                        b = 1;
                        incBlue = false;
                        decGreen = true;
                    }
                }
                if (decBlue)
                {
                    b -= wstep;
                    if (b <= 0)
                    {
                        b = 0;
                        decBlue = false;
                        incRed = true;
                    }
                }
            }

            gradientTexture.SetData(col);
            GradientColorData = col;
            return gradientTexture;
        }
    }
}
