using RelaUI.Basics.Stylers;
using RelaUI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelaUI.Sampler.Screens
{
    public class PaintScreen : IScreen
    {
        public UIPanel Panel;

        private TestUI Main;
        private UIColorPicker ColorPicker;

        public PaintScreen(TestUI main)
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

            ColorPicker = new UIColorPicker(main.Graphics, 50, 0, 100, 100, 30, 50, 30, gradientResolution: 512);
            Panel.Add(ColorPicker);

            UIButton ReturnButton = new UIButton(0, 0, 50, 50, "Back", fontsize: 20);
            Panel.Add(ReturnButton);
            ReturnButton.EventFocused += (sender, args) =>
            {
                Main.Load(Main.HomeScreen);
            };

            Resize(main.Width, main.Height);
        }


        public void Resize(int newW, int newH)
        {
            Panel.Width = newW;
            Panel.Height = newH;

            // resizable components should be updated here
            ColorPicker.x = 55;
            ColorPicker.y = 5;
            ColorPicker.Width = (newW - 55) / 2;
            ColorPicker.Height = newH / 2;

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
