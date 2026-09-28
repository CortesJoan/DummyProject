using System.Collections.Generic;
using AnimalMemory.Progression;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ComboSystem : MonoBehaviour, ISavable
{
    [SerializeField] private CardMatchUI cardMatchUI;
    [SerializeField] private DifficultyHandler difficultyHandler;
    [SerializeField] private int currentCombo;
    [SerializeField] private int maxCombo;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text comboText;

    public UnityEvent<int> onComboPerformed;
    public UnityEvent<int> onComboDropped;

    private const string MaxComboKey = "CardMatchUIMaxCombo";

    private readonly Stack<ComboSnapshot> matchHistory =
        new Stack<ComboSnapshot>();
    private bool isInCombo;
    private int score;
    private int runMaxCombo;
    private AnimalMemoryGuideRunState guideRunState;

    public int MaxCombo => maxCombo;
    public int Score => score;
    public int CurrentCombo => currentCombo;
    public int RunMaxCombo => runMaxCombo;

    public void ConfigureGuide(AnimalMemoryGuideRunState runState)
    {
        guideRunState = runState;
    }

    private void Start()
    {
        if (cardMatchUI != null)
        {
            cardMatchUI.onMatchMade.AddListener(OnMatchMade);
            cardMatchUI.onMatchUndone.AddListener(OnMatchUndone);
            cardMatchUI.onMatchFailed.AddListener(OnMatchFailed);
        }

        UpdateScoreText();
        UpdateComboText();
    }

    public void ResetForNewGame()
    {
        currentCombo = 0;
        score = 0;
        runMaxCombo = 0;
        isInCombo = false;
        matchHistory.Clear();
        UpdateScoreText();
        UpdateComboText();
    }

    private void OnMatchFailed()
    {
        CaptureTurnSnapshot();
        if (!isInCombo)
        {
            return;
        }

        if (guideRunState != null && guideRunState.TryProtectCombo(currentCombo))
        {
            UpdateComboText();
            return;
        }

        DropCombo();
    }

    private void OnMatchMade()
    {
        CaptureTurnSnapshot();
        isInCombo = true;
        UpdateCombo();
        UpdateScore();
    }

    private void CaptureTurnSnapshot()
    {
        matchHistory.Push(new ComboSnapshot(
            currentCombo,
            maxCombo,
            score,
            runMaxCombo,
            isInCombo));
    }

    private void OnMatchUndone()
    {
        if (matchHistory.Count == 0)
            return;

        ComboSnapshot snapshot = matchHistory.Pop();
        currentCombo = snapshot.CurrentCombo;
        maxCombo = snapshot.MaxCombo;
        score = snapshot.Score;
        runMaxCombo = snapshot.RunMaxCombo;
        isInCombo = snapshot.IsInCombo;
        UpdateScoreText();
        UpdateComboText();
    }

    private void UpdateCombo()
    {
        currentCombo++;
        runMaxCombo = Mathf.Max(runMaxCombo, currentCombo);
        onComboPerformed?.Invoke(currentCombo);
        MementoMatchSfx.PlayComboPulse(currentCombo);
        UpdateComboText();
    }

    private void UpdateScore()
    {
        GameDifficulty currentDifficulty = difficultyHandler != null
            ? difficultyHandler.GetCurrentDifficulty()
            : null;

        if (currentDifficulty == null)
        {
            Debug.LogWarning("Score was not updated because no difficulty is active.");
            return;
        }

        score += currentDifficulty.baseScore * currentDifficulty.scoreComboMultiplier;
        UpdateScoreText();
    }

    private void DropCombo()
    {
        maxCombo = Mathf.Max(maxCombo, currentCombo);
        currentCombo = 0;
        isInCombo = false;
        onComboDropped?.Invoke(currentCombo);
        UpdateComboText();
    }

    public void UseToolkitHud(bool enabled)
    {
        if (scoreText != null)
            scoreText.gameObject.SetActive(!enabled);
        if (comboText != null)
            comboText.gameObject.SetActive(!enabled);
    }

    public void SaveData(GameData data)
    {
        maxCombo = Mathf.Max(maxCombo, currentCombo);
        data.SetData(MaxComboKey, maxCombo);
    }

    public void LoadData(GameData data)
    {
        maxCombo = data.GetData<int>(MaxComboKey);
        currentCombo = 0;
        score = 0;
        runMaxCombo = 0;
        isInCombo = false;
        UpdateScoreText();
        UpdateComboText();
    }

    private void OnDestroy()
    {
        if (cardMatchUI == null)
            return;

        cardMatchUI.onMatchMade.RemoveListener(OnMatchMade);
        cardMatchUI.onMatchUndone.RemoveListener(OnMatchUndone);
        cardMatchUI.onMatchFailed.RemoveListener(OnMatchFailed);
    }

    private readonly struct ComboSnapshot
    {
        public int CurrentCombo { get; }
        public int MaxCombo { get; }
        public int Score { get; }
        public int RunMaxCombo { get; }
        public bool IsInCombo { get; }

        public ComboSnapshot(
            int currentCombo,
            int maxCombo,
            int score,
            int runMaxCombo,
            bool isInCombo)
        {
            CurrentCombo = currentCombo;
            MaxCombo = maxCombo;
            Score = score;
            RunMaxCombo = runMaxCombo;
            IsInCombo = isInCombo;
        }
    }

    private void UpdateScoreText()
    {
        if (scoreText != null)
        {
            scoreText.text = $"Score: {score}";
        }
    }

    private void UpdateComboText()
    {
        if (comboText != null)
        {
            comboText.text = $"Combo: {currentCombo}";
        }
    }
}
