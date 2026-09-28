using Action = System.Action;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[DefaultExecutionOrder(-900)]
public sealed class AnimalMemoryJuice : MonoBehaviour
{
    private const float ReferenceWidth = 1920f;
    private const float ReferenceHeight = 1080f;
    private const int TextureSize = 64;

    private static AnimalMemoryJuice instance;
    private static bool isQuitting;

    private readonly Dictionary<AnimalMemoryParticleShape, Sprite> shapeSprites =
        new Dictionary<AnimalMemoryParticleShape, Sprite>();
    private readonly Dictionary<RectTransform, Coroutine> activePunches =
        new Dictionary<RectTransform, Coroutine>();
    private readonly Dictionary<RectTransform, Vector3> punchBaseScales =
        new Dictionary<RectTransform, Vector3>();

    private RectTransform overlayRect;
    private int themeIndex;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        isQuitting = false;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    private static void Bootstrap()
    {
        EnsureInstance();
    }

    public static void SetThemeGlobal(int selectedTheme)
    {
        AnimalMemoryJuice effect = EnsureInstance();
        if (effect != null)
        {
            effect.themeIndex = AnimalMemoryJuiceTheme.ClampTheme(selectedTheme);
        }
    }

    public static void PlayFlip(RectTransform card)
    {
        AnimalMemoryJuice effect = EnsureInstance();
        if (effect == null || card == null)
        {
            return;
        }

        effect.StartPunch(card, 1.09f, 0.18f);
        effect.SpawnBurst(effect.ToOverlayPosition(card), 3, 55f, 105f, 0.38f, false);
    }

    public static void PlayMatch(RectTransform firstCard, RectTransform secondCard)
    {
        AnimalMemoryJuice effect = EnsureInstance();
        if (effect == null)
        {
            return;
        }

        Vector2 first = effect.ToOverlayPosition(firstCard);
        Vector2 second = effect.ToOverlayPosition(secondCard);
        Vector2 center = (first + second) * 0.5f;

        effect.SpawnBurst(first, 5, 90f, 180f, 0.62f, false);
        effect.SpawnBurst(second, 5, 90f, 180f, 0.62f, false);
        effect.SpawnBurst(center, 8, 120f, 250f, 0.78f, false);

        if (firstCard != null)
        {
            effect.StartPunch(firstCard, 1.18f, 0.27f);
        }

        if (secondCard != null)
        {
            effect.StartPunch(secondCard, 1.18f, 0.27f);
        }
    }

    public static void PlayDuelAttack(
        RectTransform firstCard,
        RectTransform secondCard,
        Vector2 rivalScreenPoint,
        bool hostile,
        Action onImpact)
    {
        AnimalMemoryJuice effect = EnsureInstance();
        if (effect == null || firstCard == null || secondCard == null)
        {
            return;
        }

        effect.StartCoroutine(
            effect.DuelAttack(firstCard, secondCard, rivalScreenPoint, hostile, onImpact));
    }

    public static void PlayFail(RectTransform firstCard, RectTransform secondCard)
    {
        AnimalMemoryJuice effect = EnsureInstance();
        if (effect == null)
        {
            return;
        }

        if (firstCard != null)
        {
            effect.StartCoroutine(effect.Shake(firstCard, 0.24f, 11f));
        }

        if (secondCard != null)
        {
            effect.StartCoroutine(effect.Shake(secondCard, 0.24f, 11f));
        }

        Vector2 center = (effect.ToOverlayPosition(firstCard) + effect.ToOverlayPosition(secondCard)) * 0.5f;
        effect.SpawnBurst(center, 7, 55f, 130f, 0.48f, true);
    }

    public static void PlayVictory()
    {
        AnimalMemoryJuice effect = EnsureInstance();
        if (effect != null)
        {
            effect.StartCoroutine(effect.VictoryRain());
        }
    }

