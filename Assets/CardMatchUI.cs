using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static UnityEngine.UIElements.UxmlAttributeDescription;

public enum CardState
{
    FaceDown,
    FaceUp,
    Matched
}

public class CardMatchUI : MonoBehaviour
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
    private void Start()
    {
        CreateGrid();
        StartCoroutine(RevealCardsThenHide(timeUntilHide));
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

}