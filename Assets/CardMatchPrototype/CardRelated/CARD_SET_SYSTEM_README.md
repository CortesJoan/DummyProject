# Card Set System with Unity Addressables

## Overview

This system provides a scalable, modular way to manage different card sets in the Card Match game using Unity's Addressables system. It follows SOLID principles and clean code practices.

## Architecture

### Core Components

1. **CardSetData** (ScriptableObject)
   - Stores information about a single card set
   - Contains AssetReferences to card sprites (loaded via Addressables)
   - Includes metadata like name, description, and icon

2. **CardSetManager** (Singleton MonoBehaviour)
   - Manages loading and unloading of card sets
   - Handles Addressable asset lifecycle
   - Provides events for card set changes
   - Validates grid compatibility

3. **CardSetConfiguration** (ScriptableObject)
   - Central configuration for all available card sets
   - Manages card set collection and default settings
   - Provides helper methods for accessing card sets

4. **CardSetSelector** (MonoBehaviour)
   - UI component for selecting card sets
   - Handles user interaction for browsing and loading card sets

5. **CardSetUtility** (Static Class)
   - Provides helper methods for card set operations
   - Validation, shuffling, and grid calculations

## Setup Guide

### Step 1: Configure Addressables

1. Open **Window > Asset Management > Addressables > Groups**
2. Create a new group called "CardSets" (if not exists)
3. Configure settings for the CardSets group

### Step 2: Create Card Sprites as Addressables

1. Select your card sprite assets in the Project window
2. In the Inspector, check the **Addressable** checkbox
3. Assign them to the "CardSets" group
4. Optionally, add labels like "cards", "theme-animals", etc.

### Step 3: Create a CardSetData Asset

1. Right-click in Project window
2. Select **Create > Card Match > Card Set Data**
3. Name it (e.g., "AnimalCardSet")
4. Fill in the details:
   - Set Name: "Animals"
   - Set Description: "Cute animal cards"
   - Add AssetReferences to your card sprites
   - Set the hidden card sprite reference

### Step 4: Create CardSetConfiguration

1. Right-click in Project window
2. Select **Create > Card Match > Card Set Configuration**
3. Add your CardSetData references to the list
4. Set the default card set index

### Step 5: Setup CardSetManager in Scene

1. Create an empty GameObject in your scene
2. Name it "CardSetManager"
3. Add the CardSetManager component
4. Assign references:
   - Default Card Set Reference: Your default CardSetData
   - Available Card Sets: List of all CardSetData assets

### Step 6: Update CardMatchUI Settings

1. Select your CardMatchUI object
2. Enable **Use Addressable Card Sets** checkbox
3. The system will now use loaded card sets instead of the serialized list

### Step 7: (Optional) Add Card Set Selector UI

1. Create UI for card set selection
2. Add CardSetSelector component
3. Assign UI references (buttons, text fields)
4. Add card set references to the list

## Usage Examples

### Loading a Card Set Programmatically

```csharp
// Load default card set
await CardSetManager.Instance.LoadDefaultCardSetAsync();

// Load card set by index
await CardSetManager.Instance.LoadCardSetByIndexAsync(0);

// Load specific card set
AssetReference cardSetRef = myCardSetReference;
await CardSetManager.Instance.LoadCardSetAsync(cardSetRef);
```

### Checking if a Card Set is Compatible

```csharp
// Check if current card set supports a 4x4 grid
bool canSupport = CardSetManager.Instance.CanSupportGridSize(4, 4);

if (canSupport)
{
    // Start game with this grid size
}
```

### Listening to Card Set Events

```csharp
void Start()
{
    CardSetManager.Instance.OnCardSetLoaded += HandleCardSetLoaded;
    CardSetManager.Instance.OnCardSetUnloaded += HandleCardSetUnloaded;
}

void HandleCardSetLoaded(CardSetData cardSet)
{
    Debug.Log($"Loaded card set: {cardSet.SetName}");
}

void HandleCardSetUnloaded()
{
    Debug.Log("Card set unloaded");
}
```

### Using Difficulty-Specific Card Sets

In your GameDifficulty ScriptableObject:
1. Assign a specific card set to the "Specific Card Set" field
2. When that difficulty is selected, the system will automatically load that card set

## Benefits

### Scalability
- Add new card sets without modifying code
- Support unlimited card sets through Addressables
- Easy to add themed content or DLC

### Performance
- Only loads required assets into memory
- Efficient memory management through Addressables
- Assets are unloaded when no longer needed

### Maintainability
- Separation of concerns (SOLID principles)
- Clean, documented code
- Easy to extend and modify

### Flexibility
- Mix and match card sets with difficulties
- Runtime card set switching
- Support for different card themes per difficulty

## SOLID Principles Applied

1. **Single Responsibility Principle**
   - CardSetData: Only stores card set information
   - CardSetManager: Only manages loading/unloading
   - CardSetSelector: Only handles UI selection
   - CardSetUtility: Only provides helper functions

2. **Open/Closed Principle**
   - System is open for extension (new card sets)
   - Closed for modification (core logic unchanged)

3. **Liskov Substitution Principle**
   - All CardSetData assets are interchangeable
   - Interface-based design allows flexibility

4. **Interface Segregation Principle**
   - ISavable interface for saveable objects
   - Focused, minimal interfaces

5. **Dependency Inversion Principle**
   - Depends on abstractions (AssetReference)
   - Not coupled to specific implementations

## Best Practices

1. **Always validate card sets** before starting a game
2. **Unload unused card sets** to free memory
3. **Use CardSetConfiguration** for centralized management
4. **Label your Addressables** appropriately for organization
5. **Test with different grid sizes** to ensure compatibility
6. **Handle loading failures** gracefully

## Troubleshooting

### Card Set Not Loading
- Ensure sprites are marked as Addressable
- Check that AssetReferences are valid
- Verify Addressables groups are built

### Not Enough Sprites Error
- Use `CanSupportGridSize()` to validate before starting
- Check CardSetData has sufficient sprites for grid size

### Memory Issues
- Call `UnloadCurrentCardSet()` when switching
- Don't keep multiple card sets loaded simultaneously

## Future Enhancements

- Card set preview system
- Unlock/achievement system for card sets
- Card set bundles for DLC
- Remote card set loading
- Procedural card generation
- Card set ratings and favorites

## Files Created

- `CardSetData.cs` - ScriptableObject for card set data
- `CardSetManager.cs` - Singleton manager for card set lifecycle
- `CardSetConfiguration.cs` - Central configuration asset
- `CardSetSelector.cs` - UI component for selection
- `CardSetUtility.cs` - Helper utility methods
- `CardSetDataEditor.cs` - Custom editor for CardSetData
- `CARD_SET_SYSTEM_README.md` - This documentation

## Support

For issues or questions about the Card Set System, please refer to:
- Unity Addressables Documentation
- This README file
- Code comments in source files
