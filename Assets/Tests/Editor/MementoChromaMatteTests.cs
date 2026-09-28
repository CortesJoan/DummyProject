using NUnit.Framework;
using UnityEngine;

public sealed class MementoChromaMatteTests
{
    [TestCase(255, 255, 255)]
    [TestCase(248, 236, 244)]
    [TestCase(255, 204, 170)]
    [TestCase(0, 0, 0)]
    [TestCase(25, 30, 100)]
    [TestCase(255, 255, 0)]
    [TestCase(0, 255, 255)]
    public void Extract_IntentionalColoursIncludingWhites_ArePreserved(int r, int g, int b)
    {
        var colour = new Color32((byte)r, (byte)g, (byte)b, 255);
        var source = new[] { new Color32(0, 255, 0, 255), colour };
        var result = MementoChromaMatte.Extract(source, 2, 1, MementoChromaMatte.Key.Green);
        Assert.That(result[1], Is.EqualTo(colour));
        Assert.That(result[0].a, Is.Zero);
        Assert.That(source[0].a, Is.EqualTo(255), "The original must not be modified.");
    }

    [Test]
    public void Extract_EnclosedBackgroundHole_IsRemovedWithoutEatingWhiteSurround()
    {
        var source = new Color32[25];
        for (int i = 0; i < source.Length; i++) source[i] = new Color32(255, 255, 255, 255);
        source[12] = new Color32(0, 255, 0, 255);
        var result = MementoChromaMatte.Extract(source, 5, 5, MementoChromaMatte.Key.Green);
        Assert.That(result[12].a, Is.Zero);
        for (int i = 0; i < result.Length; i++)
            if (i != 12) Assert.That(result[i], Is.EqualTo(source[i]));
    }

    [Test]
    public void Extract_WhiteAntialiasOnGreen_ReconstructsWhiteWithPartialAlpha()
    {
        var source = new[] {
            new Color32(0,255,0,255), new Color32(128,255,128,255),
            new Color32(255,255,255,255)
        };
        var result = MementoChromaMatte.Extract(source, 3, 1, MementoChromaMatte.Key.Green);
        Assert.That(result[1].r, Is.EqualTo(255));
        Assert.That(result[1].g, Is.EqualTo(255));
        Assert.That(result[1].b, Is.EqualTo(255));
        Assert.That(result[1].a, Is.InRange(127,129));
    }

    [TestCase(MementoChromaMatte.Key.Magenta, 255, 0, 255)]
    [TestCase(MementoChromaMatte.Key.Cyan, 0, 255, 255)]
    public void Extract_DeclaredAlternativeKey_PreservesWhite(MementoChromaMatte.Key key, int r, int g, int b)
    {
        var source = new[] { new Color32((byte)r,(byte)g,(byte)b,255), new Color32(255,255,255,255) };
        var result = MementoChromaMatte.Extract(source,2,1,key);
        Assert.That(result[0].a, Is.Zero);
        Assert.That(result[1], Is.EqualTo(source[1]));
    }

    [TestCase(MementoChromaMatte.Key.Green, 90, 150, 100)]
    [TestCase(MementoChromaMatte.Key.Magenta, 150, 90, 140)]
    [TestCase(MementoChromaMatte.Key.Cyan, 90, 150, 140)]
    public void Extract_OptionalEdgeDespill_ChangesOnlyEdgeColourNotAlpha(
        MementoChromaMatte.Key key, int r, int g, int b)
    {
        var keyPixel = key == MementoChromaMatte.Key.Green ? new Color32(0,255,0,255) :
            key == MementoChromaMatte.Key.Magenta ? new Color32(255,0,255,255) :
            new Color32(0,255,255,255);
        var tinted = new Color32((byte)r, (byte)g, (byte)b, 255);
        var source = new Color32[11];
        for (int i = 0; i < source.Length; i++) source[i] = tinted;
        source[0] = keyPixel;
        var before = MementoChromaMatte.Extract(source, 11, 1, key);
        var after = MementoChromaMatte.Extract(source, 11, 1, key, true);
        Assert.That(after[1], Is.Not.EqualTo(before[1]), "Residual edge tint should be removed.");
        for (int i = 0; i < source.Length; i++)
            Assert.That(after[i].a, Is.EqualTo(before[i].a), "Never erode alpha during despill.");
        for (int i = 4; i < source.Length; i++)
            Assert.That(after[i], Is.EqualTo(before[i]), "Keep interior colour untouched.");
        Assert.That(source[1], Is.EqualTo(tinted), "Do not mutate source pixels.");
    }

    [TestCase(MementoChromaMatte.Key.Green)]
    [TestCase(MementoChromaMatte.Key.Magenta)]
    [TestCase(MementoChromaMatte.Key.Cyan)]
    public void Extract_OptionalEdgeDespill_PreservesNeutralWhites(MementoChromaMatte.Key key)
    {
        var colour = new Color32(255,255,255,255);
        var source = new[] { colour, colour, colour };
        var result = MementoChromaMatte.Extract(source, 3, 1, key, true);
        Assert.That(result, Is.EqualTo(source));
    }

    [Test]
    public void Extract_InvalidDimensions_RejectsInsteadOfCorruptingPixels()
    {
        Assert.Throws<System.ArgumentException>(() =>
            MementoChromaMatte.Extract(new Color32[2],3,1,MementoChromaMatte.Key.Green));
    }
}
