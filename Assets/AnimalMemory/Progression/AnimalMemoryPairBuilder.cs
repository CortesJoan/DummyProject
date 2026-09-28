using System;
using System.Collections.Generic;

namespace AnimalMemory.Progression
{
    public static class AnimalMemoryPairBuilder
    {
        public static List<int> BuildPairedIndices(int pairCount)
        {
            if (pairCount < 0)
                throw new ArgumentOutOfRangeException(nameof(pairCount));

            List<int> result = new List<int>(pairCount * 2);
            for (int pairId = 0; pairId < pairCount; pairId++)
            {
                result.Add(pairId);
                result.Add(pairId);
            }

            return result;
        }
    }
}
