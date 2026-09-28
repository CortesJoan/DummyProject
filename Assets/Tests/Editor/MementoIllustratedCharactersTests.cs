using NUnit.Framework;

[TestFixture]
public class MementoIllustratedCharactersTests
{
    [TestCase(0, "neutral")]
    [TestCase(1, "thinking")]
    [TestCase(2, "ability")]
    [TestCase(3, "defeated")]
    [TestCase(4, "victory")]
    [TestCase(5, "damage")]
    public void StateForPose_KnownPose_PreservesPresentationMeaning(int pose, string expected)
    {
        Assert.That(MementoIllustratedCharacters.StateForPose(pose), Is.EqualTo(expected));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("not-a-pose")]
    public void GetSprite_InvalidState_DoesNotLoadAnyPose(string state)
    {
        Assert.That(MementoIllustratedCharacters.GetSprite(5, state), Is.Null);
    }

    [Test]
    public void OwnsTexture_UnrelatedTexture_IsNotBorrowed()
    {
        var texture = new UnityEngine.Texture2D(2, 2);
        try { Assert.That(MementoIllustratedCharacters.OwnsTexture(texture), Is.False); }
        finally { UnityEngine.Object.DestroyImmediate(texture); }
        Assert.That(MementoIllustratedCharacters.OwnsTexture(null), Is.False);
    }

    [Test]
    public void MenuDestroy_BorrowedEarlyGuideTexture_DoesNotDestroySharedArt()
    {
        var sprite = MementoIllustratedCharacters.GetSprite(5, "neutral");
        Assert.That(sprite, Is.Not.Null);
        var host = new UnityEngine.GameObject("Character ownership test");
        host.SetActive(false);
        try
        {
            var view = host.AddComponent<AnimalMemoryMenuController>();
            var flags = System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic;
            var portraits = (System.Collections.Generic.Dictionary<int, UnityEngine.Texture2D>)
                typeof(AnimalMemoryMenuController).GetField("guideTextures", flags).GetValue(view);
            // Early guide IDs previously destroyed every texture regardless of owner.
            portraits[0] = sprite.texture;
            typeof(AnimalMemoryMenuController).GetMethod("OnDestroy", flags).Invoke(view, null);
            Assert.That(sprite.texture != null, Is.True);
            Assert.That(MementoIllustratedCharacters.GetSprite(5, "neutral"), Is.SameAs(sprite));
            Assert.That(portraits, Is.Empty);
        }
        finally { UnityEngine.Object.DestroyImmediate(host); }
    }

    [TestCase(0, "aki-poses-magenta-v2", MementoChromaMatte.Key.Magenta)]
    [TestCase(1, "mika-poses-green-v2", MementoChromaMatte.Key.Green)]
    [TestCase(2, "yoru-poses-green-v2", MementoChromaMatte.Key.Green)]
    [TestCase(3, "hana-poses-magenta-v2", MementoChromaMatte.Key.Magenta)]
    [TestCase(4, "momo-poses-green-v2", MementoChromaMatte.Key.Green)]
    public void NormalizeFrames_PreservesEveryPaintedPixel(int guide, string path, MementoChromaMatte.Key key)
    {
        // Load cache first because it releases its imported source after extraction.
        string[] states = { "neutral", "thinking", "ability", "damage", "defeated", "victory" };
        long actual = 0;
        foreach (string state in states)
        {
            var sprite = MementoIllustratedCharacters.GetSprite(guide, state);
            Assert.That(sprite, Is.Not.Null, state);
            foreach (var pixel in sprite.texture.GetPixels32())
                if (pixel.a != 0) actual++;
        }
        var source = UnityEngine.Resources.Load<UnityEngine.Texture2D>("AnimalMemory/ArtV2/" + path);
        Assert.That(source, Is.Not.Null);
        var matte = MementoChromaMatte.CreateTexture(source, key);
        try
        {
            long expected = 0;
            foreach (var pixel in matte.GetPixels32())
                if (pixel.a != 0) expected++;
            Assert.That(actual, Is.EqualTo(expected), "Normalization must not cut paint or duplicate neighboring poses.");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(matte);
            UnityEngine.Resources.UnloadAsset(source);
        }
    }

    [TestCase(-1)]
    [TestCase(AnimalMemory.Progression.MementoPostgameEncounters.LastOpponentId + 1)]
    [TestCase(int.MaxValue)]
    public void GetSprite_UnregisteredGuide_KeepsLegacyFallback(int guide)
    {
        Assert.That(MementoIllustratedCharacters.IsRegistered(guide), Is.False);
        Assert.That(MementoIllustratedCharacters.GetSprite(guide, "neutral"), Is.Null);
    }

