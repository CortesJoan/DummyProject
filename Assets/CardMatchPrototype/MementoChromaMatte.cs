using System;
using UnityEngine;

/// <summary>
/// Extracts a deliberately keyed production source without treating brightness as background.
/// Only use the declared key when that saturated colour is absent from the painted subject.
/// This is not a general-purpose remover for photographs or baked checkerboards.
/// </summary>
public static class MementoChromaMatte
{
    public enum Key { Green, Magenta, Cyan }

    /// <summary>Returns new RGBA pixels. Source pixels and intentional neutral colours are never edited.</summary>
    public static Color32[] Extract(Color32[] source, int width, int height, Key key, bool reduceEdgeSpill = false)
    {
        if (source == null) throw new ArgumentNullException(nameof(source));
        if (width <= 0 || height <= 0 || (long)width * height != source.Length)
            throw new ArgumentException("Pixel dimensions do not match the source.");

        var result = (Color32[])source.Clone();
        var background = new bool[source.Length];
        for (int i = 0; i < source.Length; i++)
        {
            background[i] = IsCore(source[i], key);
            if (background[i]) result[i] = new Color32(0, 0, 0, 0);
        }

        Vector3 keyRgb = KeyRgb(key);
        for (int y = 0; y < height; y++)
        for (int x = 0; x < width; x++)
        {
            int index = y * width + x;
            Color32 pixel = source[index];
            if (background[index] || pixel.a == 0 || Excess(pixel, key) <= 12)
                continue;
            if (!HasBackground(background, width, height, x, y, 3))
                continue;

            // Solve C = alpha*F + (1-alpha)*K using the nearest uncontaminated
            // foreground sample. Unlike a luma threshold, white F remains white.
            if (!TryFindForeground(source, background, width, height, x, y, key, out Color32 foreground))
                continue;
            Vector3 colour = Rgb(pixel);
            Vector3 direction = Rgb(foreground) - keyRgb;
            float denominator = Vector3.Dot(direction, direction);
            if (denominator < 0.0001f) continue;
            float alpha = Mathf.Clamp01(Vector3.Dot(colour - keyRgb, direction) / denominator);
            if (alpha < 0.01f)
            {
                result[index] = new Color32(0, 0, 0, 0);
                continue;
            }
            Vector3 clean = (colour - keyRgb * (1f - alpha)) / alpha;
            result[index] = new Color32(
                Byte(clean.x), Byte(clean.y), Byte(clean.z),
                (byte)Mathf.RoundToInt(pixel.a * alpha));
        }
        if (reduceEdgeSpill)
        {
            // Optional character-sheet cleanup. Change colour, NEVER opacity, and
            // only within three pixels of the declared saturated background.
            // Interior paint and card extraction retain their existing treatment.
            for (int y = 0; y < height; y++)
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                Color32 pixel = result[index];
                int excess = Excess(pixel, key);
                if (pixel.a == 0 || excess <= 12 ||
                    !HasBackground(background, width, height, x, y, 3)) continue;
                if (key == Key.Green)
                    pixel.g = (byte)Math.Max(pixel.r, pixel.b);
                else if (key == Key.Magenta)
                {
                    pixel.r = (byte)(pixel.r - excess);
                    pixel.b = (byte)(pixel.b - excess);
                }
                else
                {
                    pixel.g = (byte)(pixel.g - excess);
                    pixel.b = (byte)(pixel.b - excess);
                }
                result[index] = pixel;
            }
        }
        return result;
    }

    /// <summary>
    /// Creates a transient RGBA texture, including a GPU readback for non-readable imported
    /// textures. The caller owns the returned texture and must destroy it when no longer used.
    /// Nothing is written to disk and the imported source remains untouched.
    /// </summary>
    public static Texture2D CreateTexture(Texture2D source, Key key, bool reduceEdgeSpill = false)
    {
        if (source == null) return null;
        Color32[] pixels = ReadPixels(source);
        var output = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false)
        {
            name = source.name + "_rgba",
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.DontSave
        };
        output.SetPixels32(Extract(pixels, source.width, source.height, key, reduceEdgeSpill));
        output.Apply(false, false);
        return output;
    }

    private static Color32[] ReadPixels(Texture2D source)
    {
        if (source.isReadable) return source.GetPixels32();
        RenderTexture previous = RenderTexture.active;
        RenderTexture temporary = RenderTexture.GetTemporary(
            source.width, source.height, 0, RenderTextureFormat.ARGB32,
            RenderTextureReadWrite.sRGB);
        Texture2D readable = null;
        try
        {
            Graphics.Blit(source, temporary);
            RenderTexture.active = temporary;
            readable = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            readable.ReadPixels(new Rect(0, 0, source.width, source.height), 0, 0);
            readable.Apply(false, false);
            return readable.GetPixels32();
        }
        finally
        {
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(temporary);
            if (readable != null)
            {
                if (Application.isPlaying) UnityEngine.Object.Destroy(readable);
                else UnityEngine.Object.DestroyImmediate(readable);
            }
        }
    }

    private static bool IsCore(Color32 p, Key key)
    {
        if (key == Key.Green) return p.g >= 180 && p.r <= 60 && p.b <= 60;
        if (key == Key.Magenta) return p.r >= 180 && p.b >= 180 && p.g <= 60;
        return p.g >= 180 && p.b >= 180 && p.r <= 60;
    }

    private static int Excess(Color32 p, Key key)
    {
        if (key == Key.Green) return p.g - Math.Max(p.r, p.b);
        if (key == Key.Magenta) return Math.Min(p.r, p.b) - p.g;
        return Math.Min(p.g, p.b) - p.r;
    }

    private static Vector3 KeyRgb(Key key)
    {
        if (key == Key.Green) return new Vector3(0, 1, 0);
        if (key == Key.Magenta) return new Vector3(1, 0, 1);
        return new Vector3(0, 1, 1);
    }

    private static Vector3 Rgb(Color32 p) => new Vector3(p.r / 255f, p.g / 255f, p.b / 255f);
    private static byte Byte(float value) => (byte)Mathf.RoundToInt(Mathf.Clamp01(value) * 255f);

    private static bool HasBackground(bool[] mask, int width, int height, int x, int y, int radius)
    {
        for (int dy = -radius; dy <= radius; dy++)
        for (int dx = -radius; dx <= radius; dx++)
        {
            int px = x + dx, py = y + dy;
            if (px >= 0 && px < width && py >= 0 && py < height && mask[py * width + px])
                return true;
        }
        return false;
    }

    private static bool TryFindForeground(Color32[] source, bool[] background,
        int width, int height, int x, int y, Key key, out Color32 foreground)
    {
        for (int radius = 1; radius <= 4; radius++)
        for (int dy = -radius; dy <= radius; dy++)
        for (int dx = -radius; dx <= radius; dx++)
        {
            if (Math.Max(Math.Abs(dx), Math.Abs(dy)) != radius) continue;
            int px = x + dx, py = y + dy;
            if (px < 0 || px >= width || py < 0 || py >= height) continue;
            int index = py * width + px;
            if (background[index] || source[index].a < 250 || Excess(source[index], key) > 12)
                continue;
            foreground = source[index];
            return true;
        }
        foreground = default;
        return false;
    }
}
