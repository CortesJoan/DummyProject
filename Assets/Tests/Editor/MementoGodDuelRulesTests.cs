using System.Collections.Generic;
using MementoMatch.Duel;
using NUnit.Framework;

/// <summary>
/// Regresion pura de GOD de dos fases: transicion unica, cooldowns,
/// Juicio, Borrado/huerfana y reconstruccion por imposibilidad estructural.
/// Sin UnityEngine ni UI: solo el modelo.
/// </summary>
public sealed class MementoGodDuelRulesTests
{
    private const int Revision = 3;

    private static List<GodBoardCard> Board(params int[] pairIds)
    {
        var cards = new List<GodBoardCard>(pairIds.Length);
        for (int i = 0; i < pairIds.Length; i++)
            cards.Add(new GodBoardCard(new DuelCardId(Revision, i), pairIds[i]));
        return cards;
    }

    private static List<GodBoardCard> WithStatus(
        List<GodBoardCard> cards,
        int ordinal,
        GodBoardCardStatus status)
    {
        var copy = new List<GodBoardCard>(cards);
        copy[ordinal] = copy[ordinal].WithStatus(status);
        return copy;
    }

    private static GodDuelState Started(int health = 10)
    {
        var state = new GodDuelState();
        state.Begin(health, Revision);
        return state;
    }

    [Test]
    public void PhaseTwo_CrossesExactlyAtHalf_AndOnlyOnce()
    {
        var state = Started(10);
        Assert.That(state.Phase, Is.EqualTo(GodDuelPhase.PhaseOne));
        Assert.That(state.TryEnterPhaseTwo(6, false), Is.False, "60% must not cross.");
        Assert.That(state.TryEnterPhaseTwo(5, false), Is.True, "50% must cross.");
        Assert.That(state.Phase, Is.EqualTo(GodDuelPhase.PhaseTwo));
        Assert.That(state.TryEnterPhaseTwo(5, false), Is.False, "The transition happens once.");
        Assert.That(state.TryEnterPhaseTwo(1, false), Is.False);
    }

    [Test]
    public void PhaseTwo_OvershootByComboStillCrosses()
    {
        var state = Started(10);
        Assert.That(state.TryEnterPhaseTwo(2, false), Is.True, "A big hit below the threshold must cross.");
    }

    [Test]
    public void PhaseTwo_NeverOverridesLethalOrTerminalDamage()
    {
        var lethal = Started(10);
        Assert.That(lethal.TryEnterPhaseTwo(0, false), Is.False, "Lethal damage wins: no resurrection for the transition.");
        Assert.That(lethal.Phase, Is.EqualTo(GodDuelPhase.PhaseOne));

        var terminal = Started(10);
        Assert.That(terminal.TryEnterPhaseTwo(4, true), Is.False, "A terminal duel never transitions.");
    }

    [Test]
    public void PhaseTwo_ArmsEraseOnItsOwnCooldown()
    {
        var state = Started(10);
        Assert.That(state.IsEraseReady, Is.False, "Erase belongs to phase two only.");
        state.TryEnterPhaseTwo(4, false);
        Assert.That(state.EraseTurnsUntilReady, Is.EqualTo(GodDuelRules.EraseCooldownTurns));
        for (int turn = 0; turn < GodDuelRules.EraseCooldownTurns; turn++)
            state.BeginOwnerTurn();
        Assert.That(state.IsEraseReady, Is.True);
        Assert.That(state.CommitErase(), Is.True);
        Assert.That(state.IsEraseReady, Is.False);
        Assert.That(state.CommitErase(), Is.False, "No stacking two attacks in the same turn.");
    }

    [Test]
    public void Judgement_ReducesOncePerOwnerTurn_AndNeverStacks()
    {
        var state = Started(10);
        Assert.That(state.IsJudgementReady, Is.False);
        Assert.That(state.JudgementTurnsUntilReady, Is.EqualTo(GodDuelRules.JudgementCooldownTurns));

        for (int turn = 0; turn < GodDuelRules.JudgementCooldownTurns; turn++)
        {
            Assert.That(state.IsJudgementReady, Is.False, "turn " + turn);
            state.BeginOwnerTurn();
        }

        Assert.That(state.IsJudgementReady, Is.True);
        Assert.That(state.CommitJudgement(), Is.True);
        Assert.That(state.CommitJudgement(), Is.False, "One cast per turn.");
        Assert.That(state.JudgementTurnsUntilReady, Is.EqualTo(GodDuelRules.JudgementCooldownTurns));
    }

