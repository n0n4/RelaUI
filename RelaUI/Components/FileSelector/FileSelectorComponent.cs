using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using RelaUI.Components;
using RelaUI.Input;
using System;
using System.Collections.Generic;
using System.IO;

namespace RelaUI.Components.FileSelector
{
    public class FileSelectorComponent : UIPanel
    {
        private const int RowHeight = 30;
        private const int ButtonHeight = 32;
        private const int ButtonGap = 10;
        private const int PreferredWidth = 640;
        private const int PreferredHeight = 800;
        private const int OuterMargin = 20;
        private const double DoubleClickMs = 350.0;

        private struct FileEntry
        {
            public string Name;
            public string Path;
            public bool IsDirectory;
        }

        private UILabel TitleLabel;
        private UITextField FolderText;
        private UIButton UpButton;
        private UIPanel FileListPanel;
        private UITextField FileText;
        private UILabel ErrorLabel;
        private UIButton NewFolderButton;
        private UIButton SelectButton;
        private UIButton CancelButton;

        private UIPanel NewFolderPanel;
        private UILabel NewFolderTitleLabel;
        private UILabel NewFolderPromptLabel;
        private UITextField NewFolderText;
        private UIButton NewFolderCreateButton;
        private UIButton NewFolderCancelButton;

        private UIPanel OverwritePanel;
        private UILabel OverwriteLabel;
        private UIButton OverwriteYesButton;
        private UIButton OverwriteNoButton;        
        private readonly List<FileEntry> Entries = new List<FileEntry>();
        private FileSelectorOptions? Options;
        private FileSelectorStrings Strings = new FileSelectorStrings();
        private FileSelectorResult? Result;
        private string CurrentFolder = string.Empty;
        private string PendingOverwritePath = string.Empty;
        private UIButton? SelectedRow;
        private string LastClickedPath = string.Empty;
        private double UiElapsedMs;
        private double LastClickElapsedMs = double.MinValue;

        public bool IsOpen => Visible;
        public FileSelectorOptions? CurrentOptions => Options;

        public event Action<FileSelectorResult>? Closed;

        public FileSelectorComponent()
            : base(0, 0, 640, 460)
        {
            Visible = false;
            LockFocus = false;
            InnerMargin = 0;
            Build();
        }

        private void Build()
        {
            TitleLabel = new UILabel(10, 8, Width - 20, 10, string.Empty, fontsize: 20, autoheight: true);
            Add(TitleLabel);

            FolderText = new UITextField(10, 40, Width - 90, 30, string.Empty, fontsize: 14, autoheight: false);
            FolderText.EventEnterPressed += (sender, args) =>
            {
                UITextField.EventEnterPressedHandlerArgs eargs = args as UITextField.EventEnterPressedHandlerArgs;
                ShowFolder(FolderText.Text, eargs?.ElapsedMS ?? 0, eargs?.Input);
            };
            Add(FolderText);

            UpButton = new UIButton(Width - 70, 40, 50, 30, "Up", fontsize: 14);
            UpButton.EventFocused += (sender, args) => GoUp();
            Add(UpButton);

            FileListPanel = new UIPanel(10, 78, Width - 20, Height - 198, hasscrolling: true)
            {
                HasBorder = false,
                InnerMargin = 2
            };
            Add(FileListPanel);

            FileText = new UITextField(10, Height - 90, Width - 20, 30, string.Empty, fontsize: 14, autoheight: false);
            FileText.EventEnterPressed += (sender, args) => Confirm();
            Add(FileText);

            ErrorLabel = new UILabel(10, Height - 114, Width - 20, 20, string.Empty, fontsize: 12, autoheight: false);
            ErrorLabel.Visible = false;
            ErrorLabel.EventPostInit += (sender, args) =>
            {
                ErrorLabel.FontSettings.Color = new Color(220, 80, 80);
                ErrorLabel.ProcessText();
            };
            Add(ErrorLabel);

            NewFolderButton = new UIButton(10, Height - 50, 100, 32, "New Folder", fontsize: 14);
            NewFolderButton.EventFocused += (sender, args) => ShowNewFolder();
            Add(NewFolderButton);

            SelectButton = new UIButton(120, Height - 50, 100, 32, "Select", fontsize: 14);
            SelectButton.EventFocused += (sender, args) => Confirm();
            Add(SelectButton);

            CancelButton = new UIButton(Width - 110, Height - 50, 100, 32, "Cancel", fontsize: 14);
            CancelButton.EventFocused += (sender, args) => Cancel();
            Add(CancelButton);

            BuildNewFolderPanel();
            BuildOverwritePanel();
            LayoutControls();
        }

