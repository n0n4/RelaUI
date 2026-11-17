using System;

namespace RelaUI.Components.FileSelector
{
    public class FileSelectorOptions
    {
        public FileSelectorMode Mode = FileSelectorMode.SelectFile;
        public string StartFolder = string.Empty;
        public string DefaultFileName = string.Empty;
        public string Title = string.Empty;
        public string DefaultExtension = string.Empty;
        public string[] Extensions = [];
        public bool AllowNewFolder = true;
        public bool ConfirmOverwrite = true;
        public FileSelectorStrings Strings = new FileSelectorStrings();
    }
}