    [Test]
    public void Judgement_ChoosesAtMostTwoCompletePairs()
    {
        var cards = Board(1, 1, 2, 2, 3, 3);
        var random = new SystemDuelRandom(7);
        Assert.That(GodDuelRules.TryChooseJudgementTargets(Revision, cards, random, out List<GodBoardCard> targets), Is.True);
        Assert.That(targets.Count, Is.EqualTo(GodDuelRules.JudgementMaxPairs * 2));

        var pairIds = new HashSet<int>();
        foreach (GodBoardCard target in targets)
            pairIds.Add(target.PairId);
        Assert.That(pairIds.Count, Is.EqualTo(GodDuelRules.JudgementMaxPairs), "Two different pairs, never four cards of one.");
    }

    [Test]
    public void Judgement_SingleEligiblePairMarksOnlyThatPair()
    {
        var cards = Board(1, 1, 2, 2);
        cards = WithStatus(cards, 2, GodBoardCardStatus.Resolved);
        var random = new SystemDuelRandom(7);
        Assert.That(GodDuelRules.TryChooseJudgementTargets(Revision, cards, random, out List<GodBoardCard> targets), Is.True);
        Assert.That(targets.Count, Is.EqualTo(2));
        Assert.That(targets[0].PairId, Is.EqualTo(1));
    }

    [Test]
    public void Judgement_NoEligiblePairIsNotPrepared()
    {
        var cards = Board(1, 1);
        cards = WithStatus(cards, 0, GodBoardCardStatus.Erased);
        cards = WithStatus(cards, 1, GodBoardCardStatus.Orphaned);
        Assert.That(GodDuelRules.TryChooseJudgementTargets(Revision, cards, new SystemDuelRandom(1), out List<GodBoardCard> targets), Is.False);
        Assert.That(targets, Is.Empty);
    }

    [Test]
    public void Judgement_StrikesOnlyStillValidPairs_AndNeverSubstitutes()
    {
        var cards = Board(1, 1, 2, 2);
        var judgement = new GodJudgementState();
        judgement.BeginBoard(Revision);
        Assert.That(judgement.Arm(Revision, new List<GodBoardCard>
        {
            cards[0], cards[1], cards[2], cards[3]
        }), Is.True);
        Assert.That(judgement.TargetPairCount, Is.EqualTo(2));

        var afterPlayer = WithStatus(cards, 0, GodBoardCardStatus.Resolved);
        afterPlayer = WithStatus(afterPlayer, 1, GodBoardCardStatus.Resolved);
        Assert.That(judgement.TryResolve(afterPlayer, out List<GodBoardCard> struck), Is.True);
        Assert.That(struck.Count, Is.EqualTo(2), "Only the pair still complete is struck.");
        Assert.That(struck[0].PairId, Is.EqualTo(2));
        Assert.That(judgement.IsArmed, Is.False);
    }

    [Test]
    public void Judgement_RejectsOddOrNonLiveMarks()
    {
        var cards = Board(1, 1, 2, 2);
        var judgement = new GodJudgementState();
        judgement.BeginBoard(Revision);

        Assert.That(judgement.Arm(Revision, new List<GodBoardCard> { cards[0] }), Is.False, "Odd marks.");
        var nonLive = WithStatus(cards, 0, GodBoardCardStatus.Resolved);
        Assert.That(judgement.Arm(Revision, new List<GodBoardCard> { nonLive[0], cards[1] }), Is.False, "Only live cards are marked.");
        Assert.That(judgement.IsArmed, Is.False);
    }

    [Test]
    public void Judgement_StaleRevisionIsRejected()
    {
        var judgement = new GodJudgementState();
        judgement.BeginBoard(Revision);
        var stale = new List<GodBoardCard>
        {
            new GodBoardCard(new DuelCardId(Revision + 1, 0), 1),
            new GodBoardCard(new DuelCardId(Revision + 1, 1), 1)
        };
        Assert.That(() => judgement.Arm(Revision, stale), Throws.TypeOf<System.InvalidOperationException>());
    }

