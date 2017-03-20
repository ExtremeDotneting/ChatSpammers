using Helpers;
using System.IO;
using System.Collections.Generic;

namespace ChatSpammers
{
    public class CacheDirManager
    {
        bool isWorkingNow = false;
        /// <summary>
        /// True if free.
        /// </summary>
        Dictionary<string, bool> dirNameAndStatus = new Dictionary<string, bool>();
        public string CacheFolderGlobal { get; private set; }

        public CacheDirManager(string cacheFolderGlobal)
        {
            CacheFolderGlobal = cacheFolderGlobal;
            if (!Directory.Exists(cacheFolderGlobal))
                Directory.CreateDirectory(cacheFolderGlobal);
            foreach (string dirPath in Directory.GetDirectories(cacheFolderGlobal))
            {
                dirNameAndStatus.Add(Path.GetFileName(dirPath), true);
            }
        }
        public string GetFreeDir()
        {
            Block();
            string res = null;
            foreach (var item in dirNameAndStatus)
            {
                if (item.Value)
                {
                    dirNameAndStatus[item.Key] = false;
                    res = Path.Combine(CacheFolderGlobal, item.Key);
                    break;
                }
            }
            if (res == null)
            {
                string dirName = RandomTextGenerator.Generate(7);
                res = Path.Combine(CacheFolderGlobal, dirName);
                Directory.CreateDirectory(res);
                dirNameAndStatus.Add(dirName, false);
            }
            Unblock();
            return res;
        }
        public void SetDirAsFree(string dirNameOrPath)
        {
            Block();
            dirNameOrPath = Path.GetFileName(dirNameOrPath);
            if (dirNameAndStatus.ContainsKey(dirNameOrPath))
                dirNameAndStatus[dirNameOrPath] = true;
            Unblock();
        }

        void Block()
        {
            while (isWorkingNow)
            {
                SynchronizationHelper.Pause(10);
            }
            isWorkingNow = true;
        }
        void Unblock()
        {
            isWorkingNow = false;
        }
    }
}
