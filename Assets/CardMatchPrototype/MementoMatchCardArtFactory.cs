using System.Collections.Generic;
using AnimalMemory.Progression;
using UnityEngine;

public static class MementoMatchCardArtFactory
{
    private const int Width = 128;
    private const int Height = 128;
    private static readonly Dictionary<int, List<Sprite>> Cache =
        new Dictionary<int, List<Sprite>>();

    private readonly struct Palette
    {
        public Palette(Color32 outline, Color32 primary, Color32 secondary, Color32 accent, Color32 light, Color32 blush)
        {
            Outline = outline;
            Primary = primary;
            Secondary = secondary;
            Accent = accent;
            Light = light;
            Blush = blush;
        }

        public Color32 Outline { get; }
        public Color32 Primary { get; }
        public Color32 Secondary { get; }
        public Color32 Accent { get; }
        public Color32 Light { get; }
        public Color32 Blush { get; }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetRuntimeCache()
    {
        // Enter Play Mode can keep managed statics while Unity disposes
        // DontSave textures. Never carry those dead sprite references into a
        // new game session.
        Cache.Clear();
    }

    public static IReadOnlyList<Sprite> GetSprites(int setId)
    {
        if (setId <= AnimalMemoryContentIds.ForestSet)
            return new Sprite[0];

        int normalizedSetId = Mathf.Clamp(
            setId,
            AnimalMemoryContentIds.CoralSet,
            AnimalMemoryContentIds.ClockworkSet);
        if (!Cache.TryGetValue(normalizedSetId, out List<Sprite> sprites) ||
            !IsUsableSet(sprites))
        {
            sprites = BuildSet(normalizedSetId);
            Cache[normalizedSetId] = sprites;
        }

        return sprites;
    }

    private static bool IsUsableSet(IReadOnlyList<Sprite> sprites)
    {
        if (sprites == null ||
            sprites.Count != AnimalMemoryCardIdentityCatalog.IdentitiesPerSet)
        {
            return false;
        }

        for (int i = 0; i < sprites.Count; i++)
        {
            // Unity's overloaded null catches native objects destroyed between
            // Edit/Play sessions even when the managed reference still exists.
            if (sprites[i] == null || sprites[i].texture == null)
                return false;
        }

        return true;
    }

    private static List<Sprite> BuildSet(int setId)
    {
        IReadOnlyList<string> identities =
            AnimalMemoryCardIdentityCatalog.GetIdentityKeys(setId);
        List<Sprite> result = new List<Sprite>(identities.Count);
        Palette palette = GetPalette(setId);
        string theme = GetThemeKey(setId);

        for (int identity = 0; identity < identities.Count; identity++)
        {
            Color32[] pixels = new Color32[Width * Height];
            DrawSemanticSilhouette(pixels, setId, identity, palette);

            Texture2D texture = new Texture2D(
                Width,
                Height,
                TextureFormat.RGBA32,
                false)
            {
                name = theme + "_" + identities[identity],
                filterMode = FilterMode.Bilinear,
                wrapMode = TextureWrapMode.Clamp,
                hideFlags = HideFlags.DontSave
            };
            texture.SetPixels32(pixels);
            texture.Apply(false, false);

            Sprite sprite = Sprite.Create(
                texture,
                new Rect(0f, 0f, Width, Height),
                new Vector2(0.5f, 0.5f),
                100f);
            sprite.name = texture.name;
            sprite.hideFlags = HideFlags.DontSave;
            result.Add(sprite);
        }

        return result;
    }

    private static void DrawSemanticSilhouette(
        Color32[] pixels,
        int setId,
        int identity,
        Palette palette)
    {
        switch (setId)
        {
            case AnimalMemoryContentIds.CoralSet:
                DrawCoralIdentity(pixels, identity, palette);
                break;
            case AnimalMemoryContentIds.ConstellationSet:
                DrawConstellationIdentity(pixels, identity, palette);
                break;
            case AnimalMemoryContentIds.GardenSet:
                DrawGardenIdentity(pixels, identity, palette);
                break;
            case AnimalMemoryContentIds.SweetsSet:
                DrawSweetsIdentity(pixels, identity, palette);
                break;
            default:
                DrawClockworkIdentity(pixels, identity, palette);
                break;
        }

        ApplySemanticOrientationContract(pixels, setId, identity);
        ApplySharedArtFinish(pixels, palette);
    }

    private static void DrawCoralIdentity(Color32[] p, int id, Palette c)
    {
        switch (id)
        {
            case 0: // Nautilus
                Disc(p, 54, 73, 43, c.Outline);
                Disc(p, 54, 73, 36, c.Primary);
                Spiral(p, 54, 73, 30, c.Light, 6, 0.4f);
                Ellipse(p, 91, 61, 18, 23, c.Outline);
                Ellipse(p, 89, 62, 12, 17, c.Secondary);
                Eye(p, 94, 69, c);
                for (int tentacle = 0; tentacle < 5; tentacle++)
                    VerticalWave(p, 81 + tentacle * 6, 15 + tentacle * 3, 52, 4, tentacle % 2 == 0 ? c.Accent : c.Secondary, 5, tentacle * 0.8f);
                Triangle(p, 82, 72, 69, 84, 86, 81, c.Accent);
                break;
            case 1: // Pearl oyster
                Ellipse(p, 64, 72, 50, 31, c.Outline);
                Ellipse(p, 64, 75, 44, 24, c.Primary);
                for (int ray = -3; ray <= 3; ray++)
                    Stroke(p, 64, 88, 64 + ray * 12, 51 + Mathf.Abs(ray) * 3, c.Secondary, 4);
                Ellipse(p, 64, 48, 47, 20, c.Outline);
                Ellipse(p, 64, 49, 41, 14, c.Secondary);
                Disc(p, 64, 70, 16, c.Outline);
                Disc(p, 64, 70, 11, c.Light);
                Face(p, 64, 70, 7, c);
                break;
            case 2: // Coral branch
                RoundedRect(p, 35, 13, 93, 28, 7, c.Outline);
                RoundedRect(p, 42, 17, 86, 24, 3, c.Secondary);
                Stroke(p, 64, 23, 64, 98, c.Outline, 13);
                Stroke(p, 64, 25, 64, 98, c.Primary, 7);
                CoralBranch(p, 64, 48, -31, 25, c);
                CoralBranch(p, 65, 57, 32, 30, c);
                CoralBranch(p, 63, 73, -27, 25, c);
                CoralBranch(p, 65, 84, 25, 22, c);
                Disc(p, 64, 102, 8, c.Accent);
                Face(p, 64, 48, 7, c);
                break;
            case 3: // Message bottle
                RoundedRect(p, 35, 17, 93, 86, 14, c.Outline);
                RoundedRect(p, 42, 24, 86, 79, 10, c.Secondary);
                RoundedRect(p, 49, 78, 79, 106, 7, c.Outline);
                RoundedRect(p, 54, 82, 74, 101, 4, c.Primary);
                RoundedRect(p, 50, 103, 78, 115, 4, c.Outline);
                RoundedRect(p, 54, 106, 74, 112, 2, c.Accent);
                RoundedRect(p, 46, 39, 82, 67, 5, c.Light);
                Stroke(p, 49, 58, 79, 58, c.Accent, 4);
                Stroke(p, 49, 50, 75, 50, c.Accent, 3);
                Wave(p, 42, 31, 86, 5, c.Primary, 4, 0.3f);
                break;
            case 4: // Diving helmet
                Disc(p, 64, 66, 48, c.Outline);
                Disc(p, 64, 66, 41, c.Accent);
                Disc(p, 64, 63, 29, c.Outline);
                Disc(p, 64, 63, 23, c.Secondary);
                Stroke(p, 41, 63, 87, 63, c.Light, 4);
                Stroke(p, 64, 40, 64, 86, c.Light, 4);
                for (int i = 0; i < 4; i++)
                {
                    float a = i * Mathf.PI * 0.5f + 0.78f;
                    Disc(p, 64 + Mathf.RoundToInt(Mathf.Cos(a) * 37), 66 + Mathf.RoundToInt(Mathf.Sin(a) * 37), 4, c.Light);
                }
                RoundedRect(p, 25, 101, 103, 116, 5, c.Outline);
                RoundedRect(p, 33, 104, 95, 112, 3, c.Primary);
                break;
            case 5: // Seahorse
                Ellipse(p, 54, 94, 22, 17, c.Outline);
                Ellipse(p, 53, 93, 16, 11, c.Primary);
                Triangle(p, 36, 95, 16, 88, 38, 83, c.Outline);
                Triangle(p, 34, 92, 23, 88, 37, 86, c.Accent);
                Eye(p, 58, 97, c);
                VerticalWave(p, 65, 39, 83, 13, c.Outline, 14, 0.2f);
                VerticalWave(p, 65, 39, 82, 8, c.Secondary, 7, 0.2f);
                Spiral(p, 70, 35, 22, c.Accent, 6, 1.8f);
                Triangle(p, 48, 75, 33, 67, 51, 62, c.Primary);
                Triangle(p, 51, 60, 37, 50, 54, 48, c.Primary);
                break;
            case 6: // Compass
                Disc(p, 64, 64, 50, c.Outline);
                Disc(p, 64, 64, 43, c.Secondary);
                Ring(p, 64, 64, 34, c.Light, 5);
                Diamond(p, 64, 22, 78, 64, 64, 106, 50, 64, c.Accent);
                Diamond(p, 64, 37, 71, 64, 64, 91, 57, 64, c.Primary);
                Disc(p, 64, 64, 7, c.Outline);
                Disc(p, 64, 64, 3, c.Light);
                break;
            case 7: // Anchor
                Ring(p, 64, 104, 13, c.Outline, 6);
                Ring(p, 64, 104, 7, c.Light, 3);
                Stroke(p, 64, 91, 64, 33, c.Outline, 13);
                Stroke(p, 64, 88, 64, 36, c.Primary, 6);
                Stroke(p, 35, 79, 93, 79, c.Outline, 12);
                Stroke(p, 39, 79, 89, 79, c.Secondary, 5);
                Stroke(p, 64, 30, 28, 46, c.Outline, 12);
                Stroke(p, 64, 30, 100, 46, c.Outline, 12);
                Triangle(p, 24, 47, 43, 51, 33, 31, c.Accent);
                Triangle(p, 104, 47, 85, 51, 95, 31, c.Accent);
                break;
            case 8: // Lantern jellyfish
                Ellipse(p, 64, 88, 46, 35, c.Outline);
                Ellipse(p, 64, 86, 39, 28, c.Primary);
                Ellipse(p, 64, 82, 26, 20, c.Light);
                Face(p, 64, 84, 10, c);
                Stroke(p, 25, 69, 103, 69, c.Outline, 7);
                for (int i = -2; i <= 2; i++)
                    VerticalWave(p, 64 + i * 17, 17 + Mathf.Abs(i) * 5, 65, 5, i % 2 == 0 ? c.Accent : c.Secondary, 6, i);
                break;
            case 9: // Sunken key
                Ring(p, 38, 47, 24, c.Outline, 9);
                Ring(p, 38, 47, 13, c.Secondary, 5);
                Stroke(p, 54, 64, 101, 101, c.Outline, 15);
                Stroke(p, 55, 62, 98, 96, c.Accent, 7);
                Stroke(p, 80, 83, 91, 68, c.Outline, 10);
                Stroke(p, 92, 93, 104, 78, c.Outline, 10);
                Disc(p, 36, 45, 5, c.Light);
                break;
            case 10: // Turtle token
                Ellipse(p, 60, 66, 40, 32, c.Outline);
                Ellipse(p, 60, 66, 33, 25, c.Primary);
                Ring(p, 60, 66, 17, c.Accent, 5);
                Disc(p, 106, 65, 15, c.Outline);
                Disc(p, 103, 65, 10, c.Secondary);
                Eye(p, 108, 62, c);
                Disc(p, 35, 36, 12, c.Outline);
                Disc(p, 83, 38, 12, c.Outline);
                Disc(p, 34, 94, 12, c.Outline);
                Disc(p, 82, 94, 12, c.Outline);
                Triangle(p, 18, 65, 4, 57, 5, 74, c.Accent);
                break;
            case 11: // Starfish
                Star(p, 64, 64, 50, 20, c.Outline);
                Star(p, 64, 64, 42, 17, c.Primary);
                Disc(p, 64, 64, 15, c.Secondary);
                Face(p, 64, 64, 7, c);
                for (int i = 0; i < 5; i++)
                {
                    float a = -Mathf.PI * 0.5f + i * Mathf.PI * 0.4f;
                    Disc(p, 64 + Mathf.RoundToInt(Mathf.Cos(a) * 28), 64 + Mathf.RoundToInt(Mathf.Sin(a) * 28), 3, c.Light);
                }
                break;
            case 12: // Scallop shell
                Ellipse(p, 64, 68, 51, 42, c.Outline);
                Ellipse(p, 64, 69, 44, 35, c.Secondary);
                RoundedRect(p, 43, 13, 85, 29, 7, c.Outline);
                RoundedRect(p, 49, 17, 79, 25, 4, c.Accent);
                for (int ray = -4; ray <= 4; ray++)
                    Stroke(p, 64, 24, 64 + ray * 11, 101 - Mathf.Abs(ray) * 3, ray % 2 == 0 ? c.Light : c.Primary, 5);
                break;
            case 13: // Moon wave
                Disc(p, 79, 91, 30, c.Outline);
                Disc(p, 77, 91, 24, c.Light);
                ClearDisc(p, 90, 99, 21);
                Eye(p, 68, 91, c);
                Wave(p, 14, 47, 114, 7, c.Primary, 10, 0.2f);
                Wave(p, 14, 31, 114, 7, c.Secondary, 9, 1.0f);
                Disc(p, 29, 87, 5, c.Accent);
                break;
            default: // Treasure chest
                RoundedRect(p, 18, 52, 110, 108, 10, c.Outline);
                RoundedRect(p, 26, 60, 102, 100, 5, c.Primary);
                Ellipse(p, 64, 52, 46, 31, c.Outline);
                Ellipse(p, 64, 54, 38, 23, c.Secondary);
                Stroke(p, 20, 62, 108, 62, c.Accent, 9);
                Stroke(p, 39, 57, 39, 103, c.Accent, 8);
                Stroke(p, 89, 57, 89, 103, c.Accent, 8);
                RoundedRect(p, 53, 70, 75, 94, 5, c.Outline);
                Disc(p, 64, 79, 5, c.Light);
                Stroke(p, 64, 83, 64, 90, c.Light, 4);
                break;
        }
    }

    private static void DrawConstellationIdentity(Color32[] p, int id, Palette c)
    {
        switch (id)
        {
            case 0: // Spiral galaxy
                Ellipse(p, 64, 64, 51, 36, c.Outline);
                Ellipse(p, 64, 64, 45, 30, c.Primary);
                Spiral(p, 64, 64, 40, c.Light, 7, 0.8f);
                Disc(p, 64, 64, 8, c.Accent);
                MiniStar(p, 25, 38, c.Secondary);
                MiniStar(p, 103, 85, c.Secondary);
                break;
            case 1: // Crescent moon
                Disc(p, 61, 63, 50, c.Outline);
                Disc(p, 60, 62, 43, c.Light);
                ClearDisc(p, 80, 48, 38);
                Eye(p, 43, 57, c);
                Stroke(p, 38, 76, 48, 78, c.Blush, 4);
                MiniStar(p, 97, 87, c.Accent);
                break;
            case 2: // Star cluster
                int[,] cluster = {{24,61},{42,94},{64,75},{86,104},{106,68},{79,31},{43,25}};
                for (int i = 0; i < cluster.GetLength(0) - 1; i++)
                    Stroke(p, cluster[i,0], cluster[i,1], cluster[i+1,0], cluster[i+1,1], c.Secondary, 7);
                for (int i = 0; i < cluster.GetLength(0); i++)
                    Star(p, cluster[i,0], cluster[i,1], i == 2 ? 16 : 12, i == 2 ? 7 : 5, i % 2 == 0 ? c.Light : c.Accent);
                break;
            case 3: // Comet
                Stroke(p, 20, 24, 82, 85, c.Outline, 24);
                Stroke(p, 21, 27, 82, 88, c.Primary, 14);
                Stroke(p, 16, 43, 72, 96, c.Secondary, 8);
                Stroke(p, 31, 15, 78, 65, c.Accent, 8);
                Disc(p, 91, 96, 25, c.Outline);
                Disc(p, 91, 96, 18, c.Light);
                MiniStar(p, 97, 101, c.Accent);
                break;
            case 4: // Ringed planet
                Disc(p, 64, 64, 39, c.Outline);
                Disc(p, 64, 64, 32, c.Primary);
                EllipseRing(p, 64, 64, 55, 20, c.Outline, 8);
                EllipseRing(p, 64, 64, 51, 16, c.Accent, 4);
                Wave(p, 39, 57, 89, 5, c.Secondary, 5, 0.4f);
                Disc(p, 53, 50, 6, c.Light);
                break;
            case 5: // Orion
                Stroke(p, 64, 108, 47, 79, c.Secondary, 6);
                Stroke(p, 47, 79, 82, 75, c.Secondary, 6);
                Stroke(p, 82, 75, 101, 104, c.Secondary, 6);
                Stroke(p, 47, 79, 36, 29, c.Secondary, 6);
                Stroke(p, 82, 75, 92, 23, c.Secondary, 6);
                Stroke(p, 36, 29, 64, 49, c.Secondary, 6);
                Stroke(p, 64, 49, 92, 23, c.Secondary, 6);
                int[,] orion = {{64,108},{47,79},{60,77},{71,76},{82,75},{101,104},{36,29},{64,49},{92,23}};
                for (int i = 0; i < orion.GetLength(0); i++)
                    Disc(p, orion[i,0], orion[i,1], i == 0 ? 11 : 8, i % 3 == 0 ? c.Accent : c.Light);
                break;
            case 6: // Celestial compass
                Ring(p, 64, 64, 48, c.Outline, 8);
                Ring(p, 64, 64, 38, c.Primary, 5);
                EightPointStar(p, 64, 64, 43, 13, c.Light);
                Disc(p, 64, 64, 9, c.Accent);
                MiniStar(p, 64, 15, c.Secondary);
                break;
            case 7: // Rocket
                Ellipse(p, 64, 72, 27, 45, c.Outline);
                Ellipse(p, 64, 74, 20, 37, c.Light);
                Triangle(p, 44, 46, 28, 24, 50, 31, c.Primary);
                Triangle(p, 84, 46, 100, 24, 78, 31, c.Primary);
                Disc(p, 64, 78, 11, c.Outline);
                Disc(p, 64, 78, 7, c.Secondary);
                Triangle(p, 52, 36, 64, 9, 76, 36, c.Accent);
                Face(p, 64, 78, 4, c);
                break;
            case 8: // Aurora
                for (int curtain = 0; curtain < 5; curtain++)
                    VerticalWave(p, 25 + curtain * 20, 17, 111, 9 + (curtain % 2) * 3, curtain % 2 == 0 ? c.Primary : c.Secondary, 12, curtain * 0.9f);
                MiniStar(p, 16, 96, c.Light);
                MiniStar(p, 110, 31, c.Accent);
                Disc(p, 102, 106, 5, c.Light);
                break;
            case 9: // Shooting star
                Star(p, 84, 39, 31, 13, c.Outline);
                Star(p, 84, 39, 24, 10, c.Light);
                Stroke(p, 65, 58, 21, 102, c.Outline, 15);
                Stroke(p, 65, 58, 22, 99, c.Primary, 8);
                Stroke(p, 75, 65, 43, 105, c.Accent, 7);
                MiniStar(p, 27, 68, c.Secondary);
                break;
            case 10: // Ursa major bear
                Ellipse(p, 57, 61, 43, 29, c.Outline);
                Ellipse(p, 57, 62, 35, 22, c.Primary);
                Disc(p, 97, 78, 24, c.Outline);
                Disc(p, 94, 78, 17, c.Secondary);
                Disc(p, 83, 97, 10, c.Outline);
                Disc(p, 105, 99, 10, c.Outline);
                Eye(p, 100, 82, c);
                Ellipse(p, 109, 72, 11, 8, c.Accent);
                Stroke(p, 33, 43, 29, 17, c.Outline, 15);
                Stroke(p, 72, 42, 79, 17, c.Outline, 15);
                Disc(p, 22, 76, 9, c.Primary);
                MiniStar(p, 31, 103, c.Light);
                MiniStar(p, 67, 102, c.Accent);
                break;
            case 11: // Five-star constellation
                int[,] fiveStars = {{64,104},{102,75},{88,29},{40,29},{26,75}};
                for (int i = 0; i < fiveStars.GetLength(0); i++)
                {
                    int radius = i == 0 ? 18 : 15;
                    Star(p, fiveStars[i,0], fiveStars[i,1], radius, radius / 2, i % 2 == 0 ? c.Light : c.Accent);
                }
                Disc(p, 64, 64, 7, c.Primary);
                break;
            case 12: // Eclipse
                Disc(p, 64, 64, 51, c.Accent);
                Ring(p, 64, 64, 46, c.Light, 7);
                Disc(p, 64, 64, 37, c.Outline);
                Disc(p, 54, 54, 8, c.Primary);
                for (int ray = 0; ray < 12; ray++)
                {
                    float a = ray * Mathf.PI / 6f;
                    Stroke(p, 64 + Mathf.RoundToInt(Mathf.Cos(a) * 52), 64 + Mathf.RoundToInt(Mathf.Sin(a) * 52), 64 + Mathf.RoundToInt(Mathf.Cos(a) * 59), 64 + Mathf.RoundToInt(Mathf.Sin(a) * 59), c.Light, 5);
                }
                break;
            case 13: // Lunar tide
                Disc(p, 42, 93, 26, c.Outline);
                Disc(p, 40, 93, 20, c.Light);
                ClearDisc(p, 51, 101, 18);
                Wave(p, 13, 49, 115, 8, c.Primary, 11, 0.2f);
                Wave(p, 13, 31, 115, 8, c.Secondary, 10, 1.0f);
                MiniStar(p, 94, 94, c.Accent);
                break;
            default: // Observatory
                RoundedRect(p, 28, 67, 100, 110, 9, c.Outline);
                RoundedRect(p, 36, 74, 92, 104, 5, c.Primary);
                Ellipse(p, 64, 68, 41, 37, c.Outline);
                Ellipse(p, 64, 70, 34, 30, c.Secondary);
                Stroke(p, 64, 31, 64, 75, c.Outline, 8);
                Stroke(p, 59, 47, 98, 25, c.Outline, 16);
                Stroke(p, 61, 47, 96, 28, c.Light, 8);
                Disc(p, 101, 23, 9, c.Accent);
                MiniStar(p, 23, 28, c.Light);
                break;
        }
    }

    private static void DrawGardenIdentity(Color32[] p, int id, Palette c)
    {
        switch (id)
        {
            case 0: // Rose
                Stroke(p, 64, 73, 64, 15, c.Outline, 12);
                Stroke(p, 64, 72, 64, 17, c.Secondary, 6);
                Leaf(p, 47, 45, 18, 11, 0.5f, c.Primary, c.Outline);
                Leaf(p, 81, 35, 18, 11, -0.5f, c.Primary, c.Outline);
                for (int petal = 0; petal < 7; petal++)
                {
                    float a = petal * Mathf.PI * 2f / 7f;
                    Ellipse(p, 64 + Mathf.RoundToInt(Mathf.Cos(a) * 19), 88 + Mathf.RoundToInt(Mathf.Sin(a) * 16), 17, 13, c.Outline);
                    Ellipse(p, 64 + Mathf.RoundToInt(Mathf.Cos(a) * 19), 88 + Mathf.RoundToInt(Mathf.Sin(a) * 16), 12, 9, c.Accent);
                }
                Spiral(p, 64, 88, 18, c.Light, 5, 0.3f);
                break;
            case 1: // Lotus dewdrop
                Ellipse(p, 42, 48, 30, 15, c.Outline);
                Ellipse(p, 43, 50, 25, 10, c.Primary);
                Ellipse(p, 86, 48, 30, 15, c.Outline);
                Ellipse(p, 85, 50, 25, 10, c.Primary);
                LotusPetal(p, 39, 52, 22, 35, c.Primary, c.Outline);
                LotusPetal(p, 89, 52, 22, 35, c.Primary, c.Outline);
                LotusPetal(p, 51, 51, 20, 43, c.Blush, c.Outline);
                LotusPetal(p, 77, 51, 20, 43, c.Blush, c.Outline);
                LotusPetal(p, 64, 52, 18, 48, c.Light, c.Outline);
                Disc(p, 64, 55, 9, c.Accent);
                Teardrop(p, 64, 109, 8, 12, c.Secondary, c.Outline);
                Wave(p, 14, 25, 114, 5, c.Secondary, 6, 0.2f);
                break;
            case 2: // Flowering vine
                VerticalWave(p, 62, 17, 112, 15, c.Outline, 12, 0.4f);
                VerticalWave(p, 62, 19, 110, 10, c.Secondary, 6, 0.4f);
                Leaf(p, 39, 42, 20, 11, -0.7f, c.Primary, c.Outline);
                Leaf(p, 87, 67, 20, 11, 0.7f, c.Primary, c.Outline);
                Flower(p, 36, 88, 14, c.Accent, c.Light, c.Outline);
                Flower(p, 88, 30, 13, c.Blush, c.Light, c.Outline);
                break;
            case 3: // Seed bottle
                RoundedRect(p, 35, 16, 93, 86, 14, c.Outline);
                RoundedRect(p, 43, 23, 85, 79, 10, c.Light);
                RoundedRect(p, 48, 78, 80, 106, 7, c.Outline);
                RoundedRect(p, 53, 82, 75, 101, 4, c.Secondary);
                RoundedRect(p, 49, 103, 79, 114, 4, c.Accent);
                for (int i = 0; i < 7; i++)
                    Ellipse(p, 49 + (i % 3) * 15, 39 + (i / 3) * 14, 6, 9, i % 2 == 0 ? c.Primary : c.Accent);
                Face(p, 64, 67, 8, c);
                break;
            case 4: // Flower pot
                Trapezoid(p, 32, 57, 96, 57, 84, 14, 44, 14, c.Outline);
                Trapezoid(p, 40, 51, 88, 51, 78, 21, 50, 21, c.Blush);
                RoundedRect(p, 29, 52, 99, 66, 6, c.Outline);
                RoundedRect(p, 37, 55, 91, 62, 3, c.Accent);
                Stroke(p, 64, 61, 64, 91, c.Outline, 10);
                Leaf(p, 43, 83, 22, 13, 0.4f, c.Secondary, c.Outline);
                Leaf(p, 85, 88, 22, 13, -0.4f, c.Secondary, c.Outline);
                Flower(p, 64, 101, 18, c.Accent, c.Light, c.Outline);
                Face(p, 64, 35, 9, c);
                break;
            case 5: // Young sprout
                Disc(p, 64, 25, 31, c.Outline);
                Disc(p, 64, 26, 24, c.Blush);
                Stroke(p, 64, 43, 64, 87, c.Outline, 11);
                Stroke(p, 64, 44, 64, 86, c.Secondary, 5);
                Leaf(p, 42, 89, 27, 17, 0.5f, c.Primary, c.Outline);
                Leaf(p, 86, 96, 27, 17, -0.5f, c.Primary, c.Outline);
                Face(p, 64, 26, 10, c);
                break;
            case 6: // Sunflower
                Stroke(p, 64, 68, 64, 14, c.Outline, 13);
                Stroke(p, 64, 67, 64, 16, c.Secondary, 6);
                Leaf(p, 43, 39, 23, 13, 0.5f, c.Primary, c.Outline);
                for (int petal = 0; petal < 12; petal++)
                {
                    float a = petal * Mathf.PI / 6f;
                    Ellipse(p, 64 + Mathf.RoundToInt(Mathf.Cos(a) * 31), 88 + Mathf.RoundToInt(Mathf.Sin(a) * 31), 9, 18, c.Accent);
                }
                Disc(p, 64, 88, 23, c.Outline);
                Disc(p, 64, 88, 17, c.Blush);
                Face(p, 64, 88, 8, c);
                break;
            case 7: // Watering can
                RoundedRect(p, 24, 19, 86, 69, 12, c.Outline);
                RoundedRect(p, 32, 26, 79, 62, 8, c.Primary);
                EllipseRing(p, 39, 70, 28, 36, c.Outline, 8);
                Stroke(p, 84, 56, 111, 83, c.Outline, 17);
                Stroke(p, 85, 57, 108, 80, c.Secondary, 9);
                Ellipse(p, 109, 85, 14, 9, c.Outline);
                Ellipse(p, 107, 84, 9, 5, c.Light);
                Face(p, 56, 43, 9, c);
                for (int i = 0; i < 3; i++)
                    Teardrop(p, 94 + i * 8, 72 - i * 7, 5, 9, c.Light, c.Secondary);
                break;
            case 8: // Mushroom
                RoundedRect(p, 47, 15, 81, 69, 12, c.Outline);
                RoundedRect(p, 54, 22, 74, 63, 8, c.Light);
                Ellipse(p, 64, 83, 49, 34, c.Outline);
                Ellipse(p, 64, 80, 42, 27, c.Accent);
                Disc(p, 41, 89, 7, c.Light);
                Disc(p, 69, 101, 6, c.Light);
                Disc(p, 89, 80, 8, c.Light);
                Face(p, 64, 42, 7, c);
                break;
            case 9: // Garden shears
                Ring(p, 38, 92, 21, c.Outline, 9);
                Ring(p, 90, 92, 21, c.Outline, 9);
                Ring(p, 38, 92, 12, c.Blush, 5);
                Ring(p, 90, 92, 12, c.Blush, 5);
                Stroke(p, 50, 77, 101, 20, c.Outline, 13);
                Stroke(p, 78, 77, 27, 20, c.Outline, 13);
                Stroke(p, 51, 75, 96, 25, c.Light, 5);
                Stroke(p, 77, 75, 32, 25, c.Secondary, 5);
                Disc(p, 64, 70, 8, c.Accent);
                break;
            case 10: // Leaf beetle
                Leaf(p, 42, 69, 36, 24, 0.2f, c.Primary, c.Outline);
                Ellipse(p, 76, 67, 30, 38, c.Outline);
                Ellipse(p, 76, 67, 23, 31, c.Accent);
                Stroke(p, 76, 35, 76, 99, c.Outline, 6);
                Stroke(p, 56, 49, 38, 36, c.Outline, 7);
                Stroke(p, 56, 66, 34, 66, c.Outline, 7);
                Stroke(p, 57, 83, 39, 98, c.Outline, 7);
                Stroke(p, 96, 49, 111, 35, c.Outline, 7);
                Stroke(p, 97, 67, 115, 67, c.Outline, 7);
                Stroke(p, 96, 84, 111, 100, c.Outline, 7);
                Disc(p, 67, 58, 4, c.Light);
                Disc(p, 85, 76, 4, c.Light);
                Face(p, 76, 91, 7, c);
                break;
            case 11: // Daisy
                Stroke(p, 64, 68, 64, 14, c.Secondary, 9);
                Leaf(p, 82, 39, 18, 10, -0.5f, c.Primary, c.Outline);
                for (int petal = 0; petal < 10; petal++)
                {
                    float a = petal * Mathf.PI / 5f;
                    Ellipse(p, 64 + Mathf.RoundToInt(Mathf.Cos(a) * 32), 87 + Mathf.RoundToInt(Mathf.Sin(a) * 32), 10, 21, c.Light);
                }
                Disc(p, 64, 87, 24, c.Outline);
                Disc(p, 64, 87, 18, c.Accent);
                Face(p, 64, 87, 8, c);
                break;
            case 12: // Monstera leaf
                Leaf(p, 64, 62, 49, 40, 0.1f, c.Primary, c.Outline);
                Stroke(p, 64, 37, 64, 114, c.Outline, 9);
                Stroke(p, 64, 39, 64, 111, c.Secondary, 4);
                for (int i = -2; i <= 2; i++)
                {
                    int y = 52 + (i + 2) * 12;
                    Stroke(p, 64, y, 29 + Mathf.Abs(i) * 4, y - 10, c.Light, 5);
                    Stroke(p, 64, y, 99 - Mathf.Abs(i) * 4, y - 10, c.Light, 5);
                }
                break;
            case 13: // Pond lily
                Disc(p, 64, 54, 46, c.Outline);
                Disc(p, 64, 54, 39, c.Primary);
                Triangle(p, 64, 54, 112, 37, 112, 71, new Color32(0,0,0,0));
                Wave(p, 15, 20, 113, 5, c.Secondary, 6, 0.3f);
                Flower(p, 64, 88, 24, c.Blush, c.Light, c.Outline);
                Disc(p, 64, 88, 8, c.Accent);
                break;
            default: // Greenhouse
                RoundedRect(p, 19, 14, 109, 77, 4, c.Outline);
                RoundedRect(p, 27, 21, 101, 70, 2, c.Light);
                Triangle(p, 16, 75, 64, 116, 112, 75, c.Outline);
                Triangle(p, 27, 78, 64, 107, 101, 78, c.Secondary);
                Stroke(p, 64, 19, 64, 110, c.Outline, 6);
                Stroke(p, 24, 48, 104, 48, c.Outline, 5);
                RoundedRect(p, 48, 17, 80, 46, 4, c.Primary);
                Flower(p, 40, 37, 10, c.Accent, c.Light, c.Outline);
                Leaf(p, 89, 36, 15, 9, -0.6f, c.Primary, c.Outline);
                break;
        }
    }

    private static void DrawSweetsIdentity(Color32[] p, int id, Palette c)
    {
        switch (id)
        {
            case 0: // Roll cake
                Ellipse(p, 64, 72, 45, 39, c.Outline);
                Ellipse(p, 64, 72, 37, 31, c.Primary);
                Spiral(p, 64, 72, 27, c.Light, 7, 0.4f);
                RoundedRect(p, 18, 18, 110, 34, 7, c.Outline);
                RoundedRect(p, 27, 22, 101, 29, 3, c.Secondary);
                Disc(p, 92, 101, 9, c.Accent);
                Face(p, 64, 71, 9, c);
                break;
            case 1: // Macaron
                Ellipse(p, 64, 64, 49, 37, c.Outline);
                Ellipse(p, 64, 55, 42, 25, c.Primary);
                Ellipse(p, 64, 75, 42, 25, c.Secondary);
                RoundedRect(p, 22, 57, 106, 72, 6, c.Light);
                Face(p, 64, 63, 10, c);
                Disc(p, 33, 52, 6, c.Accent);
                Disc(p, 94, 79, 6, c.Accent);
                break;
            case 2: // Candy cane
                Ring(p, 64, 91, 35, c.Outline, 15);
                Ring(p, 64, 91, 25, c.Light, 8);
                ClearRect(p, 20, 54, 64, 91);
                Stroke(p, 64, 90, 64, 17, c.Outline, 21);
                Stroke(p, 64, 89, 64, 19, c.Light, 12);
                for (int y = 26; y < 82; y += 18)
                    Stroke(p, 54, y, 74, y + 10, c.Accent, 7);
                Stroke(p, 75, 109, 89, 102, c.Accent, 7);
                break;
            case 3: // Soda bottle
                RoundedRect(p, 39, 16, 89, 88, 13, c.Outline);
                RoundedRect(p, 46, 23, 82, 81, 9, c.Primary);
                RoundedRect(p, 50, 80, 78, 109, 6, c.Outline);
                RoundedRect(p, 55, 84, 73, 105, 3, c.Light);
                RoundedRect(p, 48, 107, 80, 116, 4, c.Accent);
                RoundedRect(p, 44, 40, 84, 64, 7, c.Secondary);
                Face(p, 64, 52, 8, c);
                Disc(p, 53, 73, 4, c.Accent);
                Disc(p, 74, 29, 5, c.Accent);
                break;
            case 4: // Cupcake
                Trapezoid(p, 31, 61, 97, 61, 86, 17, 42, 17, c.Outline);
                Trapezoid(p, 39, 55, 89, 55, 79, 24, 49, 24, c.Secondary);
                Ellipse(p, 64, 78, 47, 32, c.Outline);
                Ellipse(p, 64, 80, 40, 25, c.Primary);
                Disc(p, 64, 111, 11, c.Accent);
                Stroke(p, 64, 111, 70, 119, c.Secondary, 4);
                Face(p, 64, 79, 9, c);
                break;
            case 5: // Lollipop
                Disc(p, 61, 51, 43, c.Outline);
                Disc(p, 61, 51, 35, c.Primary);
                Spiral(p, 61, 51, 31, c.Light, 7, 0.1f);
                Stroke(p, 78, 84, 103, 115, c.Outline, 13);
                Stroke(p, 78, 84, 101, 113, c.Secondary, 6);
                Bow(p, 82, 89, c.Accent, c.Outline);
                break;
            case 6: // Donut
                Disc(p, 64, 65, 51, c.Outline);
                Disc(p, 64, 65, 44, c.Secondary);
                Ring(p, 64, 65, 26, c.Primary, 13);
                ClearDisc(p, 64, 65, 15);
                Sprinkle(p, 38, 39, c.Accent);
                Sprinkle(p, 83, 36, c.Light);
                Sprinkle(p, 94, 70, c.Accent);
                Sprinkle(p, 42, 88, c.Light);
                Face(p, 64, 91, 9, c);
                break;
            case 7: // Dessert fork
                Stroke(p, 31, 16, 31, 92, c.Outline, 12);
                Stroke(p, 31, 19, 31, 90, c.Light, 5);
                for (int tine = 0; tine < 4; tine++)
                    Stroke(p, 19 + tine * 8, 88, 19 + tine * 8, 115, c.Outline, 7);
                Triangle(p, 52, 21, 112, 21, 95, 77, c.Outline);
                Triangle(p, 60, 28, 104, 28, 91, 67, c.Primary);
                Stroke(p, 65, 44, 98, 44, c.Secondary, 8);
                Disc(p, 94, 78, 8, c.Accent);
                break;
            case 8: // Jelly
                Ellipse(p, 64, 41, 49, 25, c.Outline);
                RoundedRect(p, 25, 36, 103, 89, 19, c.Outline);
                RoundedRect(p, 33, 43, 95, 82, 14, c.Primary);
                Ellipse(p, 64, 85, 36, 16, c.Light);
                Disc(p, 64, 97, 8, c.Accent);
                Face(p, 64, 61, 10, c);
                for (int i = 0; i < 4; i++)
                    Disc(p, 41 + i * 16, 43, 6, c.Secondary);
                break;
            case 9: // Chocolate bar
                RoundedRect(p, 24, 16, 104, 113, 8, c.Outline);
                RoundedRect(p, 31, 23, 97, 106, 4, c.Primary);
                for (int row = 0; row < 3; row++)
                    for (int col = 0; col < 2; col++)
                        RoundedRect(p, 38 + col * 29, 31 + row * 25, 60 + col * 29, 49 + row * 25, 3, (row + col) % 2 == 0 ? c.Secondary : c.Accent);
                Face(p, 64, 96, 9, c);
                break;
            case 10: // Cookie
                Disc(p, 64, 64, 50, c.Outline);
                Disc(p, 64, 64, 43, c.Primary);
                int[,] chips = {{37,42},{67,31},{89,50},{44,72},{77,69},{94,90},{57,98}};
                for (int i = 0; i < chips.GetLength(0); i++)
                    Disc(p, chips[i,0], chips[i,1], 6, i % 2 == 0 ? c.Secondary : c.Accent);
                Face(p, 63, 81, 9, c);
                break;
            case 11: // Star candy
                Star(p, 64, 64, 51, 22, c.Outline);
                Star(p, 64, 64, 43, 18, c.Accent);
                Ring(p, 64, 64, 18, c.Light, 5);
                Face(p, 64, 64, 8, c);
                Disc(p, 28, 24, 5, c.Secondary);
                Disc(p, 103, 96, 5, c.Secondary);
                break;
            case 12: // Croissant
                Ellipse(p, 64, 55, 45, 27, c.Outline);
                Triangle(p, 39, 39, 10, 79, 45, 72, c.Outline);
                Triangle(p, 89, 39, 118, 79, 83, 72, c.Outline);
                Ellipse(p, 64, 55, 38, 21, c.Accent);
                Triangle(p, 42, 43, 18, 75, 48, 68, c.Accent);
                Triangle(p, 86, 43, 110, 75, 80, 68, c.Accent);
                ClearEllipse(p, 64, 83, 24, 14);
                Stroke(p, 44, 40, 50, 62, c.Primary, 6);
                Stroke(p, 64, 35, 64, 60, c.Primary, 6);
                Stroke(p, 84, 40, 78, 62, c.Primary, 6);
                Face(p, 64, 49, 9, c);
                break;
            case 13: // Ice cream
                Triangle(p, 37, 67, 91, 67, 64, 14, c.Outline);
                Triangle(p, 45, 62, 83, 62, 64, 24, c.Secondary);
                Disc(p, 64, 82, 35, c.Outline);
                Disc(p, 64, 83, 28, c.Primary);
                Disc(p, 45, 74, 18, c.Outline);
                Disc(p, 45, 76, 13, c.Accent);
                Disc(p, 83, 74, 18, c.Outline);
                Disc(p, 83, 76, 13, c.Light);
                Disc(p, 64, 108, 19, c.Outline);
                Disc(p, 64, 109, 14, c.Blush);
                Face(p, 64, 83, 8, c);
                break;
            default: // Celebration cake
                RoundedRect(p, 22, 15, 106, 59, 8, c.Outline);
                RoundedRect(p, 29, 22, 99, 52, 4, c.Primary);
                RoundedRect(p, 35, 52, 93, 85, 7, c.Outline);
                RoundedRect(p, 42, 59, 86, 79, 4, c.Secondary);
                Stroke(p, 32, 43, 96, 43, c.Light, 8);
                Stroke(p, 64, 84, 64, 106, c.Outline, 8);
                Teardrop(p, 64, 108, 8, 14, c.Accent, c.Blush);
                Face(p, 64, 32, 9, c);
                Disc(p, 43, 68, 4, c.Accent);
                Disc(p, 85, 68, 4, c.Accent);
                break;
        }
    }

    private static void DrawClockworkIdentity(Color32[] p, int id, Palette c)
    {
        switch (id)
        {
            case 0: // Mainspring
                Disc(p, 64, 64, 50, c.Outline);
                Disc(p, 64, 64, 43, c.Primary);
                Spiral(p, 64, 64, 39, c.Light, 8, 0.2f);
                RoundedRect(p, 57, 55, 71, 73, 4, c.Accent);
                Stroke(p, 99, 64, 116, 64, c.Outline, 10);
                Stroke(p, 103, 64, 114, 64, c.Secondary, 4);
                break;
            case 1: // Pendulum
                RoundedRect(p, 26, 45, 102, 116, 12, c.Outline);
                RoundedRect(p, 34, 53, 94, 108, 8, c.Primary);
                Disc(p, 64, 84, 23, c.Outline);
                Disc(p, 64, 84, 17, c.Light);
                Stroke(p, 64, 84, 64, 99, c.Accent, 5);
                Stroke(p, 64, 84, 76, 77, c.Secondary, 5);
                Stroke(p, 64, 61, 64, 25, c.Outline, 8);
                Disc(p, 64, 18, 15, c.Outline);
                Disc(p, 64, 18, 9, c.Accent);
                break;
            case 2: // Gear train
                Gear(p, 43, 48, 31, 21, 10, c.Outline, c.Primary);
                Gear(p, 83, 79, 35, 23, 12, c.Outline, c.Secondary);
                Gear(p, 30, 94, 19, 12, 8, c.Outline, c.Accent);
                Disc(p, 43, 48, 7, c.Light);
                Disc(p, 83, 79, 7, c.Light);
                break;
            case 3: // Oil bottle
                RoundedRect(p, 29, 14, 91, 81, 16, c.Outline);
                RoundedRect(p, 37, 22, 83, 73, 12, c.Light);
                RoundedRect(p, 40, 23, 80, 50, 10, c.Primary);
                Stroke(p, 41, 51, 79, 51, c.Accent, 5);
                RoundedRect(p, 45, 72, 75, 104, 7, c.Outline);
                RoundedRect(p, 50, 77, 70, 100, 4, c.Light);
                RoundedRect(p, 48, 101, 73, 114, 5, c.Accent);
                Stroke(p, 73, 97, 104, 91, c.Outline, 11);
                Stroke(p, 74, 97, 102, 92, c.Light, 5);
                Teardrop(p, 108, 73, 7, 12, c.Primary, c.Outline);
                break;
            case 4: // Pocket watch
                Ring(p, 64, 108, 13, c.Outline, 7);
                RoundedRect(p, 55, 102, 73, 116, 4, c.Accent);
                Disc(p, 64, 57, 46, c.Outline);
                Disc(p, 64, 57, 38, c.Light);
                Ring(p, 64, 57, 29, c.Primary, 4);
                Stroke(p, 64, 57, 64, 86, c.Outline, 6);
                Stroke(p, 64, 57, 84, 47, c.Accent, 6);
                Disc(p, 64, 57, 6, c.Secondary);
                break;
            case 5: // Winding key
                Ring(p, 39, 94, 22, c.Outline, 9);
                Ring(p, 89, 94, 22, c.Outline, 9);
                Ring(p, 39, 94, 12, c.Light, 4);
                Ring(p, 89, 94, 12, c.Light, 4);
                Stroke(p, 50, 82, 64, 67, c.Outline, 13);
                Stroke(p, 78, 82, 64, 67, c.Outline, 13);
                Stroke(p, 64, 68, 64, 17, c.Outline, 15);
                Stroke(p, 64, 65, 64, 20, c.Accent, 7);
                Stroke(p, 64, 28, 84, 28, c.Outline, 11);
                break;
            case 6: // Brass compass
                Disc(p, 64, 64, 49, c.Outline);
                Disc(p, 64, 64, 41, c.Primary);
                Ring(p, 64, 64, 31, c.Light, 5);
                Triangle(p, 64, 19, 78, 65, 64, 57, c.Accent);
                Triangle(p, 64, 109, 50, 63, 64, 71, c.Secondary);
                Stroke(p, 21, 64, 107, 64, c.Light, 4);
                Disc(p, 64, 64, 7, c.Outline);
                break;
            case 7: // Anchor escapement
                Gear(p, 64, 50, 40, 28, 14, c.Outline, c.Primary);
                Disc(p, 64, 50, 7, c.Light);
                Stroke(p, 64, 93, 64, 57, c.Outline, 10);
                Disc(p, 64, 96, 10, c.Secondary);
                Stroke(p, 64, 76, 34, 66, c.Accent, 12);
                Stroke(p, 64, 76, 94, 66, c.Accent, 12);
                Triangle(p, 29, 68, 45, 59, 41, 78, c.Light);
                Triangle(p, 99, 68, 83, 59, 87, 78, c.Light);
                Stroke(p, 64, 106, 64, 116, c.Outline, 7);
                break;
            case 8: // Alarm bell
                Ellipse(p, 64, 59, 43, 38, c.Outline);
                Ellipse(p, 64, 57, 35, 30, c.Primary);
                Stroke(p, 22, 31, 106, 31, c.Outline, 12);
                RoundedRect(p, 52, 89, 76, 105, 6, c.Outline);
                RoundedRect(p, 57, 92, 71, 101, 3, c.Accent);
                Disc(p, 64, 22, 10, c.Secondary);
                Stroke(p, 36, 88, 23, 105, c.Outline, 9);
                Stroke(p, 92, 88, 105, 105, c.Outline, 9);
                Face(p, 64, 58, 10, c);
                break;
            case 9: // Clock key
                Ring(p, 36, 38, 24, c.Outline, 10);
                Gear(p, 36, 38, 17, 11, 8, c.Secondary, c.Light);
                Stroke(p, 50, 54, 101, 105, c.Outline, 16);
                Stroke(p, 52, 53, 98, 99, c.Primary, 7);
                Stroke(p, 82, 84, 97, 68, c.Outline, 11);
                Stroke(p, 94, 97, 108, 82, c.Outline, 11);
                break;
            case 10: // Automaton turtle
                Ellipse(p, 58, 62, 40, 29, c.Outline);
                Ellipse(p, 58, 63, 33, 22, c.Primary);
                Gear(p, 58, 63, 24, 16, 8, c.Secondary, c.Light);
                Disc(p, 101, 66, 16, c.Outline);
                Disc(p, 99, 66, 10, c.Accent);
                Eye(p, 103, 70, c);
                Stroke(p, 34, 40, 30, 18, c.Outline, 11);
                Stroke(p, 77, 40, 83, 18, c.Outline, 11);
                Triangle(p, 20, 63, 5, 55, 7, 72, c.Secondary);
                RoundedRect(p, 48, 91, 68, 106, 4, c.Outline);
                Stroke(p, 58, 91, 58, 82, c.Light, 5);
                break;
            case 11: // Star wheel
                Gear(p, 64, 64, 50, 37, 16, c.Outline, c.Primary);
                EightPointStar(p, 64, 64, 38, 15, c.Light);
                Ring(p, 64, 64, 21, c.Accent, 5);
                Disc(p, 64, 64, 8, c.Secondary);
                break;
            case 12: // Hourglass
                RoundedRect(p, 24, 14, 104, 31, 5, c.Outline);
                RoundedRect(p, 24, 97, 104, 114, 5, c.Outline);
                Stroke(p, 34, 29, 94, 99, c.Outline, 10);
                Stroke(p, 94, 29, 34, 99, c.Outline, 10);
                Triangle(p, 39, 32, 89, 32, 64, 62, c.Light);
                Triangle(p, 39, 96, 89, 96, 64, 66, c.Accent);
                Stroke(p, 64, 58, 64, 72, c.Secondary, 5);
                Disc(p, 64, 75, 5, c.Secondary);
                break;
            case 13: // Moon dial
                Disc(p, 64, 64, 49, c.Outline);
                Disc(p, 64, 64, 41, c.Primary);
                Disc(p, 54, 57, 27, c.Light);
                ClearDisc(p, 66, 48, 22);
                Stroke(p, 64, 64, 94, 42, c.Accent, 7);
                Ring(p, 64, 64, 31, c.Secondary, 4);
                for (int i = 0; i < 8; i++)
                {
                    float a = i * Mathf.PI * 0.25f;
                    Disc(p, 64 + Mathf.RoundToInt(Mathf.Cos(a) * 37), 64 + Mathf.RoundToInt(Mathf.Sin(a) * 37), 3, c.Light);
                }
                break;
            default: // Clock tower
                RoundedRect(p, 35, 13, 93, 94, 5, c.Outline);
                RoundedRect(p, 43, 20, 85, 87, 2, c.Primary);
                Triangle(p, 29, 92, 64, 120, 99, 92, c.Outline);
                Triangle(p, 40, 96, 64, 113, 88, 96, c.Secondary);
                Disc(p, 64, 65, 23, c.Outline);
                Disc(p, 64, 65, 17, c.Light);
                Stroke(p, 64, 65, 64, 79, c.Accent, 4);
                Stroke(p, 64, 65, 76, 59, c.Accent, 4);
                RoundedRect(p, 53, 15, 75, 40, 4, c.Outline);
                RoundedRect(p, 58, 18, 70, 36, 2, c.Secondary);
                break;
        }
    }

    private static void ApplySemanticOrientationContract(
        Color32[] pixels,
        int setId,
        int identity)
    {
        bool requiresVerticalCorrection;
        switch (setId)
        {
            case AnimalMemoryContentIds.CoralSet:
                requiresVerticalCorrection = identity == 4 || identity == 14;
                break;
            case AnimalMemoryContentIds.ConstellationSet:
                requiresVerticalCorrection = identity == 14;
                break;
            case AnimalMemoryContentIds.GardenSet:
                requiresVerticalCorrection = identity == 9 || identity == 12;
                break;
            case AnimalMemoryContentIds.SweetsSet:
                // Both the lollipop and jelly are authored in the drawing
                // helper's opposite vertical convention.
                requiresVerticalCorrection = identity == 5;
                break;
            default:
                requiresVerticalCorrection = false;
                break;
        }

        if (!requiresVerticalCorrection)
            return;

        for (int y = 0; y < Height / 2; y++)
        {
            int oppositeY = Height - 1 - y;
            int lowerRow = y * Width;
            int upperRow = oppositeY * Width;
            for (int x = 0; x < Width; x++)
            {
                Color32 temporary = pixels[lowerRow + x];
                pixels[lowerRow + x] = pixels[upperRow + x];
                pixels[upperRow + x] = temporary;
            }
        }
    }

    private static void ApplySharedArtFinish(Color32[] pixels, Palette palette)
    {
        Color32[] source = (Color32[])pixels.Clone();
        Color32 softShadow = palette.Outline;
        softShadow.a = 105;

        // A restrained lower-right contact shadow and upper-left highlight give
        // every set the same two-layer finish as the original animal cards.
        for (int y = 2; y < Height - 2; y++)
        {
            for (int x = 2; x < Width - 2; x++)
            {
                int index = y * Width + x;
                if (source[index].a == 0)
                {
                    Color32 caster = source[(y + 2) * Width + (x - 2)];
                    if (caster.a > 0)
                        pixels[index] = softShadow;
                    continue;
                }

                Color32 upperLeft = source[(y + 1) * Width + (x - 1)];
                if (upperLeft.a == 0 &&
                    (source[index].r != palette.Outline.r ||
                     source[index].g != palette.Outline.g ||
                     source[index].b != palette.Outline.b ||
                     source[index].a != palette.Outline.a))
                {
                    pixels[index] = (Color32)Color.Lerp(
                        source[index],
                        palette.Light,
                        0.22f);
                }
            }
        }
    }

    private static Palette GetPalette(int setId)
    {
        switch (setId)
        {
            case AnimalMemoryContentIds.CoralSet:
                return new Palette(
                    new Color32(21, 54, 78, 255),
                    new Color32(55, 201, 207, 255),
                    new Color32(48, 132, 196, 255),
                    new Color32(255, 126, 105, 255),
                    new Color32(255, 243, 188, 255),
                    new Color32(255, 179, 186, 255));
            case AnimalMemoryContentIds.ConstellationSet:
                return new Palette(
                    new Color32(35, 29, 82, 255),
                    new Color32(89, 84, 214, 255),
                    new Color32(77, 160, 232, 255),
                    new Color32(255, 183, 82, 255),
                    new Color32(246, 240, 255, 255),
                    new Color32(232, 145, 218, 255));
            case AnimalMemoryContentIds.GardenSet:
                return new Palette(
                    new Color32(36, 75, 55, 255),
                    new Color32(83, 190, 103, 255),
                    new Color32(38, 137, 91, 255),
                    new Color32(255, 174, 71, 255),
                    new Color32(255, 246, 201, 255),
                    new Color32(246, 124, 157, 255));
            case AnimalMemoryContentIds.SweetsSet:
                return new Palette(
                    new Color32(91, 45, 72, 255),
                    new Color32(244, 126, 167, 255),
                    new Color32(176, 92, 197, 255),
                    new Color32(255, 193, 68, 255),
                    new Color32(255, 241, 211, 255),
                    new Color32(255, 160, 142, 255));
            default:
                return new Palette(
                    new Color32(64, 47, 36, 255),
                    new Color32(190, 126, 55, 255),
                    new Color32(117, 83, 59, 255),
                    new Color32(237, 177, 57, 255),
                    new Color32(255, 239, 183, 255),
                    new Color32(197, 104, 78, 255));
        }
    }

    private static string GetThemeKey(int setId)
    {
        switch (setId)
        {
            case AnimalMemoryContentIds.CoralSet: return "coral_relic";
            case AnimalMemoryContentIds.ConstellationSet: return "constellation";
            case AnimalMemoryContentIds.GardenSet: return "garden_bloom";
            case AnimalMemoryContentIds.SweetsSet: return "patisserie";
            default: return "clockwork";
        }
    }

    private static void CoralBranch(Color32[] p, int x, int y, int dx, int dy, Palette c)
    {
        Stroke(p, x, y, x + dx, y + dy, c.Outline, 12);
        Stroke(p, x, y, x + dx, y + dy, c.Primary, 6);
        Disc(p, x + dx, y + dy, 7, c.Accent);
    }

    private static void Face(Color32[] p, int cx, int cy, int spacing, Palette c)
    {
        Eye(p, cx - spacing, cy + 3, c);
        Eye(p, cx + spacing, cy + 3, c);
        Stroke(p, cx - 4, cy - 7, cx, cy - 9, c.Outline, 3);
        Stroke(p, cx, cy - 9, cx + 4, cy - 7, c.Outline, 3);
    }

    private static void Eye(Color32[] p, int x, int y, Palette c)
    {
        Color32 pupil = new Color32(24, 27, 31, 255);
        Disc(p, x, y, 6, c.Outline);
        Disc(p, x, y, 5, new Color32(255, 255, 255, 255));
        Disc(p, x, y, 2, pupil);
    }

    private static void MiniStar(Color32[] p, int x, int y, Color32 color)
    {
        Star(p, x, y, 9, 4, color);
    }

    private static void LotusPetal(
        Color32[] p,
        int cx,
        int baseY,
        int halfWidth,
        int height,
        Color32 fill,
        Color32 outline)
    {
        Triangle(
            p,
            cx,
            baseY + height,
            cx - halfWidth - 3,
            baseY,
            cx + halfWidth + 3,
            baseY,
            outline);
        Triangle(
            p,
            cx,
            baseY + height - 6,
            cx - halfWidth + 2,
            baseY + 4,
            cx + halfWidth - 2,
            baseY + 4,
            fill);
        Disc(p, cx, baseY + 8, Mathf.Max(7, halfWidth - 4), fill);
    }

    private static void Flower(Color32[] p, int cx, int cy, int radius, Color32 petal, Color32 center, Color32 outline)
    {
        for (int i = 0; i < 6; i++)
        {
            float a = i * Mathf.PI / 3f;
            int x = cx + Mathf.RoundToInt(Mathf.Cos(a) * radius);
            int y = cy + Mathf.RoundToInt(Mathf.Sin(a) * radius);
            Disc(p, x, y, Mathf.Max(5, radius / 2), outline);
            Disc(p, x, y, Mathf.Max(3, radius / 2 - 4), petal);
        }
        Disc(p, cx, cy, Mathf.Max(6, radius / 2), outline);
        Disc(p, cx, cy, Mathf.Max(3, radius / 2 - 4), center);
    }

    private static void Leaf(Color32[] p, int cx, int cy, int rx, int ry, float angle, Color32 fill, Color32 outline)
    {
        RotatedEllipse(p, cx, cy, rx + 4, ry + 4, angle, outline);
        RotatedEllipse(p, cx, cy, rx, ry, angle, fill);
        int dx = Mathf.RoundToInt(Mathf.Cos(angle) * rx);
        int dy = Mathf.RoundToInt(Mathf.Sin(angle) * rx);
        Stroke(p, cx - dx, cy - dy, cx + dx, cy + dy, outline, 3);
    }

    private static void Bow(Color32[] p, int cx, int cy, Color32 fill, Color32 outline)
    {
        Triangle(p, cx, cy, cx - 25, cy - 14, cx - 22, cy + 16, outline);
        Triangle(p, cx, cy, cx + 25, cy - 14, cx + 22, cy + 16, outline);
        Triangle(p, cx, cy, cx - 19, cy - 9, cx - 17, cy + 10, fill);
        Triangle(p, cx, cy, cx + 19, cy - 9, cx + 17, cy + 10, fill);
        Disc(p, cx, cy, 8, outline);
    }

    private static void Sprinkle(Color32[] p, int x, int y, Color32 color)
    {
        Stroke(p, x - 4, y - 2, x + 4, y + 2, color, 4);
    }

    private static void Teardrop(Color32[] p, int cx, int cy, int rx, int ry, Color32 fill, Color32 outline)
    {
        Ellipse(p, cx, cy - ry / 4, rx + 3, ry * 3 / 4 + 3, outline);
        Triangle(p, cx, cy + ry, cx - rx - 2, cy - ry / 3, cx + rx + 2, cy - ry / 3, outline);
        Ellipse(p, cx, cy - ry / 4, rx, ry * 3 / 4, fill);
        Triangle(p, cx, cy + ry - 5, cx - rx + 3, cy - ry / 3, cx + rx - 3, cy - ry / 3, fill);
    }

    private static void Gear(Color32[] p, int cx, int cy, int outer, int inner, int teeth, Color32 outline, Color32 fill)
    {
        Disc(p, cx, cy, inner + 7, outline);
        Disc(p, cx, cy, inner, fill);
        for (int i = 0; i < teeth; i++)
        {
            float a = i * Mathf.PI * 2f / teeth;
            int x0 = cx + Mathf.RoundToInt(Mathf.Cos(a) * (inner - 2));
            int y0 = cy + Mathf.RoundToInt(Mathf.Sin(a) * (inner - 2));
            int x1 = cx + Mathf.RoundToInt(Mathf.Cos(a) * outer);
            int y1 = cy + Mathf.RoundToInt(Mathf.Sin(a) * outer);
            Stroke(p, x0, y0, x1, y1, outline, 10);
            Stroke(p, x0, y0, x1, y1, fill, 5);
        }
        Ring(p, cx, cy, Mathf.Max(6, inner / 2), outline, 5);
    }

    private static void EightPointStar(Color32[] p, int cx, int cy, int outer, int inner, Color32 color)
    {
        for (int i = 0; i < 8; i++)
        {
            float a = -Mathf.PI * 0.5f + i * Mathf.PI * 0.25f;
            float side = a + Mathf.PI * 0.5f;
            int tx = cx + Mathf.RoundToInt(Mathf.Cos(a) * outer);
            int ty = cy + Mathf.RoundToInt(Mathf.Sin(a) * outer);
            int ax = cx + Mathf.RoundToInt(Mathf.Cos(side) * inner);
            int ay = cy + Mathf.RoundToInt(Mathf.Sin(side) * inner);
            int bx = cx - Mathf.RoundToInt(Mathf.Cos(side) * inner);
            int by = cy - Mathf.RoundToInt(Mathf.Sin(side) * inner);
            Triangle(p, tx, ty, ax, ay, bx, by, color);
        }
    }

    private static void Star(Color32[] p, int cx, int cy, int outer, int inner, Color32 color)
    {
        Vector2Int[] points = new Vector2Int[10];
        for (int i = 0; i < points.Length; i++)
        {
            float a = Mathf.PI * 0.5f + i * Mathf.PI / 5f;
            int radius = (i & 1) == 0 ? outer : inner;
            points[i] = new Vector2Int(
                cx + Mathf.RoundToInt(Mathf.Cos(a) * radius),
                cy + Mathf.RoundToInt(Mathf.Sin(a) * radius));
        }

        for (int i = 0; i < points.Length; i++)
        {
            Vector2Int a = points[i];
            Vector2Int b = points[(i + 1) % points.Length];
            Triangle(p, cx, cy, a.x, a.y, b.x, b.y, color);
        }
    }

    private static void Diamond(Color32[] p, int topX, int topY, int rightX, int rightY, int bottomX, int bottomY, int leftX, int leftY, Color32 color)
    {
        Triangle(p, topX, topY, rightX, rightY, bottomX, bottomY, color);
        Triangle(p, topX, topY, bottomX, bottomY, leftX, leftY, color);
    }

    private static void Trapezoid(Color32[] p, int ax, int ay, int bx, int by, int cx, int cy, int dx, int dy, Color32 color)
    {
        Triangle(p, ax, ay, bx, by, cx, cy, color);
        Triangle(p, ax, ay, cx, cy, dx, dy, color);
    }

    private static void Triangle(Color32[] p, int ax, int ay, int bx, int by, int cx, int cy, Color32 color)
    {
        int minX = Mathf.Max(0, Mathf.Min(ax, Mathf.Min(bx, cx)));
        int maxX = Mathf.Min(Width - 1, Mathf.Max(ax, Mathf.Max(bx, cx)));
        int minY = Mathf.Max(0, Mathf.Min(ay, Mathf.Min(by, cy)));
        int maxY = Mathf.Min(Height - 1, Mathf.Max(ay, Mathf.Max(by, cy)));
        float area = Edge(ax, ay, bx, by, cx, cy);
        if (Mathf.Abs(area) < 0.001f)
            return;

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                float w0 = Edge(bx, by, cx, cy, x, y);
                float w1 = Edge(cx, cy, ax, ay, x, y);
                float w2 = Edge(ax, ay, bx, by, x, y);
                if ((w0 >= 0f && w1 >= 0f && w2 >= 0f) ||
                    (w0 <= 0f && w1 <= 0f && w2 <= 0f))
                    Put(p, x, y, color);
            }
        }
    }

    private static float Edge(int ax, int ay, int bx, int by, int px, int py)
    {
        return (px - ax) * (by - ay) - (py - ay) * (bx - ax);
    }

    private static void RoundedRect(Color32[] p, int x0, int y0, int x1, int y1, int radius, Color32 color)
    {
        FillRect(p, x0 + radius, y0, x1 - radius, y1, color);
        FillRect(p, x0, y0 + radius, x1, y1 - radius, color);
        Disc(p, x0 + radius, y0 + radius, radius, color);
        Disc(p, x1 - radius, y0 + radius, radius, color);
        Disc(p, x0 + radius, y1 - radius, radius, color);
        Disc(p, x1 - radius, y1 - radius, radius, color);
    }

    private static void FillRect(Color32[] p, int x0, int y0, int x1, int y1, Color32 color)
    {
        int minX = Mathf.Clamp(Mathf.Min(x0, x1), 0, Width - 1);
        int maxX = Mathf.Clamp(Mathf.Max(x0, x1), 0, Width - 1);
        int minY = Mathf.Clamp(Mathf.Min(y0, y1), 0, Height - 1);
        int maxY = Mathf.Clamp(Mathf.Max(y0, y1), 0, Height - 1);
        for (int y = minY; y <= maxY; y++)
            for (int x = minX; x <= maxX; x++)
                Put(p, x, y, color);
    }

    private static void ClearRect(Color32[] p, int x0, int y0, int x1, int y1)
    {
        FillRect(p, x0, y0, x1, y1, new Color32(0, 0, 0, 0));
    }

    private static void Disc(Color32[] p, int cx, int cy, int radius, Color32 color)
    {
        int r2 = radius * radius;
        for (int y = -radius; y <= radius; y++)
            for (int x = -radius; x <= radius; x++)
                if (x * x + y * y <= r2)
                    Put(p, cx + x, cy + y, color);
    }

    private static void ClearDisc(Color32[] p, int cx, int cy, int radius)
    {
        Disc(p, cx, cy, radius, new Color32(0, 0, 0, 0));
    }

    private static void Ellipse(Color32[] p, int cx, int cy, int rx, int ry, Color32 color)
    {
        for (int y = -ry; y <= ry; y++)
            for (int x = -rx; x <= rx; x++)
                if (x * x / (float)(rx * rx) + y * y / (float)(ry * ry) <= 1f)
                    Put(p, cx + x, cy + y, color);
    }

    private static void ClearEllipse(Color32[] p, int cx, int cy, int rx, int ry)
    {
        Ellipse(p, cx, cy, rx, ry, new Color32(0, 0, 0, 0));
    }

    private static void RotatedEllipse(Color32[] p, int cx, int cy, int rx, int ry, float angle, Color32 color)
    {
        float cos = Mathf.Cos(angle);
        float sin = Mathf.Sin(angle);
        int radius = Mathf.Max(rx, ry);
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                float localX = x * cos + y * sin;
                float localY = -x * sin + y * cos;
                if (localX * localX / (rx * rx) + localY * localY / (ry * ry) <= 1f)
                    Put(p, cx + x, cy + y, color);
            }
        }
    }

    private static void Ring(Color32[] p, int cx, int cy, int radius, Color32 color, int thickness)
    {
        int outer = radius * radius;
        int innerRadius = Mathf.Max(0, radius - thickness);
        int inner = innerRadius * innerRadius;
        for (int y = -radius; y <= radius; y++)
            for (int x = -radius; x <= radius; x++)
            {
                int d = x * x + y * y;
                if (d <= outer && d >= inner)
                    Put(p, cx + x, cy + y, color);
            }
    }

    private static void EllipseRing(Color32[] p, int cx, int cy, int rx, int ry, Color32 color, int thickness)
    {
        float innerRx = Mathf.Max(1, rx - thickness);
        float innerRy = Mathf.Max(1, ry - thickness);
        for (int y = -ry; y <= ry; y++)
            for (int x = -rx; x <= rx; x++)
            {
                float outer = x * x / (float)(rx * rx) + y * y / (float)(ry * ry);
                float inner = x * x / (innerRx * innerRx) + y * y / (innerRy * innerRy);
                if (outer <= 1f && inner >= 1f)
                    Put(p, cx + x, cy + y, color);
            }
    }

    private static void Stroke(Color32[] p, int x0, int y0, int x1, int y1, Color32 color, int thickness)
    {
        int steps = Mathf.Max(Mathf.Abs(x1 - x0), Mathf.Abs(y1 - y0));
        if (steps == 0)
        {
            Disc(p, x0, y0, thickness / 2, color);
            return;
        }

        int radius = Mathf.Max(1, thickness / 2);
        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            Disc(
                p,
                Mathf.RoundToInt(Mathf.Lerp(x0, x1, t)),
                Mathf.RoundToInt(Mathf.Lerp(y0, y1, t)),
                radius,
                color);
        }
    }

    private static void Spiral(Color32[] p, int cx, int cy, int radius, Color32 color, int thickness, float phase)
    {
        Vector2 previous = new Vector2(cx, cy);
        for (int i = 1; i <= 120; i++)
        {
            float t = i / 120f;
            float angle = phase + t * Mathf.PI * 5.4f;
            Vector2 next = new Vector2(
                cx + Mathf.Cos(angle) * radius * t,
                cy + Mathf.Sin(angle) * radius * t);
            Stroke(p, Mathf.RoundToInt(previous.x), Mathf.RoundToInt(previous.y), Mathf.RoundToInt(next.x), Mathf.RoundToInt(next.y), color, thickness);
            previous = next;
        }
    }

    private static void Wave(Color32[] p, int x0, int y, int x1, int amplitude, Color32 color, int thickness, float phase)
    {
        int previousY = y;
        for (int x = x0 + 1; x <= x1; x++)
        {
            int nextY = y + Mathf.RoundToInt(Mathf.Sin((x - x0) * 0.16f + phase) * amplitude);
            Stroke(p, x - 1, previousY, x, nextY, color, thickness);
            previousY = nextY;
        }
    }

    private static void VerticalWave(Color32[] p, int x, int y0, int y1, int amplitude, Color32 color, int thickness, float phase)
    {
        int previousX = x;
        for (int y = y0 + 1; y <= y1; y++)
        {
            int nextX = x + Mathf.RoundToInt(Mathf.Sin((y - y0) * 0.18f + phase) * amplitude);
            Stroke(p, previousX, y - 1, nextX, y, color, thickness);
            previousX = nextX;
        }
    }

    private static void Put(Color32[] p, int x, int y, Color32 color)
    {
        if (x < 0 || x >= Width || y < 0 || y >= Height)
            return;
        p[y * Width + x] = color;
    }
}