    [Test]
    public void Erase_SavedWhenPlayerRemovesTheTarget_AndNeverRedirects()
    {
        var cards = Board(1, 1, 2, 2);
        var erase = new GodEraseState();
        erase.BeginBoard(Revision);
        Assert.That(erase.Arm(Revision, new DuelCardId(Revision, 0), cards), Is.True);

        var savedBoard = WithStatus(cards, 0, GodBoardCardStatus.Resolved);
        savedBoard = WithStatus(savedBoard, 1, GodBoardCardStatus.Resolved);
        Assert.That(erase.TryResolve(savedBoard, out List<GodBoardCard> updated, out bool saved), Is.False);
        Assert.That(saved, Is.True);
        Assert.That(erase.IsArmed, Is.False, "A failed ray is not re-armed on another card.");
        Assert.That(updated.Count, Is.EqualTo(savedBoard.Count));
        foreach (GodBoardCard card in updated)
            Assert.That(card.Status, Is.Not.EqualTo(GodBoardCardStatus.Erased));
    }

    [Test]
    public void Erase_LeavesAnUnambiguousOrphan_DistinctFromResolved()
    {
        var cards = Board(1, 1, 2, 2);
        var erase = new GodEraseState();
        erase.BeginBoard(Revision);
        Assert.That(erase.Arm(Revision, new DuelCardId(Revision, 0), cards), Is.True);

        Assert.That(erase.TryResolve(cards, out List<GodBoardCard> updated, out bool saved), Is.True);
        Assert.That(saved, Is.False);
        Assert.That(updated[0].Status, Is.EqualTo(GodBoardCardStatus.Erased));
        Assert.That(updated[1].Status, Is.EqualTo(GodBoardCardStatus.Orphaned));
        Assert.That(updated[1].Status, Is.Not.EqualTo(GodBoardCardStatus.Resolved));
        Assert.That(updated[1].Status, Is.Not.EqualTo(GodBoardCardStatus.Live));
        Assert.That(GodDuelRules.HasStructuralPair(Revision, updated), Is.True, "The untouched pair remains playable.");
    }

    [Test]
    public void Erase_RejectsAlreadyMatchedOrErasedTargets()
    {
        var cards = Board(1, 1);
        var erase = new GodEraseState();
        erase.BeginBoard(Revision);

        var resolved = WithStatus(cards, 0, GodBoardCardStatus.Resolved);
        Assert.That(erase.Arm(Revision, new DuelCardId(Revision, 0), resolved), Is.False);
        var erased = WithStatus(cards, 0, GodBoardCardStatus.Erased);
        Assert.That(erase.Arm(Revision, new DuelCardId(Revision, 0), erased), Is.False);
        Assert.That(erase.IsArmed, Is.False);
    }

    [Test]
    public void Erase_TargetFromAnotherBoardIsRejected()
    {
        var cards = Board(1, 1);
        var erase = new GodEraseState();
        erase.BeginBoard(Revision);
        Assert.That(
            () => erase.Arm(Revision + 1, new DuelCardId(Revision + 1, 0), cards),
            Throws.TypeOf<System.InvalidOperationException>());
    }

    [Test]
    public void Rebuild_DecidedByStructuralImpossibility_NotByTemporaryBlocks()
    {
        var state = Started(10);
        var playable = Board(1, 1, 2, 2);
        Assert.That(state.NeedsRebuild(Revision, playable), Is.False, "A temporary curtain or selection is not a card state.");

        var exhausted = WithStatus(playable, 0, GodBoardCardStatus.Resolved);
        exhausted = WithStatus(exhausted, 1, GodBoardCardStatus.Resolved);
        exhausted = WithStatus(exhausted, 2, GodBoardCardStatus.Resolved);
        exhausted = WithStatus(exhausted, 3, GodBoardCardStatus.Resolved);
        Assert.That(state.NeedsRebuild(Revision, exhausted), Is.True);

        var broken = Board(1, 1);
        broken = WithStatus(broken, 0, GodBoardCardStatus.Erased);
        broken = WithStatus(broken, 1, GodBoardCardStatus.Orphaned);
        Assert.That(state.NeedsRebuild(Revision, broken), Is.True, "Cards remain but no identity keeps two live cards.");
    }

