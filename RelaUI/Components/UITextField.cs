using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RelaUI.DrawHandles;
using RelaUI.Input;
using RelaUI.Input.Clipboards;
using RelaUI.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RelaUI.Utilities;

namespace RelaUI.Components
{
    public class UITextField : UIComponent
    {
        public int Width = 100;
        public int Height = 25;
        public bool AutoHeight = true;

        public bool HasBackground = true;
        public Color BackgroundColor;
        public bool HasBorder = true;
        public Color BorderColor;
        public int BorderWidth;

        public int ScrollSize = 10; // width of the scroll bar
        public Color ScrollBarBackgroundColor;
        public Color ScrollBarColor;
        private bool WasScrollingVert = false;
        private bool WasScrollingHorz = false;
        private bool IsDragScrolling = false;

        public string PlaceholderText = string.Empty;
        public Color PlaceholderColor;

        public int CaretLine = 0;
        public int CaretPosition = 0;
        private bool CaretBlinking = false;
        private double CaretBlinkTimer = 0;
        public double CaretBlinkTimerMax = 0.5;
        public double CaretBlinkTimerVanishMax = 0.5;

        private int LastTStart = 0; // for a single line textfield, what index is currently '0'?
        // (when a textfield scrolls, it is only displaying a substring of the text at once)
        private int LastTEnd = 0;
        private int TStartAtStartOfScrolling = -1;
        private int TEndAtStartOfScrolling = -1;

        private bool MouseWasDown = false;
        private bool Highlighting = false;
        private int HighlightStartLine = 0;
        private int HighlightEndLine = 0;
        private int HighlightStartPos = 0;
        private int HighlightEndPos = 0;
        private bool ForceHighlightAll = false;
        public Color HighlightColor;

        // for multiline: these are used to track what lines are visible and what
        // their horizontal offset is
        private float WindowX = 0;
        private float WindowY = 0;

        public RenderedTextLines RenderedTextLines = new RenderedTextLines();
        private string TextField = string.Empty;
        public RenderedText RenderedText = new RenderedText();
        private StringBuilder MultiLineStringBuilder;
        public string Text
        {
            get
            {
                if (!MultiLine)
                {
                    return TextField;
                }
                else
                {
                    if (MultiLineStringBuilder == null)
                    {
                        MultiLineStringBuilder = new StringBuilder();
                    }
                    else
                    {
                        MultiLineStringBuilder.Clear();
                    }
                    for (int i = 0; i < RenderedTextLines.Count(); i++)
                    {
                        MultiLineStringBuilder.AppendLine(RenderedTextLines.GetLine(i).Text);
                    }
                    string text = MultiLineStringBuilder.ToString();
                    RenderedText.Text = text;
                    return text;
                }
            }
            private set
            {
                if (!MultiLine)
                {
                    TextField = value;
                    RenderedText.Text = value;
                }
                else
                {
                    RenderedText.Text = value;
                    RenderedTextLines.SetAll(value);
                }
            }
        }
        // NOTE: to update font, updating above will not auto-update the font
        // you must re-init the label or do SFont = FontManager.ResolveFont(Font, FontSize)
        public Color TextColor;
        public RelaFont SFont;
        public TextSettings FontSettings = new TextSettings();

        private ITextStyler TextStyler = null;
        private List<TextStyles> ComputedStyles = new List<TextStyles>();
        private bool SkipNextCompute = false;

        public bool MultiLine = false;

        private float Lastdx = 0; // need to store these for GetFocus
        private float Lastdy = 0;

        public UITextField(float x, float y, int w, int h, string text, string font = "", int? fontsize = null,
            bool autoheight = true, string placeholdertext = "", bool multiLine = false)
        {
            this.x = x;
            this.y = y;
            Width = w;
            Height = h;
            MultiLine = multiLine;
            Text = text;
            FontSettings.FontName = font;
            FontSettings.FontSize = fontsize != null ? (int)fontsize : 0;

            PlaceholderText = placeholdertext;

            // need to change these for width/height preferences to be actually used
            AutoHeight = autoheight;

            ComputeStyles();
        }

        protected override void SelfInit()
        {
            FontSettings.Color = Style.ForegroundColor;
            BackgroundColor = Style.BackgroundColor;
            BorderColor = Style.BackgroundAccent;
            BorderWidth = Style.BorderWidth;

            ScrollBarColor = Style.ForegroundColor;
            ScrollBarBackgroundColor = Style.BackgroundAccent;

            HighlightColor = Style.BackgroundAccent;

            PlaceholderColor = Style.SecondaryTextColor;

            SFont = FontSettings.Init(Style);
            TextColor = FontSettings.Color;

            if (AutoHeight)
            {
                Height = (HasBorder ? BorderWidth * 2 : 1) + 8 + SFont.LineSpacing;
            }

            ComputeStyles();
        }

        public override int GetWidth()
        {
            return Width;
        }

        public override int GetHeight()
        {
            return Height;
        }

