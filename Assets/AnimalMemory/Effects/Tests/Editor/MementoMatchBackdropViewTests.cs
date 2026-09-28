using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public sealed class MementoMatchBackdropViewTests
{
    [TestCase(0, "Forest", "Leaf")]
    [TestCase(1, "Coral", "Coral")]
    [TestCase(2, "Astral", "Star")]
    public void ApplyTheme_CreatesThemeSpecificAnimatedMotifs(
        int themeId,
        string themeName,
        string firstMotifName)
    {
        GameObject targetObject = new GameObject(
            "Backdrop Target",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        GameObject viewObject = new GameObject("Backdrop View");
        MementoMatchBackdropView view =
            viewObject.AddComponent<MementoMatchBackdropView>();
        Image target = targetObject.GetComponent<Image>();

        try
        {
            view.Bind(target);
            view.ApplyTheme(themeId, true);

            Assert.That(target.sprite, Is.Not.Null);
            Assert.That(target.sprite.name, Does.Contain(themeId.ToString()));
            Assert.That(target.rectTransform.childCount, Is.EqualTo(10));
            Assert.That(
                target.rectTransform.GetChild(0).name,
                Does.Contain(themeName).And.Contain(firstMotifName));

            Image motif =
                target.rectTransform.GetChild(0).GetComponent<Image>();
            Assert.That(motif, Is.Not.Null);
            Assert.That(motif.sprite, Is.Not.Null);
            Assert.That(motif.raycastTarget, Is.False);
        }
        finally
        {
            Object.DestroyImmediate(viewObject);
            Object.DestroyImmediate(targetObject);
        }
    }
}
