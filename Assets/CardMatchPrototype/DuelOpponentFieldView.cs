using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public sealed class DuelOpponentFieldView : MonoBehaviour
{
    private const string SpriteResourcePath =
        "AnimalMemory/Duel/memento-duel-opponents-sheet-4k-v1";
    private const string AkiSpriteResourcePath =
        "AnimalMemory/Duel/aki-duel-opponents-sheet-4k-v1";
    private const string NewGuideResourceRoot =
        "AnimalMemory/Duel/NewGuides/";
    private const string UltimateMasterResourceRoot =
        "AnimalMemory/Duel/UltimateMaster/";

    private readonly Dictionary<string, Sprite> sprites =
        new Dictionary<string, Sprite>();
    private readonly List<Object> ownedPortraitAssets =
        new List<Object>();

    private CardMatchUI source;
    private RectTransform gridReference;
    private RectTransform stageRect;
    private RectTransform portraitRect;
    private RectTransform attackTargetRect;
    private Image portraitImage;
    private TextMeshProUGUI damageLabel;
    private Coroutine hitRoutine;
    private int displayedGuide = -1;
    private int displayedPose = -1;
    private bool lastPortrait;
    private Rect lastLocalStageArea;
    private bool layoutInitialized;
    private bool reacting;
    private float neutralTopPadding;

    public RectTransform AttackTarget => attackTargetRect;

    private static Color RestingColor(bool portrait)
    {
        return portrait ? new Color(1f, 1f, 1f, 0.96f) : Color.white;
    }

    public void Bind(CardMatchUI duelSource, RectTransform cardGrid)
    {
        source = duelSource;
        gridReference = cardGrid;
        LoadSprites();
        EnsureHierarchy();
        RefreshNow();
    }

    public void PlayHitReaction()
    {
        if (stageRect == null || !stageRect.gameObject.activeInHierarchy)
            return;

        if (hitRoutine != null)
            StopCoroutine(hitRoutine);
        hitRoutine = StartCoroutine(HitReaction());
    }

    private void LateUpdate()
    {
        RefreshNow();
    }

    private void RefreshNow()
    {
        if (source == null || stageRect == null)
            return;

        bool visible = source.IsDuelConfigured &&
            (source.IsDuelActive || source.DuelOpponentPose >= 3);
        if (stageRect.gameObject.activeSelf != visible)
            stageRect.gameObject.SetActive(visible);
        if (!visible)
            return;

        bool portrait = Screen.height > Screen.width;
        RectTransform parent = stageRect.parent as RectTransform;
        Canvas canvas = gridReference != null
            ? gridReference.GetComponentInParent<Canvas>() : null;
        Camera camera = canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? canvas.worldCamera : null;
        Rect localArea = parent != null
            ? MementoGameplayLayout.ScreenToLocalRect(
                source.GetOpponentPresentationArea(), parent, camera)
            : new Rect();
        if (!layoutInitialized || portrait != lastPortrait || localArea != lastLocalStageArea)
            ApplyLayout(portrait, localArea);

        int guideId = source.DuelOpponentGuideId;
        int pose = source.DuelOpponentPose;
        if (!reacting && (guideId != displayedGuide || pose != displayedPose))
            ApplySprite(guideId, pose);
        if (!reacting && portraitImage.sprite != null)
        {
            // Atlas padding is not part of the character. Align the neutral painted
            // top with the stage top in portrait, keeping every pose on one baseline.
            float aspect = portraitImage.sprite.rect.width / portraitImage.sprite.rect.height;
            float paintedFrameHeight = Mathf.Min(portraitRect.rect.height, portraitRect.rect.width / aspect);
            float offset = portrait ? paintedFrameHeight * neutralTopPadding : 0f;
            portraitRect.anchoredPosition = new Vector2(0f, offset);
        }
    }

    private void EnsureHierarchy()
    {
        if (stageRect != null || gridReference == null)
            return;

        RectTransform parent = gridReference.parent as RectTransform;
        if (parent == null)
            return;

        GameObject stageObject = new GameObject(
            "Duel Opponent Field",
            typeof(RectTransform),
            typeof(CanvasGroup));
        stageObject.layer = parent.gameObject.layer;
        stageRect = stageObject.GetComponent<RectTransform>();
        stageRect.SetParent(parent, false);
        stageRect.SetSiblingIndex(gridReference.GetSiblingIndex());

        CanvasGroup canvasGroup = stageObject.GetComponent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        GameObject portraitObject = new GameObject(
            "Opponent Full Body",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image));
        portraitObject.layer = stageObject.layer;
        portraitRect = portraitObject.GetComponent<RectTransform>();
        portraitRect.SetParent(stageRect, false);
        portraitRect.anchorMin = Vector2.zero;
        portraitRect.anchorMax = Vector2.one;
        portraitRect.offsetMin = Vector2.zero;
        portraitRect.offsetMax = Vector2.zero;

        portraitImage = portraitObject.GetComponent<Image>();
        portraitImage.preserveAspect = true;
        portraitImage.raycastTarget = false;

        GameObject targetObject = new GameObject(
            "Opponent Attack Target",
            typeof(RectTransform));
        targetObject.layer = stageObject.layer;
        attackTargetRect = targetObject.GetComponent<RectTransform>();
        attackTargetRect.SetParent(stageRect, false);
        attackTargetRect.anchorMin = new Vector2(0.7f, 0.55f);
        attackTargetRect.anchorMax = new Vector2(0.7f, 0.55f);
        attackTargetRect.sizeDelta = new Vector2(24f, 24f);
        attackTargetRect.anchoredPosition = Vector2.zero;

        GameObject damageObject = new GameObject(
            "Opponent Damage",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(TextMeshProUGUI));
        damageObject.layer = stageObject.layer;
        RectTransform damageRect = damageObject.GetComponent<RectTransform>();
        damageRect.SetParent(stageRect, false);
        damageRect.anchorMin = new Vector2(0.54f, 0.72f);
        damageRect.anchorMax = new Vector2(0.96f, 0.84f);
        damageRect.offsetMin = Vector2.zero;
        damageRect.offsetMax = Vector2.zero;

        damageLabel = damageObject.GetComponent<TextMeshProUGUI>();
        damageLabel.text = "-1 PAREJA";
        damageLabel.alignment = TextAlignmentOptions.Center;
        damageLabel.fontSize = 27f;
        damageLabel.fontStyle = FontStyles.Bold;
        damageLabel.color = new Color(1f, 0.93f, 0.76f, 0f);
        damageLabel.enableAutoSizing = true;
        damageLabel.fontSizeMin = 14f;
        damageLabel.fontSizeMax = 29f;
        damageLabel.raycastTarget = false;

        stageObject.SetActive(false);
    }

    private void ApplyLayout(bool portrait, Rect localArea)
    {
        lastPortrait = portrait;
        lastLocalStageArea = localArea;
        layoutInitialized = true;
        MementoGameplayLayout.PlaceInParent(stageRect, localArea);
        if (!reacting)
        {
            portraitRect.anchoredPosition = Vector2.zero;
            portraitImage.color = RestingColor(portrait);
        }
        attackTargetRect.anchorMin = portrait
            ? new Vector2(0.62f, 0.66f) : new Vector2(0.67f, 0.58f);
        attackTargetRect.anchorMax = attackTargetRect.anchorMin;
    }

    private void LoadSprites()
    {
        if (sprites.Count > 0)
            return;

        LoadSpriteSheet(SpriteResourcePath);
        LoadSpriteSheet(AkiSpriteResourcePath);
        LoadIndividualGuide("hana", NewGuideResourceRoot);
        LoadIndividualGuide("momo", NewGuideResourceRoot);
        LoadIndividualGuide("rei", UltimateMasterResourceRoot);
    }

    private void LoadSpriteSheet(string resourcePath)
    {
        Sprite[] loaded = Resources.LoadAll<Sprite>(resourcePath);
        for (int i = 0; i < loaded.Length; i++)
        {
            Sprite sourceSprite = loaded[i];
            if (sourceSprite == null)
                continue;

            bool removeOuterPaleArtifacts =
                resourcePath == SpriteResourcePath;
            Sprite displaySprite =
                MementoMatchPortraitMatteCleaner.CreateCleanedSprite(
                    sourceSprite,
                    removeOuterPaleArtifacts);
            sprites[sourceSprite.name] = displaySprite;
            if (displaySprite != sourceSprite)
            {
                ownedPortraitAssets.Add(displaySprite);
                if (displaySprite.texture != null)
                    ownedPortraitAssets.Add(displaySprite.texture);
            }
        }
    }

    private void OnDestroy()
    {
        for (int i = 0; i < ownedPortraitAssets.Count; i++)
        {
            Object owned = ownedPortraitAssets[i];
            if (owned != null)
                Destroy(owned);
        }

        ownedPortraitAssets.Clear();
    }

    private void LoadIndividualGuide(string guide, string resourceRoot)
    {
        string[] states = { "neutral", "ability", "defeated", "victory" };
        for (int i = 0; i < states.Length; i++)
        {
            string key = guide + "_" + states[i];
            Sprite sprite = Resources.Load<Sprite>(resourceRoot + key);
            if (sprite != null)
                sprites[key] = sprite;
        }
    }

    private void ApplySprite(int guideId, int pose)
    {
        if (portraitImage == null)
            return;

        Sprite illustrated = MementoIllustratedCharacters.GetSprite(
            guideId, MementoIllustratedCharacters.StateForPose(pose));
        portraitImage.enabled = true;
        if (illustrated != null)
        {
            if (guideId != displayedGuide)
                neutralTopPadding = GetNeutralTopPadding(guideId);
            portraitImage.sprite = illustrated;
            displayedGuide = guideId;
            displayedPose = pose;
            return;
        }

        if (guideId >= 6)
        {
            portraitImage.sprite = null;
            portraitImage.enabled = false;
            displayedGuide = guideId;
            displayedPose = pose;
            return; // A missing new portrait must not become Aki through the legacy fallback.
        }

        string guide = guideId == 5
            ? "rei"
            : guideId == 4
                ? "momo"
                : guideId == 3
                ? "hana"
                : guideId == 2
                    ? "yoru"
                    : guideId == 1
                        ? "mika"
                        : "aki";
        string state = pose == 1
            ? "thinking"
            : pose == 2
                ? "ability"
                : (pose == 3 || pose == 5)
                    ? "defeated"
                    : pose == 4
                        ? "victory"
                        : "neutral";
        if ((guideId == 3 || guideId == 4 || guideId == 5) && state == "thinking")
            state = "neutral";
        if (sprites.TryGetValue(guide + "_" + state, out Sprite sprite))
        {
            portraitImage.sprite = sprite;
            displayedGuide = guideId;
            displayedPose = pose;
        }
    }

    private static float GetNeutralTopPadding(int guideId)
    {
        Sprite neutral = MementoIllustratedCharacters.GetSprite(guideId, "neutral");
        if (neutral == null || !neutral.texture.isReadable) return 0f;
        Color32[] pixels = neutral.texture.GetPixels32();
        int width = neutral.texture.width, height = neutral.texture.height;
        for (int y = height - 1; y >= 0; y--)
            for (int x = 0; x < width; x++)
                if (pixels[y * width + x].a > 128)
                    return Mathf.Clamp01((height - 1 - y) / (float)height);
        return 0f;
    }

    private IEnumerator HitReaction()
    {
        reacting = true;
        if (damageLabel != null && source != null)
            damageLabel.text = $"-{Mathf.Max(1, source.LastPlayerDuelDamage)} DAÑO";
        int guideId = source != null ? source.DuelOpponentGuideId : displayedGuide;
        ApplySprite(guideId, 5);

        Vector2 origin = portraitRect.anchoredPosition;
        Color baseColor = RestingColor(lastPortrait);
        float elapsed = 0f;
        const float duration = 0.46f;

        while (elapsed < duration && portraitRect != null)
        {
            elapsed += Time.unscaledDeltaTime;
            float normalized = Mathf.Clamp01(elapsed / duration);
            float strength = (1f - normalized) * 22f;
            float wave = normalized * Mathf.PI * 10f;
            portraitRect.anchoredPosition = origin + new Vector2(
                Mathf.Sin(wave) * strength,
                Mathf.Cos(wave * 0.73f) * strength * 0.35f);
            portraitImage.color = Color.Lerp(
                new Color(1f, 0.34f, 0.3f, baseColor.a),
                baseColor,
                normalized);

            Color damageColor = damageLabel.color;
            damageColor.a = normalized < 0.58f
                ? 1f
                : 1f - Mathf.InverseLerp(0.58f, 1f, normalized);
            damageLabel.color = damageColor;
            yield return null;
        }

        if (portraitRect != null)
            portraitRect.anchoredPosition = origin;
        if (portraitImage != null)
            portraitImage.color = RestingColor(lastPortrait);
        if (damageLabel != null)
        {
            Color damageColor = damageLabel.color;
            damageColor.a = 0f;
            damageLabel.color = damageColor;
        }

        reacting = false;
        hitRoutine = null;
        if (source != null && source.IsDuelConfigured)
            ApplySprite(source.DuelOpponentGuideId, source.DuelOpponentPose);
    }
}
