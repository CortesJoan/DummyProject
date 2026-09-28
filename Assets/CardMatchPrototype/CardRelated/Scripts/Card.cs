using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] private Image cardRenderer;
    [SerializeField] private LayoutElement layoutElement;
    private Sprite currentSprite;
    private Sprite originalSprite;
    const int minimumPriority=-1;
    const int defaultPriority=1;
    public Sprite CurrentSprite
    {
        get { return currentSprite; }
        set {
            currentSprite = value; 
        cardRenderer.sprite = currentSprite;
        }
    }

    public Sprite OriginalSprite
    {
        get { return originalSprite; }
        set
        {
            originalSprite = value; 
        }
    }

    public Color VisualTint
    {
        get { return cardRenderer != null ? cardRenderer.color : Color.white; }
        set
        {
            if (cardRenderer != null)
                cardRenderer.color = value;
        }
    }

    public void RestoreOriginalSprite()
    {
        CurrentSprite= originalSprite;
        layoutElement.layoutPriority = defaultPriority;
        VisualTint = Color.white;
    }

    public void ResetVisualOrientation()
    {
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;
        if (cardRenderer == null)
            return;

        RectTransform rendererRect = cardRenderer.rectTransform;
        rendererRect.anchorMin = new Vector2(0.5f, 0.5f);
        rendererRect.anchorMax = new Vector2(0.5f, 0.5f);
        rendererRect.anchoredPosition = Vector2.zero;
        rendererRect.localRotation = Quaternion.identity;
        rendererRect.localScale = Vector3.one;
        cardRenderer.preserveAspect = true;
    }

    public void FitVisualTo(Vector2 cardSize)
    {
        if (cardRenderer == null)
            return;

        RectTransform rendererRect = cardRenderer.rectTransform;
        rendererRect.anchorMin = new Vector2(0.5f, 0.5f);
        rendererRect.anchorMax = new Vector2(0.5f, 0.5f);
        rendererRect.anchoredPosition = Vector2.zero;
        rendererRect.sizeDelta = new Vector2(
            Mathf.Max(1f, cardSize.x),
            Mathf.Max(1f, cardSize.y));
    }

    public void ToggleRenderer(bool on)
    {
        cardRenderer.enabled = on;
    }
    public void DownPriority()
    {
        layoutElement.layoutPriority = minimumPriority;
    }
}