using EHKP.VisualNovel;
using NUnit.Framework;
using UnityEngine.UIElements;

public sealed class VisualNovelPresentationTests
{
    private const string TestKey = "MementoMatch.Tests.PlayerIdentity";

    [SetUp]
    public void SetUp()
    {
        UnityEngine.PlayerPrefs.DeleteKey(TestKey);
    }

    [TearDown]
    public void TearDown()
    {
        UnityEngine.PlayerPrefs.DeleteKey(TestKey);
    }

    [Test]
    public void PlayerIdentity_RejectsTooShortNames()
    {
        VisualNovelPlayerIdentity identity =
            new VisualNovelPlayerIdentity(TestKey, "ASPIRANTE", 16);

        bool accepted = identity.TrySetName(" ! ", out string normalized);

        Assert.That(accepted, Is.False);
        Assert.That(normalized, Is.Empty);
        Assert.That(identity.HasCustomName, Is.False);
        Assert.That(identity.Name, Is.EqualTo("ASPIRANTE"));
    }

    [Test]
    public void PlayerIdentity_NormalizesAndPersistsPlayerName()
    {
        VisualNovelPlayerIdentity identity =
            new VisualNovelPlayerIdentity(TestKey, "ASPIRANTE", 12);

        bool accepted = identity.TrySetName("  Ana   María!!  ", out string normalized);

        Assert.That(accepted, Is.True);
        Assert.That(normalized, Is.EqualTo("Ana María"));
        Assert.That(identity.Name, Is.EqualTo("Ana María"));
        Assert.That(identity.HasCustomName, Is.True);
    }

    [TestCase(800f, 480f, true)]
    [TestCase(1920f, 1080f, false)]
    [TestCase(390f, 844f, false)]
    [TestCase(900f, 600f, false)]
    public void ResponsiveLayout_ClassifiesSmallLandscape(float width, float height, bool expected)
    {
        var root = new VisualElement();
        VisualNovelResponsiveLayout.Apply(root, width, height);
        Assert.That(root.ClassListContains(VisualNovelResponsiveLayout.SmallLandscapeClass),
            Is.EqualTo(expected));
    }

    [Test]
    public void ResponsiveLayout_RotationClearsSmallLandscapeClass()
    {
        var root = new VisualElement();
        VisualNovelResponsiveLayout.Apply(root, 800f, 480f);
        VisualNovelResponsiveLayout.Apply(root, 480f, 800f);
        Assert.That(root.ClassListContains(VisualNovelResponsiveLayout.SmallLandscapeClass), Is.False);
    }

    [Test]
    public void ResponsiveLayout_UsesExclusiveAspectClasses()
    {
        VisualElement root = new VisualElement();

        bool portrait = VisualNovelResponsiveLayout.Apply(root, 390f, 844f);

        Assert.That(portrait, Is.True);
        Assert.That(root.ClassListContains("portrait"), Is.True);
        Assert.That(root.ClassListContains("landscape"), Is.False);

        portrait = VisualNovelResponsiveLayout.Apply(root, 1920f, 1080f);

        Assert.That(portrait, Is.False);
        Assert.That(root.ClassListContains("portrait"), Is.False);
        Assert.That(root.ClassListContains("landscape"), Is.True);
    }
}
