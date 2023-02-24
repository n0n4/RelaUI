using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RelaUI.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Components
{
    public class UINumberField : UIComponent
    {
        private int WidthField = 100;
        public int Width
        {
            get { return WidthField; }
            set
            {
                WidthField = value;
                
                Resize();
            }
        }
        private int HeightField = 100;
        public int Height
        {
            get { return HeightField; }
            set
            {
                HeightField = value;

                Resize();
            }
        }
        private int ButtonWidthField = 30;
        public int ButtonWidth
        {
            get { return ButtonWidthField; }
            set
            {
                ButtonWidthField = value;

                Resize();
            }
        }

        public bool IsInt = false;
        public int IntIncrement = 1;
        public double DoubleIncrement = 1.0;
        public int? IntMax = null;
        public int? IntMin = null;
        public double? DoubleMax = null;
        public double? DoubleMin = null;

        public UITextField TextField;
        public UIButton ButtonUp;
        public UIButton ButtonDown;

        private string LastValue = string.Empty;
        private bool IgnoreNext = false;

        private double CheckValidTimer = 0;
        public double CheckValidTimerMax = 0.5; // seconds

        public UINumberField(float x, float y, int width, int height, int buttonWidth, string initialValue, bool isInt,
            string font = "", int? fontsize = null, bool useButtons = true)
        {
            this.x = x;
            this.y = y;
            // set by field to bypass resize since components are not built yet
            WidthField = width;
            HeightField = height;
            ButtonWidthField = buttonWidth;
            IsInt = isInt;
            LastValue = initialValue;

            TextField = new UITextField(0, 0, Width - ButtonWidth, Height, LastValue, font, fontsize, false);
            Add(TextField);
            TextField.EventTextChanged += (sender, args) =>
            {
                if (IgnoreNext)
                {
                    IgnoreNext = false;
                    return;
                }
                CheckValidTimer = CheckValidTimerMax;
            };
            /*TextField.EventTextChanged += (sender, args) =>
            {
                if(IgnoreNext)
                {
                    IgnoreNext = false;
                    return;
                }

                var eargs = args as UITextField.EventTextChangedHandlerArgs;
                if (IsInt)
                {
                    if(!int.TryParse(TextField.Text, out int intval))
                    {
                        IgnoreNext = true;
                        TextField.SetText(LastValue, eargs.Time, eargs.Input);
                    }
                    else
                    {
                        Set(intval);
                    }
                }
                else
                {
                    if(!double.TryParse(TextField.Text, out double doubleval))
                    {
                        IgnoreNext = true;
                        TextField.SetText(LastValue, eargs.Time, eargs.Input);
                    }
                    else
                    {
                        Set(doubleval);
                    }
                }
            };*/

            if (useButtons)
            {
                ButtonUp = new UIButton(Width - buttonWidth, 0, ButtonWidth, Height / 2, "^", font, fontsize);
                Add(ButtonUp);
                ButtonUp.EventFocused += (sender, args) =>
                {
                    var eargs = args as UIComponent.EventFocusedHandlerArgs;
                    if (IsInt)
                    {
                        Add(IntIncrement, eargs.ElapsedMS, eargs.Input);
                    }
                    else
                    {
                        Add(DoubleIncrement, eargs.ElapsedMS, eargs.Input);
                    }
                };

                ButtonDown = new UIButton(Width - buttonWidth, Height / 2, ButtonWidth, Height / 2, "v", font, fontsize);
                Add(ButtonDown);
                ButtonDown.EventFocused += (sender, args) =>
                {
                    var eargs = args as UIComponent.EventFocusedHandlerArgs;
                    if (IsInt)
                    {
                        Add(-IntIncrement, eargs.ElapsedMS, eargs.Input);
                    }
                    else
                    {
                        Add(-DoubleIncrement, eargs.ElapsedMS, eargs.Input);
                    }
                };
            }
        }

        public void Resize()
        {
            TextField.Width = Width - ButtonWidth;
            TextField.Height = Height;

            if (ButtonUp != null) 
            {
                ButtonUp.x = Width - ButtonWidth;
                ButtonUp.y = 0;
                ButtonUp.Width = ButtonWidth;
                ButtonUp.Height = Height / 2;
            }

            if (ButtonDown != null)
            {
                ButtonDown.x = Width - ButtonWidth;
                ButtonDown.y = Height / 2;
                ButtonDown.Width = ButtonWidth;
                ButtonDown.Height = Height / 2;
            }
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
            if (CheckValidTimer > 0)
            {
                CheckValidTimer -= elapsedms/1000.0f;
                if (CheckValidTimer <= 0)
                {
                    if (IsInt)
                    {
                        if (!int.TryParse(TextField.Text, out int intval))
                        {
                            IgnoreNext = true;
                            TextField.SetText(LastValue, elapsedms, input);
                        }
                        else
                        {
                            Set(intval, elapsedms, input);
                        }
                    }
                    else
                    {
                        if (!double.TryParse(TextField.Text, out double doubleval))
                        {
                            IgnoreNext = true;
                            TextField.SetText(LastValue, elapsedms, input);
                        }
                        else
                        {
                            Set(doubleval, elapsedms, input);
                        }
                    }
                }
            }
        }

        public override void SelfParentInput(float elapsedms, InputManager input)
        {
            //accept scroll wheel and up/down arrow input
            if (input.IsKeyFresh(Keys.Up))
            {
                input.CaptureInput(Keys.Up);
                if (IsInt)
                    Add(IntIncrement, elapsedms, input);
                else
                    Add(DoubleIncrement, elapsedms, input);
            }
            if (input.IsKeyFresh(Keys.Down))
            {
                input.CaptureInput(Keys.Down);
                if (IsInt)
                    Add(-IntIncrement, elapsedms, input);
                else
                    Add(-DoubleIncrement, elapsedms, input);
            }

            int scrolldiff = input.FreshMouseScrollDifference();
            if (scrolldiff > 0)
            {
                if (IsInt)
                    Add(IntIncrement * scrolldiff / 2, elapsedms, input);
                else
                    Add(DoubleIncrement * scrolldiff / 2, elapsedms, input);
                input.CaptureMouseScroll();
            }
            else if (scrolldiff < 0)
            {
                if (IsInt)
                    Add(IntIncrement * scrolldiff / 2, elapsedms, input);
                else
                    Add(DoubleIncrement * scrolldiff / 2, elapsedms, input);
                input.CaptureMouseScroll();
            }
        }

        public void Set(int i, float elapsedms, InputManager input)
        {
            if (!IsInt)
            {
                Set((double)i, elapsedms, input);
                return;
            }

            if (IntMax.HasValue && i > IntMax.Value)
                i = IntMax.Value;
            if (IntMin.HasValue && i < IntMin.Value)
                i = IntMin.Value;

            IgnoreNext = true;
            LastValue = i.ToString();
            TextField.SetText(i.ToString(), elapsedms, input);
            ValueChanged(elapsedms, input);
        }

        public void Set(double d, float elapsedms, InputManager input)
        {
            if (IsInt)
            {
                Set((int)d, elapsedms, input);
                return;
            }

            if (DoubleMax.HasValue && d > DoubleMax.Value)
                d = DoubleMax.Value;
            if (DoubleMin.HasValue && d < DoubleMin.Value)
                d = DoubleMin.Value;

            IgnoreNext = true;
            LastValue = d.ToString();
            TextField.SetText(d.ToString(), elapsedms, null);
            ValueChanged(elapsedms, input);
        }

        public void Add(int i, float elapsedms, InputManager input)
        {
            if (!IsInt)
            {
                Add((double)i, elapsedms, input);
                return;
            }

            if (!int.TryParse(TextField.Text, out int intval))
            {
                return; // how did we get here?
            }

            Set(intval + i, elapsedms, input);
        }

        public void Add(double d, float elapsedms, InputManager input)
        {
            if (IsInt)
            {
                Add((int)d, elapsedms, input);
                return;
            }

            if (!double.TryParse(TextField.Text, out double doubleval))
            {
                return; // how did we get here?
            }

            Set(doubleval + d, elapsedms, input);
        }

        public int AsInt()
        {
            if (!int.TryParse(TextField.Text, out int intval))
                return 0;
            return intval;
        }

        public double AsDouble()
        {
            if (!double.TryParse(TextField.Text, out double doubleval))
                return 0;
            return doubleval;
        }

        // events
        #region events
        // value changed event
        public EventHandler EventValueChangedHandler;
        public class EventValueChangedHandlerArgs : EventArgs
        {
            public float ElapsedMS;
            public InputManager Input;
        }
        public event EventHandler EventValueChanged
        {
            add
            {
                lock (EventLock)
                {
                    EventValueChangedHandler += value;
                }
            }
            remove
            {
                lock (EventLock)
                {
                    EventValueChangedHandler -= value;
                }
            }
        }

        public void ValueChanged(float elapsedms, InputManager input)
        {
            SelfValueChanged(elapsedms, input);
            OnEventValueChanged(new EventValueChangedHandlerArgs()
            {
                ElapsedMS = elapsedms,
                Input = input,
            });
        }

        protected virtual void SelfValueChanged(float elapsedms, InputManager input)
        {

        }

        protected virtual void OnEventValueChanged(EventArgs e)
        {
            EventHandler handler;
            lock (EventLock)
            {
                handler = EventValueChangedHandler;
            }
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion events
    }
}
