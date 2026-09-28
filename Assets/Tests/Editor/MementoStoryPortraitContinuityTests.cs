using NUnit.Framework;

[TestFixture]
public class MementoStoryPortraitContinuityTests
{
    [Test]
    public void ConsecutiveLines_SameGuardian_DoesNotRepeatEntrance()
    {
        var state = new MementoStoryPortraitContinuity();
        Assert.That(state.ShouldEnter(0, false), Is.True);
        Assert.That(state.ShouldEnter(0, false), Is.False);
        Assert.That(state.ShouldEnter(0, false), Is.False);
    }

    [Test]
    public void NewGuardian_Enters()
    {
        var state = new MementoStoryPortraitContinuity();
        state.ShouldEnter(0, false);
        Assert.That(state.ShouldEnter(1, false), Is.True);
    }

    [Test]
    public void ProtagonistAlone_ShowsNoPortrait_ThenGuardianReenters()
    {
        var state = new MementoStoryPortraitContinuity();
        Assert.That(state.ShouldEnter(-1, false), Is.False);
        state.ShouldEnter(0, false);
        Assert.That(state.ShouldEnter(-1, false), Is.False);
        Assert.That(state.ShouldEnter(0, false), Is.True);
    }

    [Test]
    public void SilhouetteReveal_AnimatesOnce()
    {
        var state = new MementoStoryPortraitContinuity();
        Assert.That(state.ShouldEnter(5, true), Is.True);
        Assert.That(state.ShouldEnter(5, true), Is.False);
        Assert.That(state.ShouldEnter(5, false), Is.True);
        Assert.That(state.ShouldEnter(5, false), Is.False);
    }

    [Test]
    public void Reset_NewSceneWithSameGuardian_EntersAgain()
    {
        var state = new MementoStoryPortraitContinuity();
        state.ShouldEnter(0, false);
        state.Reset();
        Assert.That(state.ShouldEnter(0, false), Is.True);
    }
}
