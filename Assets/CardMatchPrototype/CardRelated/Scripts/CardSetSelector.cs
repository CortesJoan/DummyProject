using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.AddressableAssets;

/// <summary>
/// UI component for selecting card sets in the game.
/// </summary>
public class CardSetSelector : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_Text cardSetNameText;
    [SerializeField] private TMP_Text cardSetDescriptionText;
    [SerializeField] private Image cardSetIconImage;
    [SerializeField] private Button previousButton;
    [SerializeField] private Button nextButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private GameObject loadingIndicator;

    [Header("Card Set References")]
    [SerializeField] private List<AssetReference> cardSetReferences = new List<AssetReference>();

    private int currentIndex = 0;
    private bool isLoading = false;

    private void Start()
    {
        if (previousButton != null)
            previousButton.onClick.AddListener(OnPreviousButtonClicked);

        if (nextButton != null)
            nextButton.onClick.AddListener(OnNextButtonClicked);

        if (loadButton != null)
            loadButton.onClick.AddListener(OnLoadButtonClicked);

        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);

        UpdateUI();
    }

    private void OnDestroy()
    {
        if (previousButton != null)
            previousButton.onClick.RemoveListener(OnPreviousButtonClicked);

        if (nextButton != null)
            nextButton.onClick.RemoveListener(OnNextButtonClicked);

        if (loadButton != null)
            loadButton.onClick.RemoveListener(OnLoadButtonClicked);
    }

    private void OnPreviousButtonClicked()
    {
        if (isLoading) return;

        currentIndex--;
        if (currentIndex < 0)
            currentIndex = cardSetReferences.Count - 1;

        UpdateUI();
    }

    private void OnNextButtonClicked()
    {
        if (isLoading) return;

        currentIndex++;
        if (currentIndex >= cardSetReferences.Count)
            currentIndex = 0;

        UpdateUI();
    }

    private async void OnLoadButtonClicked()
    {
        if (isLoading || CardSetManager.Instance == null) return;

        isLoading = true;
        SetButtonsInteractable(false);

        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);

        bool success = await CardSetManager.Instance.LoadCardSetByIndexAsync(currentIndex);

        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);

        isLoading = false;
        SetButtonsInteractable(true);

        if (success)
        {
            Debug.Log($"Card set loaded successfully!");
        }
        else
        {
            Debug.LogError("Failed to load card set!");
        }
    }

    private void UpdateUI()
    {
        if (cardSetReferences.Count == 0)
        {
            if (cardSetNameText != null)
                cardSetNameText.text = "No Card Sets Available";
            
            if (cardSetDescriptionText != null)
                cardSetDescriptionText.text = "";
            
            return;
        }

        // For preview, we'd need to load the CardSetData temporarily
        // For simplicity, showing basic info
        if (cardSetNameText != null)
            cardSetNameText.text = $"Card Set {currentIndex + 1}";

        if (cardSetDescriptionText != null)
            cardSetDescriptionText.text = $"Select to load this card set";

        UpdateButtonStates();
    }

    private void UpdateButtonStates()
    {
        if (previousButton != null)
            previousButton.interactable = cardSetReferences.Count > 1 && !isLoading;

        if (nextButton != null)
            nextButton.interactable = cardSetReferences.Count > 1 && !isLoading;

        if (loadButton != null)
            loadButton.interactable = cardSetReferences.Count > 0 && !isLoading;
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (previousButton != null)
            previousButton.interactable = interactable && cardSetReferences.Count > 1;

        if (nextButton != null)
            nextButton.interactable = interactable && cardSetReferences.Count > 1;

        if (loadButton != null)
            loadButton.interactable = interactable;
    }

    public void SetCardSetReferences(List<AssetReference> references)
    {
        cardSetReferences = references;
        currentIndex = 0;
        UpdateUI();
    }
}
