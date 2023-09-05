using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RelaUI.Input.Clipboards;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Input
{
    public enum eMouseButtons
    {
        Left,
        Right
    }

    public class InputManager
    {
        public InputState State = new InputState();
        public InputState LastState = new InputState();

        public float KeyRepeatTimeMS = 500;
        public float KeyRepeatIntervalMS = 50;
        public float KeyStickingDelayTimeMS = 100;

        public CursorManager Cursor;

        public Keys[] InputCaptured = new Keys[8];
        public int InputCapturedCount = 0;
        public Keys[] InputUncaptured = new Keys[8];
        public int InputUncapturedCount = 0;

        public bool MouseScrollCaptured = false;

        public InputManager(CursorManager cursorManager)
        {
            Cursor = cursorManager;
        }

        public void Render(GraphicsDevice g, SpriteBatch sb)
        {
            if (MouseDown(eMouseButtons.Left))
            {
                Cursor.TrySet(eCursorState.DOWN);
            }
            Cursor.Render(g, sb, State.MousePos.X, State.MousePos.Y);
        }

        public void Update(float elapsedms)
        {
            MouseScrollCaptured = false;
            InputCapturedCount = 0;
            State.Load(LastState, Keyboard.GetState(), Mouse.GetState(), elapsedms, KeyRepeatTimeMS, KeyRepeatIntervalMS);
        }

        // to be called after all components have updated
        public void PostUpdate()
        {
            // find uncaptured keys
            for (int i = 0; i < State.PressedKeysCount; i++)
            {
                Keys k = State.PressedKeys[i];
                if (!IsKeyUncaptured(k) && IsKeyFresh(k)) // !LastState.Keys.IsKeyUp(k) && 
                {
                    if (InputUncaptured.Length <= InputUncapturedCount)
                    {
                        Keys[] nius = new Keys[InputUncaptured.Length * 2];
                        for (int o = 0; o < InputUncapturedCount; o++)
                            nius[o] = InputUncaptured[o];
                        InputUncaptured = nius;
                    }

                    InputUncaptured[InputUncapturedCount] = k;
                    InputUncapturedCount++;
                }
            }

            // clean out uncaptured keys that are no longer being pressed
            for (int i = 0; i < InputUncapturedCount; i++)
            {
                Keys k = InputUncaptured[i];
                if (!State.IsPressed(k) || IsKeyCaptured(k))
                {
                    InputUncapturedCount--;
                    InputUncaptured[i] = InputUncaptured[InputUncapturedCount];

                    i--;
                }
            }
        }

        public bool KeyDown(Keys k)
        {
            return State.Keys.IsKeyDown(k);
        }

        // Pressed vs down: pressed is only true on the frame the key is pressed
        public bool KeyPressed(Keys k)
        {
            return (State.Keys.IsKeyDown(k) && LastState.Keys.IsKeyUp(k));
        }

        public bool KeyReleased(Keys k)
        {
            return (State.Keys.IsKeyUp(k) && LastState.Keys.IsKeyDown(k));
        }

        // fresh keys are all keys that have just been pressed, or that have been pressed
        // long enough to trigger the repeat 

        // capturing
        public bool IsKeyUncaptured(Keys k)
        {
            for (int i = 0; i < InputUncapturedCount; i++)
                if (InputUncaptured[i] == k)
                    return true;
            return false;
        }

        public bool IsKeyCaptured(Keys k)
        {
            for (int i = 0; i < InputCapturedCount; i++)
                if (InputCaptured[i] == k)
                    return true;
            return false;
        }

        public bool IsKeyFresh(Keys k)
        {
            return State.IsFresh(k) && !IsKeyCaptured(k);
        }

        public bool IsAtLeastOneFresh(Keys[] ks, int keycount)
        {
            for (int i = 0; i < keycount; i++)
            {
                if (IsKeyFresh(ks[i]))
                    return true;
            }
            return false;
        }

        public bool IsAtLeastOnePressed(Keys[] ks, int keycount)
        {
            for (int i = 0; i < keycount; i++)
            {
                if (KeyPressed(ks[i]))
                    return true;
            }
            return false;
        }

        public bool IsAtLeastOneReleased(Keys[] ks, int keycount)
        {
            for (int i = 0; i < keycount; i++)
            {
                if (KeyReleased(ks[i]))
                    return true;
            }
            return false;
        }

        /*public List<Keys> FreshKeys()
        {
            return State.FreshKeys.Where(x => !InputCaptured.Contains(x)).ToList();
        }*/

        /*public List<Keys> AwaitingKeys()
        {
            List<Keys> ks = new List<Keys>();
            ks.AddRange(FreshKeys());
            ks.AddRange(InputUncaptured.Where(x => !ks.Contains(x)));
            return ks;
        }*/

        public void CaptureInput(Keys k)
        {
            if (InputCaptured.Length <= InputCapturedCount)
            {
                Keys[] nics = new Keys[InputCaptured.Length * 2];
                for (int o = 0; o < InputCapturedCount; o++)
                    nics[o] = InputCaptured[o];
                InputCaptured = nics;
            }

            InputCaptured[InputCapturedCount] = k;
            InputCapturedCount++;
        }


        public enum eWritingOrder
        {
            NONE,
            ADD,
            BACKSPACE,
            DELETE,
            LEFT,
            UP,
            DOWN,
            RIGHT,
            ADDTOEND
        }
        public struct WritingOrder
        {
            public string Add;
            public eWritingOrder Order;
        }
        public int FreshWriting(WritingOrder[] orders, bool capture, bool ignoreTabs = false)
        {
            // if alt is held, we bypass everything
            if (AltDown())
            {
                return 0;
            }

            // if ctrl is held, we enter a special workflow
            if (CtrlDown())
            {
                if (IsKeyFresh(Keys.V))
                {
                    // paste
                    if (capture)
                        CaptureInput(Keys.V);
                    orders[0] = new WritingOrder()
                        {
                            Add = Clipboard.Get(),
                            Order = eWritingOrder.ADD
                        };
                    return 1;
                }
                return 0;
            }
            
            bool shift = ShiftDown();
            int count = 0;
            Keys k;
            for (int i = 0; i < State.FreshKeysCount; i++)
            {
                k = State.FreshKeys[i];
                if (IsKeyCaptured(k))
                    continue;

                if (k == Keys.Back)
                {
                    orders[count] = new WritingOrder() { Order = eWritingOrder.BACKSPACE };
                    count++;
                    continue;
                }
                else if (k == Keys.Delete)
                {
                    orders[count] = new WritingOrder() { Order = eWritingOrder.DELETE };
                    count++;
                    continue;
                }
                else if (k == Keys.Left)
                {
                    orders[count] = new WritingOrder() { Order = eWritingOrder.LEFT };
                    count++;
                    continue;
                }
                else if (k == Keys.Up)
                {
                    orders[count] = new WritingOrder() { Order = eWritingOrder.UP };
                    count++;
                    continue;
                }
                else if (k == Keys.Down)
                {
                    orders[count] = new WritingOrder() { Order = eWritingOrder.DOWN };
                    count++;
                    continue;
                }
                else if (k == Keys.Right)
                {
                    orders[count] = new WritingOrder() { Order = eWritingOrder.RIGHT };
                    count++;
                    continue;
                }
                else if (k == Keys.Enter)
                {
                    orders[count] = new WritingOrder()
                    {
                        Add = "\n",
                        Order = eWritingOrder.ADD
                    };
                    count++;
                    continue;
                }
                else if (k == Keys.Tab && !ignoreTabs)
                {
                    orders[count] = new WritingOrder()
                    {
                        Add = "\t",
                        Order = eWritingOrder.ADD
                    };
                    count++;
                    continue;
                }


                char? c = TypingHelper.GetCharFromKey(k, shift);
                if (c != null)
                {
                    orders[count] = new WritingOrder()
                    {
                        Add = c.ToString(),
                        Order = eWritingOrder.ADD
                    };
                    count++;
                    if (capture)
                        CaptureInput(k);
                }
            }
            return count;
        }

        public bool ShiftDown()
        {
            return KeyDown(Keys.LeftShift) || KeyDown(Keys.RightShift);
        }

        public bool AltDown()
        {
            return KeyDown(Keys.LeftAlt) || KeyDown(Keys.RightAlt);
        }

        public bool CtrlDown()
        {
            return KeyDown(Keys.LeftControl) || KeyDown(Keys.RightControl);
        }

        public bool EnterFresh()
        {
            return IsKeyFresh(Keys.Enter);
        }

        public bool TabFresh()
        {
            return IsKeyFresh(Keys.Tab);
        }

        public bool BackspaceFresh()
        {
            return IsKeyFresh(Keys.Back);
        }

        public bool DeleteFresh()
        {
            return IsKeyFresh(Keys.Delete);
        }

        public bool LeftArrowFresh()
        {
            return IsKeyFresh(Keys.Left);
        }

        public bool RightArrowFresh()
        {
            return IsKeyFresh(Keys.Right);
        }

        public bool UpArrowFresh()
        {
            return IsKeyFresh(Keys.Up);
        }

        public bool DownArrowFresh()
        {
            return IsKeyFresh(Keys.Down);
        }

        public bool MouseDown(eMouseButtons b)
        {
            switch (b)
            {
                case eMouseButtons.Left:
                    return State.Mouse.LeftButton == ButtonState.Pressed;
                case eMouseButtons.Right:
                    return State.Mouse.RightButton == ButtonState.Pressed;
                default:
                    return false;
            }
        }

        public bool MousePressed(eMouseButtons b)
        {
            switch (b)
            {
                case eMouseButtons.Left:
                    return State.Mouse.LeftButton == ButtonState.Pressed
                        && LastState.Mouse.LeftButton == ButtonState.Released;
                case eMouseButtons.Right:
                    return State.Mouse.RightButton == ButtonState.Pressed
                        && LastState.Mouse.RightButton == ButtonState.Released;
                default:
                    return false;
            }
        }

        public bool MouseReleased(eMouseButtons b)
        {
            switch (b)
            {
                case eMouseButtons.Left:
                    return LastState.Mouse.LeftButton == ButtonState.Pressed
                        && State.Mouse.LeftButton == ButtonState.Released;
                case eMouseButtons.Right:
                    return LastState.Mouse.RightButton == ButtonState.Pressed
                        && State.Mouse.RightButton == ButtonState.Released;
                default:
                    return false;
            }
        }

        public int MouseScrollDifference()
        {
            return State.Mouse.ScrollWheelValue - LastState.Mouse.ScrollWheelValue;
        }

        public int FreshMouseScrollDifference()
        {
            if (MouseScrollCaptured)
                return 0;
            return State.Mouse.ScrollWheelValue - LastState.Mouse.ScrollWheelValue;
        }

        public void CaptureMouseScroll()
        {
            MouseScrollCaptured = true;
        }
    }
}