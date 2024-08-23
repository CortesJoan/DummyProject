using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public enum CardState
{
    FaceDown,
    FaceUp,
    Matched
}

public class CardMatchUI : MonoBehaviour, ISavable
{
    [SerializeField] Card cardPrefab;
    [SerializeField] GridLayoutGroup gridLayout;
    [SerializeField] int gridRows = 2;
    [SerializeField] int gridColumns = 2;
    [SerializeField] List<Sprite> cardFaceSprites;
    [SerializeField] private Sprite hiddenSprite;
    [SerializeField] private float timeUntilHide = 2f;
    private List<Card> flippedCards = new List<Card>();
    private Dictionary<Card, CardState> cardStates = new Dictionary<Card, CardState>();
    private int score = 0;
    const int minimumNumbersOfCardsToMakeAMatch = 2;
    float matchDelay = 0.5f;
    [SerializeReference] CanvasScaler canvasScaler;
    [SerializeField] Vector2 defaultMobileResolution = new Vector2(1080, 1920);
    [SerializeField] Vector2 defaultPCResolution = new Vector2(1920, 1080);
    [Header("UI Related")]
    [SerializeField] private TMP_Text scoreText;
    [SerializeField] private TMP_Text matchesText;
    [SerializeField] private TMP_Text turnsText;
    int matches,turns=0;
    private void Start()
    {

        UpdateReferenceResolution();
        CreateGrid();
        AdjustGridCellSize();
        StartCoroutine(RevealCardsThenHide(timeUntilHide));



        UpdateScoreText();
        UpdateMatchesText();
        UpdateTurnsText();
    }

    //TODO move this to another class
    public void UpdateReferenceResolution(){
        if (Application.isMobilePlatform)
        {
            canvasScaler.referenceResolution = defaultMobileResolution;
        
        }
        else
        {
            canvasScaler.referenceResolution = defaultPCResolution;
        }
    }
    

  
    
    private void AdjustGridCellSize()
    {
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = gridColumns;

        float availableHeight = gridLayout.GetComponent<RectTransform>().rect.height;
        float cellHeight = availableHeight / gridRows - gridLayout.spacing.y;
        gridLayout.cellSize = new Vector2(cellHeight, cellHeight); // Assume square cells 
    }
    private void CreateGrid()
    {
        if ((gridRows * gridColumns) % minimumNumbersOfCardsToMakeAMatch != 0)
        {
            Debug.LogError("The total number of cards (rows * columns) must be even for a matching game.");
            return;
        }
        int uniqueCardCount = (gridRows * gridColumns) / minimumNumbersOfCardsToMakeAMatch;
        List<Sprite> spritesToAssign = new List<Sprite>();
        for (int i = 0; i < uniqueCardCount; i++)
        {

            if (i < cardFaceSprites.Count)
            {
                spritesToAssign.Add(cardFaceSprites[i]);
                spritesToAssign.Add(cardFaceSprites[i]);
            }
            else
            {
                Debug.LogError("Not enough unique sprites for this grid size!");
                return;
            }
        }
        for (int i = 0; i < spritesToAssign.Count; i++)
        {
            Sprite temp = spritesToAssign[i];
            int randomIndex = Random.Range(i, spritesToAssign.Count);
            spritesToAssign[i] = spritesToAssign[randomIndex];
            spritesToAssign[randomIndex] = temp;
        }

        for (int i = 0; i < gridRows * gridColumns; i++)
        {
            Card newCard = Instantiate<Card>(cardPrefab, gridLayout.transform);
            newCard.name = $"Card_{i}";


            if (cardFaceSprites.Count > 0)
            {
                newCard.OriginalSprite = spritesToAssign[0];
                newCard.RestoreOriginalSprite();
                spritesToAssign.RemoveAt(0);
            }
            else
            {
                Debug.LogError("No card face sprites assigned in the Inspector!");
            }

            Button cardButton = newCard.GetComponent<Button>();
            cardButton.onClick.AddListener(() => OnCardClicked(newCard));

            cardStates.Add(newCard, CardState.FaceDown);
        }
    }

    private void OnCardClicked(Card card)
    {
        if (cardStates[card] == CardState.Matched || flippedCards.Count >= 2 || cardStates[card] == CardState.FaceUp)
        {
            return;
        }

        FlipCard(card);
        flippedCards.Add(card);
        cardStates[card] = CardState.FaceUp;

        if (flippedCards.Count == minimumNumbersOfCardsToMakeAMatch)
        {
            StartCoroutine(CheckForMatch());
        }
    }

    private IEnumerator CheckForMatch()
    {
        yield return new WaitForSeconds(matchDelay);
        bool allCardsMatch = true;
        Sprite firstCardSprite = flippedCards[0].CurrentSprite;
        foreach (var card in flippedCards)
        {
            allCardsMatch &= firstCardSprite == card.CurrentSprite;
        }
        if (allCardsMatch)
        {
            Debug.Log("Match!");
            score++;
            matches++;
            UpdateScoreText();
            UpdateMatchesText();
            //TODO play match sound effect.
            foreach (var card in flippedCards)
            {
                cardStates[card] = CardState.Matched;
                card.ToggleRenderer(false);
            }
            flippedCards.Clear();

            if (CheckGameOver())
            {
                Debug.Log("Game Over! You Win!");
            }
        }
        else
        {
            Debug.Log("No Match!");
            //TODO play no match sound effect.
            foreach (var card in flippedCards)
            {
                FlipCard(card);
                cardStates[card] = CardState.FaceDown;
            }
            flippedCards.Clear();
        }
        turns++;
        UpdateTurnsText();
    }

    private void FlipCard(Card card)
    {
        //TODO play card flipping sound
        if (cardStates[card] == CardState.FaceDown)
        {
            ShowCardFace(card);
            cardStates[card] = CardState.FaceUp;
        }
        else if (cardStates[card] == CardState.FaceUp)
        {
            HideCardFace(card);
            cardStates[card] = CardState.FaceDown;
        }
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
        foreach (var cardState in cardStates.Values)
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
        foreach (Card card in cardStates.Keys)
        {
            ShowCardFace(card);
        }

        yield return new WaitForSeconds(delay);


        foreach (Card card in cardStates.Keys)
        {
            HideCardFace(card);
        }
    }
    private void UpdateScoreText()
    {
        scoreText.text = $"Score: {score}";
    }

    private void UpdateMatchesText()
    {
        matchesText.text = $"Matches: {matches}";
    }

    private void UpdateTurnsText()
    {
        turnsText.text = $"Turns: {turns}";
    }

    public void SaveData(GameData data)
    {
        data.score = score;
        data.totalMatches = matches;
        data.turns = turns;
    }

    public void LoadData(GameData data)
    {

        score = data.score;
        matches = data.totalMatches;
        turns = data.turns;
    }
}