using UnityEngine;

/// <summary>Art-directed face crops in the shipped neutral portrait textures.
/// Coordinates use Unity's bottom-left texture origin, not screen coordinates.</summary>
public static class MementoPortraitFraming
{
    private static readonly Vector2[] Centers =
    {
        new Vector2(.46f, .70f), new Vector2(.54f, .70f),
        new Vector2(.50f, .79f), new Vector2(.50f, .70f),
        new Vector2(.51f, .75f), new Vector2(.61f, .79f)
    };

    public static RectInt FaceRect(int guideId, int width, int height)
    {
        width = Mathf.Max(1, width);
        height = Mathf.Max(1, height);
        Vector2 center = guideId >= 0 && guideId < Centers.Length
            ? Centers[guideId] : new Vector2(.5f, .75f);
        int size = Mathf.Clamp(Mathf.RoundToInt(height * .24f), 1, Mathf.Min(width, height));
        return new RectInt(
            Mathf.Clamp(Mathf.RoundToInt(width * center.x) - size / 2, 0, width - size),
            Mathf.Clamp(Mathf.RoundToInt(height * center.y) - size / 2, 0, height - size),
            size, size);
    }
}