        protected override void SelfRender(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input, float dx, float dy)
        {
            Lastdx = dx;
            Lastdy = dy;

            bool hovering = Hovering(dx, dy, input);
            if (hovering)
            {
                input.Cursor.TrySet(eCursorState.CLICKABLE);
            }

            if (HasBackground)
                Draw.DrawRectangleHandle(sb, (int)dx, (int)dy, Width, Height, BackgroundColor);
            if (HasBorder)
                Draw.DrawRectangleHandleOutline(sb, (int)dx, (int)dy, Width, Height, BorderColor, BorderWidth);

            int textheight = SFont.LineSpacing;
            int diff = Height - textheight;
            int mod = 0;
            if (diff > 0)
            {
                mod = diff / 2;
            }
            if (MultiLine)
            {
                mod = BorderWidth;
            }
            if (string.IsNullOrEmpty(Text))
            {
                FontSettings.Color = PlaceholderColor;
                if (!Focused)
                {
                    Draw.DrawText(g, sb, SFont, dx + BorderWidth + 2, dy + mod, PlaceholderText, FontSettings);
                }
                else
                {
                    CaretBlinkTimer += elapsedms/1000.0f;
                    if (!CaretBlinking && CaretBlinkTimer >= CaretBlinkTimerMax)
                    {
                        CaretBlinkTimer = 0;
                        CaretBlinking = true;
                    }
                    else if (CaretBlinking && CaretBlinkTimer >= CaretBlinkTimerVanishMax)
                    {
                        CaretBlinkTimer = 0;
                        CaretBlinking = false;
                    }

                    if (!CaretBlinking)
                    {
                        Draw.DrawRectangleHandle(sb, (int)(dx + BorderWidth + 2), (int)(dy + mod), 2, SFont.LineSpacing, TextColor);
                    }
                }
            }
            else
            {
                // only render the caret if this is active
                bool doCaret = false;
                if (Focused)
                {
                    CaretBlinkTimer += elapsedms/1000.0f;
                    if (!CaretBlinking && CaretBlinkTimer >= CaretBlinkTimerMax)
                    {
                        CaretBlinkTimer = 0;
                        CaretBlinking = true;
                    }
                    else if (CaretBlinking && CaretBlinkTimer >= CaretBlinkTimerVanishMax)
                    {
                        CaretBlinkTimer = 0;
                        CaretBlinking = false;
                    }

                    if (!CaretBlinking)
                        doCaret = true;
                }

                FontSettings.Color = TextColor;
                int tstart = 0;
                int tend = 0;
                if (!MultiLine)
                {
                    int textwidth = TextHelper.GetWidthMultiStyles(SFont, RenderedText, ComputedStyles[0]);
                    int maxwidth = Width - (HasBorder ? BorderWidth * 2 : 0) - 4;
                    tend = Text.Length;
                    if (textwidth > maxwidth)
                    {
                        int wperc = textwidth / Text.Length;
                        int cs = maxwidth / wperc;
                        int postcaretcs = Text.Length - CaretPosition;
                        int precaretcs = CaretPosition;
                        if (postcaretcs < cs)
                        {
                            tstart = CaretPosition - (cs - postcaretcs);
                            if (tstart + 3 > CaretPosition)
                            {
                                tstart = CaretPosition - 3;
                                tend = CaretPosition + (cs - 3);
                            }
                        }
                        else if (CaretPosition < 3)
                        {
                            tstart = 0;
                            tend = cs;
                        }
                        else
                        {
                            tstart = CaretPosition - 3;
                            tend = CaretPosition + (cs - 3);
                        }

                        if (tstart < 0)
                        {
                            tstart = 0;
                        }

                        while ((tend - tstart > 0) 
                            && TextHelper.GetWidthMultiStyles(SFont, RenderedText, ComputedStyles[0], tstart, tend - tstart) > maxwidth)
                        {
                            if (postcaretcs < cs)
                            {
                                tstart++;
                            }
                            else
                            {
                                tend--;
                            }
                        }
                    }

                    // special override: if user is currently scrolling with the mouse
                    // do not move the sliding window until they stop scrolling
                    if (TStartAtStartOfScrolling != -1)
                    {
                        tstart = TStartAtStartOfScrolling;
                        tend = TEndAtStartOfScrolling;
                    }

                    TextStyles adjustedStyles = ComputedStyles[0].Adjust(tstart);

                    if (Highlighting)
                    {
                        // draw highlight background first
                        int h1pos;
                        int h2pos;
                        // user could drag backwards, so we need to sort the highlight positions
                        if (HighlightStartPos <= HighlightEndPos)
                        {
                            h1pos = HighlightStartPos;
                            h2pos = HighlightEndPos;
                        }
                        else
                        {
                            h2pos = HighlightStartPos;
                            h1pos = HighlightEndPos;
                        }

                        if (h1pos != h2pos && h2pos > tstart && h1pos < tend)
                        {
                            if (h1pos < tstart) h1pos = tstart;
                            if (h2pos > tend) h2pos = tend;

                            int hstartx = 0;
                            if (h1pos != tstart)
                                hstartx = TextHelper.GetWidthMultiStyles(SFont, RenderedText, adjustedStyles, tstart, h1pos - tstart);
                            int hendx = TextHelper.GetWidthMultiStyles(SFont, RenderedText, adjustedStyles, tstart, h2pos - tstart);

                            Draw.DrawRectangleHandle(sb,
                                (int)(dx + BorderWidth + 2 + hstartx),
                                (int)(dy + mod),
                                hendx - hstartx,
                                textheight,
                                HighlightColor);
                        }
                    }

                    LastTStart = tstart;
                    LastTEnd = tend;
                    Draw.DrawTextMultiStyles(g, sb, dx + BorderWidth + 2, dy + mod, dx, dy, Text.Substring(tstart, tend - tstart), adjustedStyles);

                    if (doCaret)
                    {
                        float caretx = 0;
                        if (CaretPosition - tstart > 0)
                        {
                            caretx = TextHelper.GetWidthMultiStyles(SFont, RenderedText, ComputedStyles[0], tstart, CaretPosition - tstart);//SFont.MeasureString(Text.Substring(tstart, CaretPosition - tstart)).X;
                        }
                        Draw.DrawRectangleHandle(sb, (int)(dx + BorderWidth + 2 + caretx), (int)(dy + mod), 2, SFont.LineSpacing, TextColor);
                    }
                }
                else
                {
                    Rectangle oldrect = RenderTargetScope.Open(sb, (int)Math.Floor(dx + BorderWidth), (int)Math.Floor(dy + BorderWidth), Width - BorderWidth * 2, Height - BorderWidth * 2);
                    {
                        if (Highlighting)
                        {
                            // draw highlight background first
                            // user could drag backwards, so we need to sort the highlight positions
                            OrderHighlights(out int h1line, out int h2line, out int h1pos, out int h2pos);

                            for (int i = h1line; i <= h2line; i++)
                            {
                                RenderedText hrendered = RenderedTextLines.GetLine(i);
                                string htext = hrendered.Text;
                                int hstart = 0;
                                int hend = htext.Length;
                                if (i == h1line) hstart = h1pos;
                                if (i == h2line) hend = h2pos;

                                if (hstart == hend)
                                    continue;

                                int hstartx = 0;
                                if (hstart != 0)
                                    hstartx = TextHelper.GetWidthMultiStyles(SFont, hrendered, ComputedStyles[i], 0, hstart);
                                int hendx = TextHelper.GetWidthMultiStyles(SFont, hrendered, ComputedStyles[i], 0, hend);

                                Draw.DrawRectangleHandle(sb,
                                    (int)(dx + BorderWidth + 2 + hstartx - WindowX),
                                    (int)(dy + mod + (textheight * i) - WindowY),
                                    hendx - hstartx,
                                    textheight,
                                    HighlightColor);
                            }
                        }

                        for (int i = 0; i < RenderedTextLines.Count(); i++)
                        {
                            string line = RenderedTextLines.GetLine(i).Render(SFont);
                            Draw.DrawTextMultiStyles(g, sb, dx + BorderWidth + 2 - WindowX, dy + mod + (textheight * i) - WindowY, dx, dy, line, ComputedStyles[i]);
                        }

                        if (doCaret)
                        {
                            float caretx = 0;
                            float carety = textheight * CaretLine;
                            if (CaretPosition > 0)
                            {
                                caretx = TextHelper.GetWidthMultiStyles(SFont, RenderedTextLines.GetLine(CaretLine), ComputedStyles[CaretLine], 0, CaretPosition);
                            }
                            Draw.DrawRectangleHandle(sb, (int)(dx + BorderWidth + 2 + caretx - WindowX), (int)(dy + mod + carety - WindowY), 2, SFont.LineSpacing, TextColor);
                        }
                    }
                    RenderTargetScope.Close(sb, oldrect);

                    // draw scrollbars 
                    // note that this happens outside the render scope so they're on top of things
                    int maxwindowy = GetMaxWindowY();
                    int maxwindowx = GetMaxWindowX();

                    // draw vertical scrollbar
                    bool hasvert = false;
                    if (ScrollVertVisible())
                    {
                        hasvert = true;
                        int sfw = ScrollSize;
                        int sfx = (int)(dx + Width - sfw);
                        int sfy = (int)(dy + BorderWidth);
                        int sfh = Height - BorderWidth * 2;
                        // draw the background
                        Draw.DrawRectangleHandle(sb, sfx, sfy, sfw, sfh, ScrollBarBackgroundColor);
                        // now draw the bar itself
                        int sfbarw = GetSliderBarWidth(vert: true);
                        int sfbarpos = (int)((sfh - sfbarw) * (WindowY / (float)maxwindowy));
                        Draw.DrawRectangleHandle(sb, sfx, sfy + sfbarpos, sfw, sfbarw, ScrollBarColor);
                    }

                    // draw horizontal scrollbar
                    if (ScrollHorzVisible())
                    {
                        int sfh = ScrollSize;
                        int sfy = (int)(dy + Height - sfh);
                        int sfx = (int)(dx + BorderWidth);
                        int sfw = Width - BorderWidth * 2;
                        if (hasvert)
                            sfw -= ScrollSize;
                        // draw the background
                        Draw.DrawRectangleHandle(sb, sfx, sfy, sfw, sfh, ScrollBarBackgroundColor);
                        // now draw the bar itself
                        int sfbarw = GetSliderBarWidth(vert: false);
                        int sfbarpos = (int)((sfw - sfbarw) * (WindowX / (float)maxwindowx));
                        Draw.DrawRectangleHandle(sb, sfx + sfbarpos, sfy, sfbarw, sfh, ScrollBarColor);
                    }
                }
            }
        }

