using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>Non-destructive preview of the runtime GOD transition.</summary>
public sealed class MementoGodAscensionPreview : EditorWindow
{
    private VisualElement stage;
    [MenuItem("Tools/Memento/Preview GOD phase transition")]
    public static void Open()
    {
        var window = GetWindow<MementoGodAscensionPreview>();
        window.titleContent = new GUIContent("GOD · Cambio de fase");
        window.minSize = new Vector2(320, 500);
        window.Show();
    }
    public void CreateGUI()
    {
        rootVisualElement.Clear();
        var replay = new Button(Replay) { text = "REPETIR CAMBIO DE FASE · toca la escena para saltar" };
        replay.style.height = 38;
        rootVisualElement.Add(replay);
        stage = new VisualElement();
        stage.style.flexGrow = 1;
        stage.style.backgroundColor = new Color(.018f,.03f,.07f);
        rootVisualElement.Add(stage);
        stage.schedule.Execute(Replay).StartingIn(300);
    }
    public void Replay()
    {
        stage.Clear();
        var view = new MementoGodAscension();
        stage.Add(view);
        view.Play(() => {
            var end = new Image { image = Resources.Load<Texture2D>("AnimalMemory/ArtV2/god-phase2-wide-preview-v1"), scaleMode = ScaleMode.ScaleToFit };
            end.style.flexGrow = 1;
            stage.Add(end);
        });
    }
}
