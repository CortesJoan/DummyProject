using System;
using System.Collections.Generic;
using AnimalMemory.Progression;
using UnityEngine;

/// <summary>Owns versioned illustrated card textures. Original animal assets never enter this path.</summary>
public static class MementoIllustratedCards
{
    private static readonly Sprite[][] Sets = new Sprite[AnimalMemoryContentIds.SetCount][];
    private static readonly bool[] Attempted = new bool[AnimalMemoryContentIds.SetCount];
    private static readonly string[] Sources =
    {
        null, "coral-atlas-magenta-v2", "constellation-atlas-green-v2",
        "garden-atlas-magenta-v2", "sweets-atlas-green-v2", "clockwork-atlas-green-v2"
    };

    public static IReadOnlyList<Sprite> GetSprites(int setId)
    {
        if (setId > AnimalMemoryContentIds.ForestSet && setId < Sources.Length)
        {
            if (!Attempted[setId])
            {
                Attempted[setId] = true;
                Sets[setId] = LoadSet(setId);
            }
            if (Sets[setId] != null) return Sets[setId];
        }
        return MementoMatchCardArtFactory.GetSprites(setId);
    }

    private static Sprite[] LoadSet(int setId)
    {
        Texture2D source = Resources.Load<Texture2D>("AnimalMemory/ArtV2/" + Sources[setId]);
        if (source == null) return null;
        Texture2D matte = null;
        var sprites = new List<Sprite>();
        Texture2D pending = null;
        try
        {
            MementoChromaMatte.Key key = setId == AnimalMemoryContentIds.CoralSet ||
                setId == AnimalMemoryContentIds.GardenSet
                ? MementoChromaMatte.Key.Magenta : MementoChromaMatte.Key.Green;
            matte = MementoChromaMatte.CreateTexture(source, key);
            Color32[] pixels = matte.GetPixels32();
            RectInt[] cells = MementoIllustratedAtlasLayout.BuildRects(
                pixels, matte.width, matte.height, 5, 3);
            int size = 0;
            foreach (RectInt cell in cells) size = Mathf.Max(size, cell.width, cell.height);
            foreach (RectInt cell in cells)
            {
                // Identical square canvases prevent varying grid proportions.
                var output = new Color32[size * size];
                int dx = (size - cell.width) / 2, dy = (size - cell.height) / 2;
                for (int y = 0; y < cell.height; y++)
                    Array.Copy(pixels, (cell.y + y) * matte.width + cell.x,
                        output, (dy + y) * size + dx, cell.width);
                pending = new Texture2D(size, size, TextureFormat.RGBA32, false)
                {
                    name = AnimalMemoryCardIdentityCatalog.GetIdentityKey(
                        setId, sprites.Count),
                    hideFlags = HideFlags.DontSave,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
                pending.SetPixels32(output);
                pending.Apply(false, true);
                Sprite sprite = Sprite.Create(pending, new Rect(0, 0, size, size),
                    new Vector2(0.5f, 0.5f), 100f, 0, SpriteMeshType.FullRect);
                sprite.name = pending.name;
                sprite.hideFlags = HideFlags.DontSave;
                sprites.Add(sprite);
                pending = null;
            }
            return sprites.ToArray();
        }
        catch (Exception exception)
        {
            foreach (Sprite sprite in sprites) { Release(sprite.texture); Release(sprite); }
            Release(pending);
            Debug.LogWarning("Illustrated set " + setId + " unavailable; using existing art. " + exception.Message);
            return null;
        }
        finally
        {
            Release(matte);
            Resources.UnloadAsset(source);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCache()
    {
        for (int setId = 0; setId < Sets.Length; setId++)
        {
            if (Sets[setId] != null)
                foreach (Sprite sprite in Sets[setId])
                    if (sprite != null) { Release(sprite.texture); Release(sprite); }
            Sets[setId] = null;
            Attempted[setId] = false;
        }
    }

    private static void Release(UnityEngine.Object value)
    {
        if (value == null) return;
        if (Application.isPlaying) UnityEngine.Object.Destroy(value);
        else UnityEngine.Object.DestroyImmediate(value);
    }
}
