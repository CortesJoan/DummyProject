using System;
using System.Collections.Generic;
using System.Linq;
using AnimalMemory.Progression;

namespace MementoMatch.Duel
{
    public enum DuelActor
    {
        Player,
        Opponent
    }

    public enum DuelTerminalAction
    {
        Continue,
        PlayerVictory,
        PlayerDefeat,
        SuddenDeath
    }

    public static class DuelRules
    {
        public const int SkillIntervalTurns = 3;
        public const int UltimateMasterSkillIntervalTurns = 2;
        public const int MaximumPlayerComboDamage = 3;

        public static int DeriveBoardSeed(int duelSeed, int boardRevision)
        {
            if (boardRevision <= 0)
                throw new ArgumentOutOfRangeException(nameof(boardRevision));
            return unchecked((duelSeed * 486187739) ^ (boardRevision * 16777619) ^ 1597463007);
        }

        public static int DeriveBrainSeed(int duelSeed, int boardRevision)
        {
            if (boardRevision <= 0)
                throw new ArgumentOutOfRangeException(nameof(boardRevision));
            return unchecked((duelSeed * 16777619) ^ (boardRevision * 486187739) ^ 1013904223);
        }

        public static int GetSkillInterval(int opponentId)
        {
            if (MementoPostgameEncounters.TryGetOpponent(opponentId, out var postgame)) return postgame.SkillInterval;
            return opponentId == AnimalMemoryContentIds.UltimateMasterOpponent ? UltimateMasterSkillIntervalTurns : SkillIntervalTurns;
        }

        public static int GetPlayerComboDamage(int combo)
        {
            return Math.Min(MaximumPlayerComboDamage, Math.Max(1, combo));
        }

        public static DuelTerminalAction GetTerminalAction(DuelState state)
        {
            if (state == null)
                throw new ArgumentNullException(nameof(state));
            if (state.RequiresSuddenDeath)
                return DuelTerminalAction.SuddenDeath;
            if (state.Winner == DuelActor.Player)
                return DuelTerminalAction.PlayerVictory;
            if (state.Winner == DuelActor.Opponent)
                return DuelTerminalAction.PlayerDefeat;
            return DuelTerminalAction.Continue;
        }

        public static int GetTargetPairs(int guideId)
        {
            if (MementoPostgameEncounters.TryGetOpponent(guideId, out var postgame)) return postgame.Health;
            switch (guideId)
            {
                case AnimalMemoryContentIds.AkiGuide: return 4;
                case AnimalMemoryContentIds.MikaGuide: return 5;
                case AnimalMemoryContentIds.YoruGuide: return 6;
                case AnimalMemoryContentIds.HanaGuide: return 6;
                case AnimalMemoryContentIds.MomoGuide: return 7;
                case AnimalMemoryContentIds.UltimateMasterOpponent: return 15;
                default: throw new ArgumentOutOfRangeException(nameof(guideId));
            }
        }

        public static int GetInitialMemoryLimit(int guideId)
        {
            if (MementoPostgameEncounters.TryGetOpponent(guideId, out var postgame)) return postgame.MemoryLimit;
            if (guideId == AnimalMemoryContentIds.UltimateMasterOpponent)
                return 8;
            if (guideId == AnimalMemoryContentIds.YoruGuide)
                return 6;
            if (guideId == AnimalMemoryContentIds.MomoGuide)
                return 5;
            if (guideId == AnimalMemoryContentIds.HanaGuide)
                return 4;
            return 3;
        }

        public static int GetSkillPeekCount(int guideId)
        {
            if (guideId == AnimalMemoryContentIds.UltimateMasterOpponent)
                return 4;
            if (guideId == AnimalMemoryContentIds.YoruGuide)
                return 3;
            if (guideId == AnimalMemoryContentIds.HanaGuide)
                return 4;
            return 2;
        }

