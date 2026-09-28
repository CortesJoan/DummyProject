using System.Collections.Generic;
using System.Linq;
using AnimalMemory.Progression;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;

public sealed class MementoMatchCardArtFactoryTests
{
    [TestCase(AnimalMemoryContentIds.CoralSet)]
    [TestCase(AnimalMemoryContentIds.ConstellationSet)]
    [TestCase(AnimalMemoryContentIds.GardenSet)]
    [TestCase(AnimalMemoryContentIds.SweetsSet)]
    [TestCase(AnimalMemoryContentIds.ClockworkSet)]
    public void ProceduralSetBuildsFifteenNamedDistinctFaces(int setId)
    {
        IReadOnlyList<Sprite> sprites = MementoMatchCardArtFactory.GetSprites(setId);
        IReadOnlyList<string> identities =
            AnimalMemoryCardIdentityCatalog.GetIdentityKeys(setId);

        Assert.That(
            sprites.Count,
            Is.EqualTo(AnimalMemoryCardIdentityCatalog.IdentitiesPerSet));
        Assert.That(
            sprites.Select(sprite => sprite.name).Distinct().Count(),
            Is.EqualTo(AnimalMemoryCardIdentityCatalog.IdentitiesPerSet));
        Assert.That(
            sprites.Select(sprite => sprite.texture).Distinct().Count(),
            Is.EqualTo(AnimalMemoryCardIdentityCatalog.IdentitiesPerSet));

        for (int pairId = 0; pairId < identities.Count; pairId++)
        {
            Assert.That(sprites[pairId], Is.Not.Null);
            Assert.That(sprites[pairId].name, Does.EndWith(identities[pairId]));
        }
    }

    [Test]
    public void DestroyedCachedSpriteIsRebuiltBeforeTheNextBoardUsesIt()
    {
        IReadOnlyList<Sprite> first =
            MementoMatchCardArtFactory.GetSprites(
                AnimalMemoryContentIds.CoralSet);
        Sprite destroyed = first[0];
        Object.DestroyImmediate(destroyed);

        IReadOnlyList<Sprite> rebuilt =
            MementoMatchCardArtFactory.GetSprites(
                AnimalMemoryContentIds.CoralSet);

        Assert.That(rebuilt, Is.Not.SameAs(first));
        Assert.That(rebuilt.Count,
            Is.EqualTo(AnimalMemoryCardIdentityCatalog.IdentitiesPerSet));
        Assert.That(rebuilt.All(sprite => sprite != null && sprite.texture != null),
            Is.True,
            "A stale DontSave sprite must never become a white UI rectangle.");
    }

    [Test]
    public void AllSeventyFiveGeneratedFacesHaveUniquePixelHashes()
    {
        HashSet<ulong> hashes = new HashSet<ulong>();

        for (int setId = AnimalMemoryContentIds.CoralSet;
             setId <= AnimalMemoryContentIds.ClockworkSet;
             setId++)
        {
            IReadOnlyList<Sprite> sprites =
                MementoMatchCardArtFactory.GetSprites(setId);
            for (int identity = 0; identity < sprites.Count; identity++)
            {
                ulong hash = PixelHash(sprites[identity].texture);
                Assert.That(
                    hashes.Add(hash),
                    Is.True,
                    $"Duplicate pixels at set {setId}, identity {identity}.");
            }
        }

        Assert.That(
            hashes.Count,
            Is.EqualTo(5 * AnimalMemoryCardIdentityCatalog.IdentitiesPerSet));
    }

    [TestCase(AnimalMemoryContentIds.CoralSet)]
    [TestCase(AnimalMemoryContentIds.ConstellationSet)]
    [TestCase(AnimalMemoryContentIds.GardenSet)]
    [TestCase(AnimalMemoryContentIds.SweetsSet)]
    [TestCase(AnimalMemoryContentIds.ClockworkSet)]
    public void GeneratedFacesAreTransparentSquareIcons(int setId)
    {
        IReadOnlyList<Sprite> sprites = MementoMatchCardArtFactory.GetSprites(setId);

        foreach (Sprite sprite in sprites)
        {
            Texture2D texture = sprite.texture;
            Assert.That(texture.width, Is.EqualTo(128));
            Assert.That(texture.height, Is.EqualTo(128));
            Assert.That(texture.isReadable, Is.True);

            Color32[] pixels = texture.GetPixels32();
            Assert.That(pixels[0].a, Is.Zero);
            Assert.That(pixels[127].a, Is.Zero);
            Assert.That(pixels[127 * 128].a, Is.Zero);
            Assert.That(pixels[(128 * 128) - 1].a, Is.Zero);
            Assert.That(
                pixels.Count(pixel => pixel.a > 0),
                Is.GreaterThan(900),
                sprite.name + " must contain a substantial visible subject.");
        }
    }

