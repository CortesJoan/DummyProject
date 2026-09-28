using System;
using System.Collections.Generic;
using System.Linq;
using AnimalMemory.Progression;
using AnimalMemory.Narrative;
using AnimalMemory.UI;
using EHKP.VisualNovel;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(UIDocument))]
public sealed class AnimalMemoryMenuController : MonoBehaviour
{
    private MementoOpeningMemoryView openingMemory;
    private UIDocument document;
    private GameManager gameManager;
    private CardMatchUI cardMatchUI;
    private MementoSafeAreaView safeAreaView;
    private VisualElement gameplayHeader;
    private VisualElement gameplayFooter;
    private Vector2 lastViewportSize;
    private Rect lastViewportSafeArea;

    private Button easyButton;
    private Button normalButton;
    private Button hardButton;
    private Button forestThemeButton;
    private Button coralThemeButton;
    private Button nightThemeButton;
    private Button gardenThemeButton;
    private Button sweetsThemeButton;
    private Button clockworkThemeButton;
    private Button akiGuideButton;
    private Button mikaGuideButton;
    private Button yoruGuideButton;
    private Button hanaGuideButton;
    private Button momoGuideButton;
    private Button challengeButton;
    private Button achievementsButton;
    private Button achievementsCloseButton;
    private Button muteButton;
    private Slider musicVolumeSlider;
    private Slider sfxVolumeSlider;
    private Slider voiceVolumeSlider;
    private Button orientationAutoButton;
    private Button orientationPortraitButton;
    private Button orientationLandscapeButton;
    private Button deleteSaveButton;
    private Button quitButton;
    private Button returnMenuButton;
    private Button powerButton;
    private VisualElement allianceBar;
    private readonly Button[] allianceButtons = new Button[5];
    private readonly Label[] allianceCooldownLabels = new Label[5];
    private Button resultContinueAdButton;
    private VisualElement resultContinueOffer;
    private Label resultContinueStatus;
    private Button resultRetryButton;
    private Button resultMenuButton;
    private Button campaignOpenButton;
    private Button campaignCloseButton;
    private Button campaignStartButton;
    private Button campaignFinalButton;
    private Button campaignCreditsButton;
    private Button storyNextButton;
    private Button storySkipButton;
    private Button playerNameConfirmButton;
    private Button creditsCloseButton;
    private Button duelDialogueContinueButton;
    private Button homeStoryButton;
    private Button guideEquipConfirmButton;
    private Button guideEquipKeepButton;
    private Button storyReplayOpenButton;
    private Button storyReplayCloseButton;
    private Button storyReplayFinalButton;
    private Button storyReplayEpilogueButton;
    private Button tutorialContinueButton;
    private Button finalCreditsButton;
    private Button finalEquipArchiveButton;
    private readonly Button[] storyReplayWorldButtons =
        new Button[MementoMatchCampaignRules.WorldCount];
    private readonly Button[] storyReplaySceneButtons = new Button[3];
    private readonly Button[] shellNavButtons = new Button[5];
    private readonly VisualElement[] shellPages = new VisualElement[5];
    private readonly Button[] campaignWorldButtons = new Button[MementoMatchCampaignRules.WorldCount];
    private readonly Button[] campaignNodeButtons = new Button[MementoMatchCampaignRules.LevelsPerWorld];

    private VisualElement menuScreen;
    private VisualElement gameplayHud;
    private VisualElement rivalPanel;
    private VisualElement playerHealthCard;
    private VisualElement rivalHealthCard;
    private VisualElement playerHealthTrack;
    private VisualElement playerHealthFill;
    private VisualElement rivalFill;
    private VisualElement resultOverlay;
    private VisualElement achievementsOverlay;
    private VisualElement achievementList;
    private VisualElement hostessImage;
    private VisualElement powerGuideFace;
    private VisualElement powerCluster;
    private VisualElement skillCutInOverlay;
    private VisualElement skillCutInFace;
    private VisualElement skillProcToast;
    private VisualElement skillProcFace;
    private VisualElement duelDialogueOverlay;
    private VisualElement duelDialogueFace;
    private VisualElement voiceSubtitleBox;
    private VisualElement voiceSubtitleFace;
    private VisualElement comboBurst;
    private VisualElement campaignOverlay;
    private VisualElement campaignGuardian;
    private VisualElement storyOverlay;
    private VisualElement storyStage;
    private VisualElement storyPortrait;
    private VisualElement playerNameOverlay;
    private VisualElement creditsOverlay;
    private VisualElement homeGuideImage;
    private VisualElement storyBackground;
    private VisualElement guideEquipOverlay;
    private VisualElement guideEquipPortrait;
    private VisualElement storyReplayOverlay;
    private VisualElement tutorialOverlay;
    private VisualElement tutorialGuideFace;
    private VisualElement finalCelebrationOverlay;
    private readonly VisualElement[] finalTeamFaces = new VisualElement[6];

    private Label themeDescription;
    private Label loadoutSummary;
    private Label setBadge;
    private Label progressLabel;
    private Label pawStarsLabel;
    private Label guideName;
    private Label guideAbility;
    private Label hostessLine;
    private Label hudMode;
    private Label hudScore;
    private Label hudCombo;
    private Label hudTurns;
    private Label hudMatches;
    private Label rivalName;
    private Label rivalStatus;
    private Label playerHealthLabel;
    private Label rivalHealthLabel;
    private Label resultKicker;
    private Label resultTitle;
    private Label resultSummary;
    private Label resultStars;
    private Label resultAchievements;
    private Label skillCutInKicker;
    private Label skillCutInName;
    private Label skillCutInQuote;
    private Label skillCutInEffect;
    private MementoSkillCinematic skillCinematic;
    private int skillPresentationVersion;
    private MementoSuppliesView suppliesView;
    private MementoPostgameSelectionView postgameSelectionView;
    private Label skillProcName;
    private Label skillProcEffect;
    private Label duelDialogueName;
    private Label duelDialogueLine;
    private Label voiceSubtitleSpeaker;
    private Label voiceSubtitleLine;
    private Label comboBurstCount;
    private Label comboBurstHeading;
    private Label comboBurstRank;
    private Label campaignOverview;
    private Label campaignWorldKicker;
    private Label campaignWorldTitle;
    private Label campaignWorldProgress;
    private Label campaignLevelKicker;
    private Label campaignLevelTitle;
    private Label campaignLevelObjective;
    private Label campaignLevelRules;
    private Label storyKicker;
    private Label storyChapter;
    private Label storyStageChapter;
    private Label storySpeaker;
    private Label storyLine;
    private Label storyProgress;
    private Label playerNameFeedback;
    private Label guideEquipName;
    private Label guideEquipAbility;
    private Label storyReplayWorldTitle;
    private Label tutorialStep;
    private Label tutorialKicker;
    private Label tutorialTitle;
    private Label tutorialBody;
    private TextField playerNameField;

    private readonly Dictionary<int, Texture2D> guideTextures =
        new Dictionary<int, Texture2D>();
    private readonly Dictionary<int, Texture2D> guideFaceTextures =
        new Dictionary<int, Texture2D>();
    private bool lastMenuVisible;
    private bool lastGameplayVisible;
    private bool lastResultVisible;
    private bool lastPortrait;
    private bool achievementsOpen;
    private float nextRefresh;
    private int previewedGuideId = -1;
    private Coroutine voiceSubtitleRoutine;
    private int lastHudCombo = -1;
    private int comboAnimationVersion;
    private string activeComboTierClass = "tier-0";
    private bool campaignOpen;
    private bool storyOpen;
    private bool creditsOpen;
    private bool finalCelebrationOpen;
    private bool guideEquipOpen;
    private bool storyReplayOpen;
    private bool postgameOpen;
    private int selectedPostgameEncounter = -1;
    private bool outcomePresentationPending;
    private int outcomePresentationVersion;
    private readonly DeleteSaveConfirmation deleteSaveConfirmation =
        new DeleteSaveConfirmation();
    private int selectedCampaignWorld;
    private int selectedCampaignLevel;
    private int selectedReplayWorld;
    private int pendingEquipGuideId = -1;
    private string activeStoryWorldClass = "world-0";
    private int lastStoryPresentationSceneId = int.MinValue;
    private int lastStoryPresentationBeat = -1;
    private readonly MementoStoryPortraitContinuity storyPortraitContinuity =
        new MementoStoryPortraitContinuity();
    private readonly MementoNarrativeRunner narrativeRunner = new MementoNarrativeRunner();
    private readonly MementoTutorialFlow tutorialFlow = new MementoTutorialFlow();
    private readonly VisualNovelPlayerIdentity playerIdentity =
        new VisualNovelPlayerIdentity("MementoMatch.PlayerName", "ASPIRANTE", 16);
    private VisualNovelTypewriterEffect storyTypewriter;
    private MementoStoryVoicePlayer storyVoice;
    private ShellPage activeShellPage = ShellPage.Home;
    private MementoMatchStoryScene activeStoryScene;
    private Action storyCompletion;
    private Action guideEquipContinuation;
    private Action pendingPlayerNameContinuation;

    private void Start()
    {
        document = GetComponent<UIDocument>();
        gameManager = GameManager.Instance != null
            ? GameManager.Instance
            : FindFirstObjectByType<GameManager>();
        cardMatchUI = FindFirstObjectByType<CardMatchUI>();

        if (document == null || document.rootVisualElement == null)
        {
            Debug.LogError("Memento Match UI cannot start: UIDocument is missing.");
            enabled = false;
            return;
        }

        storyVoice = new MementoStoryVoicePlayer(gameObject);
        BindElements();
        if (GetComponent<MementoTelemetry>() == null) gameObject.AddComponent<MementoTelemetry>();
        BuildCollectionPreviews();
        MementoGameChrome.Install(menuScreen);
        if (storyLine != null)
            storyTypewriter = new VisualNovelTypewriterEffect(storyLine);
        RegisterCallbacks();
        suppliesView = new MementoSuppliesView(document.rootVisualElement, gameManager, cardMatchUI);
        postgameSelectionView = new MementoPostgameSelectionView(
            document.rootVisualElement,
            OpenPostgameSelection,
            ClosePostgameSelection,
            SelectPostgameEncounter,
            StartSelectedPostgameEncounter,
            ApplyPostgamePortrait);
        MementoMatchVoicePlayer.LineStarted += OnVoiceLineStarted;
        if (cardMatchUI != null)
        {
            cardMatchUI.PlayerDuelPairMade += OnPlayerDuelPairMade;
            cardMatchUI.OpponentDuelPairMade += OnOpponentDuelPairMade;
            cardMatchUI.OpponentDuelSkillActivated += OnOpponentDuelSkillActivated;
            cardMatchUI.DuelOutcomePresentationRequested +=
                OnDuelOutcomePresentationRequested;
            cardMatchUI.PlayerMoveResolved += OnPlayerMoveResolved;
        }

        gameplayHud.pickingMode = PickingMode.Ignore;
        // Transparent containers and combo/subtitle decoration must not consume
        // pointer events intended for the uGUI cards beneath the Toolkit HUD.
        gameplayHud.Query<VisualElement>().ForEach(element =>
        {
            if (!(element is Button))
                element.pickingMode = PickingMode.Ignore;
        });
        if (powerCluster != null)
            powerCluster.pickingMode = PickingMode.Ignore;
        if (powerGuideFace != null)
            powerGuideFace.pickingMode = PickingMode.Ignore;
        if (powerButton != null)
            powerButton.pickingMode = PickingMode.Position;
        if (comboBurst != null)
            comboBurst.pickingMode = PickingMode.Ignore;
        if (skillCutInOverlay != null)
            skillCutInOverlay.pickingMode = PickingMode.Position;
        if (duelDialogueOverlay != null)
            duelDialogueOverlay.pickingMode = PickingMode.Position;
        if (gameManager != null)
        {
            gameManager.UseToolkitMenu(true);
            gameManager.ProgressionChanged += OnProgressionChanged;
            gameManager.ResultChanged += OnResultChanged;
            gameManager.GuidePowerPresentationRequested += OnGuidePowerPresentationRequested;
            gameManager.PostgameScriptedEncounterRequested += OnPostgameScriptedEncounterRequested;
            previewedGuideId = gameManager.SelectedGuideId >= 0
                ? gameManager.SelectedGuideId
                : AnimalMemoryContentIds.AkiGuide;
            ApplySelectedSet(gameManager.SelectedSetId);
        }
        else
        {
            previewedGuideId = 0;
            ApplySelectedSet(0);
        }

        ApplyGuidePreview(previewedGuideId);
        if ((gameManager == null || gameManager.IsMainMenuOpen) &&
            (gameManager == null || gameManager.SelectedGuideId >= 0))
            PlayGuideVoice(previewedGuideId, MementoMenuVoiceContext.MenuArrival);
        BuildAchievementList();
        RefreshAll(true);
    }

    private void Update()
    {
        RefreshAll(false);
        suppliesView?.Refresh();
    }

    private void OnDisable()
    {
        storyVoice?.Stop();
    }

    private void OnDestroy()
    {
        openingMemory?.Dispose();
        suppliesView?.Dispose();
        postgameSelectionView?.Dispose();
        postgameSelectionView = null;
        storyVoice?.Dispose();
        storyVoice = null;
        UnregisterCallbacks();
        storyTypewriter?.Dispose();
        storyTypewriter = null;
        MementoMatchVoicePlayer.LineStarted -= OnVoiceLineStarted;
        if (cardMatchUI != null)
        {
            cardMatchUI.PlayerDuelPairMade -= OnPlayerDuelPairMade;
            cardMatchUI.OpponentDuelPairMade -= OnOpponentDuelPairMade;
            cardMatchUI.OpponentDuelSkillActivated -= OnOpponentDuelSkillActivated;
            cardMatchUI.DuelOutcomePresentationRequested -=
                OnDuelOutcomePresentationRequested;
            cardMatchUI.PlayerMoveResolved -= OnPlayerMoveResolved;
        }
        if (gameManager != null)
        {
            gameManager.ProgressionChanged -= OnProgressionChanged;
            gameManager.ResultChanged -= OnResultChanged;
            gameManager.GuidePowerPresentationRequested -= OnGuidePowerPresentationRequested;
            gameManager.PostgameScriptedEncounterRequested -= OnPostgameScriptedEncounterRequested;
        }

        foreach (KeyValuePair<int, Texture2D> entry in guideTextures)
        {
            // Only legacy decoded portraits belong to this view. Illustrated
            // portraits are borrowed from the shared cache, including Aki/Mika/Yoru.
            if (entry.Key < AnimalMemoryContentIds.HanaGuide &&
                entry.Value != null && !MementoIllustratedCharacters.OwnsTexture(entry.Value))
                Destroy(entry.Value);
        }

        guideTextures.Clear();
        foreach (Texture2D texture in guideFaceTextures.Values)
        {
            if (texture != null)
                Destroy(texture);
        }

        guideFaceTextures.Clear();
    }

