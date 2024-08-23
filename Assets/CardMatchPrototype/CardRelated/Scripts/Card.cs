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

    public void RestoreOriginalSprite()
    {
        CurrentSprite= originalSprite;
        layoutElement.layoutPriority = defaultPriority;
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