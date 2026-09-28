using System.Collections.Generic;
using AnimalMemory.Progression;
using NUnit.Framework;

public sealed class GuidePowerPresentationTests
{
    [Test]
    public void MikaEmitsCastThenProcWhenShieldActuallyProtectsCombo()
    {
        AnimalMemoryGuideRunState state = new AnimalMemoryGuideRunState();
        state.Reset(AnimalMemoryContentIds.MikaGuide);
        List<bool> phases = new List<bool>();
        state.PowerPresentationRequested += (_, resolved) => phases.Add(resolved);

        Assert.That(state.TryActivatePower(), Is.True);
        Assert.That(state.HasPendingPower, Is.True);
        Assert.That(phases, Is.EqualTo(new[] { false }));

        Assert.That(state.TryProtectCombo(0), Is.False);
        Assert.That(phases, Is.EqualTo(new[] { false }));

        Assert.That(state.TryProtectCombo(2), Is.True);
        Assert.That(state.HasPendingPower, Is.False);
        Assert.That(phases, Is.EqualTo(new[] { false, true }));
    }

    [Test]
    public void MomoEmitsProcOnlyWhenEncoreIsConsumed()
    {
        AnimalMemoryGuideRunState state = new AnimalMemoryGuideRunState();
        state.Reset(AnimalMemoryContentIds.MomoGuide);
        List<bool> phases = new List<bool>();
        state.PowerPresentationRequested += (_, resolved) => phases.Add(resolved);

        Assert.That(state.TryActivatePower(), Is.True);
        Assert.That(phases, Is.EqualTo(new[] { false }));
        Assert.That(state.TryConsumeEncore(), Is.True);
        Assert.That(phases, Is.EqualTo(new[] { false, true }));
        Assert.That(state.TryConsumeEncore(), Is.False);
    }

    [Test]
    public void ImmediateGuidePowerHasCastButNoPendingProc()
    {
        AnimalMemoryGuideRunState state = new AnimalMemoryGuideRunState();
        state.Reset(AnimalMemoryContentIds.YoruGuide);
        List<bool> phases = new List<bool>();
        state.PowerPresentationRequested += (_, resolved) => phases.Add(resolved);

        Assert.That(state.TryActivatePower(), Is.True);

        Assert.That(state.HasPendingPower, Is.False);
        Assert.That(phases, Is.EqualTo(new[] { false }));
    }
}