    [TestCase(AnimalMemoryContentIds.CoralSet, 8, "jellyfish bell")]
    [TestCase(AnimalMemoryContentIds.GardenSet, 0, "rose blossom")]
    [TestCase(AnimalMemoryContentIds.GardenSet, 6, "sunflower head")]
    [TestCase(AnimalMemoryContentIds.GardenSet, 8, "mushroom cap")]
    [TestCase(AnimalMemoryContentIds.SweetsSet, 4, "cupcake cream")]
    public void TopHeavySubjectsKeepTheirSemanticMassAboveTheStemOrBase(
        int setId,
        int identity,
        string semanticTop)
    {
        Texture2D texture =
            MementoMatchCardArtFactory.GetSprites(setId)[identity].texture;
        Color32[] pixels = texture.GetPixels32();
        int bottomOpaque = CountOpaqueRows(pixels, 0, 63);
        int topOpaque = CountOpaqueRows(pixels, 64, 127);

        Assert.That(
            topOpaque,
            Is.GreaterThan(bottomOpaque),
            semanticTop + " must remain visually above its support.");
    }

    [TestCase(AnimalMemoryContentIds.CoralSet, 4, false, 500, "diving helmet collar")]
    [TestCase(AnimalMemoryContentIds.CoralSet, 14, false, 500, "treasure chest body")]
    [TestCase(AnimalMemoryContentIds.ConstellationSet, 7, true, 100, "rocket body")]
    [TestCase(AnimalMemoryContentIds.ConstellationSet, 14, false, 500, "observatory base")]
    [TestCase(AnimalMemoryContentIds.GardenSet, 9, false, 500, "garden shears handles")]
    [TestCase(AnimalMemoryContentIds.SweetsSet, 5, true, 500, "lollipop candy")]
    [TestCase(AnimalMemoryContentIds.SweetsSet, 8, false, 250, "jelly base")]
    [TestCase(AnimalMemoryContentIds.ClockworkSet, 8, false, 250, "alarm bell base")]
    public void CorrectedUprightSubjectsPlaceSemanticMassOnExpectedSide(
        int setId,
        int identity,
        bool semanticMassMustBeAbove,
        int minimumDifference,
        string semanticSubject)
    {
        Texture2D texture =
            MementoMatchCardArtFactory.GetSprites(setId)[identity].texture;
        Color32[] pixels = texture.GetPixels32();
        int bottomOpaque = CountOpaqueRows(pixels, 0, 47);
        int topOpaque = CountOpaqueRows(pixels, 80, 127);
        int signedDifference = semanticMassMustBeAbove
            ? topOpaque - bottomOpaque
            : bottomOpaque - topOpaque;

        Assert.That(
            signedDifference,
            Is.GreaterThan(minimumDifference),
            semanticSubject + " must remain upright in the generated sprite used by gameplay.");
    }

    [Test]
    public void MonsteraStemExtendsBelowItsLeaf()
    {
        Color32[] pixels = MementoMatchCardArtFactory
            .GetSprites(AnimalMemoryContentIds.GardenSet)[12]
            .texture.GetPixels32();

        Assert.That(pixels[(18 * 128) + 64].a, Is.GreaterThan(0),
            "The monstera stem must extend below the leaf.");
        Assert.That(pixels[(120 * 128) + 64].a, Is.Zero,
            "The monstera stem must not protrude above the leaf.");
    }

    [Test]
    public void SweetsFacesKeepEyesAboveMouth()
    {
        Texture2D texture =
            MementoMatchCardArtFactory
                .GetSprites(AnimalMemoryContentIds.SweetsSet)[6]
                .texture;
        Color32[] pixels = texture.GetPixels32();

        int eyeHighlights = 0;
        for (int y = 91; y <= 101; y++)
            for (int x = 47; x <= 81; x++)
            {
                Color32 pixel = pixels[y * 128 + x];
                if (pixel.r > 240 && pixel.g > 240 && pixel.b > 240 && pixel.a > 0)
                    eyeHighlights++;
            }

        Assert.That(eyeHighlights, Is.GreaterThan(20),
            "Facial eye highlights must stay above the mouth in gameplay art.");
    }

    [Test]
    public void FivePointStarAndCakeFlamePointUp()
    {
        Color32[] star = MementoMatchCardArtFactory
            .GetSprites(AnimalMemoryContentIds.SweetsSet)[11]
            .texture.GetPixels32();
        Color32[] cake = MementoMatchCardArtFactory
            .GetSprites(AnimalMemoryContentIds.SweetsSet)[14]
            .texture.GetPixels32();

        Assert.That(star[(115 * 128) + 64].a, Is.GreaterThan(0));
        Assert.That(star[(13 * 128) + 64].a, Is.Zero,
            "A five-point star must not retain its long point at the bottom.");
        Assert.That(cake[(122 * 128) + 64].a, Is.GreaterThan(0),
            "The celebration-cake flame must point upward without clipping.");
    }

