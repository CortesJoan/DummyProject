using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Removes generated pale backdrops by topology: only pale pixels that are
/// reachable from the transparent exterior are cleared. Painted whites inside
/// the character, including eyes, clothes and highlights, remain protected.
/// </summary>
public static class MementoMatchPortraitMatteCleaner
{
    private const int BackdropMinimum = 214;
    private const int BackdropSpread = 45;
    private const int FringeMinimum = 210;
    private const int FringeSpread = 45;

    public static void CleanInPlace(
        Texture2D texture,
        bool removeOuterPaleArtifacts = false)
    {
        if (texture == null || !texture.isReadable)
            return;

        Color32[] pixels = texture.GetPixels32();
        CleanPixels(pixels, texture.width, texture.height);
        texture.SetPixels32(pixels);
        texture.Apply(false, false);
    }

    public static Sprite CreateCleanedSprite(
        Sprite source,
        bool removeOuterPaleArtifacts = false)
    {
        if (source == null || source.texture == null ||
            !source.texture.isReadable)
        {
            return source;
        }

        Rect rect = source.rect;
        int width = Mathf.RoundToInt(rect.width);
        int height = Mathf.RoundToInt(rect.height);
        int x = Mathf.RoundToInt(rect.x);
        int y = Mathf.RoundToInt(rect.y);
        Color32[] pixels = source.texture.GetPixels32();
        Color32[] cropped = new Color32[width * height];
        int atlasWidth = source.texture.width;

        for (int row = 0; row < height; row++)
        {
            int sourceOffset = (y + row) * atlasWidth + x;
            int targetOffset = row * width;
            System.Array.Copy(
                pixels,
                sourceOffset,
                cropped,
                targetOffset,
                width);
        }

        CleanPixels(cropped, width, height);
        Texture2D cleanedTexture = new Texture2D(
            width,
            height,
            TextureFormat.RGBA32,
            false)
        {
            name = source.name + "_clean",
            filterMode = source.texture.filterMode,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.DontSave
        };
        cleanedTexture.SetPixels32(cropped);
        cleanedTexture.Apply(false, false);

        Vector2 normalizedPivot = new Vector2(
            source.pivot.x / Mathf.Max(1f, rect.width),
            source.pivot.y / Mathf.Max(1f, rect.height));
        Sprite cleaned = Sprite.Create(
            cleanedTexture,
            new Rect(0f, 0f, width, height),
            normalizedPivot,
            source.pixelsPerUnit,
            0,
            SpriteMeshType.FullRect,
            source.border);
        cleaned.name = source.name;
        cleaned.hideFlags = HideFlags.DontSave;
        return cleaned;
    }

    public static void CleanPixels(Color32[] pixels, int width, int height)
    {
        if (pixels == null || width <= 0 || height <= 0 ||
            pixels.Length != width * height)
        {
            return;
        }

        ClearBackdropConnectedToCanvasEdge(pixels, width, height);
        ClearPaleMatteConnectedToTransparency(pixels, width, height);
    }

    private static void ClearBackdropConnectedToCanvasEdge(
        Color32[] pixels,
        int width,
        int height)
    {
        bool[] visited = new bool[pixels.Length];
        Queue<int> pending = new Queue<int>();

        for (int x = 0; x < width; x++)
        {
            EnqueuePale(x, 0);
            EnqueuePale(x, height - 1);
        }

        for (int y = 0; y < height; y++)
        {
            EnqueuePale(0, y);
            EnqueuePale(width - 1, y);
        }

        while (pending.Count > 0)
        {
            int index = pending.Dequeue();
            int x = index % width;
            int y = index / width;
            pixels[index] = new Color32(0, 0, 0, 0);

            EnqueuePale(x - 1, y);
            EnqueuePale(x + 1, y);
            EnqueuePale(x, y - 1);
            EnqueuePale(x, y + 1);
            EnqueuePale(x - 1, y - 1);
            EnqueuePale(x + 1, y - 1);
            EnqueuePale(x - 1, y + 1);
            EnqueuePale(x + 1, y + 1);
        }

        void EnqueuePale(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return;

            int index = y * width + x;
            if (visited[index] ||
                pixels[index].a == 0 ||
                !IsPaleNeutral(
                    pixels[index],
                    BackdropMinimum,
                    BackdropSpread))
            {
                return;
            }

            visited[index] = true;
            pending.Enqueue(index);
        }
    }

    private static void ClearPaleMatteConnectedToTransparency(
        Color32[] pixels,
        int width,
        int height)
    {
        bool[] visited = new bool[pixels.Length];
        Queue<int> pending = new Queue<int>();

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int index = y * width + x;
                if (pixels[index].a != 0)
                    continue;

                EnqueuePale(x - 1, y);
                EnqueuePale(x + 1, y);
                EnqueuePale(x, y - 1);
                EnqueuePale(x, y + 1);
                EnqueuePale(x - 1, y - 1);
                EnqueuePale(x + 1, y - 1);
                EnqueuePale(x - 1, y + 1);
                EnqueuePale(x + 1, y + 1);
            }
        }

        while (pending.Count > 0)
        {
            int index = pending.Dequeue();
            int x = index % width;
            int y = index / width;
            pixels[index] = new Color32(0, 0, 0, 0);

            EnqueuePale(x - 1, y);
            EnqueuePale(x + 1, y);
            EnqueuePale(x, y - 1);
            EnqueuePale(x, y + 1);
            EnqueuePale(x - 1, y - 1);
            EnqueuePale(x + 1, y - 1);
            EnqueuePale(x - 1, y + 1);
            EnqueuePale(x + 1, y + 1);
        }

        void EnqueuePale(int x, int y)
        {
            if (x < 0 || x >= width || y < 0 || y >= height)
                return;

            int index = y * width + x;
            if (visited[index] ||
                pixels[index].a == 0 ||
                !IsPaleNeutral(
                    pixels[index],
                    FringeMinimum,
                    FringeSpread))
            {
                return;
            }

            visited[index] = true;
            pending.Enqueue(index);
        }
    }

    private static bool IsPaleNeutral(
        Color32 color,
        int minimumBrightness,
        int maximumSpread)
    {
        int minimum = Mathf.Min(color.r, Mathf.Min(color.g, color.b));
        int maximum = Mathf.Max(color.r, Mathf.Max(color.g, color.b));
        return minimum >= minimumBrightness &&
            maximum - minimum <= maximumSpread;
    }
}
