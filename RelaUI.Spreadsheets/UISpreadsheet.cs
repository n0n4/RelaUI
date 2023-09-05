using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using RelaUI.Components;
using RelaUI.Helpers;
using RelaUI.Input;
using RelaUI.Text;
using System.Diagnostics.SymbolStore;
using System.Reflection.Metadata.Ecma335;
using System.Reflection.PortableExecutable;

namespace RelaUI.Spreadsheets
{
    public class UISpreadsheet<T> : UIComponent
    {
        
        public Color BackgroundColor;
        public Color HeadersBackgroundColor;
        public Color BorderColor;
        public int BorderWidth = 2;

        public int DefaultColumnWidth = 75;
        public int DefaultRowHeight = 30;
        private int HeaderHeight = 30;

        public int Width;
        public int Height;
        private List<int> ColumnWidths = new List<int>();
        private List<int> RowHeights = new List<int>();

        private List<RenderedText> ColumnHeaders;
        private List<eUIJustify> ColumnJustify = new List<eUIJustify>();

        private List<List<RenderedText>> CellDisplays = new List<List<RenderedText>>();
        private List<T> Rows = new List<T>();

        private Func<T, int, string> GetCellDisplayFromRow;
        private Func<T, int> GetValueCountOfRow;
        private Func<T, int, eSpreadsheetType> GetTypeOfCell;

        public Func<T, int, string> GetStringValue;
        public Func<T, int, int> GetIntValue;
        public Func<T, int, double> GetDoubleValue;
        public Func<T, int, bool> GetBoolValue;

        public Action<T, int, string> SetStringValue;
        public Action<T, int, int> SetIntValue;
        public Action<T, int, double> SetDoubleValue;
        public Action<T, int, bool> SetBoolValue;

        public RelaFont SFont;
        public TextSettings FontSettings = new TextSettings();

        public Scroller Scroller;

        private UITextField TextField;
        private UINumberField IntField;
        private UINumberField DoubleField;
        private int EditingX = -1;
        private int EditingY = -1;

        private float LastDx = 0;
        private float LastDy = 0;


        public UISpreadsheet(
            float x, float y, int w, int h,
            List<string> columnHeaders,
            Func<T, int, string> getCellDisplayFromRow,
            Func<T, int> getValueCountOfRow, 
            Func<T, int, eSpreadsheetType> getTypeOfCell,
            Func<T, int, string> getStringValue = null,
            Func<T, int, int> getIntValue = null,
            Func<T, int, double> getDoubleValue = null,
            Func<T, int, bool> getBoolValue = null,
            Action<T, int, string> setStringValue = null,
            Action<T, int, int> setIntValue = null,
            Action<T, int, double> setDoubleValue = null,
            Action<T, int, bool> setBoolValue = null
            )
        {
            this.x = x;
            this.y = y;
            Width = w;
            Height = h;
            ColumnHeaders = new List<RenderedText>();
            foreach (string cheader in columnHeaders)
            {
                RenderedText crender = new RenderedText();
                crender.Text = cheader;
                ColumnHeaders.Add(crender);
            }    
            GetCellDisplayFromRow = getCellDisplayFromRow;
            GetValueCountOfRow = getValueCountOfRow;

            GetStringValue = getStringValue;
            GetIntValue = getIntValue;
            GetDoubleValue = getDoubleValue;
            GetBoolValue = getBoolValue;

            SetStringValue = setStringValue;
            SetIntValue = setIntValue;
            SetDoubleValue = setDoubleValue;
            SetBoolValue = setBoolValue;

            Scroller = new Scroller(
                () => Width, () => Height,
                () => x, () => y,
                HeaderHeight);
            GetTypeOfCell = getTypeOfCell;

            TextField = new UITextField(0, 0, 50, 50, String.Empty);
            Add(TextField);
            TextField.Visible = false;
            TextField.EventEnterPressed += (sender, args) =>
            {
                var eargs = args as UITextField.EventEnterPressedHandlerArgs;
                UpdateStringValue(TextField.Text);
            };

            IntField = new UINumberField(0, 0, 50, 50, 30, "0", true, useButtons: false);
            Add(IntField);
            IntField.Visible = false;
            IntField.TextField.EventEnterPressed += (sender, args) =>
            {
                var eargs = args as UITextField.EventEnterPressedHandlerArgs;
                UpdateIntValue(IntField.AsInt());
            };

            DoubleField = new UINumberField(0, 0, 50, 50, 30, "0", false, useButtons: false);
            Add(DoubleField);
            DoubleField.Visible = false;
            DoubleField.TextField.EventEnterPressed += (sender, args) =>
            {
                var eargs = args as UITextField.EventEnterPressedHandlerArgs;
                UpdateDoubleValue(DoubleField.AsDouble());
            };
        }