    [Test]
    public void WateringCanDropletsFallBelowTheSpout()
    {
        Texture2D texture =
            MementoMatchCardArtFactory
                .GetSprites(AnimalMemoryContentIds.GardenSet)[7]
                .texture;
        Color32[] pixels = texture.GetPixels32();

        int belowSpout = CountOpaqueRegion(pixels, 88, 0, 127, 79);
        int aboveSpout = CountOpaqueRegion(pixels, 88, 80, 127, 127);

        Assert.That(
            belowSpout,
            Is.GreaterThan(aboveSpout + 250),
            "Watering-can droplets must fall down from the spout, not rise above it.");
    }

    [TestCase(AnimalMemoryContentIds.CoralSet)]
    [TestCase(AnimalMemoryContentIds.ConstellationSet)]
    [TestCase(AnimalMemoryContentIds.GardenSet)]
    [TestCase(AnimalMemoryContentIds.SweetsSet)]
    [TestCase(AnimalMemoryContentIds.ClockworkSet)]
    public void PairAssignmentUsesSameFaceTwiceAndDifferentFacePerPair(int setId)
    {
        IReadOnlyList<Sprite> sprites = MementoMatchCardArtFactory.GetSprites(setId);
        List<int> pairedIds = AnimalMemoryPairBuilder.BuildPairedIndices(
            AnimalMemoryCardIdentityCatalog.IdentitiesPerSet);

        for (int pairId = 0;
             pairId < AnimalMemoryCardIdentityCatalog.IdentitiesPerSet;
             pairId++)
        {
            Sprite[] assignedFaces = pairedIds
                .Where(candidate => candidate == pairId)
                .Select(candidate => sprites[candidate])
                .ToArray();

            Assert.That(assignedFaces, Has.Length.EqualTo(2));
            Assert.That(assignedFaces[0], Is.SameAs(assignedFaces[1]));
        }

        Assert.That(
            pairedIds
                .Select(pairId => sprites[pairId])
                .Distinct()
                .Count(),
            Is.EqualTo(AnimalMemoryCardIdentityCatalog.IdentitiesPerSet));
    }

    [Test]
    public void CardResetVisualOrientationClearsInheritedFlips()
    {
        GameObject root = new GameObject(
            "Card under test",
            typeof(RectTransform),
            typeof(LayoutElement),
            typeof(Card));
        GameObject face = new GameObject(
            "Face",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));

        try
        {
            face.transform.SetParent(root.transform, false);
            Card card = root.GetComponent<Card>();
            Image image = face.GetComponent<Image>();
            LayoutElement layout = root.GetComponent<LayoutElement>();
            const System.Reflection.BindingFlags flags =
                System.Reflection.BindingFlags.Instance |
                System.Reflection.BindingFlags.NonPublic;
            typeof(Card).GetField("cardRenderer", flags).SetValue(card, image);
            typeof(Card).GetField("layoutElement", flags).SetValue(card, layout);

            root.transform.localRotation = Quaternion.Euler(0f, 0f, 180f);
            root.transform.localScale = new Vector3(-1f, -1f, 1f);
            image.rectTransform.localRotation = Quaternion.Euler(0f, 0f, 180f);
            image.rectTransform.localScale = new Vector3(1f, -1f, 1f);
            image.preserveAspect = false;

            card.ResetVisualOrientation();

            Assert.That(root.transform.localRotation, Is.EqualTo(Quaternion.identity));
            Assert.That(root.transform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(
                image.rectTransform.localRotation,
                Is.EqualTo(Quaternion.identity));
            Assert.That(image.rectTransform.localScale, Is.EqualTo(Vector3.one));
            Assert.That(image.preserveAspect, Is.True);
        }
        finally
        {
            Object.DestroyImmediate(root);
        }
    }

    private static int CountOpaqueRows(
        IReadOnlyList<Color32> pixels,
        int minimumY,
        int maximumY)
    {
        int count = 0;
        for (int y = minimumY; y <= maximumY; y++)
            for (int x = 0; x < 128; x++)
                if (pixels[(y * 128) + x].a > 0)
                    count++;

        return count;
    }

    private static int CountOpaqueRegion(
        IReadOnlyList<Color32> pixels,
        int minimumX,
        int minimumY,
        int maximumX,
        int maximumY)
    {
        int count = 0;
        for (int y = minimumY; y <= maximumY; y++)
            for (int x = minimumX; x <= maximumX; x++)
                if (pixels[(y * 128) + x].a > 0)
                    count++;

        return count;
    }

    private static ulong PixelHash(Texture2D texture)
    {
        const ulong offset = 1469598103934665603UL;
        const ulong prime = 1099511628211UL;
        ulong hash = offset;

        foreach (Color32 pixel in texture.GetPixels32())
        {
            hash = (hash ^ pixel.r) * prime;
            hash = (hash ^ pixel.g) * prime;
            hash = (hash ^ pixel.b) * prime;
            hash = (hash ^ pixel.a) * prime;
        }

        return hash;
    }
}