    private IEnumerator DuelAttack(
        RectTransform firstCard,
        RectTransform secondCard,
        Vector2 rivalScreenPoint,
        bool hostile,
        Action onImpact)
    {
        if (overlayRect == null)
        {
            onImpact?.Invoke();
            yield break;
        }

        Vector2 first = ToOverlayPosition(firstCard);
        Vector2 second = ToOverlayPosition(secondCard);
        Vector2 target = ScreenToOverlayPosition(rivalScreenPoint);
        AnimalMemoryJuiceStyle style = AnimalMemoryJuiceTheme.GetStyle(themeIndex);
        MementoMatchSfx.PlayDuelLaunch(hostile);

        SpawnBurst(first, 6, 75f, 145f, 0.5f, false);
        SpawnBurst(second, 6, 75f, 145f, 0.5f, false);

        const int symbolsPerCard = 7;
        const float baseDuration = 0.56f;
        const float stagger = 0.035f;
        for (int i = 0; i < symbolsPerCard; i++)
        {
            float firstOffset = Mathf.Lerp(-92f, 82f, i / (symbolsPerCard - 1f));
            float secondOffset = Mathf.Lerp(88f, -76f, i / (symbolsPerCard - 1f));
            Color firstColor = i % 2 == 0 ? style.Highlight : style.Primary;
            Color secondColor = i % 2 == 0 ? style.Secondary : style.Highlight;
            AnimalMemoryParticleShape firstShape =
                AnimalMemoryJuiceTheme.GetShapeForSequence(themeIndex, i);
            AnimalMemoryParticleShape secondShape =
                AnimalMemoryJuiceTheme.GetShapeForSequence(themeIndex, i + 1);

            SpawnAttackSymbol(
                first,
                target,
                firstOffset,
                72f + (i % 3) * 14f,
                i * stagger,
                baseDuration + (i % 2) * 0.06f,
                31f + (i % 3) * 6f,
                firstColor,
                firstShape);
            SpawnAttackSymbol(
                second,
                target,
                secondOffset,
                82f + ((i + 1) % 3) * 13f,
                0.045f + i * stagger,
                baseDuration + ((i + 1) % 2) * 0.06f,
                29f + ((i + 1) % 3) * 7f,
                secondColor,
                secondShape);
        }

        yield return new WaitForSecondsRealtime(0.83f);
        SpawnBurst(target, hostile ? 24 : 18, 125f, hostile ? 330f : 285f, 0.78f, hostile);
        MementoMatchSfx.PlayDuelImpact(hostile);
        onImpact?.Invoke();
    }

