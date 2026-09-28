using System;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>Skippable, responsive GOD ascension preview. Owns presentation only, never duel state.</summary>
public sealed class MementoGodAscension : VisualElement
{
    public const float Duration = 6.2f;
    public const float OpeningTime = 3.45f;
    private readonly ClosedWings cocoon;
    private readonly Image before;
    private readonly WingPortrait after;
    private readonly Label title;
    private readonly Label hint;
    private readonly IVisualElementScheduledItem clock;
    private float started;
    private float elapsed;
    private bool completed;
    private bool impacted;
    private Action onDone;
    private Action onImpact;

    public MementoGodAscension()
    {
        name = "god-ascension";
        style.position = Position.Absolute;
        style.left = style.right = style.top = style.bottom = 0;
        style.backgroundColor = new Color(.018f,.03f,.07f,.98f);
        pickingMode = PickingMode.Position;
        before = new Image { scaleMode = ScaleMode.ScaleToFit, pickingMode = PickingMode.Ignore };
        before.style.position = Position.Absolute;
        before.style.left = Length.Percent(15); before.style.right = Length.Percent(15);
        before.style.top = Length.Percent(6); before.style.bottom = Length.Percent(14);
        before.sprite = MementoIllustratedCharacters.GetSprite(10, "neutral");
        Add(before);
        after = new WingPortrait(Resources.Load<Texture2D>("AnimalMemory/ArtV2/god-phase2-wide-preview-v1"));
        after.style.position = Position.Absolute;
        after.style.left = Length.Percent(2); after.style.right = Length.Percent(2);
        after.style.top = Length.Percent(3); after.style.bottom = Length.Percent(15);
        Add(after);
        cocoon = new ClosedWings();
        cocoon.style.position = Position.Absolute;
        cocoon.style.left = cocoon.style.right = 0;
        cocoon.style.top = Length.Percent(3); cocoon.style.bottom = Length.Percent(15);
        Add(cocoon);
        title = new Label("LOS AROS REGRESAN A SU DUEÑA");
        title.style.position = Position.Absolute;
        title.style.bottom = Length.Percent(8); title.style.left = title.style.right = 12;
        title.style.color = new Color(1f,.87f,.55f);
        title.style.fontSize = 22; title.style.unityTextAlign = TextAnchor.MiddleCenter;
        title.style.whiteSpace = WhiteSpace.Normal;
        title.pickingMode = PickingMode.Ignore;
        Add(title);
        hint = new Label("Toca para saltar");
        hint.style.position = Position.Absolute;
        hint.style.bottom = Length.Percent(3); hint.style.left = hint.style.right = 0;
        hint.style.unityTextAlign = TextAnchor.MiddleCenter;
        hint.style.color = new Color(.75f,.83f,.92f);
        hint.style.fontSize = 15; hint.pickingMode = PickingMode.Ignore;
        Add(hint);
        generateVisualContent += DrawRings;
        RegisterCallback<PointerDownEvent>(evt => { evt.StopPropagation(); Finish(); });
        clock = schedule.Execute(Tick).Every(16);
        clock.Pause();
        RegisterCallback<DetachFromPanelEvent>(_ => { clock.Pause(); completed = true; });
    }

    public void Play(Action finished = null, Action impact = null)
    {
        onDone = finished; onImpact = impact;
        completed = impacted = false;
        started = Time.realtimeSinceStartup;
        clock.Resume(); Tick();
    }

    public void Finish()
    {
        if (completed) return;
        completed = true; clock.Pause();
        // Even skipping the visual must commit the presentation's impact exactly once.
        if (!impacted) { impacted = true; onImpact?.Invoke(); }
        Action callback = onDone;
        onDone = null; onImpact = null;
        RemoveFromHierarchy();
        callback?.Invoke();
    }