        private void BuildNewFolderPanel()
        {
            NewFolderPanel = new UIPanel(0, 0, 320, 160);
            NewFolderPanel.Visible = false;
            NewFolderPanel.LockFocus = false;
            Add(NewFolderPanel);

            NewFolderTitleLabel = new UILabel(10, 10, 300, 10, "New Folder", fontsize: 18, autoheight: true);
            NewFolderPanel.Add(NewFolderTitleLabel);

            NewFolderPromptLabel = new UILabel(10, 38, 300, 10, "Name the new folder:", fontsize: 14, autoheight: true);
            NewFolderPanel.Add(NewFolderPromptLabel);

            NewFolderText = new UITextField(10, 64, 300, 30, string.Empty, fontsize: 14, autoheight: false,
                placeholdertext: "Folder Name...");
            NewFolderText.EventEnterPressed += (sender, args) => CreateFolder();
            NewFolderPanel.Add(NewFolderText);

            NewFolderCreateButton = new UIButton(10, 112, 120, 32, "Create", fontsize: 14);
            NewFolderCreateButton.EventFocused += (sender, args) => CreateFolder();
            NewFolderPanel.Add(NewFolderCreateButton);

            NewFolderCancelButton = new UIButton(170, 112, 120, 32, "Cancel", fontsize: 14);
            NewFolderCancelButton.EventFocused += (sender, args) => HideNewFolder();
            NewFolderPanel.Add(NewFolderCancelButton);
        }

        private void BuildOverwritePanel()
        {
            OverwritePanel = new UIPanel(0, 0, 400, 176, hastitle: true, title: string.Empty, titlesize: 16);
            OverwritePanel.Visible = false;
            OverwritePanel.LockFocus = false;
            Add(OverwritePanel);

            OverwriteLabel = new UILabel(10, 15, 380, 70, string.Empty, fontsize: 14, autoheight: false);
            OverwriteLabel.Centered = true;
            OverwriteLabel.TextSplitWords = true;
            OverwritePanel.Add(OverwriteLabel);

            OverwriteYesButton = new UIButton(40, 118, 120, 32, "Yes", fontsize: 14);
            OverwriteYesButton.EventFocused += (sender, args) => ConfirmOverwrite();
            OverwritePanel.Add(OverwriteYesButton);

            OverwriteNoButton = new UIButton(240, 118, 120, 32, "No", fontsize: 14);
            OverwriteNoButton.EventFocused += (sender, args) => HideOverwrite();
            OverwritePanel.Add(OverwriteNoButton);
        }

