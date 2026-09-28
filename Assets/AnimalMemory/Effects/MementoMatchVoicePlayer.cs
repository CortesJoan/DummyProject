using System;
using UnityEngine;
using Random = UnityEngine.Random;

public readonly struct MementoMatchVoiceCue
{
    public int GuideId { get; }
    public string ClipName { get; }
    public string Subtitle { get; }
    public float Duration { get; }

    public MementoMatchVoiceCue(
        int guideId,
        string clipName,
        string subtitle,
        float duration)
    {
        GuideId = guideId;
        ClipName = clipName;
        Subtitle = subtitle;
        Duration = duration;
    }
}

public sealed class MementoMatchVoicePlayer : MonoBehaviour
{
    private const string ResourceRoot = "Audio/Voices/ja/";
    private const float VoiceVolume = 0.92f;
    private const int GuideCount = 6;
    private const int VariantCount = 3;

    private static MementoMatchVoicePlayer instance;
    private static AudioClip[] menuClips;
    private static AudioClip[] lockedClips;
    private static AudioClip[] skillClips;
    private static AudioClip[] victoryClips;
    private static AudioClip[] defeatClips;
    private static AudioClip[,] turnClips;
    private static AudioClip[,] damageClips;
    private static int[] lastTurnVariants;
    private static int[] lastDamageVariants;

    private AudioSource voiceSource;

    public static event Action<MementoMatchVoiceCue> LineStarted;

    /// <summary>Stops an existing voice without creating a persistent player.</summary>
    public static void StopCurrent()
    {
        if (instance != null && instance.voiceSource != null)
        {
            instance.voiceSource.Stop();
            instance.voiceSource.clip = null;
        }
    }

    public static float PlayMenu(int guideId)
    {
        if (guideId >= GuideCount) return 0f; // Unknown identities never borrow another character's recording.
        return Play(guideId, GetClip(guideId, VoiceLine.Menu), true);
    }

    public static float PlayLocked(int guideId)
    {
        if (guideId >= GuideCount) return 0f; // Unknown identities never borrow another character's recording.
        return Play(guideId, GetClip(guideId, VoiceLine.Locked), true);
    }

    public static float PlaySkill(int guideId)
    {
        if (guideId >= GuideCount) return 0f; // Unknown identities never borrow another character's recording.
        return Play(guideId, GetClip(guideId, VoiceLine.Skill), true);
    }

    public static float PlayTurn(int guideId)
    {
        if (guideId >= GuideCount) return 0f; // Unknown identities never borrow another character's recording.
        MementoMatchVoicePlayer player = GetOrCreate();
        if (player.voiceSource.isPlaying)
            return 0f;

        int guideIndex = GetGuideIndex(guideId);
        int variantIndex = ChooseVariant(
            guideIndex,
            ref lastTurnVariants);
        return Play(
            guideId,
            GetVariantClip(
                guideIndex,
                variantIndex,
                VariantVoiceLine.Turn),
            false);
    }

    public static float PlayDamage(int guideId)
    {
        if (guideId >= GuideCount) return 0f; // Unknown identities never borrow another character's recording.
        int guideIndex = GetGuideIndex(guideId);
        int variantIndex = ChooseVariant(
            guideIndex,
            ref lastDamageVariants);
        return Play(
            guideId,
            GetVariantClip(
                guideIndex,
                variantIndex,
                VariantVoiceLine.Damage),
            true);
    }

    public static float PlayOutcome(int guideId, bool playerWon)
    {
        if (guideId >= GuideCount) return 0f;
        VoiceLine line = playerWon ? VoiceLine.Defeat : VoiceLine.Victory;
        return Play(guideId, GetClip(guideId, line), true);
    }

    public static float GetOutcomeDuration(int guideId, bool playerWon)
    {
        if (guideId >= GuideCount) return 0f;
        VoiceLine line = playerWon ? VoiceLine.Defeat : VoiceLine.Victory;
        AudioClip clip = GetClip(guideId, line);
        return clip != null ? clip.length : 0f;
    }

