using System;
using System.Collections.Generic;
using System.Linq;
using AnimalMemory.Progression;

namespace MementoMatch.Duel
{
    public static class PostgameAbilityRules
    {
        public const int NewHandCooldownTurns = 5;
        public const int CurtainCooldownTurns = 4;
        public const int TestimonyCooldownTurns = 5;

        public static int GetCooldown(MementoPostgameAbilityId abilityId)
        {
            switch (abilityId)
            {
                case MementoPostgameAbilityId.NewHand: return NewHandCooldownTurns;
                case MementoPostgameAbilityId.Curtain: return CurtainCooldownTurns;
                case MementoPostgameAbilityId.Testimony: return TestimonyCooldownTurns;
                default: return 0;
            }
        }

        public static string GetDisplayName(MementoPostgameAbilityId abilityId)
        {
            switch (abilityId)
            {
                case MementoPostgameAbilityId.NewHand: return "Nueva mano";
                case MementoPostgameAbilityId.Curtain: return "Telón";
                case MementoPostgameAbilityId.Testimony: return "Testimonio";
                default: return string.Empty;
            }
        }

        public static int DeriveAbilitySeed(int duelSeed, int boardRevision, int activationSerial)
        {
            if (boardRevision <= 0)
                throw new ArgumentOutOfRangeException(nameof(boardRevision));
            if (activationSerial < 0)
                throw new ArgumentOutOfRangeException(nameof(activationSerial));

            return unchecked(
                (duelSeed * 374761393) ^
                (boardRevision * 668265263) ^
                (activationSerial * 1274126177) ^
                0x51ED270B);
        }

        public static bool TryBuildNewHandOrder(
            IReadOnlyList<DuelCardId> unresolvedCards,
            IReadOnlyList<int> pairIds,
            IDuelRandom random,
            out List<DuelCardId> shuffled)
        {
            if (unresolvedCards == null)
                throw new ArgumentNullException(nameof(unresolvedCards));
            if (pairIds == null)
                throw new ArgumentNullException(nameof(pairIds));
            if (random == null)
                throw new ArgumentNullException(nameof(random));
            if (unresolvedCards.Count != pairIds.Count)
                throw new ArgumentException("Card and pair counts must match.");
            if (unresolvedCards.Count <= 2)
            {
                shuffled = new List<DuelCardId>(unresolvedCards);
                return false;
            }

            int revision = unresolvedCards[0].BoardRevision;
            var distinctPairs = new HashSet<int>();
            var ordinals = new HashSet<int>();
            for (int i = 0; i < unresolvedCards.Count; i++)
            {
                if (unresolvedCards[i].BoardRevision != revision)
                    throw new InvalidOperationException("Nueva mano cannot mix board revisions.");
                if (!ordinals.Add(unresolvedCards[i].Ordinal))
                    throw new InvalidOperationException("Nueva mano requires unique stable card ids.");
                distinctPairs.Add(pairIds[i]);
            }

            if (distinctPairs.Count <= 1)
            {
                shuffled = new List<DuelCardId>(unresolvedCards);
                return false;
            }

            shuffled = new List<DuelCardId>(unresolvedCards);
            for (int i = 0; i < shuffled.Count - 1; i++)
            {
                int selected = random.Range(i, shuffled.Count);
                DuelCardId temporary = shuffled[i];
                shuffled[i] = shuffled[selected];
                shuffled[selected] = temporary;
            }

            bool changed = false;
            for (int i = 0; i < shuffled.Count; i++)
            {
                if (shuffled[i].Ordinal != unresolvedCards[i].Ordinal)
                {
                    changed = true;
                    break;
                }
            }

            if (!changed)
            {
                DuelCardId first = shuffled[0];
                shuffled.RemoveAt(0);
                shuffled.Add(first);
            }

            return true;
        }

        public static bool TryChooseCurtainRow(
            int rows,
            int columns,
            IReadOnlyList<int> selectableSlots,
            IReadOnlyList<int> pairIdsBySlot,
            IDuelRandom random,
            out int row)
        {
            if (rows <= 0)
                throw new ArgumentOutOfRangeException(nameof(rows));
            if (columns <= 0)
                throw new ArgumentOutOfRangeException(nameof(columns));
            if (selectableSlots == null)
                throw new ArgumentNullException(nameof(selectableSlots));
            if (pairIdsBySlot == null)
                throw new ArgumentNullException(nameof(pairIdsBySlot));
            if (random == null)
                throw new ArgumentNullException(nameof(random));
            if (pairIdsBySlot.Count < rows * columns)
                throw new ArgumentException("Pair table does not cover the board.", nameof(pairIdsBySlot));

            var selectable = new HashSet<int>(selectableSlots);
            var legalRows = new List<int>();
            for (int candidateRow = 0; candidateRow < rows; candidateRow++)
            {
                bool blocksSomething = false;
                var outsidePairCounts = new Dictionary<int, int>();
                foreach (int slot in selectableSlots)
                {
                    if (slot < 0 || slot >= rows * columns)
                        throw new ArgumentOutOfRangeException(nameof(selectableSlots));

                    if (slot / columns == candidateRow)
                    {
                        blocksSomething = true;
                        continue;
                    }

                    int pairId = pairIdsBySlot[slot];
                    outsidePairCounts.TryGetValue(pairId, out int count);
                    outsidePairCounts[pairId] = count + 1;
                }

                if (!blocksSomething)
                    continue;

                bool completePairOutside = outsidePairCounts.Values.Any(count => count >= 2);
                if (completePairOutside)
                    legalRows.Add(candidateRow);
            }

            if (legalRows.Count == 0)
            {
                row = -1;
                return false;
            }

            row = legalRows[random.Range(0, legalRows.Count)];
            return true;
        }
    }

