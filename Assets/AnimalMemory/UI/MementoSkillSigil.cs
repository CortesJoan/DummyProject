using UnityEngine;
using UnityEngine.UIElements;

/// <summary>Animated, semantic skill emblem drawn in panel space; never intercepts input.</summary>
public sealed class MementoSkillSigil : VisualElement
{
    private float began;
    private int technique;
    private bool observing;
    private readonly IVisualElementScheduledItem animation;
    public MementoSkillSigil()
    {
        name = "skill-semantic-animation";
        pickingMode = PickingMode.Ignore;
        style.position = Position.Absolute;
        style.left = style.right = style.top = style.bottom = 0;
        generateVisualContent += Draw;
        animation = schedule.Execute(MarkDirtyRepaint).Every(33);
        animation.Pause();
    }
    public void Play(int id, bool hostile, bool copied)
    {
        technique = id;
        observing = hostile && !copied;
        began = Time.realtimeSinceStartup;
        animation.Resume();
        MarkDirtyRepaint();
    }
    public void Stop() { animation.Pause(); }

    private void Draw(MeshGenerationContext context)
    {
        float w = contentRect.width, h = contentRect.height;
        if (w <= 0 || h <= 0) return;
        float time = Time.realtimeSinceStartup - began;
        float grow = Mathf.SmoothStep(0f, 1f, time / 0.45f);
        float r = Mathf.Min(w * 0.15f, h * 0.135f) * grow;
        Vector2 c = new Vector2(w * 0.5f, h * (h > w ? 0.27f : 0.25f));
        var p = context.painter2D;
        Color ink = new Color(1f, 0.96f, 0.86f);
        Color accent = technique == 0 || technique == 3 ? new Color(.70f,.91f,.75f)
            : technique == 2 ? new Color(.80f,.79f,1f) : new Color(1f,.69f,.62f);
        p.lineWidth = Mathf.Max(2f, r * .035f);
        Circle(p,c,r, new Color(accent.r,accent.g,accent.b,.32f));
        for (int i=0;i<6;i++)
        {
            float angle=time * .65f + i * Mathf.PI / 3f;
            Vector2 dot=c+new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*r*1.13f;
            Star(p,dot,r*.045f,accent);
        }
        if (observing || technique == 2 || technique == 4)
        {
            float pulse = .9f + .08f * Mathf.Sin(time*3f);
            Card(p,c+new Vector2(-r*.38f,0),r*.36f,ink);
            Card(p,c+new Vector2(r*.38f,0),r*.36f,ink);
            Star(p,c+new Vector2(-r*.38f,0),r*.13f*pulse,accent);
            Star(p,c+new Vector2(r*.38f,0),r*.13f*pulse,accent);
            p.strokeColor=accent; p.BeginPath();
            p.MoveTo(c+new Vector2(-r*.38f,-r*.45f));
            p.LineTo(c+new Vector2(0,-r*.68f));
            p.LineTo(c+new Vector2(r*.38f,-r*.45f)); p.Stroke();
            if (technique==4) ArrowArc(p,c,r*.84f,time,accent);
        }
        else if (technique==0)
        {
            Circle(p,c,r*.65f,ink);
            float a=-time*1.7f;
            p.strokeColor=accent; p.BeginPath();
            p.MoveTo(c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r*.45f);
            p.LineTo(c); p.LineTo(c+new Vector2(.12f,-.23f)*r); p.Stroke();
            ArrowArc(p,c,r*.86f,-time,accent);
        }
        else if(technique==1)
        {
            p.strokeColor=accent; p.BeginPath();
            p.MoveTo(c+new Vector2(0,-.72f)*r);
            p.LineTo(c+new Vector2(.57f,-.47f)*r);
            p.LineTo(c+new Vector2(.47f,.29f)*r);
            p.LineTo(c+new Vector2(0,.76f)*r);
            p.LineTo(c+new Vector2(-.47f,.29f)*r);
            p.LineTo(c+new Vector2(-.57f,-.47f)*r); p.ClosePath(); p.Stroke();
            Star(p,c,r*.26f,ink);
        }
        else
        {
            for(int i=0;i<5;i++)
            {
                float a=i*Mathf.PI*2f/5f-Mathf.PI*.5f;
                Circle(p,c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r*.37f,r*.28f,accent);
            }
            Star(p,c,r*.22f,ink);
        }
    }
    private static void Circle(Painter2D p,Vector2 c,float r,Color color)
    {
        p.strokeColor=color; p.BeginPath();
        for(int i=0;i<=48;i++){float a=i*Mathf.PI*2f/48f;var v=c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r;if(i==0)p.MoveTo(v);else p.LineTo(v);}
        p.Stroke();
    }
    private static void Card(Painter2D p,Vector2 c,float r,Color color)
    {
        p.strokeColor=color;p.BeginPath();p.MoveTo(c+new Vector2(-.72f,-1)*r);
        p.LineTo(c+new Vector2(.72f,-1)*r);p.LineTo(c+new Vector2(.72f,1)*r);
        p.LineTo(c+new Vector2(-.72f,1)*r);p.ClosePath();p.Stroke();
    }
    private static void Star(Painter2D p,Vector2 c,float r,Color color)
    {
        p.fillColor=color;p.BeginPath();
        for(int i=0;i<8;i++){float a=i*Mathf.PI/4f;var v=c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r*(i%2==0?1f:.36f);if(i==0)p.MoveTo(v);else p.LineTo(v);}
        p.ClosePath();p.Fill();
    }
    private static void ArrowArc(Painter2D p,Vector2 c,float r,float rotation,Color color)
    {
        p.strokeColor=color;p.BeginPath(); Vector2 end=Vector2.zero;
        for(int i=0;i<=32;i++){float a=rotation+i*4.6f/32f;end=c+new Vector2(Mathf.Cos(a),Mathf.Sin(a))*r;if(i==0)p.MoveTo(end);else p.LineTo(end);}
        p.Stroke();Star(p,end,r*.09f,color);
    }
}