    private void SpawnAttackSymbol(
        Vector2 start,
        Vector2 target,
        float lateralOffset,
        float lift,
        float delay,
        float duration,
        float size,
        Color color,
        AnimalMemoryParticleShape shape)
    {
        if (overlayRect == null)
        {
            return;
        }

        GameObject symbolObject = new GameObject(
            "Duel Attack " + shape,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        symbolObject.transform.SetParent(overlayRect, false);

        RectTransform rect = symbolObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = start;
        rect.sizeDelta = Vector2.one * size;
        rect.localScale = Vector3.one * 0.45f;

        Image image = symbolObject.GetComponent<Image>();
        image.sprite = GetOrCreateShapeSprite(shape);
        image.color = color;
        image.raycastTarget = false;
        image.preserveAspect = true;

        Vector2 control = DuelAttackPath.BuildControlPoint(
            start,
            target,
            lateralOffset,
            lift);
        StartCoroutine(AnimateAttackSymbol(
            rect,
            image,
            start,
            control,
            target,
            delay,
            duration));
    }

    private static IEnumerator AnimateAttackSymbol(
        RectTransform rect,
        Image image,
        Vector2 start,
        Vector2 control,
        Vector2 target,
        float delay,
        float duration)
    {
        if (delay > 0f)
        {
            yield return new WaitForSecondsRealtime(delay);
        }

        float elapsed = 0f;
        while (rect != null && elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(elapsed / duration);
            float eased = normalized * normalized * (3f - 2f * normalized);
            rect.anchoredPosition = DuelAttackPath.EvaluateQuadratic(
                start,
                control,
                target,
                eased);
            rect.localRotation = Quaternion.Euler(
                0f,
                0f,
                Mathf.Lerp(-28f, 250f, normalized));
            float pulse = Mathf.Sin(normalized * Mathf.PI);
            rect.localScale = Vector3.one * Mathf.Lerp(0.45f, 1.2f, pulse);

            Color color = image.color;
            color.a = normalized < 0.82f
                ? 1f
                : 1f - Mathf.InverseLerp(0.82f, 1f, normalized);
            image.color = color;
            yield return null;
        }

        if (rect != null)
        {
            Destroy(rect.gameObject);
        }
    }

    private static AnimalMemoryJuice EnsureInstance()
    {
        if (isQuitting || !Application.isPlaying)
        {
            return null;
        }

        if (instance != null)
        {
            return instance;
        }

        GameObject root = new GameObject("[AnimalMemory Juice]");
        DontDestroyOnLoad(root);
        instance = root.AddComponent<AnimalMemoryJuice>();
        instance.CreateOverlay();
        return instance;
    }

    private void CreateOverlay()
    {
        GameObject canvasObject = new GameObject(
            "AnimalMemory Juice Overlay",
            typeof(RectTransform),
            typeof(Canvas),
            typeof(CanvasScaler));
        canvasObject.transform.SetParent(transform, false);

        Canvas canvas = canvasObject.GetComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 1000;
        canvas.overrideSorting = true;

        CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(ReferenceWidth, ReferenceHeight);
        scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        scaler.matchWidthOrHeight = 0.5f;

        overlayRect = canvasObject.GetComponent<RectTransform>();
    }

    private Vector2 ToOverlayPosition(RectTransform target)
    {
        if (target == null || overlayRect == null)
        {
            return Vector2.zero;
        }

        Canvas sourceCanvas = target.GetComponentInParent<Canvas>();
        Camera sourceCamera = sourceCanvas != null && sourceCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? sourceCanvas.worldCamera
            : null;
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(sourceCamera, target.position);
        return ScreenToOverlayPosition(screenPoint);
    }

    private Vector2 ScreenToOverlayPosition(Vector2 screenPoint)
    {
        if (overlayRect == null)
        {
            return Vector2.zero;
        }

        return RectTransformUtility.ScreenPointToLocalPointInRectangle(
            overlayRect,
            screenPoint,
            null,
            out Vector2 localPoint)
            ? localPoint
            : Vector2.zero;
    }

    private void SpawnBurst(Vector2 position, int count, float minimumSpeed, float maximumSpeed, float duration, bool useFailColor)
    {
        AnimalMemoryJuiceStyle style = AnimalMemoryJuiceTheme.GetStyle(themeIndex);
        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float speed = Random.Range(minimumSpeed, maximumSpeed);
            Vector2 velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * speed;
            Color color = useFailColor
                ? style.Fail
                : i % 3 == 0 ? style.Highlight : i % 2 == 0 ? style.Secondary : style.Primary;
            AnimalMemoryParticleShape shape = AnimalMemoryJuiceTheme.GetShapeForSequence(themeIndex, i);
            SpawnSymbol(
                position,
                velocity,
                duration + Random.Range(-0.08f, 0.12f),
                Random.Range(26f, 52f),
                color,
                shape,
                90f);
        }
    }

    private void SpawnSymbol(
        Vector2 position,
        Vector2 velocity,
        float duration,
        float size,
        Color color,
        AnimalMemoryParticleShape shape,
        float gravity)
    {
        if (overlayRect == null)
        {
            return;
        }

        GameObject symbolObject = new GameObject(
            "Juice " + shape,
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        symbolObject.transform.SetParent(overlayRect, false);

        RectTransform rect = symbolObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0.5f, 0.5f);
        rect.anchorMax = new Vector2(0.5f, 0.5f);
        rect.pivot = new Vector2(0.5f, 0.5f);
        rect.anchoredPosition = position;
        rect.sizeDelta = Vector2.one * size;
        rect.localScale = Vector3.one * 0.35f;
        rect.localRotation = Quaternion.Euler(0f, 0f, Random.Range(-35f, 35f));

        Image image = symbolObject.GetComponent<Image>();
        image.sprite = GetOrCreateShapeSprite(shape);
        image.color = color;
        image.raycastTarget = false;
        image.preserveAspect = true;

        StartCoroutine(AnimateSymbol(rect, image, velocity, Mathf.Max(0.18f, duration), gravity));
    }

    private IEnumerator AnimateSymbol(RectTransform rect, Image image, Vector2 velocity, float duration, float gravity)
    {
        Vector2 start = rect.anchoredPosition;
        Quaternion initialRotation = rect.localRotation;
        float elapsed = 0f;

        while (rect != null && elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(elapsed / duration);
            float easedScale = Mathf.Sin(normalized * Mathf.PI);
            rect.anchoredPosition = start + velocity * elapsed + Vector2.down * (gravity * elapsed * elapsed);
            rect.localScale = Vector3.one * Mathf.Lerp(0.35f, 1.15f, easedScale);
            rect.localRotation = initialRotation * Quaternion.Euler(0f, 0f, normalized * 110f);

            Color color = image.color;
            color.a = 1f - Mathf.SmoothStep(0f, 1f, normalized);
            image.color = color;
            yield return null;
        }

        if (rect != null)
        {
            Destroy(rect.gameObject);
        }
    }

