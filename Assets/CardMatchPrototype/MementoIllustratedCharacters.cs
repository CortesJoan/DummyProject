using System;
using System.Collections.Generic;
using AnimalMemory.Progression;
using UnityEngine;

/// <summary>Shared, lazily loaded character art. Views borrow sprites; this cache owns them.</summary>
public static class MementoIllustratedCharacters
{
    private static readonly string[] States =
        { "neutral", "thinking", "ability", "damage", "defeated", "victory" };
    private readonly struct Sheet
    {
        public readonly string Source;
        public readonly MementoChromaMatte.Key Key;

        public Sheet(string source, MementoChromaMatte.Key key)
        {
            Source = source;
            Key = key;
        }
    }

    // Only reviewed production sheets are named here; candidates are never auto-discovered.
    // Ids 6..10 are the postgame opponents. They are registered so the cache can serve them,
    // and they keep the legacy duel art until their reviewed sheet exists under Resources:
    // a registered id with no asset still resolves to null, exactly like an unregistered one.
    private static readonly Dictionary<int, Sheet> Sheets = new Dictionary<int, Sheet>
    {
        { 0, new Sheet("aki-poses-magenta-v2", MementoChromaMatte.Key.Magenta) },
        { 1, new Sheet("mika-poses-green-v2", MementoChromaMatte.Key.Green) },
        { 2, new Sheet("yoru-poses-green-v2", MementoChromaMatte.Key.Green) },
        { 3, new Sheet("hana-poses-magenta-v2", MementoChromaMatte.Key.Magenta) },
        { 4, new Sheet("momo-poses-green-v2", MementoChromaMatte.Key.Green) },
        { 5, new Sheet("rei-poses-green-v3", MementoChromaMatte.Key.Green) },
        { MementoPostgameEncounters.FirstOpponentId,
            new Sheet("custodia-calculo-poses-green-v1", MementoChromaMatte.Key.Green) },
        { MementoPostgameEncounters.FirstOpponentId + 1,
            new Sheet("custodia-azar-poses-green-placeholder-v0", MementoChromaMatte.Key.Green) },
        { MementoPostgameEncounters.FirstOpponentId + 2,
            new Sheet("custodia-vinculo-poses-green-v1", MementoChromaMatte.Key.Green) },
        { MementoPostgameEncounters.FirstOpponentId + 3,
            new Sheet("custodia-orgullo-poses-green-v1", MementoChromaMatte.Key.Green) },
        { MementoPostgameEncounters.LastOpponentId,
            new Sheet("creador-poses-green-placeholder-v0", MementoChromaMatte.Key.Green) }
    };
    private static readonly Dictionary<int, Dictionary<string, Sprite>> Guides =
        new Dictionary<int, Dictionary<string, Sprite>>();
    private static readonly HashSet<int> Attempted = new HashSet<int>();

    /// <summary>Whether this id has a reviewed sheet slot, produced or not yet produced.</summary>
    public static bool IsRegistered(int guideId) => Sheets.ContainsKey(guideId);

    /// <summary>Resource name of a registered sheet, or null. Lets tests tell "not produced yet" from "broken".</summary>
    public static string GetSheetSource(int guideId) =>
        Sheets.TryGetValue(guideId, out Sheet sheet) ? sheet.Source : null;

    public static Sprite GetSprite(int guideId, string state)
    {
        if (!Sheets.ContainsKey(guideId) || state == null ||
            Array.IndexOf(States, state) < 0) return null;
        if (Attempted.Add(guideId)) LoadGuide(guideId);
        return Guides.TryGetValue(guideId, out Dictionary<string, Sprite> poses) &&
            poses.TryGetValue(state, out Sprite sprite) ? sprite : null;
    }

    /// <summary>Maps existing gameplay presentation values without changing gameplay state.</summary>
    public static string StateForPose(int pose)
    {
        switch (pose)
        {
            case 1: return "thinking";
            case 2: return "ability";
            case 3: return "defeated";
            case 4: return "victory";
            case 5: return "damage";
            default: return "neutral";
        }
    }

    /// <summary>Whether a view borrows this texture from the shared character cache.</summary>
    public static bool OwnsTexture(Texture2D texture)
    {
        if (texture == null) return false;
        foreach (Dictionary<string, Sprite> poses in Guides.Values)
            foreach (Sprite sprite in poses.Values)
                if (sprite != null && sprite.texture == texture) return true;
        return false;
    }

