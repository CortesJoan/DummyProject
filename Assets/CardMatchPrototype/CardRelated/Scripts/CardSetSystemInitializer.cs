using UnityEngine;
using UnityEngine.AddressableAssets;

/// <summary>
/// Example initializer for the Card Set System.
/// Add this to your game's startup scene or main menu controller.
/// </summary>
public class CardSetSystemInitializer : MonoBehaviour
{
    [Header("Initialization Settings")]
    [SerializeField] private bool loadDefaultSetOnStart = true;
    [SerializeField] private bool showDebugLogs = true;
    
    [Header("Optional Configuration")]
    [SerializeField] private CardSetConfiguration cardSetConfig;
    
    private bool isInitialized = false;

    private async void Start()
    {
        await InitializeCardSetSystem();
    }

    /// <summary>
    /// Initializes the card set system and loads the default card set.
    /// </summary>
    public async System.Threading.Tasks.Task InitializeCardSetSystem()
    {
        if (isInitialized)
        {
            LogDebug("Card Set System already initialized.");
            return;
        }

        LogDebug("Initializing Card Set System...");

        // Ensure CardSetManager exists
        if (CardSetManager.Instance == null)
        {
            Debug.LogError("CardSetManager not found in scene! Please add it to the scene or let it auto-create.");
            return;
        }

        // Subscribe to events
        CardSetManager.Instance.OnCardSetLoaded += OnCardSetLoaded;
        CardSetManager.Instance.OnCardSetUnloaded += OnCardSetUnloaded;

        // Load default card set if configured
        if (loadDefaultSetOnStart)
        {
            bool success = false;

            if (cardSetConfig != null)
            {
                LogDebug("Loading default card set from configuration...");
                var defaultRef = cardSetConfig.GetDefaultCardSetReference();
                success = await CardSetManager.Instance.LoadCardSetAsync(defaultRef);
            }
            else
            {
                LogDebug("Loading default card set from manager...");
                success = await CardSetManager.Instance.LoadDefaultCardSetAsync();
            }

            if (success)
            {
                LogDebug("Default card set loaded successfully!");
                isInitialized = true;
            }
            else
            {
                Debug.LogError("Failed to load default card set!");
            }
        }
        else
        {
            LogDebug("Skipping default card set load (disabled in settings).");
            isInitialized = true;
        }
    }

    private void OnCardSetLoaded(CardSetData cardSet)
    {
        LogDebug($"Card Set Loaded Event: {cardSet.SetName}");
        LogDebug($"  - Cards Available: {CardSetManager.Instance.LoadedCardSprites.Count}");
        LogDebug($"  - Hidden Sprite: {(CardSetManager.Instance.LoadedHiddenSprite != null ? "✓" : "✗")}");
        
        // You can trigger other game systems here
        // Example: UpdateMainMenuUI();
    }

    private void OnCardSetUnloaded()
    {
        LogDebug("Card Set Unloaded Event");
    }

    private void OnDestroy()
    {
        // Unsubscribe from events
        if (CardSetManager.Instance != null)
        {
            CardSetManager.Instance.OnCardSetLoaded -= OnCardSetLoaded;
            CardSetManager.Instance.OnCardSetUnloaded -= OnCardSetUnloaded;
        }
    }

    private void LogDebug(string message)
    {
        if (showDebugLogs)
        {
            Debug.Log($"[CardSetInit] {message}");
        }
    }

    #region Public API for Testing/Debugging

    /// <summary>
    /// Manually loads a card set by index (for testing).
    /// </summary>
    public async void LoadCardSetByIndex(int index)
    {
        if (CardSetManager.Instance == null) return;

        LogDebug($"Loading card set at index {index}...");
        await CardSetManager.Instance.LoadCardSetByIndexAsync(index);
    }

    /// <summary>
    /// Shows information about the currently loaded card set.
    /// </summary>
    public void ShowCurrentCardSetInfo()
    {
        if (CardSetManager.Instance == null || !CardSetManager.Instance.IsCardSetLoaded)
        {
            Debug.Log("No card set currently loaded.");
            return;
        }

        var cardSet = CardSetManager.Instance.CurrentCardSet;
        var sprites = CardSetManager.Instance.LoadedCardSprites;

        Debug.Log("=== Current Card Set Info ===");
        Debug.Log($"Name: {cardSet.SetName}");
        Debug.Log($"Description: {cardSet.SetDescription}");
        Debug.Log($"Sprites Loaded: {sprites.Count}");
        Debug.Log($"Max Grid (Square): {CardSetUtility.CalculateMaxGridSize(sprites.Count)}");
        Debug.Log("============================");
    }

    /// <summary>
    /// Tests if the current card set can support various grid sizes.
    /// </summary>
    public void TestGridCompatibility()
    {
        if (CardSetManager.Instance == null || !CardSetManager.Instance.IsCardSetLoaded)
        {
            Debug.Log("No card set loaded for testing.");
            return;
        }

        Debug.Log("=== Grid Compatibility Test ===");
        
        Vector2Int[] testSizes = new Vector2Int[]
        {
            new Vector2Int(2, 2),
            new Vector2Int(2, 4),
            new Vector2Int(4, 4),
            new Vector2Int(4, 6),
            new Vector2Int(6, 6),
        };

        foreach (var size in testSizes)
        {
            bool canSupport = CardSetManager.Instance.CanSupportGridSize(size.x, size.y);
            string result = canSupport ? "✓" : "✗";
            Debug.Log($"{result} {size.x}x{size.y} grid");
        }
        
        Debug.Log("==============================");
    }

    #endregion

    #region Editor Helpers

#if UNITY_EDITOR
    [ContextMenu("Initialize System Now")]
    private async void InitializeSystemNow()
    {
        await InitializeCardSetSystem();
    }

    [ContextMenu("Show Card Set Info")]
    private void EditorShowCardSetInfo()
    {
        ShowCurrentCardSetInfo();
    }

    [ContextMenu("Test Grid Compatibility")]
    private void EditorTestGridCompatibility()
    {
        TestGridCompatibility();
    }

    [ContextMenu("Unload Current Card Set")]
    private void EditorUnloadCardSet()
    {
        if (CardSetManager.Instance != null)
        {
            CardSetManager.Instance.UnloadCurrentCardSet();
            LogDebug("Card set unloaded via editor menu.");
        }
    }
#endif

    #endregion
}
