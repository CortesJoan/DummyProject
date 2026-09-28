using System;
using UnityEngine;

/// <summary>View-owned story audio; never falls back to menu or combat recordings.</summary>
public sealed class MementoStoryVoicePlayer : IDisposable
{
    private readonly MementoStoryVoiceCatalog catalog;
    private readonly Func<string, AudioClip> loadClip;
    private AudioSource source;

    public MementoStoryVoicePlayer(GameObject owner)
        : this(owner, LoadCatalog(), Resources.Load<AudioClip>) { }

    public MementoStoryVoicePlayer(GameObject owner, MementoStoryVoiceCatalog catalog,
        Func<string, AudioClip> loadClip)
    {
        if (owner == null) throw new ArgumentNullException(nameof(owner));
        this.catalog = catalog ?? throw new ArgumentNullException(nameof(catalog));
        this.loadClip = loadClip ?? throw new ArgumentNullException(nameof(loadClip));
        source = owner.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.priority = 32;
        ApplyVolume();
        MementoAudioSettings.Changed += ApplyVolume;
    }

    public bool Play(int guideId, string originalText)
    {
        Stop();
        string path;
        if (source == null || !catalog.TryGetResourcePath(guideId, originalText, out path))
            return false;
        AudioClip clip = loadClip(path);
        if (clip == null)
            return false;
        ApplyVolume();
        source.clip = clip;
        source.Play();
        return true;
    }

    public void Stop()
    {
        if (source == null) return;
        source.Stop();
        source.clip = null;
    }

    public void Dispose()
    {
        MementoAudioSettings.Changed -= ApplyVolume;
        Stop();
        if (source != null)
        {
            if (Application.isPlaying) UnityEngine.Object.Destroy(source);
            else UnityEngine.Object.DestroyImmediate(source);
            source = null;
        }
    }

    private void ApplyVolume()
    {
        if (source != null)
            source.volume = 0.92f * MementoAudioSettings.VoiceVolume;
    }

    private static MementoStoryVoiceCatalog LoadCatalog()
    {
        TextAsset asset = Resources.Load<TextAsset>(MementoStoryVoiceCatalog.ResourceRoot + "catalog");
        return new MementoStoryVoiceCatalog(asset == null ? null : asset.text);
    }
}