    private void StartPunch(
        RectTransform target,
        float peakScale,
        float duration)
    {
        if (target == null)
            return;

        if (!punchBaseScales.TryGetValue(target, out Vector3 baseScale))
        {
            baseScale = target.localScale;
            punchBaseScales[target] = baseScale;
        }

        if (activePunches.TryGetValue(target, out Coroutine running) &&
            running != null)
        {
            StopCoroutine(running);
            target.localScale = baseScale;
        }

        activePunches[target] = StartCoroutine(
            Punch(target, baseScale, peakScale, duration));
    }

    private IEnumerator Punch(
        RectTransform target,
        Vector3 baseScale,
        float peakScale,
        float duration)
    {
        float elapsed = 0f;
        while (target != null && elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(elapsed / duration);
            target.localScale = baseScale * Mathf.Lerp(
                1f,
                peakScale,
                Mathf.Sin(normalized * Mathf.PI));
            yield return null;
        }

        if (target != null)
        {
            target.localScale = baseScale;
            activePunches.Remove(target);
            punchBaseScales.Remove(target);
        }
    }

    private IEnumerator Shake(RectTransform target, float duration, float magnitude)
    {
        if (target == null)
        {
            yield break;
        }

        Vector2 originalPosition = target.anchoredPosition;
        float elapsed = 0f;
        while (target != null && elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(elapsed / duration);
            float wave = Mathf.Sin(normalized * Mathf.PI * 8f);
            target.anchoredPosition = originalPosition + Vector2.right * wave * magnitude * (1f - normalized);
            yield return null;
        }

        if (target != null)
        {
            target.anchoredPosition = originalPosition;
        }
    }

    private IEnumerator VictoryRain()
    {
        AnimalMemoryJuiceStyle style = AnimalMemoryJuiceTheme.GetStyle(themeIndex);
        Vector2 halfSize = overlayRect != null ? overlayRect.rect.size * 0.5f : new Vector2(960f, 540f);
        SpawnBurst(Vector2.zero, 18, 180f, 380f, 1.05f, false);

        for (int i = 0; i < 34; i++)
        {
            float x = Random.Range(-halfSize.x, halfSize.x);
            float y = halfSize.y + Random.Range(20f, 160f);
            Vector2 velocity = new Vector2(Random.Range(-65f, 65f), Random.Range(-80f, -25f));
            Color color = i % 3 == 0 ? style.Highlight : i % 2 == 0 ? style.Secondary : style.Primary;
            AnimalMemoryParticleShape shape = AnimalMemoryJuiceTheme.GetShapeForSequence(themeIndex, i);
            SpawnSymbol(
                new Vector2(x, y),
                velocity,
                Random.Range(1.25f, 2.1f),
                Random.Range(30f, 60f),
                color,
                shape,
                115f);

            if (i % 5 == 0)
            {
                yield return new WaitForSecondsRealtime(0.04f);
            }
        }
    }

    private Sprite GetOrCreateShapeSprite(AnimalMemoryParticleShape shape)
    {
        if (shapeSprites.TryGetValue(shape, out Sprite existing) && existing != null)
        {
            return existing;
        }

        Texture2D texture = new Texture2D(TextureSize, TextureSize, TextureFormat.RGBA32, false)
        {
            name = "AnimalMemory_" + shape,
            filterMode = FilterMode.Bilinear,
            wrapMode = TextureWrapMode.Clamp,
            hideFlags = HideFlags.DontSave
        };

        Color[] pixels = new Color[TextureSize * TextureSize];
        for (int y = 0; y < TextureSize; y++)
        {
            for (int x = 0; x < TextureSize; x++)
            {
                float normalizedX = ((x + 0.5f) / TextureSize) * 2f - 1f;
                float normalizedY = ((y + 0.5f) / TextureSize) * 2f - 1f;
                float alpha = IsInsideShape(shape, normalizedX, normalizedY) ? 1f : 0f;
                pixels[(y * TextureSize) + x] = new Color(1f, 1f, 1f, alpha);
            }
        }

        texture.SetPixels(pixels);
        texture.Apply(false, false);
        Sprite sprite = Sprite.Create(texture, new Rect(0f, 0f, TextureSize, TextureSize), new Vector2(0.5f, 0.5f), 100f);
        sprite.name = texture.name;
        sprite.hideFlags = HideFlags.DontSave;
        shapeSprites[shape] = sprite;
        return sprite;
    }

