using NUnit.Framework;
using UnityEngine;

public sealed class StableAudio3SfxAssetTests
{
    private const string Root = "Audio/Sfx/StableAudio3/";

    [TestCase("sfx_animal_aki_fox_cub_chirp_v01", 1)]
    [TestCase("sfx_animal_aki_squirrel_chitter_v01", 1)]
    [TestCase("sfx_animal_mika_puppy_yip_v01", 1)]
    [TestCase("sfx_animal_mika_kitten_mew_v01", 1)]
    [TestCase("sfx_animal_yoru_owl_chick_hoot_v01", 1)]
    [TestCase("sfx_animal_yoru_wolf_pup_howl_v01", 1)]
    [TestCase("sfx_card_flip_v01", 1)]
    [TestCase("sfx_match_success_v01", 1)]
    [TestCase("sfx_match_fail_soft_v01", 1)]
    [TestCase("sfx_combo_rise_v01", 1)]
    [TestCase("sfx_duel_health_hit_v01", 1)]
    [TestCase("sfx_skill_cut_in_v01", 2)]
    public void GeneratedClip_IsRuntimeReady(string assetName, int expectedChannels)
    {
        AudioClip clip = Resources.Load<AudioClip>(Root + assetName);

        Assert.That(clip, Is.Not.Null, assetName);
        Assert.That(clip.frequency, Is.EqualTo(48000), assetName);
        Assert.That(clip.channels, Is.EqualTo(expectedChannels), assetName);
        Assert.That(clip.length, Is.GreaterThan(0.08f), assetName);
    }

    [TestCase("sfx_animal_red_panda_squeak_v01")]
    [TestCase("sfx_animal_rabbit_snuffle_v01")]
    [TestCase("sfx_animal_bear_cub_grunt_v01")]
    [TestCase("sfx_animal_deer_fawn_bleat_v01")]
    [TestCase("sfx_animal_raccoon_chitter_v01")]
    [TestCase("sfx_animal_hedgehog_snuffle_v01")]
    [TestCase("sfx_animal_frog_croak_v01")]
    [TestCase("sfx_animal_duckling_peep_v01")]
    [TestCase("sfx_animal_koala_joey_squeak_v01")]
    [TestCase("sfx_animal_panda_cub_bleat_v01")]
    [TestCase("sfx_set_coral_match_v01")]
    [TestCase("sfx_set_astral_match_v01")]
    [TestCase("sfx_set_garden_match_v01")]
    [TestCase("sfx_set_sweets_match_v01")]
    [TestCase("sfx_set_clockwork_match_v01")]
    [TestCase("sfx_result_victory_v01")]
    [TestCase("sfx_result_defeat_v01")]
    [TestCase("sfx_result_draw_v01")]
    [TestCase("sfx_skill_aki_rewind_v01")]
    [TestCase("sfx_skill_mika_shield_v01")]
    [TestCase("sfx_skill_yoru_vision_v01")]
    [TestCase("sfx_skill_hana_bloom_v01")]
    [TestCase("sfx_skill_momo_encore_v01")]
    [TestCase("sfx_ui_confirm_v01")]
    [TestCase("sfx_ui_back_v01")]
    [TestCase("sfx_ui_locked_v01")]
    [TestCase("sfx_duel_start_v01")]
    [TestCase("sfx_board_deal_v01")]
    [TestCase("sfx_guide_unlock_v01")]
    [TestCase("sfx_achievement_pop_v01")]
    [TestCase("sfx_cooldown_ready_v01")]
    [TestCase("sfx_duel_launch_friendly_v01")]
    [TestCase("sfx_duel_launch_hostile_v01")]
    [TestCase("sfx_duel_rival_impact_v01")]
    [TestCase("sfx_duel_player_damage_v01")]
    [TestCase("sfx_turn_opponent_v01")]
    [TestCase("sfx_turn_player_v01")]
    [TestCase("sfx_animal_buffalo_v01")]
    [TestCase("sfx_animal_chick_v01")]
    [TestCase("sfx_animal_chicken_v01")]
    [TestCase("sfx_animal_cow_v01")]
    [TestCase("sfx_animal_crocodile_v01")]
    [TestCase("sfx_animal_elephant_v01")]
    [TestCase("sfx_animal_giraffe_v01")]
    [TestCase("sfx_animal_goat_v01")]
    [TestCase("sfx_animal_gorilla_v01")]
    [TestCase("sfx_animal_hippo_v01")]
    [TestCase("sfx_animal_horse_v01")]
    [TestCase("sfx_animal_monkey_v01")]
    [TestCase("sfx_animal_moose_v01")]
    [TestCase("sfx_animal_narwhal_v01")]
    [TestCase("sfx_animal_parrot_v01")]
    [TestCase("sfx_animal_penguin_v01")]
    [TestCase("sfx_animal_pig_v01")]
    [TestCase("sfx_animal_rhino_v01")]
    [TestCase("sfx_animal_sloth_v01")]
    [TestCase("sfx_animal_snake_v01")]
    [TestCase("sfx_animal_walrus_v01")]
    [TestCase("sfx_animal_whale_v01")]
    [TestCase("sfx_animal_zebra_v01")]
    public void ExtendedGeneratedClip_IsRuntimeReady(string assetName)
    {
        AudioClip clip = Resources.Load<AudioClip>(Root + assetName);

        Assert.That(clip, Is.Not.Null, assetName);
        Assert.That(clip.frequency, Is.EqualTo(48000), assetName);
        Assert.That(clip.channels, Is.EqualTo(1).Or.EqualTo(2), assetName);
        Assert.That(clip.length, Is.GreaterThan(0.05f), assetName);
    }
}