    private void BindElements()
    {
        VisualElement root = document.rootVisualElement;
        safeAreaView = new MementoSafeAreaView(root);
        menuScreen = root.Q<VisualElement>("screen");
        gameplayHud = root.Q<VisualElement>("gameplay-hud");
        gameplayHeader = gameplayHud?.Q<VisualElement>(className: "hud-top");
        gameplayFooter = gameplayHud?.Q<VisualElement>(className: "hud-bottom");
        rivalPanel = root.Q<VisualElement>("duel-versus-hud");
        playerHealthCard = root.Q<VisualElement>("player-health-card");
        rivalHealthCard = root.Q<VisualElement>("rival-health-card");
        playerHealthTrack = root.Q<VisualElement>("player-health-track");
        playerHealthFill = root.Q<VisualElement>("player-health-fill");
        rivalFill = root.Q<VisualElement>("rival-fill");
        resultOverlay = root.Q<VisualElement>("result-overlay");
        achievementsOverlay = root.Q<VisualElement>("achievements-overlay");
        achievementList = root.Q<VisualElement>("achievement-list");
        hostessImage = root.Q<VisualElement>("hostess-image");
        powerGuideFace = root.Q<VisualElement>("power-guide-face");
        powerCluster = root.Q<VisualElement>("power-cluster");
        skillCutInOverlay = root.Q<VisualElement>("skill-cut-in-overlay");
        skillCutInFace = root.Q<VisualElement>("skill-cut-in-face");
        skillProcToast = root.Q<VisualElement>("skill-proc-toast");
        skillProcFace = root.Q<VisualElement>("skill-proc-face");
        duelDialogueOverlay = root.Q<VisualElement>("duel-dialogue-overlay");
        duelDialogueFace = root.Q<VisualElement>("duel-dialogue-face");
        voiceSubtitleBox = root.Q<VisualElement>("voice-subtitle-box");
        voiceSubtitleFace = root.Q<VisualElement>("voice-subtitle-face");
        comboBurst = root.Q<VisualElement>("combo-burst");
        campaignOverlay = root.Q<VisualElement>("campaign-overlay");
        campaignGuardian = root.Q<VisualElement>("campaign-guardian");
        storyOverlay = root.Q<VisualElement>("story-overlay");
        storyStage = root.Q<VisualElement>("story-stage");
        storyPortrait = root.Q<VisualElement>("story-portrait");
        playerNameOverlay = root.Q<VisualElement>("player-name-overlay");
        creditsOverlay = root.Q<VisualElement>("credits-overlay");
        homeGuideImage = root.Q<VisualElement>("home-guide-image");
        storyBackground = root.Q<VisualElement>("story-background");
        guideEquipOverlay = root.Q<VisualElement>("guide-equip-overlay");
        guideEquipPortrait = root.Q<VisualElement>("guide-equip-portrait");
        storyReplayOverlay = root.Q<VisualElement>("story-replay-overlay");
        tutorialOverlay = root.Q<VisualElement>("tutorial-overlay");
        tutorialGuideFace = root.Q<VisualElement>("tutorial-guide-face");
        finalCelebrationOverlay = root.Q<VisualElement>("final-celebration-overlay");
        shellPages[(int)ShellPage.Home] = root.Q<VisualElement>("page-home");
        shellPages[(int)ShellPage.Story] = root.Q<VisualElement>("page-story");
        shellPages[(int)ShellPage.Team] = root.Q<VisualElement>("page-team");
        shellPages[(int)ShellPage.Collection] = root.Q<VisualElement>("page-collection");
        shellPages[(int)ShellPage.Settings] = root.Q<VisualElement>("page-settings");

        easyButton = root.Q<Button>("easy-button");
        normalButton = root.Q<Button>("normal-button");
        hardButton = root.Q<Button>("hard-button");
        forestThemeButton = root.Q<Button>("forest-theme");
        coralThemeButton = root.Q<Button>("coral-theme");
        nightThemeButton = root.Q<Button>("night-theme");
        gardenThemeButton = root.Q<Button>("garden-theme");
        sweetsThemeButton = root.Q<Button>("sweets-theme");
        clockworkThemeButton = root.Q<Button>("clockwork-theme");
        akiGuideButton = root.Q<Button>("aki-guide");
        mikaGuideButton = root.Q<Button>("mika-guide");
        yoruGuideButton = root.Q<Button>("yoru-guide");
        hanaGuideButton = root.Q<Button>("hana-guide");
        momoGuideButton = root.Q<Button>("momo-guide");
        challengeButton = root.Q<Button>("challenge-button");
        achievementsButton = root.Q<Button>("achievements-button");
        achievementsCloseButton = root.Q<Button>("achievements-close");
        muteButton = root.Q<Button>("mute-button");
        musicVolumeSlider = root.Q<Slider>("music-volume");
        sfxVolumeSlider = root.Q<Slider>("sfx-volume");
        voiceVolumeSlider = root.Q<Slider>("voice-volume");
        orientationAutoButton = root.Q<Button>("orientation-auto");
        orientationPortraitButton = root.Q<Button>("orientation-portrait");
        orientationLandscapeButton = root.Q<Button>("orientation-landscape");
        RefreshOrientationButtons();
        deleteSaveButton = root.Q<Button>("delete-save-button");
        quitButton = root.Q<Button>("quit-button");
        returnMenuButton = root.Q<Button>("return-menu-button");
        powerButton = root.Q<Button>("power-button");
        resultContinueAdButton = root.Q<Button>("result-continue-ad");
        resultContinueOffer = root.Q<VisualElement>("result-continue-offer");
        resultContinueStatus = root.Q<Label>("result-continue-status");
        resultRetryButton = root.Q<Button>("result-retry");
        resultMenuButton = root.Q<Button>("result-menu");
        campaignOpenButton = root.Q<Button>("campaign-open");
        campaignCloseButton = root.Q<Button>("campaign-close");
        campaignStartButton = root.Q<Button>("campaign-start");
        campaignFinalButton = root.Q<Button>("campaign-final");
        campaignCreditsButton = root.Q<Button>("campaign-credits");
        storyNextButton = root.Q<Button>("story-next");
        storySkipButton = root.Q<Button>("story-skip");
        playerNameConfirmButton = root.Q<Button>("player-name-confirm");
        creditsCloseButton = root.Q<Button>("credits-close");
        duelDialogueContinueButton = root.Q<Button>("duel-dialogue-continue");
        homeStoryButton = root.Q<Button>("home-story-button");
        guideEquipConfirmButton = root.Q<Button>("guide-equip-confirm");
        guideEquipKeepButton = root.Q<Button>("guide-equip-keep");
        storyReplayOpenButton = root.Q<Button>("story-replay-open");
        storyReplayCloseButton = root.Q<Button>("story-replay-close");
        storyReplayFinalButton = root.Q<Button>("story-replay-final");
        storyReplayEpilogueButton = root.Q<Button>("story-replay-epilogue");
        tutorialContinueButton = root.Q<Button>("tutorial-continue");
        finalCreditsButton = root.Q<Button>("final-credits");
        finalEquipArchiveButton = root.Q<Button>("final-equip-archive");
        shellNavButtons[(int)ShellPage.Home] = root.Q<Button>("nav-home");
        shellNavButtons[(int)ShellPage.Story] = root.Q<Button>("nav-story");
        shellNavButtons[(int)ShellPage.Team] = root.Q<Button>("nav-team");
        shellNavButtons[(int)ShellPage.Collection] = root.Q<Button>("nav-collection");
        shellNavButtons[(int)ShellPage.Settings] = root.Q<Button>("nav-settings");
        for (int i = 0; i < campaignWorldButtons.Length; i++)
            campaignWorldButtons[i] = root.Q<Button>($"campaign-world-{i}");
        for (int i = 0; i < campaignNodeButtons.Length; i++)
            campaignNodeButtons[i] = root.Q<Button>($"campaign-node-{i}");
        for (int i = 0; i < storyReplayWorldButtons.Length; i++)
            storyReplayWorldButtons[i] = root.Q<Button>($"story-replay-world-{i}");
        for (int i = 0; i < storyReplaySceneButtons.Length; i++)
            storyReplaySceneButtons[i] = root.Q<Button>($"story-replay-scene-{i}");
        for (int i = 0; i < finalTeamFaces.Length; i++)
            finalTeamFaces[i] = root.Q<VisualElement>($"final-team-face-{i}");

        themeDescription = root.Q<Label>("theme-description");
        loadoutSummary = root.Q<Label>("loadout-summary");
        setBadge = root.Q<Label>("set-badge");
        progressLabel = root.Q<Label>("progress-label");
        pawStarsLabel = root.Q<Label>("paw-stars-label");
        guideName = root.Q<Label>("guide-name");
        guideAbility = root.Q<Label>("guide-ability");
        hostessLine = root.Q<Label>("hostess-line");
        hudMode = root.Q<Label>("hud-mode");
        hudScore = root.Q<Label>("hud-score");
        hudCombo = root.Q<Label>("hud-combo");
        hudTurns = root.Q<Label>("hud-turns");
        hudMatches = root.Q<Label>("hud-matches");
        rivalName = root.Q<Label>("rival-name");
        rivalStatus = root.Q<Label>("rival-status");
        playerHealthLabel = root.Q<Label>("player-health-label");
        rivalHealthLabel = root.Q<Label>("rival-health-label");
        resultKicker = root.Q<Label>("result-kicker");
        resultTitle = root.Q<Label>("result-title");
        resultSummary = root.Q<Label>("result-summary");
        resultStars = root.Q<Label>("result-stars");
        resultAchievements = root.Q<Label>("result-achievements");
        skillCutInKicker = root.Q<Label>("skill-cut-in-kicker");
        skillCutInName = root.Q<Label>("skill-cut-in-name");
        skillCutInQuote = root.Q<Label>("skill-cut-in-quote");
        skillProcName = root.Q<Label>("skill-proc-name");
        skillProcEffect = root.Q<Label>("skill-proc-effect");
        duelDialogueName = root.Q<Label>("duel-dialogue-name");
        duelDialogueLine = root.Q<Label>("duel-dialogue-line");
        voiceSubtitleSpeaker = root.Q<Label>("voice-subtitle-speaker");
        voiceSubtitleLine = root.Q<Label>("voice-subtitle-line");
        comboBurstCount = root.Q<Label>("combo-burst-count");
        comboBurstHeading = root.Q<Label>("combo-burst-heading");
        comboBurstRank = root.Q<Label>("combo-burst-rank");
        campaignOverview = root.Q<Label>("campaign-overview");
        campaignWorldKicker = root.Q<Label>("campaign-world-kicker");
        campaignWorldTitle = root.Q<Label>("campaign-world-title");
        campaignWorldProgress = root.Q<Label>("campaign-world-progress");
        campaignLevelKicker = root.Q<Label>("campaign-level-kicker");
        campaignLevelTitle = root.Q<Label>("campaign-level-title");
        campaignLevelObjective = root.Q<Label>("campaign-level-objective");
        campaignLevelRules = root.Q<Label>("campaign-level-rules");
        storyKicker = root.Q<Label>("story-kicker");
        storyChapter = root.Q<Label>("story-chapter");
        storyStageChapter = root.Q<Label>("story-stage-chapter");
        storySpeaker = root.Q<Label>("story-speaker");
        storyLine = root.Q<Label>("story-line");
        storyProgress = root.Q<Label>("story-progress");
        playerNameFeedback = root.Q<Label>("player-name-feedback");
        guideEquipName = root.Q<Label>("guide-equip-name");
        guideEquipAbility = root.Q<Label>("guide-equip-ability");
        storyReplayWorldTitle = root.Q<Label>("story-replay-world-title");
        tutorialStep = root.Q<Label>("tutorial-step");
        tutorialKicker = root.Q<Label>("tutorial-kicker");
        tutorialTitle = root.Q<Label>("tutorial-title");
        tutorialBody = root.Q<Label>("tutorial-body");
        playerNameField = root.Q<TextField>("player-name-field");
    }

    private void RegisterCallbacks()
    {
        skillCutInOverlay?.RegisterCallback<ClickEvent>(OnSkipSkillClick);
        if (easyButton != null) easyButton.clicked += StartEasy;
        if (normalButton != null) normalButton.clicked += StartNormal;
        if (hardButton != null) hardButton.clicked += StartHard;
        if (forestThemeButton != null) forestThemeButton.clicked += SelectForest;
        if (coralThemeButton != null) coralThemeButton.clicked += SelectCoral;
        if (nightThemeButton != null) nightThemeButton.clicked += SelectNight;
        if (gardenThemeButton != null) gardenThemeButton.clicked += SelectGarden;
        if (sweetsThemeButton != null) sweetsThemeButton.clicked += SelectSweets;
        if (clockworkThemeButton != null) clockworkThemeButton.clicked += SelectClockwork;
        if (akiGuideButton != null) akiGuideButton.clicked += SelectAki;
        if (mikaGuideButton != null) mikaGuideButton.clicked += SelectMika;
        if (yoruGuideButton != null) yoruGuideButton.clicked += SelectYoru;
        if (hanaGuideButton != null) hanaGuideButton.clicked += SelectHana;
        if (momoGuideButton != null) momoGuideButton.clicked += SelectMomo;
        if (challengeButton != null) challengeButton.clicked += StartPreviewedChallenge;
        if (achievementsButton != null) achievementsButton.clicked += OpenAchievements;
        if (achievementsCloseButton != null) achievementsCloseButton.clicked += CloseAchievements;
        if (muteButton != null) muteButton.clicked += ToggleMute;
        musicVolumeSlider?.RegisterValueChangedCallback(OnMusicVolumeChanged);
        sfxVolumeSlider?.RegisterValueChangedCallback(OnSfxVolumeChanged);
        voiceVolumeSlider?.RegisterValueChangedCallback(OnVoiceVolumeChanged);
        if (orientationAutoButton != null) orientationAutoButton.clicked += SelectAutomaticOrientation;
        if (orientationPortraitButton != null) orientationPortraitButton.clicked += SelectPortraitOrientation;
        if (orientationLandscapeButton != null) orientationLandscapeButton.clicked += SelectLandscapeOrientation;
        if (deleteSaveButton != null) deleteSaveButton.clicked += RequestDeleteSave;
        if (quitButton != null) quitButton.clicked += Quit;
        if (returnMenuButton != null) returnMenuButton.clicked += ReturnToMenu;
        if (powerButton != null) powerButton.clicked += UsePower;
        if (resultContinueAdButton != null) resultContinueAdButton.clicked += RequestRewardedContinue;
        if (resultRetryButton != null) resultRetryButton.clicked += Retry;
        if (resultMenuButton != null) resultMenuButton.clicked += ReturnToMenu;
        if (campaignOpenButton != null) campaignOpenButton.clicked += OpenCampaign;
        if (campaignCloseButton != null) campaignCloseButton.clicked += CloseCampaign;
        if (campaignStartButton != null) campaignStartButton.clicked += StartSelectedCampaignLevel;
        if (campaignFinalButton != null) campaignFinalButton.clicked += SelectFinalCampaignLevel;
        if (campaignCreditsButton != null) campaignCreditsButton.clicked += OpenCredits;
        if (storyNextButton != null) storyNextButton.clicked += AdvanceStory;
        if (storySkipButton != null) storySkipButton.clicked += SkipStory;
        if (playerNameConfirmButton != null) playerNameConfirmButton.clicked += ConfirmPlayerName;
        playerNameField?.RegisterCallback<KeyDownEvent>(OnPlayerNameKeyDown);
        if (creditsCloseButton != null) creditsCloseButton.clicked += CloseCredits;
        if (duelDialogueContinueButton != null) duelDialogueContinueButton.clicked += ContinueDuelOutcome;
        if (homeStoryButton != null) homeStoryButton.clicked += OpenStoryPage;
        if (guideEquipConfirmButton != null) guideEquipConfirmButton.clicked += EquipPendingGuide;
        if (guideEquipKeepButton != null) guideEquipKeepButton.clicked += KeepCurrentGuide;
        if (storyReplayOpenButton != null) storyReplayOpenButton.clicked += OpenStoryReplay;
        if (storyReplayCloseButton != null) storyReplayCloseButton.clicked += CloseStoryReplay;
        if (storyReplayFinalButton != null) storyReplayFinalButton.clicked += ReplayFinalBoss;
        if (storyReplayEpilogueButton != null) storyReplayEpilogueButton.clicked += ReplayEpilogue;
        if (tutorialContinueButton != null) tutorialContinueButton.clicked += ContinueTutorial;
        if (finalCreditsButton != null) finalCreditsButton.clicked += OpenCredits;
        if (finalEquipArchiveButton != null) finalEquipArchiveButton.clicked += EquipArchiveAndOpenCredits;
        if (shellNavButtons[0] != null) shellNavButtons[0].clicked += OpenHomePage;
        if (shellNavButtons[1] != null) shellNavButtons[1].clicked += OpenStoryPage;
        if (shellNavButtons[2] != null) shellNavButtons[2].clicked += OpenTeamPage;
        if (shellNavButtons[3] != null) shellNavButtons[3].clicked += OpenCollectionPage;
        if (shellNavButtons[4] != null) shellNavButtons[4].clicked += OpenSettingsPage;
        if (campaignWorldButtons[0] != null) campaignWorldButtons[0].clicked += SelectCampaignWorld0;
        if (campaignWorldButtons[1] != null) campaignWorldButtons[1].clicked += SelectCampaignWorld1;
        if (campaignWorldButtons[2] != null) campaignWorldButtons[2].clicked += SelectCampaignWorld2;
        if (campaignWorldButtons[3] != null) campaignWorldButtons[3].clicked += SelectCampaignWorld3;
        if (campaignWorldButtons[4] != null) campaignWorldButtons[4].clicked += SelectCampaignWorld4;
        if (storyReplayWorldButtons[0] != null) storyReplayWorldButtons[0].clicked += SelectReplayWorld0;
        if (storyReplayWorldButtons[1] != null) storyReplayWorldButtons[1].clicked += SelectReplayWorld1;
        if (storyReplayWorldButtons[2] != null) storyReplayWorldButtons[2].clicked += SelectReplayWorld2;
        if (storyReplayWorldButtons[3] != null) storyReplayWorldButtons[3].clicked += SelectReplayWorld3;
        if (storyReplayWorldButtons[4] != null) storyReplayWorldButtons[4].clicked += SelectReplayWorld4;
        if (storyReplaySceneButtons[0] != null) storyReplaySceneButtons[0].clicked += ReplayWorldIntro;
        if (storyReplaySceneButtons[1] != null) storyReplaySceneButtons[1].clicked += ReplayWorldChallenge;
        if (storyReplaySceneButtons[2] != null) storyReplaySceneButtons[2].clicked += ReplayWorldRecruit;
        if (campaignNodeButtons[0] != null) campaignNodeButtons[0].clicked += SelectCampaignNode0;
        if (campaignNodeButtons[1] != null) campaignNodeButtons[1].clicked += SelectCampaignNode1;
        if (campaignNodeButtons[2] != null) campaignNodeButtons[2].clicked += SelectCampaignNode2;
        if (campaignNodeButtons[3] != null) campaignNodeButtons[3].clicked += SelectCampaignNode3;
    }

