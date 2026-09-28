using System;
using UnityEngine;

public enum MementoMatchResultSfx
{
    Victory,
    Defeat,
    Draw
}

[DefaultExecutionOrder(-850)]
public sealed class MementoMatchSfx : MonoBehaviour
{
    private const int SampleRate = 44100;
    private const int AnimalSetId = 0;
    private const string GeneratedSfxRoot = "Audio/Sfx/StableAudio3/";

    private static readonly string[] AnimalKeys =
    {
        "red_panda", "bear", "buffalo", "chick", "chicken",
        "cow", "crocodile", "dog", "duck", "elephant",
        "frog", "giraffe", "goat", "gorilla", "hippo",
        "horse", "monkey", "moose", "narwhal", "owl",
        "panda", "parrot", "penguin", "pig", "rabbit",
        "rhino", "sloth", "snake", "walrus", "whale",
        "zebra", "fox", "deer", "raccoon", "cat",
        "squirrel", "hedgehog", "koala"
    };

    private static readonly string[] AnimalAssetNames =
    {
        "sfx_animal_red_panda_squeak_v01",
        "sfx_animal_bear_cub_grunt_v01",
        "sfx_animal_buffalo_v01",
        "sfx_animal_chick_v01",
        "sfx_animal_chicken_v01",
        "sfx_animal_cow_v01",
        "sfx_animal_crocodile_v01",
        "sfx_animal_mika_puppy_yip_v01",
        "sfx_animal_duckling_peep_v01",
        "sfx_animal_elephant_v01",
        "sfx_animal_frog_croak_v01",
        "sfx_animal_giraffe_v01",
        "sfx_animal_goat_v01",
        "sfx_animal_gorilla_v01",
        "sfx_animal_hippo_v01",
        "sfx_animal_horse_v01",
        "sfx_animal_monkey_v01",
        "sfx_animal_moose_v01",
        "sfx_animal_narwhal_v01",
        "sfx_animal_yoru_owl_chick_hoot_v01",
        "sfx_animal_panda_cub_bleat_v01",
        "sfx_animal_parrot_v01",
        "sfx_animal_penguin_v01",
        "sfx_animal_pig_v01",
        "sfx_animal_rabbit_snuffle_v01",
        "sfx_animal_rhino_v01",
        "sfx_animal_sloth_v01",
        "sfx_animal_snake_v01",
        "sfx_animal_walrus_v01",
        "sfx_animal_whale_v01",
        "sfx_animal_zebra_v01",
        "sfx_animal_aki_fox_cub_chirp_v01",
        "sfx_animal_deer_fawn_bleat_v01",
        "sfx_animal_raccoon_chitter_v01",
        "sfx_animal_mika_kitten_mew_v01",
        "sfx_animal_aki_squirrel_chitter_v01",
        "sfx_animal_hedgehog_snuffle_v01",
        "sfx_animal_koala_joey_squeak_v01"
    };

    private static readonly string[] SetAssetNames =
    {
        null,
        "sfx_set_coral_match_v01",
        "sfx_set_astral_match_v01",
        "sfx_set_garden_match_v01",
        "sfx_set_sweets_match_v01",
        "sfx_set_clockwork_match_v01"
    };

    private static readonly string[] GuideSkillAssetNames =
    {
        "sfx_skill_aki_rewind_v01",
        "sfx_skill_mika_shield_v01",
        "sfx_skill_yoru_vision_v01",
        "sfx_skill_hana_bloom_v01",
        "sfx_skill_momo_encore_v01"
    };

    private static MementoMatchSfx instance;
    private static bool isQuitting;

