using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DifficultyHandler : MonoBehaviour
{
    [SerializeField] private List<GameDifficulty> availableDifficulties;
    private GameDifficulty currentDifficulty;



    public GameDifficulty GetCurrentDifficulty()
    {
        return currentDifficulty;
    }
    public void SetCurrentDifficultyLevel(int levelIndex)
    {
        if (levelIndex >= 0 && levelIndex < availableDifficulties.Count)
        {
            currentDifficulty = availableDifficulties[levelIndex];
        }
        else
        {
            Debug.LogError($"Invalid difficulty index.There are {availableDifficulties.Count} difficulties. Setting max difficulty instead");
            currentDifficulty = availableDifficulties.Last();
        }
    }
    public int GetCurrentDifficultyLevel()
    {
        return availableDifficulties.IndexOf(currentDifficulty);
    }
    public int GetNextDifficultyLevel()
    {
        return GetCurrentDifficultyLevel() + 1;
    }
}