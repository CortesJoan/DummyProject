using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

/// <summary>Reports the actual viewport shape; Screen.orientation may remain AutoRotation.</summary>
public class OrientationChangeNotifier : MonoBehaviour
{
    public float verticalMatch = 1f;
    public float horizontalMatch = 0.5f;
    [SerializeReference] private CanvasScaler canvasScaler;
    public UnityEvent<ScreenOrientation> onOrientationChanged;
    private Vector2 lastScreen;

    private void Update()
    {
        Vector2 current = new Vector2(Screen.width, Screen.height);
        if (current == lastScreen || current.x <= 0f || current.y <= 0f)
            return;
        lastScreen = current;
        // ScreenScaler owns scaling. Do not mutate CanvasScaler from a rect
        // change callback: that callback also fires as the scaler changes its rect.
        ScreenOrientation orientation = MementoGameplayLayout.IsPortrait(current)
            ? ScreenOrientation.Portrait
            : ScreenOrientation.LandscapeLeft;
        onOrientationChanged?.Invoke(orientation);
    }
}