        private void LayoutControls()
        {
            int contentWidth = Math.Max(1, Width - 20);

            TitleLabel.x = 10;
            TitleLabel.y = 8;
            TitleLabel.Width = contentWidth;
            RefreshLabel(TitleLabel);

            int upButtonWidth = 50;
            FolderText.x = 10;
            FolderText.y = 40;
            FolderText.Width = Math.Max(10, contentWidth - upButtonWidth - ButtonGap);
            FolderText.Height = 30;

            UpButton.x = Width - 10 - upButtonWidth;
            UpButton.y = 40;
            UpButton.Width = upButtonWidth;
            UpButton.Height = 30;

            int listY = 78;
            int errorY = Height - 114;
            FileListPanel.x = 10;
            FileListPanel.y = listY;
            FileListPanel.Width = contentWidth;
            FileListPanel.Height = Math.Max(20, errorY - 6 - listY);
            FileListPanel.ScrollWidth = FileListPanel.Width;
            FileListPanel.ScrollHeight = Math.Max(FileListPanel.Height, Entries.Count * RowHeight + 8);

            ErrorLabel.x = 10;
            ErrorLabel.y = errorY;
            ErrorLabel.Width = contentWidth;
            RefreshLabel(ErrorLabel);

            FileText.x = 10;
            FileText.y = Height - 90;
            FileText.Width = contentWidth;
            FileText.Height = 30;

            int footerY = Height - 50;
            int buttonWidth = 100;
            if (buttonWidth * 3 + ButtonGap * 2 > contentWidth)
                buttonWidth = Math.Max(1, (contentWidth - ButtonGap * 2) / 3);

            NewFolderButton.x = 10;
            NewFolderButton.y = footerY;
            NewFolderButton.Width = buttonWidth;
            NewFolderButton.Height = ButtonHeight;

            SelectButton.x = 10 + buttonWidth + ButtonGap;
            SelectButton.y = footerY;
            SelectButton.Width = buttonWidth;
            SelectButton.Height = ButtonHeight;

            CancelButton.x = Width - 10 - buttonWidth;
            CancelButton.y = footerY;
            CancelButton.Width = buttonWidth;
            CancelButton.Height = ButtonHeight;

            LayoutNewFolderPanel();
            LayoutOverwritePanel();
            UpdateRowWidths();
        }

        private void LayoutNewFolderPanel()
        {
            int panelWidth = Math.Max(1, Math.Min(320, Width - 20));
            int contentWidth = Math.Max(1, panelWidth - 20);

            NewFolderPanel.Width = panelWidth;
            NewFolderPanel.Height = 160;

            NewFolderTitleLabel.x = 10;
            NewFolderTitleLabel.y = 10;
            NewFolderTitleLabel.Width = contentWidth;
            RefreshLabel(NewFolderTitleLabel);

            NewFolderPromptLabel.x = 10;
            NewFolderPromptLabel.y = 38;
            NewFolderPromptLabel.Width = contentWidth;
            RefreshLabel(NewFolderPromptLabel);

            NewFolderText.x = 10;
            NewFolderText.y = 64;
            NewFolderText.Width = contentWidth;
            NewFolderText.Height = 30;

            int buttonWidth = Math.Max(1, (contentWidth - ButtonGap) / 2);

            NewFolderCreateButton.x = 10;
            NewFolderCreateButton.y = 112;
            NewFolderCreateButton.Width = buttonWidth;
            NewFolderCreateButton.Height = ButtonHeight;

            NewFolderCancelButton.x = 10 + buttonWidth + ButtonGap;
            NewFolderCancelButton.y = 112;
            NewFolderCancelButton.Width = buttonWidth;
            NewFolderCancelButton.Height = ButtonHeight;

            CenterChildPanel(NewFolderPanel);
        }

        private void LayoutOverwritePanel()
        {
            int panelWidth = Math.Max(1, Math.Min(400, Width - 20));
            int contentWidth = Math.Max(1, panelWidth - 20);

            OverwritePanel.Width = panelWidth;
            OverwritePanel.Height = 176;

            OverwriteLabel.x = 10;
            OverwriteLabel.y = 15;
            OverwriteLabel.Width = contentWidth;
            OverwriteLabel.Height = 70;
            RefreshLabel(OverwriteLabel);

            int buttonWidth = Math.Max(1, (contentWidth - ButtonGap) / 2);

            OverwriteYesButton.x = 10;
            OverwriteYesButton.y = 118;
            OverwriteYesButton.Width = buttonWidth;
            OverwriteYesButton.Height = ButtonHeight;

            OverwriteNoButton.x = 10 + buttonWidth + ButtonGap;
            OverwriteNoButton.y = 118;
            OverwriteNoButton.Width = buttonWidth;
            OverwriteNoButton.Height = ButtonHeight;

            CenterChildPanel(OverwritePanel);
        }

