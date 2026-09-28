using NUnit.Framework;

public sealed class MementoMatchAudioCatalogTests
{
    [TestCase(0, "Audio/Music/Duels/duel_aki")]
    [TestCase(1, "Audio/Music/Duels/duel_mika")]
    [TestCase(2, "Audio/Music/Duels/duel_yoru")]
    [TestCase(3, "Audio/Music/Duels/duel_hana")]
    [TestCase(4, "Audio/Music/Duels/duel_momo")]
    [TestCase(5, "Audio/Music/Duels/duel_rei")]
    [TestCase(6, "Audio/Music/Duels/duel_custodia_calculo")]
    [TestCase(7, "Audio/Music/Duels/duel_custodia_azar")]
    [TestCase(8, "Audio/Music/Duels/duel_custodia_vinculo")]
    [TestCase(9, "Audio/Music/Duels/duel_custodia_orgullo")]
    [TestCase(10, "Audio/Music/Duels/duel_creador")]
    public void DuelMusicPath_IsStableForEveryGuide(
        int guideId,
        string expected)
    {
        Assert.That(
            AudioManager.GetDuelMusicResourcePath(guideId),
            Is.EqualTo(expected));
    }

    [TestCase(-1)]
    [TestCase(11)]
    [TestCase(int.MaxValue)]
    public void DuelMusicPath_RejectsUnknownGuide(int guideId)
    {
        Assert.That(
            AudioManager.GetDuelMusicResourcePath(guideId),
            Is.Null);
    }

    [TestCase(6, "custodia_calculo")]
    [TestCase(7, "custodia_azar")]
    [TestCase(8, "custodia_vinculo")]
    [TestCase(9, "custodia_orgullo")]
    [TestCase(10, "creador")]
    public void PostgameDuelMusic_NeverBorrowsAGuardianTheme(int guideId, string expectedKey)
    {
        string path = AudioManager.GetDuelMusicResourcePath(guideId);
        Assert.That(path, Is.EqualTo("Audio/Music/Duels/duel_" + expectedKey));

        // El handoff exige que angeles y GOD nunca hereden clips de guardianas.
        for (int guardian = 0; guardian <= 5; guardian++)
            Assert.That(
                path,
                Is.Not.EqualTo(AudioManager.GetDuelMusicResourcePath(guardian)),
                "El oponente " + guideId + " no puede reutilizar el tema de la guardiana " + guardian);

        // Las cinco pistas postgame aun no estan producidas. Lo que se fija aqui es que
        // pedirlas no revienta: PlayDuelMusic degrada a silencio con aviso en vez de
        // dejar sonando el tema del rival anterior.
        Assert.That(() => AudioManager.PlayDuelMusic(guideId), Throws.Nothing);
    }

