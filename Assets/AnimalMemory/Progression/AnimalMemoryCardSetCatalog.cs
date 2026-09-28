using System.Collections.Generic;
using UnityEngine;

namespace AnimalMemory.Progression
{
    [CreateAssetMenu(fileName = "AnimalMemoryCardSetCatalog", menuName = "Animal Memory/Card Set Catalog")]
    public sealed class AnimalMemoryCardSetCatalog : ScriptableObject
    {
        [SerializeField] private Sprite[] forestSprites;
        [SerializeField] private Sprite[] coralSprites;
        [SerializeField] private Sprite[] constellationSprites;
        [SerializeField] private Sprite[] gardenSprites;
        [SerializeField] private Sprite[] sweetsSprites;
        [SerializeField] private Sprite[] clockworkSprites;

        public IReadOnlyList<Sprite> GetSprites(int setId)
        {
            switch (setId)
            {
                case AnimalMemoryContentIds.ForestSet: return forestSprites;
                case AnimalMemoryContentIds.CoralSet: return coralSprites;
                case AnimalMemoryContentIds.ConstellationSet: return constellationSprites;
                case AnimalMemoryContentIds.GardenSet: return gardenSprites;
                case AnimalMemoryContentIds.SweetsSet: return sweetsSprites;
                case AnimalMemoryContentIds.ClockworkSet: return clockworkSprites;
                default: return null;
            }
        }
    }
}
