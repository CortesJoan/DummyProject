# Card Set System - Setup Checklist

## Pre-Implementation Checklist

### Unity Project Requirements
- [ ] Unity Addressables package installed (version 2.7.4 or higher)
- [ ] Card sprite assets prepared and imported
- [ ] At least one "back/hidden" card sprite available
- [ ] Minimum 2 unique card sprites (for 2x2 grid)

---

## Phase 1: Addressables Setup

### Make Sprites Addressable
- [ ] Open Window > Asset Management > Addressables > Groups
- [ ] Create new group "CardSets" (optional, can use default)
- [ ] Select card sprites in Project window
- [ ] Check "Addressable" in Inspector for each sprite
- [ ] Verify sprites appear in Addressables Groups window
- [ ] Repeat for hidden/back card sprite

### Build Addressables (Required!)
- [ ] Window > Asset Management > Addressables > Groups
- [ ] Build > New Build > Default Build Script
- [ ] Wait for build to complete
- [ ] Verify no errors in Console

---

## Phase 2: Create Card Set Data

### Create CardSetData Asset
- [ ] Right-click in Project window
- [ ] Create > Card Match > Card Set Data
- [ ] Name it descriptively (e.g., "AnimalCardSet")
- [ ] Fill in Set Name field
- [ ] Fill in Set Description field
- [ ] (Optional) Assign Set Icon
- [ ] Add Card Sprites references (drag Addressable sprites)
- [ ] Set Hidden Card Sprite reference
- [ ] Click "Validate Card Set" button in Inspector
- [ ] Fix any validation errors

### Verify CardSetData
- [ ] CardSetData asset created successfully
- [ ] No warnings in Inspector
- [ ] Validation passes
- [ ] Statistics show correct sprite count

---

## Phase 3: Scene Setup

### Add CardSetManager
- [ ] Create empty GameObject in scene (or use existing)
- [ ] Name it "CardSetManager"
- [ ] Add Component > CardSetManager
- [ ] Assign Default Card Set Reference (your CardSetData)
- [ ] Add same CardSetData to Available Card Sets list
- [ ] (Optional) Mark as DontDestroyOnLoad if needed

### Configure CardMatchUI
- [ ] Locate CardMatchUI component in scene
- [ ] Enable checkbox: "Use Addressable Card Sets"
- [ ] Save scene

### (Optional) Add Initializer
- [ ] Create empty GameObject
- [ ] Name it "CardSetInitializer"
- [ ] Add Component > CardSetSystemInitializer
- [ ] Enable "Load Default Set On Start"
- [ ] Enable "Show Debug Logs" for testing
- [ ] (Optional) Assign CardSetConfiguration if using one

---

## Phase 4: Integration Testing

### Basic Functionality Tests
- [ ] Enter Play Mode
- [ ] Check Console for "Card set loaded successfully" message
- [ ] Start a game
- [ ] Verify cards display with sprites from CardSetData
- [ ] Verify hidden/back sprite shows initially
- [ ] Complete a match successfully
- [ ] Check for any errors in Console

### Card Set Loading Tests
- [ ] Test LoadDefaultCardSetAsync()
- [ ] Test LoadCardSetByIndexAsync(0)
- [ ] Test CanSupportGridSize(4, 4)
- [ ] Verify events fire (OnCardSetLoaded, OnCardSetUnloaded)
- [ ] Test switching between card sets (if multiple)

### Memory Tests
- [ ] Load card set
- [ ] Start game
- [ ] Exit to menu
- [ ] Verify card set unloads
- [ ] Check memory usage in Profiler
- [ ] No memory leaks detected

---

## Phase 5: Advanced Features (Optional)

### Multiple Card Sets
- [ ] Create second CardSetData asset
- [ ] Configure with different sprites
- [ ] Add to CardSetManager's Available Card Sets
- [ ] Test loading both sets
- [ ] Verify switching works correctly

### Card Set Configuration Asset
- [ ] Create > Card Match > Card Set Configuration
- [ ] Add all CardSetData references
- [ ] Set default card set index
- [ ] Assign to CardSetSystemInitializer
- [ ] Test loading from configuration

