using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

/// <summary>
/// Utility class providing helper methods for card set operations.
/// </summary>
public static class CardSetUtility
{
    /// <summary>
    /// Validates if a card set has enough sprites for a given grid configuration.
    /// </summary>
    public static bool ValidateCardSetForGrid(int totalSprites, int rows, int columns, int cardsPerMatch = 2)
    {
        int totalCards = rows * columns;
        
        if (totalCards % cardsPerMatch != 0)
        {
            Debug.LogError($"Grid size {rows}x{columns} cannot be evenly divided by {cardsPerMatch} cards per match!");
            return false;
        }

        int uniqueCardsNeeded = totalCards / cardsPerMatch;
        
        if (totalSprites < uniqueCardsNeeded)
        {
            Debug.LogError($"Not enough sprites! Need {uniqueCardsNeeded} unique sprites, but only have {totalSprites}.");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Calculates the maximum grid size that can be supported by a given number of sprites.
    /// </summary>
    public static Vector2Int CalculateMaxGridSize(int totalSprites, int cardsPerMatch = 2)
    {
        int maxCards = totalSprites * cardsPerMatch;
        
        // Try to make a square or near-square grid
        int side = Mathf.FloorToInt(Mathf.Sqrt(maxCards));
        
        // Ensure it's even for matching
        if (side % 2 != 0)
            side--;

        return new Vector2Int(side, side);
    }

    /// <summary>
    /// Generates a shuffled list of sprites for card matching.
    /// </summary>
    public static List<Sprite> GenerateShuffledCardList(List<Sprite> sourceSprites, int rows, int columns, int cardsPerMatch = 2)
    {
        int totalCards = rows * columns;
        int uniqueCardsNeeded = totalCards / cardsPerMatch;

        if (sourceSprites.Count < uniqueCardsNeeded)
        {
            Debug.LogError($"Not enough source sprites to generate card list!");
            return new List<Sprite>();
        }

        List<Sprite> cardList = new List<Sprite>();

        // Add each sprite the required number of times
        for (int i = 0; i < uniqueCardsNeeded; i++)
        {
            for (int j = 0; j < cardsPerMatch; j++)
            {
                cardList.Add(sourceSprites[i]);
            }
        }

        // Shuffle the list
        ShuffleList(cardList);

        return cardList;
    }

    /// <summary>
    /// Shuffles a list using Fisher-Yates algorithm.
    /// </summary>
    public static void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int randomIndex = Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    /// <summary>
    /// Creates a deep copy of a sprite list.
    /// </summary>
    public static List<Sprite> CloneSpriteList(List<Sprite> source)
    {
        return new List<Sprite>(source);
    }

    /// <summary>
    /// Validates an AssetReference to ensure it's properly configured.
    /// </summary>
    public static bool ValidateAssetReference(AssetReference assetRef, string assetName = "Asset")
    {
        if (assetRef == null)
        {
            Debug.LogWarning($"{assetName} reference is null!");
            return false;
        }

        if (!assetRef.RuntimeKeyIsValid())
        {
            Debug.LogWarning($"{assetName} reference has an invalid runtime key!");
            return false;
        }

        return true;
    }

    /// <summary>
    /// Gets difficulty recommendation based on card set size.
    /// </summary>
    public static string GetDifficultyRecommendation(int spriteCount)
    {
        if (spriteCount < 4)
            return "Very Easy (2x2 grid)";
        else if (spriteCount < 8)
            return "Easy (2x4 or 4x4 grid)";
        else if (spriteCount < 12)
            return "Medium (4x4 or 4x6 grid)";
        else if (spriteCount < 18)
            return "Hard (6x6 grid)";
        else
            return "Very Hard (6x6+ grid)";
    }
}
