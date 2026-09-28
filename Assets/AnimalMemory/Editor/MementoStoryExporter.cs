using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using AnimalMemory.Progression;
using UnityEditor;
using UnityEngine;

/// <summary>
/// Exports the whole effective script, in two formats, from ONE traversal of the real
/// scene sources. The handoff forbids keeping a hand-written copy as a second source of
/// truth, so both documents are generated and never edited by hand.
///
///  1. HISTORIA_COMPLETA.md — reference: tables, provenance, Japanese voice pairing.
///  2. HISTORIA_GUION.md    — plain dialogue script, for handing to outside readers who
///                            need to judge whether the story stands on its own.
/// </summary>
public static class MementoStoryExporter
{
    private const string ReferencePath = "Documentation~/HISTORIA_COMPLETA.md";
    private const string ScriptPath = "Documentation~/HISTORIA_GUION.md";
    private const string VoiceScriptPath =
        "Assets/AnimalMemory/Audio/story_voice_script_v4.json";

    [Serializable]
    private sealed class VoiceLine
    {
        public string guide, id, es, ja, direction, status;
    }

    [Serializable]
    private sealed class VoiceScript
    {
        public string _doc;
        public List<VoiceLine> lines = new List<VoiceLine>();
    }

    private struct Section
    {
        public readonly string Heading;
        public readonly MementoMatchStoryScene Scene;
        public Section(string heading, MementoMatchStoryScene scene)
        {
            Heading = heading; Scene = scene;
        }
    }

    [MenuItem("Tools/Memento/Export complete story (referencia)")]
    public static void ExportReference() => Write(ReferencePath, true);

    [MenuItem("Tools/Memento/Export dialogue script (guion)")]
    public static void ExportScript() => Write(ScriptPath, false);

    [MenuItem("Tools/Memento/Export both story documents")]
    public static void ExportBoth()
    {
        Write(ReferencePath, true);
        Write(ScriptPath, false);
    }

    private static void Write(string relativePath, bool referenceFormat)
    {
        string projectRoot = Directory.GetParent(Application.dataPath).FullName;
        string output = Path.Combine(projectRoot, relativePath);

        VoiceScript voices = LoadVoices(projectRoot);
        var byText = new Dictionary<string, VoiceLine>();
        if (voices != null)
            foreach (VoiceLine line in voices.lines)
                if (!string.IsNullOrEmpty(line.es) && !byText.ContainsKey(line.es))
                    byText[line.es] = line;

        List<Section> sections = CollectScenes();
        var text = new StringBuilder();
        if (referenceFormat) WriteReference(text, sections, voices, byText);
        else WriteScript(text, sections);

        Directory.CreateDirectory(Path.GetDirectoryName(output));
        File.WriteAllText(output, text.ToString(), new UTF8Encoding(false));
        AssetDatabase.Refresh();
        Debug.Log("[MementoStoryExporter] Escrito " + output + " (" + sections.Count + " escenas).");
    }

    /// <summary>The single traversal both formats share.</summary>
    private static List<Section> CollectScenes()
    {
        var sections = new List<Section>();
        for (int id = 0; id <= MementoMatchCampaignRules.EpilogueSceneId; id++)
            sections.Add(new Section("CAMPAÑA", MementoMatchCampaignRules.GetStoryScene(id)));
        for (int levelId = 0; levelId < MementoMatchCampaignRules.LevelCount; levelId++)
        {
            if (!MementoMatchCampaignRules.GetLevel(levelId).IsBoss) continue;
            sections.Add(new Section("CAMPAÑA · REINTENTO",
                MementoMatchCampaignRules.GetRetryScene(levelId)));
        }
        sections.Add(new Section("POSTGAME", MementoPostgameStory.BuildOpeningScene()));
        for (int id = 0; id < MementoPostgameEncounters.Count; id++)
        {
            if (MementoPostgameStory.HasIntro(id))
                sections.Add(new Section("POSTGAME · ENTRADA", MementoPostgameStory.BuildIntro(id)));
            if (MementoPostgameEncounters.Get(id).IsScriptedDefeat)
            {
                // El checkpoint guionizado ES el desenlace de ese encuentro, y en el
                // juego se juega en su sitio: antes del duelo final. Emitirlo al final
                // hacia que el documento mintiera sobre el orden de ejecucion.
                sections.Add(new Section("POSTGAME · GUIONIZADO", MementoPostgameStory.BuildScriptedScene()));
            }
            else if (MementoPostgameStory.HasOutcome(id))
            {
                sections.Add(new Section("POSTGAME · VICTORIA", MementoPostgameStory.BuildVictory(id)));
                sections.Add(new Section("POSTGAME · DERROTA", MementoPostgameStory.BuildDefeat(id)));
            }
        }
        sections.Add(new Section("POSTGAME · EPÍLOGO", MementoPostgameStory.BuildEpilogue()));
        return sections;
    }

