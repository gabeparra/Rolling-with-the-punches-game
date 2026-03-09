using System.IO;
using UnityEngine;

public static class SaveSystem
{
    private static readonly string SAVE_FOLDER = Application.dataPath + "/Saves/";
    private static readonly string SAVE_FILE = SAVE_FOLDER + "save.txt";

    public static void Init()
    {
        //test if save folder exists
        if (!Directory.Exists(SAVE_FOLDER))
        {
            Directory.CreateDirectory(SAVE_FOLDER);
        }
    }

    public static void Save(string saveString)
    {
        File.WriteAllText(SAVE_FILE, saveString);
    }

    public static string Load()
    {
        if(!File.Exists(SAVE_FILE))
            return null;

        return File.ReadAllText(SAVE_FILE);
    }
}
