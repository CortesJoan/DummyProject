using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using AnimalMemory.Progression;
using MementoMatch.Duel;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public enum CardState
{
    FaceDown,
    FaceUp,
    Matched
}

public class CardMatchUI : MonoBehaviour, ISavable
{
    [SerializeField] private Card cardPrefab;
    [SerializeField] private GridLayoutGroup gridLayout;
    [SerializeField] private RectTransform gridRect;
    [SerializeField] private Image gridBackground;
    [SerializeField] private int gridRows = 2;
    [SerializeField] private int gridColumns = 2;
    [SerializeField] private List<Sprite> cardFaceSprites;
    [SerializeField] private Sprite hiddenSprite;
    [SerializeField] private float timeUntilHide = 2f;
    [SerializeField] private bool useAddressableCardSets;

    [Header("UI Related")]
    [SerializeField] private TMP_Text revealCardsText;
    [SerializeField] private TMP_Text matchesText;
    [SerializeField] private TMP_Text turnsText;

    public UnityEvent onTryingMatch;
    public UnityEvent onMatchMade;
    public UnityEvent onMatchUndone = new UnityEvent();
    public UnityEvent onMatchFailed;
    public UnityEvent onWinEvent;
    public UnityEvent onDuelLost = new UnityEvent();
    public UnityEvent<int> onTurnCompleted;

    public event Action<RectTransform, RectTransform> PlayerDuelPairMade;
    public event Action<RectTransform, RectTransform> OpponentDuelPairMade;
    public event Action<int> OpponentDuelSkillActivated;
    public event Action<int, bool> DuelOutcomePresentationRequested;
    public event Action<bool, int> PlayerMoveResolved;

    private const int CardsPerMatch = 2;
    private const float SkillCutInSeconds = MementoSkillPresentationRules.MinimumSeconds;
    private float skillPresentationUntil;
    private const float DuelOutcomePresentationSeconds = 2.6f;
    private const string MatchesKey = "CardMatchUIMatches";
    private const string TurnsKey = "CardMatchUITurns";

    private static readonly Color[] ThemeBoardColors =
    {
        new Color(0.08f, 0.25f, 0.18f, 1f),
        new Color(0.48f, 0.17f, 0.18f, 1f),
        new Color(0.06f, 0.08f, 0.18f, 1f),
        new Color(0.12f, 0.30f, 0.19f, 1f),
        new Color(0.38f, 0.11f, 0.25f, 1f),
        new Color(0.22f, 0.16f, 0.10f, 1f)
    };

    private static readonly Color[] ThemeBackColors =
    {
        new Color(0.16f, 0.58f, 0.36f, 1f),
        new Color(0.95f, 0.39f, 0.38f, 1f),
        new Color(0.24f, 0.29f, 0.62f, 1f),
        new Color(0.36f, 0.72f, 0.39f, 1f),
        new Color(0.96f, 0.42f, 0.66f, 1f),
        new Color(0.63f, 0.43f, 0.18f, 1f)
    };

    private readonly List<Card> flippedCards = new List<Card>();
    private readonly Dictionary<Card, CardState> cardStates = new Dictionary<Card, CardState>();
    private readonly Dictionary<Card, int> cardIndices = new Dictionary<Card, int>();
    private readonly Dictionary<Card, int> cardPairIds = new Dictionary<Card, int>();
    private readonly Dictionary<Card, DuelCardId> duelCardIds = new Dictionary<Card, DuelCardId>();
    private readonly PostgameAbilityClock postgameAbilityClock = new PostgameAbilityClock();
    private readonly PostgameCurtainState postgameCurtain = new PostgameCurtainState();
    private readonly PostgameTestimonyState postgameTestimony = new PostgameTestimonyState();
    private readonly List<Card> cards = new List<Card>();
    private readonly List<Card> lastTurnCards = new List<Card>(2);
    private readonly Sprite[] proceduralCardBacks =
        new Sprite[AnimalMemoryContentIds.SetCount];
    private readonly Sprite[] proceduralBoardMats =
        new Sprite[AnimalMemoryContentIds.SetCount];

    private List<Sprite> defaultCardFaceSprites;
    private Sprite defaultHiddenSprite;
    private AnimalMemoryCardSetCatalog cardSetCatalog;
    private AnimalMemoryGuideRunState guideRunState;
    private int presentAllianceMask;
    public void ConfigureAllianceRoster(int mask) => presentAllianceMask = mask & 0x1E;

    public bool TryUseAlliancePower(int guideId)
    {
        if (!IsGodDuelConfigured || !IsDuelActive || duelState.ActiveActor != DuelActor.Player ||
            guideRunState == null || !guideRunState.IsAlliancePowerReady(guideId) || IsBusy)
            return false;
        int previous = guideRunState.SelectedGuideId;
        if (!guideRunState.SelectAllianceGuide(guideId)) return false;
        if (TryUseGuidePower()) return true;
        guideRunState.SelectAllianceGuide(previous);
        return false;
    }
    private MementoMatchBackdropView backdropView;
    private DuelOpponentFieldView opponentFieldView;
    private int selectedSetId;

    private float matchDelay = 0.1f;
    private int matches;
    private int turns;
    private int selectedThemeIndex = -1;
    private bool isRevealingCards;
    private bool isResolvingMatch;
    private bool lastTurnAvailable;
    private bool lastTurnWasMatch;
    private bool useToolkitHud;
    private bool hasPresentationViewport;
    private Vector2 presentationScreenSize;
    private Rect presentationSafeArea;
    private float presentationTopInset;
    private float presentationBottomInset;
    private bool gridLayoutInitialized;
    private Rect lastGridLocalArea;
    private Rect lastGridParentRect;
    private int lastLayoutRows;
    private int lastLayoutColumns;
    private bool opponentTurnRunning;
    private bool duelOutcomeAcknowledged;
    private int configuredDuelGuideId = -1;
    private int? configuredDuelSeed;
    private int duelSeed;
    private int boardRevision;
    private readonly DuelState duelState = new DuelState();
    private DuelOpponentBrain duelBrain;
    private ComboSystem comboSystem;
    private int lastPlayerDuelDamage = 1;
    private int ultimateMasterSkillIndex;
    private int ultimateMasterActiveTechnique = -1;
    private bool ultimateMasterShieldPending;
    private bool ultimateMasterUndoPending;
    private MementoPostgameAbilityId configuredPostgameAbility = MementoPostgameAbilityId.Unassigned;
    private int postgameAbilityActivationSerial;
    private readonly GodDuelState godDuel = new GodDuelState();
    private readonly GodJudgementState godJudgement = new GodJudgementState();
    private readonly GodEraseState godErase = new GodEraseState();
    private readonly HashSet<int> godErasedOrdinals = new HashSet<int>();
    private readonly HashSet<int> godOrphanOrdinals = new HashSet<int>();
    private bool godDuelInitialized;
    private bool godPhaseTransitionPresented;
    private int godSkillSerial;
    private const float GodPhaseTransitionSeconds = 1.6f;
    private static readonly Color GodErasedTint = new Color(0.34f, 0.11f, 0.11f, 1f);
    private static readonly Color GodOrphanTint = new Color(0.52f, 0.52f, 0.52f, 1f);

    public const int SuppliesPerAttempt = 3;
    private int suppliesUsed;
    public int SupplyUsesRemaining => Mathf.Max(0, SuppliesPerAttempt - suppliesUsed);
    public int BoardRows => gridRows;
    public int BoardColumns => gridColumns;
    public bool CanUseSupplies => SupplyUsesRemaining > 0 && !IsBusy &&
        IsPlayerTurn && (!IsDuelConfigured || IsDuelActive) &&
        flippedCards.Count == 0 && cards.Count > 0;

    public int Turns => turns;
    public int Matches => matches;
    public bool IsBusy => isRevealingCards || isResolvingMatch || opponentTurnRunning || Time.realtimeSinceStartup < skillPresentationUntil;
    public int CurrentPairCount => IsDuelConfigured && duelState.TargetPairs > 0
        ? duelState.TargetPairs
        : Mathf.Max(0, (gridRows * gridColumns) / CardsPerMatch);
    public bool IsDuelConfigured => configuredDuelGuideId >= 0;
    public bool IsDuelActive => IsDuelConfigured && duelState.IsActive;
    public bool IsGuidePowerBlocked =>
        IsDuelConfigured &&
        guideRunState != null &&
        configuredDuelGuideId == guideRunState.SelectedGuideId;
    public bool IsPlayerTurn => !IsDuelActive || duelState.ActiveActor == DuelActor.Player;
    public int DuelPlayerPairs => duelState.PlayerPairs;
    public int DuelOpponentPairs => duelState.OpponentPairs;
    public int DuelTargetPairs => duelState.TargetPairs;
    public int DuelOpponentTurns => duelState.OpponentTurns;
    public int DuelPlayerHealth => duelState.PlayerHealth;
    public int DuelOpponentHealth => duelState.OpponentHealth;
    public int LastPlayerDuelDamage => lastPlayerDuelDamage;
    public int DuelOpponentGuideId => duelState.OpponentGuideId;
    public int DuelOpponentTechniqueGuideId =>
        HasOpponentTechniqueSuite
            ? ultimateMasterActiveTechnique
            : configuredDuelGuideId;
    public int DuelOpponentPose { get; private set; }
    public RectTransform DuelOpponentAttackTarget =>
        opponentFieldView != null ? opponentFieldView.AttackTarget : null;
    public float ActiveRevealDuration => timeUntilHide;
    public int DuelSeed => duelSeed;
    public int BoardRevision => boardRevision;
    public MementoPostgameAbilityId DuelOpponentAbilityId => configuredPostgameAbility;
    public string DuelOpponentAbilityName => PostgameAbilityRules.GetDisplayName(configuredPostgameAbility);
    public int ActiveCurtainRow => IsDuelConfigured ? postgameCurtain.ActiveRow : -1;
    public int PublicTestimonyCardCount => IsDuelConfigured ? postgameTestimony.PublicCardCount : 0;
    public bool IsTestimonyWindowArmed => IsDuelConfigured && postgameTestimony.IsWindowArmed;
    public bool IsGodDuelConfigured =>
        configuredDuelGuideId == MementoPostgameEncounters.LastOpponentId;
    public GodDuelPhase GodPhase => godDuel.Phase;
    public bool IsGodPhaseTransitionPresented => godPhaseTransitionPresented;
    public int GodRebuildSerial => godDuel.RebuildSerial;
    public bool IsGodJudgementArmed => godJudgement.IsArmed;
    public int GodJudgementPairCount => godJudgement.TargetPairCount;
    public bool IsGodEraseArmed => godErase.IsArmed;
    public int GodEraseTargetOrdinal => godErase.IsArmed ? godErase.Target.Ordinal : -1;
    public int GodErasedCardCount => godErasedOrdinals.Count;
    public int GodOrphanedCardCount => godOrphanOrdinals.Count;
    public event System.Action<int> GodJudgementArmed;
    public event System.Action<int> GodJudgementResolved;
    public event System.Action<int> GodEraseArmed;
    public event System.Action GodEraseSaved;
    public event System.Action<int> GodRebuildCompleted;
    public event System.Action GodPhaseTransitioned;
    public bool IsEfficiencyBonusEarned =>
        turns <= Mathf.CeilToInt(CurrentPairCount * 1.5f);

