using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;

namespace Helpers
{
    public static class HelpFuncs
    {
        public static void MoveDirSafety(string sourceDirName, string destDirName)
        {
            try
            {
                Directory.Move(sourceDirName, destDirName);
            }
            catch
            {

            }
        }
        public static void DeleteDirSafety(string path, bool recursive=false)
        {
            try
            {
                Directory.Delete(path, recursive);
            }
            catch
            {

            }
        }
        public static void DeleteFileSafety(string path)
        {
            try
            {
                File.Delete(path);
            }
            catch
            {

            }
        }
        public static void ShowInNotebook(string text)
        {
            ShowInNotebook(text.Split('\n'));
        }
        public static void ShowInNotebook(IEnumerable<string> text)
        {
            string filePath = Environment.CurrentDirectory + "/~debug_buf_to_show.txt";
            File.WriteAllLines(filePath, text);
            Process proc = Process.Start(filePath);
            proc.WaitForExit();
            DeleteFileSafety(filePath);
        }
    }
}
