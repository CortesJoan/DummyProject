using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class ComboSystem : MonoBehaviour, ISavable
{
    [SerializeField] CardMatchUI cardMatchUI;
    [SerializeField] DifficultyHandler difficultyHandler;
    [SerializeField] int currentCombo;
    [SerializeField] int maxCombo;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text comboText;

    public UnityEvent<int> onComboPerformed;
    public UnityEvent<int> onComboDropped;
    bool isInCombo = false;

    private const string ScoreKey = "CardMatchUIScore";
    private const string MaxComboKey = "CardMatchUIMaxCombo";
    private int score;

    public int MaxCombo { get => maxCombo;   }
    public int Score { get => score; }

    private void Start()
    {
        cardMatchUI.onMatchMade.AddListener(OnMatchMade);
        cardMatchUI.onMatchFailed.AddListener(OnMatchFailed);

        UpdateScoreText();
    }

    private void OnMatchFailed()
    {
        if (!isInCombo) return;
        DropCombo();

    }

    private void OnMatchMade()
    {
        if (!isInCombo)
        {
            isInCombo = true;
        }
        UpdateCombo();
        UpdateScore();
    }
    void UpdateCombo()
    {
        isInCombo = true;
        onComboPerformed?.Invoke(currentCombo);
        currentCombo++;

        UpdateComboText();
    }
    void UpdateScore()
    {
        //Improvement catch current difficulty by event once game start
        GameDifficulty currentDifficulty = difficultyHandler.GetCurrentDifficulty();

        score += currentDifficulty.baseScore * currentDifficulty.scoreComboMultiplier;
        UpdateScoreText();
    }

    void DropCombo()
    {
        if (currentCombo > maxCombo)
        {
            maxCombo = currentCombo;
        }
        currentCombo = 0;
        isInCombo = false;
        onComboDropped?.Invoke(currentCombo);

        UpdateComboText();
    } 
    public void SaveData(GameData data)
    {
        if (currentCombo > maxCombo)
        {
            maxCombo = currentCombo;
        }
        data.SetData<int>(ScoreKey, score);
        data.SetData<int>(MaxComboKey, maxCombo);
    }

    public void LoadData(GameData data)
    {
        score = data.GetData<int>(ScoreKey);
        maxCombo = data.GetData<int>(MaxComboKey);
    }

    private void UpdateScoreText()
    {
        scoreText.text = $"Score: {score}";
    }
    private void UpdateComboText()
    {
        comboText.text = $"Combo: {currentCombo}";
    }
}