        //accept input
        private void ProcessAdd(string add, ref bool first, float elapsedMS, InputManager input)
        {
            if (!MultiLine)
            {
                if (!first)
                {
                    // if not our first, it means enter was pressed...
                    EnterPressed(elapsedMS, input);
                    return; // only handle one line
                }
                else
                {
                    first = false;
                }
                if (Text.Length == 0)
                {
                    Text = add;
                }
                else if (CaretPosition == Text.Length)
                {
                    Text = Text + add;
                }
                else if (CaretPosition == 0)
                {
                    Text = add + Text;
                }
                else if (CaretPosition > 0 && CaretPosition < Text.Length)
                {
                    Text = Text.Substring(0, CaretPosition) + add + Text.Substring(CaretPosition);
                }
                CaretPosition += add.Length;
            }
            else
            {
                if (!first)
                {
                    string curtext = RenderedTextLines.GetLine(CaretLine).Text;
                    RenderedTextLines.Insert(CaretLine + 1, curtext.Substring(CaretPosition));
                    RenderedTextLines.Set(CaretLine, curtext.Substring(0, CaretPosition));
                    CaretLine++;
                    CaretPosition = 0;
                }
                else
                {
                    first = false;
                }
                string tl = RenderedTextLines.GetLine(CaretLine).Text;
                if (tl.Length == 0)
                {
                    RenderedTextLines.Set(CaretLine, add);
                }
                else if (CaretPosition == tl.Length)
                {
                    RenderedTextLines.Set(CaretLine, RenderedTextLines.GetLine(CaretLine).Text + add);
                }
                else if (CaretPosition == 0)
                {
                    RenderedTextLines.Set(CaretLine, add + tl);
                }
                else if (CaretPosition > 0 && CaretPosition < tl.Length)
                {
                    RenderedTextLines.Set(CaretLine, tl.Substring(0, CaretPosition) + add + tl.Substring(CaretPosition));
                }
                CaretPosition += add.Length;
                if (CaretPosition > RenderedTextLines.GetLine(CaretLine).Text.Length)
                {
                    CaretPosition = RenderedTextLines.GetLine(CaretLine).Text.Length;
                }
            }
        }

        private void ProcessBackspace(bool dodelete, ref bool changedLines)
        {
            if (!MultiLine)
            {
                if (Highlighting)
                {
                    DeleteHighlighted();
                }
                else if (dodelete)
                {
                    if (Text.Length > CaretPosition + 1)
                    {
                        Text = Text.Substring(0, CaretPosition) + Text.Substring(CaretPosition + 1);
                    }
                }
                else
                {
                    if (Text.Length > 0 && CaretPosition > 0)
                    {
                        Text = Text.Substring(0, CaretPosition - 1) + Text.Substring(CaretPosition);
                        CaretPosition--;
                    }
                }
            }
            else
            {
                if (Highlighting)
                {
                    // remove the selected segment
                    DeleteHighlighted();
                }
                else if (dodelete)
                {
                    string tl = RenderedTextLines.GetLine(CaretLine).Text;
                    if (tl.Length > CaretPosition + 1)
                    {
                        RenderedTextLines.Set(CaretLine, tl.Substring(0, CaretPosition) + tl.Substring(CaretPosition + 1));
                    }
                    else if (tl.Length == CaretPosition + 1)
                    {
                        RenderedTextLines.Set(CaretLine, tl.Substring(0, CaretPosition));
                    }
                    else
                    {
                        // remove the next line
                        if (CaretLine + 1 < RenderedTextLines.Count())
                        {
                            int origlen = RenderedTextLines.GetLine(CaretLine).Text.Length;
                            RenderedTextLines.Set(CaretLine, RenderedTextLines.GetLine(CaretLine).Text + RenderedTextLines.GetLine(CaretLine + 1).Text);
                            RenderedTextLines.RemoveAt(CaretLine + 1);
                            changedLines = true;
                            CaretPosition = origlen;
                        }
                    }
                }
                else
                {
                    string tl = RenderedTextLines.GetLine(CaretLine).Text;
                    if (tl.Length > 0 && CaretPosition > 0)
                    {
                        RenderedTextLines.Set(CaretLine, tl.Substring(0, CaretPosition - 1) + tl.Substring(CaretPosition));
                        CaretPosition--;
                    }
                    else
                    {
                        // remove the new line
                        if (CaretLine > 0)
                        {
                            int origlen = RenderedTextLines.GetLine(CaretLine - 1).Text.Length;
                            RenderedTextLines.Set(CaretLine - 1, RenderedTextLines.GetLine(CaretLine - 1).Text + RenderedTextLines.GetLine(CaretLine).Text);
                            RenderedTextLines.RemoveAt(CaretLine);
                            CaretLine--;
                            changedLines = true;
                            CaretPosition = origlen;
                        }
                    }
                }
            }
        }

