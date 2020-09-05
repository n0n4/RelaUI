using RelaUI.Components;
using RelaUI.Input;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Dialogs
{
    public class DialogBase : IDialog
    {
        protected readonly object EventLock = new object();

        protected bool IsPopup = false;
        protected UISystem ParentSystem = null;
        protected UIComponent ParentComponent = null;

        public UIPanel Panel = null;

        public void PrepEmbed(UIComponent component)
        {
            ParentComponent = component;

            Construct();
        }

        public void Embed(UIComponent component)
        {
            ParentComponent = component;

            Construct();

            component.Add(Panel);
            Panel.Init();
        }

        public void EmbedAuto(UIComponent component)
        {
            ParentComponent = component;

            Construct();

            component.AddAuto(Panel);
            Panel.Init();
        }

        public void PrepPopup(UISystem system)
        {
            IsPopup = true;
            ParentSystem = system;

            Construct();
        }

        public void Popup(UISystem system)
        {
            IsPopup = true;
            ParentSystem = system;

            Construct();

            system.Add(Panel);
            Panel.Init();

            SelfPopup();
        }

        public void Close(float elapsedms, InputManager input)
        {
            // tell our system to remove us
            if (ParentSystem != null)
                ParentSystem.Remove(Panel);
            if (ParentComponent != null)
                ParentComponent.Remove(Panel);
            
            OnEventClosed(new EventClosedHandlerArgs()
            {
                ElapsedMS = elapsedms,
                Input = input,
            });
        }

        public void Dispose()
        {
            Panel.Dispose();
        }

        protected virtual void Construct()
        {
            // overwrite this with dialog construction logic
            // make sure to instantiate .Panel
        }

        protected virtual void SelfPopup()
        {

        }

        protected Tuple<float, float> GetCenterOfParent()
        {
            // for use, e.g. if this is a Popup
            if (ParentComponent == null && ParentSystem == null)
                throw new Exception("UIDialog was not prepped");
            if (!IsPopup)
                return new Tuple<float, float>(ParentComponent.GetWidth() / 2, ParentComponent.GetHeight() / 2);
            return new Tuple<float, float>(ParentSystem.GraphicsDevice.PresentationParameters.BackBufferWidth / 2,
                ParentSystem.GraphicsDevice.PresentationParameters.BackBufferHeight / 2);
        }

        // close event
        public EventHandler EventClosedHandler;
        public class EventClosedHandlerArgs : EventArgs
        {
            public float ElapsedMS;
            public InputManager Input;
        }
        public event EventHandler EventClosed
        {
            add
            {
                lock (EventLock)
                {
                    EventClosedHandler += value;
                }
            }
            remove
            {
                lock (EventLock)
                {
                    EventClosedHandler -= value;
                }
            }
        }

        protected virtual void OnEventClosed(EventArgs e)
        {
            EventHandler handler;
            lock (EventLock)
            {
                handler = EventClosedHandler;
            }
            if (handler != null)
            {
                handler(this, e);
            }
        }
    }
}
