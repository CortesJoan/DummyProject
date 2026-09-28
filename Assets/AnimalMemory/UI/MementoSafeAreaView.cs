using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace AnimalMemory.UI
{
    /// <summary>Keeps every Toolkit screen in the current safe area, including after live rotation.</summary>
    public sealed class MementoSafeAreaView
    {
        private readonly VisualElement root;
        private readonly VisualElement content;
        private Rect lastPanel;
        private Rect lastSafe;
        private Vector2 lastScreen;

        public MementoSafeAreaView(VisualElement root)
        {
            this.root = root;
            root.pickingMode = PickingMode.Ignore;
            content = new VisualElement { name = "memento-safe-area", pickingMode = PickingMode.Ignore };
            content.style.position = Position.Absolute;
            foreach (VisualElement child in root.Children().ToArray())
                content.Add(child);
            root.Add(content);
        }

        public void Refresh(Vector2 screen, Rect safeArea)
        {
            Rect panel = root.worldBound;
            if (screen.x <= 0f || screen.y <= 0f || panel.width <= 0f || panel.height <= 0f)
                return;
            if (panel == lastPanel && safeArea == lastSafe && screen == lastScreen)
                return;
            lastPanel = panel;
            lastSafe = safeArea;
            lastScreen = screen;
            float scaleX = panel.width / screen.x;
            float scaleY = panel.height / screen.y;
            content.style.left = safeArea.xMin * scaleX;
            content.style.right = (screen.x - safeArea.xMax) * scaleX;
            content.style.top = (screen.y - safeArea.yMax) * scaleY;
            content.style.bottom = safeArea.yMin * scaleY;
        }

        public bool TryGetHudInsets(
            Vector2 screen, Rect safeArea, VisualElement header, VisualElement footer,
            out float topInset, out float bottomInset)
        {
            topInset = bottomInset = 0f;
            Rect panel = root.worldBound;
            if (panel.width <= 0f || panel.height <= 0f || header == null || footer == null)
                return false;
            // Toolkit geometry can be one frame behind a new platform viewport.
            // Use the board's safe fallback until both coordinate systems agree.
            if (Mathf.Abs(panel.width / panel.height - screen.x / screen.y) > 0.02f)
                return false;
            float scaleY = screen.y / panel.height;
            float headerBottom = screen.y - (header.worldBound.yMax - panel.yMin) * scaleY;
            float footerTop = screen.y - (footer.worldBound.yMin - panel.yMin) * scaleY;
            if (header.worldBound.height <= 0f || footer.worldBound.height <= 0f)
                return false;
            topInset = Mathf.Clamp(safeArea.yMax - headerBottom, 0f, safeArea.height * 0.4f);
            bottomInset = Mathf.Clamp(footerTop - safeArea.yMin, 0f, safeArea.height * 0.45f);
            return true;
        }
    }
}