        protected override void SelfInit()
        {
            BackgroundColor = Style.BackgroundColor;
            HeadersBackgroundColor = Style.SecondaryBackgroundColor;
            BorderColor = Style.BackgroundAccent;
            BorderWidth = Style.BorderWidth;

            SFont = FontSettings.Init(Style);

            Scroller.Init(Style);
        }

        public override int GetWidth()
        {
            return Width;
        }

        public override int GetHeight()
        {
            return Height;
        }

        public void AddRow(T row)
        {
            Rows.Add(row);
            RowHeights.Add(DefaultRowHeight);
            UpdateRowDisplays(row, Rows.Count - 1);
            CalculateScrollHeight();
        }
        public void UpdateRow(T row, int index)
        {
            Rows[index] = row;
            UpdateRowDisplays(row, index);
        }

        public void SetColumnHeader(int col, string header)
        {
            while (ColumnHeaders.Count <= col)
            {
                RenderedText crender = new RenderedText();
                crender.Text = string.Empty;
                ColumnHeaders.Add(crender);
            }
            ColumnHeaders[col].Text = header;
        }
        public void SetColumnJustify(int col, eUIJustify justify)
        {
            while (ColumnJustify.Count <= col)
                ColumnJustify.Add(eUIJustify.LEFT);
            ColumnJustify[col] = justify;
        }
        public void SetHeaderHeight(int height)
        {
            HeaderHeight = height;
            Scroller.VerticalOffset = height;
        }

        private void CalculateScrollHeight()
        {
            Scroller.ScrollHeight = 0;
            for (int i = 0; i < RowHeights.Count; i++) 
            {
                Scroller.ScrollHeight += RowHeights[i];
            }
        }
        private void CalculateScrollWidth()
        {
            Scroller.ScrollWidth = 0;
            for (int i = 0; i < ColumnWidths.Count; i++)
            {
                Scroller.ScrollWidth += ColumnWidths[i];
            }
        }

        private void UpdateRowDisplays(T row, int index)
        {
            Rows[index] = row;

            int count = GetValueCountOfRow(row);
            while (CellDisplays.Count < count)
                CellDisplays.Add(new List<RenderedText>());
            while (ColumnJustify.Count < count)
                ColumnJustify.Add(eUIJustify.LEFT);

            bool newcols = false;
            while (ColumnWidths.Count < count)
            {
                ColumnWidths.Add(DefaultColumnWidth);
                newcols = true;
            }

            if (newcols)
                CalculateScrollWidth();

            for (int i = 0; i < count; i++)
            {
                List<RenderedText> displaysColumn = CellDisplays[i];
                while (displaysColumn.Count <= index)
                {
                    RenderedText rendered = new RenderedText();
                    rendered.Text = string.Empty;
                    displaysColumn.Add(rendered);
                }

                string display = GetCellDisplayFromRow(row, i);
                displaysColumn[index].Text = display;
            }
        }

