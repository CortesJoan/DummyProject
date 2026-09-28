using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private const float MusicCrossfadeSeconds = 0.7f;
    private const string DuelMusicRoot = "Audio/Music/Duels/";

    private static readonly string[] DuelMusicKeys =
    {
        "aki",
        "mika",
        "yoru",
        "hana",
        "momo",
        "rei",
        "custodia_calculo",
        "custodia_azar",
        "custodia_vinculo",
        "custodia_orgullo",
        "creador"
    };

    private static AudioManager instance;

    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioSource bgmAudioSource;

    [Header("Audio clips")]
    [SerializeField] private AudioClip triesToMatchSound;
    [SerializeField] private AudioClip matchSound;
    [SerializeField] private AudioClip mismatchSound;
    [SerializeField] private AudioClip winSound;

    [SerializeField] private CardMatchUI cardMatchUI;

    private AudioSource secondaryBgmAudioSource;
    private AudioSource activeBgmAudioSource;
    private AudioClip defaultBgmClip;
    private float defaultBgmVolume = 1f;
    private Coroutine musicTransition;
    private float TargetBgmVolume => defaultBgmVolume * MementoAudioSettings.MusicVolume;

    public static string GetDuelMusicResourcePath(int guideId)
    {
        if (guideId < 0 || guideId >= DuelMusicKeys.Length)
            return null;

        return DuelMusicRoot + "duel_" + DuelMusicKeys[guideId];
    }

    public static bool PlayDuelMusic(int guideId)
    {
        AudioManager manager = ResolveInstance();
        if (manager == null)
            return false;

        string resourcePath = GetDuelMusicResourcePath(guideId);
        if (string.IsNullOrEmpty(resourcePath))
            return false;

        AudioClip clip = Resources.Load<AudioClip>(resourcePath);
        if (clip == null)
        {
            // Unreleased opponents must not inherit the previous rival\'s theme.
            if (guideId >= 6) manager.CrossfadeTo(null);
            Debug.LogWarning(
                $"Duel music is missing for guide {guideId}: Resources/{resourcePath}.");
            return false;
        }

        manager.CrossfadeTo(clip);
        return true;
    }

    public static void RestoreDefaultMusic()
    {
        AudioManager manager = ResolveInstance();
        if (manager != null)
            manager.CrossfadeTo(manager.defaultBgmClip);
    }

    private static AudioManager ResolveInstance()
    {
        if (instance == null)
            instance = FindFirstObjectByType<AudioManager>();

        return instance;
    }

    private void Awake()
    {
        instance = this;
        MementoAudioSettings.Changed += ApplyVolumeSettings;
        activeBgmAudioSource = bgmAudioSource;
        if (bgmAudioSource == null)
            return;

        defaultBgmClip = bgmAudioSource.clip;
        defaultBgmVolume = Mathf.Max(0.0001f, bgmAudioSource.volume);
        secondaryBgmAudioSource = gameObject.AddComponent<AudioSource>();
        CopyMusicSettings(bgmAudioSource, secondaryBgmAudioSource);
        secondaryBgmAudioSource.playOnAwake = false;
        secondaryBgmAudioSource.volume = 0f;
        secondaryBgmAudioSource.Stop();
        ApplyVolumeSettings();
    }

    private void Start()
    {
        if (cardMatchUI == null)
        {
            Debug.LogWarning("AudioManager has no CardMatchUI assigned.");
            return;
        }

        cardMatchUI.onMatchMade.AddListener(PlayMatchSound);
        cardMatchUI.onMatchFailed.AddListener(PlayMismatchSound);
        cardMatchUI.onTryingMatch.AddListener(PlayTriesToMatchSound);
        cardMatchUI.onWinEvent.AddListener(PlayWinSound);
    }

    private void OnDestroy()
    {
        MementoAudioSettings.Changed -= ApplyVolumeSettings;
        if (instance == this)
            instance = null;
    }

    private static void CopyMusicSettings(AudioSource source, AudioSource target)
    {
        target.outputAudioMixerGroup = source.outputAudioMixerGroup;
        target.mute = source.mute;
        target.bypassEffects = source.bypassEffects;
        target.bypassListenerEffects = source.bypassListenerEffects;
        target.bypassReverbZones = source.bypassReverbZones;
        target.priority = source.priority;
        target.pitch = source.pitch;
        target.panStereo = source.panStereo;
        target.spatialBlend = source.spatialBlend;
        target.reverbZoneMix = source.reverbZoneMix;
        target.loop = true;
    }

    private void CrossfadeTo(AudioClip targetClip)
    {
        if (bgmAudioSource == null || secondaryBgmAudioSource == null)
            return;
        if (activeBgmAudioSource != null &&
            activeBgmAudioSource.clip == targetClip &&
            activeBgmAudioSource.isPlaying)
        {
            return;
        }

        if (musicTransition != null)
            StopCoroutine(musicTransition);
        musicTransition = StartCoroutine(CrossfadeRoutine(targetClip));
    }

    private IEnumerator CrossfadeRoutine(AudioClip targetClip)
    {
        AudioSource outgoing = activeBgmAudioSource ?? bgmAudioSource;
        AudioSource incoming = outgoing == bgmAudioSource
            ? secondaryBgmAudioSource
            : bgmAudioSource;
        float outgoingStart = outgoing != null ? outgoing.volume : 0f;

        if (targetClip != null)
        {
            incoming.Stop();
            incoming.clip = targetClip;
            incoming.loop = true;
            incoming.time = 0f;
            incoming.volume = 0f;
            incoming.Play();
        }

        float elapsed = 0f;
        while (elapsed < MusicCrossfadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float progress = Mathf.Clamp01(elapsed / MusicCrossfadeSeconds);
            if (outgoing != null)
                outgoing.volume = Mathf.Lerp(outgoingStart, 0f, progress);
            if (targetClip != null)
                incoming.volume = Mathf.Lerp(0f, TargetBgmVolume, progress);
            yield return null;
        }

        if (outgoing != null)
        {
            outgoing.Stop();
            outgoing.clip = null;
            outgoing.volume = TargetBgmVolume;
        }

        if (targetClip != null)
        {
            incoming.volume = TargetBgmVolume;
            activeBgmAudioSource = incoming;
        }
        else
        {
            activeBgmAudioSource = bgmAudioSource;
        }

        musicTransition = null;
    }

    private void ApplyVolumeSettings()
    {
        float target = TargetBgmVolume;
        if (activeBgmAudioSource != null && activeBgmAudioSource.isPlaying)
            activeBgmAudioSource.volume = target;
        if (bgmAudioSource != null && bgmAudioSource != activeBgmAudioSource)
            bgmAudioSource.volume = 0f;
        if (secondaryBgmAudioSource != null && secondaryBgmAudioSource != activeBgmAudioSource)
            secondaryBgmAudioSource.volume = 0f;
    }

    private void PlayMatchSound()
    {
        PlayOneShot(matchSound);
    }

    private void PlayMismatchSound()
    {
        PlayOneShot(mismatchSound);
    }

    private void PlayTriesToMatchSound()
    {
        PlayOneShot(triesToMatchSound);
    }

    private void PlayWinSound()
    {
        PlayOneShot(winSound);
    }

    private void PlayOneShot(AudioClip clip)
    {
        if (audioSource != null && clip != null)
            audioSource.PlayOneShot(clip, MementoAudioSettings.SfxVolume);
    }
}
