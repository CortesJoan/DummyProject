using System.Collections.Generic;
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
        if (availableDifficulties == null || availableDifficulties.Count == 0)
        {
            Debug.LogError("Cannot set difficulty because no difficulty levels are configured.");
            currentDifficulty = null;
            return;
        }

        int clampedIndex = Mathf.Clamp(levelIndex, 0, availableDifficulties.Count - 1);
        if (clampedIndex != levelIndex)
        {
            Debug.LogWarning($"Difficulty index {levelIndex} is out of range; using {clampedIndex}.");
        }

        currentDifficulty = availableDifficulties[clampedIndex];
    }

    public int GetCurrentDifficultyLevel()
    {
        if (availableDifficulties == null || availableDifficulties.Count == 0)
        {
            return 0;
        }

        int currentIndex = availableDifficulties.IndexOf(currentDifficulty);
        return currentIndex >= 0 ? currentIndex : 0;
    }

    public int GetNextDifficultyLevel()
    {
        if (availableDifficulties == null || availableDifficulties.Count == 0)
        {
            return 0;
        }

        return Mathf.Min(GetCurrentDifficultyLevel() + 1, availableDifficulties.Count - 1);
    }
}
