using NUnit.Framework;

public sealed class MementoMenuVoicePolicyTests
{
    [TestCase(MementoMenuVoiceContext.MenuArrival, true)]
    [TestCase(MementoMenuVoiceContext.GuideSelection, true)]
    [TestCase(MementoMenuVoiceContext.CollectionSetSelection, false)]
    public void GuideVoiceOnlyPlaysInCharacterFacingContexts(
        MementoMenuVoiceContext context,
        bool expected)
    {
        Assert.That(
            MementoMenuVoicePolicy.ShouldPlayGuideVoice(context),
            Is.EqualTo(expected));
    }
}
