using System;
using AnimalMemory.Progression;
using UnityEngine.UIElements;

/// <summary>Presentation only. Unlocks, selected node and progress belong to the controller/model.</summary>
public sealed class MementoPostgameSelectionView : IDisposable
{
    private readonly VisualElement overlay;
    private readonly Button storyEntry;
    private readonly Button campaignEntry;
    private readonly Button[] nodes = new Button[MementoPostgameEncounters.Count];
    private readonly Label[] statuses = new Label[MementoPostgameEncounters.Count];
    private readonly Label detail;
    private readonly Label progress;
    private readonly Button start;
    private readonly ScrollView scroll;
    private int selected = -1;
    private bool visible;

    public MementoPostgameSelectionView(VisualElement root, Action open, Action close,
        Action<int> select, Action startEncounter, Action<VisualElement, int> applyPortrait)
    {
        storyEntry = new Button(open) { name = "postgame-open", text = "LA PUERTA INTERIOR" };
        storyEntry.AddToClassList("page-secondary-button");
        root.Q<VisualElement>("page-story")?.Add(storyEntry);
        campaignEntry = new Button(open) { name = "campaign-postgame", text = "LA PUERTA INTERIOR" };
        campaignEntry.AddToClassList("postgame-entry");
        root.Q<VisualElement>("campaign-overlay")?.Q<VisualElement>(className: "campaign-panel")?.Add(campaignEntry);

        overlay = new VisualElement { name = "postgame-overlay" };
        overlay.AddToClassList("postgame-overlay");
        var panel = new VisualElement();
        panel.AddToClassList("postgame-panel");
        var heading = new VisualElement();
        heading.AddToClassList("postgame-heading");
        var title = new Label("LA PUERTA INTERIOR");
        title.AddToClassList("postgame-title");
        var back = new Button(close) { name = "postgame-close", text = "VOLVER" };
        back.AddToClassList("postgame-action");
        heading.Add(title);
        heading.Add(back);
        panel.Add(heading);
        progress = new Label();
        progress.AddToClassList("postgame-progress");
        panel.Add(progress);
        scroll = new ScrollView(ScrollViewMode.Vertical) { name = "postgame-encounters" };
        scroll.AddToClassList("postgame-scroll");
        for (int i = 0; i < nodes.Length; i++)
        {
            int id = i;
            var encounter = MementoPostgameEncounters.Get(i);
            var node = new Button(() => select(id)) { name = "postgame-node-" + i };
            node.AddToClassList("postgame-node");
            var portrait = new VisualElement { pickingMode = PickingMode.Ignore };
            portrait.AddToClassList("postgame-portrait");
            if (encounter.PlayerGuideId >= 0)
                applyPortrait?.Invoke(portrait, encounter.PlayerGuideId);
            else
            {
                var you = new Label("TÚ") { pickingMode = PickingMode.Ignore };
                portrait.Add(you);
            }
            node.Add(portrait);
            var copy = new VisualElement { pickingMode = PickingMode.Ignore };
            copy.AddToClassList("postgame-node-copy");
            var label = new Label((i + 1).ToString("00") + "  " + encounter.Title) { pickingMode = PickingMode.Ignore };
            label.AddToClassList("postgame-node-title");
            copy.Add(label);
            statuses[i] = new Label { pickingMode = PickingMode.Ignore };
            statuses[i].AddToClassList("postgame-node-status");
            copy.Add(statuses[i]);
            node.Add(copy);
            nodes[i] = node;
            scroll.Add(node);
        }
        panel.Add(scroll);
        detail = new Label();
        detail.AddToClassList("postgame-detail");
        panel.Add(detail);
        start = new Button(startEncounter) { name = "postgame-start" };
        start.AddToClassList("postgame-action");
        start.AddToClassList("postgame-start");
        panel.Add(start);
        overlay.Add(panel);
        root.Add(overlay);
        SetDisplay(storyEntry, false);
        SetDisplay(campaignEntry, false);
        SetDisplay(overlay, false);
    }

    public void Refresh(bool available, bool open, bool portrait, int selectedId,
        int completedMask, Func<int, bool> canEnter, bool canStart)
    {
        SetDisplay(storyEntry, available);
        SetDisplay(campaignEntry, available);
        bool show = available && open;
        SetDisplay(overlay, show);
        overlay.EnableInClassList("portrait", portrait);
        if (!show) { visible = false; return; }
        if (!visible) overlay.BringToFront();
        bool scrollToSelection = !visible || selected != selectedId;
        visible = true;
        selected = selectedId;
        int cleared = 0;
        for (int i = 0; i < nodes.Length; i++)
        {
            bool completed = (completedMask & (1 << i)) != 0;
            bool unlocked = canEnter(i);
            if (completed) cleared++;
            nodes[i].SetEnabled(unlocked);
            nodes[i].EnableInClassList("selected", i == selected);
            nodes[i].EnableInClassList("cleared", completed);
            statuses[i].text = completed ? "COMPLETADO · REJUGAR"
                : unlocked ? "DISPONIBLE" : "COMPLETA EL ENCUENTRO ANTERIOR";
        }
        progress.text = cleared + " / " + nodes.Length + " encuentros completados";
        bool valid = selected >= 0 && selected < nodes.Length && canEnter(selected);
        start.SetEnabled(valid && canStart);
        if (!valid)
        {
            detail.text = "";
            start.text = "ELIGE UN ENCUENTRO";
            return;
        }
        var encounter = MementoPostgameEncounters.Get(selected);
        detail.text = encounter.IsScriptedDefeat
            ? "REI · ENCUENTRO DE HISTORIA"
            : encounter.Rows + " × " + encounter.Columns + " · " +
              MementoPostgameEncounters.GetOpponentName(encounter.OpponentId).ToUpperInvariant() +
              "\n" + MementoPostgameStory.GetCustodiaIdentity(encounter.OpponentId);
        start.text = !canStart ? "ENCUENTRO NO DISPONIBLE"
            : (completedMask & (1 << selected)) != 0 ? "REJUGAR ENCUENTRO" : "CONTINUAR";
        if (scrollToSelection)
        {
            int target = selected;
            scroll.schedule.Execute(() =>
            {
                if (visible && selected == target) scroll.ScrollTo(nodes[target]);
            });
        }
    }

    public void Dispose()
    {
        overlay.RemoveFromHierarchy();
        storyEntry.RemoveFromHierarchy();
        campaignEntry.RemoveFromHierarchy();
    }

    private static void SetDisplay(VisualElement element, bool show) =>
        element.style.display = show ? DisplayStyle.Flex : DisplayStyle.None;
}
