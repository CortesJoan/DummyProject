using System;
using System.Collections.Generic;

namespace MementoMatch.Duel
{
    public enum GodDuelPhase
    {
        PhaseOne = 1,
        PhaseTwo = 2
    }

    public enum GodBoardCardStatus
    {
        Live = 0,
        Resolved = 1,
        Erased = 2,
        Orphaned = 3
    }

    public readonly struct GodBoardCard
    {
        public DuelCardId CardId { get; }
        public int PairId { get; }
        public GodBoardCardStatus Status { get; }

        public GodBoardCard(
            DuelCardId cardId,
            int pairId,
            GodBoardCardStatus status = GodBoardCardStatus.Live)
        {
            CardId = cardId;
            PairId = pairId;
            Status = status;
        }

        public GodBoardCard WithStatus(GodBoardCardStatus status)
        {
            return new GodBoardCard(CardId, PairId, status);
        }
    }
    public static class GodDuelRules
    {
        public const int PhaseTwoThresholdPercent = 50;
        public const int JudgementCooldownTurns = 5;
        public const int EraseCooldownTurns = 4;
        public const int JudgementMaxPairs = 2;

        public static bool ShouldEnterPhaseTwo(
            int initialHealth,
            int currentHealth,
            bool duelTerminal)
        {
            if (initialHealth <= 0)
                throw new ArgumentOutOfRangeException(nameof(initialHealth));
            if (currentHealth < 0)
                throw new ArgumentOutOfRangeException(nameof(currentHealth));
            if (duelTerminal || currentHealth <= 0)
                return false;

            return (long)currentHealth * 100L <=
                   (long)initialHealth * PhaseTwoThresholdPercent;
        }

        public static int DeriveEraseSeed(
            int duelSeed,
            int boardRevision,
            int activationSerial)
        {
            if (boardRevision <= 0)
                throw new ArgumentOutOfRangeException(nameof(boardRevision));
            if (activationSerial < 0)
                throw new ArgumentOutOfRangeException(nameof(activationSerial));

            return unchecked(
                (duelSeed * 486187739) ^
                (boardRevision * 16777619) ^
                (activationSerial * 374761393) ^
                0x2D7A5C31);
        }

        public static bool HasStructuralPair(
            int boardRevision,
            IReadOnlyList<GodBoardCard> cards)
        {
            if (boardRevision <= 0)
                throw new ArgumentOutOfRangeException(nameof(boardRevision));
            if (cards == null)
                throw new ArgumentNullException(nameof(cards));

            var pairCounts = new Dictionary<int, int>();
            for (int i = 0; i < cards.Count; i++)
            {
                GodBoardCard card = cards[i];
                ValidateRevision(boardRevision, card.CardId);
                if (card.Status != GodBoardCardStatus.Live)
                    continue;

                pairCounts.TryGetValue(card.PairId, out int count);
                count++;
                if (count >= 2)
                    return true;
                pairCounts[card.PairId] = count;
            }

            return false;
        }

        public static bool TryChooseEraseTarget(int boardRevision, IReadOnlyList<GodBoardCard> cards, IReadOnlyCollection<int> unavailableOrdinals, IDuelRandom random, out DuelCardId target)
        {
            if (boardRevision <= 0) throw new ArgumentOutOfRangeException(nameof(boardRevision));
            if (cards == null) throw new ArgumentNullException(nameof(cards));
            if (random == null) throw new ArgumentNullException(nameof(random));
            var unavailable = unavailableOrdinals != null ? new HashSet<int>(unavailableOrdinals) : new HashSet<int>();
            var candidates = new List<DuelCardId>();
            for (int i = 0; i < cards.Count; i++)
            {
                GodBoardCard card = cards[i];
                ValidateRevision(boardRevision, card.CardId);
                if (card.Status == GodBoardCardStatus.Live && !unavailable.Contains(card.CardId.Ordinal)) candidates.Add(card.CardId);
            }
            if (candidates.Count == 0)
            {
                target = default;
                return false;
            }
            candidates.Sort((left, right) => left.Ordinal.CompareTo(right.Ordinal));
            target = candidates[random.Range(0, candidates.Count)];
            return true;
        }

