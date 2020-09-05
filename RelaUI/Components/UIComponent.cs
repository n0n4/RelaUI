using Microsoft.Xna.Framework.Graphics;
using RelaUI.Input;
using RelaUI.Styles;
using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Components
{
    public class UIComponent : IDisposable
    {
        public List<UIComponent> Components = new List<UIComponent>();
        public UIComponent Parent = null;
        public UISystem System = null;
        public UIStyle Style = null;

        public float x = 0;
        public float y = 0;

        public float InnerX = 0; // these values are added to components positions within this
        public float InnerY = 0;

        public bool Enabled = true; // depends on component
        public bool Visible = true;
        public bool ComponentsVisible = true;
        public bool Focused = false;
        public bool SkipBaseRender = false; // used if this component is rendered somewhere else
        public bool Initialized { private set; get; } = false;

        public bool LockFocus = false; // if this is true, no other components can get input unless they're children of this

        protected readonly object EventLock = new object();

        public void Custom(UIStyle style)
        {
            // gives this element a unique style
            Style = style;
        }

        public void Add(UIComponent child)
        {
            if (child.Parent != null)
            {
                throw new Exception("A UIComponent may not have two parents!");
            }
            child.Parent = this;
            child.System = System;
            Components.Add(child);
            child.OnAdded();
        }

        public virtual void AddAuto(UIComponent child)
        {
            // some uicomponents expose an auto ordering system
            // TODO: move that system from uipanel / uiautolist to uicomponent itself??
            Add(child);
        }

        /// <summary>
        /// TryInit is like Init, but it will silently fail instead of throwing
        /// an exception if initialization is not possible.
        /// </summary>
        public void TryInit()
        {
            if ((Parent == null || Parent.Style == null) && System == null)
            {
                return;
            }
            Init();
        }

        public void Init()
        {
            // inherit styles from Parent / System

            // TODO: seriously debating just having a return here instead of an exception
            // is there any reason why this can't just silently fail? it would let us
            // avoid needing to write "if (comp.Initialized) comp.Init()" so often
            if ((Parent == null || Parent.Style == null) && System == null)
            {
                throw new Exception("Tried to initialize component with no available parent style");
            }
            else if (Parent == null || Parent.Style == null)
            {
                Style = System.Style;
            }
            else
            {
                Style = Parent.Style; // inherit the parent's style if we don't have our own
            }

            // ------------------------------------------------------------------------
            // TODO: this system needs to be rethought
            // managing style on components is really a chore
            // and often requires using an event
            // additionally, the preinit seems useless
            // and selfinit happens after components are init'd which seems wrong to me
            // ------------------------------------------------------------------------

            // because style is always inherited, to have custom styles you should use a pre or post
            // init event to set it (pre if you want it to be inherited in turn)
            SelfPreInit();
            OnEventPreInit(new EventArgs());
            for (int i = 0; i < Components.Count; i++)
            {
                Components[i].Init();
            }
            SelfInit();
            Initialized = true;
            OnEventPostInit(new EventArgs());
        }

        protected virtual void SelfPreInit()
        {
            // overwrite this if something needs to be defined before its children can be built
        }

        protected virtual void SelfInit()
        {
            // overwrite this
        }

        public virtual void Render(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input, float dx, float dy)
        {
            BaseRender(elapsedms, g, sb, input, dx, dy);
        }

        private UIComponent BaseRenderLoopComponent;
        public void BaseRender(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input, float dx, float dy)
        {
            dx += x;
            dy += y;
            SelfRender(elapsedms, g, sb, input, dx, dy);
            if (ComponentsVisible)
            {
                for (int i = 0; i < Components.Count; i++)
                {
                    BaseRenderLoopComponent = Components[i];
                    if (BaseRenderLoopComponent.Visible && !BaseRenderLoopComponent.SkipBaseRender)
                    {
                        BaseRenderLoopComponent.Render(elapsedms, g, sb, input, dx + InnerX, dy + InnerY);
                    }
                }
            }
        }

        protected virtual void SelfRender(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input, float dx, float dy)
        {
            // overwrite this
        }

        protected virtual void SelfDispose()
        {
            // if you need to dispose of anything besides other components, you can overwrite this
        }

        // must overwrite these for auto spacing to work
        public virtual int GetWidth()
        {
            return 0;
        }

        public virtual int GetHeight()
        {
            return 0;
        }

        public bool Hovering(float dx, float dy, InputManager input)
        {
            return Enabled && Visible
                && input.State.MousePos.X > (int)(Math.Floor(dx))
                && input.State.MousePos.X < (int)(Math.Floor(dx)) + GetWidth()
                && input.State.MousePos.Y > (int)(Math.Floor(dy))
                && input.State.MousePos.Y < (int)(Math.Floor(dy)) + GetHeight();
        }

        public bool Clicking(float dx, float dy, InputManager input)
        {
            return input.MouseReleased(eMouseButtons.Left)
                && Hovering(dx, dy, input);
        }

        public UIComponent CheckFocus(float dx, float dy, InputManager input)
        {
            if (!Initialized)
                return null;

            dx = dx + x;
            dy = dy + y;
            if (!Hovering(dx, dy, input))
            {
                if (LockFocus)
                {
                    return this;
                }
                return null;
            }
            UIComponent retval = this;
            if (ComponentsVisible && !PreventChildFocus(dx, dy, input))
            {
                for (int i = Components.Count - 1; i >= 0; i--)
                {
                    UIComponent comp = Components[i];
                    UIComponent option = comp.CheckFocus(dx + InnerX, dy + InnerY, input);
                    if (option != null)
                    {
                        retval = option;
                        break;
                    }
                }
            }
            return retval;
        }

        public virtual bool PreventChildFocus(float dx, float dy, InputManager input)
        {
            // can override this if you need to prevent children from taking focus in certain conditions
            return false;
        }

        public virtual void OnAdded()
        {
            // can override this to make a component do something to its parent when added
        }

        // focused event
        public EventHandler EventFocusedHandler;
        public class EventFocusedHandlerArgs : EventArgs
        {
            public float ElapsedMS;
            public InputManager Input;
        }
        public event EventHandler EventFocused
        {
            add
            {
                lock (EventLock)
                {
                    EventFocusedHandler += value;
                }
            }
            remove
            {
                lock (EventLock)
                {
                    EventFocusedHandler -= value;
                }
            }
        }

        public void GetFocus(float elapsedms, InputManager input)
        {
            Focused = true;
            SelfGetFocus(elapsedms, input);
            OnEventFocused(new EventFocusedHandlerArgs()
            {
                ElapsedMS = elapsedms,
                Input = input,
            });
        }

        protected virtual void SelfGetFocus(float elapsedms, InputManager input)
        {
            // this is called when a component is clicked
        }

        protected virtual void OnEventFocused(EventArgs e)
        {
            EventHandler handler;
            lock (EventLock)
            {
                handler = EventFocusedHandler;
            }
            if (handler != null)
            {
                handler(this, e);
            }
        }

        // unfocused event
        public EventHandler EventFocusLostHandler;
        public class EventFocusLostHandlerArgs : EventArgs
        {
            public float ElapsedMS;
            public InputManager Input;
        }
        public event EventHandler EventFocusLost
        {
            add
            {
                lock (EventLock)
                {
                    EventFocusLostHandler += value;
                }
            }
            remove
            {
                lock (EventLock)
                {
                    EventFocusLostHandler -= value;
                }
            }
        }

        public void FocusLost(float elapsedms, InputManager input)
        {
            Focused = false;
            SelfFocusLost(elapsedms, input);
            OnEventFocusLost(new EventFocusLostHandlerArgs()
            {
                ElapsedMS = elapsedms,
                Input = input,
            });
        }

        protected virtual void SelfFocusLost(float elapsedms, InputManager input)
        {
            // this is called when a component is clicked
        }

        protected virtual void OnEventFocusLost(EventArgs e)
        {
            EventHandler handler;
            lock (EventLock)
            {
                handler = EventFocusLostHandler;
            }
            if (handler != null)
            {
                handler(this, e);
            }
        }

        //accept input
        public void FocusedInput(float elapsedms, InputManager input)
        {
            SelfFocusedInput(elapsedms, input);
            if (Parent != null)
                Parent.SelfParentInput(elapsedms, input);
        }

        public virtual void SelfFocusedInput(float elapsedms, InputManager input)
        {
            // overwrite this if you need to do something with key input while focused
        }

        public virtual void SelfParentInput(float elapsedms, InputManager input)
        {
            // this is called when a child component is focused and receives input
        }

        // preinit event event
        public EventHandler EventPreInitHandler;
        public event EventHandler EventPreInit
        {
            add
            {
                lock (EventLock)
                {
                    EventPreInitHandler += value;
                }
            }
            remove
            {
                lock (EventLock)
                {
                    EventPreInitHandler -= value;
                }
            }
        }

        protected virtual void OnEventPreInit(EventArgs e)
        {
            EventHandler handler;
            lock (EventLock)
            {
                handler = EventPreInitHandler;
            }
            if (handler != null)
            {
                handler(this, e);
            }
        }

        // postinit event event
        public EventHandler EventPostInitHandler;
        public event EventHandler EventPostInit
        {
            add
            {
                lock (EventLock)
                {
                    EventPostInitHandler += value;
                }
            }
            remove
            {
                lock (EventLock)
                {
                    EventPostInitHandler -= value;
                }
            }
        }

        protected virtual void OnEventPostInit(EventArgs e)
        {
            EventHandler handler;
            lock (EventLock)
            {
                handler = EventPostInitHandler;
            }
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void Remove(UIComponent comp)
        {
            comp.Initialized = false;
            comp.Parent = null;
            Components.Remove(comp);
            SelfRemove(comp);
        }

        protected virtual void SelfRemove(UIComponent comp)
        {
            // override if needed
        }

        public void RemoveAll()
        {
            while (Components.Count > 0)
                Remove(Components[0]);
            SelfRemoveAll();
        }

        protected virtual void SelfRemoveAll()
        {
            // override if needed
        }

        #region IDisposable Support
        private bool disposedValue = false; // To detect redundant calls

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    SelfDispose();
                    if (Parent != null)
                    {
                        Parent.Remove(this);
                    }
                    if (System != null)
                    {
                        System.Remove(this);
                    }
                    for (int i = 0; i < Components.Count; i++)
                    {
                        Components[i].Dispose();
                    }
                }

                // TODO: free unmanaged resources (unmanaged objects) and override a finalizer below.
                // TODO: set large fields to null.

                disposedValue = true;
            }
        }

        // This code added to correctly implement the disposable pattern.
        public void Dispose()
        {
            // Do not change this code. Put cleanup code in Dispose(bool disposing) above.
            Dispose(true);
            // TODO: uncomment the following line if the finalizer is overridden above.
            // GC.SuppressFinalize(this);
        }
        #endregion
    }
}
