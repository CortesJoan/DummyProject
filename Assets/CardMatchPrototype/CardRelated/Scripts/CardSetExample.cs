using UnityEngine;

/// <summary>
/// Example MonoBehaviour showing how to interact with the Card Set System.
/// This can be attached to a UI button or menu controller.
/// </summary>
public class CardSetExample : MonoBehaviour
{
    [Header("Example: Load Card Set on Button Click")]
    [Tooltip("Index of the card set to load (from CardSetManager's available sets)")]
    [SerializeField] private int cardSetIndex = 0;

    /// <summary>
    /// Example: Load a card set when a button is clicked
    /// </summary>
    public async void LoadCardSetExample()
    {
        // Check if CardSetManager exists
        if (CardSetManager.Instance == null)
        {
            Debug.LogError("CardSetManager not found in scene!");
            return;
        }

        Debug.Log($"Loading card set at index {cardSetIndex}...");

        // Load the card set asynchronously
        bool success = await CardSetManager.Instance.LoadCardSetByIndexAsync(cardSetIndex);

        if (success)
        {
            Debug.Log("Card set loaded successfully!");
            
            // Access the loaded data
            var currentSet = CardSetManager.Instance.CurrentCardSet;
            Debug.Log($"Set Name: {currentSet.SetName}");
            Debug.Log($"Available Cards: {CardSetManager.Instance.LoadedCardSprites.Count}");
        }
        else
        {
            Debug.LogError("Failed to load card set!");
        }
    }

    /// <summary>
    /// Example: Load the default card set
    /// </summary>
    public async void LoadDefaultCardSetExample()
    {
        if (CardSetManager.Instance == null) return;

        Debug.Log("Loading default card set...");
        bool success = await CardSetManager.Instance.LoadDefaultCardSetAsync();

        if (success)
        {
            Debug.Log("Default card set loaded!");
        }
    }

    /// <summary>
    /// Example: Check if current card set supports a grid size
    /// </summary>
    public void CheckGridSizeExample()
    {
        if (CardSetManager.Instance == null || !CardSetManager.Instance.IsCardSetLoaded)
        {
            Debug.Log("No card set loaded!");
            return;
        }

        // Test different grid sizes
        int[] testRows = { 2, 4, 6 };
        int[] testCols = { 2, 4, 6 };

        Debug.Log("=== Grid Size Compatibility ===");
        foreach (int rows in testRows)
        {
            foreach (int cols in testCols)
            {
                bool canSupport = CardSetManager.Instance.CanSupportGridSize(rows, cols);
                Debug.Log($"{rows}x{cols}: {(canSupport ? "✓ Supported" : "✗ Not Supported")}");
            }
        }
    }

    /// <summary>
    /// Example: Subscribe to card set events
    /// </summary>
    private void Start()
    {
        // Wait for CardSetManager to be ready
        if (CardSetManager.Instance != null)
        {
            SubscribeToEvents();
        }
    }

    private void SubscribeToEvents()
    {
        CardSetManager.Instance.OnCardSetLoaded += HandleCardSetLoaded;
        CardSetManager.Instance.OnCardSetUnloaded += HandleCardSetUnloaded;
    }

    private void OnDestroy()
    {
        // Always unsubscribe to prevent memory leaks
        if (CardSetManager.Instance != null)
        {
            CardSetManager.Instance.OnCardSetLoaded -= HandleCardSetLoaded;
            CardSetManager.Instance.OnCardSetUnloaded -= HandleCardSetUnloaded;
        }
    }

    /// <summary>
    /// Called when a card set is loaded
    /// </summary>
    private void HandleCardSetLoaded(CardSetData cardSet)
    {
        Debug.Log($"[Event] Card set loaded: {cardSet.SetName}");
        Debug.Log($"[Event] Description: {cardSet.SetDescription}");
        Debug.Log($"[Event] Card count: {cardSet.CardCount}");

        // You can update UI here, start the game, etc.
        // Example: UpdateCardSetDisplayUI(cardSet);
    }

    /// <summary>
    /// Called when a card set is unloaded
    /// </summary>
    private void HandleCardSetUnloaded()
    {
        Debug.Log("[Event] Card set unloaded");

        // You can cleanup UI here, return to menu, etc.
        // Example: ClearCardSetDisplayUI();
    }

    #region Editor Testing

#if UNITY_EDITOR
    [ContextMenu("Test: Load Card Set")]
    private void TestLoadCardSet()
    {
        LoadCardSetExample();
    }

    [ContextMenu("Test: Load Default Card Set")]
    private void TestLoadDefaultCardSet()
    {
        LoadDefaultCardSetExample();
    }

    [ContextMenu("Test: Check Grid Sizes")]
    private void TestCheckGridSizes()
    {
        CheckGridSizeExample();
    }

    [ContextMenu("Test: Get Current Set Info")]
    private void TestGetCurrentSetInfo()
    {
        if (CardSetManager.Instance == null || !CardSetManager.Instance.IsCardSetLoaded)
        {
            Debug.Log("No card set currently loaded.");
            return;
        }

        var set = CardSetManager.Instance.CurrentCardSet;
        Debug.Log($"Current Set: {set.SetName}");
        Debug.Log($"Sprites Loaded: {CardSetManager.Instance.LoadedCardSprites.Count}");
    }
#endif

    #endregion
}