        public static bool TryApplyErase(int boardRevision, DuelCardId target, IReadOnlyList<GodBoardCard> cards, out List<GodBoardCard> updated)
        {
            if (boardRevision <= 0) throw new ArgumentOutOfRangeException(nameof(boardRevision));
            if (cards == null) throw new ArgumentNullException(nameof(cards));
            ValidateRevision(boardRevision, target);
            updated = new List<GodBoardCard>(cards.Count);
            int targetIndex = -1;
            int targetPairId = int.MinValue;
            for (int i = 0; i < cards.Count; i++)
            {
                GodBoardCard card = cards[i];
                ValidateRevision(boardRevision, card.CardId);
                if (card.CardId.Ordinal == target.Ordinal)
                {
                    targetIndex = i;
                    targetPairId = card.PairId;
                    updated.Add(card.Status == GodBoardCardStatus.Live ? card.WithStatus(GodBoardCardStatus.Erased) : card);
                }
                else updated.Add(card);
            }
            if (targetIndex < 0 || cards[targetIndex].Status != GodBoardCardStatus.Live)
            {
                updated = new List<GodBoardCard>(cards);
                return false;
            }
            int remainingLive = 0;
            for (int i = 0; i < updated.Count; i++) if (updated[i].PairId == targetPairId && updated[i].Status == GodBoardCardStatus.Live) remainingLive++;
            if (remainingLive == 1)
            {
                for (int i = 0; i < updated.Count; i++)
                    if (updated[i].PairId == targetPairId && updated[i].Status == GodBoardCardStatus.Live)
                        updated[i] = updated[i].WithStatus(GodBoardCardStatus.Orphaned);
            }
            return true;
        }
        public static bool TryChooseJudgementTargets(
            int boardRevision,
            IReadOnlyList<GodBoardCard> cards,
            IDuelRandom random,
            out List<GodBoardCard> targets)
        {
            if (boardRevision <= 0) throw new ArgumentOutOfRangeException(nameof(boardRevision));
            if (cards == null) throw new ArgumentNullException(nameof(cards));
            if (random == null) throw new ArgumentNullException(nameof(random));

            var liveByPair = new Dictionary<int, List<GodBoardCard>>();
            for (int i = 0; i < cards.Count; i++)
            {
                GodBoardCard card = cards[i];
                ValidateRevision(boardRevision, card.CardId);
                if (card.Status != GodBoardCardStatus.Live)
                    continue;
                if (!liveByPair.TryGetValue(card.PairId, out List<GodBoardCard> group))
                {
                    group = new List<GodBoardCard>(2);
                    liveByPair[card.PairId] = group;
                }
                group.Add(card);
            }

            var eligiblePairIds = new List<int>();
            foreach (KeyValuePair<int, List<GodBoardCard>> entry in liveByPair)
            {
                if (entry.Value.Count >= 2)
                    eligiblePairIds.Add(entry.Key);
            }

            targets = new List<GodBoardCard>();
            if (eligiblePairIds.Count == 0)
                return false;

            eligiblePairIds.Sort();
            for (int pick = 0; pick < JudgementMaxPairs && eligiblePairIds.Count > 0; pick++)
            {
                int selected = random.Range(0, eligiblePairIds.Count);
                int pairId = eligiblePairIds[selected];
                eligiblePairIds.RemoveAt(selected);

                List<GodBoardCard> group = liveByPair[pairId];
                group.Sort((left, right) => left.CardId.Ordinal.CompareTo(right.CardId.Ordinal));
                targets.Add(group[0]);
                targets.Add(group[1]);
            }

            return targets.Count > 0;
        }

        private static void ValidateRevision(int boardRevision, DuelCardId cardId)
        {
            if (cardId.BoardRevision != boardRevision)
                throw new InvalidOperationException("GOD action referenced a stale duel board.");
        }
    }
    public enum GodOwnerTurnPlan
    {
        None = 0,
        NormalPlay = 1,
        Judgement = 2,
        Erase = 3,
        Rebuild = 4
    }

    public sealed class GodDuelState
    {
        public GodDuelPhase Phase { get; private set; } = GodDuelPhase.PhaseOne;
        public int InitialHealth { get; private set; }
        public int BoardRevision { get; private set; }
        public int OwnerTurns { get; private set; }
        public int RebuildSerial { get; private set; }
        public int JudgementTurnsUntilReady { get; private set; }
        public int EraseTurnsUntilReady { get; private set; }

        public bool IsJudgementReady =>
            Phase == GodDuelPhase.PhaseOne && JudgementTurnsUntilReady <= 0;
        public bool IsEraseReady =>
            Phase == GodDuelPhase.PhaseTwo && EraseTurnsUntilReady <= 0;

        public void Begin(int initialHealth, int boardRevision)
        {
            if (initialHealth <= 0)
                throw new ArgumentOutOfRangeException(nameof(initialHealth));
            if (boardRevision <= 0)
                throw new ArgumentOutOfRangeException(nameof(boardRevision));

            Phase = GodDuelPhase.PhaseOne;
            InitialHealth = initialHealth;
            BoardRevision = boardRevision;
            OwnerTurns = 0;
            RebuildSerial = 0;
            JudgementTurnsUntilReady = GodDuelRules.JudgementCooldownTurns;
            EraseTurnsUntilReady = GodDuelRules.EraseCooldownTurns;
        }

