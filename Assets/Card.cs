using UnityEngine;
using UnityEngine.UI;

public class Card : MonoBehaviour
{
    [SerializeField] private Image cardRenderer;
    private Sprite currentSprite;
    private Sprite originalSprite;
    
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
        cardRenderer.sprite = originalSprite;
    }

    public void ToggleRenderer(bool on)
    {
        cardRenderer.enabled = on;
    }

}