        protected override void SelfRender(float elapsedms, GraphicsDevice g, SpriteBatch sb, InputManager input, float dx, float dy)
        {
            LastDx = dx;
            LastDy = dy;

            PositionEditors();

            // draw the spreadsheet
            int yspace = HeaderHeight;
            int xoff = 0;
            Rectangle mainrect = RenderTargetScope.Open(sb, (int)Math.Floor(dx), (int)Math.Floor(dy), Width, Height);
            {
                int mx = input.State.MousePos.X;
                int my = input.State.MousePos.Y;
                Draw.DrawRectangleHandle(sb, (int)dx, (int)dy, Width, Height, BackgroundColor);
                
                // draw column dividers
                xoff = 0;
                for (int i = 0; i < ColumnWidths.Count; i++)
                {
                    xoff += ColumnWidths[i];
                    if (xoff < Scroller.OffsetX)
                        continue;
                    if (xoff > Scroller.OffsetX + Width)
                        break;

                    Draw.DrawRectangleHandle(sb, (int)dx + xoff - Scroller.OffsetX, (int)dy, BorderWidth, Height, BorderColor);
                }

                // draw row dividers
                int yoff = 0;
                for (int i = 0; i < RowHeights.Count; i++)
                {
                    yoff += RowHeights[i];
                    if (yoff < Scroller.OffsetY)
                        continue;
                    if (yoff > Scroller.OffsetY + Height)
                        break;

                    Draw.DrawRectangleHandle(sb, (int)dx, (int)dy + yoff + yspace - Scroller.OffsetY, Width, BorderWidth, BorderColor);
                }

                // draw cell displays
                xoff = 0;
                for (int i = 0; i < ColumnWidths.Count; i++)
                {
                    yoff = 0;
                    int wid = ColumnWidths[i];
                    xoff += ColumnWidths[i];
                    if (xoff < Scroller.OffsetX)
                        continue;
                    if (xoff > Scroller.OffsetX + Width)
                        break;

                    eUIJustify justify = ColumnJustify[i];

                    Rectangle colrect = RenderTargetScope.Open(sb, (int)Math.Floor(dx + xoff - wid - Scroller.OffsetX), (int)Math.Floor(dy), wid, Height);
                    {
                        for (int o = 0; o < RowHeights.Count; o++)
                        {
                            yoff += RowHeights[o];
                            int hei = RowHeights[o];
                            if (yoff < Scroller.OffsetY)
                                continue;
                            if (yoff > Scroller.OffsetY + Height)
                                break;

                            RenderedText text = CellDisplays[i][o];
                            int finalx = 4 + (int)dx + xoff - wid - Scroller.OffsetX;
                            int finaly = 4 + (int)dy + yoff + yspace - hei - Scroller.OffsetY
                                + (hei - TextHelper.GetHeight(SFont, text, FontSettings)) / 2;
                            if (justify != eUIJustify.LEFT)
                            {
                                int textWidth = TextHelper.GetWidth(SFont, text, FontSettings);
                                if (justify == eUIJustify.RIGHT)
                                {
                                    finalx += wid - textWidth - 8;
                                }
                                else if (justify == eUIJustify.CENTER)
                                {
                                    finalx += ((wid - textWidth) / 2) - 4;
                                }
                            }
                            Draw.DrawText(g, sb, SFont, finalx, finaly, text.Render(SFont), FontSettings);
                        }
                    }
                    RenderTargetScope.Close(sb, colrect);
                }

                // draw scrollbars
                Scroller.SelfRender(elapsedms, g, sb, input, dx, dy);
            }
            RenderTargetScope.Close(sb, mainrect);

            // draw headers
            Draw.DrawRectangleHandle(sb, (int)dx, (int)dy, Width, yspace, HeadersBackgroundColor);
            Draw.DrawRectangleHandle(sb, (int)dx, (int)dy + yspace, Width, BorderWidth, BorderColor);
            xoff = 0;
            for (int i = 0; i < ColumnHeaders.Count; i++)
            {
                int wid = ColumnWidths[i];
                xoff += ColumnWidths[i];
                if (xoff < Scroller.OffsetX)
                    continue;
                if (xoff > Scroller.OffsetX + Width)
                    break;

                int hei = HeaderHeight;
                RenderedText text = ColumnHeaders[i];
                int textWidth = TextHelper.GetWidth(SFont, text, FontSettings);
                Draw.DrawText(g, sb, SFont,
                    (int)dx + xoff - wid - Scroller.OffsetX + (wid - textWidth) / 2,
                    (int)dy + (hei - SFont.LineSpacing) - 4,
                    text.Text, FontSettings);
            }
        }

        private void PositionEditors()
        {
            if (EditingX < 0 || EditingY < 0 ||
                EditingX >= ColumnWidths.Count || EditingY >= RowHeights.Count)
                return;

            float newx = 0 - Scroller.OffsetX;
            float newy = HeaderHeight - Scroller.OffsetY;
            for (int i = 0; i < EditingX; i++)
                newx += ColumnWidths[i];
            for (int i = 0; i < EditingY; i++)
                newy += RowHeights[i];

            TextField.x = newx;
            TextField.y = newy;
            TextField.Width = ColumnWidths[EditingX];
            TextField.Height = RowHeights[EditingY];

            IntField.x = newx;
            IntField.y = newy;
            IntField.Width = ColumnWidths[EditingX];
            IntField.Height = RowHeights[EditingY];

            DoubleField.x = newx;
            DoubleField.y = newy;
            DoubleField.Width = ColumnWidths[EditingX];
            DoubleField.Height = RowHeights[EditingY];
        }

        public override bool PreventChildFocus(float dx, float dy, InputManager input)
        {
            // can override this if you need to prevent children from taking focus in certain conditions
            return Scroller.PreventChildFocus(dx, dy, input);
        }