        private InputManager.WritingOrder[] WritingOrders = new InputManager.WritingOrder[16];
        public override void SelfFocusedInput(float elapsedms, InputManager input)
        {
            bool changed = false;
            bool changedLines = false;
            int oldCaretPosition = CaretPosition;
            bool skipDeleteHighlight = false;

            // if multiline, handle enter
            if (MultiLine && input.EnterFresh())
            {
                changedLines = true;
            }

            // handle tabs
            bool ignoreTabs = false;
            if (input.TabFresh())
            {
                if (MultiLine && Highlighting && HighlightStartLine != HighlightEndLine)
                {
                    // special case: tab all at once
                    if (!input.ShiftDown())
                        TabHighlighted();
                    else
                        ShiftTabHighlighted();
                    ignoreTabs = true;
                }
            }

            int ordercount = input.FreshWriting(WritingOrders, true, ignoreTabs);

            // handle duplicate
            if (input.CtrlDown() && input.IsKeyFresh(Keys.D))
            {
                if (Highlighting)
                {
                    WritingOrders[ordercount] = new InputManager.WritingOrder()
                    {
                        Add = GetHighlightedText(),
                        Order = InputManager.eWritingOrder.ADDTOEND
                    };
                    ordercount++;
                    skipDeleteHighlight = true;
                }
                else if (MultiLine)
                {
                    WritingOrders[ordercount] = new InputManager.WritingOrder()
                    {
                        Add = "\n" + RenderedTextLines.GetLine(CaretLine).Text,
                        Order = InputManager.eWritingOrder.ADDTOEND
                    };
                    ordercount++;
                }
                else
                {
                    WritingOrders[ordercount] = new InputManager.WritingOrder()
                    {
                        Add = Text,
                        Order = InputManager.eWritingOrder.ADD
                    };
                    ordercount++;
                }
                input.CaptureInput(Keys.D);
            }

            // process the orders
            if (ordercount > 0)
            {
                bool first = true;
                for (int i = 0; i < ordercount; i++)
                {
                    first = true;
                    InputManager.WritingOrder order = WritingOrders[i];

                    if (order.Order == InputManager.eWritingOrder.ADD || order.Order == InputManager.eWritingOrder.ADDTOEND)
                    {
                        if (order.Order == InputManager.eWritingOrder.ADDTOEND)
                        {
                            if (Highlighting)
                            {
                                OrderHighlights(out int h1line, out int h2line, out int h1pos, out int h2pos);
                                Highlighting = false;
                                CaretLine = h2line;
                                CaretPosition = h2pos;
                            }
                            else if (MultiLine)
                            {
                                CaretPosition = RenderedTextLines.GetLine(CaretLine).Text.Length;
                            }
                        }

                        if (Highlighting && !skipDeleteHighlight)
                            DeleteHighlighted();

                        if (order.Add.Contains("\n") || order.Add.Contains("\r"))
                        {
                            changed = true;
                            List<string> adds = order.Add.SplitByLines();
                            for (int o = 0; o < adds.Count; o++)
                            {
                                ProcessAdd(adds[o], ref first, elapsedms, input);
                            }
                        }
                        else
                        {
                            changed = true;
                            ProcessAdd(order.Add, ref first, elapsedms, input);
                        }
                    }
                    else if (order.Order == InputManager.eWritingOrder.BACKSPACE)
                    {
                        changed = true;

                        if (Highlighting && !skipDeleteHighlight)
                            DeleteHighlighted();
                        else
                            ProcessBackspace(false, ref changedLines);
                    }
                    else if (order.Order == InputManager.eWritingOrder.DELETE)
                    {
                        changed = true;

                        if (Highlighting && !skipDeleteHighlight)
                            DeleteHighlighted();
                        else
                            ProcessBackspace(true, ref changedLines);
                    }
                    else if (order.Order == InputManager.eWritingOrder.LEFT)
                    {
                        CaretBlinking = false; // keep caret visible
                        CaretPosition--;
                        if (CaretPosition < 0)
                        {
                            if (MultiLine && CaretLine > 0)
                            {
                                CaretLine--;
                                CaretPosition = RenderedTextLines.GetLine(CaretLine).Text.Length;
                            }
                            else
                            {
                                CaretPosition = 0;
                            }
                        }

                        if (Highlighting)
                        {
                            CaretPosition = HighlightStartPos;
                            if (MultiLine)
                                CaretLine = HighlightStartLine;
                        }
                        input.CaptureInput(Keys.Left);
                        Highlighting = false; // breaks highlight
                    }
                    else if (order.Order == InputManager.eWritingOrder.RIGHT)
                    {
                        CaretBlinking = false; // keep caret visible
                        CaretPosition++;
                        if (!MultiLine)
                        {
                            if (CaretPosition > Text.Length)
                            {
                                CaretPosition = Text.Length;
                            }
                        }
                        else
                        {
                            if (CaretPosition > RenderedTextLines.GetLine(CaretLine).Text.Length)
                            {
                                if (CaretLine < RenderedTextLines.Count() - 1)
                                {
                                    CaretLine++;
                                    CaretPosition = 0;
                                }
                                else
                                {
                                    CaretPosition = RenderedTextLines.GetLine(CaretLine).Text.Length;
                                }
                            }
                        }
                        input.CaptureInput(Keys.Right);
                        Highlighting = false; // breaks highlight
                    }
                    else if (order.Order == InputManager.eWritingOrder.UP && MultiLine)
                    {
                        CaretBlinking = false; // keep caret visible
                        int oldcaretlen = TextHelper.GetWidthMultiStyles(SFont, RenderedTextLines.GetLine(CaretLine), ComputedStyles[CaretLine], 0, CaretPosition);
                        CaretLine--;
                        if (CaretLine < 0)
                        {
                            CaretLine = 0;
                        }
                        changedLines = true;

                        // determine new caret position based on the old one
                        CaretPosition = DeterminePositionWithinLine(RenderedTextLines.GetLine(CaretLine), oldcaretlen, ComputedStyles[CaretLine]);

                        input.CaptureInput(Keys.Up);

                        Highlighting = false; // breaks highlight
                    }
                    else if (order.Order == InputManager.eWritingOrder.DOWN && MultiLine)
                    {
                        CaretBlinking = false; // keep caret visible
                        int oldcaretlen = TextHelper.GetWidthMultiStyles(SFont, RenderedTextLines.GetLine(CaretLine), ComputedStyles[CaretLine], 0, CaretPosition);
                        CaretLine++;
                        if (CaretLine >= RenderedTextLines.Count())
                        {
                            CaretLine = RenderedTextLines.Count() - 1;
                        }
                        changedLines = true;

                        // determine new caret position based on the old one
                        CaretPosition = DeterminePositionWithinLine(RenderedTextLines.GetLine(CaretLine), oldcaretlen, ComputedStyles[CaretLine]);

                        input.CaptureInput(Keys.Up);

                        Highlighting = false; // breaks highlight
                    }
                }
            }
            
            // fix caret if we changed lines
            if (MultiLine && changedLines && CaretPosition > RenderedTextLines.GetLine(CaretLine).Text.Length)
            {
                CaretPosition = RenderedTextLines.GetLine(CaretLine).Text.Length;
            }

            // handle select all
            if (input.CtrlDown() && input.IsKeyFresh(Keys.A))
            {
                ForceHighlightAll = true;
                input.CaptureInput(Keys.A);
            }

            // handle copy
            if (input.CtrlDown() && input.IsKeyFresh(Keys.C))
            {
                CopySelectionToClipboard();
                input.CaptureInput(Keys.C);
            }

            // handle cut
            if (input.CtrlDown() && input.IsKeyFresh(Keys.X))
            {
                CutSelectionToClipboard();
                input.CaptureInput(Keys.X);
                changed = true;
            }

            // compute styles if anything changed
            if (MultiLine && changed)
            {
                SkipNextCompute = false;
                ComputeStyles();
                SkipNextCompute = true;
            }

            // handle window movement
            if (MultiLine)
            {
                int linespacing = SFont.LineSpacing;
                int maxwindowy = GetMaxWindowY();
                int maxwindowx = GetMaxWindowX();

                // handle scroll wheel
                int scrolldiff = input.FreshMouseScrollDifference();
                if (scrolldiff > 0)
                {
                    WindowY -= scrolldiff / 2;
                    if (WindowY < 0)
                    {
                        WindowY = 0;
                    }
                    input.CaptureMouseScroll();
                }
                else if (scrolldiff < 0)
                {
                    WindowY -= scrolldiff / 2;
                    if (WindowY > maxwindowy)
                    {
                        WindowY = maxwindowy;
                    }
                    input.CaptureMouseScroll();
                }

                bool scrolled = false;
                // handle mouse input for scroll bars
                if (input.MouseDown(eMouseButtons.Left))
                {
                    if (ScrollHorzVisible(maxwindowx))
                    {
                        scrolled = UpdateHorzScrolling(elapsedms, input);
                    }
                    if (ScrollVertVisible(maxwindowy))
                    {
                        scrolled = scrolled ? true : UpdateVertScrolling(elapsedms, input);
                    }

                    // handle mouse input for caret location
                    // only if user isn't using the scroll bars
                    if (!scrolled)
                    {
                        int mx = input.State.MousePos.X;
                        int my = input.State.MousePos.Y;

                        if (mx > Lastdx && mx < Lastdx + Width && my > Lastdy && my < Lastdy + Height)
                        {
                            // prevent caret from being invisible while being moved
                            CaretBlinking = false;

                            // check for highlighting start
                            if (MouseWasDown && Highlighting == false)
                            {
                                // begin highlighting
                                Highlighting = true;
                                HighlightStartLine = CaretLine;
                                HighlightStartPos = CaretPosition;
                            }
                            else if (!MouseWasDown)
                            {
                                // clicking anywhere cancels highlighting
                                Highlighting = false;
                            }

                            // if mouse is inside our box, figure out where and place caret appropriately
                            int mline = (int)(my - Lastdy - BorderWidth + WindowY) / linespacing;
                            if (mline < 0)
                            {
                                CaretLine = 0;
                                CaretPosition = 0;
                            }
                            else if (mline >= RenderedTextLines.Count())
                            {
                                // trivial case; clicking after end of text
                                // place cursor at very end of text
                                CaretLine = RenderedTextLines.Count() - 1;
                                CaretPosition = RenderedTextLines.GetLine(CaretLine).Text.Length;
                            }
                            else
                            {
                                // move cursor to selected line
                                CaretLine = mline;
                                // determine which position to place caret within the line
                                CaretPosition = DeterminePositionWithinLine(RenderedTextLines.GetLine(CaretLine), (int)(mx - Lastdx - BorderWidth), ComputedStyles[CaretLine]);
                            }

                            // continue highlighting
                            if (MouseWasDown && Highlighting)
                            {
                                HighlightEndLine = CaretLine;
                                HighlightEndPos = CaretPosition;

                                if (HighlightEndLine == HighlightStartLine && HighlightEndPos == HighlightStartPos)
                                {
                                    // if we haven't highlighted anything, don't mark that we're highlighting
                                    Highlighting = false;
                                }
                            }

                            MouseWasDown = true;
                        }
                    }
                }
                else
                {
                    WasScrollingHorz = false;
                    WasScrollingVert = false;
                    MouseWasDown = false;
                    IsDragScrolling = false;
                }

                // if the line changed, make sure it's visible
                if (changedLines)
                {
                    if ((linespacing * (CaretLine + 1)) - WindowY >= Height)
                    {
                        WindowY = ((linespacing * (CaretLine + 1)) - Height) + linespacing;
                    }
                    else if ((linespacing * (CaretLine - 1)) - WindowY < 0)
                    {
                        WindowY = (linespacing * CaretLine) - linespacing;
                    }

                    if (WindowY < 0) WindowY = 0;
                }

                if (maxwindowy < Height / 3)
                    WindowY = 0; // special case: if text not long enough yet, prevent scrolling

                // if the caret position changed, make sure it's visible
                if (CaretPosition != oldCaretPosition)
                {
                    if (CaretLine >= ComputedStyles.Count)
                    {
                        SkipNextCompute = false;
                        ComputeStyles();
                        SkipNextCompute = true;
                    }
                    float caretx = TextHelper.GetWidthMultiStyles(SFont, RenderedTextLines.GetLine(CaretLine), ComputedStyles[CaretLine], 0, CaretPosition);
                    int mincaretbuffer = 15; // must have at least this many pixels on the other side of the 
                    if (caretx - WindowX - mincaretbuffer < 0)
                    {
                        WindowX = caretx - mincaretbuffer;
                    }
                    else if (caretx - WindowX + mincaretbuffer > Width)
                    {
                        WindowX = (caretx - Width) + mincaretbuffer;
                    }

                    if (WindowX < 0) WindowX = 0;
                }
            }
            else
            {
                // handle mouse input for caret location
                if (input.MouseDown(eMouseButtons.Left))
                {
                    int mx = input.State.MousePos.X;
                    int my = input.State.MousePos.Y;

                    if (mx > Lastdx && mx < Lastdx + Width && my > Lastdy && my < Lastdy + Height)
                    {
                        // prevent caret from being invisible while being moved
                        CaretBlinking = false;

                        // check for highlighting start
                        if (MouseWasDown && Highlighting == false)
                        {
                            // begin highlighting
                            Highlighting = true;
                            HighlightStartPos = CaretPosition;
                        }
                        else if (!MouseWasDown)
                        {
                            // clicking anywhere cancels highlighting
                            Highlighting = false;
                        }

                        if (TStartAtStartOfScrolling == -1)
                        {
                            TStartAtStartOfScrolling = LastTStart;
                            TEndAtStartOfScrolling = LastTEnd;
                        }

                        // if mouse is inside our box, figure out where and place caret appropriately
                        CaretPosition = TStartAtStartOfScrolling + DeterminePositionWithinLine(RenderedText, (int)(mx - Lastdx - BorderWidth), ComputedStyles[0].Adjust(TStartAtStartOfScrolling), TStartAtStartOfScrolling);

                        // continue highlighting
                        if (MouseWasDown && Highlighting)
                        {
                            HighlightEndPos = CaretPosition;

                            if (HighlightEndPos == HighlightStartPos)
                                Highlighting = false; // not really highlighting anything
                        }

                        MouseWasDown = true;
                    }
                }
                else
                {
                    TStartAtStartOfScrolling = -1;
                    MouseWasDown = false;
                }
            }

            if (ForceHighlightAll)
            {
                Highlighting = true;
                HighlightStartLine = 0;
                HighlightStartPos = 0;
                HighlightEndLine = MultiLine ? RenderedTextLines.Count() : 0;
                HighlightEndPos = Text.Length;
                CaretPosition = Text.Length;
                // keep forcing this until the mouse lets up
                if (!input.MouseDown(eMouseButtons.Left))
                    ForceHighlightAll = false;
            }

            // trigger text changed event if any modifications occured
            if (changed)
            {
                TextChanged(elapsedms, input);
            }
        }

