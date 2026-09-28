using UnityEngine;
using UnityEngine.UI;

/// <summary>Uses the same reference and aspect policy as the Toolkit HUD.</summary>
[DefaultExecutionOrder(-200)]
public class ScreenScaler : MonoBehaviour
{
    [SerializeReference] private CanvasScaler canvasScaler;
    [SerializeField] private Vector2 defaultMobileResolution = new Vector2(1080, 1920);
    [SerializeField] private Vector2 defaultPCResolution = new Vector2(1920, 1080);
    private Vector2 lastScreen;

    private void OnEnable()
    {
        lastScreen = Vector2.zero;
        UpdateReferenceResolution();
    }

    private void Update()
    {
        Vector2 current = new Vector2(Screen.width, Screen.height);
        if (current != lastScreen)
            UpdateReferenceResolution();
    }

    public void UpdateReferenceResolution()
    {
        if (canvasScaler == null)
            return;
        lastScreen = new Vector2(Screen.width, Screen.height);
        // Platform is not orientation: a phone can rotate, resize, or run split-screen.
        canvasScaler.referenceResolution = MementoGameplayLayout.ReferenceResolution;
        canvasScaler.matchWidthOrHeight = MementoGameplayLayout.ScaleMatch(lastScreen);
    }
}