    [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
    [TestCase(AnimalMemory.Progression.MementoPostgameEncounters.FirstOpponentId)]
    [TestCase(AnimalMemory.Progression.MementoPostgameEncounters.FirstOpponentId + 1)]
    [TestCase(AnimalMemory.Progression.MementoPostgameEncounters.FirstOpponentId + 2)]
    [TestCase(AnimalMemory.Progression.MementoPostgameEncounters.FirstOpponentId + 3)]
    [TestCase(AnimalMemory.Progression.MementoPostgameEncounters.LastOpponentId)]
    public void IsRegistered_CoversGuidesAndPostgameOpponents(int guide)
    {
        Assert.That(MementoIllustratedCharacters.IsRegistered(guide), Is.True);
        Assert.That(MementoIllustratedCharacters.GetSheetSource(guide), Is.Not.Null.And.Not.Empty);
    }

    /// <summary>
    /// Postgame opponents now own a sheet slot. Until the reviewed sheet is dropped into
    /// Resources the cache must keep returning null so the duel falls back to legacy art,
    /// and the moment the asset exists it must serve all six poses. Asserting against the
    /// asset's actual presence keeps this honest without hard-coding which art shipped.
    /// </summary>
    [TestCase(AnimalMemory.Progression.MementoPostgameEncounters.FirstOpponentId)]
    [TestCase(AnimalMemory.Progression.MementoPostgameEncounters.FirstOpponentId + 1)]
    [TestCase(AnimalMemory.Progression.MementoPostgameEncounters.FirstOpponentId + 2)]
    [TestCase(AnimalMemory.Progression.MementoPostgameEncounters.FirstOpponentId + 3)]
    [TestCase(AnimalMemory.Progression.MementoPostgameEncounters.LastOpponentId)]
    public void PostgameOpponentServesItsSheetExactlyWhenTheAssetExists(int guide)
    {
        string source = MementoIllustratedCharacters.GetSheetSource(guide);
        var asset = UnityEngine.Resources.Load<UnityEngine.Texture2D>("AnimalMemory/ArtV2/" + source);
        try
        {
            var sprite = MementoIllustratedCharacters.GetSprite(guide, "neutral");
            if (asset == null)
            {
                Assert.That(sprite, Is.Null,
                    "Sin hoja revisada el duelo debe seguir con el arte legacy, no con un candidato.");
                return;
            }
            foreach (string state in new[]
                     { "neutral", "thinking", "ability", "damage", "defeated", "victory" })
                Assert.That(MementoIllustratedCharacters.GetSprite(guide, state), Is.Not.Null, state);
        }
        finally
        {
            if (asset != null) UnityEngine.Resources.UnloadAsset(asset);
        }
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    public void GetSprite_ProductionGuide_LoadsSixDistinctEquallySizedTransparentPoses(int guide)
    {
        var textures = new System.Collections.Generic.HashSet<UnityEngine.Texture2D>();
        UnityEngine.Vector2 size = UnityEngine.Vector2.zero;
        foreach (string state in new[] { "neutral", "thinking", "ability", "damage", "defeated", "victory" })
        {
            var sprite = MementoIllustratedCharacters.GetSprite(guide, state);
            Assert.That(sprite, Is.Not.Null, state);
            Assert.That(textures.Add(sprite.texture), Is.True, state);
            Assert.That(MementoIllustratedCharacters.OwnsTexture(sprite.texture), Is.True, state);
            Assert.That(MementoIllustratedCharacters.GetSprite(guide, state), Is.SameAs(sprite), state);
            if (size == UnityEngine.Vector2.zero) size = sprite.rect.size;
            Assert.That(sprite.rect.size, Is.EqualTo(size), state);
            if (guide != 5)
                Assert.That(sprite.rect.width / sprite.rect.height,
                    Is.EqualTo(2f / 3f).Within(0.002f), "Shared full-body frame");
            var bounds = sprite.texture.GetPixels32();
            if (guide != 5)
            {
                int w = sprite.texture.width, h = sprite.texture.height;
                for (int x = 0; x < w; x++)
                {
                    Assert.That(bounds[x].a, Is.Zero, "No paint clipped at bottom");
                    Assert.That(bounds[(h - 1) * w + x].a, Is.Zero, "No paint clipped at top");
                }
                for (int y = 0; y < h; y++)
                {
                    Assert.That(bounds[y * w].a, Is.Zero, "No paint clipped at left");
                    Assert.That(bounds[y * w + w - 1].a, Is.Zero, "No paint clipped at right");
                }
            }
            var pixels = sprite.texture.GetPixels32();
            Assert.That(System.Array.Exists(pixels, p => p.a == 0), Is.True, state);
            Assert.That(System.Array.Exists(pixels, p => p.a == 255 && p.r > 220 && p.g > 220 && p.b > 220),
                Is.True, "Intentional whites must survive in " + state);
        }
    }
}