    private void Tick()
    {
        elapsed = Time.realtimeSinceStartup - started;
        if (!impacted && elapsed >= OpeningTime) { impacted = true; onImpact?.Invoke(); }
        float close = Mathf.SmoothStep(0,1,Mathf.InverseLerp(.6f,1.65f,elapsed));
        float growth = Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.8f,3.2f,elapsed));
        float opening = Mathf.SmoothStep(0,1,Mathf.InverseLerp(OpeningTime,4.6f,elapsed));
        float reveal = Mathf.SmoothStep(0,1,Mathf.InverseLerp(3.25f,3.55f,elapsed));
        float exit = 1f - Mathf.SmoothStep(0,1,Mathf.InverseLerp(Duration-.35f,Duration,elapsed));
        before.style.opacity = (1-Mathf.SmoothStep(.65f,1,close)) * exit;
        before.style.scale = new Scale(Vector3.one * (1f-.07f*Mathf.Clamp01(elapsed/2f)));
        after.style.opacity = reveal * exit;
        after.Openness = opening;
        cocoon.Closed = close;
        cocoon.Growth = growth;
        cocoon.Opened = opening;
        cocoon.MarkDirtyRepaint();
        after.MarkDirtyRepaint();
        title.text = elapsed < OpeningTime ? "LOS AROS REGRESAN A SU DUEÑA" : "GOD · SEGUNDA FASE";
        style.opacity = exit;
        MarkDirtyRepaint();
        if (elapsed >= Duration) Finish();
    }

    private void DrawRings(MeshGenerationContext context)
    {
        float w=contentRect.width,h=contentRect.height;
        if(w<=0 || h<=0) return;
        var p=context.painter2D;
        Vector2 center=new Vector2(w*.5f,h*.18f);
        float gather=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.25f,1.75f,elapsed));
        float radius=Mathf.Min(w,h)*.36f;
        float visibility=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(1.75f,2f,elapsed));
        for(int i=0;i<4;i++)
        {
            float angle=i*Mathf.PI*.5f+elapsed*.65f;
            Vector2 at=center+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle)*.35f)*radius*(1-gather);
            p.strokeColor=new Color(1f,.78f,.3f,visibility);
            p.lineWidth=3+gather*2;
            p.BeginPath();
            for(int j=0;j<=48;j++)
            {
                float a=j*Mathf.PI*2/48;
                Vector2 v=at+new Vector2(Mathf.Cos(a)*34,Mathf.Sin(a)*13);
                if(j==0)p.MoveTo(v);else p.LineTo(v);
            }
            p.Stroke();
        }
        float shock=Mathf.Clamp01((elapsed-OpeningTime)/.7f);
        if(elapsed>=OpeningTime && shock<1)
        {
            p.strokeColor=new Color(.65f,.94f,1f,(1-shock)*.8f);
            p.lineWidth=5*(1-shock)+1;
            p.BeginPath();
            for(int j=0;j<=64;j++)
            {
                float a=j*Mathf.PI*2/64;
                Vector2 v=center+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*radius*(.2f+shock*2.4f);
                if(j==0)p.MoveTo(v);else p.LineTo(v);
            }
            p.Stroke();
        }
    }


    /// <summary>Two independently moving foreground wings. The body is never stretched.</summary>
    private sealed class ClosedWings : VisualElement
    {
        private Texture2D matte;
        public float Closed;
        public float Growth;
        public float Opened;

        public ClosedWings()
        {
            pickingMode = PickingMode.Ignore;
            matte = MementoChromaMatte.CreateTexture(
                Resources.Load<Texture2D>("AnimalMemory/ArtV2/god-closed-wings-chroma-v1"),
                MementoChromaMatte.Key.Green, true);
            generateVisualContent += Draw;
            RegisterCallback<DetachFromPanelEvent>(_ => {
                if (matte == null) return;
                if (Application.isPlaying) UnityEngine.Object.Destroy(matte);
                else UnityEngine.Object.DestroyImmediate(matte);
                matte = null;
            });
        }

        private void Draw(MeshGenerationContext context)
        {
            if (matte == null || contentRect.width <= 0 || contentRect.height <= 0) return;
            float scale = Mathf.Min(contentRect.width / matte.width, contentRect.height / matte.height);
            float growth = Mathf.Lerp(.70f, .96f, Growth);
            float w = matte.width * scale * growth, h = matte.height * scale * growth;
            Vector2 center = new Vector2(contentRect.width * .5f, contentRect.height * .5f);
            float spread = (1-Closed) * .8f + Opened * .95f;
            float alpha = Mathf.SmoothStep(0,1,Closed * 3) * (1-Mathf.SmoothStep(.35f,1,Opened));
            var mesh = context.Allocate(8, 12, matte);
            Rect uv = mesh.uvRegion;
            for (int side=0; side<2; side++)
            {
                float sign = side == 0 ? -1 : 1;
                float u0 = side * .5f, u1 = u0 + .5f;
                for (int corner=0; corner<4; corner++)
                {
                    float u = (corner == 0 || corner == 3) ? u0 : u1;
                    float v = corner < 2 ? 0 : 1;
                    Vector2 local = new Vector2((u-.5f)*w, (v-.5f)*h);
                    float angle = sign * spread * .35f;
                    float x = local.x * Mathf.Cos(angle) - local.y * Mathf.Sin(angle);
                    float y = local.x * Mathf.Sin(angle) + local.y * Mathf.Cos(angle);
                    mesh.SetNextVertex(new Vertex {
                        position = new Vector3(center.x+x+sign*spread*w*.7f, center.y+y, Vertex.nearZ),
                        tint = new Color(1,1,1,alpha),
                        uv = new Vector2(uv.x+u*uv.width,uv.y+(1-v)*uv.height)
                    });
                }
                ushort b=(ushort)(side*4);
                mesh.SetNextIndex(b);mesh.SetNextIndex((ushort)(b+1));mesh.SetNextIndex((ushort)(b+2));
                mesh.SetNextIndex(b);mesh.SetNextIndex((ushort)(b+2));mesh.SetNextIndex((ushort)(b+3));
            }
        }
    }

    /// <summary>Deforms the outer silhouette while keeping the central face/body still.</summary>
    private sealed class WingPortrait : VisualElement
    {
        private readonly Texture2D texture;
        public float Openness;
        public WingPortrait(Texture2D source)
        {
            texture=source; pickingMode=PickingMode.Ignore;
            generateVisualContent+=Draw;
        }
        private void Draw(MeshGenerationContext context)
        {
            if(texture==null || contentRect.width<=0 || contentRect.height<=0)return;
            const int n=20;
            var mesh=context.Allocate((n+1)*(n+1),n*n*6,texture);
            float scale=Mathf.Min(contentRect.width/texture.width,contentRect.height/texture.height);
            float w=texture.width*scale,h=texture.height*scale;
            Vector2 origin=new Vector2((contentRect.width-w)*.5f,(contentRect.height-h)*.5f);
            Rect uv=mesh.uvRegion;
            for(int y=0;y<=n;y++)for(int x=0;x<=n;x++)
            {
                float u=(float)x/n,v=(float)y/n;
                float side=Mathf.SmoothStep(0,1,Mathf.InverseLerp(.16f,.48f,Mathf.Abs(u-.5f)));
                float squeeze=1f-(1-Openness)*.65f*side;
                float px=.5f+(u-.5f)*squeeze;
                mesh.SetNextVertex(new Vertex {
                    position=new Vector3(origin.x+px*w,origin.y+v*h,Vertex.nearZ),
                    tint=Color.white,
                    uv=new Vector2(uv.x+u*uv.width,uv.y+(1-v)*uv.height)
                });
            }
            for(int y=0;y<n;y++)for(int x=0;x<n;x++)
            {
                ushort a=(ushort)(y*(n+1)+x),b=(ushort)(a+1),c=(ushort)(a+n+1),d=(ushort)(c+1);
                mesh.SetNextIndex(a);mesh.SetNextIndex(b);mesh.SetNextIndex(c);
                mesh.SetNextIndex(b);mesh.SetNextIndex(d);mesh.SetNextIndex(c);
            }
        }
    }
}