    private void UnregisterCallbacks()
    {
        skillCutInOverlay?.UnregisterCallback<ClickEvent>(OnSkipSkillClick);
        if (easyButton != null) easyButton.clicked -= StartEasy;
        if (normalButton != null) normalButton.clicked -= StartNormal;
        if (hardButton != null) hardButton.clicked -= StartHard;
        if (forestThemeButton != null) forestThemeButton.clicked -= SelectForest;
        if (coralThemeButton != null) coralThemeButton.clicked -= SelectCoral;
        if (nightThemeButton != null) nightThemeButton.clicked -= SelectNight;
        if (gardenThemeButton != null) gardenThemeButton.clicked -= SelectGarden;
        if (sweetsThemeButton != null) sweetsThemeButton.clicked -= SelectSweets;
        if (clockworkThemeButton != null) clockworkThemeButton.clicked -= SelectClockwork;
        if (akiGuideButton != null) akiGuideButton.clicked -= SelectAki;
        if (mikaGuideButton != null) mikaGuideButton.clicked -= SelectMika;
        if (yoruGuideButton != null) yoruGuideButton.clicked -= SelectYoru;
        if (hanaGuideButton != null) hanaGuideButton.clicked -= SelectHana;
        if (momoGuideButton != null) momoGuideButton.clicked -= SelectMomo;
        if (challengeButton != null) challengeButton.clicked -= StartPreviewedChallenge;
        if (achievementsButton != null) achievementsButton.clicked -= OpenAchievements;
        if (achievementsCloseButton != null) achievementsCloseButton.clicked -= CloseAchievements;
        if (muteButton != null) muteButton.clicked -= ToggleMute;
        musicVolumeSlider?.UnregisterValueChangedCallback(OnMusicVolumeChanged);
        sfxVolumeSlider?.UnregisterValueChangedCallback(OnSfxVolumeChanged);
        voiceVolumeSlider?.UnregisterValueChangedCallback(OnVoiceVolumeChanged);
        if (orientationAutoButton != null) orientationAutoButton.clicked -= SelectAutomaticOrientation;
        if (orientationPortraitButton != null) orientationPortraitButton.clicked -= SelectPortraitOrientation;
        if (orientationLandscapeButton != null) orientationLandscapeButton.clicked -= SelectLandscapeOrientation;
        if (deleteSaveButton != null) deleteSaveButton.clicked -= RequestDeleteSave;
        if (quitButton != null) quitButton.clicked -= Quit;
        if (returnMenuButton != null) returnMenuButton.clicked -= ReturnToMenu;
        if (powerButton != null) powerButton.clicked -= UsePower;
        if (resultContinueAdButton != null) resultContinueAdButton.clicked -= RequestRewardedContinue;
        if (resultRetryButton != null) resultRetryButton.clicked -= Retry;
        if (resultMenuButton != null) resultMenuButton.clicked -= ReturnToMenu;
        if (campaignOpenButton != null) campaignOpenButton.clicked -= OpenCampaign;
        if (campaignCloseButton != null) campaignCloseButton.clicked -= CloseCampaign;
        if (campaignStartButton != null) campaignStartButton.clicked -= StartSelectedCampaignLevel;
        if (campaignFinalButton != null) campaignFinalButton.clicked -= SelectFinalCampaignLevel;
        if (campaignCreditsButton != null) campaignCreditsButton.clicked -= OpenCredits;
        if (storyNextButton != null) storyNextButton.clicked -= AdvanceStory;
        if (storySkipButton != null) storySkipButton.clicked -= SkipStory;
        if (playerNameConfirmButton != null) playerNameConfirmButton.clicked -= ConfirmPlayerName;
        playerNameField?.UnregisterCallback<KeyDownEvent>(OnPlayerNameKeyDown);
        if (creditsCloseButton != null) creditsCloseButton.clicked -= CloseCredits;
        if (duelDialogueContinueButton != null) duelDialogueContinueButton.clicked -= ContinueDuelOutcome;
        if (homeStoryButton != null) homeStoryButton.clicked -= OpenStoryPage;
        if (guideEquipConfirmButton != null) guideEquipConfirmButton.clicked -= EquipPendingGuide;
        if (guideEquipKeepButton != null) guideEquipKeepButton.clicked -= KeepCurrentGuide;
        if (storyReplayOpenButton != null) storyReplayOpenButton.clicked -= OpenStoryReplay;
        if (storyReplayCloseButton != null) storyReplayCloseButton.clicked -= CloseStoryReplay;
        if (storyReplayFinalButton != null) storyReplayFinalButton.clicked -= ReplayFinalBoss;
        if (storyReplayEpilogueButton != null) storyReplayEpilogueButton.clicked -= ReplayEpilogue;
        if (tutorialContinueButton != null) tutorialContinueButton.clicked -= ContinueTutorial;
        if (finalCreditsButton != null) finalCreditsButton.clicked -= OpenCredits;
        if (finalEquipArchiveButton != null) finalEquipArchiveButton.clicked -= EquipArchiveAndOpenCredits;
        if (shellNavButtons[0] != null) shellNavButtons[0].clicked -= OpenHomePage;
        if (shellNavButtons[1] != null) shellNavButtons[1].clicked -= OpenStoryPage;
        if (shellNavButtons[2] != null) shellNavButtons[2].clicked -= OpenTeamPage;
        if (shellNavButtons[3] != null) shellNavButtons[3].clicked -= OpenCollectionPage;
        if (shellNavButtons[4] != null) shellNavButtons[4].clicked -= OpenSettingsPage;
        if (campaignWorldButtons[0] != null) campaignWorldButtons[0].clicked -= SelectCampaignWorld0;
        if (campaignWorldButtons[1] != null) campaignWorldButtons[1].clicked -= SelectCampaignWorld1;
        if (campaignWorldButtons[2] != null) campaignWorldButtons[2].clicked -= SelectCampaignWorld2;
        if (campaignWorldButtons[3] != null) campaignWorldButtons[3].clicked -= SelectCampaignWorld3;
        if (campaignWorldButtons[4] != null) campaignWorldButtons[4].clicked -= SelectCampaignWorld4;
        if (storyReplayWorldButtons[0] != null) storyReplayWorldButtons[0].clicked -= SelectReplayWorld0;
        if (storyReplayWorldButtons[1] != null) storyReplayWorldButtons[1].clicked -= SelectReplayWorld1;
        if (storyReplayWorldButtons[2] != null) storyReplayWorldButtons[2].clicked -= SelectReplayWorld2;
        if (storyReplayWorldButtons[3] != null) storyReplayWorldButtons[3].clicked -= SelectReplayWorld3;
        if (storyReplayWorldButtons[4] != null) storyReplayWorldButtons[4].clicked -= SelectReplayWorld4;
        if (storyReplaySceneButtons[0] != null) storyReplaySceneButtons[0].clicked -= ReplayWorldIntro;
        if (storyReplaySceneButtons[1] != null) storyReplaySceneButtons[1].clicked -= ReplayWorldChallenge;
        if (storyReplaySceneButtons[2] != null) storyReplaySceneButtons[2].clicked -= ReplayWorldRecruit;
        if (campaignNodeButtons[0] != null) campaignNodeButtons[0].clicked -= SelectCampaignNode0;
        if (campaignNodeButtons[1] != null) campaignNodeButtons[1].clicked -= SelectCampaignNode1;
        if (campaignNodeButtons[2] != null) campaignNodeButtons[2].clicked -= SelectCampaignNode2;
        if (campaignNodeButtons[3] != null) campaignNodeButtons[3].clicked -= SelectCampaignNode3;
    }

    private void BuildCollectionPreviews()
    {
        if (cardMatchUI == null)
            return;

        Button[] buttons =
        {
            forestThemeButton,
            coralThemeButton,
            nightThemeButton,
            gardenThemeButton,
            sweetsThemeButton,
            clockworkThemeButton
        };

        for (int setId = 0; setId < buttons.Length; setId++)
        {
            Button button = buttons[setId];
            if (button == null || button.Q<VisualElement>("collection-preview-strip") != null)
                continue;

            VisualElement strip = new VisualElement
            {
                name = "collection-preview-strip",
                pickingMode = PickingMode.Ignore
            };
            strip.AddToClassList("collection-preview-strip");
            IReadOnlyList<Sprite> sprites = cardMatchUI.GetCardSetPreview(setId);
            int count = Mathf.Min(4, sprites != null ? sprites.Count : 0);
            for (int i = 0; i < count; i++)
            {
                Sprite sprite = sprites[i];
                if (sprite == null)
                    continue;
                VisualElement face = new VisualElement
                {
                    pickingMode = PickingMode.Ignore
                };
                face.AddToClassList("collection-preview-face");
                face.style.backgroundImage = new StyleBackground(sprite);
                strip.Add(face);
            }

            button.Add(strip);
        }
    }

    private void SelectAutomaticOrientation() => SelectOrientation(MementoOrientationMode.Automatic);
    private void SelectPortraitOrientation() => SelectOrientation(MementoOrientationMode.Portrait);
    private void SelectLandscapeOrientation() => SelectOrientation(MementoOrientationMode.Landscape);

    private void SelectOrientation(MementoOrientationMode mode)
    {
        MementoOrientationSettings.SetMode(mode);
        RefreshOrientationButtons();
    }

    private void RefreshOrientationButtons()
    {
        MementoOrientationMode mode = MementoOrientationSettings.Current;
        orientationAutoButton?.EnableInClassList("selected", mode == MementoOrientationMode.Automatic);
        orientationPortraitButton?.EnableInClassList("selected", mode == MementoOrientationMode.Portrait);
        orientationLandscapeButton?.EnableInClassList("selected", mode == MementoOrientationMode.Landscape);
    }

    private static void OnMusicVolumeChanged(ChangeEvent<float> evt)
    {
        MementoAudioSettings.SetMusicVolume(evt.newValue);
    }

    private static void OnSfxVolumeChanged(ChangeEvent<float> evt)
    {
        MementoAudioSettings.SetSfxVolume(evt.newValue);
    }

    private static void OnVoiceVolumeChanged(ChangeEvent<float> evt)
    {
        MementoAudioSettings.SetVoiceVolume(evt.newValue);
    }

    private void RefreshAll(bool force)
    {
        if (gameManager == null)
        {
            gameManager = GameManager.Instance;
            if (gameManager == null)
                return;

            gameManager.UseToolkitMenu(true);
            gameManager.ProgressionChanged += OnProgressionChanged;
            gameManager.ResultChanged += OnResultChanged;
            gameManager.GuidePowerPresentationRequested += OnGuidePowerPresentationRequested;
            gameManager.PostgameScriptedEncounterRequested += OnPostgameScriptedEncounterRequested;
            previewedGuideId = gameManager.SelectedGuideId;
            force = true;
        }

        Vector2 viewportSize = new Vector2(Screen.width, Screen.height);
        Rect viewportSafeArea = MementoGameplayLayout.ValidSafeArea(viewportSize, Screen.safeArea);
        bool portrait = MementoGameplayLayout.IsPortrait(viewportSize);
        if (force || portrait != lastPortrait || viewportSize != lastViewportSize ||
            viewportSafeArea != lastViewportSafeArea)
        {
            menuScreen?.EnableInClassList("portrait", portrait);
            menuScreen?.EnableInClassList("small-landscape", !portrait && Screen.height < 600);
            gameplayHud?.EnableInClassList("portrait", portrait);
            resultOverlay?.EnableInClassList("portrait", portrait);
            achievementsOverlay?.EnableInClassList("portrait", portrait);
            skillCutInOverlay?.EnableInClassList("portrait", portrait);
            duelDialogueOverlay?.EnableInClassList("portrait", portrait);
            campaignOverlay?.EnableInClassList("portrait", portrait);
            VisualNovelResponsiveLayout.Apply(
                storyOverlay,
                Screen.width,
                Screen.height);
            playerNameOverlay?.EnableInClassList("portrait", portrait);
            skillProcToast?.EnableInClassList("portrait", portrait);
            creditsOverlay?.EnableInClassList("portrait", portrait);
            guideEquipOverlay?.EnableInClassList("portrait", portrait);
            storyReplayOverlay?.EnableInClassList("portrait", portrait);
            tutorialOverlay?.EnableInClassList("portrait", portrait);
            finalCelebrationOverlay?.EnableInClassList("portrait", portrait);
            if (portrait)
            {
                if (playerHealthCard != null)
                    playerHealthCard.style.width = new Length(100f, LengthUnit.Percent);
                if (rivalHealthCard != null)
                    rivalHealthCard.style.width = new Length(100f, LengthUnit.Percent);
            }
            else
            {
                if (playerHealthCard != null)
                    playerHealthCard.style.width = StyleKeyword.Null;
                if (rivalHealthCard != null)
                    rivalHealthCard.style.width = StyleKeyword.Null;
            }

            if (document.panelSettings != null)
            {
                document.panelSettings.referenceResolution = new Vector2Int(1920, 1080);
                document.panelSettings.match = MementoGameplayLayout.ScaleMatch(viewportSize);
            }
            lastPortrait = portrait;
            lastViewportSize = viewportSize;
            lastViewportSafeArea = viewportSafeArea;
        }

        safeAreaView?.Refresh(viewportSize, viewportSafeArea);
        if (cardMatchUI != null && safeAreaView != null &&
            safeAreaView.TryGetHudInsets(viewportSize, viewportSafeArea,
                gameplayHeader, gameplayFooter, out float topInset, out float bottomInset))
            cardMatchUI.SetPresentationViewport(viewportSafeArea, topInset, bottomInset);

        float panelWidth = document.rootVisualElement.resolvedStyle.width;
        bool compact = panelWidth > 0f && panelWidth < 1000f;
        gameplayHud?.EnableInClassList("compact", compact);

        bool menuVisible = gameManager.IsMainMenuOpen;
        bool resultVisible = gameManager.IsResultOverlayOpen &&
            !outcomePresentationPending;
        bool gameplayVisible = gameManager.IsGameplayActive;

        if (force || menuVisible != lastMenuVisible)
        {
            SetDisplay(menuScreen, menuVisible);
            lastMenuVisible = menuVisible;
        }

        if (force || gameplayVisible != lastGameplayVisible)
        {
            SetDisplay(gameplayHud, gameplayVisible);
            if (gameplayVisible)
            {
                lastHudCombo = -1;
                if (!lastGameplayVisible)
                    BeginTutorialIfNeeded();
            }
            else
            {
                HideComboBurst();
                tutorialFlow.Reset();
            }
            lastGameplayVisible = gameplayVisible;
        }

        if (force || resultVisible != lastResultVisible)
        {
            SetDisplay(resultOverlay, resultVisible);
            lastResultVisible = resultVisible;
        }

        SetDisplay(achievementsOverlay,
            achievementsOpen && menuVisible && !storyOpen && !creditsOpen &&
            !guideEquipOpen && !storyReplayOpen && !finalCelebrationOpen);
        SetDisplay(campaignOverlay,
            campaignOpen && menuVisible && !storyOpen && !creditsOpen &&
            !guideEquipOpen && !storyReplayOpen && !finalCelebrationOpen);
        SetDisplay(storyReplayOverlay,
            storyReplayOpen && menuVisible && !storyOpen && !creditsOpen &&
            !guideEquipOpen && !finalCelebrationOpen);
        SetDisplay(guideEquipOverlay,
            guideEquipOpen && menuVisible && !finalCelebrationOpen);
        SetDisplay(storyOverlay, storyOpen);
        SetDisplay(playerNameOverlay, pendingPlayerNameContinuation != null);
        SetDisplay(creditsOverlay, creditsOpen);
        SetDisplay(finalCelebrationOverlay, finalCelebrationOpen && menuVisible);
        RefreshTutorialPresentation(gameplayVisible);
        if (gameplayVisible)
            RefreshComboPresentation(gameManager.CurrentCombo);

        if (force || Time.unscaledTime >= nextRefresh)
        {
            nextRefresh = Time.unscaledTime + 0.1f;
            if (menuVisible)
            {
                RefreshMenu();
                RefreshPostgameSelection(menuVisible);
            }
            if (gameplayVisible)
                RefreshHud();
            if (resultVisible)
                RefreshResult();
        }
    }

    private void RefreshComboPresentation(int combo)
    {
        if (combo == lastHudCombo)
            return;

        int previousCombo = lastHudCombo;
        lastHudCombo = combo;
        if (hudCombo != null)
        {
            hudCombo.text = ComboPresentationRules.CounterText(combo);
            hudCombo.EnableInClassList("combo-active", combo > 0);
            hudCombo.EnableInClassList("combo-legendary", combo >= 10);
        }

        if (previousCombo < 0)
        {
            ApplyComboPresentation(
                ComboPresentationRules.ForCombo(combo),
                false);
        }
        else if (combo > previousCombo)
        {
            ApplyComboPresentation(
                ComboPresentationRules.ForCombo(combo),
                true);
        }
        else if (combo == 0 && previousCombo > 0)
        {
            ApplyComboPresentation(
                ComboPresentationRules.ForBreak(previousCombo),
                true);
        }
        else
        {
            ApplyComboPresentation(
                ComboPresentationRules.ForCombo(combo),
                false);
        }
    }

    private void ApplyComboPresentation(
        ComboPresentationState state,
        bool animate)
    {
        if (comboBurst == null)
            return;

        comboAnimationVersion++;
        int animationVersion = comboAnimationVersion;
        SetComboTier(state.TierClass);
        comboBurstCount.text =
            ComboPresentationRules.CounterText(state.Count);
        comboBurstHeading.text = state.Heading;
        comboBurstRank.text = state.Rank;

        if (!state.IsVisible)
        {
            HideComboBurst();
            return;
        }

        comboBurst.style.display = DisplayStyle.Flex;
        comboBurst.EnableInClassList("combo-visible", true);
        comboBurst.EnableInClassList("combo-punch", false);
        hudCombo?.EnableInClassList("combo-punch", false);

        if (animate)
        {
            comboBurst.schedule.Execute(() =>
            {
                if (animationVersion != comboAnimationVersion)
                    return;
                comboBurst.EnableInClassList("combo-punch", true);
                hudCombo?.EnableInClassList("combo-punch", true);
            }).StartingIn(16);

            comboBurst.schedule.Execute(() =>
            {
                if (animationVersion != comboAnimationVersion)
                    return;
                comboBurst.EnableInClassList("combo-punch", false);
                hudCombo?.EnableInClassList("combo-punch", false);
            }).StartingIn(145);
        }

        if (state.IsBroken)
        {
            comboBurst.schedule.Execute(() =>
            {
                if (animationVersion != comboAnimationVersion ||
                    lastHudCombo != 0)
                {
                    return;
                }

                comboBurst.EnableInClassList("combo-visible", false);
            }).StartingIn(620);

            comboBurst.schedule.Execute(() =>
            {
                if (animationVersion != comboAnimationVersion ||
                    lastHudCombo != 0)
                {
                    return;
                }

                comboBurst.style.display = DisplayStyle.None;
            }).StartingIn(820);
        }
    }

    private void SetComboTier(string tierClass)
    {
        if (comboBurst == null)
            return;

        comboBurst.EnableInClassList(activeComboTierClass, false);
        comboBurst.EnableInClassList("broken", false);
        activeComboTierClass = string.IsNullOrEmpty(tierClass)
            ? "tier-0"
            : tierClass;
        comboBurst.EnableInClassList(activeComboTierClass, true);
    }

    private void HideComboBurst()
    {
        comboAnimationVersion++;
        if (comboBurst == null)
            return;

        comboBurst.EnableInClassList("combo-visible", false);
        comboBurst.EnableInClassList("combo-punch", false);
        comboBurst.style.display = DisplayStyle.None;
        hudCombo?.EnableInClassList("combo-punch", false);
    }