        protected override void SelfGetFocus(float elapsedms, InputManager input)
        {
            if (Scroller.SelfGetFocus(elapsedms, input))
                return;

            // try to select a cell
            int mx = input.State.MousePos.X;
            int my = input.State.MousePos.Y;
            int offx = (int)(mx - LastDx + Scroller.OffsetX);
            int offy = (int)(my - LastDy + Scroller.OffsetY);

            int foundx = -1;
            int foundy = -1;

            int curx = 0;
            for (int i = 0; i < ColumnWidths.Count; i++)
            {
                int nextwid = ColumnWidths[i];
                if (offx >= curx && offx < curx + nextwid)
                {
                    int cury = HeaderHeight;
                    for (int o = 0; o < RowHeights.Count; o++)
                    {
                        int nexthei = RowHeights[o];
                        if (offy >= cury && offy < cury + nexthei)
                        {
                            foundx = i;
                            foundy = o;
                            break;
                        }

                        cury += nexthei;
                    }
                }

                curx += nextwid;
            }

            if (foundx != -1)
            {
                SelectCell(foundx, foundy, elapsedms, input);
            }
        }

        public override void SelfFocusedInput(float elapsedms, InputManager input)
        {
            Scroller.SelfFocusedInput(elapsedms, input);
        }

        public override void SelfParentInput(float elapsedms, InputManager input)
        {
            Scroller.SelfParentInput(elapsedms, input);
        }

        public void SelectCell(int cellx, int celly, float elapsedms, InputManager input)
        {
            if (celly >= Rows.Count)
            {
                StopEditingAndSaveChanges();
                return;
            }

            TextField.Visible = false;
            IntField.Visible = false;
            DoubleField.Visible = false;

            bool sameCell = EditingX == cellx && EditingY == celly;

            if (!sameCell)
            {
                StopEditingAndSaveChanges();
            }

            EditingX = cellx;
            EditingY = celly;

            T row = Rows[celly];
            eSpreadsheetType type = GetTypeOfCell(row, cellx);
            switch (type)
            {
                case eSpreadsheetType.Readonly:
                    return;
                case eSpreadsheetType.String:
                    TextField.Visible = true;
                    TextField.SetText(GetStringValue(row, cellx), elapsedms, input);
                    TextField.TakeFocus(true);
                    return;
                case eSpreadsheetType.Int:
                    IntField.Visible = true;
                    IntField.Set(GetIntValue(row, cellx), elapsedms, input);
                    IntField.GetFocus(elapsedms, input, 0, 0);
                    IntField.TextField.TakeFocus(true);
                    return;
                case eSpreadsheetType.Double:
                    DoubleField.Visible = true;
                    DoubleField.Set(GetDoubleValue(row, cellx), elapsedms, input);
                    DoubleField.GetFocus(elapsedms, input, 0, 0);
                    DoubleField.TextField.TakeFocus(true);
                    return;
                case eSpreadsheetType.Bool:
                    // only toggle on a double click
                    if (sameCell)
                    {
                        bool flipped = !GetBoolValue(row, cellx);
                        SetBoolValue(row, cellx, flipped);
                        UpdateRowDisplays(row, celly);
                        EditingX = -1;
                        EditingY = -1;
                    }
                    return;
                case eSpreadsheetType.Custom:
                    throw new NotImplementedException();
                default:
                    throw new NotImplementedException();
            }
        }

        private void UpdateStringValue(string value)
        {
            T row = Rows[EditingY];
            SetStringValue(row, EditingX, value);
            UpdateRowDisplays(row, EditingY);
            StopEditing();
        }

        private void UpdateIntValue(int value)
        {
            T row = Rows[EditingY];
            SetIntValue(row, EditingX, value);
            UpdateRowDisplays(row, EditingY);
            StopEditing();
        }

        private void UpdateDoubleValue(double value)
        {
            T row = Rows[EditingY];
            SetDoubleValue(row, EditingX, value);
            UpdateRowDisplays(row, EditingY);
            StopEditing();
        }

        public void StopEditingAndSaveChanges()
        {
            if (EditingX != -1 && EditingY != -1)
            {
                // save the current changes
                T row = Rows[EditingY];
                eSpreadsheetType type = GetTypeOfCell(row, EditingX);
                switch (type)
                {
                    case eSpreadsheetType.String:
                        UpdateStringValue(TextField.Text);
                        break;
                    case eSpreadsheetType.Int:
                        UpdateIntValue(IntField.AsInt());
                        break;
                    case eSpreadsheetType.Bool:
                        break;
                    case eSpreadsheetType.Double:
                        UpdateDoubleValue(DoubleField.AsDouble());
                        break;
                    case eSpreadsheetType.Readonly:
                        break;
                    case eSpreadsheetType.Custom:
                        throw new NotImplementedException();
                    default:
                        throw new NotImplementedException();
                }
            }

            StopEditing();
        }

        private void StopEditing()
        {
            EditingX = -1;
            EditingY = -1;
            TextField.Visible = false;
            IntField.Visible = false;
            DoubleField.Visible = false;
        }
    }
}