        public void CopyAllToClipboard()
        {
            Clipboard.Set(Text);
        }

        public void CopySelectionToClipboard()
        {
            if (Highlighting)
            {
                Clipboard.Set(GetHighlightedText());
            }
            else
            {
                if (MultiLine)
                {
                    // copy the current line
                    Clipboard.Set(RenderedTextLines.GetLine(CaretLine).Text);
                }
                else
                {
                    // copy everything
                    Clipboard.Set(Text);
                }
            }
        }

        public void CutSelectionToClipboard()
        {
            if (Highlighting)
            {
                Clipboard.Set(GetHighlightedText());
                DeleteHighlighted();
            }
            else
            {
                if (MultiLine)
                {
                    // copy the current line
                    Clipboard.Set(RenderedTextLines.GetLine(CaretLine).Text);
                    RenderedTextLines.RemoveAt(CaretLine);
                    CaretLine--;
                    if (CaretLine < 0) CaretLine = 0;
                    if (RenderedTextLines.Count() <= 0) RenderedTextLines.Add("");
                    CaretPosition = RenderedTextLines.GetLine(CaretLine).Text.Length;
                    ComputeStyles();
                }
                else
                {
                    // copy everything
                    Clipboard.Set(Text);
                    Text = string.Empty;
                }
            }
        }

