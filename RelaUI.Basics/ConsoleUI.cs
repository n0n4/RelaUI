using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RelaUI.Components;
using RelaUI.Console;
using RelaUI.Input;
using RelaUI.Styles;
using System;
using System.Collections.Generic;

namespace RelaUI.Basics
{
    public class ConsoleUI : IUIHandle
    {
        private UIBoss Boss;

        public UISystem UISystem;
        public IConsole Console;

        private UIAutoList LogList;
        private UIPanel LogPanel;
        private UITextField InputField;

        private UILabel ScrapLabel;

        private int LogsToLoad = 30;
        private int LastLogNo = 0;

        private List<string> LogMsgs = new List<string>();

        private List<UILabel> Labels = new List<UILabel>();

        public ConsoleUI(UIBoss boss, UIStyle style, GraphicsDevice g, IConsole console,
            int width, int height,
            int logstoload = 30, string font = "NotoMono-Regular", int? fontsize = 16)
        {
            Boss = boss;

            Console = console;
            UISystem = new UISystem(g);

            LogsToLoad = logstoload;
            int hperlog = 16;

            int innerHeight = (hperlog + 4) * LogsToLoad;
            LogList = new UIAutoList(eUIOrientation.VERTICAL,width - 16 - 4, innerHeight,
                fixedindexwidth: width - 16 - 4, fixedindexheight: hperlog, alternatesBackground: true);
            LogList.HasBorder = false;
            LogList.HasOuterBorder = false;
            LogList.MarginY = 2;

            
            for (int i = 0; i < LogsToLoad; i++)
            {
                UILabel lab = new UILabel(0, 0, width - 16, hperlog, "",
                    font: font, fontsize: fontsize)
                {
                    TextOneLineOnly = true
                };
                Labels.Add(lab);
                LogList.AddAuto(lab);
            }



            LogPanel = new UIPanel(0, 0, width, height,
                true, "Console",
                hasscrolling: true, scrollh: innerHeight);
            LogPanel.AutoScrollHeight = true;
            LogPanel.AddAuto(LogList);

            LogPanel.Visible = false;

            ScrapLabel = new UILabel(0, 0, width - 32, hperlog, "");
            ScrapLabel.Visible = false;
            LogPanel.Add(ScrapLabel);

            InputField = new UITextField(0, 0, width - 24, hperlog, "",
                font: font, fontsize: fontsize, placeholdertext: "Input...");
            InputField.EventEnterPressed += (sender, e) =>
            {
                string inputtext = InputField.Text;
                UITextField.EventEnterPressedHandlerArgs eargs = (e as UITextField.EventEnterPressedHandlerArgs);
                InputField.ClearText(eargs.ElapsedMS, eargs.Input);
                InputField.CaretPosition = 0;
                Console.Input(inputtext);
            };
            LogPanel.AddAuto(InputField);

            UISystem.Add(LogPanel);

            UISystem.Init(style);

            LogPanel.SnapScrollToBottom();
            
            /*UILabel firstLabel = (LogList.Components[0] as UILabel);
            firstLabel.EventPostInit += (sender, args) =>
            {
                MaxLength = (LogPanel.Width - 32) / firstLabel.FontSettings.MonospaceSize;
            };*/
        }

        public string GetName()
        {
            return "ConsoleUI";
        }

        public UISystem GetUISystem()
        {
            return UISystem;
        }

        public void Update(float elapsedms, InputManager input)
        {
            if (!LogPanel.Initialized)
                return; // don't load until initialized

            int logcount = Console.LogCount();
            if (logcount > LastLogNo)
            {
                while (logcount > LastLogNo)
                {
                    string s = Console.GetLog(LastLogNo);
                    ScrapLabel.Text = s;
                    ScrapLabel.ProcessText();

                    if (ScrapLabel.TextLines.Count > 1)
                    {
                        for (int i = 0; i < ScrapLabel.TextLines.Count; i++)
                            LogMsgs.Add(ScrapLabel.TextLines[i]);
                    }
                    else
                    {
                        LogMsgs.Add(s);
                    }

                    LastLogNo++;
                }

                while (LogMsgs.Count > LogsToLoad)
                {
                    LogMsgs.RemoveAt(0);
                }

                int c = 0;
                for (int i = LogMsgs.Count - 1; i >= 0; i--)
                {
                    UILabel lab = Labels[Labels.Count - 1 - c];
                    lab.Text = LogMsgs[i];
                    lab.ProcessText();
                    c++;
                }
            }
        }

        public void RegisterHotkeys(HotkeyManager hotkeyManager)
        {
            //register hotkey to open close
            hotkeyManager.Register("Toggle Console", GetName(), ToggleConsole);
        }

        public bool ToggleConsole(float elapsedms, InputManager input, Keys[] keys, int keyCount)
        {
            if (!input.IsAtLeastOnePressed(keys, keyCount))
                return false;

            LogPanel.Visible = !LogPanel.Visible;
            if (LogPanel.Visible)
            {
                Boss.Unfocus(elapsedms, input);
                Boss.Focus(InputField, elapsedms, input, 0, 0);
            }
            return true;
        }
    }
}