        private void UpdateRowWidths()
        {
            if (FileListPanel == null)
                return;

            int rowWidth = Math.Max(1, FileListPanel.Width - 10);
            for (int i = 0; i < FileListPanel.Components.Count; i++)
            {
                if (FileListPanel.Components[i] is UIButton row)
                {
                    row.Width = rowWidth;
                    if (row.Initialized)
                        row.ProcessText();
                }
            }
        }

        private static void RefreshLabel(UILabel label)
        {
            if (label.Initialized)
                label.ProcessText();
        }

        public void Open(FileSelectorOptions options)
        {
            Options = options ?? new FileSelectorOptions();
            Strings = Options.Strings ?? new FileSelectorStrings();
            Result = null;
            PendingOverwritePath = string.Empty;
            LastClickedPath = string.Empty;
            LastClickElapsedMs = double.MinValue;
            UiElapsedMs = 0;
            SelectedRow = null;

            if (!Initialized)
                TryInit();

            ApplyOptions();
            ClearError();
            HideNewFolder();
            HideOverwrite();

            string startFolder = GetStartFolder(Options.StartFolder);
            string defaultName = Options.DefaultFileName ?? string.Empty;
            if (Options.Mode == FileSelectorMode.SaveFile
                && !string.IsNullOrEmpty(Options.DefaultExtension)
                && !string.IsNullOrEmpty(defaultName)
                && string.IsNullOrEmpty(Path.GetExtension(defaultName)))
            {
                defaultName += NormalizeExtension(Options.DefaultExtension);
            }

            CenterInParent();

            FileText.SetText(defaultName, 0, null);
            ShowFolder(startFolder, 0, null);

            Visible = true;
            LockFocus = true;

            if (System != null)
                System.ProposedFocus = Options.Mode == FileSelectorMode.SelectFolder ? FolderText : FileText;
        }

        private void ApplyOptions()
        {
            if (Options == null)
                return;

            SetLabelText(TitleLabel, Options.Title);
            SetButtonText(UpButton, Strings.Up);
            SetButtonText(NewFolderButton, Strings.NewFolder);
            SetButtonText(SelectButton, Options.Mode == FileSelectorMode.SaveFile
                ? Strings.Save
                : Options.Mode == FileSelectorMode.SelectFolder ? Strings.SelectFolder : Strings.Select);
            SetButtonText(CancelButton, Strings.Cancel);

            SetLabelText(NewFolderTitleLabel, Strings.NewFolderTitle);
            SetLabelText(NewFolderPromptLabel, Strings.NewFolderPrompt);
            SetButtonText(NewFolderCreateButton, Strings.Create);
            SetButtonText(NewFolderCancelButton, Strings.Cancel);

            SetLabelText(OverwriteLabel, string.Empty);
            OverwritePanel.Title = Strings.OverwriteTitle;
            SetButtonText(OverwriteYesButton, Strings.Yes);
            SetButtonText(OverwriteNoButton, Strings.No);

            NewFolderButton.Visible = Options.AllowNewFolder;
            FileText.Visible = Options.Mode != FileSelectorMode.SelectFolder;
        }

        private string GetStartFolder(string startFolder)
        {
            if (string.IsNullOrWhiteSpace(startFolder))
                startFolder = Directory.GetCurrentDirectory();

            try
            {
                startFolder = Path.GetFullPath(startFolder);
            }
            catch
            {
                startFolder = Directory.GetCurrentDirectory();
            }

            return Directory.Exists(startFolder) ? startFolder : Directory.GetCurrentDirectory();
        }