        public void Reset()
        {
            Phase = GodDuelPhase.PhaseOne;
            InitialHealth = 0;
            BoardRevision = 0;
            OwnerTurns = 0;
            RebuildSerial = 0;
            JudgementTurnsUntilReady = 0;
            EraseTurnsUntilReady = 0;
        }

        public bool IsCurrentBoard(int boardRevision)
        {
            return BoardRevision > 0 && boardRevision == BoardRevision;
        }

        /// <summary>Crosses to phase two exactly once, after the current attempt resolved.
        /// Lethal damage and terminal duels always win over the transition.</summary>
        public bool TryEnterPhaseTwo(int currentHealth, bool duelTerminal)
        {
            if (InitialHealth <= 0)
                throw new InvalidOperationException("GOD duel state was not started.");
            if (Phase == GodDuelPhase.PhaseTwo)
                return false;
            if (!GodDuelRules.ShouldEnterPhaseTwo(InitialHealth, currentHealth, duelTerminal))
                return false;

            Phase = GodDuelPhase.PhaseTwo;
            EraseTurnsUntilReady = GodDuelRules.EraseCooldownTurns;
            return true;
        }

        /// <summary>One reduction per owner turn, never per animation or per resolved pair.</summary>
        public bool BeginOwnerTurn()
        {
            OwnerTurns++;
            if (JudgementTurnsUntilReady > 0)
                JudgementTurnsUntilReady--;
            if (EraseTurnsUntilReady > 0)
                EraseTurnsUntilReady--;
            return IsJudgementReady || IsEraseReady;
        }

        public bool CommitJudgement()
        {
            if (!IsJudgementReady)
                return false;
            JudgementTurnsUntilReady = GodDuelRules.JudgementCooldownTurns;
            return true;
        }

        public bool CommitErase()
        {
            if (!IsEraseReady)
                return false;
            EraseTurnsUntilReady = GodDuelRules.EraseCooldownTurns;
            return true;
        }

        /// <summary>Priority: terminal, rebuild, phase skill, normal play. A single
        /// plan per turn prevents stacking several attacks at once.</summary>
        public GodOwnerTurnPlan PlanOwnerTurn(
            int boardRevision,
            IReadOnlyList<GodBoardCard> cards,
            bool duelTerminal)
        {
            if (duelTerminal)
                return GodOwnerTurnPlan.None;
            if (NeedsRebuild(boardRevision, cards))
                return GodOwnerTurnPlan.Rebuild;
            if (IsJudgementReady)
                return GodOwnerTurnPlan.Judgement;
            if (IsEraseReady)
                return GodOwnerTurnPlan.Erase;
            return GodOwnerTurnPlan.NormalPlay;
        }

        /// <summary>Structural impossibility only: no identity keeps two live eligible
        /// cards. Temporary blocks (curtain, selection, animation) are not card states
        /// and must expire before this is asked.</summary>
        public bool NeedsRebuild(int boardRevision, IReadOnlyList<GodBoardCard> cards)
        {
            if (BoardRevision <= 0)
                throw new InvalidOperationException("GOD duel state was not started.");
            return !GodDuelRules.HasStructuralPair(boardRevision, cards);
        }

        /// <summary>Starts a rebuild exactly once per new revision and returns the guard
        /// serial. The caller deals a full paired board and hands the turn to the player;
        /// the rebuild itself never attacks.</summary>
        public bool BeginRebuild(int newBoardRevision, out int rebuildSerial)
        {
            rebuildSerial = RebuildSerial;
            if (BoardRevision <= 0)
                throw new InvalidOperationException("GOD duel state was not started.");
            if (newBoardRevision <= BoardRevision)
                return false;

            BoardRevision = newBoardRevision;
            RebuildSerial++;
            rebuildSerial = RebuildSerial;
            return true;
        }
    }

    public sealed class GodJudgementState
    {
        private readonly List<GodBoardCard> targets = new List<GodBoardCard>(4);

        public int BoardRevision { get; private set; }
        public bool IsArmed { get; private set; }
        public int TargetPairCount { get; private set; }
        public IReadOnlyList<GodBoardCard> Targets => targets;

        public void Reset()
        {
            BoardRevision = 0;
            IsArmed = false;
            TargetPairCount = 0;
            targets.Clear();
        }

        public void BeginBoard(int boardRevision)
        {
            if (boardRevision <= 0)
                throw new ArgumentOutOfRangeException(nameof(boardRevision));
            Reset();
            BoardRevision = boardRevision;
        }

