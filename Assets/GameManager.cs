using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    [SerializeField] private DifficultyHandler difficultyHandler;
    [SerializeField] private CardMatchUI cardMatchUI;
    [SerializeField] private GameObject mainMenuUI;
    [SerializeField] private GameObject gameUI;   
    [SerializeField] private GameObject winUI;   
    [Header("Difficulty Buttons")]
    [SerializeField] private Button easyButton;
    [SerializeField] private Button mediumButton; 
    [SerializeField] private Button hardButton;
    [SerializeField] private Button newGameWithSameDifficulty;
    [SerializeField] private Button newGameWithMoreDifficulty;

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
    }
    private void InitializeGame()
    {
        mainMenuUI.SetActive(true);
        gameUI.SetActive(false);
        winUI.SetActive(false); 
    }

    private void StartGame(int difficultyLevelIndex)
    {
        difficultyHandler.SetCurrentDifficultyLevel(difficultyLevelIndex);
        winUI.SetActive(false);
        mainMenuUI.SetActive(false);
        gameUI.SetActive(true);
        GameDifficulty curentDifficulty = difficultyHandler.GetCurrentDifficulty();
        cardMatchUI.SetupGame(curentDifficulty.gridSize.x, curentDifficulty.gridSize.y, curentDifficulty.backgroundColor,curentDifficulty.timeToSeeCards);
    }

    private void OnWinGame()
    {
        winUI.SetActive(true);
        SaveSystem.Save(cardMatchUI);
        easyButton.onClick.RemoveListener(() => StartGame(0));
        mediumButton.onClick.RemoveListener(() => StartGame(1));
        hardButton.onClick.RemoveListener(() => StartGame(2));


    }
}