using RelaUI.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Components
{
    public class UICheckbox : UIButton
    {
        public bool Checked { get; private set; } = false;

        public UICheckbox(float x, float y, int w, int h, bool defaultValue = false, string font = "", int? fontsize = null,
            bool autowidth = false, bool autoheight = false)
            : base(x, y, w, h, "X", font, fontsize, autowidth, autoheight)
        {
            EventFocused += (sender, args) =>
            {
                var eargs = args as UIComponent.EventFocusedHandlerArgs;
                SetChecked(!Checked, eargs.ElapsedMS, eargs.Input);
            };

            Checked = defaultValue;
            EventPostInit += (sender, args) =>
            {
                SetChecked(Checked, 0, null);
            };

        }

        public void SetChecked(bool value, float elapsedms, InputManager input)
        {
            Checked = value;
            StayPressed = value;

            if (value)
            {
                Text = "X";
                ProcessText();
            }
            else
            {
                Text = " ";
                ProcessText();
            }

            CheckChanged(elapsedms, input);
        }

        // events
        #region events
        // check changed event
        public EventHandler EventCheckChangedHandler;
        public class EventCheckChangedHandlerArgs : EventArgs
        {
            public bool Checked;
            public float ElapsedMS;
            public InputManager Input;
        }
        public event EventHandler EventCheckChanged
        {
            add
            {
                lock (EventLock)
                {
                    EventCheckChangedHandler += value;
                }
            }
            remove
            {
                lock (EventLock)
                {
                    EventCheckChangedHandler -= value;
                }
            }
        }

        public void CheckChanged(float elapsedms, InputManager input)
        {
            SelfCheckChanged(elapsedms, input);
            OnEventCheckChanged(new EventCheckChangedHandlerArgs()
            {
                Checked = Checked,
                ElapsedMS = elapsedms,
                Input = input,
            });
        }

        protected virtual void SelfCheckChanged(float elapsedms, InputManager input)
        {

        }

        protected virtual void OnEventCheckChanged(EventArgs e)
        {
            EventHandler handler;
            lock (EventLock)
            {
                handler = EventCheckChangedHandler;
            }
            if (handler != null)
            {
                handler(this, e);
            }
        }
        #endregion
    }
}