    private void Awake()
    {
        defaultCardFaceSprites = cardFaceSprites != null
            ? new List<Sprite>(cardFaceSprites)
            : new List<Sprite>();
        defaultHiddenSprite = hiddenSprite;
        comboSystem = GetComponent<ComboSystem>();
        if (comboSystem == null)
            comboSystem = FindObjectOfType<ComboSystem>();
        backdropView = GetComponent<MementoMatchBackdropView>();
        if (backdropView == null)
            backdropView = gameObject.AddComponent<MementoMatchBackdropView>();
        backdropView.Bind(gridBackground);

        opponentFieldView = GetComponent<DuelOpponentFieldView>();
        if (opponentFieldView == null)
            opponentFieldView = gameObject.AddComponent<DuelOpponentFieldView>();
        opponentFieldView.Bind(this, gridRect);
    }

    public void PlayDuelOpponentImpact()
    {
        opponentFieldView?.PlayHitReaction();
    }

    public void ConfigureProgression(int setId, AnimalMemoryGuideRunState runState)
    {
        selectedSetId = Mathf.Clamp(setId, 0, AnimalMemoryContentIds.SetCount - 1);
        guideRunState = runState;
        ApplySelectedCardSet();
    }

    public IReadOnlyList<Sprite> GetCardSetPreview(int setId)
    {
        if (setId == AnimalMemoryContentIds.ForestSet)
        {
            if (cardSetCatalog == null)
                cardSetCatalog = Resources.Load<AnimalMemoryCardSetCatalog>(
                    "AnimalMemory/AnimalMemoryCardSetCatalog");
            IReadOnlyList<Sprite> forest = cardSetCatalog != null
                ? cardSetCatalog.GetSprites(setId)
                : null;
            return forest ?? (IReadOnlyList<Sprite>)Array.Empty<Sprite>();
        }

        return MementoIllustratedCards.GetSprites(
            Mathf.Clamp(setId, 0, AnimalMemoryContentIds.SetCount - 1));
    }

    private bool playerComboDamageEnabled = true;
    private bool HasOpponentTechniqueSuite =>
        configuredDuelGuideId == AnimalMemoryContentIds.UltimateMasterOpponent ||
        (MementoPostgameEncounters.TryGetOpponent(configuredDuelGuideId, out var postgameProfile) &&
         postgameProfile.TechniqueGuideId != MementoPostgameEncounters.NoLegacyTechnique);
    private bool HasPostgameAbility =>
        configuredPostgameAbility != MementoPostgameAbilityId.Unassigned;

    public void ConfigurePlayerRole(int guideId) { playerComboDamageEnabled = guideId < 0; }

    public void ConfigureDuel(int opponentGuideId)
    {
        configuredDuelGuideId = opponentGuideId;
        configuredDuelSeed = null;
        configuredPostgameAbility = MementoPostgameEncounters.GetAbilityId(opponentGuideId);
    }

    public void ConfigureDuel(int opponentGuideId, int seed)
    {
        configuredDuelGuideId = opponentGuideId;
        configuredDuelSeed = seed;
        configuredPostgameAbility = MementoPostgameEncounters.GetAbilityId(opponentGuideId);
    }

    public void ClearDuel()
    {
        configuredDuelGuideId = -1;
        configuredDuelSeed = null;
        duelSeed = 0;
        boardRevision = 0;
        duelCardIds.Clear();
        opponentTurnRunning = false;
        DuelOpponentPose = 0;
        lastPlayerDuelDamage = 1;
        ultimateMasterSkillIndex = 0;
        ultimateMasterActiveTechnique = -1;
        ultimateMasterShieldPending = false;
        ultimateMasterUndoPending = false;
        configuredPostgameAbility = MementoPostgameAbilityId.Unassigned;
        postgameAbilityActivationSerial = 0;
        postgameAbilityClock.Reset(MementoPostgameAbilityId.Unassigned);
        postgameCurtain.Reset();
        postgameTestimony.Reset();
        godDuel.Reset();
        godJudgement.Reset();
        godErase.Reset();
        godErasedOrdinals.Clear();
        godOrphanOrdinals.Clear();
        godDuelInitialized = false;
        godPhaseTransitionPresented = false;
        godSkillSerial = 0;
        duelBrain = null;
        duelState.Reset();
    }

    public void SetupGame(int rows, int columns, Color backgroundColor, int revealTime)
    {
        suppliesUsed = 0;
        StopAllCoroutines();
        skillPresentationUntil = 0f;
        flippedCards.Clear();
        isRevealingCards = false;
        isResolvingMatch = false;
        matches = 0;
        turns = 0;
        lastTurnCards.Clear();
        lastTurnAvailable = false;
        lastTurnWasMatch = false;
        opponentTurnRunning = false;
        ultimateMasterSkillIndex = 0;
        ultimateMasterActiveTechnique = -1;
        ultimateMasterShieldPending = false;
        ultimateMasterUndoPending = false;
        postgameAbilityActivationSerial = 0;
        postgameAbilityClock.Reset(configuredPostgameAbility);
        postgameCurtain.Reset();
        postgameTestimony.Reset();
        godDuel.Reset();
        godJudgement.Reset();
        godErase.Reset();
        godErasedOrdinals.Clear();
        godOrphanOrdinals.Clear();
        godDuelInitialized = false;
        godPhaseTransitionPresented = false;
        godSkillSerial = 0;
        if (IsDuelConfigured)
        {
            duelState.Begin(
                configuredDuelGuideId,
                Mathf.Max(1, (rows * columns) / CardsPerMatch));
            duelSeed = configuredDuelSeed ??
                unchecked(Environment.TickCount ^ (configuredDuelGuideId * 397));
            boardRevision = 0;
            duelBrain = null;
        }
        else
        {
            duelState.Reset();
            duelBrain = null;
            duelSeed = 0;
            boardRevision = 0;
            duelCardIds.Clear();
        }

        SetRevealLabelVisible(false);
        UpdateMatchesText();
        UpdateTurnsText();

        gridRows = rows;
        gridColumns = columns;
        timeUntilHide = revealTime + (guideRunState != null ? guideRunState.InitialRevealBonusSeconds : 0f);

        if (gridBackground != null)
        {
            gridBackground.color = backgroundColor;
        }

        if (useAddressableCardSets &&
            CardSetManager.Instance != null &&
            CardSetManager.Instance.IsCardSetLoaded)
        {
            cardFaceSprites = new List<Sprite>(CardSetManager.Instance.LoadedCardSprites);
            hiddenSprite = CardSetManager.Instance.LoadedHiddenSprite;

            if (!CardSetManager.Instance.CanSupportGridSize(rows, columns))
            {
                Debug.LogError($"Current card set does not have enough sprites for a {rows}x{columns} grid.");
                DisableAllCards();
                return;
            }
        }

        ApplySelectedCardSet();

        if (selectedThemeIndex >= 0)
        {
            ApplyTheme(selectedThemeIndex);
            backdropView?.ApplyTheme(selectedThemeIndex, true);
        }

        if (!CreateGrid())
        {
            DisableAllCards();
            return;
        }

        StartCoroutine(RevealCardsThenHide(timeUntilHide));
    }

    public void CancelCurrentGame()
    {
        suppliesUsed = 0;
        StopAllCoroutines();
        skillPresentationUntil = 0f;
        flippedCards.Clear();
        lastTurnCards.Clear();
        lastTurnAvailable = false;
        lastTurnWasMatch = false;
        isRevealingCards = false;
        isResolvingMatch = false;
        opponentTurnRunning = false;
        ClearDuel();
        if (guideRunState != null)
        {
            guideRunState.Reset(guideRunState.SelectedGuideId);
        }

        SetRevealLabelVisible(false);
        DisableAllCards();
    }

    public void SetTheme(int themeIndex)
    {
        selectedThemeIndex = Mathf.Clamp(themeIndex, 0, ThemeBoardColors.Length - 1);
        AnimalMemoryJuice.SetThemeGlobal(selectedThemeIndex);
        ApplyTheme(selectedThemeIndex);
        backdropView?.ApplyTheme(selectedThemeIndex, false);

        foreach (KeyValuePair<Card, CardState> entry in cardStates)
        {
            if (entry.Key != null && entry.Value == CardState.FaceDown)
            {
                HideCardUnlessPublic(entry.Key);
            }
        }
    }

    private void ApplySelectedCardSet()
    {
        if (defaultCardFaceSprites == null)
        {
            defaultCardFaceSprites = cardFaceSprites != null
                ? new List<Sprite>(cardFaceSprites)
                : new List<Sprite>();
            defaultHiddenSprite = hiddenSprite;
        }

        List<Sprite> selectedSprites = new List<Sprite>();
        if (selectedSetId == AnimalMemoryContentIds.ForestSet)
        {
            if (cardSetCatalog == null)
            {
                cardSetCatalog = Resources.Load<AnimalMemoryCardSetCatalog>(
                    "AnimalMemory/AnimalMemoryCardSetCatalog");
            }

            IReadOnlyList<Sprite> forestSprites =
                cardSetCatalog != null
                    ? cardSetCatalog.GetSprites(AnimalMemoryContentIds.ForestSet)
                    : null;
            if (forestSprites != null)
            {
                for (int i = 0; i < forestSprites.Count; i++)
                {
                    Sprite sprite = forestSprites[i];
                    if (sprite != null && !selectedSprites.Contains(sprite))
                        selectedSprites.Add(sprite);
                }
            }
        }
        else
        {
            // The five themed collections use the semantic identity contract.
            // This avoids legacy imported sheets whose faces were technically
            // different assets but visually repeated the same central motif.
            selectedSprites = new List<Sprite>(
                MementoIllustratedCards.GetSprites(selectedSetId));
        }

        cardFaceSprites =
            selectedSprites.Count >=
            AnimalMemoryCardIdentityCatalog.IdentitiesPerSet
                ? selectedSprites
                : new List<Sprite>(defaultCardFaceSprites);
        hiddenSprite = defaultHiddenSprite;
        SetTheme(selectedSetId);
    }

    private void ApplyTheme(int themeIndex)
    {
        // The scene's gridBackground is the full-screen animated backdrop.
        // Style the board itself independently; never overwrite the world image.
        Image boardMat = gridRect != null ? gridRect.GetComponent<Image>() : null;
        if (boardMat != null && boardMat != gridBackground)
        {
            boardMat.sprite = GetOrCreateBoardMat(themeIndex);
            boardMat.type = Image.Type.Tiled;
            boardMat.color = new Color(1f, 1f, 1f, 0.86f);
            boardMat.raycastTarget = false;
        }

        hiddenSprite = GetOrCreateProceduralCardBack(themeIndex);
    }

