using System;
using System.Collections.Generic;
using System.IO;


public class ResourcesAndConsts
{
    static ResourcesAndConsts instance;

    public static ResourcesAndConsts Instance()
    {
        if (instance == null)
            instance = new ResourcesAndConsts();
        return instance;
    }

    ResourcesAndConsts()
    {
        List<string> directories = new List<string>();
        directories.Add(FolderForСorrespondenceAndLogs);
        directories.Add(FolderForCache);

        foreach (string dirPath in directories)
        {
            if (!Directory.Exists(dirPath))
            {
                Directory.CreateDirectory(dirPath);
            }
        }
    }

    public string FolderForСorrespondenceAndLogs = Environment.CurrentDirectory + "/DialogLogs";
    public string FolderForCache = Environment.CurrentDirectory + "/BrowserCache";
    string jsLib_JqueryKeypressSimulator;
    public string JsLib_JqueryKeypressSimulator
    {
        get
        {
            if (jsLib_JqueryKeypressSimulator == null)
                jsLib_JqueryKeypressSimulator = File.ReadAllText(Environment.CurrentDirectory + "/jqueryKeypressSimulator.js");
            return jsLib_JqueryKeypressSimulator;
        }
    }

}