    private void RefreshMenu()
    {
        int stars = gameManager.PawStars;
        if (pawStarsLabel != null)
            pawStarsLabel.text = $"{stars} ★";

        RefreshSetButton(forestThemeButton, 0, "AKI · BOSQUE");
        RefreshSetButton(coralThemeButton, 1, "MIKA · CORAL");
        RefreshSetButton(nightThemeButton, 2, "YORU · CIELO");
        RefreshSetButton(gardenThemeButton, 3, "HANA · JARDÍN");
        RefreshSetButton(sweetsThemeButton, 4, "MOMO · DULCES");
        RefreshSetButton(clockworkThemeButton, 5, "ARCHIVO · RELOJES");
        RefreshGuideButton(akiGuideButton, 0, "AKI");
        RefreshGuideButton(mikaGuideButton, 1, "MIKA");
        RefreshGuideButton(yoruGuideButton, 2, "YORU");
        RefreshGuideButton(hanaGuideButton, 3, "HANA");
        RefreshGuideButton(momoGuideButton, 4, "MOMO");

        int unlocked = 0;
        for (int i = 0; i < AnimalMemoryContentIds.SetCount; i++)
        {
            if (gameManager.IsSetUnlocked(i))
                unlocked++;
        }

        if (setBadge != null)
            setBadge.text = $"BARAJAS {unlocked} / {AnimalMemoryContentIds.SetCount}";

        if (loadoutSummary != null)
        {
            if (gameManager.SelectedGuideId < 0)
            {
                loadoutSummary.text =
                    $"BARAJA · {gameManager.GetSetName(gameManager.SelectedSetId).ToUpperInvariant()}" +
                    "  /  GUÍA · SIN EQUIPAR  /  PASIVA · PULSO DEL RELEVO";
            }
            else
            {
                string skill = gameManager.GetGuideSkillDescription(
                    gameManager.SelectedGuideId);
                int separator = skill.IndexOf(':');
                string power = separator > 0
                    ? skill.Substring(0, separator).ToUpperInvariant()
                    : "PODER";
                loadoutSummary.text =
                    $"BARAJA · {gameManager.GetSetName(gameManager.SelectedSetId).ToUpperInvariant()}" +
                    $"  /  GUÍA · {gameManager.GetGuideName(gameManager.SelectedGuideId).ToUpperInvariant()}" +
                    $" · {power}  /  PASIVA · PULSO DEL RELEVO";
            }
        }

        int achievements = CountBits(gameManager.AchievementMask);
        if (progressLabel != null)
        {
            progressLabel.text =
                $"{gameManager.CampaignClearedCount} / {MementoMatchCampaignRules.LevelCount} niveles · " +
                $"{gameManager.CampaignTotalStars} / {MementoMatchCampaignRules.LevelCount * 3} estrellas · " +
                $"{achievements}/{MementoMatchAchievementIds.Count} logros";
        }
        int homeGuideId = gameManager.SelectedGuideId >= 0
            ? gameManager.SelectedGuideId
            : AnimalMemoryContentIds.AkiGuide;
        ApplyGuidePortrait(homeGuideImage, homeGuideId);
        for (int world = 0; world < 6; world++)
            menuScreen?.EnableInClassList("lobby-world-" + world, world == homeGuideId);
        if (campaignOpen)
            RefreshCampaign();
        if (campaignOpenButton != null)
        {
            campaignOpenButton.text = gameManager.CampaignEndingUnlocked
                ? "REVISITAR CAMPAÑA"
                : $"CONTINUAR · NIVEL {gameManager.CampaignRecommendedLevel + 1}";
        }

        if (muteButton != null)
            muteButton.text = gameManager.IsMuted ? "SONIDO: OFF" : "SONIDO: ON";
        musicVolumeSlider?.SetValueWithoutNotify(MementoAudioSettings.MusicVolume);
        sfxVolumeSlider?.SetValueWithoutNotify(MementoAudioSettings.SfxVolume);
        voiceVolumeSlider?.SetValueWithoutNotify(MementoAudioSettings.VoiceVolume);
        RefreshOrientationButtons();

        RefreshChallengeButton();
        RefreshAchievementStates();
    }

    private void OpenCampaign()
    {
        if (gameManager == null)
            return;
        MementoMatchSfx.PlayUiConfirm();
        ShowShellPage(ShellPage.Story, false);
        achievementsOpen = false;
        creditsOpen = false;
        storyVoice?.Stop();
        storyOpen = false;
        guideEquipOpen = false;
        storyReplayOpen = false;
        postgameOpen = false;
        selectedCampaignLevel = gameManager.CampaignRecommendedLevel;
        selectedCampaignWorld = Mathf.Clamp(
            selectedCampaignLevel / MementoMatchCampaignRules.LevelsPerWorld,
            0,
            MementoMatchCampaignRules.WorldCount - 1);
        campaignOpen = true;
        RefreshCampaign();
        RefreshAll(true);
    }

    private void CloseCampaign()
    {
        MementoMatchSfx.PlayUiBack();
        campaignOpen = false;
        RefreshAll(true);
    }

    private void OpenPostgameSelection()
    {
        if (gameManager == null || !gameManager.IsPostgameAvailable)
            return;

        MementoMatchSfx.PlayUiConfirm();
        achievementsOpen = false;
        campaignOpen = false;
        creditsOpen = false;
        storyVoice?.Stop();
        storyOpen = false;
        guideEquipOpen = false;
        storyReplayOpen = false;
        int recommended = gameManager.RecommendedPostgameEncounter;
        selectedPostgameEncounter = recommended >= 0
            ? recommended
            : MementoPostgameEncounters.Count - 1;
        postgameOpen = true;
        RefreshAll(true);
    }

    private void ClosePostgameSelection()
    {
        MementoMatchSfx.PlayUiBack();
        postgameOpen = false;
        RefreshAll(true);
    }

    private void SelectPostgameEncounter(int encounterId)
    {
        if (gameManager == null || !gameManager.IsPostgameEncounterUnlocked(encounterId))
        {
            MementoMatchSfx.PlayUiLocked();
            return;
        }

        MementoMatchSfx.PlayUiConfirm();
        selectedPostgameEncounter = encounterId;
        RefreshPostgameSelection(gameManager.IsMainMenuOpen);
    }

    private void StartSelectedPostgameEncounter()
    {
        if (gameManager == null || selectedPostgameEncounter < 0 ||
            !gameManager.IsPostgameEncounterUnlocked(selectedPostgameEncounter))
        {
            MementoMatchSfx.PlayUiLocked();
            return;
        }

        if (MementoPostgameStory.HasIntro(selectedPostgameEncounter))
        {
            Action begin = BeginSelectedPostgameEncounter;
            // La apertura de la puerta se presenta una sola vez, antes del
            // primer encuentro; ningún guardado extra: el propio progreso la desactiva.
            if (selectedPostgameEncounter == 0 && gameManager.PostgameCompletedMask == 0)
            {
                ShowStory(MementoPostgameStory.BuildOpeningScene(), () =>
                    ShowStory(MementoPostgameStory.BuildIntro(0), begin));
                return;
            }

            ShowStory(
                MementoPostgameStory.BuildIntro(selectedPostgameEncounter), begin);
            return;
        }

        BeginSelectedPostgameEncounter();
    }

    private void BeginSelectedPostgameEncounter()
    {
        postgameOpen = false;
        storyVoice?.Stop();
        storyOpen = false;
        if (gameManager != null &&
            gameManager.StartPostgameEncounter(selectedPostgameEncounter))
        {
            MementoMatchSfx.PlayUiConfirm();
            RefreshAll(true);
            return;
        }

        MementoMatchSfx.PlayUiLocked();
    }

    private void OnPostgameScriptedEncounterRequested(int encounterId)
    {
        postgameOpen = false;
        ShowStory(MementoPostgameStory.BuildScriptedScene(), () =>
        {
            if (gameManager == null ||
                !gameManager.CompletePostgameScriptedEncounter())
            {
                RefreshAll(true);
                return;
            }

            OpenPostgameSelection();
        });
    }

    private void ApplyPostgamePortrait(VisualElement target, int guideId) =>
        ApplyGuidePortrait(target, guideId);

    private void RefreshPostgameSelection(bool menuVisible)
    {
        if (postgameSelectionView == null || gameManager == null)
            return;

        postgameSelectionView.Refresh(
            gameManager.IsPostgameAvailable,
            postgameOpen && menuVisible && !storyOpen && !creditsOpen &&
                !guideEquipOpen && !storyReplayOpen && !finalCelebrationOpen,
            lastPortrait,
            selectedPostgameEncounter,
            gameManager.PostgameCompletedMask,
            gameManager.IsPostgameEncounterUnlocked,
            menuVisible && !gameManager.IsGameplayActive);
    }

    /// <summary>Ganar selecciona el siguiente encuentro; perder conserva el actual.</summary>
    private void PresentPendingPostgameStory()
    {
        if (gameManager == null || !gameManager.LastResultWasPostgame)
            return;

        int encounterId = Mathf.Clamp(gameManager.LastPostgameEncounter, 0,
            MementoPostgameEncounters.Count - 1);
        if (gameManager.LastResultWasVictory)
        {
            ShowStory(
                encounterId == MementoPostgameEncounters.Count - 1
                    ? MementoPostgameStory.BuildEpilogue()
                    : MementoPostgameStory.BuildVictory(encounterId),
                OpenPostgameSelection);
            return;
        }

        ShowStory(MementoPostgameStory.BuildDefeat(encounterId), OpenPostgameSelection);
    }

    private void SelectCampaignWorld0() => SelectCampaignWorld(0);
    private void SelectCampaignWorld1() => SelectCampaignWorld(1);
    private void SelectCampaignWorld2() => SelectCampaignWorld(2);
    private void SelectCampaignWorld3() => SelectCampaignWorld(3);
    private void SelectCampaignWorld4() => SelectCampaignWorld(4);
    private void SelectCampaignNode0() => SelectCampaignNode(0);
    private void SelectCampaignNode1() => SelectCampaignNode(1);
    private void SelectCampaignNode2() => SelectCampaignNode(2);
    private void SelectCampaignNode3() => SelectCampaignNode(3);

    private void SelectFinalCampaignLevel()
    {
        if (gameManager == null)
            return;

        MementoMatchSfx.PlayUiConfirm();
        selectedCampaignLevel = MementoMatchCampaignRules.FinalBossLevelId;
        selectedCampaignWorld = MementoMatchCampaignRules.WorldCount - 1;
        RefreshCampaign();
    }

    private void SelectCampaignWorld(int worldId)
    {
        if (gameManager == null)
            return;
        int firstLevel = worldId * MementoMatchCampaignRules.LevelsPerWorld;
        if (!gameManager.IsCampaignLevelUnlocked(firstLevel))
        {
            MementoMatchSfx.PlayUiLocked();
            return;
        }

        MementoMatchSfx.PlayUiConfirm();
        selectedCampaignWorld = worldId;
        selectedCampaignLevel = firstLevel;
        for (int node = 0; node < MementoMatchCampaignRules.LevelsPerWorld; node++)
        {
            int candidate = firstLevel + node;
            if (gameManager.IsCampaignLevelUnlocked(candidate) &&
                gameManager.GetCampaignStars(candidate) == 0)
            {
                selectedCampaignLevel = candidate;
                break;
            }
        }

        RefreshCampaign();
    }

    private void SelectCampaignNode(int nodeInWorld)
    {
        if (gameManager == null)
            return;
        bool finalSelected = selectedCampaignLevel == MementoMatchCampaignRules.FinalBossLevelId;
        if (finalSelected && nodeInWorld != 0) return;
        int levelId = finalSelected ? MementoMatchCampaignRules.FinalBossLevelId :
            selectedCampaignWorld * MementoMatchCampaignRules.LevelsPerWorld + nodeInWorld;
        if (!gameManager.IsCampaignLevelUnlocked(levelId))
        {
            MementoMatchSfx.PlayUiLocked();
            return;
        }

        MementoMatchSfx.PlayUiConfirm();
        selectedCampaignLevel = levelId;
        RefreshCampaign();
    }

    private void RefreshCampaign()
    {
        if (gameManager == null)
            return;

        string overview =
            $"{gameManager.CampaignClearedCount} / {MementoMatchCampaignRules.LevelCount} niveles · " +
            $"{gameManager.CampaignTotalStars} / {MementoMatchCampaignRules.LevelCount * 3} estrellas";
        if (campaignOverview != null)
            campaignOverview.text = overview;

        int firstLevel =
            selectedCampaignWorld * MementoMatchCampaignRules.LevelsPerWorld;
        int worldCleared = 0;
        for (int node = 0; node < MementoMatchCampaignRules.LevelsPerWorld; node++)
        {
            if (gameManager.GetCampaignStars(firstLevel + node) > 0)
                worldCleared++;
        }

        bool showingFinalBoss =
            selectedCampaignLevel == MementoMatchCampaignRules.FinalBossLevelId;
        if (campaignWorldKicker != null)
            campaignWorldKicker.text = showingFinalBoss
                ? "FINAL · ZONA CENTRAL"
                : $"MUNDO {selectedCampaignWorld + 1} / {MementoMatchCampaignRules.WorldCount}";
        if (campaignWorldTitle != null)
            campaignWorldTitle.text = MementoMatchCampaignRules.GetWorldName(
                showingFinalBoss
                    ? MementoMatchCampaignRules.FinalBossWorldId
                    : selectedCampaignWorld).ToUpperInvariant();
        if (campaignWorldProgress != null)
            campaignWorldProgress.text = showingFinalBoss
                ? "LAS CINCO ALIANZAS ESTÁN REGISTRADAS"
                : $"{worldCleared} / {MementoMatchCampaignRules.LevelsPerWorld} completados";
        ApplyGuideFace(campaignGuardian,
            showingFinalBoss
                ? AnimalMemoryContentIds.UltimateMasterOpponent
                : selectedCampaignWorld);

        for (int world = 0; world < campaignWorldButtons.Length; world++)
        {
            Button button = campaignWorldButtons[world];
            if (button == null)
                continue;
            bool unlocked = gameManager.IsCampaignLevelUnlocked(
                world * MementoMatchCampaignRules.LevelsPerWorld);
            button.EnableInClassList(
                "selected",
                !showingFinalBoss && world == selectedCampaignWorld);
            button.EnableInClassList("locked", !unlocked);
            button.SetEnabled(unlocked);
        }

        if (campaignFinalButton != null)
        {
            bool finalUnlocked = gameManager.IsCampaignLevelUnlocked(
                MementoMatchCampaignRules.FinalBossLevelId);
            int finalStars = gameManager.GetCampaignStars(
                MementoMatchCampaignRules.FinalBossLevelId);
            int baseCleared = Mathf.Min(
                MementoMatchCampaignRules.BaseLevelCount,
                gameManager.CampaignClearedCount);
            campaignFinalButton.text = finalUnlocked
                ? "FINAL\nREI"
                : $"FINAL\n{baseCleared}/{MementoMatchCampaignRules.BaseLevelCount}";
            campaignFinalButton.EnableInClassList("selected", showingFinalBoss);
            campaignFinalButton.EnableInClassList("locked", !finalUnlocked);
            campaignFinalButton.EnableInClassList("cleared", finalStars > 0);
            SetDisplay(campaignFinalButton, finalUnlocked);
            campaignFinalButton.SetEnabled(finalUnlocked);
        }

        for (int node = 0; node < campaignNodeButtons.Length; node++)
        {
            int levelId = showingFinalBoss ? MementoMatchCampaignRules.FinalBossLevelId : firstLevel + node;
            MementoMatchCampaignLevel level =
                MementoMatchCampaignRules.GetLevel(levelId);
            Button button = campaignNodeButtons[node];
            if (button == null)
                continue;
            SetDisplay(button, !showingFinalBoss || node == 0);
            if (showingFinalBoss && node != 0) continue;
            bool unlocked = gameManager.IsCampaignLevelUnlocked(levelId);
            int stars = gameManager.GetCampaignStars(levelId);
            button.text =
                $"{levelId + 1:00}\n{CampaignNodeTitle(level.Title)}\n{CampaignStars(stars)}";
            button.EnableInClassList("locked", !unlocked);
            button.EnableInClassList("cleared", stars > 0);
            button.EnableInClassList("boss", level.IsBoss);
            button.EnableInClassList("selected", levelId == selectedCampaignLevel);
            button.SetEnabled(unlocked);
        }

        MementoMatchCampaignLevel selected =
            MementoMatchCampaignRules.GetLevel(selectedCampaignLevel);
        int selectedStars = gameManager.GetCampaignStars(selectedCampaignLevel);
        bool selectedUnlocked =
            gameManager.IsCampaignLevelUnlocked(selectedCampaignLevel);
        if (campaignLevelKicker != null)
            campaignLevelKicker.text =
                $"NIVEL {selected.Id + 1} · {(selected.IsFinalBoss ? "EXAMEN FINAL" : selected.IsBoss ? "DUELO DE GUARDIANA" : "PARTIDA")}";
        if (campaignLevelTitle != null)
            campaignLevelTitle.text = selected.Title.ToUpperInvariant();
        if (campaignLevelObjective != null)
            campaignLevelObjective.text = selected.Objective;
        if (campaignLevelRules != null)
            campaignLevelRules.text =
                $"{selected.Rows} × {selected.Columns} · " +
                $"RÉCORD {CampaignStars(selectedStars)} · " +
                "★ COMPLETA  ★ EFICIENCIA  ★ RETO";
        if (campaignStartButton != null)
        {
            campaignStartButton.text = selectedUnlocked
                ? selected.IsBoss
                    ? $"INICIAR DUELO · {gameManager.GetGuideName(selected.OpponentGuideId).ToUpperInvariant()}"
                    : $"JUGAR NIVEL {selected.Id + 1}"
                : "NIVEL BLOQUEADO";
            campaignStartButton.SetEnabled(selectedUnlocked);
        }

        if (campaignCreditsButton != null)
        {
            campaignCreditsButton.style.display =
                gameManager.CampaignEndingUnlocked
                    ? DisplayStyle.Flex
                    : DisplayStyle.None;
        }
    }

    private static string CampaignNodeTitle(string title)
    {
        int separator = title.IndexOf('·');
        string compact = separator >= 0 ? title.Substring(0, separator).Trim() : title;
        return compact.Length <= 18
            ? compact.ToUpperInvariant()
            : compact.Substring(0, 18).ToUpperInvariant();
    }

    private static string CampaignStars(int count)
    {
        count = Mathf.Clamp(count, 0, 3);
        return new string('★', count) + new string('○', 3 - count);
    }

    private bool RequirePlayerName(Action continuation)
    {
        if (playerIdentity.HasCustomName)
            return false;

        pendingPlayerNameContinuation = continuation;
        if (playerNameField != null)
        {
            playerNameField.value = string.Empty;
            playerNameField.RemoveFromClassList("invalid");
            playerNameField.schedule.Execute(playerNameField.Focus).StartingIn(32);
        }

        if (playerNameFeedback != null)
            playerNameFeedback.text =
                "Este será el nombre que usarán contigo durante el Relevo.";
        RefreshAll(true);
        return true;
    }

    private void ConfirmPlayerName()
    {
        if (!playerIdentity.TrySetName(
                playerNameField != null ? playerNameField.value : string.Empty,
                out string normalized))
        {
            MementoMatchSfx.PlayUiLocked();
            playerNameField?.AddToClassList("invalid");
            if (playerNameFeedback != null)
                playerNameFeedback.text =
                    "Escribe un nombre de al menos dos caracteres.";
            return;
        }

        MementoMatchSfx.PlayUiConfirm();
        playerNameField?.RemoveFromClassList("invalid");
        if (playerNameFeedback != null)
            playerNameFeedback.text = $"Bienvenido al Relevo, {normalized}.";
        Action continuation = pendingPlayerNameContinuation;
        pendingPlayerNameContinuation = null;
        RefreshAll(true);
        continuation?.Invoke();
    }

    private void OnPlayerNameKeyDown(KeyDownEvent evt)
    {
        if (evt.keyCode != KeyCode.Return &&
            evt.keyCode != KeyCode.KeypadEnter)
        {
            return;
        }

        evt.StopPropagation();
        ConfirmPlayerName();
    }

