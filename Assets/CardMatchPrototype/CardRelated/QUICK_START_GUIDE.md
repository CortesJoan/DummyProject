# Quick Start Guide - Card Set System

## 5-Minute Setup

### Prerequisites
✓ Unity Addressables package installed (already done)
✓ Card sprites ready in your project

### Step-by-Step Setup

#### 1. Prepare Your Sprites (2 minutes)

1. Select all card sprites you want to use
2. In Inspector, check **"Addressable"** box
3. They'll be added to the default Addressables group
4. Repeat for the "hidden/back" card sprite

#### 2. Create Your First Card Set (1 minute)

1. **Right-click** in Project window
2. **Create > Card Match > Card Set Data**
3. Name it: `MyFirstCardSet`
4. Fill in:
   ```
   Set Name: My First Set
   Set Description: A collection of basic cards
   Card Sprites: [Drag your Addressable sprites here]
   Hidden Card Sprite: [Drag your back card sprite here]
   ```
5. Click **Validate Card Set** button to check everything is correct

#### 3. Setup the Manager (1 minute)

**Option A - Add to Existing GameObject:**
1. Find your GameManager or create a new GameObject
2. Add Component > **CardSetManager**
3. Assign your CardSetData to "Default Card Set Reference"
4. Add same CardSetData to "Available Card Sets" list

**Option B - Let it Auto-Create:**
The CardSetManager will auto-create itself as a singleton when first accessed.

#### 4. Enable in CardMatchUI (30 seconds)

1. Find your **CardMatchUI** component in the scene
2. Check the box: **☑ Use Addressable Card Sets**
3. Done!

#### 5. Test It (30 seconds)

1. Play the game
2. The system will automatically:
   - Load your card set
   - Use those sprites in the game
   - Unload when done

## That's It! 🎉

Your card match game now uses the Addressables system!

---

## Next Steps

### Add More Card Sets

1. Create more CardSetData assets (repeat step 2)
2. Add them to CardSetManager's "Available Card Sets" list
3. Optionally assign specific sets to difficulties in GameDifficulty assets

### Add UI Selection

1. Create UI with Previous/Next/Load buttons
2. Add **CardSetSelector** component
3. Assign button references
4. Add card sets to the selector

### Advanced: Themed Difficulties

1. Open a GameDifficulty asset (e.g., Easy, Medium, Hard)
2. Assign a specific CardSetData to "Specific Card Set" field
3. That difficulty will now use that card set automatically!

---

## Common Issues & Solutions

### "No card set loaded" error
**Solution:** Call `await CardSetManager.Instance.LoadDefaultCardSetAsync()` before starting game

### "Not enough sprites" error
**Solution:** Your card set needs at least (gridRows × gridColumns) ÷ 2 sprites

### Sprites not loading
**Solution:** 
1. Check sprites are marked as Addressable
2. Build Addressables: **Window > Asset Management > Addressables > Build > New Build > Default Build Script**

---

## Testing Checklist

- [ ] Created at least one CardSetData
- [ ] Marked sprites as Addressable
- [ ] Added CardSetManager to scene
- [ ] Enabled "Use Addressable Card Sets" in CardMatchUI
- [ ] Game loads and displays cards correctly
- [ ] Can complete a match successfully

---

## Example Code Snippets

### Load Card Set at Game Start

```csharp
async void Start()
{
    // Load the default card set
    bool success = await CardSetManager.Instance.LoadDefaultCardSetAsync();
    
    if (success)
    {
        Debug.Log("Card set ready!");
        // Now start your game
    }
}
```

### Switch Card Sets

```csharp
public async void SwitchToCardSet(int index)
{
    bool success = await CardSetManager.Instance.LoadCardSetByIndexAsync(index);
    
    if (success)
    {
        // Restart game with new card set
        RestartGame();
    }
}
```

### Check Compatibility

```csharp
void CheckGridSize(int rows, int cols)
{
    if (CardSetManager.Instance.CanSupportGridSize(rows, cols))
    {
        Debug.Log("Grid size supported!");
    }
    else
    {
        Debug.LogWarning("Not enough cards for this grid!");
    }
}
```

---

## Video Tutorial Outline

If creating a video tutorial, follow this structure:

1. **Intro** (30s) - What we're building
2. **Sprites Setup** (1m) - Making sprites Addressable
3. **CardSetData Creation** (1m) - Creating the asset
4. **Manager Setup** (1m) - Adding to scene
5. **Testing** (1m) - Playing the game
6. **Advanced** (1m) - Multiple sets, difficulties
7. **Outro** (30s) - Benefits and next steps

**Total: ~6 minutes**

---

## File Checklist

Essential files created:
- ✓ `CardSetData.cs` - Card set ScriptableObject
- ✓ `CardSetManager.cs` - Loading manager
- ✓ `CardSetConfiguration.cs` - Configuration helper
- ✓ `CardSetSelector.cs` - UI component
- ✓ `CardSetUtility.cs` - Helper utilities
- ✓ `CardSetDataEditor.cs` - Custom editor
- ✓ `CARD_SET_SYSTEM_README.md` - Full documentation
- ✓ `QUICK_START_GUIDE.md` - This guide

---

**Need Help?** Check the full README at `CARD_SET_SYSTEM_README.md`

**Ready to ship!** 🚀
