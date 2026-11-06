using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RelaUI.Input;
using RelaUI.Text;
using System;

namespace RelaUI.Components
{
    public class UISlider : UIComponent
    {
        public int Width = 200;
        public int Height = 40;

        public int MinValue = 0;
        public int MaxValue = 100;
        public int Value = 0;
        public int Step = 1;

        public bool ShowTicks = true;
        public bool ShowValueText = true;

        // slider bar visuals
        public int BarHeight = 6;
        public int HandleWidth = 14;
        public int HandleHeight = 24;
        public int TickHeight = 14;
        public int ValueTextWidth = 48;

        public Color TrackColor = Color.Gray;
        public Color FillColor = Color.White;
        public Color TickColor = Color.Gray;
        public Color BorderColor = Color.DarkGray;
        public Color ValueTextColor = Color.White;

        public event Action<int> ValueChanged;

        private bool Dragging = false;
        private float LastDx = 0;
        private float LastDy = 0;

        private RelaFont ValueFont;
        private TextSettings ValueFontSettings = new TextSettings();

        public UISlider(float x, float y, int width, int height)
        {
            this.x = x;
            this.y = y;
            Width = width;
            Height = height;
        }

        protected override void SelfInit()
        {
            ValueFontSettings.FontSize = 18;
            ValueFontSettings.Color = Style.ForegroundColor;
            ValueFont = ValueFontSettings.Init(Style);

            TrackColor = Style.SecondaryBackgroundColor;
            FillColor = Style.ForegroundColor;
            TickColor = Style.SecondaryTextColor;
            BorderColor = Style.BackgroundAccent;
            ValueTextColor = Style.ForegroundColor;
        }

        public void SetValue(int value, bool fireEvent = false)
        {
            if (value < MinValue)
                value = MinValue;
            if (value > MaxValue)
                value = MaxValue;
            if (value == Value)
                return;

            Value = value;
            if (fireEvent)
                ValueChanged?.Invoke(Value);
        }

        public override int GetWidth()
        {
            return Width;
        }

        public override int GetHeight()
        {
            return Height;
        }

        public override bool PreventChildFocus(float dx, float dy, InputManager input)
        {
            return true;
        }

        protected override void SelfGetFocus(float elapsedms, InputManager input)
        {
            Dragging = Hovering(LastDx, LastDy, input) && input.MouseDown(eMouseButtons.Left);
            if (Dragging)
                UpdateFromMouse(input);

            input.Cursor.TrySet(Dragging ? eCursorState.DOWN : eCursorState.CLICKABLE);
        }

        protected override void SelfFocusLost(float elapsedms, InputManager input)
        {
            Dragging = false;
        }

        public override void SelfFocusedInput(float elapsedms, InputManager input)
        {
            if (input.MouseDown(eMouseButtons.Left))
            {
                Dragging = true;
                UpdateFromMouse(input);
            }
            else
            {
                Dragging = false;
            }

            if (input.IsKeyFresh(Keys.Left) || input.IsKeyFresh(Keys.A))
            {
                input.CaptureInput(Keys.Left);
                input.CaptureInput(Keys.A);
                SetValue(Value - Step, true);
            }
            else if (input.IsKeyFresh(Keys.Right) || input.IsKeyFresh(Keys.D))
            {
                input.CaptureInput(Keys.Right);
                input.CaptureInput(Keys.D);
                SetValue(Value + Step, true);
            }
        }

        public int GetBarWidth()
        {
            int w = Width;
            if (ShowValueText)
                w -= ValueTextWidth;
            if (w < HandleWidth + 1)
                w = HandleWidth + 1;
            return w;
        }

        protected override void SelfRender(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input, float dx, float dy)
        {
            LastDx = dx;
            LastDy = dy;

            if (Hovering((int)dx, (int)dy, input))
                input.Cursor.TrySet(eCursorState.CLICKABLE);

            int barWidth = GetBarWidth();
            int trackLeft = (int)dx + HandleWidth / 2;
            int trackRight = (int)dx + barWidth - HandleWidth / 2;
            int trackWidth = trackRight - trackLeft;
            if (trackWidth < 1)
                trackWidth = 1;
            int trackY = (int)dy + Height / 2 - BarHeight / 2;

            float ratio = GetRatio();
            int handleCenter = trackLeft + (int)(trackWidth * ratio);
            int filledWidth = handleCenter - trackLeft;
            if (filledWidth < 0)
                filledWidth = 0;

            // draw the track
            Draw.DrawRectangleHandle(sb, trackLeft, trackY, trackWidth, BarHeight, TrackColor);
            Draw.DrawRectangleHandleOutline(sb, trackLeft, trackY, trackWidth, BarHeight, BorderColor, 1);

            // draw the filled portion of the track
            Draw.DrawRectangleHandle(sb, trackLeft, trackY, filledWidth, BarHeight, FillColor);

            // draw ticks
            if (ShowTicks)
            {
                int tickTop = trackY + BarHeight / 2 - TickHeight / 2;
                for (int i = 0; i <= 10; i++)
                {
                    int x = trackLeft + (trackWidth * i) / 10;
                    Draw.DrawRectangleHandle(sb, x, tickTop, 1, TickHeight, TickColor);
                }
            }

            // draw the handle
            int handleX = handleCenter - HandleWidth / 2;
            int handleY = (int)dy + Height / 2 - HandleHeight / 2;
            Draw.DrawRectangleHandle(sb, handleX, handleY, HandleWidth, HandleHeight, FillColor);
            Draw.DrawRectangleHandleOutline(sb, handleX, handleY, HandleWidth, HandleHeight, BorderColor, 1);

            if (ShowValueText && ValueFont != null)
            {
                string text = Value.ToString();
                int textX = (int)dx + barWidth + 8;
                int textY = (int)dy + Height / 2 - ValueFont.LineSpacing / 2;
                ValueFontSettings.Color = ValueTextColor;
                Draw.DrawText(g, sb, ValueFont, textX, textY, text, ValueFontSettings);
            }
        }

        private float GetRatio()
        {
            if (MaxValue <= MinValue)
                return 0f;

            float ratio = (float)(Value - MinValue) / (float)(MaxValue - MinValue);
            if (ratio < 0f)
                return 0f;
            if (ratio > 1f)
                return 1f;
            return ratio;
        }

        private void UpdateFromMouse(InputManager input)
        {
            int barWidth = GetBarWidth();
            int trackLeft = (int)LastDx + HandleWidth / 2;
            int trackWidth = barWidth - HandleWidth;
            if (trackWidth < 1)
                trackWidth = 1;
            int mouseX = input.State.MousePos.X;

            float ratio = (float)(mouseX - trackLeft) / (float)trackWidth;
            if (ratio < 0f)
                ratio = 0f;
            if (ratio > 1f)
                ratio = 1f;

            int range = MaxValue - MinValue;
            int value = MinValue + (int)Math.Round(ratio * range);
            SetValue(value, true);
        }
    }
}