    private void OpenStoryReplay()
    {
        if (gameManager == null)
            return;

        MementoMatchSfx.PlayUiConfirm();
        achievementsOpen = false;
        campaignOpen = false;
        creditsOpen = false;
        guideEquipOpen = false;
        postgameOpen = false;
        storyReplayOpen = true;

        int recommendedWorld = Mathf.Clamp(
            gameManager.CampaignRecommendedLevel /
            MementoMatchCampaignRules.LevelsPerWorld,
            0,
            MementoMatchCampaignRules.WorldCount - 1);
        selectedReplayWorld = FindNearestUnlockedReplayWorld(recommendedWorld);
        RefreshStoryReplay();
        RefreshAll(true);
    }

    private void CloseStoryReplay()
    {
        MementoMatchSfx.PlayUiBack();
        storyReplayOpen = false;
        RefreshAll(true);
    }

    private int FindNearestUnlockedReplayWorld(int preferredWorld)
    {
        for (int distance = 0; distance < MementoMatchCampaignRules.WorldCount; distance++)
        {
            int lower = preferredWorld - distance;
            if (lower >= 0 && IsReplayWorldUnlocked(lower))
                return lower;
            int upper = preferredWorld + distance;
            if (upper < MementoMatchCampaignRules.WorldCount &&
                IsReplayWorldUnlocked(upper))
                return upper;
        }

        return 0;
    }

    private bool IsReplayWorldUnlocked(int worldId)
    {
        if (gameManager == null)
            return false;

        for (int phase = 0; phase < 3; phase++)
        {
            if (MementoStoryFlowRules.IsReplaySceneAvailable(
                    MementoStoryFlowRules.GetReplaySceneId(worldId, phase),
                    gameManager.IsCampaignStorySeen))
                return true;
        }

        return false;
    }

    private void SelectReplayWorld0() => SelectReplayWorld(0);
    private void SelectReplayWorld1() => SelectReplayWorld(1);
    private void SelectReplayWorld2() => SelectReplayWorld(2);
    private void SelectReplayWorld3() => SelectReplayWorld(3);
    private void SelectReplayWorld4() => SelectReplayWorld(4);

    private void SelectReplayWorld(int worldId)
    {
        if (!IsReplayWorldUnlocked(worldId))
        {
            MementoMatchSfx.PlayUiLocked();
            return;
        }

        MementoMatchSfx.PlayUiConfirm();
        selectedReplayWorld = worldId;
        RefreshStoryReplay();
    }

    private void RefreshStoryReplay()
    {
        if (gameManager == null)
            return;

        for (int world = 0; world < storyReplayWorldButtons.Length; world++)
        {
            Button button = storyReplayWorldButtons[world];
            bool unlocked = IsReplayWorldUnlocked(world);
            button?.SetEnabled(unlocked);
            button?.EnableInClassList("locked", !unlocked);
            button?.EnableInClassList("selected",
                unlocked && world == selectedReplayWorld);
        }

        if (storyReplayWorldTitle != null)
            storyReplayWorldTitle.text =
                MementoMatchCampaignRules.GetWorldName(selectedReplayWorld)
                    .ToUpperInvariant();

        string[] labels = { "INTRODUCCIÓN", "DESAFÍO", "RECLUTAMIENTO" };
        for (int phase = 0; phase < storyReplaySceneButtons.Length; phase++)
        {
            int sceneId = MementoStoryFlowRules.GetReplaySceneId(
                selectedReplayWorld,
                phase);
            bool unlocked = MementoStoryFlowRules.IsReplaySceneAvailable(
                sceneId,
                gameManager.IsCampaignStorySeen);
            Button button = storyReplaySceneButtons[phase];
            if (button == null)
                continue;
            button.text = unlocked
                ? $"{labels[phase]}\nVOLVER A VER"
                : $"{labels[phase]}\nBLOQUEADO";
            button.SetEnabled(unlocked);
            button.EnableInClassList("locked", !unlocked);
        }

        bool finalUnlocked =
            gameManager.IsCampaignStorySeen(MementoMatchCampaignRules.FinalBossSceneId);
        if (storyReplayFinalButton != null)
        {
            storyReplayFinalButton.text = finalUnlocked
                ? "FINAL · REI · VOLVER A VER"
                : "FINAL · REI · BLOQUEADO";
            storyReplayFinalButton.SetEnabled(finalUnlocked);
            storyReplayFinalButton.EnableInClassList("locked", !finalUnlocked);
        }

        bool epilogueUnlocked =
            gameManager.IsCampaignStorySeen(MementoMatchCampaignRules.EpilogueSceneId);
        if (storyReplayEpilogueButton != null)
        {
            storyReplayEpilogueButton.text = epilogueUnlocked
                ? "EPÍLOGO · VOLVER A VER"
                : "EPÍLOGO · BLOQUEADO";
            storyReplayEpilogueButton.SetEnabled(epilogueUnlocked);
            storyReplayEpilogueButton.EnableInClassList(
                "locked",
                !epilogueUnlocked);
        }
    }

    private void ReplayWorldIntro() => ReplaySelectedStoryPhase(0);
    private void ReplayWorldChallenge() => ReplaySelectedStoryPhase(1);
    private void ReplayWorldRecruit() => ReplaySelectedStoryPhase(2);

    private void ReplaySelectedStoryPhase(int phase)
    {
        ReplayStoryScene(MementoStoryFlowRules.GetReplaySceneId(
            selectedReplayWorld,
            phase));
    }

    private void ReplayFinalBoss()
    {
        ReplayStoryScene(MementoMatchCampaignRules.FinalBossSceneId);
    }

    private void ReplayEpilogue()
    {
        ReplayStoryScene(MementoMatchCampaignRules.EpilogueSceneId);
    }

    private void ReplayStoryScene(int sceneId)
    {
        if (gameManager == null ||
            !MementoStoryFlowRules.IsReplaySceneAvailable(
                sceneId,
                gameManager.IsCampaignStorySeen))
        {
            MementoMatchSfx.PlayUiLocked();
            return;
        }

        MementoMatchSfx.PlayUiConfirm();
        storyReplayOpen = false;
        ShowStory(
            MementoMatchCampaignRules.GetStoryScene(sceneId),
            () =>
            {
                storyReplayOpen = true;
                RefreshStoryReplay();
                RefreshAll(true);
            });
    }

    private void StartSelectedCampaignLevel()
    {
        if (gameManager == null ||
            !gameManager.IsCampaignLevelUnlocked(selectedCampaignLevel))
        {
            MementoMatchSfx.PlayUiLocked();
            return;
        }

        if (RequireOpeningPractice(StartSelectedCampaignLevel))
            return;

        if (RequirePlayerName(StartSelectedCampaignLevel))
            return;

        if (gameManager.TryGetCampaignPreScene(
                selectedCampaignLevel,
                out MementoMatchStoryScene scene))
        {
            ShowStory(scene, BeginSelectedCampaignLevel);
            return;
        }

        BeginSelectedCampaignLevel();
    }

    private void BeginSelectedCampaignLevel()
    {
        campaignOpen = false;
        storyVoice?.Stop();
        storyOpen = false;
        if (gameManager != null &&
            gameManager.StartCampaignLevel(selectedCampaignLevel))
        {
            MementoMatchSfx.PlayUiConfirm();
            RefreshAll(true);
            return;
        }

        MementoMatchSfx.PlayUiLocked();
    }

    private bool RequireOpeningPractice(Action continuation)
    {
        if (gameManager == null || gameManager.TutorialCompleted) return false;
        if (openingMemory != null && openingMemory.IsOpen) return true;
        storyVoice?.Stop();
        MementoMatchVoicePlayer.StopCurrent();
        MementoTelemetry.Record("tutorial_start");
        openingMemory = new MementoOpeningMemoryView(document.rootVisualElement, false, () =>
        {
            gameManager.CompleteTutorial();
            MementoTelemetry.Record("tutorial_complete");
            continuation();
        });
        return true;
    }

    private void BeginTutorialIfNeeded()
    {
        tutorialFlow.Reset();
        if (gameManager != null)
            tutorialFlow.TryBegin(gameManager.TutorialCompleted, gameManager.ActiveCampaignLevel);
        RenderTutorial();
    }

    private void ContinueTutorial()
    {
        if (!tutorialFlow.IsOverlayVisible)
            return;

        MementoMatchSfx.PlayUiConfirm();
        bool guideLesson = tutorialFlow.Stage == MementoTutorialStage.GuideLesson;
        if (tutorialFlow.Continue() && guideLesson)
        {
            gameManager?.CompleteGuideTutorial();
            tutorialFlow.Reset();
        }

        RenderTutorial();
        RefreshAll(true);
    }

    private void OnPlayerMoveResolved(bool matched, int combo)
    {
        if (!tutorialFlow.ObservePlayerMove(matched))
            return;

        if (tutorialFlow.Stage == MementoTutorialStage.Complete)
        {
            gameManager?.CompleteTutorial();
            tutorialFlow.Reset();
        }

        RenderTutorial();
        RefreshAll(true);
    }

    private void RefreshTutorialPresentation(bool gameplayVisible)
    {
        // This is a hint inside the already-reserved header, never a board overlay.
        // The ability lesson waits for a real turn, so even Undo has a valid target.
        bool usablePower = gameplayVisible && gameManager != null &&
            cardMatchUI != null && !cardMatchUI.IsBusy &&
            gameManager.CurrentTurns > 0 && gameManager.IsPlayerTurn &&
            gameManager.IsPowerReady && !gameManager.IsGuidePowerBlocked;
        if (gameManager != null && tutorialFlow.TryBeginGuideLesson(
                gameManager.GuideTutorialCompleted,
                gameManager.SelectedGuideId,
                gameManager.SelectedGuideId >= 0 &&
                    gameManager.IsGuideUnlocked(gameManager.SelectedGuideId),
                usablePower))
        {
            ApplyGuideFace(tutorialGuideFace, gameManager.SelectedGuideId);
            RenderTutorial();
        }

        bool guideLesson = tutorialFlow.Stage == MementoTutorialStage.GuideLesson;
        bool initialReveal = cardMatchUI != null && cardMatchUI.IsBusy &&
            gameManager != null && gameManager.CurrentTurns == 0;
        bool visible = gameplayVisible && tutorialFlow.IsOverlayVisible &&
            !initialReveal && (!guideLesson || usablePower);
        SetDisplay(tutorialOverlay, visible);
        SetDisplay(tutorialGuideFace, visible && guideLesson &&
            gameplayHud != null && !gameplayHud.ClassListContains("portrait") &&
            !gameplayHud.ClassListContains("compact"));
        gameplayHud?.EnableInClassList("tutorial-active", visible);
        RefreshTutorialTypography(visible);
    }

    private void RefreshTutorialTypography(bool visible)
    {
        if (gameplayHeader == null || tutorialTitle == null || tutorialBody == null)
            return;

        if (!visible || gameplayHud == null || gameplayHud.ClassListContains("portrait"))
        {
            // Restore the original layout when dismissed and on every rotation
            // back to portrait. The shared viewport measures this same header.
            gameplayHeader.style.height = StyleKeyword.Null;
            tutorialTitle.style.fontSize = StyleKeyword.Null;
            tutorialBody.style.fontSize = StyleKeyword.Null;
            if (tutorialOverlay != null)
                tutorialOverlay.style.right = StyleKeyword.Null;
            if (tutorialContinueButton != null)
            {
                tutorialContinueButton.style.width = StyleKeyword.Null;
                tutorialContinueButton.style.minWidth = StyleKeyword.Null;
                tutorialContinueButton.style.height = StyleKeyword.Null;
                tutorialContinueButton.style.minHeight = StyleKeyword.Null;
                tutorialContinueButton.style.fontSize = StyleKeyword.Null;
            }
            if (returnMenuButton != null)
            {
                returnMenuButton.style.width = StyleKeyword.Null;
                returnMenuButton.style.minWidth = StyleKeyword.Null;
                returnMenuButton.style.height = StyleKeyword.Null;
                returnMenuButton.style.minHeight = StyleKeyword.Null;
                returnMenuButton.style.fontSize = StyleKeyword.Null;
            }
            return;
        }

        MementoTutorialTypography typography = MementoTutorialTypography.ForLandscape(
            document.rootVisualElement.resolvedStyle.width, Screen.width);
        gameplayHeader.style.height = typography.HeaderHeight;
        tutorialTitle.style.fontSize = typography.TitleSize;
        tutorialBody.style.fontSize = typography.BodySize;
        if (tutorialOverlay != null)
            tutorialOverlay.style.right = typography.ContentRightInset;
        if (tutorialContinueButton != null)
        {
            tutorialContinueButton.style.width = typography.ActionSize;
            tutorialContinueButton.style.minWidth = typography.ActionSize;
            tutorialContinueButton.style.height = typography.ActionSize;
            tutorialContinueButton.style.minHeight = typography.ActionSize;
            tutorialContinueButton.style.fontSize = typography.CloseTextSize;
        }
        if (returnMenuButton != null)
        {
            returnMenuButton.style.width = typography.MenuWidth;
            returnMenuButton.style.minWidth = typography.MenuWidth;
            returnMenuButton.style.height = typography.ActionSize;
            returnMenuButton.style.minHeight = typography.ActionSize;
            returnMenuButton.style.fontSize = typography.MenuTextSize;
        }
    }

    private void RenderTutorial()
    {
        if (tutorialContinueButton != null)
        {
            tutorialContinueButton.text = "×";
            tutorialContinueButton.tooltip = "Ocultar consejo";
        }

        switch (tutorialFlow.Stage)
        {
            case MementoTutorialStage.Introduction:
                if (tutorialStep != null) tutorialStep.text = "1 / 2";
                if (tutorialKicker != null) tutorialKicker.text = "PRIMER RELEVO";
                if (tutorialTitle != null) tutorialTitle.text = "ENCUENTRA UNA PAREJA";
                if (tutorialBody != null)
                    tutorialBody.text = "Toca dos cartas iguales. Si fallas, recuerda dónde estaban.";
                break;
            case MementoTutorialStage.ComboLesson:
                if (tutorialStep != null) tutorialStep.text = "2 / 2";
                if (tutorialKicker != null) tutorialKicker.text = "TU PASIVA";
                if (tutorialTitle != null) tutorialTitle.text = "PULSO DEL RELEVO";
                if (tutorialBody != null)
                    tutorialBody.text =
                        "Encadena parejas sin fallar. Tu combo aumenta el daño en los duelos.";
                break;
            case MementoTutorialStage.GuideLesson:
                if (gameManager == null)
                    break;
                if (tutorialStep != null) tutorialStep.text = "★";
                if (tutorialKicker != null)
                    tutorialKicker.text = gameManager.GetGuideName(gameManager.SelectedGuideId).ToUpperInvariant();
                if (tutorialTitle != null)
                    tutorialTitle.text = gameManager.PowerName.ToUpperInvariant();
                if (tutorialBody != null)
                    tutorialBody.text =
                        $"Toca su botón junto al retrato. Se recarga en {gameManager.PowerCooldownTurns} turnos tras usarlo.";
                break;
        }
    }

    private void ShowStory(MementoMatchStoryScene scene, Action onComplete)
    {
        storyVoice?.Stop();
        MementoMatchVoicePlayer.StopCurrent();
        if (scene == null || scene.Beats.Count == 0)
        {
            onComplete?.Invoke();
            return;
        }

        achievementsOpen = false;
        campaignOpen = false;
        creditsOpen = false;
        finalCelebrationOpen = false;
        guideEquipOpen = false;
        storyReplayOpen = false;
        postgameOpen = false;
        storyOpen = true;
        activeStoryScene = scene;
        narrativeRunner.Begin(scene);
        lastStoryPresentationSceneId = int.MinValue;
        lastStoryPresentationBeat = -1;
        storyPortraitContinuity.Reset();
        storyCompletion = onComplete;
        RenderStoryBeat();
        RefreshAll(true);
    }

    private void RenderStoryBeat()
    {
        if (!narrativeRunner.IsRunning)
            return;

        activeStoryScene = narrativeRunner.Scene;
        MementoTelemetry.Record("story_beat", scene: activeStoryScene.Id, beat: narrativeRunner.BeatIndex);
        ApplyStoryWorldBackground(activeStoryScene.WorldId);
        MementoMatchStoryBeat beat = narrativeRunner.CurrentBeat;
        if (storyKicker != null)
            storyKicker.text = activeStoryScene.Kicker;
        if (storyChapter != null)
            storyChapter.text = activeStoryScene.Chapter;
        string speaker = beat.Kind == MementoMatchStoryBeatKind.Protagonist
            ? playerIdentity.Name.ToUpperInvariant()
            : beat.Speaker;
        string line = (beat.Text ?? string.Empty).Replace("{PLAYER}", playerIdentity.Name);
        if (storySpeaker != null)
        {
            bool isNarration = beat.Kind == MementoMatchStoryBeatKind.Narration;
            storySpeaker.text = speaker;
            storySpeaker.style.display = isNarration
                ? DisplayStyle.None
                : DisplayStyle.Flex;
        }
        if (storyTypewriter != null)
            storyTypewriter.Play(line, 44f);
        else if (storyLine != null)
            storyLine.text = line;
        if (storyProgress != null)
            storyProgress.text =
                $"{narrativeRunner.BeatIndex + 1} / {activeStoryScene.Beats.Count}";
        if (storyNextButton != null)
            storyNextButton.text =
                narrativeRunner.BeatIndex + 1 >= activeStoryScene.Beats.Count
                    ? "CERRAR CAPÍTULO"
                    : "CONTINUAR";

        bool hasPortrait = beat.GuideId >= 0;
        bool enterPortrait = storyPortraitContinuity.ShouldEnter(beat.GuideId, beat.IsSilhouette);
        storyStage?.EnableInClassList("without-portrait", !hasPortrait);
        ApplyStoryBeatPresentation(activeStoryScene.Id, narrativeRunner.BeatIndex, beat.PresentationCue);
        storyPortrait?.EnableInClassList("silhouette", beat.IsSilhouette);
        if (storyStageChapter != null)
        {
            storyStageChapter.text = activeStoryScene.Chapter;
            storyStageChapter.style.display = hasPortrait
                ? DisplayStyle.None
                : DisplayStyle.Flex;
        }
        if (storyPortrait != null)
        {
            storyPortrait.style.display = hasPortrait
                ? DisplayStyle.Flex
                : DisplayStyle.None;
            storyPortrait.EnableInClassList("archive", false);
        }

        if (hasPortrait)
        {
            ApplyGuidePortrait(storyPortrait, beat.GuideId);
            if (enterPortrait)
                VisualNovelCharacterAnimator.Enter(
                    storyPortrait,
                    VisualNovelEntryDirection.Right);
        }
        else
        {
            VisualNovelCharacterAnimator.Reset(storyPortrait);
        }

        // Exact source text, not the player-name-interpolated display string.
        storyVoice?.Play(beat.GuideId, beat.Text);
    }

