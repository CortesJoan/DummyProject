using UnityEngine;

public enum AnimalMemoryParticleShape
{
    Paw,
    Leaf,
    Bubble,
    Moon,
    Star,
    Flower,
    Candy,
    Gear
}

public readonly struct AnimalMemoryJuiceStyle
{
    public AnimalMemoryJuiceStyle(Color primary, Color secondary, Color highlight, Color fail, AnimalMemoryParticleShape accentShape)
    {
        Primary = primary;
        Secondary = secondary;
        Highlight = highlight;
        Fail = fail;
        AccentShape = accentShape;
    }

    public Color Primary { get; }
    public Color Secondary { get; }
    public Color Highlight { get; }
    public Color Fail { get; }
    public AnimalMemoryParticleShape AccentShape { get; }
}

public static class AnimalMemoryJuiceTheme
{
    public const int ThemeCount = 6;

    private static readonly AnimalMemoryJuiceStyle[] Styles =
    {
        new AnimalMemoryJuiceStyle(
            new Color(0.20f, 0.78f, 0.43f, 1f),
            new Color(0.95f, 0.75f, 0.25f, 1f),
            new Color(0.73f, 1f, 0.72f, 1f),
            new Color(0.96f, 0.38f, 0.30f, 1f),
            AnimalMemoryParticleShape.Leaf),
        new AnimalMemoryJuiceStyle(
            new Color(1f, 0.34f, 0.35f, 1f),
            new Color(0.18f, 0.84f, 0.91f, 1f),
            new Color(1f, 0.84f, 0.56f, 1f),
            new Color(0.72f, 0.20f, 0.32f, 1f),
            AnimalMemoryParticleShape.Bubble),
        new AnimalMemoryJuiceStyle(
            new Color(0.48f, 0.54f, 1f, 1f),
            new Color(0.95f, 0.76f, 0.28f, 1f),
            new Color(0.75f, 0.87f, 1f, 1f),
            new Color(0.88f, 0.31f, 0.52f, 1f),
            AnimalMemoryParticleShape.Moon),
        new AnimalMemoryJuiceStyle(
            new Color(0.40f, 0.86f, 0.46f, 1f),
            new Color(1f, 0.48f, 0.68f, 1f),
            new Color(1f, 0.91f, 0.50f, 1f),
            new Color(0.79f, 0.26f, 0.39f, 1f),
            AnimalMemoryParticleShape.Flower),
        new AnimalMemoryJuiceStyle(
            new Color(1f, 0.38f, 0.68f, 1f),
            new Color(0.68f, 0.31f, 0.92f, 1f),
            new Color(1f, 0.86f, 0.37f, 1f),
            new Color(0.70f, 0.20f, 0.45f, 1f),
            AnimalMemoryParticleShape.Candy),
        new AnimalMemoryJuiceStyle(
            new Color(0.90f, 0.60f, 0.22f, 1f),
            new Color(0.32f, 0.77f, 0.82f, 1f),
            new Color(1f, 0.91f, 0.61f, 1f),
            new Color(0.68f, 0.24f, 0.18f, 1f),
            AnimalMemoryParticleShape.Gear)
    };

    public static int ClampTheme(int themeIndex)
    {
        return Mathf.Clamp(themeIndex, 0, ThemeCount - 1);
    }

    public static AnimalMemoryJuiceStyle GetStyle(int themeIndex)
    {
        return Styles[ClampTheme(themeIndex)];
    }

    public static AnimalMemoryParticleShape GetShapeForSequence(int themeIndex, int sequenceIndex)
    {
        switch (ClampTheme(themeIndex))
        {
            case 0:
                return sequenceIndex % 2 == 0
                    ? AnimalMemoryParticleShape.Paw
                    : AnimalMemoryParticleShape.Leaf;
            case 1:
                return sequenceIndex % 2 == 0
                    ? AnimalMemoryParticleShape.Bubble
                    : AnimalMemoryParticleShape.Star;
            case 2:
                return sequenceIndex % 2 == 0
                    ? AnimalMemoryParticleShape.Moon
                    : AnimalMemoryParticleShape.Star;
            case 3:
                return sequenceIndex % 2 == 0
                    ? AnimalMemoryParticleShape.Flower
                    : AnimalMemoryParticleShape.Leaf;
            case 4:
                return sequenceIndex % 2 == 0
                    ? AnimalMemoryParticleShape.Candy
                    : AnimalMemoryParticleShape.Star;
            default:
                return sequenceIndex % 2 == 0
                    ? AnimalMemoryParticleShape.Gear
                    : AnimalMemoryParticleShape.Star;
        }
    }
}
