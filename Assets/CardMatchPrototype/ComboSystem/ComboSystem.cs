using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ComboSystem : MonoBehaviour, ISavable
{
    [SerializeField] CardMatchUI cardMatchUI;
    [SerializeField] DifficultyHandler difficultyHandler;
    [SerializeField] int currentCombo;
    [SerializeField] int maxCombo;
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text comboText;
    bool isInCombo = false;
    private int score;
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
            currentCombo = maxCombo;
        }
        currentCombo = 0;
        isInCombo = false;
        UpdateComboText() ;
    }

    public void SaveData(GameData data)
    {
        data.maxCombo = maxCombo;
        data.score = score;
    }

    public void LoadData(GameData data)
    {
        score = data.score;
        maxCombo = data.maxCombo;
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
