# Card Set System - Architecture Overview

## System Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────────┐
│                         CARD SET SYSTEM                              │
│                         (Unity Addressables)                         │
└─────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│                          LAYER 1: DATA                               │
├──────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  ┌─────────────────────┐      ┌──────────────────────────┐          │
│  │  CardSetData        │      │ CardSetConfiguration     │          │
│  │  (ScriptableObject) │      │ (ScriptableObject)       │          │
│  ├─────────────────────┤      ├──────────────────────────┤          │
│  │ - Set Name          │      │ - List<CardSetInfo>      │          │
│  │ - Description       │      │ - Default Set Index      │          │
│  │ - Icon              │◄─────┤ - Helper Methods         │          │
│  │ - Card Sprites[]    │      │                          │          │
│  │ - Hidden Sprite     │      └──────────────────────────┘          │
│  └─────────────────────┘                                            │
│           ▲                                                          │
│           │ References via                                          │
│           │ AssetReference                                          │
│           │                                                          │
│  ┌────────┴──────────┐                                              │
│  │ Unity Addressables │                                              │
│  │ Sprite Assets      │                                              │
│  └────────────────────┘                                              │
│                                                                       │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│                        LAYER 2: MANAGEMENT                           │
├──────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  ┌────────────────────────────────────────────────────────┐          │
│  │           CardSetManager (Singleton)                   │          │
│  ├────────────────────────────────────────────────────────┤          │
│  │ Properties:                                            │          │
│  │ - CurrentCardSet: CardSetData                          │          │
│  │ - LoadedCardSprites: List<Sprite>                      │          │
│  │ - LoadedHiddenSprite: Sprite                           │          │
│  │ - IsCardSetLoaded: bool                                │          │
│  ├────────────────────────────────────────────────────────┤          │
│  │ Methods:                                               │          │
│  │ + LoadCardSetAsync(AssetReference)                     │          │
│  │ + LoadCardSetByIndexAsync(int)                         │          │
│  │ + LoadDefaultCardSetAsync()                            │          │
│  │ + UnloadCurrentCardSet()                               │          │
│  │ + CanSupportGridSize(rows, cols)                       │          │
│  ├────────────────────────────────────────────────────────┤          │
│  │ Events:                                                │          │
│  │ • OnCardSetLoaded (CardSetData)                        │          │
│  │ • OnCardSetUnloaded ()                                 │          │
│  └────────────────────────────────────────────────────────┘          │
│                                                                       │
│  ┌────────────────────────────────────────────────────────┐          │
│  │           CardSetUtility (Static Helpers)              │          │
│  ├────────────────────────────────────────────────────────┤          │
│  │ + ValidateCardSetForGrid()                             │          │
│  │ + CalculateMaxGridSize()                               │          │
│  │ + GenerateShuffledCardList()                           │          │
│  │ + ShuffleList()                                        │          │
│  │ + ValidateAssetReference()                             │          │
│  └────────────────────────────────────────────────────────┘          │
│                                                                       │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│                      LAYER 3: PRESENTATION                           │
├──────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  ┌──────────────────┐      ┌────────────────────────────┐            │
│  │ CardSetSelector  │      │ CardSetSystemInitializer   │            │
│  │ (UI Component)   │      │ (Bootstrap)                │            │
│  ├──────────────────┤      ├────────────────────────────┤            │
│  │ - Previous Btn   │      │ + InitializeCardSetSystem()│            │
│  │ - Next Button    │      │ + LoadDefaultSetOnStart    │            │
│  │ - Load Button    │      │ + Event Subscriptions      │            │
│  │ - Display Text   │      └────────────────────────────┘            │
│  └──────────────────┘                                                │
│                                                                       │
└──────────────────────────────────────────────────────────────────────┘

┌──────────────────────────────────────────────────────────────────────┐
│                    LAYER 4: GAME INTEGRATION                         │
├──────────────────────────────────────────────────────────────────────┤
│                                                                       │
│  ┌─────────────────┐     ┌──────────────────┐                        │
│  │  GameManager    │     │  CardMatchUI     │                        │
│  ├─────────────────┤     ├──────────────────┤                        │
│  │ • StartGame()   │────►│ • SetupGame()    │                        │
│  │   - Load Card   │     │ • CreateGrid()   │                        │
│  │     Set for     │     │ • Uses sprites   │                        │
│  │     Difficulty  │     │   from Manager   │                        │
│  └─────────────────┘     └──────────────────┘                        │
│                                                                       │
│  ┌─────────────────────────────────────┐                             │
│  │      GameDifficulty                  │                             │
│  │      (ScriptableObject)              │                             │
│  ├─────────────────────────────────────┤                             │
│  │ - Grid Size                          │                             │
│  │ - Background Color                   │                             │
│  │ - Specific Card Set (Optional)       │                             │
│  └─────────────────────────────────────┘                             │
│                                                                       │
└──────────────────────────────────────────────────────────────────────┘
```

---

## Data Flow Diagram

```
┌──────────────┐
│ Game Starts  │
└──────┬───────┘
       │
       ▼