        private void TabHighlighted()
        {
            OrderHighlights(out int h1line, out int h2line, out int h1pos, out int h2pos);

            for (int i = h1line; i <= h2line; i++)
            {
                RenderedTextLines.Set(i, "\t" + RenderedTextLines.GetLine(i).Text);
            }

            // account for added tab
            CaretPosition++;
            HighlightStartPos++;
            HighlightEndPos++;
        }

        private void ShiftTabHighlighted()
        {
            OrderHighlights(out int h1line, out int h2line, out int h1pos, out int h2pos);

            bool changedFirst = false;
            bool changedLast = false;

            for (int i = h1line; i <= h2line; i++)
            {
                if (RenderedTextLines.GetLine(i).Text.StartsWith('\t'))
                {
                    RenderedTextLines.Set(i, RenderedTextLines.GetLine(i).Text.Substring(1));
                    if (i == h1line)
                        changedFirst = true;
                    if (i == h2line)
                        changedLast = true;
                }
            }

            if (CaretLine == h1line && changedFirst)
                CaretPosition--;
            if (CaretLine == h2line && changedLast)
                CaretPosition--;
            if (changedFirst)
                HighlightStartPos--;
            if (changedLast)
                HighlightEndPos--; // account for missing tab
            if (HighlightStartPos < 0)
                HighlightStartPos = 0;
            if (HighlightEndPos < 0)
                HighlightEndPos = 0;
            if (CaretPosition < 0)
                CaretPosition = 0;
        }

        private void DeleteHighlighted()
        {
            if (MultiLine)
            {
                // user could drag backwards, so we need to sort the highlight positions
                OrderHighlights(out int h1line, out int h2line, out int h1pos, out int h2pos);

                // trim start
                int h1end = RenderedTextLines.GetLine(h1line).Text.Length;
                if (h2line == h1line)
                    h1end = h2pos;
                RenderedTextLines.Set(h1line, RenderedTextLines.GetLine(h1line).Text.CutOut(h1pos, h1end));

                // trim end
                if (h2line != h1line)
                {
                    RenderedTextLines.Set(h2line, RenderedTextLines.GetLine(h2line).Text.CutOut(0, h2pos));
                    // append last line to start line
                    RenderedTextLines.Set(h1line, RenderedTextLines.GetLine(h1line).Text + RenderedTextLines.GetLine(h2line).Text);
                    RenderedTextLines.RemoveAt(h2line);
                }

                // trim lines inbetween
                for (int i = h2line - 1; i > h1line; i--)
                    RenderedTextLines.RemoveAt(i);

                CaretLine = h1line;
                CaretPosition = h1pos;

                // to avoid difficulties here, just recompute the styles now
                ComputeStyles();
            }
            else
            {
                // remove the selected segment
                int hstart = HighlightStartPos;
                int hend = HighlightEndPos;
                if (HighlightEndPos < HighlightStartPos)
                {
                    hstart = HighlightEndPos;
                    hend = HighlightStartPos;
                }
                Text = Text.CutOut(hstart, hend - hstart);
                CaretPosition = hstart;
            }

            Highlighting = false;
        }

