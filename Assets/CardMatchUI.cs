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
    public GameObject cardPrefab;
    public GridLayoutGroup gridLayout;
    public int gridRows = 2;
    public int gridColumns = 2;
    public List<Sprite> cardFaceSprites;

    private List<GameObject> flippedCards = new List<GameObject>();
    private Dictionary<GameObject, CardState> cardStates = new Dictionary<GameObject, CardState>();
    private int score = 0;

    private void Start()
    {
        CreateGrid();
    }

    private void CreateGrid()
    {
        if ((gridRows * gridColumns) % 2 != 0)
        {
            Debug.LogError("The total number of cards (rows * columns) must be even for a matching game.");
            return;  
        }
        int uniqueCardCount = (gridRows * gridColumns) / 2;
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
            GameObject newCard = Instantiate(cardPrefab, gridLayout.transform);
            newCard.name = $"Card_{i}";

            Image cardImage = newCard.GetComponentInChildren<Image>();
            if (cardFaceSprites.Count > 0)
            {
                cardImage.sprite = spritesToAssign[0];  
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

    private void OnCardClicked(GameObject card)
    {
        if (cardStates[card] == CardState.Matched || flippedCards.Count >= 2 || cardStates[card] == CardState.FaceUp)
        {
            return;
        }

        FlipCard(card);
        flippedCards.Add(card);
        cardStates[card] = CardState.FaceUp;

        if (flippedCards.Count == 2)
        {
            StartCoroutine(CheckForMatch());
        }
    }

    private IEnumerator CheckForMatch()
    {
        yield return new WaitForSeconds(0.5f);

        GameObject card1 = flippedCards[0];
        GameObject card2 = flippedCards[1];

        Sprite sprite1 = card1.GetComponentInChildren<Image>().sprite;
        Sprite sprite2 = card2.GetComponentInChildren<Image>().sprite;

        if (sprite1 == sprite2)
        {
            Debug.Log("Match!");
            score++;
            //TODO play match sound effect.
            cardStates[card1] = CardState.Matched;
            cardStates[card2] = CardState.Matched;
            card1.transform.Find("Visuals").gameObject.SetActive(false);
            card2.transform.Find("Visuals").gameObject.SetActive(false);
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

            FlipCard(card1);
            FlipCard(card2);
            cardStates[card1] = CardState.FaceDown;
            cardStates[card2] = CardState.FaceDown;
            flippedCards.Clear();
        }
    }

    private void FlipCard(GameObject card)
    {
     //TODO change the sprite for other sprite
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
} 