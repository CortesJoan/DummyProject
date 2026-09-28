using NUnit.Framework;

public sealed class AnimalMemoryJuiceThemeTests
{
    [TestCase(-10, 0)]
    [TestCase(-1, 0)]
    [TestCase(0, 0)]
    [TestCase(1, 1)]
    [TestCase(2, 2)]
    [TestCase(3, 3)]
    [TestCase(4, 4)]
    [TestCase(5, 5)]
    [TestCase(99, 5)]
    public void ClampTheme_AlwaysReturnsSupportedIndex(int input, int expected)
    {
        Assert.That(AnimalMemoryJuiceTheme.ClampTheme(input), Is.EqualTo(expected));
    }

    [TestCase(0, AnimalMemoryParticleShape.Leaf)]
    [TestCase(1, AnimalMemoryParticleShape.Bubble)]
    [TestCase(2, AnimalMemoryParticleShape.Moon)]
    [TestCase(3, AnimalMemoryParticleShape.Flower)]
    [TestCase(4, AnimalMemoryParticleShape.Candy)]
    [TestCase(5, AnimalMemoryParticleShape.Gear)]
    public void GetStyle_UsesCollectionSpecificAccent(
        int themeIndex,
        AnimalMemoryParticleShape expected)
    {
        Assert.That(
            AnimalMemoryJuiceTheme.GetStyle(themeIndex).AccentShape,
            Is.EqualTo(expected));
    }

    [TestCase(0, AnimalMemoryParticleShape.Paw, AnimalMemoryParticleShape.Leaf)]
    [TestCase(1, AnimalMemoryParticleShape.Bubble, AnimalMemoryParticleShape.Star)]
    [TestCase(2, AnimalMemoryParticleShape.Moon, AnimalMemoryParticleShape.Star)]
    [TestCase(3, AnimalMemoryParticleShape.Flower, AnimalMemoryParticleShape.Leaf)]
    [TestCase(4, AnimalMemoryParticleShape.Candy, AnimalMemoryParticleShape.Star)]
    [TestCase(5, AnimalMemoryParticleShape.Gear, AnimalMemoryParticleShape.Star)]
    public void GetShapeForSequence_UsesOnlyTheSelectedCollectionLanguage(
        int themeIndex,
        AnimalMemoryParticleShape first,
        AnimalMemoryParticleShape second)
    {
        Assert.That(
            AnimalMemoryJuiceTheme.GetShapeForSequence(themeIndex, 0),
            Is.EqualTo(first));
        Assert.That(
            AnimalMemoryJuiceTheme.GetShapeForSequence(themeIndex, 1),
            Is.EqualTo(second));
        Assert.That(
            AnimalMemoryJuiceTheme.GetShapeForSequence(themeIndex, 2),
            Is.EqualTo(first));
        Assert.That(
            AnimalMemoryJuiceTheme.GetShapeForSequence(themeIndex, 3),
            Is.EqualTo(second));
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    public void GetStyle_ProvidesOpaqueReadablePalette(int themeIndex)
    {
        AnimalMemoryJuiceStyle style = AnimalMemoryJuiceTheme.GetStyle(themeIndex);

        Assert.That(style.Primary.a, Is.EqualTo(1f));
        Assert.That(style.Secondary.a, Is.EqualTo(1f));
        Assert.That(style.Highlight.a, Is.EqualTo(1f));
        Assert.That(style.Fail.a, Is.EqualTo(1f));
        Assert.That(style.Primary, Is.Not.EqualTo(style.Secondary));
        Assert.That(style.Fail, Is.Not.EqualTo(style.Primary));
    }
}
