using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Presentation-only renderer for Memento Match worlds.
/// It owns procedural textures and animated motifs, and never changes match rules or card state.
/// </summary>
[DisallowMultipleComponent]
public sealed class MementoMatchBackdropView : MonoBehaviour
{
    private const int TextureSize = 512;
    private const int MotifTextureSize = 96;
    private const int MotifCount = 10;

    private sealed class AnimatedMotif
    {
        public RectTransform Rect;
        public Image Image;
        public Vector2 Anchor;
        public Vector2 Drift;
        public float Phase;
        public float Speed;
        public float Spin;
        public float Size;
        public float Alpha;
        public Color Color;
    }

    private readonly List<AnimatedMotif> motifs = new List<AnimatedMotif>(MotifCount);
    private readonly List<Texture2D> motifTextures = new List<Texture2D>(2);
    private readonly List<Sprite> motifSprites = new List<Sprite>(2);

    private Image target;
    private Texture2D texture;
    private Sprite sprite;
    private int variation;
    private int currentTheme;
    private float animationOrigin;

    public void Bind(Image image)
    {
        target = image;
    }

    public void ApplyTheme(int themeId, bool newVariation)
    {
        if (target == null)
            return;

        if (newVariation)
            variation++;

        currentTheme = Mathf.Clamp(themeId, 0, 5);
        ReleaseGeneratedAssets();
        texture = BuildTexture(currentTheme, variation);
        sprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, texture.width, texture.height),
            new Vector2(0.5f, 0.5f),
            100f);
        sprite.name = $"MementoBackdrop_{currentTheme}_{variation}";
        sprite.hideFlags = HideFlags.DontSave;
        target.sprite = sprite;
        target.type = Image.Type.Simple;
        target.preserveAspect = false;
        target.color = Color.white;

        BuildAnimatedMotifs(currentTheme, variation);
        animationOrigin = Time.unscaledTime;
        UpdateMotifs(0f);
    }

    private void Update()
    {
        if (motifs.Count == 0 || target == null || !target.gameObject.activeInHierarchy)
            return;

        UpdateMotifs(Time.unscaledTime - animationOrigin);
    }

    private void UpdateMotifs(float elapsed)
    {
        float screenWidth = Screen.width;
        float screenHeight = Screen.height;
        if (screenWidth < 1f || screenHeight < 1f)
            return;

        float shortestSide = Mathf.Min(screenWidth, screenHeight);
        float inheritedScale = Mathf.Max(
            0.001f,
            Mathf.Min(
                Mathf.Abs(target.rectTransform.lossyScale.x),
                Mathf.Abs(target.rectTransform.lossyScale.y)));
        for (int i = 0; i < motifs.Count; i++)
        {
            AnimatedMotif motif = motifs[i];
            float wave = elapsed * motif.Speed + motif.Phase;
            float normalizedX = motif.Anchor.x + Mathf.Sin(wave) * motif.Drift.x;
            float normalizedY = motif.Anchor.y + Mathf.Cos(wave * 0.73f) * motif.Drift.y;
            Vector2 localPoint;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                target.rectTransform,
                new Vector2(normalizedX * screenWidth, normalizedY * screenHeight),
                null,
                out localPoint);
            motif.Rect.anchoredPosition = localPoint;

            float pulse = 1f + Mathf.Sin(wave * 1.17f) * 0.075f;
            float screenSize = Mathf.Clamp(shortestSide * motif.Size * pulse, 54f, 180f);
            float localSize = screenSize / inheritedScale;
            motif.Rect.sizeDelta = new Vector2(localSize, localSize);
            motif.Rect.localEulerAngles = new Vector3(
                0f,
                0f,
                motif.Phase * Mathf.Rad2Deg + elapsed * motif.Spin +
                Mathf.Sin(wave * 0.61f) * 8f);

            Color color = motif.Color;
            color.a = motif.Alpha * (0.78f + 0.22f * Mathf.Sin(wave + 0.8f));
            motif.Image.color = color;
        }
    }

    private void BuildAnimatedMotifs(int themeId, int seed)
    {
        ReleaseMotifs();

        Color primary;
        Color secondary;
        GetMotifColors(themeId, out primary, out secondary);

        for (int variant = 0; variant < 2; variant++)
        {
            Texture2D motifTexture = BuildMotifTexture(themeId, variant);
            Sprite motifSprite = Sprite.Create(
                motifTexture,
                new Rect(0f, 0f, motifTexture.width, motifTexture.height),
                new Vector2(0.5f, 0.5f),
                100f);
            motifSprite.name = $"MementoMotif_{GetThemeName(themeId)}_{variant}";
            motifSprite.hideFlags = HideFlags.DontSave;
            motifTextures.Add(motifTexture);
            motifSprites.Add(motifSprite);
        }

        System.Random random = new System.Random(
            44771 + themeId * 3571 + seed * 10103);
        for (int i = 0; i < MotifCount; i++)
        {
            int variant = i & 1;
            GameObject motifObject = new GameObject(
                $"Backdrop_{GetThemeName(themeId)}_{GetMotifName(themeId, variant)}_{i:00}",
                typeof(RectTransform),
                typeof(CanvasRenderer),
                typeof(Image));
            motifObject.hideFlags = HideFlags.DontSave;
            motifObject.transform.SetParent(target.rectTransform, false);

            RectTransform rect = motifObject.GetComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.5f, 0.5f);
            rect.anchorMax = new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);

            Image image = motifObject.GetComponent<Image>();
            image.sprite = motifSprites[variant];
            image.type = Image.Type.Simple;
            image.preserveAspect = true;
            image.raycastTarget = false;

            float edgeBias = (float)random.NextDouble();
            Vector2 anchor = new Vector2(
                0.05f + (float)random.NextDouble() * 0.90f,
                0.06f + (float)random.NextDouble() * 0.88f);
            if (edgeBias < 0.35f)
                anchor.x = edgeBias < 0.175f
                    ? 0.04f + (float)random.NextDouble() * 0.16f
                    : 0.80f + (float)random.NextDouble() * 0.16f;

            AnimatedMotif motif = new AnimatedMotif
            {
                Rect = rect,
                Image = image,
                Anchor = anchor,
                Drift = new Vector2(
                    0.018f + (float)random.NextDouble() * 0.038f,
                    0.026f + (float)random.NextDouble() * 0.052f),
                Phase = (float)random.NextDouble() * Mathf.PI * 2f,
                Speed = 0.12f + (float)random.NextDouble() * 0.15f,
                Spin = themeId == 1
                    ? 2.5f + (float)random.NextDouble() * 4f
                    : -3f + (float)random.NextDouble() * 6f,
                Size = 0.12f + (float)random.NextDouble() * 0.12f,
                Alpha = 0.13f + (float)random.NextDouble() * 0.12f,
                Color = variant == 0 ? primary : secondary
            };
            motifs.Add(motif);
        }
    }

    private static Texture2D BuildMotifTexture(int themeId, int variant)
    {
        Color32[] pixels = new Color32[MotifTextureSize * MotifTextureSize];
        for (int y = 0; y < MotifTextureSize; y++)
        {
            for (int x = 0; x < MotifTextureSize; x++)
            {
                Vector2 point = new Vector2(
                    x / (MotifTextureSize - 1f) * 2f - 1f,
                    y / (MotifTextureSize - 1f) * 2f - 1f);
                float alpha;
                if (themeId == 0)
                    alpha = variant == 0 ? LeafMask(point) : SprigMask(point);
                else if (themeId == 1)
                    alpha = variant == 0 ? CoralMask(point) : ShellMask(point);
                else if (themeId == 2)
                    alpha = variant == 0 ? StarMask(point) : CometMask(point);
                else if (themeId == 3)
                    alpha = variant == 0 ? FlowerMask(point) : LeafMask(point);
                else if (themeId == 4)
                    alpha = variant == 0 ? CandyMask(point) : StarMask(point);
                else
                    alpha = variant == 0 ? GearMask(point) : ClockMask(point);

                pixels[y * MotifTextureSize + x] =
                    new Color(1f, 1f, 1f, Mathf.Clamp01(alpha));
            }
        }

        Texture2D result = new Texture2D(
            MotifTextureSize,
            MotifTextureSize,
            TextureFormat.RGBA32,
            false)
        {
            name = $"MementoMotifTexture_{GetThemeName(themeId)}_{variant}",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.DontSave
        };
        result.SetPixels32(pixels);
        result.Apply(false, true);
        return result;
    }

    private static float FlowerMask(Vector2 point)
    {
        float result = 0f;
        for (int i = 0; i < 6; i++)
        {
            float angle = i * Mathf.PI / 3f;
            Vector2 center = new Vector2(
                Mathf.Cos(angle) * 0.48f,
                Mathf.Sin(angle) * 0.48f);
            result = Mathf.Max(
                result,
                LensMask(Rotate(point - center, -angle) * 1.8f, 0.82f, 0.44f));
        }

        float core = Mathf.Clamp01((0.26f - point.magnitude) * 14f);
        return Mathf.Max(result * 0.86f, core);
    }

    private static float CandyMask(Vector2 point)
    {
        float core = Mathf.Clamp01((0.48f - point.magnitude) * 12f);
        float left = LensMask(Rotate(point - new Vector2(-0.62f, 0f), 0.3f) * 2f, 0.82f, 0.46f);
        float right = LensMask(Rotate(point - new Vector2(0.62f, 0f), -0.3f) * 2f, 0.82f, 0.46f);
        return Mathf.Max(core, Mathf.Max(left, right) * 0.88f);
    }

    private static float GearMask(Vector2 point)
    {
        float radius = point.magnitude;
        float ring = StrokeMask(Mathf.Abs(radius - 0.58f), 0.11f);
        float teeth = 0f;
        for (int i = 0; i < 10; i++)
        {
            float angle = i * Mathf.PI / 5f;
            Vector2 tooth = Rotate(point, -angle) - new Vector2(0.78f, 0f);
            teeth = Mathf.Max(teeth, Mathf.Clamp01((0.16f - Mathf.Max(Mathf.Abs(tooth.x), Mathf.Abs(tooth.y))) * 16f));
        }
        return Mathf.Max(ring, teeth);
    }

    private static float ClockMask(Vector2 point)
    {
        float rim = StrokeMask(Mathf.Abs(point.magnitude - 0.74f), 0.045f);
        float vertical = StrokeMask(
            DistanceToSegment(point, Vector2.zero, new Vector2(0f, 0.52f)),
            0.055f);
        float diagonal = StrokeMask(
            DistanceToSegment(point, Vector2.zero, new Vector2(0.43f, -0.31f)),
            0.055f);
        return Mathf.Max(rim, Mathf.Max(vertical, diagonal));
    }

    private static float LeafMask(Vector2 point)
    {
        float body = LensMask(point, 0.82f, 0.44f);
        float vein = StrokeMask(
            DistanceToSegment(point, new Vector2(-0.68f, 0f), new Vector2(0.62f, 0f)),
            0.035f);
        return Mathf.Max(body * 0.88f, vein);
    }

    private static float SprigMask(Vector2 point)
    {
        float stem = StrokeMask(
            DistanceToSegment(point, new Vector2(-0.70f, -0.72f), new Vector2(0.64f, 0.62f)),
            0.045f);
        float leafA = LensMask(Rotate(point - new Vector2(-0.30f, -0.10f), -0.72f) * 2.4f, 0.82f, 0.42f);
        float leafB = LensMask(Rotate(point - new Vector2(0.08f, 0.14f), 2.35f) * 2.6f, 0.82f, 0.42f);
        float leafC = LensMask(Rotate(point - new Vector2(0.39f, 0.45f), -0.72f) * 2.7f, 0.82f, 0.42f);
        return Mathf.Max(stem, Mathf.Max(leafA, Mathf.Max(leafB, leafC)) * 0.92f);
    }

    private static float CoralMask(Vector2 point)
    {
        Vector2 root = new Vector2(0f, -0.88f);
        float mask = StrokeMask(DistanceToSegment(point, root, new Vector2(0f, 0.18f)), 0.085f);
        mask = Mathf.Max(mask, StrokeMask(DistanceToSegment(point, new Vector2(0f, -0.15f), new Vector2(-0.56f, 0.62f)), 0.075f));
        mask = Mathf.Max(mask, StrokeMask(DistanceToSegment(point, new Vector2(0f, -0.03f), new Vector2(0.58f, 0.70f)), 0.075f));
        mask = Mathf.Max(mask, StrokeMask(DistanceToSegment(point, new Vector2(-0.24f, 0.18f), new Vector2(-0.70f, 0.28f)), 0.060f));
        mask = Mathf.Max(mask, StrokeMask(DistanceToSegment(point, new Vector2(0.25f, 0.27f), new Vector2(0.72f, 0.18f)), 0.060f));
        mask = Mathf.Max(mask, StrokeMask(DistanceToSegment(point, new Vector2(-0.42f, 0.43f), new Vector2(-0.40f, 0.82f)), 0.055f));
        mask = Mathf.Max(mask, StrokeMask(DistanceToSegment(point, new Vector2(0.43f, 0.50f), new Vector2(0.38f, 0.88f)), 0.055f));
        return mask;
    }

    private static float ShellMask(Vector2 point)
    {
        Vector2 origin = new Vector2(0f, -0.58f);
        float mask = 0f;
        for (int i = -3; i <= 3; i++)
        {
            float angle = Mathf.Lerp(0.42f, 2.72f, (i + 3f) / 6f);
            Vector2 endpoint = origin + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * 0.86f;
            mask = Mathf.Max(mask, StrokeMask(DistanceToSegment(point, origin, endpoint), 0.030f));
        }

        float radius = Vector2.Distance(point, origin);
        float rim = StrokeMask(Mathf.Abs(radius - 0.86f), 0.045f);
        if (point.y < origin.y)
            rim = 0f;
        float baseLine = StrokeMask(
            DistanceToSegment(point, new Vector2(-0.32f, -0.62f), new Vector2(0.32f, -0.62f)),
            0.055f);
        return Mathf.Max(mask * 0.78f, Mathf.Max(rim, baseLine));
    }

    private static float StarMask(Vector2 point)
    {
        float vertical = Mathf.Max(0f, 1f - Mathf.Abs(point.y)) *
                         Mathf.Max(0f, 1f - Mathf.Abs(point.x) * 7.5f);
        float horizontal = Mathf.Max(0f, 1f - Mathf.Abs(point.x)) *
                           Mathf.Max(0f, 1f - Mathf.Abs(point.y) * 7.5f);
        Vector2 diagonal = Rotate(point, Mathf.PI * 0.25f);
        float smallVertical = Mathf.Max(0f, 0.62f - Mathf.Abs(diagonal.y)) *
                              Mathf.Max(0f, 1f - Mathf.Abs(diagonal.x) * 11f);
        float smallHorizontal = Mathf.Max(0f, 0.62f - Mathf.Abs(diagonal.x)) *
                                Mathf.Max(0f, 1f - Mathf.Abs(diagonal.y) * 11f);
        return Mathf.Clamp01(Mathf.Max(Mathf.Max(vertical, horizontal), Mathf.Max(smallVertical, smallHorizontal)) * 1.5f);
    }

    private static float CometMask(Vector2 point)
    {
        Vector2 headPoint = point - new Vector2(0.45f, 0.28f);
        float head = Mathf.Clamp01((0.23f - Mathf.Abs(headPoint.x) - Mathf.Abs(headPoint.y)) * 12f);
        float trailA = StrokeMask(
            DistanceToSegment(point, new Vector2(-0.82f, -0.48f), new Vector2(0.28f, 0.16f)),
            0.038f);
        float trailB = StrokeMask(
            DistanceToSegment(point, new Vector2(-0.74f, -0.22f), new Vector2(0.20f, 0.24f)),
            0.026f);
        float trailC = StrokeMask(
            DistanceToSegment(point, new Vector2(-0.58f, -0.66f), new Vector2(0.22f, 0.10f)),
            0.022f);
        return Mathf.Max(head, Mathf.Max(trailA, Mathf.Max(trailB, trailC)) * 0.82f);
    }

    private static float LensMask(Vector2 point, float radiusX, float radiusY)
    {
        float normalizedX = Mathf.Abs(point.x) / radiusX;
        if (normalizedX >= 1f)
            return 0f;

        float halfHeight = radiusY * (1f - normalizedX);
        float distance = Mathf.Abs(point.y) - halfHeight;
        return Mathf.Clamp01(-distance * 24f);
    }

    private static float StrokeMask(float distance, float width)
    {
        return Mathf.Clamp01((width - distance) * 24f);
    }

    private static float DistanceToSegment(Vector2 point, Vector2 start, Vector2 end)
    {
        Vector2 segment = end - start;
        float lengthSquared = segment.sqrMagnitude;
        if (lengthSquared <= 0.0001f)
            return Vector2.Distance(point, start);

        float projection = Mathf.Clamp01(Vector2.Dot(point - start, segment) / lengthSquared);
        return Vector2.Distance(point, start + segment * projection);
    }

    private static Vector2 Rotate(Vector2 point, float radians)
    {
        float cosine = Mathf.Cos(radians);
        float sine = Mathf.Sin(radians);
        return new Vector2(
            point.x * cosine - point.y * sine,
            point.x * sine + point.y * cosine);
    }

    private static void GetMotifColors(int themeId, out Color primary, out Color secondary)
    {
        switch (themeId)
        {
            case 1:
                primary = new Color(1f, 0.38f, 0.31f);
                secondary = new Color(1f, 0.78f, 0.48f);
                break;
            case 2:
                primary = new Color(0.58f, 0.54f, 1f);
                secondary = new Color(1f, 0.80f, 0.38f);
                break;
            case 3:
                primary = new Color(0.50f, 0.92f, 0.52f);
                secondary = new Color(1f, 0.72f, 0.56f);
                break;
            case 4:
                primary = new Color(1f, 0.42f, 0.67f);
                secondary = new Color(1f, 0.90f, 0.61f);
                break;
            case 5:
                primary = new Color(0.94f, 0.70f, 0.29f);
                secondary = new Color(0.48f, 0.82f, 0.78f);
                break;
            default:
                primary = new Color(0.43f, 0.80f, 0.48f);
                secondary = new Color(0.98f, 0.72f, 0.28f);
                break;
        }
    }

    private static string GetThemeName(int themeId)
    {
        switch (themeId)
        {
            case 1: return "Coral";
            case 2: return "Astral";
            case 3: return "Garden";
            case 4: return "Sweets";
            case 5: return "Clockwork";
            default: return "Forest";
        }
    }

    private static string GetMotifName(int themeId, int variant)
    {
        if (themeId == 0)
            return variant == 0 ? "Leaf" : "Sprig";
        if (themeId == 1)
            return variant == 0 ? "Coral" : "Shell";
        if (themeId == 2)
            return variant == 0 ? "Star" : "Comet";
        if (themeId == 3)
            return variant == 0 ? "Flower" : "Leaf";
        if (themeId == 4)
            return variant == 0 ? "Candy" : "SugarStar";
        return variant == 0 ? "Gear" : "Clock";
    }

    private static Texture2D BuildTexture(int themeId, int seed)
    {
        Color baseTop;
        Color baseBottom;
        Color accent;
        Color secondary;
        switch (themeId)
        {
            case 1:
                baseTop = new Color(0.34f, 0.08f, 0.11f);
                baseBottom = new Color(0.62f, 0.20f, 0.18f);
                accent = new Color(1f, 0.48f, 0.34f);
                secondary = new Color(1f, 0.80f, 0.56f);
                break;
            case 2:
                baseTop = new Color(0.025f, 0.035f, 0.13f);
                baseBottom = new Color(0.13f, 0.07f, 0.28f);
                accent = new Color(0.53f, 0.48f, 1f);
                secondary = new Color(1f, 0.78f, 0.40f);
                break;
            case 3:
                baseTop = new Color(0.035f, 0.16f, 0.09f);
                baseBottom = new Color(0.16f, 0.38f, 0.18f);
                accent = new Color(0.50f, 0.92f, 0.52f);
                secondary = new Color(1f, 0.72f, 0.56f);
                break;
            case 4:
                baseTop = new Color(0.24f, 0.035f, 0.14f);
                baseBottom = new Color(0.48f, 0.12f, 0.29f);
                accent = new Color(1f, 0.42f, 0.67f);
                secondary = new Color(1f, 0.90f, 0.61f);
                break;
            case 5:
                baseTop = new Color(0.12f, 0.07f, 0.035f);
                baseBottom = new Color(0.34f, 0.20f, 0.08f);
                accent = new Color(0.94f, 0.70f, 0.29f);
                secondary = new Color(0.48f, 0.82f, 0.78f);
                break;
            default:
                baseTop = new Color(0.025f, 0.12f, 0.09f);
                baseBottom = new Color(0.08f, 0.30f, 0.20f);
                accent = new Color(0.40f, 0.72f, 0.43f);
                secondary = new Color(0.95f, 0.70f, 0.27f);
                break;
        }

        Color32[] pixels = new Color32[TextureSize * TextureSize];
        System.Random random = new System.Random(92821 + themeId * 7919 + seed * 104729);

        for (int y = 0; y < TextureSize; y++)
        {
            float vertical = y / (TextureSize - 1f);
            for (int x = 0; x < TextureSize; x++)
            {
                float horizontal = x / (TextureSize - 1f);
                float vignette = Mathf.Clamp01(
                    1f - 0.56f * Vector2.Distance(
                        new Vector2(horizontal, vertical),
                        new Vector2(0.5f, 0.48f)));
                float grain = (float)random.NextDouble() * 0.018f;
                Color color = Color.Lerp(baseBottom, baseTop, vertical);
                color *= 0.78f + vignette * 0.22f + grain;
                pixels[y * TextureSize + x] = color;
            }
        }

        if (themeId == 0 || themeId == 3)
            DrawForest(pixels, random, accent, secondary);
        else if (themeId == 1 || themeId == 4)
            DrawCoralWorld(pixels, random, accent, secondary);
        else
            DrawConstellation(pixels, random, accent, secondary);

        Texture2D result = new Texture2D(
            TextureSize, TextureSize, TextureFormat.RGBA32, false)
        {
            name = $"MementoBackdropTexture_{themeId}_{seed}",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.DontSave
        };
        result.SetPixels32(pixels);
        result.Apply(false, true);
        return result;
    }

    private static void DrawForest(
        Color32[] pixels,
        System.Random random,
        Color leafColor,
        Color lightColor)
    {
        for (int i = 0; i < 28; i++)
        {
            int x = random.Next(-35, TextureSize + 35);
            int y = random.Next(-25, TextureSize + 25);
            int radiusX = random.Next(12, 35);
            int radiusY = random.Next(22, 54);
            float alpha = 0.08f + (float)random.NextDouble() * 0.12f;
            DrawEllipse(pixels, x, y, radiusX, radiusY, leafColor, alpha, i % 2 == 0);
        }

        for (int ray = 0; ray < 4; ray++)
        {
            int start = 40 + ray * 116 + random.Next(-24, 25);
            DrawDiagonalBand(pixels, start, 22 + ray * 4, lightColor, 0.045f);
        }
    }

    private static void DrawCoralWorld(
        Color32[] pixels,
        System.Random random,
        Color coral,
        Color brass)
    {
        for (int cluster = 0; cluster < 11; cluster++)
        {
            Vector2Int root = new Vector2Int(
                random.Next(-20, TextureSize + 20),
                random.Next(-10, TextureSize + 40));
            int height = random.Next(34, 82);
            Vector2Int crown = new Vector2Int(root.x, root.y + height);
            DrawLine(pixels, root, crown, coral, 0.15f);
            DrawLine(
                pixels,
                new Vector2Int(root.x, root.y + height / 2),
                new Vector2Int(root.x - random.Next(18, 42), root.y + height),
                cluster % 3 == 0 ? brass : coral,
                0.13f);
            DrawLine(
                pixels,
                new Vector2Int(root.x, root.y + height / 3),
                new Vector2Int(root.x + random.Next(18, 42), root.y + height - 8),
                cluster % 4 == 0 ? brass : coral,
                0.13f);
        }

        for (int bubble = 0; bubble < 14; bubble++)
        {
            DrawRing(
                pixels,
                random.Next(0, TextureSize),
                random.Next(0, TextureSize),
                random.Next(5, 18),
                1,
                bubble % 4 == 0 ? brass : coral,
                0.08f);
        }
    }

    private static void DrawConstellation(
        Color32[] pixels,
        System.Random random,
        Color violet,
        Color gold)
    {
        Vector2Int[] stars = new Vector2Int[42];
        for (int i = 0; i < stars.Length; i++)
        {
            stars[i] = new Vector2Int(
                random.Next(18, TextureSize - 18),
                random.Next(18, TextureSize - 18));
            int radius = i % 7 == 0 ? 3 : 1;
            DrawDisc(pixels, stars[i].x, stars[i].y, radius, i % 5 == 0 ? gold : Color.white, 0.72f);
        }

        for (int i = 1; i < stars.Length; i += 4)
            DrawLine(pixels, stars[i - 1], stars[i], violet, 0.20f);

        DrawRing(pixels, 395, 118, 94, 2, violet, 0.14f);
        DrawRing(pixels, 395, 118, 58, 1, gold, 0.12f);
    }

    private static void DrawDiagonalBand(
        Color32[] pixels, int offset, int width, Color color, float alpha)
    {
        for (int y = 0; y < TextureSize; y++)
        {
            int center = offset + y / 3;
            for (int dx = -width; dx <= width; dx++)
                Blend(pixels, center + dx, y, color, alpha * (1f - Mathf.Abs(dx) / (width + 1f)));
        }
    }

    private static void DrawEllipse(
        Color32[] pixels,
        int centerX,
        int centerY,
        int radiusX,
        int radiusY,
        Color color,
        float alpha,
        bool tilt)
    {
        for (int y = -radiusY; y <= radiusY; y++)
        {
            for (int x = -radiusX; x <= radiusX; x++)
            {
                float sampleX = tilt ? x + y * 0.24f : x - y * 0.24f;
                float distance =
                    sampleX * sampleX / (radiusX * radiusX) +
                    y * y / (float)(radiusY * radiusY);
                if (distance <= 1f)
                    Blend(pixels, centerX + x, centerY + y, color, alpha * (1f - distance) * 0.9f);
            }
        }
    }

    private static void DrawRing(
        Color32[] pixels,
        int centerX,
        int centerY,
        int radius,
        int thickness,
        Color color,
        float alpha)
    {
        int outer = radius + thickness;
        int inner = Mathf.Max(0, radius - thickness);
        int outerSquared = outer * outer;
        int innerSquared = inner * inner;
        for (int y = -outer; y <= outer; y++)
        {
            for (int x = -outer; x <= outer; x++)
            {
                int square = x * x + y * y;
                if (square <= outerSquared && square >= innerSquared)
                    Blend(pixels, centerX + x, centerY + y, color, alpha);
            }
        }
    }

    private static void DrawDisc(
        Color32[] pixels, int centerX, int centerY, int radius, Color color, float alpha)
    {
        int squared = radius * radius;
        for (int y = -radius; y <= radius; y++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                if (x * x + y * y <= squared)
                    Blend(pixels, centerX + x, centerY + y, color, alpha);
            }
        }
    }

    private static void DrawLine(
        Color32[] pixels, Vector2Int from, Vector2Int to, Color color, float alpha)
    {
        int steps = Mathf.Max(Mathf.Abs(to.x - from.x), Mathf.Abs(to.y - from.y));
        if (steps <= 0)
            return;

        for (int i = 0; i <= steps; i++)
        {
            float t = i / (float)steps;
            int x = Mathf.RoundToInt(Mathf.Lerp(from.x, to.x, t));
            int y = Mathf.RoundToInt(Mathf.Lerp(from.y, to.y, t));
            Blend(pixels, x, y, color, alpha);
        }
    }

    private static void Blend(Color32[] pixels, int x, int y, Color color, float alpha)
    {
        if (x < 0 || x >= TextureSize || y < 0 || y >= TextureSize)
            return;

        int index = y * TextureSize + x;
        Color current = pixels[index];
        pixels[index] = Color.Lerp(current, color, Mathf.Clamp01(alpha));
    }

    private void OnDestroy()
    {
        ReleaseGeneratedAssets();
    }

    private void ReleaseGeneratedAssets()
    {
        ReleaseMotifs();
        if (target != null && target.sprite == sprite)
            target.sprite = null;
        SafeDestroy(sprite);
        SafeDestroy(texture);
        sprite = null;
        texture = null;
    }

    private void ReleaseMotifs()
    {
        for (int i = 0; i < motifs.Count; i++)
        {
            if (motifs[i].Rect != null)
                SafeDestroy(motifs[i].Rect.gameObject);
        }
        motifs.Clear();

        for (int i = 0; i < motifSprites.Count; i++)
            SafeDestroy(motifSprites[i]);
        motifSprites.Clear();

        for (int i = 0; i < motifTextures.Count; i++)
            SafeDestroy(motifTextures[i]);
        motifTextures.Clear();
    }

    private static void SafeDestroy(UnityEngine.Object asset)
    {
        if (asset == null)
            return;

        if (Application.isPlaying)
            Destroy(asset);
        else
            DestroyImmediate(asset);
    }
}
