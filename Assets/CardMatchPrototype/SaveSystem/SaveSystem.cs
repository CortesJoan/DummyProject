using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem 
{
    // ... (Your binary serialization logic from before, in SaveGame and LoadGame) ...

    // Generic method to save/load any ISavable object 
    public static void Save<T>(T objectToSave) where T : ISavable
    { 
        GameData data = new GameData();
        objectToSave.SaveData(data); // Let the object save its specific data
        SaveGame(data); 
    }

    public static void Load<T>(T objectToLoad) where T : ISavable 
    {
        GameData data = LoadGame();
        if (data != null)
        {
            objectToLoad.LoadData(data); // Let the object load its specific data 
        }
        // ... (Error handling for no save file)
    }
    public static void SaveGame(GameData data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/game.dat";

        // Use a "using" block to ensure the FileStream is disposed properly 
        using (FileStream stream = new FileStream(path, FileMode.Create))
        {
            formatter.Serialize(stream, data);
        }
    }

    public static GameData LoadGame()
    {
        string path = Application.persistentDataPath + "/game.dat";

        if (File.Exists(path))
        {
            BinaryFormatter formatter = new BinaryFormatter();

            using (FileStream stream = new FileStream(path, FileMode.Open))
            {
                GameData data = formatter.Deserialize(stream) as GameData;
                return data;
            }
        }
        else
        {
            Debug.LogWarning("Save file not found in " + path);
            return null; // or new GameData(); depending on your error handling
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
    public int score;
    public int totalMatches;
    public int turns;
}