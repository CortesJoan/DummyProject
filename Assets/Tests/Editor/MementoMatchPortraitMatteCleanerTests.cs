using System.Linq;
using NUnit.Framework;
using UnityEngine;

public sealed class MementoMatchPortraitMatteCleanerTests
{
    [Test]
    public void CleanupRemovesEdgeConnectedPaleMatteAndKeepsInteriorWhite()
    {
        const int width = 24;
        const int height = 24;
        Color32[] pixels = Enumerable
            .Repeat(new Color32(0, 0, 0, 0), width * height)
            .ToArray();

        Color32 painted = new Color32(72, 48, 112, 255);
        for (int y = 5; y <= 18; y++)
            for (int x = 5; x <= 18; x++)
                pixels[y * width + x] = painted;

        Color32 paleMatte = new Color32(232, 225, 238, 255);
        for (int i = 3; i <= 20; i++)
        {
            pixels[3 * width + i] = paleMatte;
            pixels[20 * width + i] = paleMatte;
            pixels[i * width + 3] = paleMatte;
            pixels[i * width + 20] = paleMatte;
        }

        pixels[11 * width + 11] = new Color32(250, 250, 250, 255);
        pixels[11 * width + 12] = new Color32(250, 250, 250, 255);
        pixels[12 * width + 11] = new Color32(250, 250, 250, 255);
        pixels[12 * width + 12] = new Color32(250, 250, 250, 255);

        MementoMatchPortraitMatteCleaner.CleanPixels(
            pixels,
            width,
            height);

        Assert.That(pixels[3 * width + 8].a, Is.Zero);
        Assert.That(pixels[8 * width + 3].a, Is.Zero);
        Assert.That(pixels[10 * width + 10], Is.EqualTo(painted));
        Assert.That(pixels[11 * width + 11].a, Is.EqualTo(255),
            "An interior eye or white garment detail must not be erased.");
    }

    [Test]
    public void CleanupTraversesThickExteriorMatteWithoutErodingCharacter()
    {
        const int width = 48;
        const int height = 48;
        Color32 transparent = new Color32(0, 0, 0, 0);
        Color32 matte = new Color32(236, 232, 242, 255);
        Color32 painted = new Color32(116, 67, 48, 255);
        Color32[] pixels = Enumerable
            .Repeat(transparent, width * height)
            .ToArray();

        for (int y = 4; y < height - 4; y++)
            for (int x = 4; x < width - 4; x++)
                pixels[y * width + x] = matte;

        for (int y = 18; y <= 30; y++)
            for (int x = 18; x <= 30; x++)
                pixels[y * width + x] = painted;

        pixels[24 * width + 24] = new Color32(250, 250, 250, 255);

        MementoMatchPortraitMatteCleaner.CleanPixels(
            pixels,
            width,
            height);

        Assert.That(pixels[24 * width + 6].a, Is.Zero,
            "Exterior matte thicker than the old ten-pass limit must be removed.");
        Assert.That(pixels[24 * width + 18], Is.EqualTo(painted));
        Assert.That(pixels[24 * width + 24].a, Is.EqualTo(255),
            "A pale detail enclosed by painted pixels must remain.");
    }

    [TestCase("AnimalMemory/Duel/aki-duel-opponents-sheet-4k-v1", 4)]
    [TestCase("AnimalMemory/Duel/memento-duel-opponents-sheet-4k-v1", 8)]
    public void LegacyGuideSheetsCreateTopologyCleanedRuntimeSprites(
        string resourcePath,
        int expectedCount)
    {
        Sprite[] source = Resources.LoadAll<Sprite>(resourcePath);

        Assert.That(source, Has.Length.EqualTo(expectedCount));
        Assert.That(source.All(sprite => sprite.texture.isReadable), Is.True,
            "Runtime topology cleanup requires every legacy atlas to be readable.");

        int totalRemoved = 0;
        foreach (Sprite sprite in source)
        {
            int originalOpaque = CountOpaque(sprite);
            Sprite cleaned =
                MementoMatchPortraitMatteCleaner.CreateCleanedSprite(
                    sprite,
                    true);
            try
            {
                int cleanedOpaque = cleaned.texture
                    .GetPixels32()
                    .Count(pixel => pixel.a > 0);
                int removed = originalOpaque - cleanedOpaque;
                totalRemoved += removed;
                Assert.That(removed, Is.GreaterThanOrEqualTo(0));
                Assert.That(removed, Is.LessThan(originalOpaque * 0.08f),
                    sprite.name + " cleanup must preserve the painted character.");
            }
            finally
            {
                if (cleaned != sprite)
                {
                    Object.DestroyImmediate(cleaned.texture);
                    Object.DestroyImmediate(cleaned);
                }
            }
        }

        Assert.That(totalRemoved, Is.GreaterThan(0),
            resourcePath + " should contain at least one removable generated matte.");
    }

    private static int CountOpaque(Sprite sprite)
    {
        Rect rect = sprite.rect;
        Color32[] atlas = sprite.texture.GetPixels32();
        int atlasWidth = sprite.texture.width;
        int x0 = Mathf.RoundToInt(rect.x);
        int y0 = Mathf.RoundToInt(rect.y);
        int width = Mathf.RoundToInt(rect.width);
        int height = Mathf.RoundToInt(rect.height);
        int count = 0;

        for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
                if (atlas[(y0 + y) * atlasWidth + x0 + x].a > 0)
                    count++;

        return count;
    }
}
