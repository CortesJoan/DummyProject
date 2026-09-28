using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

/// <summary>Resolves dedicated recordings only for the exact source line and speaker.</summary>
public sealed class MementoStoryVoiceCatalog
{
    public const string ResourceRoot = "Audio/Voices/ja/StoryV3/";
    private static readonly string[] Guides = { "aki", "mika", "yoru", "hana", "momo", "rei" };
    private readonly Dictionary<string, string> paths = new Dictionary<string, string>(StringComparer.Ordinal);
    private readonly HashSet<string> ambiguous = new HashSet<string>(StringComparer.Ordinal);
    private readonly HashSet<string> usedIds = new HashSet<string>(StringComparer.Ordinal);
    private readonly HashSet<string> ambiguousPaths = new HashSet<string>(StringComparer.Ordinal);

    [Serializable]
    private sealed class Document { public Line[] lines; }
    [Serializable]
    private sealed class Line { public string id; public string guide; public string es; public string ja; }

    public MementoStoryVoiceCatalog(string json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return;
        Document document;
        try { document = JsonUtility.FromJson<Document>(json); }
        catch (ArgumentException) { return; }
        if (document == null || document.lines == null)
            return;
        foreach (Line line in document.lines)
        {
            if (line == null || Array.IndexOf(Guides, line.guide) < 0 ||
                string.IsNullOrWhiteSpace(line.es) || string.IsNullOrWhiteSpace(line.ja) ||
                line.id == null || !Regex.IsMatch(line.id, @"\Astory_[0-9]{2}_[0-9]{2}\z"))
                continue;
            if (!usedIds.Add(line.id))
                ambiguousPaths.Add(ResourceRoot + line.id);
            string key = line.guide + "\n" + line.es;
            if (paths.ContainsKey(key))
                ambiguous.Add(key);
            else
                paths.Add(key, ResourceRoot + line.id);
        }
    }

    /// <summary>Uses original text, before player-name interpolation. Ambiguous entries are silent.</summary>
    public bool TryGetResourcePath(int guideId, string originalText, out string path)
    {
        path = null;
        if (guideId < 0 || guideId >= Guides.Length || string.IsNullOrEmpty(originalText))
            return false;
        string key = Guides[guideId] + "\n" + originalText;
        if (ambiguous.Contains(key) || !paths.TryGetValue(key, out path))
            return false;
        if (!ambiguousPaths.Contains(path))
            return true;
        path = null;
        return false;
    }
}