        private void ShowFolder(string folderPath, float elapsedms, InputManager? input)
        {
            if (Options == null)
                return;

            string path;
            try
            {
                path = Path.GetFullPath(folderPath);
            }
            catch (Exception e)
            {
                ShowError(Strings.CannotOpenFolder + e.Message);
                return;
            }

            if (!Directory.Exists(path))
            {
                ShowError(Strings.CannotOpenFolder + path);
                return;
            }

            CurrentFolder = path;
            FolderText.SetText(path, elapsedms, input);
            FolderText.CaretPosition = path.Length;
            UpButton.Enabled = Directory.GetParent(path) != null;
            ClearError();
            SelectedRow = null;
            LastClickedPath = string.Empty;

            FileListPanel.RemoveAll();
            Entries.Clear();

            try
            {
                foreach (string directory in Directory.EnumerateDirectories(path))
                {
                    Entries.Add(new FileEntry()
                    {
                        Name = Path.GetFileName(directory),
                        Path = directory,
                        IsDirectory = true
                    });
                }

                if (Options.Mode != FileSelectorMode.SelectFolder)
                {
                    foreach (string file in Directory.EnumerateFiles(path))
                    {
                        if (!MatchesFilter(file))
                            continue;

                        Entries.Add(new FileEntry()
                        {
                            Name = Path.GetFileName(file),
                            Path = file,
                            IsDirectory = false
                        });
                    }
                }
            }
            catch (Exception e)
            {
                ShowError(Strings.CannotOpenFolder + e.Message);
            }

            Entries.Sort(CompareEntries);

            for (int i = 0; i < Entries.Count; i++)
            {
                FileEntry entry = Entries[i];
                UIButton row = new UIButton(0, 0, FileListPanel.Width - 10, RowHeight,
                    entry.IsDirectory ? entry.Name + "/" : entry.Name, fontsize: 14)
                {
                    Justified = eUIJustify.LEFT
                };
                row.EventPostInit += (sender, args) =>
                {
                    row.HasBorder = false;
                    row.HoverBackgroundOnly = true;
                };
                row.EventFocused += (sender, args) =>
                {
                    OnEntryFocused(entry, row, args as UIComponent.EventFocusedHandlerArgs);
                };
                FileListPanel.AddAuto(row);
            }

            FileListPanel.ScrollHeight = Math.Max(FileListPanel.Height, Entries.Count * RowHeight + 8);
            FileListPanel.Init();
            FileListPanel.Scroller.OffsetY = 0;
        }

        private static int CompareEntries(FileEntry a, FileEntry b)
        {
            if (a.IsDirectory != b.IsDirectory)
                return a.IsDirectory ? -1 : 1;

            return string.Compare(a.Name, b.Name, StringComparison.OrdinalIgnoreCase);
        }

        private void OnEntryFocused(FileEntry entry, UIButton row, UIComponent.EventFocusedHandlerArgs? args)
        {
            if (entry.IsDirectory)
            {
                ShowFolder(entry.Path, args?.ElapsedMS ?? 0, args?.Input);
                return;
            }

            FileText.SetText(entry.Name, args?.ElapsedMS ?? 0, args?.Input);
            SelectRow(row);

            double clickTime = UiElapsedMs + (args?.ElapsedMS ?? 0f);
            if (LastClickedPath == entry.Path && clickTime - LastClickElapsedMs <= DoubleClickMs)
            {
                LastClickedPath = string.Empty;
                LastClickElapsedMs = double.MinValue;
                Confirm();
                return;
            }

            LastClickedPath = entry.Path;
            LastClickElapsedMs = clickTime;
        }

        private void SelectRow(UIButton row)
        {
            if (SelectedRow != null)
            {
                SelectedRow.StayPressed = false;
                SelectedRow.HoverBackgroundOnly = true;
                SelectedRow.HoverColor = Style.ButtonHoverColor;
                SelectedRow.PressedColor = Style.ButtonPressedColor;
            }

            SelectedRow = row;
            row.StayPressed = true;
            row.HoverBackgroundOnly = false;
            row.BackgroundColor = Style.HighlightColor;
            row.HoverColor = Style.HighlightColor;
            row.PressedColor = Style.HighlightColor;
        }

        private void GoUp()
        {
            DirectoryInfo? parent = Directory.GetParent(CurrentFolder);
            if (parent != null)
                ShowFolder(parent.FullName, 0, null);
        }

