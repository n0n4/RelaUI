using RelaUI.Components;
using RelaUI.Spreadsheets;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RelaUI.Sampler.Screens
{
    public class SpreadsheetScreen : IScreen
    {
        public UIPanel Panel;

        private TestUI Main;
        private UISpreadsheet<SheetData> Spreadsheet;

        public class SheetData 
        {
            public string Name;
            public int MaxHp;
            public double Attack;
            public bool IsEnemy;

            public SheetData(string name, int maxHp, double attack, bool isEnemy)
            {
                Name = name;
                MaxHp = maxHp;
                Attack = attack;
                IsEnemy = isEnemy;
            }

            public static List<string> ColumnHeaders = new List<string>()
            {
                "Name",
                "MaxHp",
                "Attack",
                "IsEnemy"
            };

            public static string GetCellDisplayFromRow(SheetData data, int index)
            {
                switch (index)
                {
                    case 0:
                        return data.Name;
                    case 1:
                        return data.MaxHp.ToString();
                    case 2:
                        return data.Attack.ToString();
                    case 3:
                        return data.IsEnemy.ToString();
                    default:
                        throw new NotImplementedException();
                }
            }

            public static int GetValueCountOfRow(SheetData data)
            {
                return 4;
            }

            public static eSpreadsheetType GetTypeFromCell(SheetData data, int index)
            {
                switch (index)
                {
                    case 0:
                        return eSpreadsheetType.String;
                    case 1:
                        return eSpreadsheetType.Int;
                    case 2:
                        return eSpreadsheetType.Double;
                    case 3:
                        return eSpreadsheetType.Bool;
                    default:
                        throw new NotImplementedException();
                }
            }

            public static string GetStringValue(SheetData data, int index)
            {
                if (index == 0)
                    return data.Name;
                throw new NotImplementedException();
            }

            public static int GetIntValue(SheetData data, int index)
            {
                if (index == 1)
                    return data.MaxHp;
                throw new NotImplementedException();
            }

            public static double GetDoubleValue(SheetData data, int index)
            {
                if (index == 2)
                    return data.Attack;
                throw new NotImplementedException();
            }

            public static bool GetBoolValue(SheetData data, int index)
            {
                if (index == 3) 
                    return data.IsEnemy;
                throw new NotImplementedException();
            }

            public static void SetStringValue(SheetData data, int index, string value)
            {
                if (index == 0)
                {
                    data.Name = value;
                    return;
                }
                throw new NotImplementedException();
            }

            public static void SetIntValue(SheetData data, int index, int value)
            {
                if (index == 1)
                {
                    data.MaxHp = value;
                    return;
                }
                throw new NotImplementedException();
            }

            public static void SetDoubleValue(SheetData data, int index, double value)
            {
                if (index == 2)
                {
                    data.Attack = value;
                    return;
                }
                throw new NotImplementedException();
            }

            public static void SetBoolValue(SheetData data, int index, bool value)
            {
                if (index == 3)
                {
                    data.IsEnemy = value;
                    return;
                }
                throw new NotImplementedException();
            }
        }

        public SpreadsheetScreen(TestUI main)
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

            Spreadsheet = new UISpreadsheet<SheetData>(
                50, 50, 100, 100,
                SheetData.ColumnHeaders,
                SheetData.GetCellDisplayFromRow,
                SheetData.GetValueCountOfRow,
                SheetData.GetTypeFromCell,

                SheetData.GetStringValue,
                SheetData.GetIntValue,
                SheetData.GetDoubleValue,
                SheetData.GetBoolValue,

                SheetData.SetStringValue,
                SheetData.SetIntValue,
                SheetData.SetDoubleValue,
                SheetData.SetBoolValue);
            Panel.Add(Spreadsheet);

            Spreadsheet.SetColumnJustify(0, eUIJustify.CENTER);
            Spreadsheet.SetColumnJustify(1, eUIJustify.RIGHT);
            Spreadsheet.SetColumnJustify(2, eUIJustify.RIGHT);

            // throw in some default data
            Spreadsheet.AddRow(new SheetData("Hero", 100, 20, false));
            Spreadsheet.AddRow(new SheetData("Cleric", 80, 5, false));
            Spreadsheet.AddRow(new SheetData("Ghost", 40, 7, true));
            for (int i = 1; i < 30; i++)
            {
                Spreadsheet.AddRow(new SheetData("Bandit" + i, 40 + 5 * i, 2 + i, true));
            }

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
            Spreadsheet.x = 55;
            Spreadsheet.y = 5;
            Spreadsheet.Width = newW - 60;
            Spreadsheet.Height = newH - 10;

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
