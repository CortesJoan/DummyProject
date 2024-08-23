using UnityEngine;
using UnityEngine.UI;

public class ScreenScaler : MonoBehaviour
{
    [SerializeReference] CanvasScaler canvasScaler;
    [SerializeField] Vector2 defaultMobileResolution = new Vector2(1080, 1920);
    [SerializeField] Vector2 defaultPCResolution = new Vector2(1920, 1080);


    private void Start()
    {
        UpdateReferenceResolution();
    }
    public void UpdateReferenceResolution()
    {
        if (Application.isMobilePlatform)
        {
            canvasScaler.referenceResolution = defaultMobileResolution;

        }
        else
        {
            canvasScaler.referenceResolution = defaultPCResolution;
        }
    }



}