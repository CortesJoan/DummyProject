using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

/// <summary>
/// One switch for the postgame, from the editor.
///
/// MementoPostgameRelease.Enabled is the single release flag the handoff requires to end
/// at false. This window flips it by editing the source, so the change is durable and
/// visible in version control, instead of leaving a static field toggled only in memory.
///
/// It also offers a session-only toggle for play-mode testing: that one does NOT touch the
/// file and is lost on the next domain reload, which is exactly what you want while
/// trying the door without committing anything.
/// </summary>
public sealed class MementoPostgameSwitcher : EditorWindow
{
    private const string SourceName = "MementoPostgameRelease.cs";
    private const string FieldPattern =
        @"(public\s+static\s+bool\s+Enabled\s*=\s*)(true|false)(\s*;)";

    private Vector2 scroll;

    [MenuItem("Tools/Memento/Postgame - interruptor")]
    public static void Open()
    {
        MementoPostgameSwitcher window = GetWindow<MementoPostgameSwitcher>(false, "Postgame", true);
        window.minSize = new Vector2(420f, 300f);
        window.Show();
    }

    /// <summary>Quick toggle straight from the menu, showing a tick when the file says true.</summary>
    [MenuItem("Tools/Memento/Postgame/Activar o desactivar")]
    public static void ToggleFromMenu() => SetEnabled(!IsEnabledOnDisk());

    [MenuItem("Tools/Memento/Postgame/Activar o desactivar", true)]
    private static bool ToggleFromMenuValidate()
    {
        Menu.SetChecked("Tools/Memento/Postgame/Activar o desactivar", IsEnabledOnDisk());
        return true;
    }

    /// <summary>Play-mode only. Does not touch the file; resets on the next domain reload.</summary>
    [MenuItem("Tools/Memento/Postgame/Alternar solo en esta sesion (no edita el archivo)")]
    public static void ToggleInMemory()
    {
        AnimalMemory.Progression.MementoPostgameRelease.Enabled =
            !AnimalMemory.Progression.MementoPostgameRelease.Enabled;
        Debug.Log("[Postgame] Flag de sesion: " +
            AnimalMemory.Progression.MementoPostgameRelease.Enabled +
            " (no se ha tocado el archivo; se pierde al recargar el dominio).");
    }

    private void OnGUI()
    {
        string path = FindSourcePath();
        if (path == null)
        {
            EditorGUILayout.HelpBox(
                "No se encuentra " + SourceName + " en el proyecto. El interruptor no puede " +
                "editar el flag sin ese archivo.", MessageType.Error);
            return;
        }

        bool onDisk = IsEnabledOnDisk();
        bool inMemory = AnimalMemory.Progression.MementoPostgameRelease.Enabled;

        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("Segunda parte del juego", EditorStyles.boldLabel);
        EditorGUILayout.Space();

        EditorGUILayout.LabelField("Estado en el archivo", onDisk ? "ACTIVADO" : "desactivado",
            EditorStyles.boldLabel);
        EditorGUILayout.LabelField("Estado en memoria", inMemory ? "activado" : "desactivado");

        if (Application.isPlaying && inMemory != onDisk)
        {
            EditorGUILayout.HelpBox(
                "La memoria y el archivo no coinciden: hay un cambio de sesion en curso. Se " +
                "perdera al recargar el dominio.", MessageType.Info);
        }

        EditorGUILayout.Space();

        string label = onDisk
            ? "DESACTIVAR la segunda parte"
            : "ACTIVAR la segunda parte";
        if (GUILayout.Button(label, GUILayout.Height(38f)))
            SetEnabled(!onDisk);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Solo para probar en Play Mode", EditorStyles.boldLabel);
        if (GUILayout.Button(inMemory ? "Apagar el flag de esta sesion" : "Encender el flag de esta sesion"))
            ToggleInMemory();

        EditorGUILayout.Space();

        EditorGUILayout.HelpBox(
            "El flag por si solo no abre la segunda parte: el juego exige ademas haber " +
            "completado la campana (IsPostgameAvailable = Enabled && CampaignEndingUnlocked). " +
            "Con guardado de QA de campana completa, activar el flag basta para verla.",
            MessageType.None);

        EditorGUILayout.HelpBox(
            "Debe quedar en false en la entrega. Activarlo edita " + path + " y lanza una " +
            "recompilacion, asi que el cambio queda en el control de versiones y se ve en el " +
            "diff, en vez de quedar escondido en memoria.",
            MessageType.Warning);

        EditorGUILayout.EndScrollView();
    }

    private static string FindSourcePath()
    {
        string[] found = AssetDatabase.FindAssets(Path.GetFileNameWithoutExtension(SourceName) + " t:Script");
        foreach (string guid in found)
        {
            string candidate = AssetDatabase.GUIDToAssetPath(guid);
            if (Path.GetFileName(candidate) == SourceName) return candidate;
        }
        return null;
    }

    private static bool IsEnabledOnDisk()
    {
        string path = FindSourcePath();
        if (path == null) return false;
        try
        {
            Match match = Regex.Match(File.ReadAllText(path), FieldPattern);
            return match.Success && match.Groups[2].Value == "true";
        }
        catch (IOException) { return false; }
    }

    private static void SetEnabled(bool value)
    {
        string path = FindSourcePath();
        if (path == null)
        {
            Debug.LogError("[Postgame] No se encuentra " + SourceName + ".");
            return;
        }

        string text;
        try { text = File.ReadAllText(path); }
        catch (IOException e)
        {
            Debug.LogError("[Postgame] No se pudo leer " + path + ": " + e.Message);
            return;
        }

        Match match = Regex.Match(text, FieldPattern);
        if (!match.Success)
        {
            Debug.LogError("[Postgame] No se encontro la linea del flag en " + path +
                "; el archivo cambio de forma. Revisalo a mano.");
            return;
        }

        if (match.Groups[2].Value == (value ? "true" : "false"))
        {
            Debug.Log("[Postgame] Ya estaba en " + (value ? "true" : "false") + ".");
            return;
        }

        string updated = Regex.Replace(text, FieldPattern,
            m => m.Groups[1].Value + (value ? "true" : "false") + m.Groups[3].Value);

        bool confirming = value;
        if (confirming && !EditorUtility.DisplayDialog(
                "Activar la segunda parte",
                "Se va a poner MementoPostgameRelease.Enabled = true.\n\n" +
                "Recuerda que debe volver a false antes de entregar o publicar, y que para " +
                "verla hace falta ademas una partida con la campana completada.\n\n¿Continuar?",
                "Activar", "Cancelar"))
            return;

        try { File.WriteAllText(path, updated); }
        catch (IOException e)
        {
            Debug.LogError("[Postgame] No se pudo escribir " + path + ": " + e.Message);
            return;
        }

        AssetDatabase.Refresh();
        Debug.Log("[Postgame] Flag " + (value ? "ACTIVADO" : "desactivado") + " en " + path +
            ". Unity recompilara.");
    }
}