    private void ApplyStoryBeatPresentation(int sceneId, int beatIndex, MementoStoryCue cue)
    {
        if (storyStage == null)
            return;

        storyStage.EnableInClassList("intro-warning", cue == MementoStoryCue.Warning || cue == MementoStoryCue.ChampionEntrance);
        storyStage.EnableInClassList("intro-assault", cue == MementoStoryCue.OpeningDuel || cue == MementoStoryCue.Launch);
        storyStage.EnableInClassList("intro-defeat", cue == MementoStoryCue.Impact || cue == MementoStoryCue.Defeat || cue == MementoStoryCue.Aftermath);
        storyStage.EnableInClassList("intro-resolve", cue == MementoStoryCue.Resolve);

        if (lastStoryPresentationSceneId == sceneId && lastStoryPresentationBeat == beatIndex)
            return;
        lastStoryPresentationSceneId = sceneId;
        lastStoryPresentationBeat = beatIndex;

        switch (cue)
        {
            case MementoStoryCue.Warning:
                MementoMatchSfx.PlayDuelStart();
                break;
            case MementoStoryCue.ChampionEntrance:
                AudioManager.PlayDuelMusic(AnimalMemoryContentIds.UltimateMasterOpponent);
                MementoMatchSfx.PlayPower(true);
                break;
            case MementoStoryCue.OpeningDuel:
                openingMemory?.Dispose();
                openingMemory = new MementoOpeningMemoryView(document.rootVisualElement, true, () => { });
                MementoMatchSfx.PlayBoardDeal();
                break;
            case MementoStoryCue.Launch:
                MementoMatchSfx.PlayDuelLaunch(true);
                break;
            case MementoStoryCue.Impact:
                MementoMatchSfx.PlayDuelImpact(true);
                break;
            case MementoStoryCue.Defeat:
                MementoMatchSfx.PlayResult(MementoMatchResultSfx.Defeat);
                break;
            case MementoStoryCue.Resolve:
                MementoMatchSfx.PlayUiConfirm();
                break;
        }
    }

    private void ApplyStoryWorldBackground(int worldId)
    {
        if (storyBackground == null)
            return;

        storyBackground.EnableInClassList(activeStoryWorldClass, false);
        int safeWorld = Mathf.Clamp(
            worldId,
            0,
            MementoMatchCampaignRules.FinalBossWorldId);
        activeStoryWorldClass = $"world-{safeWorld}";
        storyBackground.EnableInClassList(activeStoryWorldClass, true);
    }

    private void AdvanceStory()
    {
        if (!storyOpen || !narrativeRunner.IsRunning)
            return;
        if (storyTypewriter != null && storyTypewriter.CompleteImmediately())
        {
            MementoMatchSfx.PlayUiConfirm();
            return;
        }

        storyVoice?.Stop();
        MementoMatchSfx.PlayUiConfirm();
        if (narrativeRunner.MoveNext())
        {
            RenderStoryBeat();
            return;
        }

        CompleteStory();
    }

    private void SkipStory()
    {
        MementoTelemetry.Record("story_skip", scene: activeStoryScene != null ? activeStoryScene.Id : -1, beat: narrativeRunner.BeatIndex);
        if (!storyOpen)
            return;
        MementoMatchSfx.PlayUiBack();
        CompleteStory();
    }

    private void CompleteStory()
    {
        MementoTelemetry.Record("story_end", scene: activeStoryScene != null ? activeStoryScene.Id : -1);
        openingMemory?.Dispose();
        AudioManager.RestoreDefaultMusic();
        storyVoice?.Stop();
        storyTypewriter?.Cancel();
        MementoMatchStoryScene completed = activeStoryScene;
        Action completion = storyCompletion;
        activeStoryScene = null;
        narrativeRunner.End();
        lastStoryPresentationSceneId = int.MinValue;
        lastStoryPresentationBeat = -1;
        storyPortraitContinuity.Reset();
        storyCompletion = null;
        storyVoice?.Stop();
        storyOpen = false;
        if (completed != null && completed.IsPersistent)
            gameManager?.MarkCampaignStorySeen(completed.Id);
        completion?.Invoke();
        RefreshAll(true);
    }

    private void ContinueAfterCampaignStory(MementoMatchStoryScene completedScene)
    {
        if (MementoStoryFlowRules.ShouldOfferGuideEquip(completedScene))
        {
            ShowGuideEquipChoice(
                completedScene.WorldId,
                PresentPendingCampaignStory);
            return;
        }

        PresentPendingCampaignStory();
    }

    private void ShowGuideEquipChoice(int guideId, Action onComplete)
    {
        if (gameManager == null || guideId < 0 ||
            guideId >= MementoMatchCampaignRules.WorldCount)
        {
            onComplete?.Invoke();
            return;
        }

        pendingEquipGuideId = guideId;
        guideEquipContinuation = onComplete;
        campaignOpen = false;
        storyVoice?.Stop();
        storyOpen = false;
        storyReplayOpen = false;
        creditsOpen = false;
        guideEquipOpen = true;

        ApplyGuidePortrait(guideEquipPortrait, guideId);
        VisualNovelCharacterAnimator.Enter(
            guideEquipPortrait,
            VisualNovelEntryDirection.Right);
        if (guideEquipName != null)
        {
            guideEquipName.text =
                $"{gameManager.GetGuideName(guideId).ToUpperInvariant()} " +
                "SE UNE AL EQUIPO";
        }

        if (guideEquipAbility != null)
        {
            guideEquipAbility.text =
                gameManager.GetGuideSkillDescription(guideId);
        }

        RefreshAll(true);
    }

    private void EquipPendingGuide()
    {
        if (!guideEquipOpen || gameManager == null ||
            pendingEquipGuideId < 0)
            return;

        if (!gameManager.TrySelectGuide(pendingEquipGuideId))
        {
            MementoMatchSfx.PlayUiLocked();
            return;
        }

        MementoMatchSfx.PlayUiConfirm();
        previewedGuideId = pendingEquipGuideId;
        ApplyGuidePreview(previewedGuideId);
        FinishGuideEquipChoice();
    }

    private void KeepCurrentGuide()
    {
        if (!guideEquipOpen)
            return;

        MementoMatchSfx.PlayUiBack();
        FinishGuideEquipChoice();
    }

    private void FinishGuideEquipChoice()
    {
        Action continuation = guideEquipContinuation;
        guideEquipContinuation = null;
        pendingEquipGuideId = -1;
        guideEquipOpen = false;
        VisualNovelCharacterAnimator.Reset(guideEquipPortrait);
        continuation?.Invoke();
        RefreshAll(true);
    }

    private void PresentPendingCampaignStory()
    {
        if (gameManager != null &&
            gameManager.TryGetPendingCampaignPostScene(
                out MementoMatchStoryScene scene))
        {
            ShowStory(scene, () => ContinueAfterCampaignStory(scene));
            return;
        }

        if (gameManager != null &&
            MementoStoryFlowRules.ShouldPresentFinalCelebration(
                gameManager.CampaignEndingUnlocked,
                gameManager.LastResultWasCampaign,
                gameManager.LastCampaignLevel))
            OpenFinalCelebration();
        else
            campaignOpen = true;
        RefreshAll(true);
    }

    private void OpenFinalCelebration()
    {
        if (gameManager == null || !gameManager.CampaignEndingUnlocked)
            return;

        campaignOpen = false;
        storyVoice?.Stop();
        storyOpen = false;
        creditsOpen = false;
        guideEquipOpen = false;
        storyReplayOpen = false;
        finalCelebrationOpen = true;
        for (int guideId = 0; guideId < finalTeamFaces.Length; guideId++)
            ApplyGuideFace(finalTeamFaces[guideId], guideId);

        MementoMatchSfx.PlayGuideUnlock();
        RefreshAll(true);
    }

    private void EquipArchiveAndOpenCredits()
    {
        if (gameManager == null ||
            !gameManager.TrySelectSet(AnimalMemoryContentIds.ClockworkSet))
        {
            MementoMatchSfx.PlayUiLocked();
            return;
        }

        ApplySelectedSet(AnimalMemoryContentIds.ClockworkSet);
        OpenCredits();
    }

    private void OpenCredits()
    {
        if (gameManager == null || !gameManager.CampaignEndingUnlocked)
            return;
        MementoMatchSfx.PlayUiConfirm();
        campaignOpen = false;
        storyVoice?.Stop();
        storyOpen = false;
        finalCelebrationOpen = false;
        creditsOpen = true;
        RefreshAll(true);
    }

    private void CloseCredits()
    {
        MementoMatchSfx.PlayUiBack();
        creditsOpen = false;
        campaignOpen = true;
        RefreshCampaign();
        RefreshAll(true);
    }

    private void RefreshAllianceBar(bool visible)
    {
        if (allianceBar == null && powerCluster != null)
        {
            allianceBar = new VisualElement { name = "alliance-power-bar" };
            allianceBar.style.flexDirection = FlexDirection.Row;
            allianceBar.style.flexGrow = 1;
            allianceBar.style.minWidth = 0;
            allianceBar.style.height = 64;
            powerCluster.Add(allianceBar);
            for (int id = 1; id <= 4; id++)
            {
                int owner = id;
                var button = new Button(() => {
                    if (gameManager.TryUseAlliancePower(owner))
                        RefreshHud();
                });
                button.style.flexGrow = 1; button.style.flexBasis = 0;
                button.style.minWidth = 0;
                button.style.marginLeft = button.style.marginRight = 2;
                button.style.paddingTop = button.style.paddingBottom = 2;
                button.style.borderTopLeftRadius = button.style.borderTopRightRadius = 14;
                button.style.borderBottomLeftRadius = button.style.borderBottomRightRadius = 14;
                button.tooltip = gameManager.GetGuideName(id);
                var face = new VisualElement { pickingMode = PickingMode.Ignore };
                face.style.width = 32; face.style.height = 32;
                face.style.alignSelf = Align.Center;
                face.style.borderTopLeftRadius = face.style.borderTopRightRadius = 16;
                face.style.borderBottomLeftRadius = face.style.borderBottomRightRadius = 16;
                ApplyGuideFace(face, id);
                button.Add(face);
                var label = new Label { pickingMode = PickingMode.Ignore };
                label.style.fontSize = 10;
                label.style.unityTextAlign = TextAnchor.MiddleCenter;
                button.Add(label);
                allianceButtons[id] = button;
                allianceCooldownLabels[id] = label;
                allianceBar.Add(button);
            }
        }
        SetDisplay(allianceBar, visible);
        if (!visible || allianceBar == null) return;
        for (int id = 1; id <= 4; id++)
        {
            bool present = (gameManager.AllianceMask & (1 << id)) != 0;
            SetDisplay(allianceButtons[id], present);
            bool ready = gameManager.IsAlliancePowerReady(id);
            allianceButtons[id].SetEnabled(ready);
            int cooldown = gameManager.GetAllianceCooldown(id);
            allianceCooldownLabels[id].text = cooldown > 0
                ? cooldown + " TURNOS" : ready ? "LISTO" : "ESPERA";
        }
    }

    private void RefreshHud()
    {
        bool hasGuide = gameManager.EquippedRunGuideId >= 0;
        bool alliance = gameManager.IsAllianceActive;
        SetDisplay(powerCluster, true);
        SetDisplay(powerGuideFace, hasGuide && !alliance);
        SetDisplay(powerButton, hasGuide && !alliance);
        RefreshAllianceBar(alliance);
        if (hasGuide)
            ApplyGuideFace(powerGuideFace, gameManager.EquippedRunGuideId);
        if (hudScore != null) hudScore.text = gameManager.CurrentScore.ToString();
        if (hudTurns != null) hudTurns.text = gameManager.CurrentTurns.ToString();
        if (hudMatches != null)
            hudMatches.text = gameManager.IsChallengeActive
                ? $"{gameManager.DuelPlayerPairs} / {gameManager.DuelTargetPairs}"
                : $"{gameManager.CurrentMatches} / {gameManager.CurrentPairCount}";

        if (hudMode != null)
        {
            hudMode.text = gameManager.IsPostgameActive
                ? $"POSTGAME · {MementoPostgameEncounters.Get(gameManager.ActivePostgameEncounter).Title.ToUpperInvariant()}"
                : gameManager.IsCampaignActive
                ? $"CAMPAÑA {gameManager.ActiveCampaignLevel + 1}/{MementoMatchCampaignRules.LevelCount} · " +
                  MementoMatchCampaignRules.GetLevel(gameManager.ActiveCampaignLevel).Title.ToUpperInvariant()
                : gameManager.IsChallengeActive
                    ? $"DUELO · {gameManager.GetGuideName(gameManager.ChallengeGuideId).ToUpperInvariant()}"
                    : gameManager.SelectedGuideId >= 0
                        ? $"PARTIDA LIBRE · {gameManager.GetGuideName(gameManager.SelectedGuideId).ToUpperInvariant()}"
                        : "PARTIDA LIBRE · SIN GUÍA · PULSO DEL RELEVO";
        }

        if (powerButton != null && hasGuide)
        {
            bool blocked = gameManager.IsGuidePowerBlocked;
            bool ready = gameManager.IsPowerReady;
            powerButton.text = blocked
                ? $"{gameManager.PowerName.ToUpperInvariant()}\nGUÍA RIVAL · NO DISPONIBLE"
                : ready
                    ? $"{gameManager.PowerName.ToUpperInvariant()}\nLISTO"
                    : $"{gameManager.PowerName.ToUpperInvariant()}\n{gameManager.PowerCooldownRemaining} TURNOS";
            powerButton.EnableInClassList("cooldown", !ready);
            powerButton.EnableInClassList("mirror-blocked", blocked);
            powerButton.SetEnabled(ready);
        }

        bool challenge = gameManager.IsChallengeActive;
        gameplayHud?.EnableInClassList("duel-active", challenge);
        SetDisplay(rivalPanel, challenge);
        if (challenge)
        {
            string rival = gameManager.GetGuideName(gameManager.ChallengeGuideId).ToUpperInvariant();
            string player = gameManager.ActivePlayerGuideId >= 0
                ? gameManager.GetGuideName(gameManager.ActivePlayerGuideId).ToUpperInvariant() : "TÚ";
            string turn = gameManager.IsPlayerTurn
                ? (player == "TÚ" ? "TU TURNO" : $"TURNO DE {player}") : $"TURNO DE {rival}";
            if (rivalName != null) rivalName.text = "RIVAL";
            if (rivalStatus != null)
                rivalStatus.text = turn;

            int target = gameManager.DuelTargetPairs;
            int playerHealth = gameManager.DuelPlayerHealth;
            int rivalHealth = gameManager.DuelOpponentHealth;
            if (playerHealthLabel != null)
                playerHealthLabel.text = $"{player} · {playerHealth}/{target}";
            if (rivalHealthLabel != null)
                rivalHealthLabel.text = $"{rival} · {rivalHealth}/{target}";

            playerHealthCard?.EnableInClassList("active-turn", gameManager.IsPlayerTurn);
            rivalHealthCard?.EnableInClassList("active-turn", !gameManager.IsPlayerTurn);
            SetHealthWidth(playerHealthFill, playerHealth, target);
            SetHealthWidth(rivalFill, rivalHealth, target);
        }
    }

    private static void SetHealthWidth(
        VisualElement fill,
        int currentHealth,
        int maximumHealth)
    {
        if (fill == null)
            return;

        float percent = maximumHealth > 0
            ? 100f * currentHealth / maximumHealth
            : 0f;
        fill.style.width = new Length(
            Mathf.Clamp(percent, 0f, 100f),
            LengthUnit.Percent);
    }

    private void RequestRewardedContinue()
    {
        gameManager?.RewardedContinue?.Request();
        RefreshResult();
    }

    private void RefreshResult()
    {
        float scale = Mathf.Clamp(Mathf.Min(document.rootVisualElement.resolvedStyle.width,
            document.rootVisualElement.resolvedStyle.height) / 600f, 1f, 1.8f);
        var panel = resultOverlay?.Q<VisualElement>(className: "result-panel");
        if (panel != null) panel.style.width = 590f * scale;
        if (resultTitle != null) resultTitle.style.fontSize = 28f * scale;
        if (resultSummary != null) resultSummary.style.fontSize = 15f * scale;
        foreach (var button in new[] { resultRetryButton, resultMenuButton, resultContinueAdButton })
            if (button != null) { button.style.height = 52f * scale; button.style.fontSize = 13f * scale; }
        if (resultContinueStatus != null) resultContinueStatus.style.fontSize = 12f * scale;
        if (resultStars != null) SetDisplay(resultStars.parent, gameManager.LastResultWasVictory);
        SetDisplay(resultAchievements, gameManager.LastResultWasVictory && gameManager.LastUnlockedAchievements.Count > 0);
        var rewarded = gameManager.RewardedContinue;
        bool pendingAd = rewarded != null && rewarded.IsPending;
        bool showOffer = !gameManager.LastResultWasVictory && rewarded != null &&
            (rewarded.CanOffer || pendingAd || !string.IsNullOrEmpty(rewarded.Status));
        SetDisplay(resultContinueOffer, showOffer);
        if (resultContinueAdButton != null)
        {
            resultContinueAdButton.SetEnabled(showOffer && rewarded.CanOffer && !pendingAd);
            resultContinueAdButton.text = pendingAd ? "ESPERANDO AL ANUNCIO…" : "VER ANUNCIO Y CONTINUAR";
        }
        if (resultContinueStatus != null && showOffer)
            resultContinueStatus.text = string.IsNullOrEmpty(rewarded.Status)
                ? "Una vez por intento · recupera media vida y conserva tu avance."
                : rewarded.Status;
        // Retry/menu stay available if the provider hangs. Leaving invalidates the reward.

        if (resultKicker != null)
            resultKicker.text = gameManager.LastResultWasCampaign
                ? gameManager.LastResultWasVictory
                    ? "CAMPAÑA · NIVEL SUPERADO"
                    : "CAMPAÑA · INTÉNTALO DE NUEVO"
                : gameManager.LastResultWasVictory
                    ? "VICTORIA"
                    : "DUELO PERDIDO";
        if (resultTitle != null) resultTitle.text = gameManager.LastResultTitle;
        if (resultSummary != null) resultSummary.text = gameManager.LastResultSummary;
        if (resultStars != null)
            resultStars.text = gameManager.LastResultWasCampaign
                ? $"RANGO {CampaignStars(gameManager.LastRewardStars)}"
                : $"+{gameManager.LastRewardStars} ★";

        if (resultAchievements != null)
        {
            if (gameManager.LastUnlockedAchievements.Count == 0)
            {
                resultAchievements.text = "Sigue jugando para revelar nuevos logros.";
            }
            else
            {
                resultAchievements.text = "NUEVO LOGRO · " + string.Join(
                    "  ·  ",
                    gameManager.LastUnlockedAchievements
                        .Select(gameManager.GetAchievementName)
                        .ToArray());
            }
        }

        if (resultRetryButton != null)
            resultRetryButton.text = gameManager.LastResultWasCampaign
                ? gameManager.LastResultWasVictory
                    ? "REPETIR NIVEL"
                    : "REINTENTAR NIVEL"
                : gameManager.LastResultWasVictory
                    ? "JUGAR DE NUEVO"
                    : "REINTENTAR DUELO";
        if (resultMenuButton != null)
            resultMenuButton.text = gameManager.LastResultWasCampaign
                ? gameManager.LastResultWasVictory ? "CONTINUAR HISTORIA" : "VOLVER A NIVELES"
                : "VOLVER AL MENÚ";
    }

