public enum MementoMenuVoiceContext
{
    MenuArrival,
    GuideSelection,
    CollectionSetSelection
}

/// <summary>
/// Keeps character voice feedback tied to character-facing interactions.
/// Deck browsing belongs to the collection and must remain non-verbal.
/// </summary>
public static class MementoMenuVoicePolicy
{
    public static bool ShouldPlayGuideVoice(MementoMenuVoiceContext context)
    {
        return context == MementoMenuVoiceContext.MenuArrival ||
            context == MementoMenuVoiceContext.GuideSelection;
    }
}