        public static double GetForgetChance(int guideId)
        {
            if (MementoPostgameEncounters.TryGetOpponent(guideId, out var postgame)) return postgame.ForgetChance;
            if (guideId == AnimalMemoryContentIds.UltimateMasterOpponent)
                return 0.06d;
            if (guideId == AnimalMemoryContentIds.YoruGuide)
                return 0.08d;
            if (guideId == AnimalMemoryContentIds.MomoGuide)
                return 0.12d;
            if (guideId == AnimalMemoryContentIds.HanaGuide)
                return 0.16d;
            return 0.25d;
        }
    }

    public sealed class DuelState
    {
        private bool continuationUsed;
        public int OpponentGuideId { get; private set; } = -1;
        public bool CanContinueDefeat => !continuationUsed && Winner == DuelActor.Opponent;
        public int TargetPairs { get; private set; }
        public int PlayerPairs { get; private set; }
        public int OpponentPairs { get; private set; }
        public int OpponentTurns { get; private set; }
        public int PlayerHealth { get; private set; }
        public int OpponentHealth { get; private set; }
        public bool RequiresSuddenDeath { get; private set; }
        public DuelActor ActiveActor { get; private set; }
        public DuelActor? Winner { get; private set; }
        public bool IsActive => OpponentGuideId >= 0 && !Winner.HasValue;

        public void Begin(int guideId)
        {
            Begin(guideId, DuelRules.GetTargetPairs(guideId));
        }

        public void Begin(int guideId, int boardPairCount)
        {
            if (boardPairCount <= 0)
                throw new ArgumentOutOfRangeException(nameof(boardPairCount));

            continuationUsed = false;
            OpponentGuideId = guideId;
            TargetPairs = Math.Min(DuelRules.GetTargetPairs(guideId), boardPairCount);
            PlayerPairs = 0;
            OpponentPairs = 0;
            OpponentTurns = 0;
            PlayerHealth = TargetPairs;
            OpponentHealth = TargetPairs;
            RequiresSuddenDeath = false;
            ActiveActor = DuelActor.Player;
            Winner = null;
        }

        public void CompleteTurn(
            DuelActor actor,
            bool madePair,
            bool retainTurn = false,
            int damage = 1,
            bool boardExhausted = false)
        {
            if (!IsActive || RequiresSuddenDeath)
                throw new InvalidOperationException("No active duel turn.");
            if (actor != ActiveActor)
                throw new InvalidOperationException("The wrong actor completed the turn.");

            if (madePair)
            {
                damage = Math.Max(1, damage);
                if (actor == DuelActor.Player)
                {
                    PlayerPairs++;
                    OpponentHealth = Math.Max(0, OpponentHealth - damage);
                }
                else
                {
                    OpponentPairs++;
                    PlayerHealth = Math.Max(0, PlayerHealth - 1);
                }
            }

            if (actor == DuelActor.Opponent)
                OpponentTurns++;

            if (OpponentHealth <= 0)
                Winner = DuelActor.Player;
            else if (PlayerHealth <= 0)
                Winner = DuelActor.Opponent;
            else if (retainTurn)
                ActiveActor = actor;
            else
                ActiveActor = actor == DuelActor.Player
                    ? DuelActor.Opponent
                    : DuelActor.Player;

            if (boardExhausted)
                ResolveBoardExhausted(actor);
        }

        public void ApplyOpponentBonusPair(bool boardExhausted = false)
        {
            if (!IsActive || RequiresSuddenDeath)
                throw new InvalidOperationException("No active duel turn.");
            if (ActiveActor != DuelActor.Opponent)
                throw new InvalidOperationException("Opponent bonus requires the opponent turn.");

            OpponentPairs++;
            PlayerHealth = Math.Max(0, PlayerHealth - 1);
            if (PlayerHealth <= 0)
                Winner = DuelActor.Opponent;

            if (boardExhausted)
                ResolveBoardExhausted(DuelActor.Opponent);
        }