    private static void LoadGuide(int guideId)
    {
        Sheet sheet = Sheets[guideId];
        Texture2D source = Resources.Load<Texture2D>("AnimalMemory/ArtV2/" + sheet.Source);
        if (source == null) return;
        var poses = new Dictionary<string, Sprite>();
        Texture2D matte = null;
        Texture2D pending = null;
        try
        {
            matte = MementoChromaMatte.CreateTexture(source, sheet.Key, true);
            Color32[] pixels = matte.GetPixels32();
            RectInt[] cells = MementoIllustratedAtlasLayout.BuildRects(
                pixels, matte.width, matte.height, 3, 2);
            int width = 0, height = 0;
            foreach (RectInt cell in cells)
            {
                width = Mathf.Max(width, cell.width);
                height = Mathf.Max(height, cell.height);
            }
            RectInt[] content = null;
            int baseline = 0;
            if (guideId != 5)
            {
                content = new RectInt[cells.Length];
                int contentWidth = 0, top = 0;
                baseline = int.MaxValue;
                for (int i = 0; i < cells.Length; i++)
                {
                    RectInt bounds = ContentBounds(pixels, matte.width, cells[i]);
                    content[i] = bounds;
                    contentWidth = Mathf.Max(contentWidth, bounds.width);
                    baseline = Mathf.Min(baseline, bounds.yMin - cells[i].yMin);
                    top = Mathf.Max(top, bounds.yMax - cells[i].yMin);
                }
                // A shared 2:3 frame preserves scale and center across all six poses.
                // Remove only empty source margins, then ADD padding; never trim paint.
                height = top - baseline + 8;
                width = Mathf.Max(contentWidth + 8, Mathf.CeilToInt(height * 2f / 3f));
                height = Mathf.Max(height, Mathf.CeilToInt(width * 1.5f));
            }
            for (int i = 0; i < cells.Length; i++)
            {
                RectInt originalCell = cells[i];
                RectInt cell = content == null ? originalCell : content[i];
                var output = new Color32[width * height];
                int dx = (width - cell.width) / 2;
                int dy = content == null ? (height - cell.height) / 2 :
                    4 + cell.yMin - originalCell.yMin - baseline;
                for (int y = 0; y < cell.height; y++)
                    Array.Copy(pixels, (cell.y + y) * matte.width + cell.x,
                        output, (dy + y) * width + dx, cell.width);
                pending = new Texture2D(width, height, TextureFormat.RGBA32, false)
                {
                    name = sheet.Source + "_" + States[i],
                    hideFlags = HideFlags.DontSave,
                    filterMode = FilterMode.Bilinear,
                    wrapMode = TextureWrapMode.Clamp
                };
                pending.SetPixels32(output);
                // Readability retained for UI face crops, not for background processing per frame.
                pending.Apply(false, false);
                Sprite sprite = Sprite.Create(pending, new Rect(0, 0, width, height),
                    new Vector2(0.5f, 0f), 100f, 0, SpriteMeshType.FullRect);
                sprite.name = pending.name;
                sprite.hideFlags = HideFlags.DontSave;
                poses.Add(States[i], sprite);
                pending = null;
            }
            Guides.Add(guideId, poses);
        }
        catch (Exception exception)
        {
            ClearPoses(poses);
            Release(pending);
            Debug.LogWarning("Illustrated art unavailable for guide " + guideId +
                "; keeping legacy art. " + exception.Message);
        }
        finally
        {
            Release(matte);
            Resources.UnloadAsset(source);
        }
    }

    private static RectInt ContentBounds(Color32[] pixels, int stride, RectInt cell)
    {
        int left = cell.xMax, right = cell.xMin, bottom = cell.yMax, top = cell.yMin;
        for (int y = cell.yMin; y < cell.yMax; y++)
        for (int x = cell.xMin; x < cell.xMax; x++)
        {
            if (pixels[y * stride + x].a == 0) continue;
            left = Mathf.Min(left, x);
            right = Mathf.Max(right, x + 1);
            bottom = Mathf.Min(bottom, y);
            top = Mathf.Max(top, y + 1);
        }
        if (right <= left || top <= bottom)
            throw new InvalidOperationException("Character pose is empty.");
        return new RectInt(left, bottom, right - left, top - bottom);
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetCache()
    {
        Clear();
        Attempted.Clear();
    }

    private static void Clear()
    {
        foreach (Dictionary<string, Sprite> poses in Guides.Values)
            ClearPoses(poses);
        Guides.Clear();
    }

    private static void ClearPoses(Dictionary<string, Sprite> poses)
    {
        foreach (Sprite sprite in poses.Values)
            if (sprite != null) { Release(sprite.texture); Release(sprite); }
        poses.Clear();
    }

    private static void Release(UnityEngine.Object value)
    {
        if (value == null) return;
        if (Application.isPlaying) UnityEngine.Object.Destroy(value);
        else UnityEngine.Object.DestroyImmediate(value);
    }
}
