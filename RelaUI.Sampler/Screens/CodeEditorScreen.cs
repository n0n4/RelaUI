﻿using RelaUI.Basics.Stylers;
using RelaUI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelaUI.Sampler.Screens
{
    public class CodeEditorScreen : IScreen
    {
        public UIPanel Panel;

        private TestUI Main;
        private UITabView TabView;
        private List<UIPanel> Tabs = new List<UIPanel>();
        private List<UITextField> TabTexts = new List<UITextField>();

        public CodeEditorScreen(TestUI main)
        {
            Main = main;

            Panel = new UIPanel(0, 0, main.Width, main.Height)
            {
                InnerMargin = 0
            };
            Panel.EventPostInit += (sender, args) =>
            {
                Panel.HasBorder = false;
            };

            TabView = new UITabView(0, 0, Main.Width - 50, Main.Height, fontsize: 20);
            Panel.Add(TabView);

            AddTab("test.rela", "import basic:math anon\n" +
                "f:sinc := {\n" + 
                "\tf:sin(a:0) / a:o\n"
                +"};\n"
                +"f:sinc(0.33) // 0.174");
            AddTab("string_comma_array.rela", "import basic:string l:str\n" +
                "v:s := ('a','b','c');\n" +
                "l:str.f:commaand(v:s) // a, b, and c");
            AddTab("blank.rela", "");

            UIButton ReturnButton = new UIButton(0, 0, 50, 50, "Back", fontsize: 20);
            Panel.Add(ReturnButton);
            ReturnButton.EventFocused += (sender, args) =>
            {
                Main.Load(Main.HomeScreen);
            };

            Resize(main.Width, main.Height);
        }

        public void AddTab(string name, string text)
        {
            UIPanel panel = new UIPanel(0, 0, 0, 0)
            {
                HasBorder = false,
                InnerMargin = 0
            };
            Tabs.Add(panel);

            UITextField relaMultiText = new UITextField(0, 0, 0, 0, text, autoheight: false,
                fontsize: 18, font: "NotoMono-Regular", multiLine: true);
            relaMultiText.EventPostInit += (sender, eargs) =>
            {
                relaMultiText.TextColor = new Microsoft.Xna.Framework.Color(218, 218, 218, 255);
            };
            relaMultiText.SetTextStyler(new TextStylerRelaScript());
            panel.Add(relaMultiText);
            TabTexts.Add(relaMultiText);

            TabView.AddTab(name, name, panel);
        }

        public void Resize(int newW, int newH)
        {
            Panel.Width = newW;
            Panel.Height = newH;

            // resizable components should be updated here
            TabView.x = 50;
            TabView.y = 0;
            TabView.Width = newW - 50;
            TabView.Height = newH;

            foreach (UIPanel panel in Tabs)
            {
                panel.Width = TabView.Width;
                panel.Height = TabView.Height;
            }

            foreach (UITextField text in TabTexts)
            {
                text.Width = TabView.Width;
                text.Height = TabView.Height;
            }

            // reinit if appropriate
            Panel.TryInit();
        }

        public UIPanel GetPanel()
        {
            return Panel;
        }

        public void Loaded()
        {

        }

        public void Unloaded()
        {

        }

        public void Update(float elapsedms)
        {

        }
    }
}
