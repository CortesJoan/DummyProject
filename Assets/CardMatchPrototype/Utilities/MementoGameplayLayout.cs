using UnityEngine;

/// <summary>Presentation geometry in screen pixels; has no access to match state.</summary>
public static class MementoGameplayLayout
{
    public static readonly Vector2 ReferenceResolution = new Vector2(1920f, 1080f);

    public static bool IsPortrait(Vector2 viewport) => viewport.y > viewport.x;

    public static float ScaleMatch(Vector2 viewport) => IsPortrait(viewport) ? 1f : 0.5f;

    public static Rect ValidSafeArea(Vector2 viewport, Rect safeArea)
    {
        Rect screen = new Rect(Vector2.zero, viewport);
        if (safeArea.width <= 0f || safeArea.height <= 0f)
            return screen;
        float left = Mathf.Clamp(safeArea.xMin, 0f, viewport.x);
        float bottom = Mathf.Clamp(safeArea.yMin, 0f, viewport.y);
        float right = Mathf.Clamp(safeArea.xMax, left, viewport.x);
        float top = Mathf.Clamp(safeArea.yMax, bottom, viewport.y);
        return right > left && top > bottom
            ? Rect.MinMaxRect(left, bottom, right, top)
            : screen;
    }

    /// <summary>Reserves real HUD occupancy before selecting the card area.</summary>
    public static Rect BoardArea(
        Rect safeArea, float topInset, float bottomInset, bool duel)
    {
        float margin = Mathf.Min(safeArea.width, safeArea.height) * 0.025f;
        float availableHeight = Mathf.Max(1f, safeArea.height - topInset - bottomInset - margin * 2f);
        Rect available = new Rect(
            safeArea.xMin + margin,
            safeArea.yMin + bottomInset + margin,
            Mathf.Max(1f, safeArea.width - margin * 2f),
            availableHeight);
        bool portrait = safeArea.height > safeArea.width;
        if (duel && portrait)
        {
            // Leave a face-to-face band above the cards; bottom remains above skills/health.
            available.height *= 0.76f;
        }
        else if (duel)
        {
            available.width *= 0.55f;
        }
        else if (!portrait)
        {
            float width = available.width * 0.76f;
            available.x += (available.width - width) * 0.5f;
            available.width = width;
        }

        return available;
    }

    public static Vector2 FitCells(
        Vector2 budget, int rows, int columns, Vector2 spacing, RectOffset padding)
    {
        rows = Mathf.Max(1, rows);
        columns = Mathf.Max(1, columns);
        float width = Mathf.Max(0f,
            budget.x - padding.horizontal - spacing.x * (columns - 1));
        float height = Mathf.Max(0f,
            budget.y - padding.vertical - spacing.y * (rows - 1));
        float cellWidth = Mathf.Max(0f, Mathf.Min(width / columns, height / rows * 0.75f));
        return new Vector2(cellWidth, cellWidth / 0.75f);
    }

    public static Vector2 GridSize(
        Vector2 cell, int rows, int columns, Vector2 spacing, RectOffset padding)
    {
        return new Vector2(
            cell.x * columns + spacing.x * Mathf.Max(0, columns - 1) + padding.horizontal,
            cell.y * rows + spacing.y * Mathf.Max(0, rows - 1) + padding.vertical);
    }

    public static Rect ScreenToLocalRect(Rect screenRect, RectTransform parent, Camera camera)
    {
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, screenRect.min, camera, out Vector2 min);
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parent, screenRect.max, camera, out Vector2 max);
        return Rect.MinMaxRect(
            Mathf.Min(min.x, max.x), Mathf.Min(min.y, max.y),
            Mathf.Max(min.x, max.x), Mathf.Max(min.y, max.y));
    }

    public static void PlaceInParent(RectTransform rect, Rect localRect)
    {
        var parent = rect.parent as RectTransform;
        rect.anchorMin = rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.sizeDelta = localRect.size;
        rect.anchoredPosition = localRect.center - (parent != null ? parent.rect.center : Vector2.zero);
    }
}