    private static bool IsInsideShape(AnimalMemoryParticleShape shape, float x, float y)
    {
        switch (shape)
        {
            case AnimalMemoryParticleShape.Paw:
                return InEllipse(x, y + 0.20f, 0.46f, 0.38f) ||
                       InEllipse(x + 0.48f, y - 0.30f, 0.18f, 0.25f) ||
                       InEllipse(x + 0.17f, y - 0.48f, 0.17f, 0.24f) ||
                       InEllipse(x - 0.17f, y - 0.48f, 0.17f, 0.24f) ||
                       InEllipse(x - 0.48f, y - 0.30f, 0.18f, 0.25f);
            case AnimalMemoryParticleShape.Leaf:
                float rotatedX = (x + y) * 0.70710678f;
                float rotatedY = (y - x) * 0.70710678f;
                float width = Mathf.Max(0f, 1f - Mathf.Abs(rotatedX));
                return Mathf.Abs(rotatedY) < width * 0.48f ||
                       (Mathf.Abs(rotatedY) < 0.055f && Mathf.Abs(rotatedX) < 0.95f);
            case AnimalMemoryParticleShape.Bubble:
                float bubbleRadius = Mathf.Sqrt((x * x) + (y * y));
                return (bubbleRadius > 0.49f && bubbleRadius < 0.78f) ||
                       InEllipse(x + 0.34f, y - 0.34f, 0.10f, 0.15f);
            case AnimalMemoryParticleShape.Moon:
                bool outer = InEllipse(x + 0.08f, y, 0.73f, 0.78f);
                bool cutout = InEllipse(x - 0.23f, y + 0.06f, 0.63f, 0.69f);
                return outer && !cutout;
            case AnimalMemoryParticleShape.Star:
                float starAngle = Mathf.Atan2(y, x);
                float starRadius = Mathf.Sqrt((x * x) + (y * y));
                float starEdge = 0.54f + (0.22f * Mathf.Cos(starAngle * 5f));
                return starRadius < starEdge;
            case AnimalMemoryParticleShape.Flower:
                if (InEllipse(x, y, 0.22f, 0.22f))
                    return true;
                for (int petal = 0; petal < 5; petal++)
                {
                    float petalAngle = petal * Mathf.PI * 2f / 5f;
                    float petalX = x - Mathf.Cos(petalAngle) * 0.42f;
                    float petalY = y - Mathf.Sin(petalAngle) * 0.42f;
                    if (InEllipse(petalX, petalY, 0.30f, 0.22f))
                        return true;
                }
                return false;
            case AnimalMemoryParticleShape.Candy:
                return InEllipse(x, y, 0.48f, 0.48f) ||
                       (x < -0.40f && Mathf.Abs(y) < 0.56f + (x * 0.35f)) ||
                       (x > 0.40f && Mathf.Abs(y) < 0.56f - (x * 0.35f));
            case AnimalMemoryParticleShape.Gear:
                float gearRadius = Mathf.Sqrt((x * x) + (y * y));
                float gearAngle = Mathf.Atan2(y, x);
                float gearEdge = 0.68f + 0.12f * Mathf.Sign(Mathf.Cos(gearAngle * 8f));
                return gearRadius < gearEdge && gearRadius > 0.28f;
            default:
                return false;
        }
    }

    private static bool InEllipse(float x, float y, float radiusX, float radiusY)
    {
        float normalizedX = x / radiusX;
        float normalizedY = y / radiusY;
        return (normalizedX * normalizedX) + (normalizedY * normalizedY) <= 1f;
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    private void OnDestroy()
    {
        foreach (Sprite sprite in shapeSprites.Values)
        {
            if (sprite == null)
            {
                continue;
            }

            Texture2D texture = sprite.texture;
            Destroy(sprite);
            if (texture != null)
            {
                Destroy(texture);
            }
        }

        shapeSprites.Clear();
        activePunches.Clear();
        punchBaseScales.Clear();
        if (instance == this)
        {
            instance = null;
        }
    }
}
