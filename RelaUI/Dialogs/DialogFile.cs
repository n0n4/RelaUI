using RelaUI.Components;
using RelaUI.Input;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace RelaUI.Dialogs
{
    public class DialogFile : DialogBase
    {
        public enum eMode
        {
            SELECT_FILE,
            SELECT_FOLDER,
            CREATE_FILE,
        }

        public eMode Mode = eMode.CREATE_FILE;
        public string DefaultFolderPath = string.Empty;
        public string DefaultFileName = string.Empty;

        public string Title = string.Empty;
        public int TitleFontSize = 16;

        public UITextField FolderText;

        public UIPanel FileListPanel;

        public UITextField FileText;

        public UIButton NewFolderButton;
        public UIButton SelectButton;
        public UIButton CancelButton;

        public bool Result = false; // if false, user cancelled
        public string ResultPath = string.Empty;

        public Dictionary<eMode, string> SelectNames = new Dictionary<eMode, string>()
        {
            { eMode.CREATE_FILE, "Create" },
            { eMode.SELECT_FILE, "Select" },
            { eMode.SELECT_FOLDER, "Select Folder" },
        };

        public int Width = 0;
        public int Height = 0;

        public DialogFile(eMode mode, string defaultFolderPath, string defaultFileName, string title, int titlefontsize = 16)
        {
            Mode = mode;
            Title = title;
            TitleFontSize = titlefontsize;
            DefaultFolderPath = defaultFolderPath;
            DefaultFileName = defaultFileName;
        }

        protected override void Construct()
        {
            Tuple<float, float> cpos = GetCenterOfParent();

            int width = ((int)cpos.Item2 * 5 / 3);
            int height = 400;

            Width = width;
            Height = height;

            Panel = new UIPanel(cpos.Item1 - width / 2, cpos.Item2 - height / 2, width, height,
                hastitle: !string.IsNullOrEmpty(Title), title: Title, titlesize: TitleFontSize);
            Panel.LockFocus = true;

            UIAutoList tops = new UIAutoList(eUIOrientation.HORIZONTAL, width - 10, 40)
            {
                HasBorder = false,
                HasOuterBorder = false,
                MarginX = 0,
                MarginY = 0,
            };
            Panel.AddAuto(tops);

            UIButton upbtn = new UIButton(0, 0, 50, 30, "Up");
            upbtn.EventFocused += (sender, args) =>
            {
                var eargs = args as UITextField.EventFocusedHandlerArgs;
                try
                {
                    DisplayFolder(Directory.GetParent(FolderText.Text).FullName, eargs.ElapsedMS, eargs.Input);
                }
                catch
                {
                    // just don't do anything
                }

                if (ParentSystem != null)
                    ParentSystem.ProposedFocus = FileListPanel;
            };
            tops.AddAuto(upbtn);

            FolderText = new UITextField(0, 0, width - 50 - 10, 30, DefaultFolderPath, fontsize: 12, autoheight: false);
            FolderText.EventEnterPressed += (sender, args) =>
            {
                var eargs = args as UITextField.EventEnterPressedHandlerArgs;
                DisplayFolder(FolderText.Text, eargs.ElapsedMS, eargs.Input);
            };
            tops.AddAuto(FolderText);

            FileListPanel = new UIPanel(0, 5, width - 10, height - 35 - 110, hasscrolling: true, scrollh: height - 30 - 110);
            Panel.AddAuto(FileListPanel);

            FileText = new UITextField(0, 5, width - 10, 30, DefaultFileName, fontsize: 12, autoheight: false);
            FileText.EventEnterPressed += (sender, args) =>
            {
                var eargs = args as UITextField.EventEnterPressedHandlerArgs;
                OKResult(eargs.ElapsedMS, eargs.Input);
            };
            if (Mode != eMode.SELECT_FOLDER)
                Panel.AddAuto(FileText);

            UIAutoList bottomButtons = new UIAutoList(eUIOrientation.HORIZONTAL, width - 10, 50)
            {
                LeftAlign = true,
                HasBorder = false,
                HasOuterBorder = false,
                MarginX = 10,
                MarginY = 5,
                y = 5,
            };
            Panel.AddAuto(bottomButtons);

            NewFolderButton = new UIButton(0, 5, 100, 35, "New Folder");
            bottomButtons.AddAuto(NewFolderButton);
            SelectButton = new UIButton(0, 5, 100, 35, SelectNames[Mode]);
            bottomButtons.AddAuto(SelectButton);
            CancelButton = new UIButton(0, 5, 100, 35, "Cancel");
            bottomButtons.AddAuto(CancelButton);

            NewFolderButton.EventFocused += (sender, e) =>
            {
                UIComponent.EventFocusedHandlerArgs eargs = e as UIComponent.EventFocusedHandlerArgs;
                DialogTextInput dialogTextInput = new DialogTextInput("New Folder", "Name the new folder:", "", "Folder Name...");
                dialogTextInput.Popup(ParentSystem);

                dialogTextInput.EventClosedHandler += (innersender, innerargs) =>
                {
                    var innereargs = innerargs as DialogBase.EventClosedHandlerArgs;
                    // only process if OK was selected
                    if (dialogTextInput.Result)
                    {
                        string folderName = dialogTextInput.ResultText;
                        string folderPath = string.Empty;
                        bool failed = false;
                        // try to create this folder
                        try
                        {
                            folderPath = Path.Combine(FolderText.Text, folderName);
                            Directory.CreateDirectory(folderPath);
                        }
                        catch (Exception ex)
                        {
                            failed = true;
                            DialogPopup errorPopup = new DialogPopup("Failed to Create Folder", "Folder could not be created: " + ex.Message);
                            errorPopup.Popup(ParentSystem);
                        }

                        if (!failed)
                            DisplayFolder(folderPath, innereargs.ElapsedMS, innereargs.Input);
                    }
                };
            };
            SelectButton.EventFocused += (sender, e) =>
            {
                UIComponent.EventFocusedHandlerArgs eargs = e as UIComponent.EventFocusedHandlerArgs;
                OKResult(eargs.ElapsedMS, eargs.Input);
            };
            CancelButton.EventFocused += (sender, e) =>
            {
                UIComponent.EventFocusedHandlerArgs eargs = e as UIComponent.EventFocusedHandlerArgs;
                CancelResult(eargs.ElapsedMS, eargs.Input);
            };

            DisplayFolder(DefaultFolderPath, 0, null, skipinit: true);

            if (ParentSystem != null)
                ParentSystem.ProposedFocus = FileListPanel;
        }

        public void OKResult(float elapsedms, InputManager input)
        {
            Result = true;
            ResultPath = Path.Combine(FolderText.Text, FileText.Text);
            Close(elapsedms, input);
        }
        public void CancelResult(float elapsedms, InputManager input)
        {
            Result = false;
            Close(elapsedms, input);
        }

        private void SetFolderText(string path, float elapsedms, InputManager input)
        {
            if (path.EndsWith("\\"))
                path = path.Substring(0, path.Length - 1);
            FolderText.SetText(path, elapsedms, input);
            FolderText.CaretPosition = path.Length - 1;
        }

        private void DisplayFolder(string folderPath, float elapsedms, InputManager input, bool skipinit = false)
        {
            // clear all items in the file list panel
            FileListPanel.RemoveAll();
            FileListPanel.ScrollCurrentY = 0;

            // find the given folder
            DirectoryInfo dir = new DirectoryInfo(folderPath);
            if (!dir.Exists)
                return;

            SetFolderText(dir.FullName, elapsedms, input);

            List<DirectoryInfo> subdirs = dir.EnumerateDirectories().ToList();
            List<FileInfo> subfiles = dir.EnumerateFiles().ToList();

            int maxheight = subdirs.Count * 30 + subfiles.Count * 30 + 1;
            FileListPanel.ScrollHeight = maxheight;

            // find each folder in the given folderpath and display it in the file list
            foreach (var subdir in subdirs)
            {
                UIButton dirbtn = new UIButton(0, 0, FileListPanel.Width - 10, 30, "[ ] " + subdir.Name)
                {
                    Justified = eUIJustify.LEFT
                };
                dirbtn.EventPostInit += (sender, args) =>
                {
                    dirbtn.HasBorder = false;
                };
                dirbtn.EventFocused += (sender, e) =>
                {
                    UIComponent.EventFocusedHandlerArgs eargs = e as UIComponent.EventFocusedHandlerArgs;
                    DisplayFolder(subdir.FullName, eargs.ElapsedMS, eargs.Input);
                };
                FileListPanel.AddAuto(dirbtn);
                if (!skipinit)
                    dirbtn.Init();
            }

            // find each file in the given folderpath and dsiplay it in the file list
            foreach (var subfile in subfiles)
            {
                UIButton filebtn = new UIButton(0, 0, FileListPanel.Width - 10, 30, " # " + subfile.Name)
                {
                    Justified = eUIJustify.LEFT
                };
                filebtn.EventPostInit += (sender, args) =>
                {
                    filebtn.HasBorder = false;
                    filebtn.HoverBackgroundOnly = true;
                };
                filebtn.EventFocused += (sender, e) =>
                {
                    UIComponent.EventFocusedHandlerArgs eargs = e as UIComponent.EventFocusedHandlerArgs;
                    SelectFile(subfile.Name, eargs.Input, eargs.ElapsedMS);
                };
                if (Mode == eMode.SELECT_FOLDER)
                    filebtn.Enabled = false;
                FileListPanel.AddAuto(filebtn);
                if (!skipinit)
                    filebtn.Init();
            }

            // call panel init again to properly space all the components
            if (!skipinit)
                FileListPanel.Init();
        }

        private void SelectFile(string fileName, InputManager input, float elapsedms)
        {
            FileText.SetText(fileName, elapsedms, input);
        }
    }
}