### UI Selection System
- [ ] Create UI Panel for card set selection
- [ ] Add Previous, Next, Load buttons
- [ ] Add text fields for name/description
- [ ] Add CardSetSelector component
- [ ] Assign button references
- [ ] Add card set references
- [ ] Test UI navigation and loading

### Difficulty-Specific Card Sets
- [ ] Open GameDifficulty asset (Easy/Medium/Hard)
- [ ] Assign Specific Card Set field
- [ ] Start game with that difficulty
- [ ] Verify correct card set loads
- [ ] Test all difficulty levels

---

## Phase 6: Polish & Optimization

### Validation
- [ ] All card sets have sufficient sprites for grid sizes
- [ ] No missing Addressable references
- [ ] No null reference exceptions
- [ ] Proper error handling for edge cases

### Performance
- [ ] Addressables build is up to date
- [ ] Only one card set loaded at a time
- [ ] Proper cleanup on scene change
- [ ] No performance drops during loading

### User Experience
- [ ] Loading indicators show during async operations
- [ ] Clear error messages for users
- [ ] Smooth transitions between card sets
- [ ] No visible hitches or stutters

---

## Phase 7: Documentation Review

### Code Review
- [ ] All public methods have XML documentation
- [ ] Code follows project naming conventions
- [ ] No commented-out code
- [ ] Console logs appropriate (not excessive)

### Documentation Check
- [ ] Read CARD_SET_SYSTEM_README.md
- [ ] Read QUICK_START_GUIDE.md
- [ ] Read IMPLEMENTATION_SUMMARY.md
- [ ] Understand all features and limitations

---

## Common Issues & Solutions

### Issue: "Card set not loading"
**Check:**
- [ ] Sprites marked as Addressable?
- [ ] Addressables built?
- [ ] AssetReferences valid in CardSetData?
- [ ] CardSetManager in scene?

### Issue: "Not enough sprites" error
**Check:**
- [ ] Sprite count >= (gridRows × gridCols) ÷ 2
- [ ] Use CardSetUtility.CalculateMaxGridSize() to check

### Issue: "Hidden sprite not showing"
**Check:**
- [ ] Hidden Card Sprite reference set?
- [ ] Hidden sprite marked as Addressable?
- [ ] Reference valid in Inspector?

### Issue: "Memory not releasing"
**Check:**
- [ ] UnloadCurrentCardSet() called?
- [ ] Event handlers unsubscribed?
- [ ] No references kept to old sprites?

---

## Final Verification

### Before Committing to Version Control
- [ ] All new scripts compile without errors
- [ ] Modified scripts still work correctly
- [ ] Scene saves properly
- [ ] Addressables groups configured
- [ ] Documentation files included
- [ ] Example assets created (if applicable)

### Before Building
- [ ] Addressables built for target platform
- [ ] Test build on target device
- [ ] Memory usage acceptable
- [ ] Loading times acceptable
- [ ] No missing references in build

---

## Post-Implementation

### Team Communication
- [ ] Inform team about new system
- [ ] Share documentation
- [ ] Provide examples and demos
- [ ] Schedule training session if needed

### Continuous Improvement
- [ ] Monitor usage and performance
- [ ] Collect feedback from team
- [ ] Track issues and improvements needed
- [ ] Plan future enhancements

---

## Success Criteria

✅ **System is ready when:**
- All checklist items completed
- No errors in Console
- Games plays with Addressable card sets
- Can switch between multiple card sets
- Memory management works correctly
- Documentation complete and accessible

---

## Next Steps After Setup

1. **Create more card sets** for variety
2. **Design card set themes** (animals, food, space, etc.)
3. **Implement unlock system** for progression
4. **Add card set preview** in menus
5. **Consider remote content** for live updates

---

## Support Resources

- **Documentation:** CARD_SET_SYSTEM_README.md
- **Quick Start:** QUICK_START_GUIDE.md
- **Examples:** CardSetExample.cs
- **Unity Docs:** Unity Addressables Manual

---

**Checklist Version:** 1.0  
**Last Updated:** 2024  
**Status:** Production Ready ✅