    private Sprite GetOrCreateBoardMat(int themeIndex)
    {
        if (proceduralBoardMats[themeIndex] != null)
            return proceduralBoardMats[themeIndex];

        const int size = 96;
        // A subdued themed felt keeps the illustrated faces readable without
        // tinting the character yellow or competing with the world backdrop.
        Color baseColor = Color.Lerp(ThemeBoardColors[themeIndex], new Color(0.025f, 0.045f, 0.055f), 0.72f);
        Color accent = Color.Lerp(baseColor, Color.white, 0.2f);
        Color shadow = Color.Lerp(baseColor, Color.black, 0.22f);
        Color[] pixels = new Color[size * size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x + 0.5f) / size - 0.5f;
                float ny = (y + 0.5f) / size - 0.5f;
                float vignette = Mathf.Clamp01(1f - Mathf.Sqrt(nx * nx + ny * ny) * 0.34f);
                bool motif;
                switch (themeIndex)
                {
                    case 0: motif = ((x + y) % 31 < 2) || ((x - y + size) % 47 < 2); break;
                    case 1: motif = ((x / 12 + y / 12) % 2 == 0) && ((x + y) % 9 < 2); break;
                    case 2: motif = ((x % 24 == 12) && (y % 24 >= 8 && y % 24 <= 16)) ||
                                           ((y % 24 == 12) && (x % 24 >= 8 && x % 24 <= 16)); break;
                    case 3: motif = ((x % 32 - 16) * (x % 32 - 16) + (y % 24 - 12) * (y % 24 - 12)) < 38; break;
                    case 4: motif = (Mathf.Abs((x % 24) - 12) + Mathf.Abs((y % 24) - 12)) < 2; break;
                    default: motif = ((x % 24 == 0) || (y % 24 == 0)) && ((x + y) % 5 < 3); break;
                }

                Color tone = motif ? Color.Lerp(baseColor, accent, 0.38f) : baseColor;
                if (((x + 2 * y + themeIndex * 11) % 37) == 0)
                    tone = Color.Lerp(tone, shadow, 0.28f);
                pixels[y * size + x] = Color.Lerp(shadow, tone, vignette);
            }
        }

        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
        {
            name = $"BoardMatTheme_{themeIndex}",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Repeat,
            hideFlags = HideFlags.DontSave
        };
        texture.SetPixels(pixels);
        texture.Apply(false, false);
        Sprite boardMat = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            100f);
        boardMat.name = texture.name;
        boardMat.hideFlags = HideFlags.DontSave;
        proceduralBoardMats[themeIndex] = boardMat;
        return boardMat;
    }

    private Sprite GetOrCreateProceduralCardBack(int themeIndex)
    {
        if (proceduralCardBacks[themeIndex] != null)
        {
            return proceduralCardBacks[themeIndex];
        }

        const int width = 48;
        const int height = 64;
        const int border = 3;

        Color baseColor = ThemeBackColors[themeIndex];
        Color accentColor = Color.Lerp(baseColor, Color.white, 0.55f);
        Color edgeColor = Color.Lerp(baseColor, Color.black, 0.45f);
        Color[] pixels = new Color[width * height];

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isBorder = x < border || x >= width - border || y < border || y >= height - border;
                bool isPattern = ((x + y) % 14 < 3) || ((x - y + height) % 14 < 3);
                pixels[(y * width) + x] = isBorder ? edgeColor : isPattern ? accentColor : baseColor;
            }
        }

        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
        {
            name = $"CardBackTheme_{themeIndex}",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.DontSave
        };
        texture.SetPixels(pixels);
        texture.Apply(false, false);

        Sprite cardBack = Sprite.Create(
            texture,
            new Rect(0f, 0f, width, height),
            new Vector2(0.5f, 0.5f),
            100f);
        cardBack.name = texture.name;
        cardBack.hideFlags = HideFlags.DontSave;
        proceduralCardBacks[themeIndex] = cardBack;
        return cardBack;
    }

    private bool CreateGrid()
    {
        int totalCards = gridRows * gridColumns;
        if (totalCards <= 0 || totalCards % CardsPerMatch != 0)
        {
            Debug.LogError("The total number of cards must be positive and even.");
            return false;
        }

        int uniqueCardCount = totalCards / CardsPerMatch;
        if (cardFaceSprites == null || cardFaceSprites.Count < uniqueCardCount)
        {
            Debug.LogError($"The grid needs {uniqueCardCount} unique card sprites, but not enough are assigned.");
            return false;
        }

        if (cardPrefab == null || gridLayout == null)
        {
            Debug.LogError("Cannot create card grid: prefab or GridLayoutGroup is not assigned.");
            return false;
        }

        int nextBoardRevision = boardRevision;
        IDuelRandom boardShuffleRandom = null;
        if (IsDuelConfigured)
        {
            nextBoardRevision = boardRevision + 1;
            boardShuffleRandom = new SystemDuelRandom(
                DuelRules.DeriveBoardSeed(duelSeed, nextBoardRevision));
        }

        List<int> pairedIndices = AnimalMemoryPairBuilder.BuildPairedIndices(uniqueCardCount);
        List<Sprite> spritesToAssign = new List<Sprite>(totalCards);
        List<int> pairIdsToAssign = new List<int>(totalCards);
        for (int i = 0; i < pairedIndices.Count; i++)
        {
            spritesToAssign.Add(cardFaceSprites[pairedIndices[i]]);
            pairIdsToAssign.Add(pairedIndices[i]);
        }

        for (int i = 0; i < spritesToAssign.Count; i++)
        {
            int randomIndex = boardShuffleRandom != null
                ? boardShuffleRandom.Range(i, spritesToAssign.Count)
                : UnityEngine.Random.Range(i, spritesToAssign.Count);
            Sprite temporarySprite = spritesToAssign[i];
            spritesToAssign[i] = spritesToAssign[randomIndex];
            spritesToAssign[randomIndex] = temporarySprite;
            int temporaryPairId = pairIdsToAssign[i];
            pairIdsToAssign[i] = pairIdsToAssign[randomIndex];
            pairIdsToAssign[randomIndex] = temporaryPairId;
        }

        // Card objects are reused, but an undo snapshot belongs to the old board.
        flippedCards.Clear();
        lastTurnCards.Clear();
        lastTurnAvailable = false;
        lastTurnWasMatch = false;
        cards.RemoveAll(card => card == null);
        for (int i = cards.Count - 1; i >= totalCards; i--)
        {
            Card surplusCard = cards[i];
            if (surplusCard != null)
            {
                Button surplusButton = surplusCard.GetComponent<Button>();
                if (surplusButton != null)
                {
                    surplusButton.interactable = false;
                    surplusButton.onClick.RemoveAllListeners();
                }

                surplusCard.gameObject.SetActive(false);
                Destroy(surplusCard.gameObject);
            }

            cards.RemoveAt(i);
        }

        while (cards.Count < totalCards)
        {
            Card newCard = Instantiate(cardPrefab, gridLayout.transform);
            cards.Add(newCard);
        }

        cardStates.Clear();
        cardIndices.Clear();
        cardPairIds.Clear();
        duelCardIds.Clear();
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = Mathf.Max(1, gridColumns);
        ApplyResponsiveGridLayout();

        for (int i = 0; i < totalCards; i++)
        {
            Card currentCard = cards[i];
            currentCard.name = $"Card_{i}";
            currentCard.gameObject.SetActive(true);
            currentCard.ResetVisualOrientation();
            currentCard.FitVisualTo(gridLayout.cellSize);
            currentCard.OriginalSprite = spritesToAssign[i];
            currentCard.RestoreOriginalSprite();
            currentCard.ToggleRenderer(true);
            cardStates[currentCard] = CardState.FaceDown;
            cardIndices[currentCard] = i;
            cardPairIds[currentCard] = pairIdsToAssign[i];
            if (IsDuelConfigured)
                duelCardIds[currentCard] = new DuelCardId(nextBoardRevision, i);

            Button cardButton = currentCard.GetComponent<Button>();
            if (cardButton == null)
            {
                Debug.LogError($"Card prefab is missing a Button component: {currentCard.name}");
                return false;
            }

            cardButton.onClick.RemoveAllListeners();
            cardButton.interactable = true;
            Card capturedCard = currentCard;
            cardButton.onClick.AddListener(() => OnCardClicked(capturedCard));
        }

        if (IsDuelConfigured)
        {
            boardRevision = nextBoardRevision;
            duelBrain = new DuelOpponentBrain(
                new SystemDuelRandom(DuelRules.DeriveBrainSeed(duelSeed, boardRevision)),
                DuelRules.GetForgetChance(configuredDuelGuideId));
            duelBrain.BeginBoard(boardRevision);
            postgameCurtain.BeginBoard(boardRevision);
            postgameTestimony.BeginBoard(boardRevision);
            if (IsGodDuelConfigured)
            {
                if (!godDuelInitialized)
                {
                    godDuel.Begin(duelState.TargetPairs, boardRevision);
                    godDuelInitialized = true;
                }
                godJudgement.BeginBoard(boardRevision);
                godErase.BeginBoard(boardRevision);
            }
        }

        return true;
    }

    /// <summary>Updates presentation only. Rotation must never rebuild or reshuffle a match.</summary>
    public void SetPresentationViewport(Rect safeArea, float topInset, float bottomInset)
    {
        presentationScreenSize = new Vector2(Screen.width, Screen.height);
        presentationSafeArea = safeArea;
        presentationTopInset = topInset;
        presentationBottomInset = bottomInset;
        hasPresentationViewport = true;
    }

    public Rect GetOpponentPresentationArea()
    {
        Vector2 screen = new Vector2(Screen.width, Screen.height);
        bool reportedHud = hasPresentationViewport && presentationScreenSize == screen;
        Rect safe = MementoGameplayLayout.ValidSafeArea(
            screen, reportedHud ? presentationSafeArea : Screen.safeArea);
        bool portrait = MementoGameplayLayout.IsPortrait(screen);
        float topInset = reportedHud ? presentationTopInset : safe.height * (portrait ? 0.15f : 0.12f);
        // Keep the top anchored below the HUD. A shorter portrait frame raises
        // the full-body sprite's face above the board without moving card input.
        float height = safe.height * (portrait ? 0.70f : 0.76f);
        float width = safe.width * (portrait ? 0.94f : 0.43f);
        float left = portrait ? safe.center.x - width * 0.5f : safe.xMin + safe.width * 0.56f;
        return new Rect(left, safe.yMax - topInset - height, width, height);
    }

    private void LateUpdate()
    {
        if (cards.Count > 0)
            ApplyResponsiveGridLayout();
    }

    private void ApplyResponsiveGridLayout()
    {
        if (gridLayout == null || gridRect == null ||
            !(gridRect.parent is RectTransform parent))
            return;

        Vector2 screen = new Vector2(Screen.width, Screen.height);
        if (screen.x <= 0f || screen.y <= 0f)
            return;
        bool reportedHud = hasPresentationViewport && presentationScreenSize == screen;
        Rect safe = MementoGameplayLayout.ValidSafeArea(
            screen, reportedHud ? presentationSafeArea : Screen.safeArea);
        bool portrait = MementoGameplayLayout.IsPortrait(screen);
        float topInset = reportedHud ? presentationTopInset : safe.height * (portrait ? 0.15f : 0.12f);
        float bottomInset = reportedHud ? presentationBottomInset : safe.height * (IsDuelConfigured ? 0.22f : 0.1f);
        Rect screenArea = MementoGameplayLayout.BoardArea(
            safe, topInset, bottomInset, IsDuelConfigured);
        Canvas canvas = gridRect.GetComponentInParent<Canvas>();
        Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera : null;
        Rect localArea = MementoGameplayLayout.ScreenToLocalRect(screenArea, parent, camera);
        if (localArea.width <= 0f || localArea.height <= 0f)
            return;

        if (gridLayoutInitialized && lastGridLocalArea == localArea &&
            lastGridParentRect == parent.rect && lastLayoutRows == gridRows &&
            lastLayoutColumns == gridColumns)
            return;

        gridLayoutInitialized = true;
        lastGridLocalArea = localArea;
        lastGridParentRect = parent.rect;
        lastLayoutRows = gridRows;
        lastLayoutColumns = gridColumns;
        // There is exactly one owner of the grid bounds. A stretched rect plus
        // ContentSizeFitter previously retained the old orientation's dimensions.
        ContentSizeFitter fitter = gridRect.GetComponent<ContentSizeFitter>();
        if (fitter != null)
            fitter.enabled = false;
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = Mathf.Max(1, gridColumns);
        gridLayout.childAlignment = TextAnchor.MiddleCenter;
        Vector2 cellSize = MementoGameplayLayout.FitCells(
            localArea.size, gridRows, gridColumns, gridLayout.spacing, gridLayout.padding);
        gridLayout.cellSize = cellSize;
        Vector2 size = MementoGameplayLayout.GridSize(
            cellSize, gridRows, gridColumns, gridLayout.spacing, gridLayout.padding);
        MementoGameplayLayout.PlaceInParent(
            gridRect, new Rect(localArea.center - size * 0.5f, size));

        foreach (Card card in cards)
        {
            if (card != null)
                card.FitVisualTo(cellSize);
        }
        LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);
    }

    private void OnCardClicked(Card card)
    {
        if (card == null ||
            IsBusy ||
            (IsDuelActive && duelState.ActiveActor != DuelActor.Player) ||
            flippedCards.Count >= CardsPerMatch ||
            !cardStates.TryGetValue(card, out CardState state) ||
            IsCurtainBlocked(card, DuelActor.Player) ||
            IsGodCardOutOfPlay(card) ||
            state == CardState.FaceUp ||
            state == CardState.Matched)
        {
            return;
        }

        ShowCardFace(card);
        AnimalMemoryJuice.PlayFlip(card.transform as RectTransform);
        MementoMatchSfx.PlayCardFlip();
        cardStates[card] = CardState.FaceUp;
        flippedCards.Add(card);
        ObserveForOpponent(card);
        onTryingMatch?.Invoke();

        if (flippedCards.Count == CardsPerMatch)
        {
            isResolvingMatch = true;
            StartCoroutine(CheckForMatch());
        }
    }

    private IEnumerator CheckForMatch()
    {
        yield return new WaitForSeconds(matchDelay);

        if (flippedCards.Count != CardsPerMatch ||
            flippedCards.Exists(card => card == null || !cardStates.ContainsKey(card)))
        {
            flippedCards.Clear();
            isResolvingMatch = false;
            yield break;
        }

        bool cardsMatch = flippedCards[0].CurrentSprite == flippedCards[1].CurrentSprite;
        bool playerDuelHit = cardsMatch && IsDuelActive;
        RegisterTestimonyAttempt(DuelActor.Player, cardsMatch, flippedCards[0], flippedCards[1]);
        if (cardsMatch)
        {
            guideRunState?.RegisterMatch();
            lastTurnCards.Clear();
            lastTurnCards.AddRange(flippedCards);
            lastTurnAvailable = true;
            lastTurnWasMatch = true;
            matches++;
            UpdateMatchesText();
            onMatchMade?.Invoke();
            if (playerDuelHit)
            {
                int currentCombo = comboSystem != null
                    ? comboSystem.CurrentCombo
                    : 1;
                lastPlayerDuelDamage = playerComboDamageEnabled ? DuelRules.GetPlayerComboDamage(currentCombo) : 1;
                if (HasOpponentTechniqueSuite &&
                    ultimateMasterShieldPending)
                {
                    lastPlayerDuelDamage = Mathf.Max(1, lastPlayerDuelDamage - 1);
                    ultimateMasterShieldPending = false;
                }
            }
            RectTransform firstMatchedCard =
                flippedCards[0].transform as RectTransform;
            RectTransform secondMatchedCard =
                flippedCards[1].transform as RectTransform;
            AnimalMemoryJuice.PlayMatch(firstMatchedCard, secondMatchedCard);
            MementoMatchSfx.PlaySetMatch(
                selectedSetId,
                flippedCards[0].CurrentSprite != null
                    ? flippedCards[0].CurrentSprite.name
                    : string.Empty);
            if (playerDuelHit)
            {
                PlayerDuelPairMade?.Invoke(firstMatchedCard, secondMatchedCard);
            }

            ResolveTestimonyMatched(flippedCards[0], flippedCards[1]);
            foreach (Card card in flippedCards)
            {
                cardStates[card] = CardState.Matched;
                card.ToggleRenderer(false);

                Button button = card.GetComponent<Button>();
                if (button != null)
                {
                    button.interactable = false;
                }
            }

            flippedCards.Clear();
        }
        else
        {
            guideRunState?.RegisterMismatch();
            lastTurnCards.Clear();
            lastTurnCards.AddRange(flippedCards);
            lastTurnAvailable = true;
            lastTurnWasMatch = false;
            onMatchFailed?.Invoke();
            AnimalMemoryJuice.PlayFail(
                flippedCards[0].transform as RectTransform,
                flippedCards[1].transform as RectTransform);
            MementoMatchSfx.PlayMatchFail();
            foreach (Card card in flippedCards)
            {
                cardStates[card] = CardState.FaceDown;
                HideCardUnlessPublic(card);
            }

            flippedCards.Clear();
        }

        int resolvedCombo = comboSystem != null
            ? comboSystem.CurrentCombo
            : cardsMatch ? 1 : 0;
        PlayerMoveResolved?.Invoke(cardsMatch, resolvedCombo);
        ResolveCurtainAfterAttempt(DuelActor.Player);

        if (playerDuelHit)
        {
            yield return new WaitForSecondsRealtime(0.86f);
        }

        bool retainPlayerTurn =
            guideRunState != null && guideRunState.TryConsumeEncore();
        if (!retainPlayerTurn)
        {
            turns++;
            guideRunState?.RegisterTurnCompleted();
            UpdateTurnsText();
            onTurnCompleted?.Invoke(turns);
        }
        isResolvingMatch = false;

        if (IsDuelConfigured && duelState.IsActive)
        {
            yield return CompletePlayerDuelTurn(cardsMatch, retainPlayerTurn,
                cardsMatch ? lastPlayerDuelDamage : 1);
            yield break;
        }

        if (cardsMatch && CheckGameOver())
        {
            AnimalMemoryJuice.PlayVictory();
            onWinEvent?.Invoke();
            Debug.Log("Game Over! You Win!");
        }
    }

    /// <summary>One closure for manual and automatic player pairs, including GOD phases.</summary>
    private IEnumerator CompletePlayerDuelTurn(bool madePair, bool retainTurn, int damage)
    {
            bool boardExhausted = CheckGameOver();
            // The GOD duel never resolves exhaustion through sudden death: its contract
            // is winning by health and its answer to impossibility is Ira/Reescritura.
            bool godRebuildPending = boardExhausted && IsGodDuelConfigured;
            duelState.CompleteTurn(
                DuelActor.Player,
                madePair,
                retainTurn,
                damage,
                boardExhausted: boardExhausted && !godRebuildPending);
            if (IsGodDuelConfigured &&
                godDuel.TryEnterPhaseTwo(
                    duelState.OpponentHealth,
                    DuelRules.GetTerminalAction(duelState) != DuelTerminalAction.Continue))
            {
                // Phase one marks never survive the transition.
                godJudgement.Clear();
                yield return PresentGodPhaseTransition();
            }
            DuelTerminalAction terminalAction =
                DuelRules.GetTerminalAction(duelState);
            if (terminalAction == DuelTerminalAction.SuddenDeath)
            {
                yield return BeginSuddenDeathRound();
                yield break;
            }
            if (terminalAction != DuelTerminalAction.Continue)
            {
                yield return PresentDuelOutcome(terminalAction);
                yield break;
            }
            if (!opponentTurnRunning &&
                duelState.ActiveActor == DuelActor.Opponent)
            {
                MementoMatchSfx.PlayTurnSwap(true);
                StartCoroutine(RunOpponentTurn());
            }
            else if (IsGodDuelConfigured &&
                     duelState.ActiveActor == DuelActor.Player &&
                     godDuel.NeedsRebuild(boardRevision, BuildGodBoardCards()))
            {
                // A player-side impossible board is closed automatically: no click,
                // no failure, no combo loss and no extra turn for the player.
                yield return RebuildGodBoard(true);
            }

    }

    private IEnumerator BeginSuddenDeathRound()
    {
        isResolvingMatch = true;
        opponentTurnRunning = false;
        flippedCards.Clear();
        lastTurnCards.Clear();
        lastTurnAvailable = false;
        lastTurnWasMatch = false;
        DuelOpponentPose = 1;
        duelState.BeginSuddenDeathRound();

        gridRows = 2;
        gridColumns = 2;
        if (!CreateGrid())
        {
            Debug.LogError("Sudden-death board could not be created.");
            isResolvingMatch = false;
            yield break;
        }

        MementoMatchSfx.PlayPower(false);
        yield return RevealCardsThenHide(Mathf.Min(1.15f, timeUntilHide));
        DuelOpponentPose = 0;
        isResolvingMatch = false;
        MementoMatchSfx.PlayTurnSwap(false);
    }

    private IEnumerator RunOpponentTurn()
    {
        if (opponentTurnRunning ||
            !IsDuelActive ||
            duelBrain == null ||
            duelState.ActiveActor != DuelActor.Opponent)
        {
            yield break;
        }

        opponentTurnRunning = true;
        isResolvingMatch = true;
        DuelOpponentPose = 1;
        yield return new WaitForSeconds(0.55f);

        if (IsGodDuelConfigured)
        {
            godDuel.BeginOwnerTurn();
            bool resolvedPendingRay = false;
            if (godJudgement.IsArmed)
            {
                yield return ResolveGodJudgement();
                resolvedPendingRay = true;
            }
            if (godErase.IsArmed)
            {
                yield return ResolveGodErase();
                resolvedPendingRay = true;
            }

            DuelTerminalAction afterPending = DuelRules.GetTerminalAction(duelState);
            if (afterPending != DuelTerminalAction.Continue)
            {
                yield return CloseGodTurn(afterPending);
                yield break;
            }
            if (resolvedPendingRay)
            {
                // A resolved ray consumes GOD's turn: no normal play or second attack after it.
                yield return CloseGodTurn(DuelTerminalAction.Continue);
                yield break;
            }

            GodOwnerTurnPlan plan = godDuel.PlanOwnerTurn(
                boardRevision,
                BuildGodBoardCards(),
                duelTerminal: false);
            if (plan == GodOwnerTurnPlan.Rebuild)
            {
                yield return RebuildGodBoard(false);
                yield break;
            }
            if (plan == GodOwnerTurnPlan.Judgement)
            {
                yield return ArmGodJudgement();
                yield return CloseGodTurn(DuelTerminalAction.Continue);
                yield break;
            }
            if (plan == GodOwnerTurnPlan.Erase)
            {
                yield return ArmGodErase();
                yield return CloseGodTurn(DuelTerminalAction.Continue);
                yield break;
            }
            // NormalPlay keeps the shared attempt below.
        }

        bool postgameAbilityReady =
            HasPostgameAbility && postgameAbilityClock.BeginOwnerTurn();
        if (postgameAbilityReady &&
            configuredPostgameAbility == MementoPostgameAbilityId.NewHand &&
            TryPrepareNewHand(out List<DuelCardId> newHandOrder))
        {
            postgameAbilityClock.CommitCast();
            postgameAbilityActivationSerial++;
            yield return PresentPostgameAbility();
            ApplyNewHandOrder(newHandOrder);
        }
        else if (postgameAbilityReady &&
                 configuredPostgameAbility == MementoPostgameAbilityId.Testimony)
        {
            postgameAbilityClock.CommitCast();
            postgameAbilityActivationSerial++;
            postgameTestimony.ArmRound(boardRevision);
            yield return PresentPostgameAbility();
        }

        int upcomingTurn = duelState.OpponentTurns + 1;
        int skillInterval = DuelRules.GetSkillInterval(configuredDuelGuideId);
        bool skillTurn =
            !IsGodDuelConfigured &&
            (HasOpponentTechniqueSuite ||
             !MementoPostgameEncounters.IsPostgameOpponent(configuredDuelGuideId)) &&
            upcomingTurn % skillInterval == 0;
        if (skillTurn)
        {
            if (HasOpponentTechniqueSuite)
                yield return UltimateMasterSkill();
            else
                yield return OpponentSkillPeek();
        }
        else
        {
            MementoMatchVoicePlayer.PlayTurn(configuredDuelGuideId);
        }

        DuelTerminalAction preTurnAction =
            DuelRules.GetTerminalAction(duelState);
        if (preTurnAction == DuelTerminalAction.SuddenDeath)
        {
            isResolvingMatch = false;
            opponentTurnRunning = false;
            yield return BeginSuddenDeathRound();
            yield break;
        }
        if (preTurnAction != DuelTerminalAction.Continue)
        {
            yield return PresentDuelOutcome(preTurnAction);
            yield break;
        }

        List<int> available = GetSelectableDuelOrdinals(DuelActor.Opponent);
        if (available.Count < CardsPerMatch)
        {
            if (IsGodDuelConfigured)
            {
                // GOD answers a structurally impossible board with a rebuild.
                yield return RebuildGodBoard(false);
                yield break;
            }
            // Defensive terminal path: a duel must never wait for an AI move
            // when fewer than two selectable cards remain.
            if (duelState.IsActive)
                duelState.ResolveBoardExhausted(DuelActor.Opponent);
            DuelTerminalAction emptyBoardAction =
                DuelRules.GetTerminalAction(duelState);
            if (emptyBoardAction == DuelTerminalAction.SuddenDeath)
            {
                isResolvingMatch = false;
                opponentTurnRunning = false;
                yield return BeginSuddenDeathRound();
                yield break;
            }
            if (emptyBoardAction != DuelTerminalAction.Continue)
            {
                yield return PresentDuelOutcome(emptyBoardAction);
                yield break;
            }

            Debug.LogError(
                "Duel has fewer than two selectable cards but no terminal outcome.");
            isResolvingMatch = false;
            opponentTurnRunning = false;
            yield break;
        }

        int firstOrdinal = duelBrain.ChooseFirst(available);
        Card first = FindCardByDuelOrdinal(firstOrdinal);
        if (first == null)
            throw new InvalidOperationException("Opponent selected a stale duel card id.");
        ShowCardFace(first);
        cardStates[first] = CardState.FaceUp;
        ObserveForOpponent(first);
        AnimalMemoryJuice.PlayFlip(first.transform as RectTransform);
        MementoMatchSfx.PlayCardFlip();
        yield return new WaitForSeconds(0.65f);

        available = GetSelectableDuelOrdinals(DuelActor.Opponent);
        int secondOrdinal = duelBrain.ChooseSecond(firstOrdinal, available);
        Card second = FindCardByDuelOrdinal(secondOrdinal);
        if (second == null)
            throw new InvalidOperationException("Opponent selected a stale second duel card id.");
        ShowCardFace(second);
        cardStates[second] = CardState.FaceUp;
        ObserveForOpponent(second);
        AnimalMemoryJuice.PlayFlip(second.transform as RectTransform);
        MementoMatchSfx.PlayCardFlip();
        yield return new WaitForSeconds(0.8f);

        bool cardsMatch = cardPairIds[first] == cardPairIds[second];
        RegisterTestimonyAttempt(DuelActor.Opponent, cardsMatch, first, second);
        if (cardsMatch)
        {
            RectTransform firstMatchedCard = first.transform as RectTransform;
            RectTransform secondMatchedCard = second.transform as RectTransform;
            AnimalMemoryJuice.PlayMatch(firstMatchedCard, secondMatchedCard);
            MementoMatchSfx.PlaySetMatch(
                selectedSetId,
                first.CurrentSprite != null
                    ? first.CurrentSprite.name
                    : string.Empty);
            OpponentDuelPairMade?.Invoke(firstMatchedCard, secondMatchedCard);
            ResolveTestimonyMatched(first, second);
            cardStates[first] = CardState.Matched;
            cardStates[second] = CardState.Matched;
            first.ToggleRenderer(false);
            second.ToggleRenderer(false);
            SetCardInteractable(first, false);
            SetCardInteractable(second, false);
        }
        else
        {
            cardStates[first] = CardState.FaceDown;
            cardStates[second] = CardState.FaceDown;
            HideCardUnlessPublic(first);
            HideCardUnlessPublic(second);
            AnimalMemoryJuice.PlayFail(
                first.transform as RectTransform,
                second.transform as RectTransform);
            MementoMatchSfx.PlayMatchFail();
        }

        if (cardsMatch)
            yield return new WaitForSecondsRealtime(0.86f);

        turns++;
        UpdateTurnsText();
        bool retainOpponentTurn =
            skillTurn && cardsMatch &&
            (configuredDuelGuideId == AnimalMemoryContentIds.MomoGuide ||
             (HasOpponentTechniqueSuite &&
              ultimateMasterActiveTechnique == AnimalMemoryContentIds.MomoGuide));
        if (HasOpponentTechniqueSuite &&
            cardsMatch && ultimateMasterUndoPending)
        {
            // Deshacer only protects the immediately following attempt.
            ultimateMasterUndoPending = false;
        }
        ResolveCurtainAfterAttempt(DuelActor.Opponent);
        if (HasOpponentTechniqueSuite &&
            !cardsMatch && ultimateMasterUndoPending)
        {
            // Rei uses Aki's Deshacer only on her own mistake. It grants one
            // fair retry without claiming mismatched cards or erasing player progress.
            retainOpponentTurn = true;
            ultimateMasterUndoPending = false;
        }
        bool opponentBoardExhausted = CheckGameOver();
        // The GOD duel answers exhaustion with a rebuild, never with sudden death.
        bool godOpponentRebuild = opponentBoardExhausted && IsGodDuelConfigured;
        duelState.CompleteTurn(
            DuelActor.Opponent,
            cardsMatch,
            retainOpponentTurn,
            boardExhausted: opponentBoardExhausted && !godOpponentRebuild);
        if (godOpponentRebuild && duelState.IsActive)
        {
            // The rebuild occupies GOD's action in the same closure: one transition,
            // no second turn and no free attack on the freshly dealt board.
            yield return RebuildGodBoard(true);
            yield break;
        }
        if (duelState.RequiresSuddenDeath)
        {
            isResolvingMatch = false;
            opponentTurnRunning = false;
            yield return BeginSuddenDeathRound();
            yield break;
        }
        duelBrain.DecayMemory();
        isResolvingMatch = false;
        opponentTurnRunning = false;
        DuelOpponentPose = 0;

        DuelTerminalAction completedTurnAction =
            DuelRules.GetTerminalAction(duelState);
        if (completedTurnAction != DuelTerminalAction.Continue)
        {
            yield return PresentDuelOutcome(completedTurnAction);
            yield break;
        }

        if (postgameAbilityReady &&
            configuredPostgameAbility == MementoPostgameAbilityId.Curtain &&
            duelState.ActiveActor == DuelActor.Player &&
            TryArmCurtain())
        {
            postgameAbilityClock.CommitCast();
            postgameAbilityActivationSerial++;
            yield return PresentPostgameAbility();
            RefreshCardInteractability();
        }

        if (duelState.ActiveActor == DuelActor.Opponent)
        {
            MementoMatchSfx.PlayTurnSwap(true);
            yield return new WaitForSecondsRealtime(0.35f);
            StartCoroutine(RunOpponentTurn());
        }
        else
        {
            MementoMatchSfx.PlayTurnSwap(false);
        }
    }

    private IEnumerator PresentDuelOutcome(
        DuelTerminalAction terminalAction)
    {
        if (terminalAction != DuelTerminalAction.PlayerVictory &&
            terminalAction != DuelTerminalAction.PlayerDefeat)
        {
            throw new InvalidOperationException(
                $"Cannot present non-terminal duel action {terminalAction}.");
        }

        bool playerWon = terminalAction == DuelTerminalAction.PlayerVictory;
        isResolvingMatch = true;
        opponentTurnRunning = !playerWon;
        DuelOpponentPose = playerWon ? 3 : 4;
        duelOutcomeAcknowledged = DuelOutcomePresentationRequested == null;
        DuelOutcomePresentationRequested?.Invoke(
            configuredDuelGuideId,
            playerWon);
        if (playerWon)
            AnimalMemoryJuice.PlayVictory();

        yield return WaitForDuelOutcomePresentation(playerWon);
        isResolvingMatch = false;
        opponentTurnRunning = false;
        if (playerWon)
            onWinEvent?.Invoke();
        else
            onDuelLost?.Invoke();
    }

    public bool CanContinueDefeat => IsDuelConfigured && !IsBusy && duelState.CanContinueDefeat;

    /// <summary>Continues an ended duel only after the reward boundary has authorized it.</summary>
    public bool TryContinueDefeat()
    {
        if (!CanContinueDefeat) return false;
        bool exhausted = GetFaceDownCardIndices().Count < CardsPerMatch;
        if (!duelState.TryContinueDefeat(exhausted)) return false;
        StopAllCoroutines();
        skillPresentationUntil = 0f;
        flippedCards.Clear();
        lastTurnCards.Clear();
        lastTurnAvailable = false;
        lastTurnWasMatch = false;
        duelOutcomeAcknowledged = true;
        isResolvingMatch = false;
        isRevealingCards = false;
        opponentTurnRunning = false;
        DuelOpponentPose = 0;
        if (exhausted)
            StartCoroutine(BeginSuddenDeathRound());
        else
        {
            foreach (var entry in cardStates)
                SetCardInteractable(entry.Key, entry.Value == CardState.FaceDown);
            MementoMatchSfx.PlayTurnSwap(false);
        }
        return true;
    }

    public void AcknowledgeDuelOutcomePresentation()
    {
        duelOutcomeAcknowledged = true;
    }

    private IEnumerator WaitForDuelOutcomePresentation(bool playerWon)
    {
        float minimumDuration = GetDuelOutcomePresentationSeconds(playerWon);
        float elapsed = 0f;
        while (!duelOutcomeAcknowledged && elapsed < minimumDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }

        // The dialogue Continue action normally acknowledges this immediately.
        // A bounded fallback avoids a softlock if a presentation view is removed.
        float timeout = minimumDuration + 12f;
        while (!duelOutcomeAcknowledged && elapsed < timeout)
        {
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
    }

    private float GetDuelOutcomePresentationSeconds(bool playerWon)
    {
        float spokenLineDuration = MementoMatchVoicePlayer.GetOutcomeDuration(
            configuredDuelGuideId,
            playerWon);
        return Mathf.Max(
            DuelOutcomePresentationSeconds,
            spokenLineDuration + 0.25f);
    }

    private IEnumerator UltimateMasterSkill()
    {
        ultimateMasterActiveTechnique = MementoPostgameEncounters.TryGetOpponent(configuredDuelGuideId, out var profile) && profile.TechniqueGuideId >= 0
            ? profile.TechniqueGuideId
            : ultimateMasterSkillIndex % AnimalMemoryContentIds.GuideCount;
        ultimateMasterSkillIndex++;
        DuelOpponentPose = 2;
        HoldForSkillPresentation(SkillCutInSeconds);
        OpponentDuelSkillActivated?.Invoke(configuredDuelGuideId);
        MementoMatchSfx.PlayPower(true);
        yield return WaitForSkillPresentation();

        switch (ultimateMasterActiveTechnique)
        {
            case AnimalMemoryContentIds.AkiGuide:
                ultimateMasterUndoPending = true;
                break;
            case AnimalMemoryContentIds.MikaGuide:
                ultimateMasterShieldPending = true;
                break;
            case AnimalMemoryContentIds.YoruGuide:
                yield return OpponentSkillPeek(true, false);
                break;
            case AnimalMemoryContentIds.HanaGuide:
                yield return OpponentSkillBloom();
                break;
            case AnimalMemoryContentIds.MomoGuide:
                break;
        }

        DuelOpponentPose = 1;
    }

    private IEnumerator OpponentSkillBloom()
    {
        if (!TryFindHiddenPair(out Card first, out Card second))
            yield break;

        ShowCardFace(first);
        ShowCardFace(second);
        AnimalMemoryJuice.PlayFlip(first.transform as RectTransform);
        AnimalMemoryJuice.PlayFlip(second.transform as RectTransform);
        MementoMatchSfx.PlayCardFlip();
        yield return new WaitForSecondsRealtime(0.42f);

        RectTransform firstRect = first.transform as RectTransform;
        RectTransform secondRect = second.transform as RectTransform;
        AnimalMemoryJuice.PlayMatch(firstRect, secondRect);
        MementoMatchSfx.PlaySetMatch(
            selectedSetId,
            first.CurrentSprite != null ? first.CurrentSprite.name : string.Empty);
        OpponentDuelPairMade?.Invoke(firstRect, secondRect);
        cardStates[first] = CardState.Matched;
        cardStates[second] = CardState.Matched;
        first.ToggleRenderer(false);
        second.ToggleRenderer(false);
        SetCardInteractable(first, false);
        SetCardInteractable(second, false);

        duelState.ApplyOpponentBonusPair(
            boardExhausted: CheckGameOver());
        yield return new WaitForSecondsRealtime(0.86f);
    }

    private IEnumerator OpponentSkillPeek(bool forcePair = false, bool showCutIn = true)
    {
        List<int> available = GetFaceDownCardIndices();
        if (available.Count == 0)
            yield break;

        if (showCutIn)
        {
            DuelOpponentPose = 2;
            HoldForSkillPresentation(SkillCutInSeconds);
            OpponentDuelSkillActivated?.Invoke(configuredDuelGuideId);
            MementoMatchSfx.PlayPower(true);
            yield return WaitForSkillPresentation();
        }
        int peekCount = DuelRules.GetSkillPeekCount(configuredDuelGuideId);
        IReadOnlyList<int> targets;
        if ((forcePair || configuredDuelGuideId == AnimalMemoryContentIds.YoruGuide) &&
            TryFindHiddenPair(out Card visionFirst, out Card visionSecond) &&
            cardIndices.TryGetValue(visionFirst, out int visionFirstIndex) &&
            cardIndices.TryGetValue(visionSecond, out int visionSecondIndex))
        {
            targets = new[] { visionFirstIndex, visionSecondIndex };
        }
        else
        {
            targets = duelBrain.ChoosePeekTargets(available, peekCount);
        }

        for (int i = 0; i < targets.Count; i++)
        {
            Card card = cards[targets[i]];
            ShowCardFace(card);
            ObserveForOpponent(card);
            AnimalMemoryJuice.PlayFlip(card.transform as RectTransform);
        }

        yield return new WaitForSeconds(0.9f);

        for (int i = 0; i < targets.Count; i++)
        {
            Card card = cards[targets[i]];
            if (card != null &&
                cardStates.TryGetValue(card, out CardState state) &&
                state == CardState.FaceDown)
                HideCardUnlessPublic(card);
        }

        DuelOpponentPose = 1;
    }

    private IEnumerator PresentPostgameAbility()
    {
        DuelOpponentPose = 2;
        HoldForSkillPresentation(SkillCutInSeconds);
        OpponentDuelSkillActivated?.Invoke(configuredDuelGuideId);
        MementoMatchSfx.PlayPower(true);
        yield return WaitForSkillPresentation();
        DuelOpponentPose = 1;
    }

    private bool TryPrepareNewHand(out List<DuelCardId> newOrder)
    {
        var unresolved = cards
            .Where(card => card != null &&
                           cardStates.TryGetValue(card, out CardState state) &&
                           state == CardState.FaceDown &&
                           duelCardIds.ContainsKey(card))
            .OrderBy(card => cardIndices[card])
            .ToList();

        var ids = new List<DuelCardId>(unresolved.Count);
        var pairIds = new List<int>(unresolved.Count);
        foreach (Card card in unresolved)
        {
            ids.Add(duelCardIds[card]);
            pairIds.Add(cardPairIds[card]);
        }

        var random = new SystemDuelRandom(
            PostgameAbilityRules.DeriveAbilitySeed(
                duelSeed,
                boardRevision,
                postgameAbilityActivationSerial));
        return PostgameAbilityRules.TryBuildNewHandOrder(
            ids,
            pairIds,
            random,
            out newOrder);
    }

    private void ApplyNewHandOrder(IReadOnlyList<DuelCardId> newOrder)
    {
        var unresolvedSlots = cards
            .Where(card => card != null &&
                           cardStates.TryGetValue(card, out CardState state) &&
                           state == CardState.FaceDown &&
                           cardIndices.ContainsKey(card))
            .Select(card => cardIndices[card])
            .OrderBy(slot => slot)
            .ToList();

        if (unresolvedSlots.Count != newOrder.Count)
            throw new InvalidOperationException("Nueva mano order no longer matches unresolved cards.");

        var cardsByOrdinal = new Dictionary<int, Card>();
        foreach (Card card in cards)
        {
            if (card != null && duelCardIds.TryGetValue(card, out DuelCardId cardId))
                cardsByOrdinal[cardId.Ordinal] = card;
        }

        for (int i = 0; i < unresolvedSlots.Count; i++)
        {
            if (!cardsByOrdinal.TryGetValue(newOrder[i].Ordinal, out Card movedCard))
                throw new InvalidOperationException("Nueva mano referenced a stale card id.");
            cards[unresolvedSlots[i]] = movedCard;
        }

        for (int slot = 0; slot < cards.Count; slot++)
        {
            Card card = cards[slot];
            if (card == null)
                continue;
            cardIndices[card] = slot;
            card.name = $"Card_{slot}";
            card.transform.SetSiblingIndex(slot);
        }

        duelBrain?.ForgetObservedCards();
        ReobservePublicTestimonyCards();
        RefreshCardInteractability();
        if (gridRect != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(gridRect);
    }

    private bool TryArmCurtain()
    {
        var selectableSlots = GetFaceDownCardIndices();
        var pairIdsBySlot = new List<int>(cards.Count);
        for (int slot = 0; slot < cards.Count; slot++)
        {
            Card card = cards[slot];
            pairIdsBySlot.Add(
                card != null && cardPairIds.TryGetValue(card, out int pairId)
                    ? pairId
                    : int.MinValue);
        }

        var random = new SystemDuelRandom(
            PostgameAbilityRules.DeriveAbilitySeed(
                duelSeed,
                boardRevision,
                postgameAbilityActivationSerial));
        if (!PostgameAbilityRules.TryChooseCurtainRow(
                gridRows,
                gridColumns,
                selectableSlots,
                pairIdsBySlot,
                random,
                out int row))
            return false;

        postgameCurtain.Arm(boardRevision, row, DuelActor.Player);
        return true;
    }

    private List<int> GetSelectableDuelOrdinals(DuelActor actor)
    {
        var result = new List<int>();
        foreach (Card card in cards)
        {
            if (card == null ||
                !cardStates.TryGetValue(card, out CardState state) ||
                state != CardState.FaceDown ||
                IsCurtainBlocked(card, actor) ||
                !duelCardIds.TryGetValue(card, out DuelCardId cardId) ||
                IsGodCardOutOfPlay(cardId))
                continue;
            result.Add(cardId.Ordinal);
        }

        result.Sort();
        return result;
    }

    private Card FindCardByDuelOrdinal(int ordinal)
    {
        foreach (Card card in cards)
        {
            if (card != null &&
                duelCardIds.TryGetValue(card, out DuelCardId cardId) &&
                cardId.BoardRevision == boardRevision &&
                cardId.Ordinal == ordinal)
                return card;
        }
        return null;
    }

    private bool IsCurtainBlocked(Card card, DuelActor actor)
    {
        return card != null &&
               duelCardIds.TryGetValue(card, out DuelCardId cardId) &&
               cardIndices.TryGetValue(card, out int slot) &&
               postgameCurtain.Blocks(cardId, slot, gridColumns, actor);
    }

    private void ResolveCurtainAfterAttempt(DuelActor actor)
    {
        if (postgameCurtain.RegisterAttempt(actor))
            RefreshCardInteractability();
    }

    private void RegisterTestimonyAttempt(
        DuelActor actor,
        bool matched,
        Card first,
        Card second)
    {
        if (first == null || second == null ||
            !duelCardIds.TryGetValue(first, out DuelCardId firstId) ||
            !duelCardIds.TryGetValue(second, out DuelCardId secondId))
            return;

        bool captured = postgameTestimony.RegisterAttempt(
            actor,
            matched,
            firstId,
            secondId);
        if (!captured)
            return;

        ShowCardFace(first);
        ShowCardFace(second);
        ObserveForOpponent(first);
        ObserveForOpponent(second);
    }

    private void ResolveTestimonyMatched(Card first, Card second)
    {
        if (first == null || second == null ||
            !duelCardIds.TryGetValue(first, out DuelCardId firstId) ||
            !duelCardIds.TryGetValue(second, out DuelCardId secondId))
            return;
        postgameTestimony.ResolveMatched(firstId, secondId);
    }

    private bool IsTestimonyPublic(Card card)
    {
        return card != null &&
               duelCardIds.TryGetValue(card, out DuelCardId cardId) &&
               postgameTestimony.IsPublic(cardId);
    }

    private void HideCardUnlessPublic(Card card)
    {
        if (IsGodCardOutOfPlay(card))
        {
            // An invalidated card stays visible with its own signal: it must never
            // look like a matched card nor like a normal hidden card.
            ShowCardFace(card);
            ApplyGodOutOfPlayTint(card);
            return;
        }
        if (IsTestimonyPublic(card))
            ShowCardFace(card);
        else
            HideCardFace(card);
    }

    private void ApplyGodOutOfPlayTint(Card card)
    {
        if (card == null || !duelCardIds.TryGetValue(card, out DuelCardId cardId))
            return;
        if (godErasedOrdinals.Contains(cardId.Ordinal))
            card.VisualTint = GodErasedTint;
        else if (godOrphanOrdinals.Contains(cardId.Ordinal))
            card.VisualTint = GodOrphanTint;
    }

    private void ReobservePublicTestimonyCards()
    {
        if (duelBrain == null || postgameTestimony.PublicCardCount == 0)
            return;
        foreach (Card card in cards)
        {
            if (card != null && IsTestimonyPublic(card))
                ObserveForOpponent(card);
        }
    }

    private void RefreshCardInteractability()
    {
        foreach (Card card in cards)
        {
            if (card == null)
                continue;
            bool interactable =
                cardStates.TryGetValue(card, out CardState state) &&
                state == CardState.FaceDown &&
                !IsCurtainBlocked(card, DuelActor.Player) &&
                !IsGodCardOutOfPlay(card);
            SetCardInteractable(card, interactable);
        }
    }

    private List<int> GetFaceDownCardIndices()
    {
        List<int> result = new List<int>();
        foreach (KeyValuePair<Card, CardState> entry in cardStates)
        {
            if (entry.Key != null &&
                entry.Value == CardState.FaceDown &&
                cardIndices.TryGetValue(entry.Key, out int index))
                result.Add(index);
        }

        result.Sort();
        return result;
    }

    private bool IsGodCardOutOfPlay(Card card)
    {
        return card != null &&
               duelCardIds.TryGetValue(card, out DuelCardId cardId) &&
               IsGodCardOutOfPlay(cardId);
    }

    private bool IsGodCardOutOfPlay(DuelCardId cardId)
    {
        return IsGodDuelConfigured &&
               (godErasedOrdinals.Contains(cardId.Ordinal) ||
                godOrphanOrdinals.Contains(cardId.Ordinal));
    }

    private List<GodBoardCard> BuildGodBoardCards()
    {
        var result = new List<GodBoardCard>(cards.Count);
        foreach (Card card in cards)
        {
            if (card == null ||
                !cardStates.TryGetValue(card, out CardState state) ||
                !duelCardIds.TryGetValue(card, out DuelCardId cardId) ||
                !cardPairIds.TryGetValue(card, out int pairId))
                continue;

            GodBoardCardStatus status;
            if (godErasedOrdinals.Contains(cardId.Ordinal))
                status = GodBoardCardStatus.Erased;
            else if (godOrphanOrdinals.Contains(cardId.Ordinal))
                status = GodBoardCardStatus.Orphaned;
            else if (state == CardState.Matched)
                status = GodBoardCardStatus.Resolved;
            else
                status = GodBoardCardStatus.Live;

            result.Add(new GodBoardCard(cardId, pairId, status));
        }
        return result;
    }

    private SystemDuelRandom CreateGodSkillRandom()
    {
        return new SystemDuelRandom(unchecked(
            duelSeed * 374761393 + boardRevision * 668265263 + godSkillSerial * 1274126177));
    }

    private IEnumerator ArmGodJudgement()
    {
        List<GodBoardCard> boardCards = BuildGodBoardCards();
        if (GodDuelRules.TryChooseJudgementTargets(
                boardRevision, boardCards, CreateGodSkillRandom(), out List<GodBoardCard> targets) &&
            godJudgement.Arm(boardRevision, targets))
        {
            godDuel.CommitJudgement();
            godSkillSerial++;
            DuelOpponentPose = 2;
            HoldForSkillPresentation(SkillCutInSeconds);
            OpponentDuelSkillActivated?.Invoke(configuredDuelGuideId);
            MementoMatchSfx.PlayPower(true);
            yield return WaitForSkillPresentation();
            GodJudgementArmed?.Invoke(godJudgement.TargetPairCount);
            DuelOpponentPose = 1;
        }
    }

    private IEnumerator ResolveGodJudgement()
    {
        godJudgement.TryResolve(BuildGodBoardCards(), out List<GodBoardCard> struck);
        int pairs = struck.Count / 2;
        if (pairs > 0)
        {
            DuelOpponentPose = 2;
            HoldForSkillPresentation(SkillCutInSeconds);
            MementoMatchSfx.PlayPower(true);
            yield return WaitForSkillPresentation();
            int resolvedPairs = 0;
            for (int i = 0; i < pairs; i++)
            {
                if (DuelRules.GetTerminalAction(duelState) != DuelTerminalAction.Continue)
                    break;
                Card first = FindCardByDuelOrdinal(struck[i * 2].CardId.Ordinal);
                Card second = FindCardByDuelOrdinal(struck[i * 2 + 1].CardId.Ordinal);
                ResolveGodStruckCard(first);
                ResolveGodStruckCard(second);
                duelState.ApplyOpponentBonusPair(boardExhausted: false);
                resolvedPairs++;
            }
            GodJudgementResolved?.Invoke(resolvedPairs);
        }
        DuelOpponentPose = 1;
        yield return new WaitForSecondsRealtime(0.6f);
    }

    private void ResolveGodStruckCard(Card card)
    {
        if (card == null)
            return;
        cardStates[card] = CardState.Matched;
        card.ToggleRenderer(false);
        SetCardInteractable(card, false);
    }

    private IEnumerator ArmGodErase()
    {
        List<GodBoardCard> boardCards = BuildGodBoardCards();
        if (GodDuelRules.TryChooseEraseTarget(
                boardRevision, boardCards, godErasedOrdinals, CreateGodSkillRandom(), out DuelCardId target) &&
            godErase.Arm(boardRevision, target, boardCards))
        {
            godDuel.CommitErase();
            godSkillSerial++;
            DuelOpponentPose = 2;
            HoldForSkillPresentation(SkillCutInSeconds);
            OpponentDuelSkillActivated?.Invoke(configuredDuelGuideId);
            MementoMatchSfx.PlayPower(true);
            yield return WaitForSkillPresentation();
            GodEraseArmed?.Invoke(target.Ordinal);
            DuelOpponentPose = 1;
        }
    }

    private IEnumerator ResolveGodErase()
    {
        int targetOrdinal = godErase.Target.Ordinal;
        bool applied = godErase.TryResolve(
            BuildGodBoardCards(), out List<GodBoardCard> updated, out bool saved);
        if (applied)
        {
            DuelOpponentPose = 2;
            HoldForSkillPresentation(SkillCutInSeconds);
            MementoMatchSfx.PlayPower(true);
            yield return WaitForSkillPresentation();
            foreach (GodBoardCard boardCard in updated)
            {
                if (boardCard.Status == GodBoardCardStatus.Erased)
                    godErasedOrdinals.Add(boardCard.CardId.Ordinal);
                else if (boardCard.Status == GodBoardCardStatus.Orphaned)
                    godOrphanOrdinals.Add(boardCard.CardId.Ordinal);
            }
            Card erasedCard = FindCardByDuelOrdinal(targetOrdinal);
            if (erasedCard != null)
            {
                cardStates[erasedCard] = CardState.FaceDown;
                ShowCardFace(erasedCard);
                erasedCard.VisualTint = GodErasedTint;
                SetCardInteractable(erasedCard, false);
            }
            foreach (int orphanOrdinal in godOrphanOrdinals)
            {
                Card orphan = FindCardByDuelOrdinal(orphanOrdinal);
                if (orphan != null)
                {
                    cardStates[orphan] = CardState.FaceDown;
                    ShowCardFace(orphan);
                    orphan.VisualTint = GodOrphanTint;
                    SetCardInteractable(orphan, false);
                }
            }
            // Erasing never scores: no pair, no combo, no reward and no damage.
        }
        else if (saved)
        {
            GodEraseSaved?.Invoke();
        }
        DuelOpponentPose = 1;
        yield return new WaitForSecondsRealtime(0.6f);
    }

    /// <summary>One-shot visible transition: the reclaim pose holds for a bounded beat
    /// and the event fires exactly once, even if the presentation is skipped.</summary>
    private IEnumerator PresentGodPhaseTransition()
    {
        if (godPhaseTransitionPresented)
            yield break;
        godPhaseTransitionPresented = true;
        guideRunState?.EnableAlliance(presentAllianceMask);
        DuelOpponentPose = 3;
        HoldForSkillPresentation(MementoGodAscension.Duration);
        GodPhaseTransitioned?.Invoke();
        var menu = UnityEngine.Object.FindFirstObjectByType<AnimalMemoryMenuController>();
        var document = menu != null ? menu.GetComponent<UnityEngine.UIElements.UIDocument>() : null;
        MementoGodAscension cinematic = null;
        if (document != null && document.rootVisualElement != null)
        {
            cinematic = new MementoGodAscension();
            document.rootVisualElement.Add(cinematic);
            cinematic.BringToFront();
            cinematic.Play(SkipSkillPresentation, () => MementoMatchSfx.PlayPower(true));
        }
        else
        {
            MementoMatchSfx.PlayPower(true);
        }
        try
        {
            yield return WaitForSkillPresentation();
        }
        finally
        {
            cinematic?.RemoveFromHierarchy();
        }
        DuelOpponentPose = 1;
    }

    private IEnumerator CloseGodTurn(DuelTerminalAction terminalAction)
    {
        DuelOpponentPose = 0;
        isResolvingMatch = false;
        opponentTurnRunning = false;
        if (terminalAction == DuelTerminalAction.Continue)
        {
            if (godDuel.NeedsRebuild(boardRevision, BuildGodBoardCards()))
            {
                yield return RebuildGodBoard(false);
                yield break;
            }
            duelState.CompleteTurn(DuelActor.Opponent, false, false, 1, false);
            terminalAction = DuelRules.GetTerminalAction(duelState);
        }
        if (terminalAction == DuelTerminalAction.SuddenDeath)
        {
            yield return RebuildGodBoard(false);
            yield break;
        }
        if (terminalAction != DuelTerminalAction.Continue)
        {
            yield return PresentDuelOutcome(terminalAction);
            yield break;
        }
        duelBrain?.DecayMemory();
        MementoMatchSfx.PlayTurnSwap(false);
    }

    private IEnumerator RebuildGodBoard(bool fromPlayerTurn)
    {
        if (!IsGodDuelConfigured || !duelState.IsActive)
            yield break;

        if (!godDuel.BeginRebuild(boardRevision + 1, out int rebuildSerial))
            yield break;

        isResolvingMatch = true;
        opponentTurnRunning = true;
        godJudgement.Clear();
        godErase.Clear();
        godErasedOrdinals.Clear();
        godOrphanOrdinals.Clear();
        DuelOpponentPose = 3;
        HoldForSkillPresentation(0.35f);
        yield return WaitForSkillPresentation();

        if (!CreateGrid())
        {
            Debug.LogError("GOD rebuild could not create the replacement board.");
            isResolvingMatch = false;
            opponentTurnRunning = false;
            DuelOpponentPose = 0;
            yield break;
        }

        isResolvingMatch = true;
        yield return RevealCardsThenHide(Mathf.Min(1.15f, timeUntilHide));
        DuelOpponentPose = 0;
        isResolvingMatch = false;
        opponentTurnRunning = false;
        if (!fromPlayerTurn)
            duelState.CompleteTurn(DuelActor.Opponent, false, false, 1, false);
        MementoMatchSfx.PlayTurnSwap(false);
        GodRebuildCompleted?.Invoke(rebuildSerial);
    }

    private void ObserveForOpponent(Card card)
    {
        if (duelBrain == null || card == null)
            return;
        if (duelCardIds.TryGetValue(card, out DuelCardId cardId) &&
            cardPairIds.TryGetValue(card, out int pairId))
            duelBrain.Observe(cardId, pairId);
    }

    private static void SetCardInteractable(Card card, bool interactable)
    {
        if (card == null)
            return;
        Button button = card.GetComponent<Button>();
        if (button != null)
            button.interactable = interactable;
    }

    public bool TryUseGuidePower()
    {
        if (guideRunState == null || IsBusy || IsGuidePowerBlocked)
            return false;

        switch (guideRunState.SelectedGuideId)
        {
            case AnimalMemoryContentIds.AkiGuide:
                if (!lastTurnAvailable ||
                    lastTurnCards.Count != CardsPerMatch ||
                    !guideRunState.TryActivatePower())
                    return false;

                Card undoFirst = lastTurnCards[0];
                Card undoSecond = lastTurnCards[1];
                bool undoWasMatch = lastTurnWasMatch;
                lastTurnAvailable = false;
                lastTurnWasMatch = false;
                lastTurnCards.Clear();
                turns = Mathf.Max(0, turns - 1);
                UpdateTurnsText();

                if (undoWasMatch)
                {
                    RestoreMatchedCard(undoFirst);
                    RestoreMatchedCard(undoSecond);
                    matches = Mathf.Max(0, matches - 1);
                    UpdateMatchesText();
                    if (IsDuelConfigured)
                        duelState.UndoPlayerPair(lastPlayerDuelDamage);
                }

                onMatchUndone?.Invoke();
                StartCoroutine(PulseSpecificCards(undoFirst, undoSecond, 1.05f));
                return true;

            case AnimalMemoryContentIds.MikaGuide:
                return guideRunState.TryActivatePower();

            case AnimalMemoryContentIds.YoruGuide:
                if (!guideRunState.IsPowerReady)
                    return false;

                CancelPartialSelection();
                if (!TryFindHiddenPair(out Card first, out Card second) ||
                    !guideRunState.TryActivatePower())
                    return false;

                StartCoroutine(PulseSpecificCards(first, second, 2.2f));
                return true;

            case AnimalMemoryContentIds.HanaGuide:
                if (!guideRunState.IsPowerReady)
                    return false;

                CancelPartialSelection();
                if (!TryFindHiddenPair(out Card bloomFirst, out Card bloomSecond) ||
                    !guideRunState.TryActivatePower())
                    return false;

                StartCoroutine(ResolveHanaBloom(bloomFirst, bloomSecond));
                return true;

            case AnimalMemoryContentIds.MomoGuide:
                return guideRunState.TryActivatePower();

            default:
                return false;
        }
    }

    public void UseToolkitHud(bool enabled)
    {
        useToolkitHud = enabled;
        if (matchesText != null)
            matchesText.gameObject.SetActive(!enabled);
        if (turnsText != null)
            turnsText.gameObject.SetActive(!enabled);
        if (revealCardsText != null)
            revealCardsText.gameObject.SetActive(!enabled);
    }

    private bool TryFindHiddenPair(out Card first, out Card second)
    {
        first = null;
        second = null;
        for (int i = 0; i < cards.Count && first == null; i++)
        {
            Card candidate = cards[i];
            if (candidate == null ||
                !cardStates.TryGetValue(candidate, out CardState state) ||
                state != CardState.FaceDown ||
                IsGodCardOutOfPlay(candidate) ||
                (IsDuelActive && IsCurtainBlocked(candidate, duelState.ActiveActor)) ||
                !cardPairIds.TryGetValue(candidate, out int pairId))
                continue;

            for (int j = i + 1; j < cards.Count; j++)
            {
                Card possibleMatch = cards[j];
                if (possibleMatch != null &&
                    cardStates.TryGetValue(possibleMatch, out CardState possibleState) &&
                    possibleState == CardState.FaceDown &&
                    !IsGodCardOutOfPlay(possibleMatch) &&
                    (!IsDuelActive || !IsCurtainBlocked(possibleMatch, duelState.ActiveActor)) &&
                    cardPairIds.TryGetValue(possibleMatch, out int possiblePairId) &&
                    pairId == possiblePairId)
                {
                    first = candidate;
                    second = possibleMatch;
                    break;
                }
            }
        }

        return first != null && second != null;
    }

    private void CancelPartialSelection()
    {
        if (flippedCards.Count == 0)
            return;

        foreach (Card card in flippedCards)
        {
            if (card != null &&
                cardStates.TryGetValue(card, out CardState state) &&
                state == CardState.FaceUp)
            {
                cardStates[card] = CardState.FaceDown;
                HideCardUnlessPublic(card);
            }
        }

        flippedCards.Clear();
    }

    private void RestoreMatchedCard(Card card)
    {
        if (card == null || !cardStates.ContainsKey(card))
            return;

        cardStates[card] = CardState.FaceDown;
        card.ToggleRenderer(true);
        HideCardFace(card);
        SetCardInteractable(card, true);
    }

    private IEnumerator ResolveHanaBloom(Card first, Card second)
    {
        isResolvingMatch = true;
        yield return WaitForSkillPresentation();
        ShowCardFace(first);
        ShowCardFace(second);
        AnimalMemoryJuice.PlayFlip(first.transform as RectTransform);
        AnimalMemoryJuice.PlayFlip(second.transform as RectTransform);
        MementoMatchSfx.PlayCardFlip();
        yield return new WaitForSecondsRealtime(0.42f);

        guideRunState?.RegisterMatch();
        matches++;
        UpdateMatchesText();
        onMatchMade?.Invoke();

        RectTransform firstRect = first.transform as RectTransform;
        RectTransform secondRect = second.transform as RectTransform;
        AnimalMemoryJuice.PlayMatch(firstRect, secondRect);
        MementoMatchSfx.PlaySetMatch(
            selectedSetId,
            first.CurrentSprite != null ? first.CurrentSprite.name : string.Empty);
        if (IsDuelActive)
            PlayerDuelPairMade?.Invoke(firstRect, secondRect);

        cardStates[first] = CardState.Matched;
        cardStates[second] = CardState.Matched;
        first.ToggleRenderer(false);
        second.ToggleRenderer(false);
        SetCardInteractable(first, false);
        SetCardInteractable(second, false);

        turns++;
        guideRunState?.RegisterTurnCompleted();
        UpdateTurnsText();
        onTurnCompleted?.Invoke(turns);
        yield return new WaitForSecondsRealtime(0.86f);
        isResolvingMatch = false;

        if (IsDuelConfigured && duelState.IsActive)
        {
            yield return CompletePlayerDuelTurn(true, false, 1);

            yield break;
        }

        if (CheckGameOver())
        {
            AnimalMemoryJuice.PlayVictory();
            onWinEvent?.Invoke();
        }
    }

    private IEnumerator PulseHiddenPair(float duration)
    {
        if (!TryFindHiddenPair(out Card first, out Card second))
            yield break;

        ShowCardFace(first);
        ShowCardFace(second);
        AnimalMemoryJuice.PlayFlip(first.transform as RectTransform);
        AnimalMemoryJuice.PlayFlip(second.transform as RectTransform);
        yield return new WaitForSeconds(duration);

        if (first != null &&
            cardStates.TryGetValue(first, out CardState firstState) &&
            firstState == CardState.FaceDown)
        {
            HideCardUnlessPublic(first);
        }

        if (second != null &&
            cardStates.TryGetValue(second, out CardState secondState) &&
            secondState == CardState.FaceDown)
        {
            HideCardUnlessPublic(second);
        }
    }

    private IEnumerator PulseSpecificCards(Card first, Card second, float duration)
    {
        if (first == null || second == null)
            yield break;

        yield return WaitForSkillPresentation();
        ShowCardFace(first);
        ShowCardFace(second);
        AnimalMemoryJuice.PlayFlip(first.transform as RectTransform);
        AnimalMemoryJuice.PlayFlip(second.transform as RectTransform);
        yield return new WaitForSeconds(duration);

        if (first != null &&
            cardStates.TryGetValue(first, out CardState firstState) &&
            firstState == CardState.FaceDown)
            HideCardUnlessPublic(first);

        if (second != null &&
            cardStates.TryGetValue(second, out CardState secondState) &&
            secondState == CardState.FaceDown)
            HideCardUnlessPublic(second);
    }

    /// <summary>Reserves reading time without changing the global time scale or duel ownership.</summary>
    public bool CanRevealSupplyLine(MementoSupplyKind kind, int line)
    {
        if (!CanUseSupplies || !MementoSupplyWallet.IsValid(kind) || line < 0 ||
            line >= (kind == MementoSupplyKind.Row ? gridRows : gridColumns))
            return false;
        foreach (var entry in cardIndices)
            if (entry.Key != null && cardStates.TryGetValue(entry.Key, out CardState state) &&
                state == CardState.FaceDown &&
                !IsGodCardOutOfPlay(entry.Key) &&
                !IsCurtainBlocked(entry.Key, DuelActor.Player) &&
                (kind == MementoSupplyKind.Row ? entry.Value / gridColumns : entry.Value % gridColumns) == line)
                return true;
        return false;
    }

    public bool TryRevealSupplyLine(MementoSupplyKind kind, int line)
    {
        if (!CanRevealSupplyLine(kind, line)) return false;
        var targets = new List<Card>();
        foreach (var entry in cardIndices)
            if (entry.Key != null && cardStates[entry.Key] == CardState.FaceDown &&
                !IsGodCardOutOfPlay(entry.Key) &&
                !IsCurtainBlocked(entry.Key, DuelActor.Player) &&
                (kind == MementoSupplyKind.Row ? entry.Value / gridColumns : entry.Value % gridColumns) == line)
                targets.Add(entry.Key);
        suppliesUsed++;
        StartCoroutine(RevealSupplyLine(targets));
        return true;
    }

    private IEnumerator RevealSupplyLine(List<Card> targets)
    {
        isRevealingCards = true;
        foreach (Card card in targets)
        {
            ShowCardFace(card);
            AnimalMemoryJuice.PlayFlip(card.transform as RectTransform);
        }
        MementoMatchSfx.PlayCardFlip();
        // A private scouting aid, not an AI observation or a resolved turn.
        yield return new WaitForSecondsRealtime(3f);
        foreach (Card card in targets)
            if (card != null && cardStates.TryGetValue(card, out CardState state) &&
                state == CardState.FaceDown) HideCardUnlessPublic(card);
        isRevealingCards = false;
    }

    /// <summary>Release presentation only. Pending effect coroutines still run.
    /// The brief input guard prevents the dismissing pointer from flipping a card.</summary>
    public void SkipSkillPresentation()
    {
        skillPresentationUntil = Time.realtimeSinceStartup + .12f;
    }

    public void HoldForSkillPresentation(float seconds)
    {
        skillPresentationUntil = Mathf.Max(skillPresentationUntil, Time.realtimeSinceStartup + Mathf.Clamp(seconds, 0f, 8.5f));
    }

    private IEnumerator WaitForSkillPresentation(float minimumSeconds = 0f)
    {
        HoldForSkillPresentation(minimumSeconds);
        while (Time.realtimeSinceStartup < skillPresentationUntil)
            yield return null;
    }

    private void ShowCardFace(Card card)
    {
        card.RestoreOriginalSprite();
    }

    private void HideCardFace(Card card)
    {
        card.CurrentSprite = hiddenSprite;
        card.DownPriority();
    }

    private bool CheckGameOver()
    {
        if (cardStates.Count == 0)
        {
            return false;
        }

        foreach (CardState cardState in cardStates.Values)
        {
            if (cardState != CardState.Matched)
            {
                return false;
            }
        }

        return true;
    }

    private IEnumerator RevealCardsThenHide(float delay)
    {
        isRevealingCards = true;
        SetRevealLabelVisible(true);

        foreach (Card card in cardStates.Keys)
        {
            ShowCardFace(card);
            ObserveForOpponent(card);
        }

        yield return new WaitForSeconds(delay);

        foreach (KeyValuePair<Card, CardState> entry in cardStates)
        {
            if (entry.Key != null && entry.Value == CardState.FaceDown)
            {
                HideCardUnlessPublic(entry.Key);
            }
        }

        if (duelBrain != null && IsDuelConfigured)
            duelBrain.TrimInitialMemory(
                DuelRules.GetInitialMemoryLimit(configuredDuelGuideId));

        isRevealingCards = false;
        SetRevealLabelVisible(false);
    }

    private void DisableAllCards()
    {
        foreach (Card card in cards)
        {
            if (card == null)
            {
                continue;
            }

            Button button = card.GetComponent<Button>();
            if (button != null)
            {
                button.interactable = false;
            }
        }
    }

    private void SetRevealLabelVisible(bool visible)
    {
        if (revealCardsText != null)
        {
            revealCardsText.gameObject.SetActive(visible && !useToolkitHud);
        }
    }

    private void UpdateMatchesText()
    {
        if (matchesText != null)
        {
            matchesText.text = $"Matches: {matches}";
        }
    }

    private void UpdateTurnsText()
    {
        if (turnsText != null)
        {
            turnsText.text = $"Turns: {turns}";
        }
    }

    public void SaveData(GameData data)
    {
        data.SetData(MatchesKey, matches);
        data.SetData(TurnsKey, turns);
    }

    public void LoadData(GameData data)
    {
        matches = data.GetData<int>(MatchesKey);
        turns = data.GetData<int>(TurnsKey);
        UpdateMatchesText();
        UpdateTurnsText();
    }

    private void OnDestroy()
    {
        DestroyGeneratedSprites(proceduralCardBacks);
        DestroyGeneratedSprites(proceduralBoardMats);
    }

    private static void DestroyGeneratedSprites(IEnumerable<Sprite> sprites)
    {
        foreach (Sprite sprite in sprites)
        {
            if (sprite == null)
                continue;

            Texture2D texture = sprite.texture;
            Destroy(sprite);
            if (texture != null)
                Destroy(texture);
        }
    }
}