┌──────────────────────────────┐
│ CardSetSystemInitializer     │
│ Initializes System           │
└──────┬───────────────────────┘
       │
       ▼
┌──────────────────────────────┐
│ CardSetManager.              │
│ LoadDefaultCardSetAsync()    │
└──────┬───────────────────────┘
       │
       ▼
┌──────────────────────────────┐
│ Load CardSetData from        │
│ Addressables                 │
└──────┬───────────────────────┘
       │
       ▼
┌──────────────────────────────┐
│ Load Each Sprite             │
│ Referenced in CardSetData    │
└──────┬───────────────────────┘
       │
       ▼
┌──────────────────────────────┐
│ Store in Memory:             │
│ - LoadedCardSprites          │
│ - LoadedHiddenSprite         │
└──────┬───────────────────────┘
       │
       ▼
┌──────────────────────────────┐
│ Fire Event:                  │
│ OnCardSetLoaded              │
└──────┬───────────────────────┘
       │
       ▼
┌──────────────────────────────┐
│ Player Starts Game           │
└──────┬───────────────────────┘
       │
       ▼
┌──────────────────────────────┐
│ GameManager.StartGame()      │
└──────┬───────────────────────┘
       │
       ▼
┌──────────────────────────────┐
│ CardMatchUI.SetupGame()      │
│ Gets sprites from Manager    │
└──────┬───────────────────────┘
       │
       ▼
┌──────────────────────────────┐
│ Create Grid with Cards       │
│ Using Loaded Sprites         │
└──────┬───────────────────────┘
       │
       ▼
┌──────────────────────────────┐
│ Game Plays                   │
└──────────────────────────────┘
```

---

## Class Relationship Diagram

```
┌─────────────────────────────────────────────────────────────┐
│                      Inheritance                             │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  MonoBehaviour                    ScriptableObject          │
│       ▲                                  ▲                   │
│       │                                  │                   │
│       ├─ CardSetManager                 ├─ CardSetData      │
│       ├─ CardSetSelector                ├─ GameDifficulty   │
│       ├─ CardSetSystemInitializer       └─ CardSetConfig    │
│       ├─ CardMatchUI                                        │
│       └─ GameManager                                        │
│                                                              │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                      Dependencies                            │
├─────────────────────────────────────────────────────────────┤
│                                                              │
│  CardSetManager  ──uses──►  CardSetData                     │
│         │                                                    │
│         └──uses──►  Unity Addressables API                  │
│                                                              │
│  CardMatchUI  ──uses──►  CardSetManager                     │
│                                                              │
│  GameManager  ──uses──►  CardSetManager                     │
│         │                                                    │
│         └──uses──►  GameDifficulty                          │
│                                                              │
│  CardSetSelector  ──uses──►  CardSetManager                 │
│                                                              │
│  CardSetSystemInitializer  ──uses──►  CardSetManager        │
│         │                                                    │
│         └──uses──►  CardSetConfiguration (optional)         │
│                                                              │
└─────────────────────────────────────────────────────────────┘
```

---

## Event Flow Diagram

```
┌────────────────────────────────────────────────────────────────┐
│                        Event System                             │
├────────────────────────────────────────────────────────────────┤
│                                                                 │
│  CardSetManager                                                 │
│  ┌──────────────────────────────────────────┐                  │
│  │                                           │                  │
│  │  OnCardSetLoaded Event                   │                  │
│  │  ────────────────────────────►           │                  │
│  │                               ▼           │                  │
│  │                    ┌──────────────────┐  │                  │
│  │                    │  Event Listeners │  │                  │
│  │                    ├──────────────────┤  │                  │
│  │                    │ - Initializer    │  │                  │
│  │                    │ - UI Components  │  │                  │
│  │                    │ - Game Systems   │  │                  │
│  │                    └──────────────────┘  │                  │
│  │                                           │                  │
│  │  OnCardSetUnloaded Event                 │                  │
│  │  ────────────────────────────►           │                  │
│  │                               ▼           │                  │
│  │                    ┌──────────────────┐  │                  │
│  │                    │  Event Listeners │  │                  │
│  │                    ├──────────────────┤  │                  │
│  │                    │ - UI Cleanup     │  │                  │
│  │                    │ - Memory Release │  │                  │
│  │                    └──────────────────┘  │                  │
│  │                                           │                  │
│  └──────────────────────────────────────────┘                  │
│                                                                 │
└────────────────────────────────────────────────────────────────┘
```

---

## Memory Management Flow

```
┌─────────────────────────────────────────────────────────┐
│              Memory Lifecycle                            │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  1. Load Card Set                                       │
│     ┌───────────────────────────────────┐               │
│     │ Addressables.LoadAssetAsync()     │               │
│     │   - CardSetData                   │               │
│     │   - Sprite References             │               │
│     └───────┬───────────────────────────┘               │
│             │                                            │
│             ▼                                            │
│  2. Store Handles                                       │
│     ┌───────────────────────────────────┐               │
│     │ activeHandles Dictionary          │               │
│     │   - Keep references for cleanup   │               │
│     └───────┬───────────────────────────┘               │
│             │                                            │
│             ▼                                            │
│  3. Use Assets                                          │
│     ┌───────────────────────────────────┐               │
│     │ Game uses loaded sprites          │               │
│     │   - Display in UI                 │               │
│     │   - Card matching logic           │               │
│     └───────┬───────────────────────────┘               │
│             │                                            │
│             ▼                                            │
│  4. Unload Card Set                                     │
│     ┌───────────────────────────────────┐               │
│     │ UnloadCurrentCardSet()            │               │
│     │   - Release all handles           │               │
│     │   - Clear sprite lists            │               │
│     │   - Fire unload event             │               │
│     └───────────────────────────────────┘               │
│                                                          │
│  Result: Memory Freed ✓                                 │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

