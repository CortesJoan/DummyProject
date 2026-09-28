using NUnit.Framework;
using AnimalMemory.Progression;
using System.Collections.Generic;
using UnityEngine;

[TestFixture]
public class MementoIllustratedCardsTests
{
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    public void GetSprites_IllustratedSet_HasFifteenDistinctStableSquareFaces(int setId)
    {
        var sprites = MementoIllustratedCards.GetSprites(setId);
        Assert.That(sprites.Count, Is.EqualTo(15));
        var textures = new HashSet<Texture2D>();
        Vector2 size = sprites[0].rect.size;
        for (int i = 0; i < sprites.Count; i++)
        {
            Assert.That(sprites[i].name,
                Is.EqualTo(AnimalMemoryCardIdentityCatalog.GetIdentityKey(setId, i)));
            Assert.That(textures.Add(sprites[i].texture), Is.True);
            Assert.That(sprites[i].rect.size, Is.EqualTo(size));
            Assert.That(sprites[i].rect.width, Is.EqualTo(sprites[i].rect.height));
            Assert.That(sprites[i].texture.width, Is.GreaterThan(128),
                "Must load illustrated asset, not procedural fallback.");
        }
        Assert.That(MementoIllustratedCards.GetSprites(setId), Is.SameAs(sprites),
            "Repeated access must not regenerate textures.");
    }
}
