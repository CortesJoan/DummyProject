using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem
{
    /// <summary>Test-only redirect so automated runs never touch the player's
    /// game.dat. Null (default) keeps the real persistent path.</summary>
    internal static string TestOnlySavePathOverride;

    public static void Save(List<ISavable> objectsToSave)
    {
        GameData gameData = new GameData();
        foreach (ISavable obj in objectsToSave)
        {
            obj?.SaveData(gameData);
        }

        SaveGame(gameData);
    }

    public static void Load(List<ISavable> objectsToLoad)
    {
        GameData gameData = LoadGame();
        if (gameData == null)
        {
            return;
        }

        foreach (ISavable obj in objectsToLoad)
        {
            obj?.LoadData(gameData);
        }
    }

    private static void SaveGame(GameData data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = GetSavePath();

        try
        {
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                formatter.Serialize(stream, data);
            }
        }
        catch (Exception exception)
        {
            Debug.LogError("Error saving game data: " + exception.Message);
        }
    }

    /// <summary>Real save path, for editor tooling that manages save slots. Never used by gameplay.</summary>
    public static string SavePath => GetSavePath();

    public static bool DeleteSave()
    {
        string path = GetSavePath();
        try
        {
            if (File.Exists(path))
                File.Delete(path);
            return true;
        }
        catch (Exception exception)
        {
            Debug.LogError("Error deleting game data: " + exception.Message);
            return false;
        }
    }

    private static string GetSavePath()
    {
        if (!string.IsNullOrEmpty(TestOnlySavePathOverride))
            return TestOnlySavePathOverride;
        return Path.Combine(Application.persistentDataPath, "game.dat");
    }

    private static GameData LoadGame()
    {
        string path = GetSavePath();
        if (!File.Exists(path))
        {
            Debug.LogWarning("Save file not found at: " + path);
            return null;
        }

        try
        {
            BinaryFormatter formatter = new BinaryFormatter();
            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                return formatter.Deserialize(stream) as GameData;
            }
        }
        catch (Exception exception)
        {
            Debug.LogWarning("Save data could not be loaded and will be ignored: " + exception.Message);
            return null;
        }
    }
}

public interface ISavable
{
    void SaveData(GameData data);
    void LoadData(GameData data);
}

[Serializable]
public class GameData
{
    private Dictionary<string, object> dataDictionary = new Dictionary<string, object>();

    public void SetData<T>(string key, T value)
    {
        if (dataDictionary.ContainsKey(key))
        {
            dataDictionary[key] = value;
        }
        else
        {
            dataDictionary.Add(key, value);
        }
    }

    public T GetData<T>(string key)
    {
        if (dataDictionary == null)
        {
            dataDictionary = new Dictionary<string, object>();
        }

        if (dataDictionary.ContainsKey(key) && dataDictionary[key] is T)
        {
            return (T)dataDictionary[key];
        }

        Debug.Log("GameData: Data for key '" + key + "' not found or of incorrect type. If this is the first launch, this is expected.");
        return default;
    }
}
