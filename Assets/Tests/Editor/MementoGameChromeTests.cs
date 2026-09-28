using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class MementoGameChromeTests
{
    [Test]
    public void Install_Twice_PreservesButtonsAndAddsOneNonInteractiveIcon()
    {
        var root = new VisualElement();
        string[] names = { "nav-home", "nav-story", "nav-team", "nav-collection", "nav-settings" };
        foreach (string name in names) root.Add(new Button { name = name, text = name });
        MementoGameChrome.Install(root);
        MementoGameChrome.Install(root);
        foreach (string name in names)
        {
            Button button = root.Q<Button>(name);
            Assert.That(button.text, Is.EqualTo(name));
            Assert.That(button.childCount, Is.EqualTo(1));
            Image image = button.Q<Image>("navigation-art");
            Assert.That(image, Is.Not.Null);
            Assert.That(image.sprite, Is.Not.Null);
            Assert.That(image.pickingMode, Is.EqualTo(PickingMode.Ignore));
        }
    }

    [TestCase(-1f, 0f)]
    [TestCase(0f, 0f)]
    [TestCase(.24f, .875f)]
    [TestCase(.48f, 1f)]
    [TestCase(8f, 1f)]
    public void Entrance_Time_EasesAndClamps(float time, float expected)
    {
        Assert.That(MementoSkillCinematic.Entrance(time), Is.EqualTo(expected).Within(.0001f));
    }

    [Test]
    public void Play_ExplicitOwnerPose_UsesSameSpriteAndStopsCleanly()
    {
        Texture2D texture = new Texture2D(4, 6);
        Sprite pose = Sprite.Create(texture, new Rect(0, 0, 4, 6), Vector2.zero);
        var view = new MementoSkillCinematic();
        try
        {
            view.Play(pose, 5, 3.6f);
            VisualElement actor = view.Q<VisualElement>("skill-character-pose");
            Assert.That(actor.style.backgroundImage.value.sprite, Is.SameAs(pose));
            Assert.That(actor.pickingMode, Is.EqualTo(PickingMode.Ignore));
            view.Stop();
            Assert.That(actor.style.opacity.value, Is.EqualTo(0));
        }
        finally { view.Stop(); Object.DestroyImmediate(pose); Object.DestroyImmediate(texture); }
    }

    [Test]
    public void AlbumDecoration_AlreadyTransparentSprite_HasNoOpaqueBacking()
    {
        var view = new MementoAlbumDecorations(_ => new Sprite[0]);
        for (int i = 0; i < 7; i++)
            Assert.That(view.Q<VisualElement>("album-sticker-" + i).style.backgroundColor.value.a,
                Is.EqualTo(0));
    }
}
