using UnityEngine;
using UnityEngine.UIElements;

/// <summary>Presentation-only illustrated chrome. All existing buttons keep their callbacks.</summary>
public static class MementoGameChrome
{
    private static Sprite[] icons;
    private static readonly string[] Buttons = { "nav-home", "nav-story", "nav-team", "nav-collection", "nav-settings" };

    public static void Install(VisualElement root)
    {
        if (root == null) return;
        if (icons == null)
        {
            Texture2D sheet = Resources.Load<Texture2D>("MementoNavigationV4");
            if (sheet != null)
            {
                icons = new Sprite[5];
                for (int i = 0; i < icons.Length; i++)
                {
                    Rect rect = new Rect(i * sheet.width / 5f, sheet.height * .19f,
                        sheet.width / 5f, sheet.height * .67f);
                    icons[i] = Sprite.Create(sheet, rect, Vector2.one * .5f, 100, 0, SpriteMeshType.FullRect);
                    icons[i].name = Buttons[i] + "-illustration";
                    icons[i].hideFlags = HideFlags.DontSave;
                }
            }
        }
        for (int i = 0; i < Buttons.Length; i++)
        {
            Button button = root.Q<Button>(Buttons[i]);
            if (button == null || icons == null || button.Q("navigation-art") != null) continue;
            var art = new Image { name = "navigation-art", sprite = icons[i], scaleMode = ScaleMode.ScaleToFit,
                pickingMode = PickingMode.Ignore };
            art.AddToClassList("navigation-art");
            button.Insert(0, art);
        }
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void Reset()
    {
        if (icons != null)
            foreach (Sprite icon in icons)
                if (icon != null)
                {
                    if (Application.isPlaying) Object.Destroy(icon);
                    else Object.DestroyImmediate(icon);
                }
        icons = null;
    }
}