    [Test]
    public void Rebuild_PlanComesBeforeSkillsAndTerminalWins()
    {
        var state = Started(10);
        var broken = new List<GodBoardCard>
        {
            new GodBoardCard(new DuelCardId(Revision, 0), 1, GodBoardCardStatus.Erased),
            new GodBoardCard(new DuelCardId(Revision, 1), 1, GodBoardCardStatus.Orphaned)
        };
        Assert.That(state.PlanOwnerTurn(Revision, broken, false), Is.EqualTo(GodOwnerTurnPlan.Rebuild));
        Assert.That(state.PlanOwnerTurn(Revision, broken, true), Is.EqualTo(GodOwnerTurnPlan.None), "Terminal results win.");
    }

    [Test]
    public void Rebuild_PhaseOneExhaustionUsesRebuild_NotSuddenDeath()
    {
        var state = Started(4);
        var exhausted = new List<GodBoardCard>
        {
            new GodBoardCard(new DuelCardId(Revision, 0), 1, GodBoardCardStatus.Resolved),
            new GodBoardCard(new DuelCardId(Revision, 1), 1, GodBoardCardStatus.Resolved)
        };
        Assert.That(state.Phase, Is.EqualTo(GodDuelPhase.PhaseOne));
        Assert.That(state.PlanOwnerTurn(Revision, exhausted, false), Is.EqualTo(GodOwnerTurnPlan.Rebuild));
    }

    [Test]
    public void Rebuild_GuardRejectsStaleOrDuplicateRevision()
    {
        var state = Started(10);
        Assert.That(state.BeginRebuild(Revision + 1, out int firstSerial), Is.True);
        Assert.That(firstSerial, Is.EqualTo(1));
        Assert.That(state.BoardRevision, Is.EqualTo(Revision + 1));
        Assert.That(state.BeginRebuild(Revision + 1, out int duplicateSerial), Is.False, "The same revision never rebuilds twice.");
        Assert.That(duplicateSerial, Is.EqualTo(1));
        Assert.That(state.BeginRebuild(Revision, out _), Is.False, "An older revision is stale.");
        Assert.That(state.BeginRebuild(Revision + 2, out int secondSerial), Is.True);
        Assert.That(secondSerial, Is.EqualTo(2));
    }

    [Test]
    public void Rebuild_KeepsPhaseAndCooldownProgress()
    {
        var state = Started(10);
        state.TryEnterPhaseTwo(5, false);
        state.BeginOwnerTurn();
        int eraseBefore = state.EraseTurnsUntilReady;
        Assert.That(state.BeginRebuild(Revision + 1, out _), Is.True);
        Assert.That(state.Phase, Is.EqualTo(GodDuelPhase.PhaseTwo), "A rebuild never resets the phase.");
        Assert.That(state.EraseTurnsUntilReady, Is.EqualTo(eraseBefore), "A rebuild never resets cooldowns.");
    }

    [Test]
    public void PlanOwnerTurn_ReturnsOneSingleActionPerTurn()
    {
        var state = Started(10);
        var playable = Board(1, 1, 2, 2);
        Assert.That(state.PlanOwnerTurn(Revision, playable, false), Is.EqualTo(GodOwnerTurnPlan.NormalPlay));

        for (int turn = 0; turn < GodDuelRules.JudgementCooldownTurns; turn++)
            state.BeginOwnerTurn();
        Assert.That(state.PlanOwnerTurn(Revision, playable, false), Is.EqualTo(GodOwnerTurnPlan.Judgement));

        state.CommitJudgement();
        Assert.That(state.PlanOwnerTurn(Revision, playable, false), Is.EqualTo(GodOwnerTurnPlan.NormalPlay), "Cooldown prevents a second skill.");
    }

    [Test]
    public void StaleRevision_IsRejectedEverywhere()
    {
        var cards = Board(1, 1);
        Assert.That(() => GodDuelRules.HasStructuralPair(Revision + 1, cards), Throws.TypeOf<System.InvalidOperationException>());
        Assert.That(
            () => GodDuelRules.TryChooseEraseTarget(Revision + 1, cards, null, new SystemDuelRandom(1), out _),
            Throws.TypeOf<System.InvalidOperationException>());
    }
}
