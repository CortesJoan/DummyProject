using System;
using System.Collections.Generic;
using AnimalMemory.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour, ISavable
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private DifficultyHandler difficultyHandler;
    [SerializeField] private CardMatchUI cardMatchUI;
    [SerializeField] private ComboSystem comboSystem;
    [SerializeField] private TMP_Text statsText;

    [Header("Menus UI")]
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject gameUI;
    [SerializeField] private GameObject winUI;
    [SerializeField] private GameObject statsMenuUI;

    [Header("Difficulty Buttons")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button mediumButton;
    [SerializeField] private Button hardButton;

    [Header("Other Buttons")]
    [SerializeField] private Button openStatsButton;
    [SerializeField] private Button newGameWithSameDifficulty;
    [SerializeField] private Button newGameWithMoreDifficulty;
    [SerializeField] private List<Button> buttonsThatReturnToMainMenu;

    [SerializeField] private List<ISavable> savableObjects;
    [SerializeField] private int actualWins;

    private const string ActualWinsKey = "CardMatchUIActualWins";
    private const string ProgressionVersionKey = "ProgressionVersion";
    private const string PawStarsKey = "PawStars";
    private const string UnlockedSetMaskKey = "UnlockedSetMask";
    private const string UnlockedGuideMaskKey = "UnlockedGuideMask";
    private const string SelectedSetIdKey = "SelectedSetId";
    private const string SelectedGuideIdKey = "SelectedGuideId";
    private const string FirstClearDifficultyMaskKey = "FirstClearDifficultyMask";
    private const string DefeatedGuideMaskKey = "DefeatedGuideMask";
    private const string AchievementMaskKey = "AchievementMask";
    private const string CampaignClearedMaskKey = "CampaignClearedMask";
    private const string CampaignTwoStarMaskKey = "CampaignTwoStarMask";
    private const string CampaignThreeStarMaskKey = "CampaignThreeStarMask";
    private const string CampaignStorySeenMaskKey = "CampaignStorySeenMask";
    private const string TutorialCompletedKey = "MementoTutorialCompleted";
    private const string GuideTutorialCompletedKey = "MementoGuideTutorialCompleted";

    private readonly AnimalMemoryProgression progression = new AnimalMemoryProgression();
    private readonly AnimalMemoryGuideRunState guideRunState = new AnimalMemoryGuideRunState();
    private readonly GuideChallengeState challengeState = new GuideChallengeState();
    private readonly List<int> lastUnlockedAchievements = new List<int>();

    private readonly MementoPostgameProgression postgame = new MementoPostgameProgression();
    private readonly MementoSupplyWallet supplies = new MementoSupplyWallet();
    public MementoSupplyWallet Supplies => supplies;

    private bool useToolkitMenu;
    private int gameRequestVersion;
    private int activeDifficultyId;
    private int runMismatches;
    private int lastChallengeGuideId = -1;
    private bool lastResultWasChallenge;
    private bool lastResultWasCampaign;
    private int activeCampaignLevel = -1;
    private int lastCampaignLevel = -1;
    private int activePostgameEncounter = -1;
    private int lastPostgameEncounter = -1;
    private bool lastResultWasPostgame;
    private bool postgameOutcomePending;
    public event Action<int> PostgameScriptedEncounterRequested;
    private bool lastPowerReady;

    public event Action ProgressionChanged;
    public event Action ResultChanged;
    public event Action<int, bool> GuidePowerPresentationRequested;

    public MementoRewardedContinue RewardedContinue { get; private set; }
    public bool CanContinueDefeat => IsResultOverlayOpen && !LastResultWasVictory &&
        lastResultWasChallenge && cardMatchUI != null && cardMatchUI.CanContinueDefeat;

    public int ActualWins => actualWins;
    public int MaxCombo => comboSystem != null ? comboSystem.MaxCombo : 0;
    public int CurrentCombo => comboSystem != null ? comboSystem.CurrentCombo : 0;
    public int RunMaxCombo => comboSystem != null ? comboSystem.RunMaxCombo : 0;
    public int CurrentScore => comboSystem != null ? comboSystem.Score : 0;
    public int CurrentTurns => cardMatchUI != null ? cardMatchUI.Turns : 0;
    public int CurrentMatches => cardMatchUI != null ? cardMatchUI.Matches : 0;
    public int CurrentPairCount => cardMatchUI != null ? cardMatchUI.CurrentPairCount : 0;
    public int PawStars => progression.PawStars;
    public int SelectedSetId => progression.SelectedSetId;
    public int SelectedGuideId => progression.SelectedGuideId;
    public int AchievementMask => progression.AchievementMask;
    public int CampaignClearedCount => progression.CampaignClearedCount;
    public int CampaignTotalStars => progression.CampaignTotalStars;
    public int CampaignRecommendedLevel => progression.CampaignRecommendedLevel;
    public bool CampaignEndingUnlocked => progression.CampaignEndingUnlocked;
    public bool IsPostgameAvailable => MementoPostgameRelease.Enabled && CampaignEndingUnlocked;
    public bool IsTrueEndingUnlocked => postgame.IsTrueEndingUnlocked(MementoPostgameRelease.Enabled, CampaignEndingUnlocked);
    public int PostgameCompletedMask => postgame.CompletedMask;
    public int RecommendedPostgameEncounter => IsPostgameAvailable ? postgame.NextEncounterId() : -1;
    public bool IsPostgameEncounterUnlocked(int id) => postgame.CanEnter(id, MementoPostgameRelease.Enabled, CampaignEndingUnlocked);
    public bool LastResultWasCampaign => lastResultWasCampaign;
    public int LastCampaignLevel => lastCampaignLevel;
    public bool IsCampaignActive => activeCampaignLevel >= 0;
    public int ActiveCampaignLevel => activeCampaignLevel;
    public bool IsPostgameActive => activePostgameEncounter >= 0;
    public int ActivePostgameEncounter => activePostgameEncounter;
    public bool LastResultWasPostgame => lastResultWasPostgame;
    public int LastPostgameEncounter => lastPostgameEncounter;

    /// <summary>True exactly once per finished postgame outcome. Guards the
    /// return presentation against stale victories after an abandoned duel.</summary>
    public bool TryConsumePostgameOutcome()
    {
        if (!postgameOutcomePending) return false;
        postgameOutcomePending = false;
        return true;
    }
    public bool IsAllianceActive => IsGameplayActive && guideRunState.IsAllianceActive;
    public int AllianceMask => guideRunState.AllianceMask;
    public int GetAllianceCooldown(int id) => guideRunState.GetAllianceCooldown(id);
    public bool IsAlliancePowerReady(int id) => IsGameplayActive && IsPlayerTurn &&
        cardMatchUI != null && !cardMatchUI.IsBusy && guideRunState.IsAlliancePowerReady(id);
    public bool TryUseAlliancePower(int id) => IsGameplayActive && cardMatchUI != null &&
        cardMatchUI.TryUseAlliancePower(id);

    public int EquippedRunGuideId => guideRunState.SelectedGuideId;
    public int ActivePlayerGuideId => IsPostgameActive ? MementoPostgameEncounters.Get(activePostgameEncounter).PlayerGuideId : -1;
    public bool TutorialCompleted { get; private set; }
    public bool GuideTutorialCompleted { get; private set; }
    public int PowerCooldownRemaining => guideRunState.PowerCooldownRemaining;
    public int PowerCooldownTurns => guideRunState.PowerCooldownTurns;
    public string PowerName => guideRunState.PowerName;
    public bool IsGuidePowerBlocked =>
        cardMatchUI != null && cardMatchUI.IsGuidePowerBlocked;
    public bool IsPowerReady =>
        guideRunState.IsPowerReady && !IsGuidePowerBlocked;
    public bool IsMuted { get; private set; }
    public bool IsMainMenuOpen { get; private set; }
    public bool IsResultOverlayOpen { get; private set; }
    public bool IsGameplayActive => !IsMainMenuOpen && !IsResultOverlayOpen;
    public bool IsChallengeActive => challengeState.IsActive;
    public int ChallengeGuideId => challengeState.OpponentGuideId;
    public int ChallengeMismatches => challengeState.Mismatches;
    public int ChallengeMissLimit => challengeState.MissLimit;
    public int DuelPlayerPairs => cardMatchUI != null ? cardMatchUI.DuelPlayerPairs : 0;
    public int DuelOpponentPairs => cardMatchUI != null ? cardMatchUI.DuelOpponentPairs : 0;
    public int DuelTargetPairs => cardMatchUI != null ? cardMatchUI.DuelTargetPairs : 0;
    public int DuelPlayerHealth => cardMatchUI != null ? cardMatchUI.DuelPlayerHealth : 0;
    public int DuelOpponentHealth => cardMatchUI != null ? cardMatchUI.DuelOpponentHealth : 0;
    public bool IsPlayerTurn => cardMatchUI == null || cardMatchUI.IsPlayerTurn;
    public int DuelOpponentPose => cardMatchUI != null ? cardMatchUI.DuelOpponentPose : 0;
    public bool LastResultWasVictory { get; private set; }
    public int LastRewardStars { get; private set; }
    public string LastResultTitle { get; private set; } = string.Empty;
    public string LastResultSummary { get; private set; } = string.Empty;
    public IReadOnlyList<int> LastUnlockedAchievements => lastUnlockedAchievements;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitialEventSubscription();
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        progression.Changed += OnProgressionChanged;
        guideRunState.PowerPresentationRequested += OnGuidePowerPresentationRequested;
        savableObjects = new List<ISavable>();
        if (comboSystem != null)
            savableObjects.Add(comboSystem);
        savableObjects.Add(this);

        RewardedContinue = GetComponent<MementoRewardedContinue>();
        if (RewardedContinue == null) RewardedContinue = gameObject.AddComponent<MementoRewardedContinue>();
        RewardedContinue.Configure(this, null);
        LoadData();
        InitializeGame();
    }

    public void InitialEventSubscription()
    {
        easyButton?.onClick.AddListener(StartEasyGame);
        mediumButton?.onClick.AddListener(StartNormalGame);
        hardButton?.onClick.AddListener(StartHardGame);
        newGameWithSameDifficulty?.onClick.AddListener(StartSameDifficultyGame);
        newGameWithMoreDifficulty?.onClick.AddListener(StartNextDifficultyGame);
        openStatsButton?.onClick.AddListener(ShowStats);

        if (cardMatchUI != null)
        {
            cardMatchUI.onWinEvent.AddListener(OnWinGame);
            cardMatchUI.onMatchFailed.AddListener(OnMatchFailed);
            cardMatchUI.onDuelLost.AddListener(FinishChallengeDefeat);
            cardMatchUI.onTurnCompleted.AddListener(OnTurnCompleted);
        }

        if (buttonsThatReturnToMainMenu == null)
            return;

        foreach (Button button in buttonsThatReturnToMainMenu)
            button?.onClick.AddListener(ShowMainMenu);
    }

    public void UseToolkitMenu(bool enabled)
    {
        useToolkitMenu = enabled;
        ApplyLegacyMainMenuVisibility();
        cardMatchUI?.UseToolkitHud(enabled);
        comboSystem?.UseToolkitHud(enabled);

        if (enabled)
        {
            SetActiveIfAssigned(statsMenuUI, false);
            SetActiveIfAssigned(winUI, false);
        }
    }

    public void StartEasyGame() => StartFreeGame(0);
    public void StartNormalGame() => StartFreeGame(1);
    public void StartHardGame() => StartFreeGame(2);

    private void StartFreeGame(int difficultyId)
    {
        activePostgameEncounter = -1;
        lastResultWasPostgame = false;
        activeCampaignLevel = -1;
        lastResultWasCampaign = false;
        StartGame(difficultyId, false);
    }

    public bool IsSetUnlocked(int setId) => progression.IsSetUnlocked(setId);
    public bool IsGuideUnlocked(int guideId) => progression.IsGuideUnlocked(guideId);
    public bool CanChallengeGuide(int guideId) => progression.CanChallengeGuide(guideId);
    public bool IsAchievementUnlocked(int id) => progression.IsAchievementUnlocked(id);
    public int GetSetUnlockRequirement(int setId) => progression.GetSetUnlockRequirement(setId);
    public int GetGuideUnlockRequirement(int guideId) => progression.GetGuideUnlockRequirement(guideId);
    public string GetSetName(int setId) => progression.GetSetName(setId);
    public string GetGuideName(int guideId) => MementoPostgameEncounters.IsPostgameOpponent(guideId)
        ? MementoPostgameEncounters.GetOpponentName(guideId) : progression.GetGuideName(guideId);
    public string GetGuideSkillDescription(int guideId) => progression.GetGuideSkillDescription(guideId);
    public string GetAchievementName(int id) => progression.GetAchievementName(id);
    public string GetAchievementDescription(int id) => progression.GetAchievementDescription(id);
    public bool IsCampaignLevelUnlocked(int levelId) =>
        progression.IsCampaignLevelUnlocked(levelId);
    public int GetCampaignStars(int levelId) => progression.GetCampaignStars(levelId);
    public bool IsCampaignStorySeen(int sceneId) =>
        progression.IsCampaignStorySeen(sceneId);
    public MementoMatchCampaignLevel GetCampaignLevel(int levelId) =>
        MementoMatchCampaignRules.GetLevel(levelId);

    public bool TryGetCampaignPreScene(
        int levelId,
        out MementoMatchStoryScene scene)
    {
        scene = null;
        if (!progression.IsCampaignLevelUnlocked(levelId))
            return false;
        int sceneId = MementoMatchCampaignRules.GetPreSceneId(levelId);
        if (sceneId < 0)
            return false;

        // A pre-level scene only stays consumed after its level is cleared.
        // This prevents a quit, crash or interrupted attempt from silently
        // skipping the chapter on the next visit.
        bool levelAlreadyCleared = progression.GetCampaignStars(levelId) > 0;
        if (levelAlreadyCleared && progression.IsCampaignStorySeen(sceneId))
            return false;

        scene = MementoMatchCampaignRules.GetStoryScene(sceneId);
        return true;
    }

    public bool TryGetPendingCampaignPostScene(out MementoMatchStoryScene scene)
    {
        scene = null;
        if (!lastResultWasCampaign ||
            !LastResultWasVictory ||
            lastCampaignLevel < 0)
            return false;

        int sceneId = MementoMatchCampaignRules.GetPostSceneId(lastCampaignLevel);
        if (sceneId >= 0 && !progression.IsCampaignStorySeen(sceneId))
        {
            scene = MementoMatchCampaignRules.GetStoryScene(sceneId);
            return true;
        }

        if (progression.CampaignEndingUnlocked &&
            !progression.IsCampaignStorySeen(MementoMatchCampaignRules.EpilogueSceneId))
        {
            scene = MementoMatchCampaignRules.GetStoryScene(
                MementoMatchCampaignRules.EpilogueSceneId);
            return true;
        }

        return false;
    }

    public void MarkCampaignStorySeen(int sceneId)
    {
        if (progression.MarkCampaignStorySeen(sceneId))
            SaveSystem.Save(savableObjects);
    }

    public void CompleteTutorial()
    {
        if (TutorialCompleted)
            return;

        TutorialCompleted = true;
        SaveSystem.Save(savableObjects);
    }

    public void CompleteGuideTutorial()
    {
        if (GuideTutorialCompleted)
            return;

        GuideTutorialCompleted = true;
        SaveSystem.Save(savableObjects);
    }

    public bool StartCampaignLevel(int levelId)
    {
        if (!progression.IsCampaignLevelUnlocked(levelId))
            return false;
        activePostgameEncounter = -1;
        lastResultWasPostgame = false;

        MementoMatchCampaignLevel level =
            MementoMatchCampaignRules.GetLevel(levelId);
        activeCampaignLevel = levelId;
        lastCampaignLevel = levelId;
        lastResultWasCampaign = true;
        if (level.IsBoss)
        {
            challengeState.Begin(level.OpponentGuideId);
            lastChallengeGuideId = level.OpponentGuideId;
            StartGame(level.DifficultyId, true);
        }
        else
        {
            StartGame(level.DifficultyId, false);
        }

        return true;
    }

    /// <summary>Starts only released, unlocked encounters; presentation must handle the scripted Rei scene.</summary>
    public bool StartPostgameEncounter(int encounterId)
    {
        if (!IsPostgameEncounterUnlocked(encounterId)) return false;
        MementoPostgameEncounter encounter = MementoPostgameEncounters.Get(encounterId);
        if (encounter.IsScriptedDefeat && PostgameScriptedEncounterRequested == null) return false;
        if (!encounter.IsScriptedDefeat && (difficultyHandler == null || cardMatchUI == null)) return false;
        activePostgameEncounter = encounterId;
        lastPostgameEncounter = encounterId;
        lastResultWasPostgame = true;
        postgameOutcomePending = false;
        activeCampaignLevel = -1;
        lastResultWasCampaign = false;
        if (encounter.IsScriptedDefeat)
        {
            gameRequestVersion++;
            RewardedContinue?.CancelPending();
            lastResultWasChallenge = false;
            challengeState.Reset();
            cardMatchUI?.CancelCurrentGame();
            IsResultOverlayOpen = false;
            PostgameScriptedEncounterRequested.Invoke(encounterId);
        }
        else
        {
            challengeState.Begin(encounter.OpponentId);
            lastChallengeGuideId = encounter.OpponentId;
            StartGame(2, true);
        }
        return true;
    }

    /// <summary>Only the explicitly presented Rei checkpoint can advance after a scripted defeat.</summary>
    public bool CompletePostgameScriptedEncounter()
    {
        if (!postgame.TryCompleteScriptedDefeat(activePostgameEncounter,
            MementoPostgameRelease.Enabled, CampaignEndingUnlocked)) return false;
        SaveSystem.Save(savableObjects);
        ProgressionChanged?.Invoke();
        ShowMainMenu();
        return true;
    }

    private void FinishPostgameVictory()
    {
        MementoPostgameEncounter encounter = MementoPostgameEncounters.Get(activePostgameEncounter);
        if (encounter.IsScriptedDefeat) return;
        bool firstClear = postgame.TryCompleteDuel(encounter.Id, MementoPostgameRelease.Enabled, CampaignEndingUnlocked);
        if (firstClear)
        {
            actualWins++;
            supplies.AwardVictory();
        }
        challengeState.End();
        LastResultWasVictory = true;
        LastRewardStars = 0;
        LastResultTitle = encounter.Id == MementoPostgameEncounters.Count - 1 ? "EL DESEO NOS PERTENECE" : "CUSTODIA SUPERADA";
        LastResultSummary = encounter.Title + (firstClear ? " · +" + MementoSupplyWallet.VictoryCoins + " monedas de suministros" : " · REPLAY COMPLETADO");
        postgameOutcomePending = true;
        MementoMatchSfx.PlayResult(MementoMatchResultSfx.Victory);
        IsResultOverlayOpen = true;
        SetActiveIfAssigned(winUI, !useToolkitMenu);
        SaveSystem.Save(savableObjects);
        ProgressionChanged?.Invoke();
        ResultChanged?.Invoke();
    }

    public bool TrySelectSet(int setId)
    {
        bool selected = progression.TrySelectSet(setId);
        if (selected)
            SaveSystem.Save(savableObjects);
        return selected;
    }

    public bool TrySelectGuide(int guideId)
    {
        bool selected = progression.TrySelectGuide(guideId);
        if (selected)
            SaveSystem.Save(savableObjects);
        return selected;
    }

    public bool StartGuideChallenge(int guideId)
    {
        if (!progression.CanChallengeGuide(guideId))
            return false;
        activePostgameEncounter = -1;
        lastResultWasPostgame = false;
        activeCampaignLevel = -1;
        lastResultWasCampaign = false;

        challengeState.Begin(guideId);
        lastChallengeGuideId = guideId;
        StartGame(GuideChallengeRules.GetDifficultyId(guideId), true);
        return true;
    }

    public bool TryBuySupply(MementoSupplyKind kind)
    {
        if (!IsMainMenuOpen || !supplies.TryBuy(kind)) return false;
        SaveSystem.Save(savableObjects);
        ProgressionChanged?.Invoke();
        return true;
    }

    public bool TryUseSupply(MementoSupplyKind kind, int line)
    {
        if (!IsGameplayActive || IsResultOverlayOpen || cardMatchUI == null ||
            !supplies.TryUse(kind, () => cardMatchUI.TryRevealSupplyLine(kind, line)))
            return false;
        SaveSystem.Save(savableObjects);
        ProgressionChanged?.Invoke();
        return true;
    }

    public bool TryUseGuidePower()
    {
        if (!IsGameplayActive || cardMatchUI == null)
            return false;

        bool used = cardMatchUI.TryUseGuidePower();
        if (used)
            lastPowerReady = IsPowerReady;
        return used;
    }

    public void ContinueDuelOutcomePresentation()
    {
        cardMatchUI?.AcknowledgeDuelOutcomePresentation();
    }

    public bool TryContinueDefeat()
    {
        if (!CanContinueDefeat || !cardMatchUI.TryContinueDefeat()) return false;
        challengeState.Begin(lastChallengeGuideId);
        LastResultTitle = "";
        LastResultSummary = "";
        IsResultOverlayOpen = false;
        IsMainMenuOpen = false;
        SetActiveIfAssigned(winUI, false);
        SetActiveIfAssigned(gameUI, true);
        MementoMatchVoicePlayer.StopCurrent();
        AudioManager.PlayDuelMusic(lastChallengeGuideId);
        ResultChanged?.Invoke();
        return true;
    }

    public void StartSameDifficultyGame()
    {
        if (lastResultWasPostgame && lastPostgameEncounter >= 0)
        {
            StartPostgameEncounter(lastPostgameEncounter);
            return;
        }
        if (lastResultWasCampaign && lastCampaignLevel >= 0)
        {
            StartCampaignLevel(lastCampaignLevel);
            return;
        }

        if (lastResultWasChallenge && lastChallengeGuideId >= 0)
        {
            StartGuideChallenge(lastChallengeGuideId);
            return;
        }

        if (difficultyHandler == null)
        {
            Debug.LogError("Cannot repeat the game because DifficultyHandler is not assigned.");
            return;
        }

        StartGame(difficultyHandler.GetCurrentDifficultyLevel(), false);
    }

    public void StartNextDifficultyGame()
    {
        if (difficultyHandler == null)
        {
            Debug.LogError("Cannot advance difficulty because DifficultyHandler is not assigned.");
            return;
        }

        StartFreeGame(difficultyHandler.GetNextDifficultyLevel());
    }

    public void ShowMainMenu()
    {
        AudioManager.RestoreDefaultMusic();
        gameRequestVersion++;
        RewardedContinue?.CancelPending();
        cardMatchUI?.CancelCurrentGame();
        guideRunState.Reset(progression.SelectedGuideId);
        challengeState.Reset();
        activeCampaignLevel = -1;
        activePostgameEncounter = -1;
        IsMainMenuOpen = true;
        IsResultOverlayOpen = false;
        ApplyLegacyMainMenuVisibility();
        SetActiveIfAssigned(gameUI, false);
        SetActiveIfAssigned(winUI, false);
        SetActiveIfAssigned(statsMenuUI, false);
        ResultChanged?.Invoke();
    }

    public void ShowStats()
    {
        AudioManager.RestoreDefaultMusic();
        IsMainMenuOpen = false;
        IsResultOverlayOpen = false;
        ApplyLegacyMainMenuVisibility();
        SetActiveIfAssigned(gameUI, false);
        SetActiveIfAssigned(winUI, false);
        SetActiveIfAssigned(statsMenuUI, !useToolkitMenu);
        UpdateStatsText();
    }

    public void ToggleMute()
    {
        IsMuted = !IsMuted;
        AudioListener.volume = IsMuted ? 0f : 1f;
    }

    public void DeleteSaveAndRestart()
    {
        if (!SaveSystem.DeleteSave())
            return;

        Time.timeScale = 1f;
        AudioListener.volume = 1f;
        Scene activeScene = SceneManager.GetActiveScene();
        if (activeScene.buildIndex >= 0)
            SceneManager.LoadScene(activeScene.buildIndex);
        else
            SceneManager.LoadScene(activeScene.name);
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        Debug.Log("Quit requested.");
#else
        Application.Quit();
#endif
    }

    public string GetStatsSummary()
    {
        return $"Victorias: {ActualWins}\nEstrellas: {PawStars}\nMejor combo: {MaxCombo}";
    }

    private void InitializeGame()
    {
        AudioManager.RestoreDefaultMusic();
        IsMainMenuOpen = true;
        IsResultOverlayOpen = false;
        ApplyLegacyMainMenuVisibility();
        SetActiveIfAssigned(gameUI, false);
        SetActiveIfAssigned(winUI, false);
        SetActiveIfAssigned(statsMenuUI, false);
    }

    private void StartGame(int difficultyLevelIndex, bool preserveChallenge)
    {
        if (difficultyHandler == null || cardMatchUI == null)
        {
            Debug.LogError("Cannot start game: required gameplay references are not assigned.");
            return;
        }

        if (!preserveChallenge)
            challengeState.Reset();

        difficultyHandler.SetCurrentDifficultyLevel(difficultyLevelIndex);
        GameDifficulty currentDifficulty = difficultyHandler.GetCurrentDifficulty();
        if (currentDifficulty == null)
        {
            Debug.LogError("Cannot start game: no valid difficulty is available.");
            return;
        }

        RewardedContinue?.BeginRun();
        activeDifficultyId = difficultyHandler.GetCurrentDifficultyLevel();
        runMismatches = 0;
        lastUnlockedAchievements.Clear();
        LastRewardStars = 0;
        LastResultTitle = string.Empty;
        LastResultSummary = string.Empty;
        IsResultOverlayOpen = false;
        lastResultWasChallenge = preserveChallenge;

        int runGuide = IsPostgameActive && ActivePlayerGuideId >= 0 ? ActivePlayerGuideId : progression.SelectedGuideId;
        // Aki remains unlocked in the collection, but is absent after her betrayal.
        if (IsPostgameActive && activePostgameEncounter == MementoPostgameEncounters.Count - 1 &&
            runGuide == AnimalMemoryContentIds.AkiGuide)
            runGuide = AnimalMemoryContentIds.NoGuide;
        guideRunState.Reset(runGuide);
        cardMatchUI.ConfigurePlayerRole(ActivePlayerGuideId);
        lastPowerReady = IsPowerReady;
        int setId = IsPostgameActive ? MementoPostgameEncounters.Get(activePostgameEncounter).SetId
            : activeCampaignLevel >= 0
            ? MementoMatchCampaignRules.GetLevel(activeCampaignLevel).SetId
            : preserveChallenge
                ? challengeState.OpponentGuideId
                : progression.SelectedSetId;
        if (preserveChallenge)
        {
            cardMatchUI.ConfigureDuel(challengeState.OpponentGuideId);
            AudioManager.PlayDuelMusic(challengeState.OpponentGuideId);
            MementoMatchSfx.PlayDuelStart();
        }
        else
        {
            cardMatchUI.ClearDuel();
            AudioManager.RestoreDefaultMusic();
        }
        cardMatchUI.ConfigureProgression(setId, guideRunState);
        int allianceMask = 0;
        if (IsPostgameActive && activePostgameEncounter == MementoPostgameEncounters.Count - 1)
            for (int id = 1; id < AnimalMemoryContentIds.GuideCount; id++)
                if (progression.IsGuideUnlocked(id)) allianceMask |= 1 << id;
        cardMatchUI.ConfigureAllianceRoster(allianceMask);
        comboSystem?.ConfigureGuide(guideRunState);
        comboSystem?.ResetForNewGame();
        IsMainMenuOpen = false;
        int requestVersion = ++gameRequestVersion;

        ApplyLegacyMainMenuVisibility();
        SetActiveIfAssigned(winUI, false);
        SetActiveIfAssigned(statsMenuUI, false);
        SetActiveIfAssigned(gameUI, true);

        if (currentDifficulty.specificCardSet != null &&
            currentDifficulty.specificCardSet.RuntimeKeyIsValid() &&
            CardSetManager.Instance != null)
        {
            LoadCardSetAndStartGame(currentDifficulty, requestVersion);
        }
        else
        {
            SetupCurrentGame(currentDifficulty, requestVersion);
        }

        ResultChanged?.Invoke();
    }

    private async void LoadCardSetAndStartGame(GameDifficulty difficulty, int requestVersion)
    {
        bool success = await CardSetManager.Instance.LoadCardSetAsync(difficulty.specificCardSet);
        if (requestVersion != gameRequestVersion || IsMainMenuOpen)
            return;

        if (!success)
            Debug.LogError("Failed to load card set for difficulty. Using current card set.");

        SetupCurrentGame(difficulty, requestVersion);
    }

    private void SetupCurrentGame(GameDifficulty difficulty, int requestVersion)
    {
        if (requestVersion != gameRequestVersion ||
            IsMainMenuOpen ||
            difficulty == null ||
            cardMatchUI == null)
            return;

        int rows = difficulty.gridSize.x;
        int columns = difficulty.gridSize.y;
        if (IsPostgameActive)
        {
            MementoPostgameEncounter encounter = MementoPostgameEncounters.Get(activePostgameEncounter);
            rows = encounter.Rows;
            columns = encounter.Columns;
        }
        else if (activeCampaignLevel >= 0)
        {
            MementoMatchCampaignLevel campaignLevel =
                MementoMatchCampaignRules.GetLevel(activeCampaignLevel);
            rows = campaignLevel.Rows;
            columns = campaignLevel.Columns;
        }
        else if (challengeState.IsActive)
        {
            rows = 4;
            columns =
                challengeState.OpponentGuideId == AnimalMemoryContentIds.YoruGuide ||
                challengeState.OpponentGuideId == AnimalMemoryContentIds.MomoGuide
                    ? 5
                    : 4;
        }

        cardMatchUI.SetupGame(
            rows,
            columns,
            difficulty.backgroundColor,
            difficulty.timeToSeeCards);
        MementoMatchSfx.PlayBoardDeal();
    }

    private void OnMatchFailed()
    {
        runMismatches++;
    }

    private void OnTurnCompleted(int turn)
    {
        bool powerReady = IsPowerReady;
        if (!lastPowerReady && powerReady)
            MementoMatchSfx.PlayCooldownReady();
        lastPowerReady = powerReady;
    }

    private void OnWinGame()
    {
        if (IsResultOverlayOpen)
            return;
        if (IsPostgameActive)
        {
            FinishPostgameVictory();
            return;
        }

        int defeatedGuideId = -1;
        bool guideWasUnlocked = true;
        if (challengeState.IsActive)
        {
            defeatedGuideId = challengeState.OpponentGuideId;
            bool isRecruitableGuide = defeatedGuideId < AnimalMemoryContentIds.GuideCount;
            guideWasUnlocked = !isRecruitableGuide || progression.IsGuideUnlocked(defeatedGuideId);
            if (isRecruitableGuide)
            {
                if (lastResultWasCampaign && activeCampaignLevel >= 0)
                    progression.CompleteCampaignBoss(defeatedGuideId);
                else
                    progression.CompleteGuideChallenge(defeatedGuideId);
            }
            challengeState.End();
        }

        actualWins++;
        supplies.AwardVictory();
        int pawReward = progression.AwardVictory(
            activeDifficultyId,
            cardMatchUI != null && cardMatchUI.IsEfficiencyBonusEarned);
        LastRewardStars = activeCampaignLevel >= 0
            ? progression.RecordCampaignVictory(
                activeCampaignLevel,
                CurrentTurns,
                CurrentPairCount,
                runMismatches,
                RunMaxCombo)
            : pawReward;

        MementoMatchResult result = new MementoMatchResult
        {
            DifficultyId = activeDifficultyId,
            Turns = CurrentTurns,
            PairCount = CurrentPairCount,
            Mismatches = runMismatches,
            MaxCombo = RunMaxCombo,
            DefeatedGuideId = defeatedGuideId
        };
        lastUnlockedAchievements.AddRange(progression.RecordResult(result));
        if (defeatedGuideId >= 0 && !guideWasUnlocked)
            MementoMatchSfx.PlayGuideUnlock();
        if (lastUnlockedAchievements.Count > 0)
            MementoMatchSfx.PlayAchievementPop();
        MementoMatchSfx.PlayResult(MementoMatchResultSfx.Victory);

        LastResultWasVictory = true;
        if (lastResultWasCampaign && activeCampaignLevel >= 0)
        {
            MementoMatchCampaignLevel completedLevel =
                MementoMatchCampaignRules.GetLevel(activeCampaignLevel);
            LastResultTitle = completedLevel.IsFinalBoss
                ? "MAESTRA DEFINITIVA SUPERADA"
                : completedLevel.IsBoss
                    ? $"ALIANZA CON {progression.GetGuideName(completedLevel.OpponentGuideId).ToUpperInvariant()}"
                    : $"NIVEL {completedLevel.Id + 1} COMPLETADO";
            LastResultSummary =
                $"{completedLevel.Title} · {LastRewardStars}/3 estrellas · " +
                $"{CurrentMatches} parejas en {CurrentTurns} turnos.";
        }
        else
        {
            LastResultTitle = defeatedGuideId >= 0
                ? $"HAS SUPERADO A {progression.GetGuideName(defeatedGuideId).ToUpperInvariant()}"
                : "RECUERDO COMPLETO";
            LastResultSummary = defeatedGuideId >= 0
                ? "Duelo completado. Su técnica y su tema quedan listos para seguir practicando."
                : $"{CurrentMatches} parejas en {CurrentTurns} turnos · combo máximo {RunMaxCombo}.";
        }
        LastResultSummary += $" · +{MementoSupplyWallet.VictoryCoins} monedas de suministros";
        IsResultOverlayOpen = true;
        SetActiveIfAssigned(winUI, !useToolkitMenu);
        SaveSystem.Save(savableObjects);
        ResultChanged?.Invoke();
    }

    private void FinishChallengeDefeat()
    {
        if (IsResultOverlayOpen)
            return;

        int opponent = challengeState.OpponentGuideId;
        challengeState.End();
        // Keep the board and rival alive behind the result presentation so the
        // victory pose and spoken line remain visible until the player leaves.
        LastResultWasVictory = false;
        if (lastResultWasPostgame) postgameOutcomePending = true;
        LastRewardStars = 0;
        LastResultTitle = $"{GetGuideName(opponent).ToUpperInvariant()} GANA EL DUELO";
        MementoMatchSfx.PlayResult(MementoMatchResultSfx.Defeat);
        LastResultSummary =
            "Tu rival ha ganado este duelo. " +
            "Cambia de estrategia y vuelve a intentarlo.";
        IsResultOverlayOpen = true;
        SetActiveIfAssigned(winUI, false);
        ResultChanged?.Invoke();
    }

    private void LoadData()
    {
        SaveSystem.Load(savableObjects);
    }

    public void SaveData(GameData data)
    {
        MementoPostgameProgression.Snapshot postgameSnapshot = postgame.CreateSnapshot();
        data.SetData("PostgameVersion", postgameSnapshot.version);
        data.SetData("PostgameCompletedMask", postgameSnapshot.completedMask);
        data.SetData("SuppliesVersion", 1);
        data.SetData("SuppliesCoins", supplies.Coins);
        data.SetData("SuppliesRows", supplies.Rows);
        data.SetData("SuppliesColumns", supplies.Columns);
        data.SetData(ActualWinsKey, actualWins);
        AnimalMemoryProgressionSnapshot snapshot = progression.CreateSnapshot();
        data.SetData(ProgressionVersionKey, snapshot.progressionVersion);
        data.SetData(PawStarsKey, snapshot.pawStars);
        data.SetData(UnlockedSetMaskKey, snapshot.unlockedSetMask);
        data.SetData(UnlockedGuideMaskKey, snapshot.unlockedGuideMask);
        data.SetData(SelectedSetIdKey, snapshot.selectedSetId);
        data.SetData(SelectedGuideIdKey, snapshot.selectedGuideId);
        data.SetData(FirstClearDifficultyMaskKey, snapshot.firstClearDifficultyMask);
        data.SetData(DefeatedGuideMaskKey, snapshot.defeatedGuideMask);
        data.SetData(AchievementMaskKey, snapshot.achievementMask);
        data.SetData(CampaignClearedMaskKey, snapshot.campaignClearedMask);
        data.SetData(CampaignTwoStarMaskKey, snapshot.campaignTwoStarMask);
        data.SetData(CampaignThreeStarMaskKey, snapshot.campaignThreeStarMask);
        data.SetData(CampaignStorySeenMaskKey, snapshot.campaignStorySeenMask);
        data.SetData(TutorialCompletedKey, TutorialCompleted);
        data.SetData(GuideTutorialCompletedKey, GuideTutorialCompleted);
    }

    public void LoadData(GameData data)
    {
        postgame.LoadSnapshot(new MementoPostgameProgression.Snapshot
        {
            version = data.GetData<int>("PostgameVersion"),
            completedMask = data.GetData<int>("PostgameCompletedMask")
        });
        actualWins = data.GetData<int>(ActualWinsKey);
        supplies.Load(data.GetData<int>("SuppliesVersion"), data.GetData<int>("SuppliesCoins"),
            data.GetData<int>("SuppliesRows"), data.GetData<int>("SuppliesColumns"), actualWins);
        progression.LoadSnapshot(new AnimalMemoryProgressionSnapshot
        {
            progressionVersion = data.GetData<int>(ProgressionVersionKey),
            pawStars = data.GetData<int>(PawStarsKey),
            unlockedSetMask = data.GetData<int>(UnlockedSetMaskKey),
            unlockedGuideMask = data.GetData<int>(UnlockedGuideMaskKey),
            selectedSetId = data.GetData<int>(SelectedSetIdKey),
            selectedGuideId = data.GetData<int>(SelectedGuideIdKey),
            firstClearDifficultyMask = data.GetData<int>(FirstClearDifficultyMaskKey),
            defeatedGuideMask = data.GetData<int>(DefeatedGuideMaskKey),
            achievementMask = data.GetData<int>(AchievementMaskKey),
            campaignClearedMask = data.GetData<int>(CampaignClearedMaskKey),
            campaignTwoStarMask = data.GetData<int>(CampaignTwoStarMaskKey),
            campaignThreeStarMask = data.GetData<int>(CampaignThreeStarMaskKey),
            campaignStorySeenMask = data.GetData<int>(CampaignStorySeenMaskKey)
        });
        TutorialCompleted = data.GetData<bool>(TutorialCompletedKey);
        GuideTutorialCompleted = data.GetData<bool>(GuideTutorialCompletedKey);
    }

    private void OnProgressionChanged()
    {
        ProgressionChanged?.Invoke();
    }

    private void OnGuidePowerPresentationRequested(int guideId, bool resolved)
    {
        GuidePowerPresentationRequested?.Invoke(guideId, resolved);
    }

    private void OnDestroy()
    {
        progression.Changed -= OnProgressionChanged;
        guideRunState.PowerPresentationRequested -= OnGuidePowerPresentationRequested;
        if (cardMatchUI != null)
        {
            cardMatchUI.onWinEvent.RemoveListener(OnWinGame);
            cardMatchUI.onMatchFailed.RemoveListener(OnMatchFailed);
            cardMatchUI.onDuelLost.RemoveListener(FinishChallengeDefeat);
            cardMatchUI.onTurnCompleted.RemoveListener(OnTurnCompleted);
        }

        if (Instance == this)
            Instance = null;
    }

    public void UpdateStatsText()
    {
        if (statsText != null)
            statsText.text = GetStatsSummary();
    }

    private void ApplyLegacyMainMenuVisibility()
    {
        SetActiveIfAssigned(mainMenuUI, IsMainMenuOpen && !useToolkitMenu);
    }

    private static void SetActiveIfAssigned(GameObject target, bool active)
    {
        if (target != null)
            target.SetActive(active);
    }
}
