using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

public static class SaveSystem 
{ 
     public static void Save(List<ISavable> objectsToSave)
    {
        GameData gameData = new GameData();
        foreach (var obj in objectsToSave)
        {
            obj.SaveData(gameData);
        }

        SaveGame(gameData);
    }

    public static void Load(List<ISavable> objectsToLoad)
    {
        GameData gameData = LoadGame();
        if (gameData != null)
        {
            foreach (var obj in objectsToLoad)
            {
                obj.LoadData(gameData);
            }
        }

    }
    private static void SaveGame(GameData data)
    {
        BinaryFormatter formatter = new BinaryFormatter();
        string path = Application.persistentDataPath + "/game.dat";

        try
        {
            using (FileStream stream = new FileStream(path, FileMode.Create))
            {
                formatter.Serialize(stream, data);
            }
        }
        catch (Exception e)
        {
            Debug.LogError("Error saving game data: " + e.Message);
       
        }
    }

    private static GameData LoadGame()
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
            Debug.LogWarning("Save file not found at: " + path);
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

        if(dataDictionary.ContainsKey(key) && dataDictionary[key] is T)
        {
            return (T)dataDictionary[key];
        }
        else
        {
            Debug.Log("GameData: Data for key '" + key + "' not found or of incorrect type. If is the first time launching the game don't worry about this");
            return default(T);
        }
    }
}