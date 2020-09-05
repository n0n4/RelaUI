using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace RelaUI.Utilities
{
    public class FileUtility
    {
        public static string ReadFromFile(string path)
        {
            FileInfo fi = new FileInfo(path);
            if (!fi.Exists)
            {
                return string.Empty;
            }
            string results = string.Empty;
            using (StreamReader sr = new StreamReader(path))
            {
                results += sr.ReadToEnd();
            }
            return results;
        }
        public static List<string> ReadLinesFromFile(string path)
        {
            FileInfo fi = new FileInfo(path);
            if (!fi.Exists)
            {
                return new List<string>();
            }
            List<string> results = new List<string>();
            using (StreamReader sr = new StreamReader(path))
            {
                while (!sr.EndOfStream)
                {
                    results.Add(sr.ReadLine());
                }
            }
            return results;
        }
        public static byte[] ReadBinaryFromFile(string path)
        {
            FileInfo fi = new FileInfo(path);
            if (!fi.Exists)
            {
                return new byte[0];
            }
            byte[] bs = File.ReadAllBytes(path);
            return bs;
        }


        public static void WriteLinesToFile(string path, List<string> data)
        {
            WriteToFile(path, String.Join("\r\n", data.ToArray()));
        }
        public static void WriteToFile(string path, string data)
        {
            // if the directory doesn't exist, create it
            string dir = Path.GetDirectoryName(path);//path.Replace(Path.GetFileName(path), "");
            if (!Directory.Exists(dir))
            {
                Directory.CreateDirectory(dir);
            }
            // if the file exists already, delete it
            FileInfo fi = new FileInfo(path);
            if (fi.Exists)
            {
                fi.Delete();
            }
            // now write the file
            using (StreamWriter sw = new StreamWriter(path))
            {
                sw.Write(data);
            }
        }
    }
}
