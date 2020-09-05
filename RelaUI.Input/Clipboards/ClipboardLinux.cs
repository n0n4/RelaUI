using RelaUI.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RelaUI.Input.Clipboards
{
    public class ClipboardLinux : IClipboard
    {
        // here we're dumping the clipboard into a temp file,
        // then reading the temp file
        public string GetClipboard()
        {
            string tempPath = Path.GetTempFileName();

            try
            {
                Bash.Run("xclip -o > " + tempPath);
                return FileUtility.ReadFromFile(tempPath);
            }
            catch (Exception e)
            {
                // bubble up a more descriptive error
                throw new Exception("Failed to get Linux Clipboard", e);
            }
            finally
            {
                File.Delete(tempPath);
            }
        }

        // here we dump our string into a temp file,
        // then load the clipboard from that file
        public void SetClipboard(string s)
        {
            string tempPath = Path.GetTempFileName();
            FileUtility.WriteToFile(
                tempPath,
                s);

            try
            {
                Bash.Run("cat " + tempPath + " | xclip");
            }
            catch (Exception e)
            {
                // bubble up a more descriptive error
                throw new Exception("Failed to set Linux Clipboard", e);
            }
            finally
            {
                File.Delete(tempPath);
            }
        }
    }
}
