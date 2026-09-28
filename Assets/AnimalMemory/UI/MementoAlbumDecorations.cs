using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// Read-only album decoration using the exact sprites shown by the collection and board.
/// No generated substitutes, texture copies, tinting, progress changes or input handlers.
/// </summary>
public sealed class MementoAlbumDecorations : VisualElement
{
    private static readonly int[] Sets = { 0, 0, 1, 2, 3, 4, 5 };
    private static readonly int[] Faces = { 0, 0, 0, 1, 0, 0, 2 };
    private readonly List<VisualElement> stickers = new List<VisualElement>();

    public MementoAlbumDecorations(Func<int, IReadOnlyList<Sprite>> getSprites)
    {
        if (getSprites == null) throw new ArgumentNullException(nameof(getSprites));
        name = "album-drawings";
        pickingMode = PickingMode.Ignore;
        style.position = Position.Absolute;
        style.left = 0;
        style.right = 0;
        style.top = 0;
        style.bottom = 0;
        style.overflow = Overflow.Hidden;
        for (int i = 0; i < Sets.Length; i++)
        {
            IReadOnlyList<Sprite> sprites = getSprites(Sets[i]);
            Sprite sprite = sprites != null && Faces[i] < sprites.Count ? sprites[Faces[i]] : null;
            var sticker = new VisualElement { name = "album-sticker-" + i, pickingMode = PickingMode.Ignore };
            sticker.style.position = Position.Absolute;
            // The sprite already has alpha. Never add a paper plate behind it.
            sticker.style.backgroundColor = Color.clear;
            var face = new VisualElement { name = "album-face-" + i, pickingMode = PickingMode.Ignore };
            face.style.position = Position.Absolute;
            face.style.left = face.style.right = face.style.top = face.style.bottom = 8;
            face.style.unityBackgroundScaleMode = ScaleMode.ScaleToFit;
            if (sprite != null)
                face.style.backgroundImage = new StyleBackground(sprite);
            else
                sticker.style.display = DisplayStyle.None;
            sticker.Add(face);
            stickers.Add(sticker);
            Add(sticker);
        }
        RegisterCallback<GeometryChangedEvent>(OnGeometryChanged);
    }

    private void OnGeometryChanged(GeometryChangedEvent evt)
    {
        for (int i = 0; i < stickers.Count; i++)
        {
            Rect rect = GetPlacement(i, evt.newRect.size);
            stickers[i].style.left = rect.x;
            stickers[i].style.top = rect.y;
            stickers[i].style.width = rect.width;
            stickers[i].style.height = rect.height;
        }
    }

    /// <summary>Resolution-aware square placements. Artwork is never stretched or flipped.</summary>
    public static Rect GetPlacement(int index, Vector2 viewport)
    {
        if (index < 0 || index >= Sets.Length) throw new ArgumentOutOfRangeException(nameof(index));
        float width = Mathf.Max(0, viewport.x), height = Mathf.Max(0, viewport.y);
        bool portrait = height > width;
        Vector2[] points = portrait
            ? new[] { new Vector2(.15f, .17f), new Vector2(.37f, .19f),
                new Vector2(.82f, .30f), new Vector2(.64f, .16f),
                new Vector2(.84f, .17f), new Vector2(.12f, .43f), new Vector2(.88f, .44f) }
            : new[] { new Vector2(.14f, .15f), new Vector2(.25f, .15f),
                new Vector2(.075f, .72f), new Vector2(.57f, .24f),
                new Vector2(.92f, .33f), new Vector2(.90f, .66f), new Vector2(.60f, .68f) };
        float size = Mathf.Min(Mathf.Min(width, height), Mathf.Clamp(Mathf.Min(width, height) * .16f, 40, 154));
        Vector2 center = points[index];
        return new Rect(Mathf.Clamp(center.x * width - size * .5f, 0, width - size),
            Mathf.Clamp(center.y * height - size * .5f, 0, height - size), size, size);
    }
}
