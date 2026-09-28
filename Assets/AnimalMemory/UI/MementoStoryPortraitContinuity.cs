/// <summary>
/// Presentation-only continuity for the shared VN animator. Does not advance dialogue,
/// own scenes, or replace the shared visual novel engine.
/// </summary>
public sealed class MementoStoryPortraitContinuity
{
    private int previousGuide = -1;
    private bool previousSilhouette;

    public bool ShouldEnter(int guideId, bool silhouette)
    {
        bool enter = guideId >= 0 &&
            (guideId != previousGuide || silhouette != previousSilhouette);
        previousGuide = guideId;
        previousSilhouette = silhouette;
        return enter;
    }

    public void Reset()
    {
        previousGuide = -1;
        previousSilhouette = false;
    }
}
