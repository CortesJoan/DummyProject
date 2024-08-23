using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
public class OrientationChangeNotifier : MonoBehaviour
{
    public float verticalMatch = 1f;
    public float horizontalMatch = 0f;
    [SerializeReference] CanvasScaler canvasScaler;
    
    void OnRectTransformDimensionsChange()
    {

        ScreenOrientation orientation = Screen.orientation;

        canvasScaler.matchWidthOrHeight = (orientation == ScreenOrientation.Portrait ||
                                            orientation == ScreenOrientation.PortraitUpsideDown) ? verticalMatch : horizontalMatch;
    }
}
