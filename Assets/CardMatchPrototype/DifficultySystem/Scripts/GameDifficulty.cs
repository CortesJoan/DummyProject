using UnityEngine; 

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
}
 