    private void RefreshSetButton(Button button, int id, string title)
    {
        if (button == null)
            return;

        bool unlocked = gameManager.IsSetUnlocked(id);
        bool selected = gameManager.SelectedSetId == id;
        button.EnableInClassList("locked", !unlocked);
        button.EnableInClassList("selected", selected);
        if (id == AnimalMemoryContentIds.ClockworkSet && !unlocked)
        {
            button.text = $"{title}\nHISTORIA · VENCE A REI";
            return;
        }

        button.text = selected
            ? $"{title}\n✓ BARAJA ACTIVA"
            : unlocked
                ? $"{title}\nSELECCIONAR BARAJA"
                : $"{title}\nHISTORIA · COMPLETA MUNDO {id + 1}";
    }

    private void RefreshGuideButton(Button button, int id, string title)
    {
        if (button == null)
            return;

        bool unlocked = gameManager.IsGuideUnlocked(id);
        bool selected = gameManager.SelectedGuideId == id;
        button.EnableInClassList("locked", !unlocked);
        button.EnableInClassList("selected", selected);
        button.text = selected
            ? $"{title}\n✓ PODER EQUIPADO"
            : unlocked
                ? $"{title}\nEQUIPAR PODER"
                : $"{title}\nHISTORIA · COMPLETA MUNDO {id + 1}";
    }

    private void StartEasy()
    {
        if (RequireOpeningPractice(StartEasy)) return;
        MementoMatchSfx.PlayUiConfirm();
        gameManager?.StartEasyGame();
    }

    private void StartNormal()
    {
        if (RequireOpeningPractice(StartNormal)) return;
        MementoMatchSfx.PlayUiConfirm();
        gameManager?.StartNormalGame();
    }

    private void StartHard()
    {
        if (RequireOpeningPractice(StartHard)) return;
        MementoMatchSfx.PlayUiConfirm();
        gameManager?.StartHardGame();
    }

    private void SelectForest() => TrySelectThemeOrPreview(0);
    private void SelectCoral() => TrySelectThemeOrPreview(1);
    private void SelectNight() => TrySelectThemeOrPreview(2);
    private void SelectGarden() => TrySelectThemeOrPreview(3);
    private void SelectSweets() => TrySelectThemeOrPreview(4);
    private void SelectClockwork() => TrySelectThemeOrPreview(5);
    private void SelectAki() => SelectOrPreviewGuide(0);
    private void SelectMika() => SelectOrPreviewGuide(1);
    private void SelectYoru() => SelectOrPreviewGuide(2);
    private void SelectHana() => SelectOrPreviewGuide(3);
    private void SelectMomo() => SelectOrPreviewGuide(4);

    private void TrySelectThemeOrPreview(int id)
    {
        if (gameManager == null)
            return;

        bool selected = gameManager.TrySelectSet(id);
        if (selected)
        {
            MementoMatchSfx.PlayUiConfirm();
            ApplySelectedSet(id);
        }
        else
        {
            MementoMatchSfx.PlayUiLocked();
        }

        if (id < AnimalMemoryContentIds.GuideCount)
        {
            previewedGuideId = id;
            ApplyGuidePreview(id);
            PlayGuideVoice(id, MementoMenuVoiceContext.CollectionSetSelection);
        }
        else
        {
            SetGuideLine(selected
                ? "El Archivo de Relojes ya está listo. Cada engranaje guarda una pareja."
                : "Derrota a Rei en el duelo final de la historia para desbloquear esta baraja.");
        }
        RefreshMenu();
    }

    private void SelectOrPreviewGuide(int id)
    {
        previewedGuideId = id;
        bool unlocked = gameManager != null && gameManager.IsGuideUnlocked(id);
        if (unlocked)
        {
            MementoMatchSfx.PlayUiConfirm();
            gameManager.TrySelectGuide(id);
            if (gameManager.IsSetUnlocked(id))
            {
                gameManager.TrySelectSet(id);
                ApplySelectedSet(id);
            }
        }
        else
        {
            MementoMatchSfx.PlayUiLocked();
        }

        ApplyGuidePreview(id);
        PlayGuideVoice(id, MementoMenuVoiceContext.GuideSelection);
        RefreshMenu();
    }

    private static void PlayGuideVoice(
        int guideId,
        MementoMenuVoiceContext context)
    {
        if (MementoMenuVoicePolicy.ShouldPlayGuideVoice(context))
            MementoMatchVoicePlayer.PlayMenu(guideId);
    }

    private void ApplySelectedSet(int index)
    {
        forestThemeButton?.EnableInClassList("selected", index == 0);
        coralThemeButton?.EnableInClassList("selected", index == 1);
        nightThemeButton?.EnableInClassList("selected", index == 2);
        gardenThemeButton?.EnableInClassList("selected", index == 3);
        sweetsThemeButton?.EnableInClassList("selected", index == 4);
        clockworkThemeButton?.EnableInClassList("selected", index == 5);

        if (themeDescription != null)
        {
            switch (index)
            {
                case 1: themeDescription.text = "Mika · Taller Coral · reliquias marinas"; break;
                case 2: themeDescription.text = "Yoru · Cielo de Tinta · constelaciones vivas"; break;
                case 3: themeDescription.text = "Hana · Jardín Secreto · flores y hojas"; break;
                case 4: themeDescription.text = "Momo · Dulce Atelier · repostería mágica"; break;
                case 5: themeDescription.text = "Archivo de Relojes · engranajes y mecanismos"; break;
                default: themeDescription.text = "Aki · Bosque Vivo · criaturas y hojas doradas"; break;
            }
        }

        cardMatchUI?.SetTheme(index);
    }

    private void ApplyGuidePreview(int id)
    {
        previewedGuideId = Mathf.Clamp(id, 0, AnimalMemoryContentIds.GuideCount - 1);
        akiGuideButton?.EnableInClassList("selected",
            gameManager != null && gameManager.SelectedGuideId == 0);
        mikaGuideButton?.EnableInClassList("selected",
            gameManager != null && gameManager.SelectedGuideId == 1);
        yoruGuideButton?.EnableInClassList("selected",
            gameManager != null && gameManager.SelectedGuideId == 2);
        hanaGuideButton?.EnableInClassList("selected",
            gameManager != null && gameManager.SelectedGuideId == 3);
        momoGuideButton?.EnableInClassList("selected",
            gameManager != null && gameManager.SelectedGuideId == 4);

        if (guideName != null)
        {
            guideName.text = id == 1
                ? "MIKA · INVENTORA"
                : id == 2
                    ? "YORU · ASTRÓNOMA"
                    : id == 3
                        ? "HANA · JARDINERA"
                        : id == 4
                            ? "MOMO · PASTELERA"
                            : "AKI · EXPLORADORA";
        }

        if (guideAbility != null)
        {
            // Share the canonical explanation with the recruitment/equip panel.
            // Separate abbreviated strings previously changed the meaning of Encore.
            guideAbility.text = gameManager != null
                ? gameManager.GetGuideSkillDescription(id)
                : string.Empty;
        }

        SetGuideLine(GetMenuLine(id));
        LoadGuidePortrait(id);
        ApplyGuideFace(powerGuideFace, id);
        RefreshChallengeButton();
    }

    private void RefreshChallengeButton()
    {
        // Guardian access belongs to the campaign map. Keeping this button
        // hidden prevents the team screen from bypassing recruitment order.
        SetDisplay(challengeButton, false);
    }

    private void StartPreviewedChallenge()
    {
        if (gameManager != null &&
            gameManager.StartGuideChallenge(previewedGuideId))
        {
            MementoMatchSfx.PlayUiConfirm();
            return;
        }

        MementoMatchSfx.PlayUiLocked();
        SetGuideLine(GetLockedLine(previewedGuideId));
        MementoMatchVoicePlayer.PlayLocked(previewedGuideId);
    }

    private void UsePower()
    {
        if (gameManager != null && gameManager.TryUseGuidePower())
        {
            // Successful use also completes this independent lesson, even if
            // the player never dismissed its non-blocking hint.
            gameManager.CompleteGuideTutorial();
            if (tutorialFlow.Stage == MementoTutorialStage.GuideLesson)
                tutorialFlow.Reset();
            // Presentation is emitted by GameManager so deferred powers can
            // distinguish CAST from their later PROC.
        }
        else
        {
            MementoMatchSfx.PlayUiLocked();
        }

        RefreshHud();
    }

    private void Retry()
    {
        MementoMatchSfx.PlayUiConfirm();
        if (gameManager != null && gameManager.LastResultWasCampaign &&
            !gameManager.LastResultWasVictory && gameManager.LastCampaignLevel >= 0)
        {
            ShowStory(
                MementoMatchCampaignRules.GetRetryScene(gameManager.LastCampaignLevel),
                StartSameDifficultyAfterStory);
            return;
        }

        gameManager?.StartSameDifficultyGame();
        RefreshAll(true);
    }

    private void StartSameDifficultyAfterStory()
    {
        gameManager?.StartSameDifficultyGame();
        RefreshAll(true);
    }

    private void OpenAchievements()
    {
        MementoMatchSfx.PlayUiConfirm();
        achievementsOpen = true;
        RefreshAchievementStates();
        RefreshAll(true);
    }

    private void CloseAchievements()
    {
        MementoMatchSfx.PlayUiBack();
        achievementsOpen = false;
        RefreshAll(true);
    }

    private void BuildAchievementList()
    {
        if (achievementList == null || gameManager == null)
            return;

        achievementList.Clear();
        for (int id = 0; id < MementoMatchAchievementIds.Count; id++)
        {
            VisualElement item = new VisualElement();
            item.name = $"achievement-{id}";
            item.AddToClassList("achievement-item");

            Label name = new Label(gameManager.GetAchievementName(id));
            name.AddToClassList("achievement-name");
            Label description = new Label(gameManager.GetAchievementDescription(id));
            description.AddToClassList("achievement-description");
            Label state = new Label();
            state.name = $"achievement-state-{id}";
            state.AddToClassList("achievement-state");

            item.Add(name);
            item.Add(description);
            item.Add(state);
            achievementList.Add(item);
        }

        RefreshAchievementStates();
    }

    private void RefreshAchievementStates()
    {
        if (achievementList == null || gameManager == null)
            return;

        for (int id = 0; id < MementoMatchAchievementIds.Count; id++)
        {
            bool unlocked = gameManager.IsAchievementUnlocked(id);
            VisualElement item = achievementList.Q<VisualElement>($"achievement-{id}");
            item?.EnableInClassList("unlocked", unlocked);
            Label state = achievementList.Q<Label>($"achievement-state-{id}");
            if (state != null)
                state.text = unlocked ? "DESBLOQUEADO" : "BLOQUEADO";
        }
    }

    private void OnGuidePowerPresentationRequested(int guideId, bool resolved)
    {
        if (!resolved)
        {
            MementoMatchSfx.PlayPower(guideId, false);
            ShowSkillCutIn(guideId, false);
            return;
        }

        ShowSkillProcToast(guideId);
    }

    private void ShowSkillProcToast(int guideId)
    {
        if (skillProcToast == null)
            return;

        GetSkillPresentation(guideId, false, out string skillName, out _);
        if (skillProcName != null)
            skillProcName.text = skillName;
        if (skillProcEffect != null)
        {
            skillProcEffect.text = guideId == AnimalMemoryContentIds.MikaGuide
                ? "ESCUDO CONSUMIDO · COMBO PROTEGIDO"
                : guideId == AnimalMemoryContentIds.MomoGuide
                    ? "ENCORE RESUELTO · CONSERVAS EL TURNO"
                    : "EFECTO DIFERIDO RESUELTO";
        }

        ApplyGuideFace(skillProcFace, guideId);
        skillProcToast.RemoveFromClassList("active");
        SetDisplay(skillProcToast, true);
        skillProcToast.schedule.Execute(
            () => skillProcToast.AddToClassList("active")).StartingIn(16);
        skillProcToast.schedule.Execute(
            () => skillProcToast.RemoveFromClassList("active")).StartingIn(2350);
        skillProcToast.schedule.Execute(
            () => SetDisplay(skillProcToast, false)).StartingIn(2700);
    }

    private void OnOpponentDuelSkillActivated(int guideId)
    {
        if (guideId == AnimalMemoryContentIds.UltimateMasterOpponent || MementoPostgameEncounters.IsPostgameOpponent(guideId))
        {
            int techniqueGuide = cardMatchUI != null
                ? cardMatchUI.DuelOpponentTechniqueGuideId
                : -1;
            MementoMatchSfx.PlayPower(true);
            ShowSkillCutIn(guideId, true, techniqueGuide);
            return;
        }

        MementoMatchSfx.PlayPower(guideId, true);
        ShowSkillCutIn(guideId, true);
    }

    private void ShowSkillCutIn(
        int guideId,
        bool hostile,
        int techniqueGuideId = -1)
    {
        if (skillCutInOverlay == null)
            return;

        int presentationTechnique = techniqueGuideId >= 0
            ? techniqueGuideId
            : guideId;
        GetSkillPresentation(
            presentationTechnique,
            hostile,
            out string skillName,
            out string quote);
        bool isUltimateMaster =
            guideId == AnimalMemoryContentIds.UltimateMasterOpponent;
        if (isUltimateMaster)
        {
            string source = gameManager != null && techniqueGuideId >= 0
                ? gameManager.GetGuideName(techniqueGuideId).ToUpperInvariant()
                : "GUARDIANA";
            skillName = $"COPIA DE {source} · {skillName}";
            quote = "El Archivo la recuerda. Yo decido cómo usarla.";
            if (skillCutInKicker != null)
                skillCutInKicker.text = "ACTIVACIÓN DE HABILIDAD · REI";
        }
        else if (skillCutInKicker != null)
        {
            string guide = gameManager != null
                ? gameManager.GetGuideName(guideId).ToUpperInvariant()
                : "GUÍA";
            skillCutInKicker.text =
                $"ACTIVACIÓN DE HABILIDAD · {guide}";
        }

        if (MementoPostgameEncounters.IsPostgameOpponent(guideId))
            quote = "La técnica entra en juego.";

        if (skillCutInName != null)
            skillCutInName.text = skillName;
        if (skillCutInQuote != null)
            skillCutInQuote.text = quote;

        // Technique logic may be copied, but the audiovisual identity is always
        // that of the opponent who is actually on screen.
        ApplyGuideFace(skillCutInFace, guideId);
        if (skillCinematic == null)
        {
            skillCinematic = new MementoSkillCinematic();
            skillCutInOverlay.Insert(0, skillCinematic);
            skillCutInEffect = new Label();
            skillCutInEffect.name = "skill-cut-in-effect";
            skillCutInEffect.AddToClassList("skill-cut-in-effect");
            skillCutInQuote?.parent.Add(skillCutInEffect);
        }
        skillCutInEffect.text = MementoSkillPresentationRules.Effect(presentationTechnique, hostile, isUltimateMaster);
        skillCutInOverlay.EnableInClassList("hostile", hostile);
        skillCutInOverlay.RemoveFromClassList("active");
        skillCutInOverlay.RemoveFromClassList("leaving");
        SetDisplay(skillCutInOverlay, true);
        SetDisplay(voiceSubtitleBox, false);
        int version = ++skillPresentationVersion;
        float voiceSeconds = MementoMatchVoicePlayer.PlaySkill(guideId);
        float duration = MementoSkillPresentationRules.Duration(voiceSeconds,
            (skillCutInQuote != null ? skillCutInQuote.text : quote) + " " + skillCutInEffect.text);
        // Rei copies mechanics, never another character\'s pose or palette.
        Sprite actionPose = MementoIllustratedCharacters.GetSprite(guideId, "ability");
        skillCinematic.Play(actionPose, guideId, duration);
        cardMatchUI?.HoldForSkillPresentation(duration + 0.1f);
        skillCutInOverlay.schedule.Execute(() =>
        {
            if (version == skillPresentationVersion) skillCutInOverlay.AddToClassList("leaving");
        }).StartingIn(Mathf.RoundToInt((duration - .25f) * 1000f));
        skillCutInOverlay.schedule.Execute(() =>
        {
            if (version == skillPresentationVersion) skillCutInOverlay.AddToClassList("active");
        }).StartingIn(16);
        skillCutInOverlay.schedule.Execute(() =>
        {
            if (version != skillPresentationVersion) return;
            HideSkillCutIn();
        }).StartingIn(Mathf.RoundToInt(duration * 1000f));
    }

    private void OnSkipSkillClick(ClickEvent evt)
    {
        evt.StopImmediatePropagation();
        SkipSkillCutIn();
    }

    private void SkipSkillCutIn()
    {
        if (skillCutInOverlay == null ||
            skillCutInOverlay.resolvedStyle.display == DisplayStyle.None) return;
        cardMatchUI?.SkipSkillPresentation();
        MementoMatchVoicePlayer.StopCurrent();
        HideSkillCutIn();
    }

    private void HideSkillCutIn()
    {
        skillPresentationVersion++;
        skillCinematic?.Stop();
        SetDisplay(skillCutInOverlay, false);
    }

