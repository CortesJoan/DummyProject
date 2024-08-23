using System;
using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] Card cardPrefab;
    [SerializeField] GridLayoutGroup gridLayout;
    [SerializeField] Image gridBackground;
    [SerializeField] int gridRows = 2;
    [SerializeField] int gridColumns = 2;
    [SerializeField] List<Sprite> cardFaceSprites;
    [SerializeField] private Sprite hiddenSprite;
    [SerializeField] private float timeUntilHide = 2f;
    private List<Card> flippedCards = new List<Card>();
    private Dictionary<Card, CardState> cardStates = new Dictionary<Card, CardState>();
   
    const int minimumNumbersOfCardsToMakeAMatch = 2;
    float matchDelay = 0.1f;
    List<Card> cards = new List<Card>();
    [Header("UI Related")]


    [SerializeField] private TMP_Text matchesText;
    [SerializeField] private TMP_Text turnsText;
    int matches, turns = 0;

    public UnityEvent onTryingMatch;
    public UnityEvent onMatchMade;
    public UnityEvent onMatchFailed;
    public UnityEvent onWinEvent;





    public void SetupGame(int rows, int columns, Color backgroundColor, int timeUntilHide)
    {
        gridRows = rows;
        gridColumns = columns;
        gridBackground.color = backgroundColor;

        CreateGrid();
        //  AdjustGridCellSize();
        this.timeUntilHide = timeUntilHide;
        StartCoroutine(RevealCardsThenHide(timeUntilHide));

        UpdateMatchesText();
        UpdateTurnsText();

    }

    private void AdjustGridCellSize()
    {
        gridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayout.constraintCount = gridColumns;

        float availableHeight = gridLayout.GetComponent<RectTransform>().rect.height;
        float cellHeight = availableHeight / gridRows - gridLayout.spacing.y;
        gridLayout.cellSize = new Vector2(cellHeight, cellHeight);
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
            int randomIndex = UnityEngine.Random.Range(i, spritesToAssign.Count);
            spritesToAssign[i] = spritesToAssign[randomIndex];
            spritesToAssign[randomIndex] = temp;
        }
        int startPoint = 0;
        startPoint = TryToUsePreviousCards(startPoint, spritesToAssign);



        for (int i = startPoint; i < gridRows * gridColumns; i++)
        {
            Card newCard = Instantiate<Card>(cardPrefab, gridLayout.transform);
            newCard.name = $"Card_{i}";
            cards.Add(newCard);

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
    public int TryToUsePreviousCards(int startPoint, List<Sprite> spritesToAssign)
    {
        if (cards.Count == 0) return startPoint;


        cardStates.Clear();
        for (int i = 0; i < cards.Count; i++)
        {
            var currentCard = cards[i];
            currentCard.OriginalSprite = spritesToAssign[i];
            currentCard.RestoreOriginalSprite();
            currentCard.ToggleRenderer(true);
            cardStates[currentCard] =CardState.FaceDown ;
        }
        return cards.Count;


    }

    private void OnCardClicked(Card card)
    {
        if (cardStates[card] == CardState.Matched || flippedCards.Count >= 2 || cardStates[card] == CardState.FaceUp)
        {
            return;
        }

        FlipCard(card);
        onTryingMatch?.Invoke();
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
             
            matches++;
            onMatchMade?.Invoke();
         
            UpdateMatchesText();
          
            foreach (var card in flippedCards)
            {
                cardStates[card] = CardState.Matched;
                card.ToggleRenderer(false);
            }
            flippedCards.Clear();

            if (CheckGameOver())
            {
                onWinEvent?.Invoke();
                Debug.Log("Game Over! You Win!");
            }
        }
        else
        {
            Debug.Log("No Match!");
            onMatchFailed?.Invoke();
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
  
        data.totalMatches = matches;
        data.turns = turns;
    }

    public void LoadData(GameData data)
    {

      
        matches = data.totalMatches;
        turns = data.turns;
    }
}