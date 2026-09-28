using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using System.Threading.Tasks;

/// <summary>
/// Manages loading and unloading of card sets using Unity Addressables.
/// Follows the Singleton pattern for easy access across the game.
/// </summary>
public class CardSetManager : MonoBehaviour
{
    public static CardSetManager Instance { get; private set; }

    [Header("Card Set Configuration")]
    [SerializeField] private AssetReference defaultCardSetReference;
    [SerializeField] private List<AssetReference> availableCardSets = new List<AssetReference>();

    private CardSetData currentCardSet;
    private List<Sprite> loadedCardSprites = new List<Sprite>();
    private Sprite loadedHiddenSprite;
    
    private Dictionary<string, AsyncOperationHandle> activeHandles = new Dictionary<string, AsyncOperationHandle>();

    public event Action<CardSetData> OnCardSetLoaded;
    public event Action OnCardSetUnloaded;

    public CardSetData CurrentCardSet => currentCardSet;
    public List<Sprite> LoadedCardSprites => loadedCardSprites;
    public Sprite LoadedHiddenSprite => loadedHiddenSprite;
    public bool IsCardSetLoaded => currentCardSet != null && loadedCardSprites.Count > 0;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        UnloadCurrentCardSet();
    }

    /// <summary>
    /// Loads a card set asynchronously using its Addressable reference.
    /// </summary>
    public async Task<bool> LoadCardSetAsync(AssetReference cardSetReference)
    {
        if (cardSetReference == null || !cardSetReference.RuntimeKeyIsValid())
        {
            Debug.LogError("Invalid card set reference!");
            return false;
        }

        UnloadCurrentCardSet();

        try
        {
            // Load the CardSetData ScriptableObject
            AsyncOperationHandle<CardSetData> cardSetHandle = cardSetReference.LoadAssetAsync<CardSetData>();
            await cardSetHandle.Task;

            if (cardSetHandle.Status != AsyncOperationStatus.Succeeded)
            {
                Debug.LogError($"Failed to load card set: {cardSetReference.Asset}");
                Addressables.Release(cardSetHandle);
                return false;
            }

            currentCardSet = cardSetHandle.Result;
            activeHandles["CardSetData"] = cardSetHandle;

            // Load all card sprites
            loadedCardSprites.Clear();
            for (int i = 0; i < currentCardSet.CardSprites.Count; i++)
            {
                var spriteRef = currentCardSet.CardSprites[i];
                if (spriteRef != null && spriteRef.RuntimeKeyIsValid())
                {
                    AsyncOperationHandle<Sprite> spriteHandle = spriteRef.LoadAssetAsync<Sprite>();
                    await spriteHandle.Task;

                    if (spriteHandle.Status == AsyncOperationStatus.Succeeded)
                    {
                        loadedCardSprites.Add(spriteHandle.Result);
                        activeHandles[$"CardSprite_{i}"] = spriteHandle;
                    }
                    else
                    {
                        Debug.LogWarning($"Failed to load card sprite {i} from {currentCardSet.SetName}");
                        Addressables.Release(spriteHandle);
                    }
                }
            }

            // Load hidden card sprite
            if (currentCardSet.HiddenCardSprite != null && currentCardSet.HiddenCardSprite.RuntimeKeyIsValid())
            {
                AsyncOperationHandle<Sprite> hiddenHandle = currentCardSet.HiddenCardSprite.LoadAssetAsync<Sprite>();
                await hiddenHandle.Task;

                if (hiddenHandle.Status == AsyncOperationStatus.Succeeded)
                {
                    loadedHiddenSprite = hiddenHandle.Result;
                    activeHandles["HiddenSprite"] = hiddenHandle;
                }
                else
                {
                    Debug.LogWarning($"Failed to load hidden sprite for {currentCardSet.SetName}");
                    Addressables.Release(hiddenHandle);
                }
            }

            Debug.Log($"Card set '{currentCardSet.SetName}' loaded successfully with {loadedCardSprites.Count} sprites.");
            OnCardSetLoaded?.Invoke(currentCardSet);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading card set: {ex.Message}");
            UnloadCurrentCardSet();
            return false;
        }
    }

    /// <summary>
    /// Loads a card set by index from the available card sets list.
    /// </summary>
    public async Task<bool> LoadCardSetByIndexAsync(int index)
    {
        if (index < 0 || index >= availableCardSets.Count)
        {
            Debug.LogError($"Card set index {index} out of range!");
            return false;
        }

        return await LoadCardSetAsync(availableCardSets[index]);
    }

    /// <summary>
    /// Loads the default card set.
    /// </summary>
    public async Task<bool> LoadDefaultCardSetAsync()
    {
        if (defaultCardSetReference == null || !defaultCardSetReference.RuntimeKeyIsValid())
        {
            Debug.LogError("No default card set configured!");
            return false;
        }

        return await LoadCardSetAsync(defaultCardSetReference);
    }

    /// <summary>
    /// Unloads the current card set and releases all Addressable assets.
    /// </summary>
    public void UnloadCurrentCardSet()
    {
        foreach (var handle in activeHandles.Values)
        {
            if (handle.IsValid())
            {
                Addressables.Release(handle);
            }
        }

        activeHandles.Clear();
        loadedCardSprites.Clear();
        loadedHiddenSprite = null;
        currentCardSet = null;

        OnCardSetUnloaded?.Invoke();
        Debug.Log("Card set unloaded successfully.");
    }

    /// <summary>
    /// Gets the number of available card sets.
    /// </summary>
    public int GetAvailableCardSetCount()
    {
        return availableCardSets.Count;
    }

    /// <summary>
    /// Validates if the current card set can support a given grid size.
    /// </summary>
    public bool CanSupportGridSize(int rows, int columns)
    {
        if (!IsCardSetLoaded)
        {
            Debug.LogWarning("No card set is currently loaded!");
            return false;
        }

        int requiredUniqueCards = (rows * columns) / 2;
        return loadedCardSprites.Count >= requiredUniqueCards;
    }

    /// <summary>
    /// Preloads all available card sets for quick switching (optional).
    /// Use with caution as this can consume significant memory.
    /// </summary>
    public async Task PreloadAllCardSetsAsync()
    {
        Debug.Log("Preloading is not recommended for card sets. Load them on-demand instead.");
        await Task.CompletedTask;
    }
}