        private void Confirm()
        {
            if (Options == null)
                return;

            ClearError();

            if (Options.Mode == FileSelectorMode.SelectFolder)
            {
                Complete(CurrentFolder);
                return;
            }

            string fileName = (FileText.Text ?? string.Empty).Trim();
            if (!IsValidFileName(fileName))
            {
                ShowError(Strings.InvalidFileName);
                return;
            }

            if (Options.Mode == FileSelectorMode.SaveFile
                && !string.IsNullOrEmpty(Options.DefaultExtension)
                && string.IsNullOrEmpty(Path.GetExtension(fileName)))
            {
                fileName += NormalizeExtension(Options.DefaultExtension);
            }

            if (Options.Mode == FileSelectorMode.SaveFile && !MatchesFilter(fileName))
            {
                ShowError(Strings.InvalidFileName);
                return;
            }

            string path;
            try
            {
                path = Path.Combine(CurrentFolder, fileName);
            }
            catch
            {
                ShowError(Strings.InvalidFileName);
                return;
            }

            if (Options.Mode == FileSelectorMode.SelectFile)
            {
                if (!File.Exists(path))
                {
                    ShowError(Strings.FileNotFound);
                    return;
                }

                Complete(path);
            }
            else if (Options.Mode == FileSelectorMode.SaveFile)
            {
                if (Options.ConfirmOverwrite && File.Exists(path))
                {
                    ShowOverwrite(path);
                    return;
                }

                Complete(path);
            }
        }