        public bool Arm(int boardRevision, IReadOnlyList<GodBoardCard> marks)
        {
            if (boardRevision != BoardRevision)
                throw new InvalidOperationException("Judgement marks belong to a stale duel board.");
            if (marks == null)
                throw new ArgumentNullException(nameof(marks));
            if (marks.Count == 0 || marks.Count % 2 != 0)
                return false;

            var cardsPerPair = new Dictionary<int, int>();
            for (int i = 0; i < marks.Count; i++)
            {
                GodBoardCard mark = marks[i];
                if (mark.CardId.BoardRevision != boardRevision)
                    throw new InvalidOperationException("Judgement marks belong to a stale duel board.");
                if (mark.Status != GodBoardCardStatus.Live)
                    return false;
                cardsPerPair.TryGetValue(mark.PairId, out int count);
                cardsPerPair[mark.PairId] = count + 1;
            }

            if (cardsPerPair.Count == 0 || cardsPerPair.Count > GodDuelRules.JudgementMaxPairs)
                return false;
            foreach (KeyValuePair<int, int> entry in cardsPerPair)
            {
                if (entry.Value != 2)
                    return false;
            }

            targets.Clear();
            for (int i = 0; i < marks.Count; i++)
                targets.Add(marks[i]);
            TargetPairCount = cardsPerPair.Count;
            IsArmed = true;
            return true;
        }

        /// <summary>Strikes only the marked pairs that are still complete. A pair the
        /// player already removed cancels its ray and is never substituted.</summary>
        public bool TryResolve(IReadOnlyList<GodBoardCard> cards, out List<GodBoardCard> struck)
        {
            struck = new List<GodBoardCard>();
            if (!IsArmed)
                return false;
            if (cards == null)
                throw new ArgumentNullException(nameof(cards));

            var livePerPair = new Dictionary<int, int>();
            for (int i = 0; i < cards.Count; i++)
            {
                GodBoardCard card = cards[i];
                if (card.CardId.BoardRevision != BoardRevision)
                    throw new InvalidOperationException("Judgement resolution referenced a stale duel board.");
                if (card.Status != GodBoardCardStatus.Live)
                    continue;
                livePerPair.TryGetValue(card.PairId, out int count);
                livePerPair[card.PairId] = count + 1;
            }

            for (int i = 0; i + 1 < targets.Count; i += 2)
            {
                int pairId = targets[i].PairId;
                if (livePerPair.TryGetValue(pairId, out int live) && live >= 2)
                {
                    struck.Add(targets[i]);
                    struck.Add(targets[i + 1]);
                }
            }

            Clear();
            return struck.Count > 0;
        }

        public void Clear()
        {
            targets.Clear();
            IsArmed = false;
            TargetPairCount = 0;
        }
    }

    public sealed class GodEraseState
    {
        public int BoardRevision { get; private set; }
        public bool IsArmed { get; private set; }
        public DuelCardId Target { get; private set; }

        public void Reset()
        {
            BoardRevision = 0;
            IsArmed = false;
            Target = default;
        }

        public void BeginBoard(int boardRevision)
        {
            if (boardRevision <= 0)
                throw new ArgumentOutOfRangeException(nameof(boardRevision));
            Reset();
            BoardRevision = boardRevision;
        }

        public bool Arm(int boardRevision, DuelCardId target, IReadOnlyList<GodBoardCard> cards)
        {
            if (boardRevision != BoardRevision)
                throw new InvalidOperationException("Erase target belongs to a stale duel board.");
            if (cards == null)
                throw new ArgumentNullException(nameof(cards));

            bool live = false;
            for (int i = 0; i < cards.Count; i++)
            {
                GodBoardCard card = cards[i];
                if (card.CardId.BoardRevision != boardRevision)
                    throw new InvalidOperationException("Erase target belongs to a stale duel board.");
                if (card.CardId.Ordinal == target.Ordinal &&
                    card.Status == GodBoardCardStatus.Live)
                    live = true;
            }

            if (!live)
                return false;

            Target = target;
            IsArmed = true;
            return true;
        }

        /// <summary>Fires only if the marked card is still live. If the player already
        /// removed it, the ray fails and is never redirected to another card.</summary>
        public bool TryResolve(
            IReadOnlyList<GodBoardCard> cards,
            out List<GodBoardCard> updated,
            out bool saved)
        {
            if (cards == null)
                throw new ArgumentNullException(nameof(cards));
            updated = new List<GodBoardCard>(cards);
            saved = false;
            if (!IsArmed)
                return false;

            bool applied = GodDuelRules.TryApplyErase(
                BoardRevision, Target, cards, out List<GodBoardCard> result);
            Clear();
            if (!applied)
            {
                saved = true;
                return false;
            }

            updated = result;
            return true;
        }

        public void Clear()
        {
            IsArmed = false;
            Target = default;
        }
    }
}
