using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Input
{
    public class InputState
    {
        public KeyboardState Keys;
        public MouseState Mouse;
        public Point MousePos;

        public Keys[] PressedKeys = new Keys[8];
        public float[] PressedKeysTime = new float[8];
        public int[] PressedKeysRepeats = new int[8];
        public int PressedKeysCount = 0;

        public Keys[] FreshKeys = new Keys[8];
        public int FreshKeysCount = 0;
        
        public InputState()
        {
            Keys = Keyboard.GetState();
            Mouse = Microsoft.Xna.Framework.Input.Mouse.GetState();
            MousePos = Mouse.Position;
        }

        public void CopyFrom(InputState state)
        {
            Keys = state.Keys;
            Mouse = state.Mouse;
            MousePos = state.MousePos;

            if (PressedKeys.Length < state.PressedKeysCount)
                PressedKeys = new Keys[state.PressedKeysCount];
            if (PressedKeysTime.Length < state.PressedKeysCount)
                PressedKeysTime = new float[state.PressedKeysCount];
            if (PressedKeysRepeats.Length < state.PressedKeysCount)
                PressedKeysRepeats = new int[state.PressedKeysCount];
            if (FreshKeys.Length < state.FreshKeysCount)
                FreshKeys = new Keys[state.FreshKeysCount];

            PressedKeysCount = state.PressedKeysCount;
            for (int i = 0; i < PressedKeysCount; i++)
            {
                PressedKeys[i] = state.PressedKeys[i];
                PressedKeysTime[i] = state.PressedKeysTime[i];
                PressedKeysRepeats[i] = state.PressedKeysRepeats[i];
            }

            FreshKeysCount = state.FreshKeysCount;
            for (int i = 0; i < FreshKeysCount; i++)
            {
                FreshKeys[i] = state.FreshKeys[i];
            }
        }

        public void Load(InputState last, KeyboardState keys, MouseState mouse, float elapsedms, float repeatdelay, float repeatinterval)
        {
            last.CopyFrom(this);

            Keys = keys;
            Mouse = mouse;
            MousePos = mouse.Position;

            PressedKeysCount = Keys.GetPressedKeyCount();
            if (PressedKeys.Length < PressedKeysCount)
            {
                PressedKeys = new Keys[PressedKeysCount];
            }
            Keys.GetPressedKeys(PressedKeys);

            FreshKeysCount = 0;

            Keys currentKey;
            float time = 0;
            int repeats = 0;
            for (int i = 0; i < PressedKeysCount; i++)
            {
                currentKey = PressedKeys[i];

                // check if this key was pressed in the last state
                time = 0;
                for (int o = 0; o < last.PressedKeysCount; o++)
                {
                    if (last.PressedKeys[o] == currentKey)
                    {
                        time = last.PressedKeysTime[o] + elapsedms;
                        repeats = last.PressedKeysRepeats[o];
                        break;
                    }
                }
                PressedKeysTime[i] = time;

                if (time == 0)
                {
                    while (FreshKeys.Length < FreshKeysCount)
                    {
                        Keys[] nfks = new Keys[FreshKeys.Length * 2];
                        for (int o = 0; o < FreshKeys.Length; o++)
                            nfks[o] = FreshKeys[o];
                        FreshKeys = nfks;
                    }
                    FreshKeys[FreshKeysCount] = currentKey;
                    FreshKeysCount++;

                    PressedKeysRepeats[i] = 0;
                }
                else
                {
                    if (time >= repeatdelay + repeatinterval * repeats)
                    {
                        while (FreshKeys.Length < FreshKeysCount)
                        {
                            Keys[] nfks = new Keys[FreshKeys.Length * 2];
                            for (int o = 0; o < FreshKeys.Length; o++)
                                nfks[o] = FreshKeys[o];
                            FreshKeys = nfks;
                        }
                        FreshKeys[FreshKeysCount] = currentKey;
                        FreshKeysCount++;

                        repeats++;
                    }

                    PressedKeysRepeats[i] = repeats;
                }
            }
        }

        public bool IsPressed(Keys key)
        {
            for (int i = 0; i < PressedKeysCount; i++)
                if (PressedKeys[i] == key)
                    return true;
            return false;
        }

        public bool IsFresh(Keys key)
        {
            for (int i = 0; i < FreshKeysCount; i++)
                if (FreshKeys[i] == key)
                    return true;
            return false;
        }
    }
}