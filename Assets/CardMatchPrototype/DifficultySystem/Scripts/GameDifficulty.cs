using UnityEngine;
using UnityEngine.AddressableAssets;

[CreateAssetMenu(fileName = "New GameDifficulty", menuName = "Game/Difficulty", order = 1)] 
public class GameDifficulty : ScriptableObject
{
    public string difficultyName;
    public Vector2Int gridSize; 
    public Color backgroundColor;
    public int timeToSeeCards;
    [Header("Score:")]
    public int baseScore;
    public int scoreComboMultiplier;
    [Header("Card Set (Optional)")]
    [Tooltip("If set, this difficulty will use a specific card set. Leave empty to use the currently loaded card set.")]
    public AssetReference specificCardSet;
}
 