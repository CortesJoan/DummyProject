using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// Configuration asset that holds references to all available card sets.
/// This makes it easy to manage card sets from a central location.
/// </summary>
[CreateAssetMenu(fileName = "CardSetConfiguration", menuName = "Card Match/Card Set Configuration", order = 0)]
public class CardSetConfiguration : ScriptableObject
{
    [Header("Card Set Collection")]
    [SerializeField] private List<CardSetInfo> cardSets = new List<CardSetInfo>();

    [Header("Default Settings")]
    [SerializeField] private int defaultCardSetIndex = 0;

    public List<CardSetInfo> CardSets => cardSets;
    public int DefaultCardSetIndex => defaultCardSetIndex;

    /// <summary>
    /// Gets a card set reference by index.
    /// </summary>
    public AssetReference GetCardSetReference(int index)
    {
        if (index < 0 || index >= cardSets.Count)
        {
            Debug.LogError($"Card set index {index} out of range!");
            return null;
        }

        return cardSets[index].cardSetReference;
    }

    /// <summary>
    /// Gets the default card set reference.
    /// </summary>
    public AssetReference GetDefaultCardSetReference()
    {
        return GetCardSetReference(defaultCardSetIndex);
    }

    /// <summary>
    /// Gets all card set references as a list.
    /// </summary>
    public List<AssetReference> GetAllCardSetReferences()
    {
        List<AssetReference> references = new List<AssetReference>();
        foreach (var cardSet in cardSets)
        {
            if (cardSet.cardSetReference != null && cardSet.cardSetReference.RuntimeKeyIsValid())
            {
                references.Add(cardSet.cardSetReference);
            }
        }
        return references;
    }

    /// <summary>
    /// Finds a card set index by name.
    /// </summary>
    public int FindCardSetIndexByName(string setName)
    {
        for (int i = 0; i < cardSets.Count; i++)
        {
            if (cardSets[i].displayName.Equals(setName, System.StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }
        return -1;
    }

    private void OnValidate()
    {
        if (defaultCardSetIndex < 0 || defaultCardSetIndex >= cardSets.Count)
        {
            Debug.LogWarning("Default card set index is out of range!");
            defaultCardSetIndex = Mathf.Clamp(defaultCardSetIndex, 0, Mathf.Max(0, cardSets.Count - 1));
        }
    }
}

/// <summary>
/// Represents information about a card set including its reference and metadata.
/// </summary>
[System.Serializable]
public class CardSetInfo
{
    [Tooltip("Display name for the card set")]
    public string displayName;

    [Tooltip("Category or theme of the card set")]
    public string category;

    [Tooltip("Addressable reference to the CardSetData asset")]
    public AssetReference cardSetReference;

    [Tooltip("Is this card set unlocked and available to use?")]
    public bool isUnlocked = true;

    [Tooltip("Minimum difficulty level required to use this card set")]
    public int minimumDifficultyLevel = 0;
}