---

## Component Communication

```
┌────────────────────────────────────────────────────────────┐
│                 Component Communication                     │
├────────────────────────────────────────────────────────────┤
│                                                             │
│  UI Layer               Manager Layer          Data Layer  │
│  ┌──────────┐          ┌──────────┐          ┌──────────┐ │
│  │ Selector │──calls──►│ Manager  │──loads──►│   Data   │ │
│  └────┬─────┘          └────┬─────┘          └──────────┘ │
│       │                     │                              │
│       │    ┌────────────────┘                              │
│       │    │ fires events                                  │
│       ▼    ▼                                               │
│  ┌──────────────┐                                          │
│  │ Initializer  │                                          │
│  └──────────────┘                                          │
│       │                                                     │
│       │ subscribes to events                               │
│       ▼                                                     │
│  ┌──────────────┐                                          │
│  │ Game Systems │                                          │
│  └──────────────┘                                          │
│                                                             │
└────────────────────────────────────────────────────────────┘
```

---

## Key Design Patterns

### 1. Singleton Pattern
```
CardSetManager.Instance
- Single point of access
- Persistent across scenes (optional)
- Easy global access
```

### 2. Observer Pattern
```
Events:
- OnCardSetLoaded
- OnCardSetUnloaded

Subscribers can react to state changes
```

### 3. Factory Pattern
```
ScriptableObject.CreateInstance<>
- Creates card set data assets
- Creates configuration assets
```

### 4. Strategy Pattern
```
Different CardSetData = Different Strategies
- Easy to swap
- Polymorphic behavior
```

### 5. Facade Pattern
```
CardSetManager facades Unity Addressables:
- Simpler API
- Hides complexity
- Error handling
```

---

## Integration Points

```
┌─────────────────────────────────────────────────────────┐
│          System Integration Points                      │
├─────────────────────────────────────────────────────────┤
│                                                          │
│  1. CardMatchUI                                         │
│     - Toggle: useAddressableCardSets                    │
│     - Gets sprites from CardSetManager                  │
│                                                          │
│  2. GameManager                                         │
│     - LoadCardSetAndStartGame() method                  │
│     - Handles difficulty-specific sets                  │
│                                                          │
│  3. GameDifficulty                                      │
│     - specificCardSet field                             │
│     - Optional per-difficulty sets                      │
│                                                          │
│  4. Save System                                         │
│     - Can save selected card set index                  │
│     - Load last used set on startup                     │
│                                                          │
└─────────────────────────────────────────────────────────┘
```

---

## Scalability Features

```
1. Unlimited Card Sets
   - Add more without code changes
   - Runtime asset loading

2. Memory Efficient
   - Load only what's needed
   - Automatic cleanup

3. Extensible
   - Easy to add features
   - Plugin architecture

4. DLC Ready
   - Remote assets support
   - Dynamic content loading

5. Theme Support
   - Category system
   - Unlock mechanics ready
```

---

This architecture provides:
- ✅ Clean separation of concerns
- ✅ Easy maintenance
- ✅ Scalable design
- ✅ Memory efficient
- ✅ Professional quality
