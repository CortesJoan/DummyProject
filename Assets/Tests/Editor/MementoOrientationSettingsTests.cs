using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

[TestFixture]
public sealed class MementoOrientationSettingsTests
{
    private sealed class MemoryStore : IMementoOrientationStore
    {
        public int Value;
        public int Writes;
        public int Read() { return Value; }
        public void Write(int value) { Value = value; Writes++; }
    }

    [TestCase(0, ScreenOrientation.AutoRotation, true, true, true, true)]
    [TestCase(1, ScreenOrientation.Portrait, true, false, false, false)]
    [TestCase(2, ScreenOrientation.AutoRotation, false, false, true, true)]
    public void PolicyFor_Mode_UsesAllowedRotations(int mode, ScreenOrientation requested,
        bool portrait, bool upsideDown, bool left, bool right)
    {
        var policy = MementoOrientationPreferences.PolicyFor((MementoOrientationMode)mode);
        Assert.That(policy.Orientation, Is.EqualTo(requested));
        Assert.That(policy.Portrait, Is.EqualTo(portrait));
        Assert.That(policy.PortraitUpsideDown, Is.EqualTo(upsideDown));
        Assert.That(policy.LandscapeLeft, Is.EqualTo(left));
        Assert.That(policy.LandscapeRight, Is.EqualTo(right));
    }

    [TestCase(-1)]
    [TestCase(3)]
    [TestCase(999)]
    public void Normalize_InvalidSavedValue_FallsBackToAutomatic(int value)
    {
        var store = new MemoryStore { Value = value };
        var preferences = new MementoOrientationPreferences(store, _ => { });
        Assert.That(preferences.Current, Is.EqualTo(MementoOrientationMode.Automatic));
        Assert.That(store.Writes, Is.Zero, "Reading settings must not write progress or preferences.");
    }

    [Test]
    public void Constructor_DefaultPreference_DoesNotApplyUntilInitialized()
    {
        int applications = 0;
        var preferences = new MementoOrientationPreferences(new MemoryStore(), _ => applications++);
        Assert.That(preferences.Current, Is.EqualTo(MementoOrientationMode.Automatic));
        Assert.That(applications, Is.Zero);
        preferences.Apply();
        Assert.That(applications, Is.EqualTo(1));
    }

    [TestCase(MementoOrientationMode.Automatic)]
    [TestCase(MementoOrientationMode.Portrait)]
    [TestCase(MementoOrientationMode.Landscape)]
    public void Select_Preference_PersistsAndAppliesImmediately(MementoOrientationMode mode)
    {
        var store = new MemoryStore();
        var applied = new List<MementoOrientationPolicy>();
        var preferences = new MementoOrientationPreferences(store, applied.Add);
        preferences.Select(mode);
        Assert.That(preferences.Current, Is.EqualTo(mode));
        Assert.That(store.Value, Is.EqualTo((int)mode));
        Assert.That(store.Writes, Is.EqualTo(1));
        Assert.That(applied, Has.Count.EqualTo(1));
        Assert.That(applied[0].Orientation,
            Is.EqualTo(MementoOrientationPreferences.PolicyFor(mode).Orientation));
        var restarted = new MementoOrientationPreferences(store, applied.Add);
        Assert.That(restarted.Current, Is.EqualTo(mode));
        restarted.Apply();
        Assert.That(applied, Has.Count.EqualTo(2));
        Assert.That(store.Writes, Is.EqualTo(1), "Restart must only read the preference.");
    }

    [Test]
    public void Select_LockedThenAutomatic_RestoresAllOriginalRotations()
    {
        var applied = new List<MementoOrientationPolicy>();
        var preferences = new MementoOrientationPreferences(new MemoryStore(), applied.Add);
        preferences.Select(MementoOrientationMode.Portrait);
        preferences.Select(MementoOrientationMode.Landscape);
        preferences.Select(MementoOrientationMode.Automatic);
        var last = applied[2];
        Assert.That(last.Orientation, Is.EqualTo(ScreenOrientation.AutoRotation));
        Assert.That(last.Portrait && last.PortraitUpsideDown && last.LandscapeLeft && last.LandscapeRight, Is.True);
    }
}