    /// <summary>Plain script: headings, stage directions and dialogue. Nothing else.</summary>
    private static void WriteScript(StringBuilder text, List<Section> sections)
    {
        text.AppendLine("# MEMENTO MATCH — GUION COMPLETO");
        text.AppendLine();
        text.AppendLine("Documento generado desde las fuentes efectivas del juego. No editado a mano.");
        text.AppendLine("«ESCENA» es una acotación; el resto son réplicas. `{PLAYER}` es el nombre que");
        text.AppendLine("elige quien juega.");
        text.AppendLine();

        string currentSection = null;
        foreach (Section section in sections)
        {
            MementoMatchStoryScene scene = section.Scene;
            if (scene == null) continue;
            if (currentSection != section.Heading)
            {
                currentSection = section.Heading;
                text.AppendLine();
                text.AppendLine("# " + currentSection);
                text.AppendLine();
            }
            text.AppendLine("## " + scene.Chapter);
            text.AppendLine();
            foreach (MementoMatchStoryBeat beat in scene.Beats)
            {
                text.AppendLine(beat.Speaker);
                text.AppendLine("    " + beat.Text);
                text.AppendLine();
            }
        }
    }

    private static void WriteReference(StringBuilder text, List<Section> sections,
        VoiceScript voices, Dictionary<string, VoiceLine> byText)
    {
        int beats = 0, voiced = 0;

        text.AppendLine("# Memento Match — HISTORIA COMPLETA");
        text.AppendLine();
        text.AppendLine("Documento **generado**, no escrito a mano. Se produce con");
        text.AppendLine("`Tools > Memento > Export complete story (referencia)` leyendo las fuentes");
        text.AppendLine("efectivas: `MementoMatchCampaignRules`, `MementoPostgameStory`,");
        text.AppendLine("`MementoPostgameEncounters` y el manifiesto `story_voice_script_v4.json`.");
        text.AppendLine("Para leer la historia como guion, usa `HISTORIA_GUION.md`.");
        text.AppendLine();
        text.AppendLine("- Versión de Unity: " + Application.unityVersion);
        text.AppendLine("- Versión del juego: " + Application.version);
        text.AppendLine("- Generado: " + DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm") + " UTC");
        if (voices != null && !string.IsNullOrEmpty(voices._doc))
        {
            text.AppendLine();
            text.AppendLine("> " + voices._doc.Replace("\n", "\n> "));
        }
        text.AppendLine();

        text.AppendLine("## Campaña — tabla de niveles");
        text.AppendLine();
        text.AppendLine("| # | Mundo | Título | Objetivo | Tablero | Jefe | Oponente | Dificultad |");
        text.AppendLine("|---|---|---|---|---|---|---|---|");
        for (int id = 0; id < MementoMatchCampaignRules.LevelCount; id++)
        {
            MementoMatchCampaignLevel level = MementoMatchCampaignRules.GetLevel(id);
            text.AppendLine(string.Format("| {0} | {1} | {2} | {3} | {4}×{5} | {6} | {7} | {8} |",
                level.Id, level.WorldId, Escape(level.Title), Escape(level.Objective),
                level.Rows, level.Columns,
                level.IsFinalBoss ? "examen final" : level.IsBoss ? "duelo" : "—",
                level.IsBoss ? GuideName(level.OpponentGuideId) : "—",
                level.DifficultyId));
        }
        text.AppendLine();

        text.AppendLine("## Postgame — tabla de encuentros");
        text.AppendLine();
        text.AppendLine("| # | Título | Compañera | Oponente | Tablero | Guionizado |");
        text.AppendLine("|---|---|---|---|---|---|");
        for (int id = 0; id < MementoPostgameEncounters.Count; id++)
        {
            MementoPostgameEncounter encounter = MementoPostgameEncounters.Get(id);
            text.AppendLine(string.Format("| {0} | {1} | {2} | {3} | {4}×{5} | {6} |",
                encounter.Id, Escape(encounter.Title), GuideName(encounter.PlayerGuideId),
                MementoPostgameEncounters.GetOpponentName(encounter.OpponentId),
                encounter.Rows, encounter.Columns,
                encounter.IsScriptedDefeat ? "sí" : "—"));
        }
        text.AppendLine();

        text.AppendLine("## Escenas");
        foreach (Section section in sections)
        {
            if (section.Scene == null) continue;
            AppendScene(text, section.Scene, byText, ref beats, ref voiced);
        }

        text.AppendLine("## Cobertura de doblaje");
        text.AppendLine();
        text.AppendLine($"- Réplicas en las escenas: **{beats}**");
        text.AppendLine($"- Con línea de voz japonesa localizada por texto: **{voiced}**");
        text.AppendLine($"- Líneas del manifiesto: **{(voices != null ? voices.lines.Count : 0)}**");
        text.AppendLine();
        text.AppendLine("Una réplica sin línea de voz coincide por texto exacto con el manifiesto;");
        text.AppendLine("si el manifiesto cambia, este número lo delata en la siguiente exportación.");
    }

    private static void AppendScene(StringBuilder text, MementoMatchStoryScene scene,
        Dictionary<string, VoiceLine> byText, ref int beats, ref int voiced)
    {
        if (scene == null) return;
        text.AppendLine();
        text.AppendLine($"### Escena {scene.Id} — {Escape(scene.Chapter)}");
        text.AppendLine();
        text.AppendLine($"- Fase: `{scene.Phase}` · Mundo: {scene.WorldId} · Kicker: «{Escape(scene.Kicker)}»");
        text.AppendLine();
        for (int i = 0; i < scene.Beats.Count; i++)
        {
            MementoMatchStoryBeat beat = scene.Beats[i];
            beats++;
            string flags = beat.IsSilhouette ? " · silueta" : "";
            text.AppendLine($"**{i + 1}. {Escape(beat.Speaker)}** ({beat.Kind}, guía {beat.GuideId}, voz {beat.VoiceCue}{flags})");
            text.AppendLine();
            text.AppendLine($"> {Escape(beat.Text)}");
            text.AppendLine();
            if (byText.TryGetValue(beat.Text, out VoiceLine line))
            {
                voiced++;
                text.AppendLine($"- JA: {Escape(line.ja)}");
                text.AppendLine($"- Dirección: {Escape(line.direction)} · Estado: `{line.status}`");
                text.AppendLine();
            }
        }
    }

    private static VoiceScript LoadVoices(string projectRoot)
    {
        string path = Path.Combine(projectRoot, VoiceScriptPath);
        if (!File.Exists(path)) return null;
        try { return JsonUtility.FromJson<VoiceScript>(File.ReadAllText(path)); }
        catch (ArgumentException) { return null; }
    }

    private static string GuideName(int guideId)
    {
        if (guideId < 0) return "protagonista";
        if (guideId < AnimalMemoryContentIds.GuideCount)
            return new AnimalMemoryProgression().GetGuideName(guideId);
        return MementoPostgameEncounters.IsPostgameOpponent(guideId)
            ? MementoPostgameEncounters.GetOpponentName(guideId)
            : "guía " + guideId;
    }

    private static string Escape(string value) =>
        string.IsNullOrEmpty(value) ? string.Empty : value.Replace("|", "\\|").Replace("\n", " ");
}
