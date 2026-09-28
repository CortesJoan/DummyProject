using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Editor-only save slots.
///
/// The game keeps a single save file (SaveSystem.SavePath, game.dat), so this works by
/// copying that file. It exists to let the owner stash the current playthrough, start a
/// fresh one, and come back — which the game itself does not need to expose yet.
///
/// Safety rules it follows, from the handoff: destructive actions always ask first, the
/// tool offers to back up the current save before replacing or deleting it, and it never
/// touches anything except that file and the slot folder.
/// </summary>
public sealed class MementoSaveSlots : EditorWindow
{
    private const string SlotFolderName = "memento-save-slots";
    private const string SlotPrefix = "ranura-";
    private const string Extension = ".dat";

    private string renameTarget;
    private string renameValue = "";
    private Vector2 scroll;

    [MenuItem("Tools/Memento/Partidas guardadas (ranuras)")]
    public static void Open()
    {
        MementoSaveSlots window = GetWindow<MementoSaveSlots>(false, "Partidas", true);
        window.minSize = new Vector2(560f, 360f);
        window.Show();
    }

    private static string SavePath => SaveSystem.SavePath;

    private static string SlotFolder =>
        Path.Combine(Path.GetDirectoryName(SavePath) ?? ".", SlotFolderName);

    private void OnGUI()
    {
        scroll = EditorGUILayout.BeginScrollView(scroll);

        EditorGUILayout.LabelField("Partida actual", EditorStyles.boldLabel);
        EditorGUILayout.LabelField(Summarise(SavePath), EditorStyles.wordWrappedLabel);
        EditorGUILayout.LabelField(SavePath, EditorStyles.miniLabel);

        EditorGUILayout.Space();
        using (new EditorGUILayout.HorizontalScope())
        {
            if (GUILayout.Button("Guardar la partida actual en una ranura nueva", GUILayout.Height(28f)))
                SaveCurrentToNewSlot();
            if (GUILayout.Button("Abrir la carpeta de ranuras", GUILayout.Height(28f)))
                EditorUtility.RevealInFinder(SlotFolder);
        }

        EditorGUILayout.Space();
        if (GUILayout.Button("Empezar una partida nueva (borra la actual)", GUILayout.Height(28f)))
            StartFreshRun();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Ranuras", EditorStyles.boldLabel);

        List<string> slots = ListSlots();
        if (slots.Count == 0)
        {
            EditorGUILayout.HelpBox(
                "Todavia no hay ranuras. Guarda la partida actual para crear la primera.",
                MessageType.Info);
        }

        foreach (string slot in slots)
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField(Path.GetFileNameWithoutExtension(slot), EditorStyles.boldLabel);
            EditorGUILayout.LabelField(Summarise(slot), EditorStyles.wordWrappedLabel);

            if (renameTarget == slot)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    renameValue = EditorGUILayout.TextField(renameValue);
                    if (GUILayout.Button("Confirmar", GUILayout.Width(90f)))
                    {
                        RenameSlot(slot, renameValue);
                        renameTarget = null;
                    }
                    if (GUILayout.Button("Cancelar", GUILayout.Width(80f)))
                        renameTarget = null;
                }
            }
            else
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("Cargar esta ranura", GUILayout.Width(160f)))
                        LoadSlot(slot);
                    if (GUILayout.Button("Sobrescribir con la actual", GUILayout.Width(180f)))
                        OverwriteSlot(slot);
                    if (GUILayout.Button("Renombrar", GUILayout.Width(90f)))
                    {
                        renameTarget = slot;
                        renameValue = Path.GetFileNameWithoutExtension(slot);
                    }
                    if (GUILayout.Button("Borrar", GUILayout.Width(70f)))
                        DeleteSlot(slot);
                }
            }
            EditorGUILayout.EndVertical();
        }

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox(
            "Las ranuras son copias de game.dat en " + SlotFolder + ". Cargar una ranura " +
            "sustituye la partida actual, asi que antes se guarda una copia de seguridad " +
            "automatica. Tras cargar o borrar hay que reiniciar el Play Mode para que el " +
            "juego relea el archivo.",
            MessageType.None);

        EditorGUILayout.EndScrollView();
    }

    // ---------- actions ----------

    private static void SaveCurrentToNewSlot()
    {
        if (!File.Exists(SavePath))
        {
            EditorUtility.DisplayDialog("Sin partida", "Todavia no existe game.dat que guardar.", "Vale");
            return;
        }
        Directory.CreateDirectory(SlotFolder);
        string name = SlotPrefix + DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture);
        string target = Path.Combine(SlotFolder, name + Extension);
        // Never overwrite an existing slot from an automatic name.
        int suffix = 1;
        while (File.Exists(target))
            target = Path.Combine(SlotFolder, name + "-" + suffix++ + Extension);

        File.Copy(SavePath, target);
        Debug.Log("[Partidas] Partida actual guardada en " + target);
    }

    private static void OverwriteSlot(string slot)
    {
        if (!EditorUtility.DisplayDialog("Sobrescribir ranura",
                "Se va a reemplazar:\n" + Path.GetFileName(slot) +
                "\n\ncon la partida actual.", "Sobrescribir", "Cancelar"))
            return;
        if (!File.Exists(SavePath))
        {
            EditorUtility.DisplayDialog("Sin partida", "No hay game.dat actual que copiar.", "Vale");
            return;
        }
        File.Copy(SavePath, slot, true);
        Debug.Log("[Partidas] Ranura sobrescrita: " + slot);
    }

    private static void LoadSlot(string slot)
    {
        if (!EditorUtility.DisplayDialog("Cargar ranura",
                "Se va a sustituir la partida actual por:\n" + Path.GetFileName(slot) +
                "\n\nLa partida actual se copiara antes a una ranura de seguridad.",
                "Cargar", "Cancelar"))
            return;

        if (File.Exists(SavePath))
        {
            Directory.CreateDirectory(SlotFolder);
            string backup = Path.Combine(SlotFolder,
                "antes-de-cargar-" + DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + Extension);
            File.Copy(SavePath, backup);
            Debug.Log("[Partidas] Copia de seguridad de la partida actual: " + backup);
        }

        File.Copy(slot, SavePath, true);
        Debug.Log("[Partidas] Ranura cargada: " + slot + ". Reinicia el Play Mode para que el juego la lea.");
    }

    private static void DeleteSlot(string slot)
    {
        if (!EditorUtility.DisplayDialog("Borrar ranura",
                "Se va a borrar:\n" + Path.GetFileName(slot) + "\n\nEsto no se puede deshacer.",
                "Borrar", "Cancelar"))
            return;
        File.Delete(slot);
        Debug.Log("[Partidas] Ranura borrada: " + slot);
    }

    private static void StartFreshRun()
    {
        if (!EditorUtility.DisplayDialog("Empezar una partida nueva",
                "Se va a BORRAR la partida actual (game.dat).\n\n" +
                "Si quieres conservarla, cancela y usa antes \"Guardar la partida actual en " +
                "una ranura nueva\".\n\n¿Borrar y empezar de cero?",
                "Borrar y empezar", "Cancelar"))
            return;

        if (File.Exists(SavePath))
        {
            Directory.CreateDirectory(SlotFolder);
            string backup = Path.Combine(SlotFolder,
                "antes-de-empezar-de-cero-" + DateTime.Now.ToString("yyyyMMdd-HHmmss", CultureInfo.InvariantCulture) + Extension);
            File.Copy(SavePath, backup);
            Debug.Log("[Partidas] Copia de seguridad antes de empezar de cero: " + backup);
        }

        if (!SaveSystem.DeleteSave())
        {
            EditorUtility.DisplayDialog("No se pudo borrar", "SaveSystem.DeleteSave devolvio false.", "Vale");
            return;
        }
        Debug.Log("[Partidas] Partida borrada. Entra en Play Mode para empezar una nueva.");
    }

    private static void RenameSlot(string slot, string value)
    {
        string cleaned = string.IsNullOrWhiteSpace(value) ? null : value.Trim();
        if (cleaned == null) return;
        foreach (char invalid in Path.GetInvalidFileNameChars())
            cleaned = cleaned.Replace(invalid, '-');

        string target = Path.Combine(SlotFolder, cleaned + Extension);
        if (File.Exists(target) && target != slot)
        {
            EditorUtility.DisplayDialog("Nombre en uso", "Ya existe una ranura con ese nombre.", "Vale");
            return;
        }
        File.Move(slot, target);
        Debug.Log("[Partidas] Ranura renombrada a " + target);
    }

    // ---------- reading ----------

    private static List<string> ListSlots()
    {
        var slots = new List<string>();
        if (!Directory.Exists(SlotFolder)) return slots;
        slots.AddRange(Directory.GetFiles(SlotFolder, "*" + Extension));
        slots.Sort(StringComparer.OrdinalIgnoreCase);
        return slots;
    }

    /// <summary>Reads the save to describe it. Falls back to the file date if unreadable.</summary>
    private static string Summarise(string path)
    {
        if (!File.Exists(path)) return "sin partida guardada";

        string when = File.GetLastWriteTime(path).ToString("yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
        string size = (new FileInfo(path).Length / 1024) + " KB";

        try
        {
            GameData data;
            var formatter = new BinaryFormatter();
            using (var stream = new FileStream(path, FileMode.Open, FileAccess.Read))
                data = formatter.Deserialize(stream) as GameData;
            if (data == null) return when + " · " + size;

            int cleared = CountBits(data.GetData<int>("CampaignClearedMask"));
            int two = CountBits(data.GetData<int>("CampaignTwoStarMask"));
            int three = CountBits(data.GetData<int>("CampaignThreeStarMask"));
            int postgame = CountBits(data.GetData<int>("PostgameCompletedMask"));
            int stars = cleared + two + three;
            return string.Format(CultureInfo.InvariantCulture,
                "{0}/21 niveles · {1} estrellas · postgame {2}/6 · {3} · {4}",
                cleared, stars, postgame, when, size);
        }
        catch (Exception)
        {
            // An unreadable save still shows its date, so the slot stays identifiable.
            return when + " · " + size + " · (no se pudo leer el contenido)";
        }
    }

    private static int CountBits(int value)
    {
        int count = 0;
        uint remaining = unchecked((uint)value);
        while (remaining != 0) { remaining &= remaining - 1; count++; }
        return count;
    }
}
