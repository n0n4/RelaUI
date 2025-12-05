using RelaUI.Basics.Stylers;
using RelaUI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelaUI.Sampler.Screens
{
    public class JPTextScreen : IScreen
    {
        public UIPanel Panel;

        private TestUI Main;
        private UITabView TabView;
        private List<UIPanel> Tabs = new List<UIPanel>();
        private List<UITextField> TabTexts = new List<UITextField>();

        public JPTextScreen(TestUI main)
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

            AddTab("test", @"よって、ここに、国連総会は
すべての人間は
加盟国は、国際連合と協力し");
            AddTab("test 2", @"check this out: 人類社会のすべての構成員の固有の尊厳と平等で譲ることのできない権利とを承認することは);");

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
                fontsize: 32, font: "NotoSansJP-Regular", multiLine: true);
            relaMultiText.EventPostInit += (sender, eargs) =>
            {
                relaMultiText.TextColor = new Microsoft.Xna.Framework.Color(218, 218, 218, 255);
            };
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