        private static bool IsValidFileName(string fileName)
        {
            if (string.IsNullOrWhiteSpace(fileName) || fileName == "." || fileName == "..")
                return false;

            if (fileName.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
                return false;

            return !fileName.Contains(Path.DirectorySeparatorChar)
                && !fileName.Contains(Path.AltDirectorySeparatorChar);
        }

        private bool MatchesFilter(string filePath)
        {
            if (Options == null || Options.Extensions == null || Options.Extensions.Length == 0)
                return true;

            string extension = Path.GetExtension(filePath);
            for (int i = 0; i < Options.Extensions.Length; i++)
            {
                if (string.Equals(extension, NormalizeExtension(Options.Extensions[i]), StringComparison.OrdinalIgnoreCase))
                    return true;
            }

            return false;
        }

        private static string NormalizeExtension(string extension)
        {
            if (string.IsNullOrEmpty(extension))
                return extension;

            return extension[0] == '.' ? extension : "." + extension;
        }

        private void ShowOverwrite(string path)
        {
            PendingOverwritePath = path;
            SetLabelText(OverwriteLabel, string.Format(Strings.OverwritePrompt, Path.GetFileName(path)));
            OverwritePanel.Visible = true;
            OverwritePanel.LockFocus = true;
            CenterChildPanel(OverwritePanel);
        }

        private void HideOverwrite()
        {
            OverwritePanel.Visible = false;
            OverwritePanel.LockFocus = false;
            if (Visible && System != null)
                System.ProposedFocus = FileText;
        }

        private void ConfirmOverwrite()
        {
            if (string.IsNullOrEmpty(PendingOverwritePath))
                return;

            string path = PendingOverwritePath;
            HideOverwrite();
            Complete(path);
        }

        private void ShowNewFolder()
        {
            NewFolderText.SetText(string.Empty, 0, null);
            NewFolderPanel.Visible = true;
            NewFolderPanel.LockFocus = true;
            CenterChildPanel(NewFolderPanel);
            if (System != null)
                System.ProposedFocus = NewFolderText;
        }

        private void HideNewFolder()
        {
            NewFolderPanel.Visible = false;
            NewFolderPanel.LockFocus = false;
            if (Visible && System != null)
                System.ProposedFocus = FileText;
        }

        private void CreateFolder()
        {
            string name = (NewFolderText.Text ?? string.Empty).Trim();
            if (!IsValidFileName(name))
            {
                ShowError(Strings.InvalidFileName);
                return;
            }

            string path;
            try
            {
                path = Path.Combine(CurrentFolder, name);
                Directory.CreateDirectory(path);
            }
            catch (Exception e)
            {
                ShowError(Strings.FolderCreateFailed + e.Message);
                return;
            }

            HideNewFolder();
            ShowFolder(path, 0, null);
        }

        private void Complete(string path)
        {
            Result = new FileSelectorResult()
            {
                Confirmed = true,
                Path = path,
                Mode = Options?.Mode ?? FileSelectorMode.SelectFile
            };
            CloseSelector();
        }

        public void Cancel()
        {
            if (!Visible)
                return;

            Result = new FileSelectorResult()
            {
                Confirmed = false,
                Path = string.Empty,
                Mode = Options?.Mode ?? FileSelectorMode.SelectFile
            };
            CloseSelector();
        }

        private void CloseSelector()
        {
            Visible = false;
            LockFocus = false;
            HideNewFolder();
            HideOverwrite();

            FileSelectorResult? result = Result;
            if (result != null)
                Closed?.Invoke(result);
        }

        private void ShowError(string message)
        {
            ErrorLabel.Text = message;
            ErrorLabel.Visible = !string.IsNullOrEmpty(message);
            if (ErrorLabel.Initialized)
                ErrorLabel.ProcessText();
        }

        private void ClearError()
        {
            ErrorLabel.Text = string.Empty;
            ErrorLabel.Visible = false;
        }

        private static void SetLabelText(UILabel label, string text)
        {
            label.Text = text;
            if (label.Initialized)
                label.ProcessText();
        }

        private static void SetButtonText(UIButton button, string text)
        {
            button.Text = text;
            if (button.Initialized)
                button.ProcessText();
        }

        public void CenterInParent()
        {
            int parentWidth = 0;
            int parentHeight = 0;

            if (Parent != null)
            {
                parentWidth = Parent.GetWidth();
                parentHeight = Parent.GetHeight();
            }
            else if (System != null)
            {
                parentWidth = System.GraphicsDevice.PresentationParameters.BackBufferWidth;
                parentHeight = System.GraphicsDevice.PresentationParameters.BackBufferHeight;
            }

            if (parentWidth <= 0 || parentHeight <= 0)
                return;

            int width = Math.Min(PreferredWidth, Math.Max(1, parentWidth - OuterMargin));
            int height = Math.Min(PreferredHeight, Math.Max(1, parentHeight - OuterMargin));

            if (Width != width || Height != height)
            {
                Width = width;
                Height = height;
                LayoutControls();
            }

            x = Math.Max(0, (parentWidth - Width) / 2);
            y = Math.Max(0, (parentHeight - Height) / 2);
        }

        private void CenterChildPanel(UIPanel panel)
        {
            panel.x = Math.Max(0, (Width - panel.Width) / 2);
            panel.y = Math.Max(0, (Height - panel.Height) / 2);
        }

        public override void SelfFocusedInput(float elapsedms, InputManager input)
        {
            base.SelfFocusedInput(elapsedms, input);
            UiElapsedMs += elapsedms;
            HandleKeys(input);
        }

        public override void SelfParentInput(float elapsedms, InputManager input)
        {
            base.SelfParentInput(elapsedms, input);
            UiElapsedMs += elapsedms;
            HandleKeys(input);
        }

        private void HandleKeys(InputManager input)
        {
            if (!Visible)
                return;

            if (input.IsKeyFresh(Keys.Escape))
            {
                input.CaptureInput(Keys.Escape);

                if (NewFolderPanel.Visible)
                    HideNewFolder();
                else if (OverwritePanel.Visible)
                    HideOverwrite();
                else
                    Cancel();

                return;
            }

            if (!input.IsKeyFresh(Keys.Enter)
                || FolderText.Focused
                || FileText.Focused
                || NewFolderText.Focused)
            {
                return;
            }

            input.CaptureInput(Keys.Enter);

            if (NewFolderPanel.Visible)
                CreateFolder();
            else if (OverwritePanel.Visible)
                ConfirmOverwrite();
            else
                Confirm();
        }
    }
}