    private static void GetSkillPresentation(
        int guideId,
        bool hostile,
        out string skillName,
        out string quote)
    {
        if (guideId == AnimalMemoryContentIds.MikaGuide)
        {
            skillName = hostile ? "CÁLCULO CORAL" : "ESCUDO DE COMBO";
            quote = "Tal como calculé. Ya puedo ver el siguiente movimiento.";
            return;
        }

        if (guideId == AnimalMemoryContentIds.YoruGuide)
        {
            skillName = hostile ? "LECTURA ASTRAL" : "VISIÓN ESTELAR";
            quote = "Estrellas, iluminad el recuerdo oculto.";
            return;
        }

        if (guideId == AnimalMemoryContentIds.HanaGuide)
        {
            skillName = hostile ? "JARDÍN INVASOR" : "FLORACIÓN";
            quote = "¡Florece, recuerdo oculto!";
            return;
        }

        if (guideId == AnimalMemoryContentIds.MomoGuide)
        {
            skillName = hostile ? "RECETA ENCORE" : "ENCORE DULCE";
            quote = "Una pareja más. ¡Mi turno continúa!";
            return;
        }

        skillName = hostile ? "REBOBINAR HUELLA" : "DESHACER";
        quote = "Todavía no ha terminado. ¡Recuérdalo una vez más!";
    }

    private void OnDuelOutcomePresentationRequested(
        int guideId,
        bool playerWon)
    {
        outcomePresentationPending = true;
        outcomePresentationVersion++;
        int presentationVersion = outcomePresentationVersion;
        string guide = gameManager != null
            ? gameManager.GetGuideName(guideId).ToUpperInvariant()
            : "RIVAL";
        if (duelDialogueName != null)
            duelDialogueName.text = $"{guide} · ÚLTIMA PALABRA";
        if (duelDialogueLine != null)
            duelDialogueLine.text = GetOutcomeLine(guideId, playerWon);

        ApplyGuideFace(duelDialogueFace, guideId);
        duelDialogueOverlay?.EnableInClassList("defeat", !playerWon);
        SetDisplay(duelDialogueOverlay, true);
        bool canSkipVoice = MementoDefeatPresentation.HasHeard(guideId, playerWon);
        MementoDefeatPresentation.Remember(guideId, playerWon);
        float voiceDuration = MementoMatchVoicePlayer.PlayOutcome(guideId, playerWon);
        if (duelDialogueContinueButton != null)
        {
            duelDialogueContinueButton.text = canSkipVoice ? "CONTINUAR" : "ESCUCHAR…";
            duelDialogueContinueButton.SetEnabled(canSkipVoice);
        }
        int enableDelay = Mathf.RoundToInt(Mathf.Max(0.65f, voiceDuration) * 1000f);
        document.rootVisualElement.schedule.Execute(() =>
        {
            if (presentationVersion != outcomePresentationVersion)
                return;
            if (duelDialogueContinueButton != null)
            {
                duelDialogueContinueButton.text = "CONTINUAR";
                duelDialogueContinueButton.SetEnabled(true);
            }
        }).StartingIn(enableDelay);
        document.rootVisualElement.schedule.Execute(() =>
        {
            if (presentationVersion != outcomePresentationVersion)
                return;
            outcomePresentationPending = false;
            SetDisplay(duelDialogueOverlay, false);
            RefreshAll(true);
        }).StartingIn(12200);
        RefreshAll(true);
    }

    private void ContinueDuelOutcome()
    {
        if (!outcomePresentationPending)
            return;
        MementoMatchSfx.PlayUiConfirm();
        outcomePresentationPending = false;
        outcomePresentationVersion++;
        MementoMatchVoicePlayer.StopCurrent();
        SetDisplay(duelDialogueOverlay, false);
        gameManager?.ContinueDuelOutcomePresentation();
        RefreshAll(true);
    }

    private static string GetOutcomeLine(int guideId, bool playerWon)
    {
        return MementoMatchVoicePlayer.GetOutcomeSubtitle(
            guideId,
            playerWon);
    }

    private void OnPlayerDuelPairMade(
        RectTransform firstCard,
        RectTransform secondCard)
    {
        if (gameManager == null ||
            !gameManager.IsChallengeActive ||
            cardMatchUI == null)
        {
            return;
        }

        RectTransform target = cardMatchUI.DuelOpponentAttackTarget;
        if (target == null)
            return;

        Canvas canvas = target.GetComponentInParent<Canvas>();
        Camera camera = canvas != null &&
                        canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera
            : null;
        Vector2 targetScreenPoint =
            RectTransformUtility.WorldToScreenPoint(camera, target.position);
        AnimalMemoryJuice.PlayDuelAttack(
            firstCard,
            secondCard,
            targetScreenPoint,
            false,
            OnRivalImpact);
    }

    private void OnOpponentDuelPairMade(
        RectTransform firstCard,
        RectTransform secondCard)
    {
        if (gameManager == null ||
            !gameManager.IsChallengeActive ||
            !TryGetToolkitScreenPoint(playerHealthTrack, out Vector2 targetScreenPoint))
        {
            return;
        }

        AnimalMemoryJuice.PlayDuelAttack(
            firstCard,
            secondCard,
            targetScreenPoint,
            true,
            OnPlayerHealthImpact);
    }

    private bool TryGetToolkitScreenPoint(
        VisualElement element,
        out Vector2 screenPoint)
    {
        screenPoint = Vector2.zero;
        if (element == null || document == null)
            return false;

        Rect panelBounds = document.rootVisualElement.worldBound;
        Rect targetBounds = element.worldBound;
        if (panelBounds.width <= 0f ||
            panelBounds.height <= 0f ||
            targetBounds.width <= 0f ||
            targetBounds.height <= 0f)
        {
            return false;
        }

        float normalizedX =
            (targetBounds.center.x - panelBounds.xMin) / panelBounds.width;
        float normalizedY =
            (targetBounds.center.y - panelBounds.yMin) / panelBounds.height;
        screenPoint = new Vector2(
            normalizedX * Screen.width,
            (1f - normalizedY) * Screen.height);
        return true;
    }

    private static void FlashHealthCard(VisualElement card)
    {
        if (card == null)
            return;

        card.AddToClassList("damaged");
        card.schedule.Execute(
            () => card.RemoveFromClassList("damaged")).StartingIn(320);
    }

    private void OnPlayerHealthImpact()
    {
        if (gameManager == null || !gameManager.IsChallengeActive)
            return;

        FlashHealthCard(playerHealthCard);
    }

    private void OnRivalImpact()
    {
        if (gameManager == null || !gameManager.IsChallengeActive)
            return;

        FlashHealthCard(rivalHealthCard);
        MementoMatchVoicePlayer.PlayDamage(gameManager.ChallengeGuideId);
        cardMatchUI?.PlayDuelOpponentImpact();
    }

    private void LoadGuidePortrait(int guideId)
    {
        if (hostessImage == null)
            return;

        Sprite illustrated = MementoIllustratedCharacters.GetSprite(guideId, "neutral");
        if (illustrated != null)
        {
            guideTextures[guideId] = illustrated.texture;
            hostessImage.style.backgroundImage =
                new StyleBackground(Background.FromTexture2D(illustrated.texture));
            return;
        }

        if (MementoPostgameEncounters.IsPostgameOpponent(guideId)) return;

        if (!guideTextures.TryGetValue(guideId, out Texture2D texture) || texture == null)
        {
            string guideKey = guideId == AnimalMemoryContentIds.UltimateMasterOpponent
                ? "rei"
                : guideId == AnimalMemoryContentIds.MikaGuide
                    ? "mika"
                : guideId == AnimalMemoryContentIds.YoruGuide
                    ? "yoru"
                    : guideId == AnimalMemoryContentIds.HanaGuide
                        ? "hana"
                        : guideId == AnimalMemoryContentIds.MomoGuide
                            ? "momo"
                            : "aki";

            if (guideId >= AnimalMemoryContentIds.HanaGuide)
            {
                string resourceRoot = guideId == AnimalMemoryContentIds.UltimateMasterOpponent
                    ? "AnimalMemory/Duel/UltimateMaster/"
                    : "AnimalMemory/Duel/NewGuides/";
                texture = Resources.Load<Texture2D>(
                    $"{resourceRoot}{guideKey}_neutral");
                if (texture == null)
                {
                    Debug.LogWarning($"Memento Match portrait not found for {guideKey}.");
                    return;
                }

                guideTextures[guideId] = texture;
            }
            else
            {
                TextAsset[] chunks = Resources.LoadAll<TextAsset>(
                    $"AnimalMemory/Guides/{guideKey}");
                if (chunks == null || chunks.Length == 0)
                {
                    Debug.LogWarning(
                        $"Memento Match portrait chunks not found for {guideKey}.");
                    return;
                }

                string base64 = string.Concat(
                    chunks.OrderBy(chunk => chunk.name).Select(chunk => chunk.text));
                try
                {
                    byte[] pngBytes = Convert.FromBase64String(base64);
                    texture = new Texture2D(2, 2, TextureFormat.RGBA32, false)
                    {
                        name = $"Memento Match Guide {guideKey}"
                    };
                    if (!ImageConversion.LoadImage(texture, pngBytes, false))
                    {
                        Destroy(texture);
                        return;
                    }

                    bool removeOuterPaleArtifacts =
                        guideId == AnimalMemoryContentIds.MikaGuide ||
                        guideId == AnimalMemoryContentIds.YoruGuide;
                    RemoveGeneratedBackdrop(
                        texture,
                        removeOuterPaleArtifacts);
                    guideTextures[guideId] = texture;
                }
                catch (FormatException exception)
                {
                    Debug.LogWarning(
                        $"Invalid Memento Match portrait: {exception.Message}");
                    return;
                }
            }
        }

        hostessImage.style.backgroundImage =
            new StyleBackground(Background.FromTexture2D(texture));
    }

    private static void RemoveGeneratedBackdrop(
        Texture2D texture,
        bool removeOuterPaleArtifacts)
    {
        MementoMatchPortraitMatteCleaner.CleanInPlace(
            texture,
            removeOuterPaleArtifacts);
    }

    private void ApplyGuidePortrait(VisualElement target, int guideId)
    {
        if (target == null)
            return;
        target.style.backgroundImage = new StyleBackground(StyleKeyword.None);
        if (!guideTextures.ContainsKey(guideId))
            LoadGuidePortrait(guideId);
        if (!guideTextures.TryGetValue(guideId, out Texture2D portrait) ||
            portrait == null)
            return;
        target.style.backgroundImage =
            new StyleBackground(Background.FromTexture2D(portrait));
    }

    private void ApplyGuideFace(VisualElement target, int guideId)
    {
        if (target == null)
            return;

        target.style.backgroundImage = new StyleBackground(StyleKeyword.None);
        if (!guideTextures.ContainsKey(guideId))
            LoadGuidePortrait(guideId);
        if (!guideTextures.TryGetValue(guideId, out Texture2D portrait) ||
            portrait == null ||
            !portrait.isReadable)
        {
            return;
        }

        if (!guideFaceTextures.TryGetValue(guideId, out Texture2D face) ||
            face == null)
        {
            RectInt crop = MementoPortraitFraming.FaceRect(guideId, portrait.width, portrait.height);
            int size = crop.width;
            int x = crop.x;
            int y = crop.y;
            face = new Texture2D(size, size, TextureFormat.RGBA32, false)
            {
                name = $"Memento Match Guide Face {guideId}",
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp
            };
            face.SetPixels(portrait.GetPixels(x, y, size, size));
            face.Apply(false, false);
            guideFaceTextures[guideId] = face;
        }

        target.style.backgroundImage =
            new StyleBackground(Background.FromTexture2D(face));
    }

    private void OnVoiceLineStarted(MementoMatchVoiceCue cue)
    {
        if (string.IsNullOrEmpty(cue.Subtitle))
            return;

        if (cue.ClipName.EndsWith("_skill", StringComparison.Ordinal) && skillCutInQuote != null)
        {
            skillCutInQuote.text = cue.Subtitle;
            // The cut-in owns this subtitle; do not duplicate it over the board.
            SetDisplay(voiceSubtitleBox, false);
            return;
        }
        if ((cue.ClipName.EndsWith("_victory", StringComparison.Ordinal) ||
             cue.ClipName.EndsWith("_defeat", StringComparison.Ordinal)) &&
            duelDialogueLine != null)
            duelDialogueLine.text = cue.Subtitle;

        if (gameManager == null || gameManager.IsMainMenuOpen)
        {
            SetGuideLine(cue.Subtitle);
            return;
        }

        if (voiceSubtitleSpeaker != null)
            voiceSubtitleSpeaker.text =
                gameManager.GetGuideName(cue.GuideId).ToUpperInvariant();
        if (voiceSubtitleLine != null)
            voiceSubtitleLine.text = cue.Subtitle;
        ApplyGuideFace(voiceSubtitleFace, cue.GuideId);
        SetDisplay(voiceSubtitleBox, true);

        if (voiceSubtitleRoutine != null)
            StopCoroutine(voiceSubtitleRoutine);
        voiceSubtitleRoutine = StartCoroutine(
            HideVoiceSubtitleAfter(Mathf.Max(1.2f, cue.Duration + 0.35f)));
    }

    private System.Collections.IEnumerator HideVoiceSubtitleAfter(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        SetDisplay(voiceSubtitleBox, false);
        voiceSubtitleRoutine = null;
    }

    private void OnProgressionChanged()
    {
        RefreshMenu();
        BuildAchievementList();
    }

    private void OnResultChanged()
    {
        HideSkillCutIn();
        if (!outcomePresentationPending)
            SetDisplay(duelDialogueOverlay, false);
        SetDisplay(voiceSubtitleBox, false);
        RefreshAll(true);
    }

    private void ToggleMute()
    {
        gameManager?.ToggleMute();
        RefreshMenu();
    }

    private void RequestDeleteSave()
    {
        if (deleteSaveButton == null || gameManager == null)
            return;

        // The first click only arms the button; the save is deleted on the
        // second click while that arm is still valid.
        if (!deleteSaveConfirmation.Request())
        {
            MementoMatchSfx.PlayUiLocked();
            int armedVersion = deleteSaveConfirmation.Version;
            deleteSaveButton.text = "CONFIRMAR · BORRAR TODO";
            deleteSaveButton.AddToClassList("confirming");
            deleteSaveButton.schedule.Execute(() =>
            {
                if (!deleteSaveConfirmation.Expire(armedVersion))
                    return;
                deleteSaveButton.text = "BORRAR PARTIDA";
                deleteSaveButton.RemoveFromClassList("confirming");
            }).StartingIn(DeleteSaveConfirmation.ArmTimeoutMilliseconds);
            return;
        }

        MementoMatchSfx.PlayUiConfirm();
        deleteSaveButton.text = "BORRANDO…";
        deleteSaveButton.SetEnabled(false);
        playerIdentity.Clear();
        pendingPlayerNameContinuation = null;
        gameManager.DeleteSaveAndRestart();
    }

    private void Quit()
    {
        gameManager?.QuitGame();
    }

    private void ReturnToMenu()
    {
        openingMemory?.Dispose();
        MementoMatchSfx.PlayUiBack();
        achievementsOpen = false;
        campaignOpen = false;
        storyVoice?.Stop();
        storyOpen = false;
        creditsOpen = false;
        finalCelebrationOpen = false;
        guideEquipOpen = false;
        storyReplayOpen = false;
        postgameOpen = false;
        tutorialFlow.Reset();
        guideEquipContinuation = null;
        pendingEquipGuideId = -1;
        outcomePresentationPending = false;
        outcomePresentationVersion++;
        HideSkillCutIn();
        SetDisplay(skillProcToast, false);
        SetDisplay(duelDialogueOverlay, false);
        gameManager?.ShowMainMenu();
        previewedGuideId = gameManager != null ? gameManager.SelectedGuideId : 0;
        ApplyGuidePreview(previewedGuideId);
        RefreshAll(true);

        if (gameManager != null && gameManager.LastResultWasPostgame)
        {
            // Un resultado terminado presenta su escena y reabre el selector;
            // un duelo abandonado sólo vuelve al selector, sin escena.
            if (gameManager.TryConsumePostgameOutcome())
                PresentPendingPostgameStory();
            else
                OpenPostgameSelection();
        }
        else if (gameManager != null && gameManager.LastResultWasCampaign)
        {
            selectedCampaignLevel = gameManager.LastResultWasVictory
                ? gameManager.CampaignRecommendedLevel
                : Mathf.Max(0, gameManager.LastCampaignLevel);
            selectedCampaignWorld = Mathf.Clamp(
                selectedCampaignLevel / MementoMatchCampaignRules.LevelsPerWorld,
                0, MementoMatchCampaignRules.WorldCount - 1);
            PresentPendingCampaignStory();
        }
        else
        {
            PlayGuideVoice(previewedGuideId, MementoMenuVoiceContext.MenuArrival);
        }
    }

    private void OpenHomePage() => ShowShellPage(ShellPage.Home);
    private void OpenStoryPage() => ShowShellPage(ShellPage.Story);
    private void OpenTeamPage() => ShowShellPage(ShellPage.Team);
    private void OpenCollectionPage() => ShowShellPage(ShellPage.Collection);
    private void OpenSettingsPage() => ShowShellPage(ShellPage.Settings);

    private void ShowShellPage(ShellPage page, bool playSound = true)
    {
        if (playSound && page != activeShellPage)
            MementoMatchSfx.PlayUiConfirm();
        MementoTelemetry.Record("menu_page", (int)page);
        activeShellPage = page;
        for (int i = 0; i < shellPages.Length; i++)
        {
            shellPages[i]?.EnableInClassList("active", i == (int)page);
            shellNavButtons[i]?.EnableInClassList("selected", i == (int)page);
        }
    }

    private enum ShellPage
    {
        Home,
        Story,
        Team,
        Collection,
        Settings
    }

    private static string GetMenuLine(int guideId)
    {
        if (guideId == AnimalMemoryContentIds.MikaGuide)
            return "En mi duelo, muéstrame un plan que resista la presión.";
        if (guideId == AnimalMemoryContentIds.YoruGuide)
            return "Cruza mi cielo ocho veces sin perderte.";
        if (guideId == AnimalMemoryContentIds.HanaGuide)
            return "Cada pareja puede hacer florecer un recuerdo nuevo.";
        if (guideId == AnimalMemoryContentIds.MomoGuide)
            return "Un buen combo se disfruta mejor con algo dulce.";
        return "Un error no tiene por qué ser el final.";
    }

    private static string GetLockedLine(int guideId)
    {
        int world = Mathf.Clamp(guideId, 0, AnimalMemoryContentIds.GuideCount - 1) + 1;
        return $"Completa el mundo {world} de la historia para reclutar a esta guía y equipar su habilidad.";
    }

    private void SetGuideLine(string line)
    {
        if (hostessLine != null)
            hostessLine.text = line;
    }

    private static void SetDisplay(VisualElement element, bool visible)
    {
        if (element != null)
            element.style.display = visible ? DisplayStyle.Flex : DisplayStyle.None;
    }

    private static int CountBits(int value)
    {
        int count = 0;
        while (value != 0)
        {
            count += value & 1;
            value >>= 1;
        }

        return count;
    }
}
