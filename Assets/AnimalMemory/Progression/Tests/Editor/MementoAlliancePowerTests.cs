using AnimalMemory.Progression;
using NUnit.Framework;

public sealed class MementoAlliancePowerTests
{
    [Test]
    public void EnableAlliance_OnlyPresentCompanions_ExcludesAki()
    {
        var state = new AnimalMemoryGuideRunState();
        state.Reset(0);
        state.EnableAlliance((1 << 0) | (1 << 2));
        Assert.That(state.AllianceMask, Is.EqualTo(1 << 2));
        Assert.That(state.SelectedGuideId, Is.EqualTo(2));
        Assert.That(state.SelectAllianceGuide(0), Is.False);
        Assert.That(state.SelectAllianceGuide(1), Is.False);
    }

    [Test]
    public void Activate_SecondAllySameTurn_IsRejected()
    {
        var state = new AnimalMemoryGuideRunState();
        state.Reset(1); state.EnableAlliance(30);
        Assert.That(state.TryActivatePower(), Is.True);
        state.SelectAllianceGuide(2);
        Assert.That(state.TryActivatePower(), Is.False);
        state.RegisterTurnCompleted();
        Assert.That(state.TryActivatePower(), Is.True);
        Assert.That(state.GetAllianceCooldown(1), Is.EqualTo(5));
        Assert.That(state.GetAllianceCooldown(2), Is.EqualTo(7));
    }

    [Test]
    public void SwitchGuide_ArmedShield_RemainsUsableAndReportsMika()
    {
        var state = new AnimalMemoryGuideRunState();
        state.Reset(1); state.EnableAlliance(30);
        state.TryActivatePower(); state.RegisterTurnCompleted();
        state.SelectAllianceGuide(2);
        int resolved = -1;
        state.PowerPresentationRequested += (id, proc) => { if (proc) resolved = id; };
        Assert.That(state.TryProtectCombo(3), Is.True);
        Assert.That(resolved, Is.EqualTo(1));
        Assert.That(state.MikaProtectionArmed, Is.False);
    }

    [Test]
    public void SwitchGuide_Encore_RemainsUsableAndReportsMomo()
    {
        var state = new AnimalMemoryGuideRunState();
        state.Reset(4); state.EnableAlliance(30);
        state.TryActivatePower(); state.SelectAllianceGuide(2);
        int resolved = -1;
        state.PowerPresentationRequested += (id, proc) => { if (proc) resolved = id; };
        Assert.That(state.TryProtectCombo(2), Is.True);
        Assert.That(state.TryConsumeEncore(), Is.True);
        Assert.That(state.TryConsumeEncore(), Is.False);
        Assert.That(resolved, Is.EqualTo(4));
    }

    [Test]
    public void EnableAlliance_ExistingCooldown_IsNotRefilled()
    {
        var state = new AnimalMemoryGuideRunState();
        state.Reset(2); state.TryActivatePower(); state.RegisterTurnCompleted();
        state.EnableAlliance(30);
        Assert.That(state.GetAllianceCooldown(2), Is.EqualTo(6));
        state.EnableAlliance(30);
        Assert.That(state.GetAllianceCooldown(2), Is.EqualTo(6));
    }

    [Test]
    public void Reset_NewRun_RemovesAllianceAndPendingEffects()
    {
        var state = new AnimalMemoryGuideRunState();
        state.Reset(1); state.EnableAlliance(30); state.TryActivatePower();
        state.Reset(2);
        Assert.That(state.IsAllianceActive, Is.False);
        Assert.That(state.HasPendingPower, Is.False);
        Assert.That(state.AlliancePowerUsedThisTurn, Is.False);
        Assert.That(state.PowerCooldownRemaining, Is.Zero);
    }
}
