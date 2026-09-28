using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "New Card Set", menuName = "Card Match/Card Set Data", order = 1)]
public class CardSetData : ScriptableObject
{
    [Header("Card Set Info")]
    [SerializeField] private string setName;
    [SerializeField] private string setDescription;
    [SerializeField] private Sprite setIcon;

    [Header("Card Sprites")]
    [SerializeField] private List<AssetReferenceSprite> cardSprites = new List<AssetReferenceSprite>();

    [Header("Hidden Card Sprite")]
    [SerializeField] private AssetReferenceSprite hiddenCardSprite;

    public string SetName => setName;
    public string SetDescription => setDescription;
    public Sprite SetIcon => setIcon;
    public List<AssetReferenceSprite> CardSprites => cardSprites;
    public AssetReferenceSprite HiddenCardSprite => hiddenCardSprite;

    public int CardCount => cardSprites.Count;

    private void OnValidate()
    {
        if (cardSprites.Count == 0)
        {
            Debug.LogWarning($"Card Set '{setName}' has no card sprites assigned!");
        }

        if (hiddenCardSprite == null || !hiddenCardSprite.RuntimeKeyIsValid())
        {
            Debug.LogWarning($"Card Set '{setName}' has no hidden card sprite assigned!");
        }
    }
}