        public string GetHighlightedText()
        {
            if (MultiLine)
            {
                // user could drag backwards, so we need to sort the highlight positions
                OrderHighlights(out int h1line, out int h2line, out int h1pos, out int h2pos);

                // simple case: same line
                if (h1line == h2line)
                {
                    return RenderedTextLines.GetLine(h1line).Text.Substring(h1pos, h2pos - h1pos);
                }

                StringBuilder sb = new StringBuilder();

                // append first line
                sb.AppendLine(RenderedTextLines.GetLine(h1line).Text.Substring(h1pos));

                // append middle lines
                for (int i = h1line + 1; i < h2line; i++)
                {
                    sb.AppendLine(RenderedTextLines.GetLine(i).Text);
                }

                // append last line
                sb.Append(RenderedTextLines.GetLine(h2line).Text.Substring(0, h2pos));

                return sb.ToString();
            }
            else
            {
                int hstart = HighlightStartPos;
                int hend = HighlightEndPos;
                if (HighlightEndPos < HighlightStartPos)
                {
                    hstart = HighlightEndPos;
                    hend = HighlightStartPos;
                }

                if (hstart == hend)
                    return string.Empty;
                return Text.Substring(hstart, hend - hstart);
            }
        }

        private bool UpdateVertScrolling(float elapsedms, InputManager input)
        {
            bool changed = false;
            int mx = input.State.MousePos.X;
            int my = input.State.MousePos.Y;

            float sfx = Lastdx + Width - ScrollSize;
            float sfy = Lastdy + BorderWidth;
            int sfw = ScrollSize;
            int sfh = Height - BorderWidth * 2;
            if (((mx > sfx && mx < sfx + sfw && my > sfy && my < sfy + sfh) || WasScrollingVert) && !WasScrollingHorz)
            {
                WasScrollingVert = true;
                int maxwindowy = GetMaxWindowY();
                int sbarw = GetSliderBarWidth(true);
                float sliderratio = ((float)(WindowY) / (float)(maxwindowy));
                float slidery = sfy + (sliderratio * (sfh - sbarw));
                if (IsDragScrolling || (my >= slidery && my <= slidery + sbarw))
                {
                    WindowY += my - input.LastState.MousePos.Y;
                    IsDragScrolling = true;
                }
                else
                {
                    WindowY = (int)(Math.Round((1.0f - (((sfy + sfh) - my) / (sfh))) * (maxwindowy)));
                }

                changed = true;
                if (WindowY < 0)
                {
                    WindowY = 0;
                }
                if (WindowY > maxwindowy)
                {
                    WindowY = maxwindowy;
                }
            }
            return changed;
        }

        private bool UpdateHorzScrolling(float elapsedms, InputManager input)
        {
            bool changed = false;
            int mx = input.State.MousePos.X;
            int my = input.State.MousePos.Y;

            float sfx = Lastdx + BorderWidth;
            float sfy = Lastdy + Height - ScrollSize - BorderWidth;
            int sfw = Width - (BorderWidth * 2);
            int sfh = ScrollSize;

            if (ScrollVertVisible())
            {
                sfw -= ScrollSize;
            }

            if (((mx > sfx && mx < sfx + sfw && my > sfy && my < sfy + sfh) || WasScrollingHorz) && !WasScrollingVert)
            {
                WasScrollingHorz = true;
                int maxwindowx = GetMaxWindowX();
                int sbarw = GetSliderBarWidth(false);
                float sliderratio = ((float)(WindowX) / (float)(maxwindowx));
                float sliderx = sfx + (sliderratio * (sfw - sbarw));
                if (IsDragScrolling || (mx >= sliderx && mx <= sliderx + sbarw))
                {
                    WindowX += mx - input.LastState.MousePos.X;
                    IsDragScrolling = true;
                }
                else
                {
                    WindowX = (int)(Math.Round((1.0f - (((sfx + sfw) - mx) / (sfw))) * (maxwindowx)));
                }

                changed = true;
                if (WindowX < 0)
                {
                    WindowX = 0;
                }
                if (WindowX > maxwindowx)
                {
                    WindowX = maxwindowx;
                }
            }
            return changed;
        }

        private int GetMaxWindowY()
        {
            int linespacing = SFont.LineSpacing;
            int linesPer = Height / linespacing;
            return linespacing * (RenderedTextLines.Count() - ((2 * linesPer) / 3));
        }

        private string GetLongestLine()
        {
            string longest = string.Empty;
            for (int i = 0; i < RenderedTextLines.Count(); i++)
                if (RenderedTextLines.GetLine(i).Text.Length > longest.Length)
                    longest = RenderedTextLines.GetLine(i).Text;
            return longest;
        }

        private int GetLongestLineIndex()
        {
            string longest = string.Empty;
            int lindex = 0;
            for (int i = 0; i < RenderedTextLines.Count(); i++)
            {
                if (RenderedTextLines.GetLine(i).Text.Length > longest.Length)
                {
                    longest = RenderedTextLines.GetLine(i).Text;
                    lindex = i;
                }
            }
            return lindex;
        }

        private int DeterminePositionWithinLine(RenderedText rendered, int x, TextStyles styles, int? offset = null)
        {
            string text = rendered.Text;
            // assumes x=0 is start of line
            // first check if we're out of bounds
            if (x <= 0)
                return 0;
            int maxlen = TextHelper.GetWidthMultiStyles(SFont, rendered, styles, offset);
            if (x >= maxlen)
                return text.Length;
            if (offset == null)
                offset = 0;
            // try a binary search to discover the closest position
            int min = 0;
            int max = text.Length;
            while (true)
            {
                if (min == max - 1)
                {
                    // special case: we're down to two options
                    int lenmin = TextHelper.GetWidthMultiStyles(SFont, rendered, styles, offset, min);
                    int lenmax = TextHelper.GetWidthMultiStyles(SFont, rendered, styles, offset, max);
                    int mindist = x - lenmin;
                    int maxdist = lenmax - x;
                    if (mindist < maxdist)
                        return min;
                    else
                        return max;
                }

                int pos = min + ((max - min) / 2);
                int len = TextHelper.GetWidthMultiStyles(SFont, rendered, styles, offset, pos);
                if (len > x)
                {
                    max = pos < max ? pos : max;
                }
                else if (len < x)
                {
                    min = pos > min ? pos : min;
                }
                else
                {
                    return pos; // odd case where len == x
                }
                if (min == max)
                    return min;
            }
        }

        private int GetMaxWindowX()
        {
            int lindex = GetLongestLineIndex();
            RenderedText longest = RenderedTextLines.GetLine(lindex);
            int len = TextHelper.GetWidthMultiStyles(SFont, longest, ComputedStyles[lindex]) + ScrollSize; // add some buffer room
            len -= Width;
            if (len < 0) len = 0;
            return len;
        }

