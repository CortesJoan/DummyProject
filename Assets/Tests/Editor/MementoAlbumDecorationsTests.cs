using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class MementoAlbumDecorationsTests
{
    [TestCase(320, 568)]
    [TestCase(1080, 1920)]
    [TestCase(1920, 1080)]
    [TestCase(2048, 1536)]
    [TestCase(2560, 1080)]
    [TestCase(0, 0)]
    public void GetPlacement_Viewport_KeepsSquareArtInsideBounds(int width, int height)
    {
        for (int i = 0; i < 7; i++)
        {
            Rect rect = MementoAlbumDecorations.GetPlacement(i, new Vector2(width, height));
            Assert.That(rect.width, Is.EqualTo(rect.height));
            Assert.That(rect.xMin, Is.GreaterThanOrEqualTo(0));
            Assert.That(rect.yMin, Is.GreaterThanOrEqualTo(0));
            Assert.That(rect.xMax, Is.LessThanOrEqualTo(width));
            Assert.That(rect.yMax, Is.LessThanOrEqualTo(height));
        }
    }

    [Test]
    public void Constructor_SuppliedArt_RetainsExactSpriteAndIgnoresInput()
    {
        var texture = new Texture2D(8, 8);
        Sprite sprite = Sprite.Create(texture, new Rect(0, 0, 8, 8), Vector2.one * .5f);
        try
        {
            var requestedSets = new List<int>();
            var view = new MementoAlbumDecorations(set =>
            {
                requestedSets.Add(set);
                return new[] { sprite, sprite, sprite };
            });
            Assert.That(requestedSets, Is.EqualTo(new[] { 0, 0, 1, 2, 3, 4, 5 }));
            Assert.That(view.pickingMode, Is.EqualTo(PickingMode.Ignore));
            for (int i = 0; i < 7; i++)
            {
                VisualElement sticker = view.Q<VisualElement>("album-sticker-" + i);
                VisualElement face = sticker.Q<VisualElement>("album-face-" + i);
                Assert.That(face.style.backgroundImage.value.sprite, Is.SameAs(sprite));
                Assert.That(face.pickingMode, Is.EqualTo(PickingMode.Ignore));
                Assert.That(sticker.pickingMode, Is.EqualTo(PickingMode.Ignore));
            }
        }
        finally
        {
            Object.DestroyImmediate(sprite);
            Object.DestroyImmediate(texture);
        }
    }

    [Test]
    public void Constructor_MissingArt_HidesEmptySticker()
    {
        var view = new MementoAlbumDecorations(_ => new Sprite[0]);
        for (int i = 0; i < 7; i++)
            Assert.That(view.Q<VisualElement>("album-sticker-" + i).style.display.value,
                Is.EqualTo(DisplayStyle.None));
    }
}