    public sealed class PostgameAbilityClock
    {
        public MementoPostgameAbilityId AbilityId { get; private set; }
        public int TurnsUntilReady { get; private set; }
        public bool IsReady =>
            AbilityId != MementoPostgameAbilityId.Unassigned &&
            TurnsUntilReady <= 0;

        public void Reset(MementoPostgameAbilityId abilityId)
        {
            AbilityId = abilityId;
            TurnsUntilReady = PostgameAbilityRules.GetCooldown(abilityId);
        }

        public bool BeginOwnerTurn()
        {
            if (AbilityId == MementoPostgameAbilityId.Unassigned)
                return false;
            if (TurnsUntilReady > 0)
                TurnsUntilReady--;
            return IsReady;
        }

        public bool CommitCast()
        {
            if (!IsReady)
                return false;
            TurnsUntilReady = PostgameAbilityRules.GetCooldown(AbilityId);
            return true;
        }
    }

    public sealed class PostgameCurtainState
    {
        public int BoardRevision { get; private set; }
        public int ActiveRow { get; private set; } = -1;
        public DuelActor? TargetActor { get; private set; }
        public bool IsActive => ActiveRow >= 0 && TargetActor.HasValue;

        public void Reset()
        {
            BoardRevision = 0;
            Clear();
        }

        public void BeginBoard(int boardRevision)
        {
            if (boardRevision <= 0)
                throw new ArgumentOutOfRangeException(nameof(boardRevision));
            Reset();
            BoardRevision = boardRevision;
        }

        public void Arm(int boardRevision, int row, DuelActor targetActor)
        {
            if (boardRevision != BoardRevision)
                throw new InvalidOperationException("Curtain target belongs to a stale duel board.");
            if (row < 0)
                throw new ArgumentOutOfRangeException(nameof(row));
            ActiveRow = row;
            TargetActor = targetActor;
        }

        public bool Blocks(DuelCardId cardId, int slot, int columns, DuelActor actor)
        {
            if (!IsActive || actor != TargetActor.Value)
                return false;
            if (cardId.BoardRevision != BoardRevision || columns <= 0 || slot < 0)
                return false;
            return slot / columns == ActiveRow;
        }

        public bool RegisterAttempt(DuelActor actor)
        {
            if (!IsActive || actor != TargetActor.Value)
                return false;
            Clear();
            return true;
        }

        public void Clear()
        {
            ActiveRow = -1;
            TargetActor = null;
        }
    }

    public sealed class PostgameTestimonyState
    {
        private readonly HashSet<int> publicOrdinals = new HashSet<int>();
        private bool playerAttempted;
        private bool opponentAttempted;

        public int BoardRevision { get; private set; }
        public bool IsWindowArmed { get; private set; }
        public int PublicCardCount => publicOrdinals.Count;
        public IEnumerable<int> PublicOrdinals => publicOrdinals;

        public void Reset()
        {
            BoardRevision = 0;
            IsWindowArmed = false;
            playerAttempted = false;
            opponentAttempted = false;
            publicOrdinals.Clear();
        }

        public void BeginBoard(int boardRevision)
        {
            if (boardRevision <= 0)
                throw new ArgumentOutOfRangeException(nameof(boardRevision));
            Reset();
            BoardRevision = boardRevision;
        }

        public void ArmRound(int boardRevision)
        {
            if (boardRevision != BoardRevision)
                throw new InvalidOperationException("Testimony window belongs to a stale duel board.");
            IsWindowArmed = true;
            playerAttempted = false;
            opponentAttempted = false;
        }

        public bool RegisterAttempt(
            DuelActor actor,
            bool matched,
            DuelCardId first,
            DuelCardId second)
        {
            if (!IsWindowArmed)
                return false;
            ValidateCard(first);
            ValidateCard(second);

            if (actor == DuelActor.Player)
            {
                if (playerAttempted)
                    return false;
                playerAttempted = true;
            }
            else
            {
                if (opponentAttempted)
                    return false;
                opponentAttempted = true;
            }

            if (!matched)
            {
                publicOrdinals.Add(first.Ordinal);
                publicOrdinals.Add(second.Ordinal);
                IsWindowArmed = false;
                return true;
            }

            if (playerAttempted && opponentAttempted)
                IsWindowArmed = false;
            return false;
        }

        public bool IsPublic(DuelCardId cardId)
        {
            return cardId.BoardRevision == BoardRevision &&
                   publicOrdinals.Contains(cardId.Ordinal);
        }

        public void ResolveMatched(DuelCardId first, DuelCardId second)
        {
            ValidateCard(first);
            ValidateCard(second);
            publicOrdinals.Remove(first.Ordinal);
            publicOrdinals.Remove(second.Ordinal);
        }

        private void ValidateCard(DuelCardId cardId)
        {
            if (cardId.BoardRevision != BoardRevision)
                throw new InvalidOperationException("Testimony card belongs to a stale duel board.");
        }
    }
}