    private AudioSource source;
    private AudioClip friendlyLaunch;
    private AudioClip hostileLaunch;
    private AudioClip rivalImpact;
    private AudioClip playerDamage;
    private AudioClip turnToOpponent;
    private AudioClip turnToPlayer;
    private AudioClip generatedCardFlip;
    private AudioClip generatedMatchSuccess;
    private AudioClip generatedMatchFail;
    private AudioClip generatedComboRise;
    private AudioClip generatedHealthHit;
    private AudioClip generatedSkillCutIn;
    private AudioClip generatedDuelStart;
    private AudioClip generatedBoardDeal;
    private AudioClip generatedVictory;
    private AudioClip generatedDefeat;
    private AudioClip generatedDraw;
    private AudioClip generatedUiConfirm;
    private AudioClip generatedUiBack;
    private AudioClip generatedUiLocked;
    private AudioClip generatedGuideUnlock;
    private AudioClip generatedAchievementPop;
    private AudioClip generatedCooldownReady;
    private AudioClip[] generatedGuideSkills;
    private AudioClip[] generatedAnimalMatches;
    private AudioClip[] themedSetMatches;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        isQuitting = false;
    }

    public static string GetAnimalMatchResourcePath(string pairAssetName)
    {
        int index = FindAnimalIndex(pairAssetName);
        return index >= 0
            ? GeneratedSfxRoot + AnimalAssetNames[index]
            : null;
    }

    public static string GetSetMatchResourcePath(int setId)
    {
        return setId > AnimalSetId && setId < SetAssetNames.Length
            ? GeneratedSfxRoot + SetAssetNames[setId]
            : null;
    }

    public static string GetGuideSkillResourcePath(int guideId)
    {
        return guideId >= 0 && guideId < GuideSkillAssetNames.Length
            ? GeneratedSfxRoot + GuideSkillAssetNames[guideId]
            : null;
    }

    public static void PlayDuelStart()
    {
        EnsureInstance()?.Play(EnsureInstance()?.generatedDuelStart, 0.34f);
    }

    public static void PlayBoardDeal()
    {
        EnsureInstance()?.Play(EnsureInstance()?.generatedBoardDeal, 0.25f);
    }

    public static void PlayDuelLaunch(bool hostile)
    {
        MementoMatchSfx sfx = EnsureInstance();
        sfx?.Play(hostile ? sfx.hostileLaunch : sfx.friendlyLaunch, 0.32f);
    }

    public static void PlayDuelImpact(bool playerDamaged)
    {
        MementoMatchSfx sfx = EnsureInstance();
        if (sfx == null)
            return;

        sfx.Play(sfx.generatedHealthHit, 0.42f);
        sfx.Play(playerDamaged ? sfx.playerDamage : sfx.rivalImpact, 0.22f);
    }

    public static void PlayTurnSwap(bool opponentTurn)
    {
        MementoMatchSfx sfx = EnsureInstance();
        sfx?.Play(opponentTurn ? sfx.turnToOpponent : sfx.turnToPlayer, 0.24f);
    }

    public static void PlayPower(int guideId, bool hostile)
    {
        MementoMatchSfx sfx = EnsureInstance();
        if (sfx == null)
            return;

        if (!hostile)
            sfx.Play(sfx.generatedSkillCutIn, 0.34f);
        if (guideId >= 0 && guideId < sfx.generatedGuideSkills.Length)
        {
            sfx.Play(
                sfx.generatedGuideSkills[guideId],
                hostile ? 0.28f : 0.32f);
        }
    }

    public static void PlayPower(bool hostile)
    {
        MementoMatchSfx sfx = EnsureInstance();
        sfx?.Play(sfx.generatedSkillCutIn, hostile ? 0.38f : 0.34f);
    }

    public static void PlayCardFlip()
    {
        MementoMatchSfx sfx = EnsureInstance();
        sfx?.Play(sfx.generatedCardFlip, 0.2f);
    }

    public static void PlaySetMatch(int setId, string pairAssetName)
    {
        MementoMatchSfx sfx = EnsureInstance();
        if (sfx == null)
            return;

        sfx.Play(sfx.generatedMatchSuccess, 0.3f);
        AudioClip accent = setId == AnimalSetId
            ? sfx.SelectAnimalMatch(pairAssetName)
            : sfx.SelectSetMatch(setId);
        sfx.Play(accent, 0.27f);
    }

    public static void PlayComboPulse(int combo)
    {
        if (combo != 2 && combo != 3 && combo != 5 &&
            combo != 7 && combo != 10)
        {
            return;
        }

        MementoMatchSfx sfx = EnsureInstance();
        sfx?.Play(
            sfx.generatedComboRise,
            Mathf.Clamp(0.14f + (combo * 0.012f), 0.14f, 0.28f));
    }

    public static void PlayMatchFail()
    {
        MementoMatchSfx sfx = EnsureInstance();
        sfx?.Play(sfx.generatedMatchFail, 0.24f);
    }

    public static void PlayResult(MementoMatchResultSfx result)
    {
        MementoMatchSfx sfx = EnsureInstance();
        if (sfx == null)
            return;

        AudioClip clip = result == MementoMatchResultSfx.Victory
            ? sfx.generatedVictory
            : result == MementoMatchResultSfx.Draw
                ? sfx.generatedDraw
                : sfx.generatedDefeat;
        sfx.Play(clip, 0.4f);
    }

    public static void PlayUiConfirm() => EnsureInstance()?.Play(
        EnsureInstance()?.generatedUiConfirm, 0.25f);

    public static void PlayUiBack() => EnsureInstance()?.Play(
        EnsureInstance()?.generatedUiBack, 0.23f);

    public static void PlayUiLocked() => EnsureInstance()?.Play(
        EnsureInstance()?.generatedUiLocked, 0.28f);

    public static void PlayGuideUnlock() => EnsureInstance()?.Play(
        EnsureInstance()?.generatedGuideUnlock, 0.38f);

    public static void PlayAchievementPop() => EnsureInstance()?.Play(
        EnsureInstance()?.generatedAchievementPop, 0.34f);

    public static void PlayCooldownReady() => EnsureInstance()?.Play(
        EnsureInstance()?.generatedCooldownReady, 0.3f);

    private static MementoMatchSfx EnsureInstance()
    {
        if (isQuitting || !Application.isPlaying)
            return null;
        if (instance != null)
            return instance;

        GameObject root = new GameObject("[Memento Match SFX]");
        DontDestroyOnLoad(root);
        instance = root.AddComponent<MementoMatchSfx>();
        instance.Initialize();
        return instance;
    }

    private void Initialize()
    {
        source = gameObject.AddComponent<AudioSource>();
        source.playOnAwake = false;
        source.loop = false;
        source.spatialBlend = 0f;
        source.priority = 64;

        friendlyLaunch = LoadGeneratedOrFallback(
            "sfx_duel_launch_friendly_v01",
            BuildSweep("Friendly Launch", 520f, 980f, 0.18f, 0.12f, 0f));
        hostileLaunch = LoadGeneratedOrFallback(
            "sfx_duel_launch_hostile_v01",
            BuildSweep("Hostile Launch", 330f, 190f, 0.22f, 0.16f, 0.08f));
        rivalImpact = LoadGeneratedOrFallback(
            "sfx_duel_rival_impact_v01",
            BuildSweep("Rival Impact", 150f, 72f, 0.2f, 0.22f, 0.22f));
        playerDamage = LoadGeneratedOrFallback(
            "sfx_duel_player_damage_v01",
            BuildSweep("Player Damage", 118f, 48f, 0.28f, 0.28f, 0.42f));
        turnToOpponent = LoadGeneratedOrFallback(
            "sfx_turn_opponent_v01",
            BuildSweep("Turn Opponent", 440f, 300f, 0.12f, 0.1f, 0f));
        turnToPlayer = LoadGeneratedOrFallback(
            "sfx_turn_player_v01",
            BuildSweep("Turn Player", 520f, 720f, 0.12f, 0.1f, 0f));

        generatedCardFlip = LoadGenerated("sfx_card_flip_v01");
        generatedMatchSuccess = LoadGenerated("sfx_match_success_v01");
        generatedMatchFail = LoadGenerated("sfx_match_fail_soft_v01");
        generatedComboRise = LoadGenerated("sfx_combo_rise_v01");
        generatedHealthHit = LoadGenerated("sfx_duel_health_hit_v01");
        generatedSkillCutIn = LoadGenerated("sfx_skill_cut_in_v01");
        generatedDuelStart = LoadGenerated("sfx_duel_start_v01");
        generatedBoardDeal = LoadGenerated("sfx_board_deal_v01");
        generatedVictory = LoadGenerated("sfx_result_victory_v01");
        generatedDefeat = LoadGenerated("sfx_result_defeat_v01");
        generatedDraw = LoadGenerated("sfx_result_draw_v01");
        generatedUiConfirm = LoadGenerated("sfx_ui_confirm_v01");
        generatedUiBack = LoadGenerated("sfx_ui_back_v01");
        generatedUiLocked = LoadGenerated("sfx_ui_locked_v01");
        generatedGuideUnlock = LoadGenerated("sfx_guide_unlock_v01");
        generatedAchievementPop = LoadGenerated("sfx_achievement_pop_v01");
        generatedCooldownReady = LoadGenerated("sfx_cooldown_ready_v01");

        generatedGuideSkills = new AudioClip[GuideSkillAssetNames.Length];
        for (int i = 0; i < generatedGuideSkills.Length; i++)
            generatedGuideSkills[i] = LoadGenerated(GuideSkillAssetNames[i]);

        generatedAnimalMatches = new AudioClip[AnimalAssetNames.Length];
        for (int i = 0; i < generatedAnimalMatches.Length; i++)
            generatedAnimalMatches[i] = LoadGenerated(AnimalAssetNames[i]);

        themedSetMatches = new AudioClip[SetAssetNames.Length];
        for (int i = 1; i < themedSetMatches.Length; i++)
            themedSetMatches[i] = LoadGenerated(SetAssetNames[i]);
    }

    private AudioClip SelectAnimalMatch(string pairAssetName)
    {
        int index = FindAnimalIndex(pairAssetName);
        return index >= 0 && index < generatedAnimalMatches.Length
            ? generatedAnimalMatches[index]
            : null;
    }

    private AudioClip SelectSetMatch(int setId)
    {
        return themedSetMatches != null &&
               setId > AnimalSetId &&
               setId < themedSetMatches.Length
            ? themedSetMatches[setId]
            : null;
    }

    private static int FindAnimalIndex(string pairAssetName)
    {
        if (string.IsNullOrWhiteSpace(pairAssetName))
            return -1;

        string normalized = pairAssetName
            .Replace('\\', '/')
            .ToLowerInvariant();
        for (int i = 0; i < AnimalKeys.Length; i++)
        {
            if (ContainsIdentity(normalized, AnimalKeys[i]))
                return i;
        }

        return -1;
    }

    private static bool ContainsIdentity(string value, string identity)
    {
        int start = 0;
        while (start < value.Length)
        {
            int match = value.IndexOf(identity, start, StringComparison.Ordinal);
            if (match < 0)
                return false;

            int end = match + identity.Length;
            bool startsAtBoundary =
                match == 0 || !char.IsLetterOrDigit(value[match - 1]);
            bool endsAtBoundary =
                end == value.Length || !char.IsLetterOrDigit(value[end]);
            if (startsAtBoundary && endsAtBoundary)
                return true;

            start = match + 1;
        }

        return false;
    }

    private static AudioClip LoadGenerated(string assetName)
    {
        return Resources.Load<AudioClip>(GeneratedSfxRoot + assetName);
    }

    private static AudioClip LoadGeneratedOrFallback(
        string assetName,
        AudioClip fallback)
    {
        AudioClip generated = LoadGenerated(assetName);
        return generated != null ? generated : fallback;
    }

    private void Play(AudioClip clip, float volume)
    {
        if (source != null && clip != null)
            source.PlayOneShot(clip, volume * MementoAudioSettings.SfxVolume);
    }

    private static AudioClip BuildSweep(
        string clipName,
        float startFrequency,
        float endFrequency,
        float duration,
        float harmonicMix,
        float noiseMix)
    {
        int sampleCount = Mathf.Max(1, Mathf.CeilToInt(duration * SampleRate));
        float[] samples = new float[sampleCount];
        float phase = 0f;
        uint noiseState = 0xA341316Cu;

        for (int i = 0; i < sampleCount; i++)
        {
            float normalized = sampleCount > 1 ? i / (sampleCount - 1f) : 1f;
            float frequency = Mathf.Lerp(startFrequency, endFrequency, normalized);
            phase += 2f * Mathf.PI * frequency / SampleRate;

            noiseState = (noiseState * 1664525u) + 1013904223u;
            float noise = (((noiseState >> 8) & 0xFFFFu) / 32767.5f) - 1f;
            float attack = Mathf.Clamp01(normalized / 0.045f);
            float release = 1f - Mathf.SmoothStep(0.48f, 1f, normalized);
            float envelope = attack * release;
            float fundamental = Mathf.Sin(phase);
            float harmonic = Mathf.Sin(phase * 2.01f) * harmonicMix;
            samples[i] = (fundamental + harmonic + (noise * noiseMix)) * envelope * 0.62f;
        }

        AudioClip clip = AudioClip.Create(clipName, sampleCount, 1, SampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }

    private void OnApplicationQuit()
    {
        isQuitting = true;
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }
}
