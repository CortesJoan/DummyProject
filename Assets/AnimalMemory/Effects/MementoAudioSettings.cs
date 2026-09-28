using System;
using UnityEngine;

public static class MementoAudioSettings
{
    private const string MusicKey = "MementoMatch.Audio.Music";
    private const string SfxKey = "MementoMatch.Audio.Sfx";
    private const string VoiceKey = "MementoMatch.Audio.Voice";

    public static event Action Changed;

    public static float MusicVolume =>
        Mathf.Clamp01(PlayerPrefs.GetFloat(MusicKey, 0.28f));
    public static float SfxVolume =>
        Mathf.Clamp01(PlayerPrefs.GetFloat(SfxKey, 0.9f));
    public static float VoiceVolume =>
        Mathf.Clamp01(PlayerPrefs.GetFloat(VoiceKey, 1f));

    public static void SetMusicVolume(float value)
    {
        SetVolume(MusicKey, value);
    }

    public static void SetSfxVolume(float value)
    {
        SetVolume(SfxKey, value);
    }

    public static void SetVoiceVolume(float value)
    {
        SetVolume(VoiceKey, value);
    }

    private static void SetVolume(string key, float value)
    {
        float clamped = Mathf.Clamp01(value);
        if (Mathf.Approximately(PlayerPrefs.GetFloat(key, -1f), clamped))
            return;

        PlayerPrefs.SetFloat(key, clamped);
        PlayerPrefs.Save();
        Changed?.Invoke();
    }
}
