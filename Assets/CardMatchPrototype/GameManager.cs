using System.Collections.Generic;
using TMPro;
using UnityEngine;
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
        //Improvement move this list initiation to a dependency injection in the future
        savableObjects = new List<ISavable>() { cardMatchUI, comboSystem, this };
        LoadData();
        InitializeGame();
    }
    public void InitialEventSubscription()
    {
        easyButton.onClick.AddListener(() => StartGame(0));
        mediumButton.onClick.AddListener(() => StartGame(1));
        hardButton.onClick.AddListener(() => StartGame(2));
        newGameWithSameDifficulty.onClick.AddListener(() => StartGame(difficultyHandler.GetCurrentDifficultyLevel()));
        newGameWithMoreDifficulty.onClick.AddListener(() => StartGame(difficultyHandler.GetNextDifficultyLevel()));
        cardMatchUI.onWinEvent.AddListener(OnWinGame);
        openStatsButton.onClick.AddListener(OpenStatsMenu);
        foreach (var button in buttonsThatReturnToMainMenu)
        {
            button.onClick.AddListener(ReturnToMainMenu);

        }
    }

    private void ReturnToMainMenu()
    {
        LoadData();
        mainMenuUI.SetActive(true);
        gameUI.SetActive(false);
        winUI.SetActive(false);
        statsMenuUI.SetActive(false);
    }
    private void InitializeGame()
    {
        mainMenuUI.SetActive(true);
        gameUI.SetActive(false);
        winUI.SetActive(false);
        statsMenuUI.SetActive(false);
    }
    private void OpenStatsMenu()
    {
        UpdateStatsText();
        mainMenuUI.SetActive(false);
        gameUI.SetActive(false);
        winUI.SetActive(false);
        statsMenuUI.SetActive(true);
        UpdateStatsText();
    }

    private void StartGame(int difficultyLevelIndex)
    {
        difficultyHandler.SetCurrentDifficultyLevel(difficultyLevelIndex);
        winUI.SetActive(false);
        mainMenuUI.SetActive(false);
        gameUI.SetActive(true);
        GameDifficulty curentDifficulty = difficultyHandler.GetCurrentDifficulty();
        cardMatchUI.SetupGame(curentDifficulty.gridSize.x, curentDifficulty.gridSize.y, curentDifficulty.backgroundColor, curentDifficulty.timeToSeeCards);
    }

    private void OnWinGame()
    {
        winUI.SetActive(true);
        actualWins++;
        easyButton.onClick.RemoveListener(() => StartGame(0));
        mediumButton.onClick.RemoveListener(() => StartGame(1));
        hardButton.onClick.RemoveListener(() => StartGame(2));
        SaveSystem.Save(savableObjects);


    }
    private void LoadData()
    {
        SaveSystem.Load(savableObjects);
    }

    public void SaveData(GameData data)
    {
        data.SetData(ActualWinsKey, actualWins);
    }

    public void LoadData(GameData data)
    {
        actualWins = data.GetData<int>(ActualWinsKey);
        
    }

    public void UpdateStatsText()
    {
        statsText.text = $"" +
            $"Total Wins: {actualWins} \n" +
            $"Max Combo: {comboSystem.MaxCombo} \n" +
            $"Last Score:  {comboSystem.Score} \n" +
            $"";
 

    }
}