        public void ResolveBoardExhausted(DuelActor lastPairActor)
        {
            if (OpponentGuideId < 0 || Winner.HasValue)
                return;

            if (OpponentHealth < PlayerHealth)
                Winner = DuelActor.Player;
            else if (PlayerHealth < OpponentHealth)
                Winner = DuelActor.Opponent;
            else
                RequiresSuddenDeath = true;
        }

        public void BeginSuddenDeathRound()
        {
            if (!IsActive || !RequiresSuddenDeath)
                throw new InvalidOperationException("Sudden death was not requested.");

            RequiresSuddenDeath = false;
            ActiveActor = DuelActor.Player;
        }

        /// <summary>Restores half health and the player's turn without erasing score, pairs or rival damage.</summary>
        public bool TryContinueDefeat(bool boardExhausted)
        {
            if (!CanContinueDefeat) return false;
            continuationUsed = true;
            PlayerHealth = Math.Max(PlayerHealth, (TargetPairs + 1) / 2);
            Winner = null;
            ActiveActor = DuelActor.Player;
            RequiresSuddenDeath = boardExhausted;
            return true;
        }

        public bool UndoPlayerPair(int damage = 1)
        {
            if (OpponentGuideId < 0 || PlayerPairs <= 0)
                return false;

            PlayerPairs--;
            OpponentHealth = Math.Min(TargetPairs, OpponentHealth + Math.Max(1, damage));
            Winner = null;
            RequiresSuddenDeath = false;
            return true;
        }

        public void Reset()
        {
            OpponentGuideId = -1;
            TargetPairs = 0;
            PlayerPairs = 0;
            OpponentPairs = 0;
            OpponentTurns = 0;
            PlayerHealth = 0;
            OpponentHealth = 0;
            RequiresSuddenDeath = false;
            ActiveActor = DuelActor.Player;
            Winner = null;
        }
    }

    public interface IDuelRandom
    {
        int Range(int minimumInclusive, int maximumExclusive);
        double Value();
    }

    public sealed class SystemDuelRandom : IDuelRandom
    {
        private readonly Random random;

        public SystemDuelRandom(int seed)
        {
            random = new Random(seed);
        }

        public int Range(int minimumInclusive, int maximumExclusive)
        {
            return random.Next(minimumInclusive, maximumExclusive);
        }

        public double Value()
        {
            return random.NextDouble();
        }
    }

    public struct DuelCardId
    {
        public int BoardRevision { get; }
        public int Ordinal { get; }

        public DuelCardId(int boardRevision, int ordinal)
        {
            if (boardRevision <= 0)
                throw new ArgumentOutOfRangeException(nameof(boardRevision));
            if (ordinal < 0)
                throw new ArgumentOutOfRangeException(nameof(ordinal));
            BoardRevision = boardRevision;
            Ordinal = ordinal;
        }
    }

    public sealed class DuelOpponentBrain
    {
        private readonly Dictionary<int, int> seenPairs = new Dictionary<int, int>();
        private readonly IDuelRandom random;
        private readonly double forgetChance;

        public DuelOpponentBrain(IDuelRandom random, double forgetChance)
        {
            this.random = random ?? throw new ArgumentNullException(nameof(random));
            this.forgetChance = Math.Max(0d, Math.Min(1d, forgetChance));
        }

        public int KnownCardCount => seenPairs.Count;
        public int BoardRevision { get; private set; }

        public void BeginBoard(int boardRevision)
        {
            if (boardRevision <= 0)
                throw new ArgumentOutOfRangeException(nameof(boardRevision));
            if (BoardRevision == boardRevision)
                return;
            BoardRevision = boardRevision;
            seenPairs.Clear();
        }

        public void Observe(DuelCardId cardId, int pairId)
        {
            if (cardId.BoardRevision != BoardRevision)
                throw new InvalidOperationException("Card belongs to a stale duel board.");
            Observe(cardId.Ordinal, pairId);
        }

        public void Observe(int cardIndex, int pairId)
        {
            if (cardIndex < 0)
                throw new ArgumentOutOfRangeException(nameof(cardIndex));
            seenPairs[cardIndex] = pairId;
        }