    [TestCase("forest_red_panda_front", "sfx_animal_red_panda_squeak_v01")]
    [TestCase("forest_fox_front", "sfx_animal_aki_fox_cub_chirp_v01")]
    [TestCase("forest_rabbit_front", "sfx_animal_rabbit_snuffle_v01")]
    [TestCase("forest_owl_front", "sfx_animal_yoru_owl_chick_hoot_v01")]
    [TestCase("forest_bear_front", "sfx_animal_bear_cub_grunt_v01")]
    [TestCase("forest_deer_front", "sfx_animal_deer_fawn_bleat_v01")]
    [TestCase("forest_raccoon_front", "sfx_animal_raccoon_chitter_v01")]
    [TestCase("forest_cat_front", "sfx_animal_mika_kitten_mew_v01")]
    [TestCase("forest_dog_front", "sfx_animal_mika_puppy_yip_v01")]
    [TestCase("forest_squirrel_front", "sfx_animal_aki_squirrel_chitter_v01")]
    [TestCase("forest_hedgehog_front", "sfx_animal_hedgehog_snuffle_v01")]
    [TestCase("forest_frog_front", "sfx_animal_frog_croak_v01")]
    [TestCase("forest_duck_front", "sfx_animal_duckling_peep_v01")]
    [TestCase("forest_koala_front", "sfx_animal_koala_joey_squeak_v01")]
    [TestCase("forest_panda_front", "sfx_animal_panda_cub_bleat_v01")]
    [TestCase("bear.png", "sfx_animal_bear_cub_grunt_v01")]
    [TestCase("buffalo.png", "sfx_animal_buffalo_v01")]
    [TestCase("chick.png", "sfx_animal_chick_v01")]
    [TestCase("chicken.png", "sfx_animal_chicken_v01")]
    [TestCase("cow.png", "sfx_animal_cow_v01")]
    [TestCase("crocodile.png", "sfx_animal_crocodile_v01")]
    [TestCase("dog.png", "sfx_animal_mika_puppy_yip_v01")]
    [TestCase("duck.png", "sfx_animal_duckling_peep_v01")]
    [TestCase("elephant.png", "sfx_animal_elephant_v01")]
    [TestCase("frog.png", "sfx_animal_frog_croak_v01")]
    [TestCase("giraffe.png", "sfx_animal_giraffe_v01")]
    [TestCase("goat.png", "sfx_animal_goat_v01")]
    [TestCase("gorilla.png", "sfx_animal_gorilla_v01")]
    [TestCase("hippo.png", "sfx_animal_hippo_v01")]
    [TestCase("horse.png", "sfx_animal_horse_v01")]
    [TestCase("monkey.png", "sfx_animal_monkey_v01")]
    [TestCase("moose.png", "sfx_animal_moose_v01")]
    [TestCase("narwhal.png", "sfx_animal_narwhal_v01")]
    [TestCase("owl.png", "sfx_animal_yoru_owl_chick_hoot_v01")]
    [TestCase("panda.png", "sfx_animal_panda_cub_bleat_v01")]
    [TestCase("parrot.png", "sfx_animal_parrot_v01")]
    [TestCase("penguin.png", "sfx_animal_penguin_v01")]
    [TestCase("pig.png", "sfx_animal_pig_v01")]
    [TestCase("rabbit.png", "sfx_animal_rabbit_snuffle_v01")]
    [TestCase("rhino.png", "sfx_animal_rhino_v01")]
    [TestCase("sloth.png", "sfx_animal_sloth_v01")]
    [TestCase("snake.png", "sfx_animal_snake_v01")]
    [TestCase("walrus.png", "sfx_animal_walrus_v01")]
    [TestCase("whale.png", "sfx_animal_whale_v01")]
    [TestCase("zebra.png", "sfx_animal_zebra_v01")]
    public void AnimalPath_UsesTheSemanticIdentity(
        string pairAssetName,
        string expectedAssetName)
    {
        Assert.That(
            MementoMatchSfx.GetAnimalMatchResourcePath(pairAssetName),
            Is.EqualTo("Audio/Sfx/StableAudio3/" + expectedAssetName));
    }

    [TestCase(null)]
    [TestCase("")]
    [TestCase("forest_wolf_front")]
    [TestCase("unknown_creature")]
    public void AnimalPath_HasNoIncorrectFallback(string pairAssetName)
    {
        Assert.That(
            MementoMatchSfx.GetAnimalMatchResourcePath(pairAssetName),
            Is.Null);
    }

    [TestCase(1, "coral")]
    [TestCase(2, "astral")]
    [TestCase(3, "garden")]
    [TestCase(4, "sweets")]
    [TestCase(5, "clockwork")]
    public void SetAccentPath_IsStable(int setId, string key)
    {
        Assert.That(
            MementoMatchSfx.GetSetMatchResourcePath(setId),
            Is.EqualTo(
                $"Audio/Sfx/StableAudio3/sfx_set_{key}_match_v01"));
    }

    [TestCase(0, "aki_rewind")]
    [TestCase(1, "mika_shield")]
    [TestCase(2, "yoru_vision")]
    [TestCase(3, "hana_bloom")]
    [TestCase(4, "momo_encore")]
    public void GuideSkillPath_IsSpecific(int guideId, string key)
    {
        Assert.That(
            MementoMatchSfx.GetGuideSkillResourcePath(guideId),
            Is.EqualTo(
                $"Audio/Sfx/StableAudio3/sfx_skill_{key}_v01"));
    }


    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    public void DuelMusicAsset_IsRuntimeReady(int guideId)
    {
        string path = AudioManager.GetDuelMusicResourcePath(guideId);
        UnityEngine.AudioClip clip =
            UnityEngine.Resources.Load<UnityEngine.AudioClip>(path);

        Assert.That(clip, Is.Not.Null, path);
        Assert.That(clip.frequency, Is.EqualTo(48000), path);
        Assert.That(clip.channels, Is.EqualTo(2), path);
        Assert.That(clip.length, Is.GreaterThan(5f), path);
    }

    [TestCase(0, "Esta vez gané yo. ¿Jugamos otra?")]
    [TestCase(1, "Resultado confirmado: gané. Revancha cuando quieras.")]
    [TestCase(2, "Esta noche, las estrellas me eligieron a mí.")]
    [TestCase(3, "¡Esta ronda ha florecido para mí!")]
    [TestCase(4, "¡Gana Momo! ¡El premio es mío!")]
    public void EnemyVictorySubtitle_HasOneCanonicalSource(
        int guideId,
        string expected)
    {
        Assert.That(
            MementoMatchVoicePlayer.GetOutcomeSubtitle(
                guideId,
                false),
            Is.EqualTo(expected));
    }
}
