using System;
using System.Collections.Generic;
using System.Text;

namespace RelaUI.Components
{
    public class UITabView : UIPanel
    {
        private UIPanel TabPanel = null;
        private UIPanel DisplayPanel = null;
        private List<TabEntry> Tabs = new List<TabEntry>();

        /// <summary>
        /// If ButtonWidth is 0, buttons will be as large as their text requires
        /// </summary>
        private int ButtonWidth = 0;
        private int ButtonHeight = 30;

        private class TabEntry
        {
            public string Name = string.Empty;
            public string DisplayName = string.Empty;
            public UIComponent Tab = null;
            public bool Active = false;
            public UIButton Button = null;
            public UIButton XButton = null;

            public TabEntry(string name, string displayname, UIComponent tab, UIButton button, UIButton xbutton)
            {
                Name = name;
                DisplayName = displayname;
                Tab = tab;
                Button = button;
                XButton = xbutton;
            }
        }

        public UITabView(float x, float y, int w, int h,
            bool hastitle = false, string title = "", string titlefont = "", int titlesize = 0,
            eUIOrientation autoorientation = eUIOrientation.VERTICAL,
            bool hasscrolling = false, int scrollw = 0, int scrollh = 0,
            bool hasclose = false,
            int buttonwidth = 0, int buttonheight = 30)
            : base(x, y, w, h, hastitle, title, titlefont, titlesize, autoorientation, hasscrolling, scrollw, scrollh, hasclose)
        {
            TabPanel = new UIPanel(0, 0, w, buttonheight,
                autoorientation: eUIOrientation.HORIZONTAL,
                hasscrolling: true);
            Add(TabPanel);

            DisplayPanel = new UIPanel(0, buttonheight, w, h - buttonheight,
                hasscrolling: true);
            Add(DisplayPanel);
        }

        public void AddTab(string name, string displayname, UIComponent tab)
        {
            UIButton btn = new UIButton(0, 0, ButtonWidth, ButtonHeight, displayname,
                autowidth: (ButtonWidth == 0));
            TabPanel.AddAuto(btn);

            UIButton xbtn = new UIButton(0, 0, 0, ButtonHeight, "X",
                autowidth: true);
            TabPanel.AddAuto(xbtn);

            Tabs.Add(new TabEntry(name, displayname, tab, btn, xbtn));

            // add event to button
            btn.EventFocused += (s, e) =>
            {
                ChangeTab(name);
            };

            xbtn.EventFocused += (s, e) =>
            {
                RemoveTab(name);
            };

            if (Tabs.Count == 1) // if no other tabs, autoselect this one
                ChangeTab(name);
        }

        public void RemoveTab(string name)
        {
            for (int i = 0; i < Tabs.Count; i++)
            {
                var t = Tabs[i];
                if (t.Name == name)
                {
                    Tabs.RemoveAt(i);
                    i--;

                    TabPanel.Remove(t.Button);
                    TabPanel.Remove(t.XButton);
                    TabPanel.Init();

                    if (t.Active)
                    {
                        // if possible, switch to a new tab if this one was the active tab
                        if (Tabs.Count != 0)
                        {
                            ChangeTab(i);
                        }
                        else
                        {
                            // otherwise just remove this tab
                            DisplayPanel.Remove(t.Tab);
                            t.Active = false;
                        }
                    }
                }
            }
        }

        public void ChangeTab(string name)
        {
            for (int i = 0; i < Tabs.Count; i++)
            {
                if (Tabs[i].Name == name)
                {
                    ChangeTab(i);
                    return;
                }
            }
        }

        public void ChangeTab(int index)
        {
            for (int i = 0; i < Tabs.Count; i++)
            {
                var t = Tabs[i];
                if (i == index)
                {
                    DisplayPanel.Add(t.Tab);
                    DisplayPanel.ScrollWidth = t.Tab.GetWidth();
                    DisplayPanel.ScrollHeight = t.Tab.GetHeight();
                    t.Active = true;
                    t.Button.StayPressed = true;
                }
                else
                {
                    t.Button.StayPressed = false;
                    if (t.Active)
                    {
                        DisplayPanel.Remove(t.Tab);
                        t.Active = false;
                    }
                }
            }
        }
    }
}
