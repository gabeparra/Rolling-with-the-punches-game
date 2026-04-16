using System;
using System.IO;
using UnityEngine;

// Original by Rae, persistentDataPath fix, auto-init, and try-catch added by Gabriel
public static class SaveSystem
{
    private static readonly string SAVE_FOLDER = Application.persistentDataPath + "/Saves/";
    private static readonly string SAVE_FILE = SAVE_FOLDER + "save.txt";
    private static bool _initialized = false;

    private static void EnsureInitialized()
    {
        if (_initialized) return;
        _initialized = true;
        if (!Directory.Exists(SAVE_FOLDER))
            Directory.CreateDirectory(SAVE_FOLDER);
    }

    public static void Init() => EnsureInitialized();

    public static void Save(string saveString)
    {
        EnsureInitialized();
        try
        {
            File.WriteAllText(SAVE_FILE, saveString);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to save: {e.Message}");
        }
    }

    public static string Load()
    {
        EnsureInitialized();
        try
        {
            if (!File.Exists(SAVE_FILE))
                return null;
            return File.ReadAllText(SAVE_FILE);
        }
        catch (Exception e)
        {
            Debug.LogError($"[SaveSystem] Failed to load: {e.Message}");
            return null;
        }
    }
}