        public bool KnowsCard(int cardIndex)
        {
            return seenPairs.ContainsKey(cardIndex);
        }

        public void ForgetObservedCards()
        {
            seenPairs.Clear();
        }

        public int ChooseFirst(IReadOnlyList<int> availableCards)
        {
            ValidateAvailable(availableCards);
            if (TryFindKnownPair(availableCards, out int first, out _))
                return first;
            return availableCards[random.Range(0, availableCards.Count)];
        }

        public int ChooseSecond(int firstCard, IReadOnlyList<int> availableCards)
        {
            ValidateAvailable(availableCards);
            if (seenPairs.TryGetValue(firstCard, out int pairId))
            {
                for (int i = 0; i < availableCards.Count; i++)
                {
                    int candidate = availableCards[i];
                    if (candidate != firstCard &&
                        seenPairs.TryGetValue(candidate, out int candidatePair) &&
                        candidatePair == pairId)
                        return candidate;
                }
            }

            List<int> alternatives = availableCards
                .Where(card => card != firstCard)
                .ToList();
            if (alternatives.Count == 0)
                throw new InvalidOperationException("No legal second card is available.");
            return alternatives[random.Range(0, alternatives.Count)];
        }

        public IReadOnlyList<int> ChoosePeekTargets(
            IReadOnlyList<int> availableCards,
            int count)
        {
            ValidateAvailable(availableCards);
            List<int> candidates = availableCards.ToList();
            List<int> result = new List<int>(Math.Min(count, candidates.Count));
            while (result.Count < count && candidates.Count > 0)
            {
                int selected = random.Range(0, candidates.Count);
                result.Add(candidates[selected]);
                candidates.RemoveAt(selected);
            }

            return result;
        }

        public void TrimInitialMemory(int maximumKnownCards)
        {
            maximumKnownCards = Math.Max(0, maximumKnownCards);
            if (maximumKnownCards == 0)
            {
                seenPairs.Clear();
                return;
            }

            List<KeyValuePair<int, int>> candidates = seenPairs.ToList();
            HashSet<int> rememberedPairIds = new HashSet<int>();
            seenPairs.Clear();

            while (seenPairs.Count < maximumKnownCards && candidates.Count > 0)
            {
                int selected = random.Range(0, candidates.Count);
                KeyValuePair<int, int> memory = candidates[selected];
                candidates.RemoveAt(selected);
                if (!rememberedPairIds.Add(memory.Value))
                    continue;

                seenPairs[memory.Key] = memory.Value;
                for (int i = candidates.Count - 1; i >= 0; i--)
                {
                    if (candidates[i].Value == memory.Value)
                        candidates.RemoveAt(i);
                }
            }
        }

        public void DecayMemory()
        {
            if (seenPairs.Count > 0 && random.Value() < forgetChance)
                ForgetOne();
        }

        public bool TryFindKnownPair(
            IReadOnlyList<int> availableCards,
            out int first,
            out int second)
        {
            HashSet<int> available = new HashSet<int>(availableCards);
            foreach (KeyValuePair<int, int> entry in seenPairs)
            {
                if (!available.Contains(entry.Key))
                    continue;

                foreach (KeyValuePair<int, int> possible in seenPairs)
                {
                    if (possible.Key != entry.Key &&
                        possible.Value == entry.Value &&
                        available.Contains(possible.Key))
                    {
                        first = entry.Key;
                        second = possible.Key;
                        return true;
                    }
                }
            }

            first = -1;
            second = -1;
            return false;
        }

        private void ForgetOne()
        {
            int selected = random.Range(0, seenPairs.Count);
            int key = seenPairs.Keys.ElementAt(selected);
            seenPairs.Remove(key);
        }

        private static void ValidateAvailable(IReadOnlyList<int> availableCards)
        {
            if (availableCards == null || availableCards.Count == 0)
                throw new InvalidOperationException("No available cards.");
        }
    }
}
