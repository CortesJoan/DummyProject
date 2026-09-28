using UnityEngine;
using UnityEngine.UIElements;

/// <summary>Borrowed character pose with an entrance, impact burst and clean exit. No gameplay state.</summary>
public sealed class MementoSkillCinematic : VisualElement
{
    private readonly VisualElement actor;
    private readonly VisualElement echo;
    private readonly IVisualElementScheduledItem animation;
    private float began;
    private float duration;
    private Color accent;
    public MementoSkillCinematic()
    {
        name = "skill-character-cinematic";
        pickingMode = PickingMode.Ignore;
        style.position = Position.Absolute;
        style.left = style.right = style.top = style.bottom = 0;
        echo = new VisualElement { name = "skill-character-echo", pickingMode = PickingMode.Ignore };
        actor = new VisualElement { name = "skill-character-pose", pickingMode = PickingMode.Ignore };
        echo.AddToClassList("skill-cinematic-actor");
        actor.AddToClassList("skill-cinematic-actor");
        Add(echo);
        Add(actor);
        generateVisualContent += DrawImpact;
        animation = schedule.Execute(Tick).Every(33);
        animation.Pause();
        RegisterCallback<DetachFromPanelEvent>(_ => animation.Pause());
    }

    public void Play(Sprite pose, int ownerGuideId, float seconds)
    {
        began = Time.realtimeSinceStartup;
        duration = Mathf.Max(1f, seconds);
        actor.style.backgroundImage = pose != null ? new StyleBackground(pose) : StyleKeyword.None;
        echo.style.backgroundImage = actor.style.backgroundImage;
        Color[] colors = { new Color(.45f,.95f,.65f), new Color(1f,.68f,.35f),
            new Color(.68f,.57f,1f), new Color(.96f,.61f,.79f),
            new Color(1f,.50f,.65f), new Color(.55f,.85f,1f) };
        accent = colors[Mathf.Clamp(ownerGuideId, 0, colors.Length - 1)];
        echo.style.unityBackgroundImageTintColor = accent;
        animation.Resume();
        Tick();
    }

    public void Stop()
    {
        animation.Pause();
        actor.style.opacity = echo.style.opacity = 0;
    }

    private void Tick()
    {
        float elapsed = Time.realtimeSinceStartup - began;
        float entrance = Entrance(elapsed);
        float exit = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(duration - .25f, duration, elapsed));
        bool portrait = contentRect.height > contentRect.width;
        float slide = (1f - entrance) * (portrait ? -90f : -260f) + exit * 100f;
        actor.style.translate = new Translate(slide, 0);
        actor.style.scale = new Scale(Vector3.one * (1f + (1f - entrance) * .13f));
        actor.style.opacity = entrance * (1f - exit);
        echo.style.translate = new Translate(slide - 35f * (1f - entrance), 0);
        echo.style.scale = new Scale(Vector3.one * 1.055f);
        echo.style.opacity = Mathf.Max(0, .35f - elapsed * .4f) * (1f - exit);
        MarkDirtyRepaint();
        if (elapsed >= duration) animation.Pause();
    }

    /// <summary>Fast launch, eased landing; exposed for deterministic presentation tests.</summary>
    public static float Entrance(float elapsed)
    {
        float t = Mathf.Clamp01(elapsed / .48f);
        return 1f - Mathf.Pow(1f - t, 3f);
    }

    private void DrawImpact(MeshGenerationContext context)
    {
        float w = contentRect.width, h = contentRect.height;
        if (w <= 0 || h <= 0) return;
        float time = Time.realtimeSinceStartup - began;
        if (time >= duration) return;
        bool portrait = h > w;
        Vector2 c = new Vector2(w * (portrait ? .50f : .28f), h * (portrait ? .38f : .46f));
        float radius = Mathf.Min(w, h) * .47f;
        var p = context.painter2D;
        // A fan of broad light shards converges on the actual character, not an unrelated icon.
        float impact = Mathf.Clamp01(1f - Mathf.Abs(time - .42f) / .52f);
        for (int i = 0; i < 12; i++)
        {
            float angle = i * Mathf.PI * 2f / 12f + .14f;
            float outer = radius * (1f + impact * .45f);
            Vector2 a = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle));
            Vector2 b = new Vector2(Mathf.Cos(angle + .085f), Mathf.Sin(angle + .085f));
            p.fillColor = new Color(accent.r, accent.g, accent.b, .035f + impact * .17f);
            p.BeginPath(); p.MoveTo(c + a * radius * .25f);
            p.LineTo(c + a * outer); p.LineTo(c + b * outer * .90f); p.ClosePath(); p.Fill();
        }
        // Short-lived impact diamonds then slow drifting motes, always behind the pose/text.
        for (int i = 0; i < 18; i++)
        {
            float angle = i * 2.39996f;
            float distance = radius * (.38f + (i % 5) * .11f);
            Vector2 v = c + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
            v.y -= time * (5f + i % 4);
            float r = 2f + i % 3 + impact * 2f;
            p.fillColor = new Color(accent.r, accent.g, accent.b, .28f + impact * .5f);
            p.BeginPath(); p.MoveTo(v + Vector2.up * r * 2f);
            p.LineTo(v + Vector2.right * r); p.LineTo(v + Vector2.down * r * 2f);
            p.LineTo(v + Vector2.left * r); p.ClosePath(); p.Fill();
        }
    }
}