        private int GetSliderBarWidth(bool vert)
        {
            // this is just the size of the bar in the slider that the user drags
            int minsize = BorderWidth * 3;
            int w = vert ? Height : Width;
            int sw = w + (vert ? GetMaxWindowY() : GetMaxWindowX());
            int size = (int)(((float)w / (float)sw) * (((float)w * 3.0f) / 4.0f));

            return size > minsize ? size : minsize;
        }

        private bool ScrollVertVisible()
        {
            return (GetMaxWindowY() > Height / 3);
        }

        private bool ScrollVertVisible(int maxwindowy)
        {
            return (maxwindowy > Height / 3);
        }

        private bool ScrollHorzVisible(int maxwindowx)
        {
            return (maxwindowx > 0);
        }

        private bool ScrollHorzVisible()
        {
            return (GetMaxWindowX() > 0);
        }

        public void ClearText(float elapsedms, InputManager input)
        {
            CaretPosition = 0;
            CaretLine = 0;
            Text = string.Empty;
            TextChanged(elapsedms, input);
        }

        public void SetText(string newtext, float elapsedms, InputManager input)
        {
            Highlighting = false; // breaks highlighting
            Text = newtext;
            if (!MultiLine)
            {
                if (CaretPosition > Text.Length)
                    CaretPosition = Text.Length;
            }
            else
            {
                if (CaretLine >= RenderedTextLines.Count())
                    CaretLine = RenderedTextLines.Count() - 1;
                if (CaretPosition > RenderedTextLines.GetLine(CaretLine).Text.Length)
                    CaretPosition = RenderedTextLines.GetLine(CaretLine).Text.Length;
                int maxx = GetMaxWindowX();
                if (WindowX > maxx)
                    WindowX = maxx;
                int maxy = GetMaxWindowY();
                if (WindowY > maxy)
                    WindowY = maxy;
            }
            TextChanged(elapsedms, input);
        }

        public void SetTextStyler(ITextStyler styler)
        {
            TextStyler = styler;
            ComputeStyles(); // recompute styles with the new styler
        }

        private TextStyles DefaultTextStyles = new TextStyles(new List<TextSettings>() { null }, new List<int>() { 0 }, new List<int>() { 0 }, new List<RelaFont>() { null });
        private void ComputeStyles()
        {
            if (SFont == null)
                return; // not inited
            if (SkipNextCompute)
            {
                SkipNextCompute = false;
                return;
            }
            ComputedStyles.Clear();
            if (MultiLine)
            {
                if (TextStyler != null)
                    TextStyler.PreStyling();
                for (int i = 0; i < RenderedTextLines.Count(); i++)
                {
                    // if we have no styler, use a default styles set
                    if (TextStyler == null || RenderedTextLines.GetLine(i).Text.Length <= 0)
                    {
                        DefaultTextStyles.Styles[0] = FontSettings;
                        DefaultTextStyles.Fonts[0] = SFont;
                        ComputedStyles.Add(DefaultTextStyles);
                    }
                    else
                    {
                        ComputedStyles.Add(TextStyler.GetTextStyles(SFont, RenderedTextLines.GetLine(i).Text, FontSettings, i));
                    }
                }
                if (TextStyler != null)
                    TextStyler.PostStyling();
            }
            else
            {
                if (TextStyler == null)
                {
                    DefaultTextStyles.Styles[0] = FontSettings;
                    DefaultTextStyles.Fonts[0] = SFont;
                    ComputedStyles.Add(DefaultTextStyles);
                }
                else
                {
                    TextStyler.PreStyling();
                    ComputedStyles.Add(TextStyler.GetTextStyles(SFont, Text, FontSettings, 0));
                    TextStyler.PostStyling();
                }
            }
        }

        private void OrderHighlights(out int h1line, out int h2line, out int h1pos, out int h2pos)
        {
            // user could drag backwards, so we need to sort the highlight positions
            if (HighlightStartLine < HighlightEndLine || (HighlightStartLine == HighlightEndLine && HighlightStartPos <= HighlightEndPos))
            {
                h1line = HighlightStartLine;
                h2line = HighlightEndLine;
                h1pos = HighlightStartPos;
                h2pos = HighlightEndPos;
            }
            else
            {
                h2line = HighlightStartLine;
                h1line = HighlightEndLine;
                h2pos = HighlightStartPos;
                h1pos = HighlightEndPos;
            }
        }

        public void TakeFocus(bool selectAll)
        {
            GetRoot().System.ProposedFocus = this;
            ForceHighlightAll = selectAll;
        }

        // events
        #region events
        // enter press event
        public EventHandler EventEnterPressedHandler;
        public class EventEnterPressedHandlerArgs : EventArgs
        {
            public float ElapsedMS;
            public InputManager Input;
        }
        public event EventHandler EventEnterPressed
        {
            add
            {
                lock (EventLock)
                {
                    EventEnterPressedHandler += value;
                }
            }
            remove
            {
                lock (EventLock)
                {
                    EventEnterPressedHandler -= value;
                }
            }
        }

        public void EnterPressed(float elapsedms, InputManager input)
        {
            SelfEnterPressed(elapsedms, input);
            OnEventEnterPressed(new EventEnterPressedHandlerArgs()
            {
                ElapsedMS = elapsedms,
                Input = input,
            });
        }

        protected virtual void SelfEnterPressed(float elapsedms, InputManager input)
        {

        }

        protected virtual void OnEventEnterPressed(EventArgs e)
        {
            EventHandler handler;
            lock (EventLock)
            {
                handler = EventEnterPressedHandler;
            }
            if (handler != null)
            {
                handler(this, e);
            }
        }

        // text changed event
        public EventHandler EventTextChangedHandler;
        public class EventTextChangedHandlerArgs : EventArgs
        {
            public float ElapsedMS;
            public InputManager Input;
        }
        public event EventHandler EventTextChanged
        {
            add
            {
                lock (EventLock)
                {
                    EventTextChangedHandler += value;
                }
            }
            remove
            {
                lock (EventLock)
                {
                    EventTextChangedHandler -= value;
                }
            }
        }

        public void TextChanged(float elapsedms, InputManager input)
        {
            SelfTextChanged(elapsedms, input);
            ComputeStyles(); // recompute styles whenever the text changes
            OnEventTextChanged(new EventTextChangedHandlerArgs()
            {
                ElapsedMS = elapsedms,
                Input = input,
            });
        }

        protected virtual void SelfTextChanged(float elapsedms, InputManager input)
        {

        }

        protected virtual void OnEventTextChanged(EventArgs e)
        {
            EventHandler handler;
            lock (EventLock)
            {
                handler = EventTextChangedHandler;
            }
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion events
    }
}
