using System;

namespace AnimalMemory.UI
{
    /// <summary>Four-card teaching board, independent of scores, saves and combat.</summary>
    public sealed class MementoPracticeBoard
    {
        private readonly int[] identities = { 0, 1, 0, 1 };
        private readonly bool[] matched = new bool[4];
        public int First { get; private set; } = -1;
        public int Second { get; private set; } = -1;
        public int Pairs { get; private set; }
        public bool IsComplete => Pairs == 2;
        public bool AwaitingResolution => Second >= 0;
        public MementoPracticeBoard(int seed)
        {
            var random = new Random(seed);
            for (int i = 3; i > 0; i--)
            {
                int j = random.Next(i + 1);
                int value = identities[i]; identities[i] = identities[j]; identities[j] = value;
            }
        }
        public int Identity(int index) => identities[index];
        public bool IsMatched(int index) => matched[index];
        public bool IsVisible(int index) => matched[index] || First == index || Second == index;
        public bool TryFlip(int index)
        {
            if (index < 0 || index >= 4 || matched[index] || index == First ||
                AwaitingResolution || IsComplete) return false;
            if (First < 0) First = index; else Second = index;
            return true;
        }
        public bool Resolve()
        {
            if (!AwaitingResolution) return false;
            bool match = identities[First] == identities[Second];
            if (match) { matched[First] = matched[Second] = true; Pairs++; }
            First = Second = -1;
            return match;
        }
    }
}