    private static float Play(
        int guideId,
        AudioClip clip,
        bool interruptCurrentLine)
    {
        if (clip == null)
            return 0f;

        MementoMatchVoicePlayer player = GetOrCreate();
        if (!interruptCurrentLine && player.voiceSource.isPlaying)
            return 0f;

        player.voiceSource.Stop();
        player.ApplyVolume();
        player.voiceSource.clip = clip;
        player.voiceSource.Play();
        LineStarted?.Invoke(new MementoMatchVoiceCue(
            GetGuideIndex(guideId),
            clip.name,
            GetSubtitleForClip(clip.name),
            clip.length));
        return clip.length;
    }

    private static MementoMatchVoicePlayer GetOrCreate()
    {
        if (instance != null)
            return instance;

        GameObject host = new GameObject("Memento Match Voice Player");
        DontDestroyOnLoad(host);
        instance = host.AddComponent<MementoMatchVoicePlayer>();
        return instance;
    }

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        voiceSource = gameObject.AddComponent<AudioSource>();
        voiceSource.playOnAwake = false;
        voiceSource.loop = false;
        voiceSource.spatialBlend = 0f;
        voiceSource.priority = 32;
        ApplyVolume();
        MementoAudioSettings.Changed += ApplyVolume;
    }

    private void ApplyVolume()
    {
        if (voiceSource != null)
            voiceSource.volume = VoiceVolume * MementoAudioSettings.VoiceVolume;
    }

    private void OnDestroy()
    {
        MementoAudioSettings.Changed -= ApplyVolume;
        if (instance == this)
            instance = null;
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetStatics()
    {
        instance = null;
        menuClips = null;
        lockedClips = null;
        skillClips = null;
        victoryClips = null;
        defeatClips = null;
        turnClips = null;
        damageClips = null;
        lastTurnVariants = null;
        lastDamageVariants = null;
        LineStarted = null;
    }

    public static string GetOutcomeSubtitle(int guideId, bool playerWon)
    {
        if (guideId >= GuideCount) return string.Empty;
        int guideIndex = GetGuideIndex(guideId);
        string suffix = playerWon ? "_defeat" : "_victory";
        return GetSubtitleForClip(GetGuideName(guideIndex) + suffix);
    }

    private static AudioClip GetClip(int guideId, VoiceLine line)
    {
        int index = GetGuideIndex(guideId);
        AudioClip[] clips = GetClipArray(line);
        if (clips[index] == null)
            clips[index] = Resources.Load<AudioClip>(GetResourcePath(index, line));

        if (clips[index] == null)
            Debug.LogWarning(
                $"Japanese voice clip is missing for guide {guideId}, line {line}.");

        return clips[index];
    }

    private static AudioClip GetVariantClip(
        int guideIndex,
        int variantIndex,
        VariantVoiceLine line)
    {
        AudioClip[,] clips = GetVariantClipArray(line);
        if (clips[guideIndex, variantIndex] == null)
        {
            string guideName = GetGuideName(guideIndex);
            string lineName = line == VariantVoiceLine.Turn
                ? "turn"
                : "damage";
            string path =
                $"{ResourceRoot}{guideName}_{lineName}_{variantIndex + 1}";
            clips[guideIndex, variantIndex] =
                Resources.Load<AudioClip>(path);
        }

        if (clips[guideIndex, variantIndex] == null)
        {
            Debug.LogWarning(
                $"Japanese voice variant is missing for guide {guideIndex}, " +
                $"line {line}, variant {variantIndex + 1}.");
        }

        return clips[guideIndex, variantIndex];
    }

    private static AudioClip[] GetClipArray(VoiceLine line)
    {
        switch (line)
        {
            case VoiceLine.Menu:
                return menuClips ?? (menuClips = new AudioClip[GuideCount]);
            case VoiceLine.Locked:
                return lockedClips ?? (lockedClips = new AudioClip[GuideCount]);
            case VoiceLine.Skill:
                return skillClips ?? (skillClips = new AudioClip[GuideCount]);
            case VoiceLine.Victory:
                return victoryClips ?? (victoryClips = new AudioClip[GuideCount]);
            default:
                return defeatClips ?? (defeatClips = new AudioClip[GuideCount]);
        }
    }

    private static AudioClip[,] GetVariantClipArray(VariantVoiceLine line)
    {
        if (line == VariantVoiceLine.Turn)
        {
            return turnClips ??
                (turnClips = new AudioClip[GuideCount, VariantCount]);
        }

        return damageClips ??
            (damageClips = new AudioClip[GuideCount, VariantCount]);
    }

    private static int ChooseVariant(
        int guideIndex,
        ref int[] variantHistory)
    {
        if (variantHistory == null)
        {
            variantHistory = new int[GuideCount];
            for (int i = 0; i < variantHistory.Length; i++)
                variantHistory[i] = -1;
        }

        int previous = variantHistory[guideIndex];
        int next;
        if (previous < 0)
        {
            next = Random.Range(0, VariantCount);
        }
        else
        {
            next = Random.Range(0, VariantCount - 1);
            if (next >= previous)
                next++;
        }

        variantHistory[guideIndex] = next;
        return next;
    }

    private static int GetGuideIndex(int guideId)
    {
        return Mathf.Clamp(guideId, 0, GuideCount - 1);
    }

    private static string GetGuideName(int guideIndex)
    {
        if (guideIndex == 1)
            return "mika";
        if (guideIndex == 2)
            return "yoru";
        if (guideIndex == 3)
            return "hana";
        if (guideIndex == 4)
            return "momo";
        if (guideIndex == 5)
            return "rei";
        return "aki";
    }

    private static string GetResourcePath(int guideIndex, VoiceLine line)
    {
        if (line == VoiceLine.Locked && guideIndex >= 0 && guideIndex < 5)
            return ResourceRoot + "LockedV3/" + GetGuideName(guideIndex) + "_locked_v3";
        string lineName;
        switch (line)
        {
            case VoiceLine.Menu:
                lineName = "menu";
                break;
            case VoiceLine.Locked:
                lineName = "locked";
                break;
            case VoiceLine.Skill:
                lineName = "skill";
                break;
            case VoiceLine.Victory:
                lineName = "victory";
                break;
            default:
                lineName = "defeat";
                break;
        }

        return ResourceRoot + GetGuideName(guideIndex) + "_" + lineName;
    }

    public static string GetSubtitleForClip(string clipName)
    {
        switch (clipName)
        {
            case "aki_locked_v3": return "Vénceme en el Bosque Vivo y podremos formar una alianza.";
            case "mika_locked_v3": return "Vence mi defensa en el Taller Coral. Después hablaremos de una alianza.";
            case "yoru_locked_v3": return "Encuéntrame en el Cielo de Tinta y supera mi duelo.";
            case "hana_locked_v3": return "Gana mi duelo en el Jardín Errante y caminaremos juntas.";
            case "momo_locked_v3": return "¡Primero gánale a Momo en el Dulce Atelier!";
            case "aki_menu": return "Si te equivocas, respira. Siempre podemos intentarlo de nuevo.";
            case "aki_locked": return "Aún no. Reúne más estrellas y vuelve conmigo.";
            case "aki_skill": return "¡Deshacer! Volvamos un paso y encontraremos el camino.";
            case "aki_victory": return "Esta vez gané yo. ¿Jugamos otra?";
            case "aki_defeat": return "Qué buena memoria... Esta vez me has superado.";
            case "aki_turn_1": return "Veamos... escogeré esta carta.";
            case "aki_turn_2": return "Recuerdo haber visto algo por aquí.";
            case "aki_turn_3": return "Con calma. El bosque siempre deja una pista.";
            case "aki_damage_1": return "¡Itai! Esa pareja me alcanzó.";
            case "aki_damage_2": return "¡Uuuh! Buena combinación.";
            case "aki_damage_3": return "Ay... eso sí que dolió.";

            case "mika_menu": return "La improvisación sirve, pero un buen plan sirve más.";
            case "mika_locked": return "Todavía no cumples las condiciones. Vuelve mejor preparado.";
            case "mika_skill": return "Escudo de combo. Protocolo defensivo activado.";
            case "mika_victory": return "Resultado confirmado: gané. Revancha cuando quieras.";
            case "mika_defeat": return "Cálculo incorrecto... Admito tu victoria.";
            case "mika_turn_1": return "Evaluando probabilidades. Elijo esta.";
            case "mika_turn_2": return "He reducido las opciones.";
            case "mika_turn_3": return "No necesito verlo todo, sólo recordar lo suficiente.";
            case "mika_damage_1": return "¡Itai...! Impacto confirmado.";
            case "mika_damage_2": return "Tch... esa pareja rompió mi defensa.";
            case "mika_damage_3": return "Daño recibido. Recalculando.";

            case "yoru_menu": return "Las estrellas guardan cada recuerdo, incluso los olvidados.";
            case "yoru_locked": return "El cielo aún no te reconoce. Reúne más luz.";
            case "yoru_skill": return "Visión estelar. La pareja correcta ya brilla.";
            case "yoru_victory": return "Esta noche, las estrellas me eligieron a mí.";
            case "yoru_defeat": return "Has unido las estrellas... Esta constelación es tuya.";
            case "yoru_turn_1": return "Una estrella llama a otra.";
            case "yoru_turn_2": return "La respuesta duerme bajo esta carta.";
            case "yoru_turn_3": return "Seguiré el hilo de luz.";
            case "yoru_damage_1": return "¡Itai...! Mi constelación se rompe.";
            case "yoru_damage_2": return "Ah... esa luz ha dado en el blanco.";
            case "yoru_damage_3": return "Uu... una estrella menos.";

            case "hana_menu": return "Las flores recuerdan a quienes las hacen florecer.";
            case "hana_locked": return "Todavía soy un capullo. Reúne estrellas y vuelve.";
            case "hana_skill": return "¡Floración! Florece, recuerdo oculto.";
            case "hana_victory": return "¡Esta ronda ha florecido para mí!";
            case "hana_defeat": return "Has hecho florecer el tablero. El jardín es tuyo.";
            case "hana_turn_1": return "¿Estará debajo de esta hoja?";
            case "hana_turn_2": return "El viento me ha dado una pista.";
            case "hana_turn_3": return "Esta vez no me perderé.";
            case "hana_damage_1": return "¡Itai! Mis pétalos.";
            case "hana_damage_2": return "Vaya, buena pareja.";
            case "hana_damage_3": return "Aún no voy a marchitarme.";

            case "momo_menu": return "Un recuerdo dulce puede animarte de un solo bocado.";
            case "momo_locked": return "Nada de probar aún. Reúne estrellas primero.";
            case "momo_skill": return "¡Encore dulce! Una vez más, sigue siendo mi turno.";
            case "momo_victory": return "¡Gana Momo! ¡El premio es mío!";
            case "momo_defeat": return "Una pareja perfecta. He perdido, pero se ve deliciosa.";
            case "momo_turn_1": return "Esta carta huele dulce.";
            case "momo_turn_2": return "El siguiente bocado está aquí.";
            case "momo_turn_3": return "Si sigo la receta, debe ser esta.";
            case "momo_damage_1": return "¡Itai! Se rompió el caramelo.";
            case "momo_damage_2": return "Vaya, esa pareja es fuerte.";
            case "momo_damage_3": return "La crema se está cayendo.";

            case "rei_menu": return "El Archivo Cero registra cada decisión.";
            case "rei_locked": return "Reúne a las cinco guardianas antes del examen final.";
            case "rei_skill": return "Técnica registrada. Ejecutando siguiente protocolo.";
            case "rei_victory": return "La alianza aún no actúa como una sola. Corrige y vuelve.";
            case "rei_defeat": return "Examen aprobado. Vuestro deseo pertenece a las seis.";
            case "rei_turn_1": return "Registraré esta posibilidad.";
            case "rei_turn_2": return "El Archivo recuerda el patrón.";
            case "rei_turn_3": return "Quedan menos variables.";
            case "rei_damage_1": return "Impacto confirmado.";
            case "rei_damage_2": return "Buena coordinación.";
            case "rei_damage_3": return "La alianza supera la previsión.";
            default: return string.Empty;
        }
    }

    private enum VoiceLine
    {
        Menu,
        Locked,
        Skill,
        Victory,
        Defeat
    }

    private enum VariantVoiceLine
    {
        Turn,
        Damage
    }
}
