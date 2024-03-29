﻿using RelaUI.Basics.Stylers;
using RelaUI.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelaUI.Sampler.Screens
{
    public class HomeScreen : IScreen
    {
        public UIPanel Panel;

        private TestUI Main;

        private UIButton SamplerButton;
        private UIButton CodeEditorButton;
        private UIButton PaintButton;
        private UIButton SpreadsheetButton;
        private UIButton RichTextButton;

        private SamplerScreen SamplerScreen;
        private CodeEditorScreen CodeEditorScreen;
        private PaintScreen PaintScreen;
        private SpreadsheetScreen SpreadsheetScreen;
        private RichTextScreen RichTextScreen;

        public HomeScreen(TestUI main)
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

            SamplerButton = new UIButton(0, 0, 0, 0, "Sampler", fontsize: 32, autoheight: true);
            Panel.Add(SamplerButton);
            SamplerButton.EventFocused += (sender, args) =>
            {
                OpenSampler();
            };

            CodeEditorButton = new UIButton(0, 0, 0, 0, "Code Editor", fontsize: 32, autoheight: true);
            Panel.Add(CodeEditorButton);
            CodeEditorButton.EventFocused += (sender, args) =>
            {
                OpenCodeEditor();
            };

            PaintButton = new UIButton(0, 0, 0, 0, "Paint", fontsize: 32, autoheight: true);
            Panel.Add(PaintButton);
            PaintButton.EventFocused += (sender, args) =>
            {
                OpenPaint();
            };

            SpreadsheetButton = new UIButton(0, 0, 0, 0, "Spreadsheet", fontsize: 32, autoheight: true);
            Panel.Add(SpreadsheetButton);
            SpreadsheetButton.EventFocused += (sender, args) =>
            {
                OpenSpreadsheet();
            };

            RichTextButton = new UIButton(0, 0, 0, 0, "Rich Text", fontsize: 32, autoheight: true);
            Panel.Add(RichTextButton);
            RichTextButton.EventFocused += (sender, args) =>
            {
                OpenRichText();
            };

            Resize(main.Width, main.Height);
        }

        public void Resize(int newW, int newH)
        {
            Panel.Width = newW;
            Panel.Height = newH;

            // resizable components should be updated here
            int h = 25;
            SamplerButton.Width = 250;
            SamplerButton.Height = 50;
            SamplerButton.x = Main.Width / 2 - (SamplerButton.Width / 2);
            SamplerButton.y = h;
            h += 75;

            CodeEditorButton.Width = 250;
            CodeEditorButton.Height = 50;
            CodeEditorButton.x = Main.Width / 2 - (CodeEditorButton.Width / 2);
            CodeEditorButton.y = h;
            h += 75;

            PaintButton.Width = 250;
            PaintButton.Height = 50;
            PaintButton.x = Main.Width / 2 - (PaintButton.Width / 2);
            PaintButton.y = h;
            h += 75;

            SpreadsheetButton.Width = 250;
            SpreadsheetButton.Height = 50;
            SpreadsheetButton.x = Main.Width / 2 - (SpreadsheetButton.Width / 2);
            SpreadsheetButton.y = h;
            h += 75;

            RichTextButton.Width = 250;
            RichTextButton.Height = 50;
            RichTextButton.x = Main.Width / 2 - (RichTextButton.Width / 2);
            RichTextButton.y = h;
            h += 75;

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

        public void OpenSampler()
        {
            if (SamplerScreen == null)
            {
                SamplerScreen = new SamplerScreen(Main);
            }

            Main.Load(SamplerScreen);
        }

        public void OpenCodeEditor()
        {
            if (CodeEditorScreen == null)
            {
                CodeEditorScreen = new CodeEditorScreen(Main);
            }

            Main.Load(CodeEditorScreen);
        }

        public void OpenPaint()
        {
            if (PaintScreen == null)
            {
                PaintScreen = new PaintScreen(Main);
            }

            Main.Load(PaintScreen);
        }

        public void OpenSpreadsheet()
        {
            if (SpreadsheetScreen == null)
            {
                SpreadsheetScreen = new SpreadsheetScreen(Main);
            }

            Main.Load(SpreadsheetScreen);
        }

        public void OpenRichText()
        {
            if (RichTextScreen == null)
            {
                RichTextScreen = new RichTextScreen(Main);
            }

            Main.Load(RichTextScreen);
        }
    }
}
