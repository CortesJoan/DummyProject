using System;
using System.Collections.Generic;

namespace AnimalMemory.Progression
{
    /// <summary>
    /// Stable semantic identities for every card face in each collection.
    /// Pair construction refers to the index in these lists, so one identity
    /// always maps to exactly one pair and never aliases another face.
    /// </summary>
    public static class AnimalMemoryCardIdentityCatalog
    {
        public const int IdentitiesPerSet = 15;

        private static readonly string[][] IdentityKeys =
        {
            new[]
            {
                "red_panda", "fox", "rabbit", "owl", "bear",
                "deer", "raccoon", "cat", "dog", "squirrel",
                "hedgehog", "frog", "duck", "koala", "panda"
            },
            new[]
            {
                "nautilus", "pearl_oyster", "coral_branch", "message_bottle", "diving_helmet",
                "seahorse", "compass", "anchor", "lantern_jellyfish", "sunken_key",
                "turtle_token", "starfish", "scallop_shell", "moon_wave", "treasure_chest"
            },
            new[]
            {
                "spiral_galaxy", "crescent_moon", "star_cluster", "comet", "ringed_planet",
                "orion", "celestial_compass", "rocket", "aurora", "shooting_star",
                "ursa_major", "five_star", "eclipse", "lunar_tide", "observatory"
            },
            new[]
            {
                "rose", "lotus_dewdrop", "flowering_vine", "seed_bottle", "flower_pot",
                "young_sprout", "sunflower", "watering_can", "mushroom", "garden_shears",
                "leaf_beetle", "daisy", "monstera_leaf", "pond_lily", "greenhouse"
            },
            new[]
            {
                "swirl_cake", "macaron", "candy_cane", "soda_bottle", "cupcake",
                "lollipop", "donut", "dessert_fork", "jelly", "chocolate_bar",
                "cookie", "star_candy", "croissant", "ice_cream", "celebration_cake"
            },
            new[]
            {
                "mainspring", "pendulum", "gear_train", "oil_bottle", "pocket_watch",
                "winding_key", "brass_compass", "anchor_escapement", "alarm_bell", "clock_key",
                "automaton_turtle", "star_wheel", "hourglass", "moon_dial", "clock_tower"
            }
        };

        public static IReadOnlyList<string> GetIdentityKeys(int setId)
        {
            if (setId < 0 || setId >= IdentityKeys.Length)
                throw new ArgumentOutOfRangeException(nameof(setId));

            return IdentityKeys[setId];
        }

        public static string GetIdentityKey(int setId, int pairId)
        {
            IReadOnlyList<string> identities = GetIdentityKeys(setId);
            if (pairId < 0 || pairId >= identities.Count)
                throw new ArgumentOutOfRangeException(nameof(pairId));

            return identities[pairId];
        }
    }
}
