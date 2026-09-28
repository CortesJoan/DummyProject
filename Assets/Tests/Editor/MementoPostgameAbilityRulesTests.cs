using System.Collections.Generic;
using System.Linq;
using AnimalMemory.Progression;
using MementoMatch.Duel;
using NUnit.Framework;
using UnityEngine;

public sealed class MementoPostgameAbilityRulesTests
{
    [Test]
    public void CooldownsBecomeReadyOnContractTurn_AndInvalidCastDoesNotSpend()
    {
        var clock = new PostgameAbilityClock();
        clock.Reset(MementoPostgameAbilityId.NewHand);
        for (int turn = 1; turn < PostgameAbilityRules.NewHandCooldownTurns; turn++)
            Assert.That(clock.BeginOwnerTurn(), Is.False, "turn " + turn);
        Assert.That(clock.BeginOwnerTurn(), Is.True);
        Assert.That(clock.CommitCast(), Is.True);
        Assert.That(clock.TurnsUntilReady, Is.EqualTo(PostgameAbilityRules.NewHandCooldownTurns));

        clock.Reset(MementoPostgameAbilityId.Curtain);
        for (int turn = 1; turn <= PostgameAbilityRules.CurtainCooldownTurns; turn++)
            clock.BeginOwnerTurn();
        Assert.That(clock.IsReady, Is.True);
        Assert.That(clock.BeginOwnerTurn(), Is.True);
        Assert.That(clock.TurnsUntilReady, Is.Zero);
    }

    [Test]
    public void NewHandShufflesStableIds_AndRequiresUsefulMix()
    {
        var ids = Enumerable.Range(0, 6).Select(i => new DuelCardId(3, i)).ToList();
        var pairs = new List<int> { 0, 0, 1, 1, 2, 2 };
        Assert.That(PostgameAbilityRules.TryBuildNewHandOrder(
            ids, pairs, new SystemDuelRandom(12345), out List<DuelCardId> shuffled), Is.True);
        Assert.That(shuffled.Select(x => x.Ordinal), Is.EquivalentTo(ids.Select(x => x.Ordinal)));
        Assert.That(shuffled.Select(x => x.Ordinal).SequenceEqual(ids.Select(x => x.Ordinal)), Is.False);
        Assert.That(shuffled.All(x => x.BoardRevision == 3), Is.True);

        Assert.That(PostgameAbilityRules.TryBuildNewHandOrder(
            new[] { new DuelCardId(3, 0), new DuelCardId(3, 1) },
            new[] { 0, 0 }, new SystemDuelRandom(1), out _), Is.False);
    }

    [Test]
    public void AbilitySeedIsDeterministicAndSeparatedByActivation()
    {
        int first = PostgameAbilityRules.DeriveAbilitySeed(99, 2, 0);
        Assert.That(PostgameAbilityRules.DeriveAbilitySeed(99, 2, 0), Is.EqualTo(first));
        Assert.That(PostgameAbilityRules.DeriveAbilitySeed(99, 2, 1), Is.Not.EqualTo(first));
        Assert.That(PostgameAbilityRules.DeriveAbilitySeed(99, 3, 0), Is.Not.EqualTo(first));
    }

    [Test]
    public void CurtainSelectsOnlyRowThatLeavesCompletePairOutside()
    {
        Assert.That(PostgameAbilityRules.TryChooseCurtainRow(
            2, 3, new[] { 0, 1, 2, 3 }, new[] { 0, 0, 1, 1, 2, 2 },
            new SystemDuelRandom(7), out int row), Is.True);
        Assert.That(row, Is.EqualTo(1));
    }

    [Test]
    public void CurtainRejectsCastWhenNoCompletePairWouldRemainOutside()
    {
        Assert.That(PostgameAbilityRules.TryChooseCurtainRow(
            2, 2, new[] { 0, 3 }, new[] { 0, 1, 2, 3 },
            new SystemDuelRandom(1), out int row), Is.False);
        Assert.That(row, Is.EqualTo(-1));
    }

    [Test]
    public void CurtainBlocksExactlyNextTargetAttempt()
    {
        var curtain = new PostgameCurtainState();
        curtain.BeginBoard(4);
        curtain.Arm(4, 1, DuelActor.Player);
        Assert.That(curtain.Blocks(new DuelCardId(4, 0), 2, 2, DuelActor.Player), Is.True);
        Assert.That(curtain.RegisterAttempt(DuelActor.Opponent), Is.False);
        Assert.That(curtain.IsActive, Is.True);
        Assert.That(curtain.RegisterAttempt(DuelActor.Player), Is.True);
        Assert.That(curtain.IsActive, Is.False);
    }

    [Test]
    public void TestimonyCoversOneAttemptPerActor_ThenCapturesFirstMismatch()
    {
        var testimony = new PostgameTestimonyState();
        testimony.BeginBoard(5);
        testimony.ArmRound(5);
        var playerA = new DuelCardId(5, 0);
        var playerB = new DuelCardId(5, 1);
        var opponentA = new DuelCardId(5, 2);
        var opponentB = new DuelCardId(5, 3);

        Assert.That(testimony.RegisterAttempt(DuelActor.Player, true, playerA, playerB), Is.False);
        Assert.That(testimony.IsWindowArmed, Is.True);
        Assert.That(testimony.RegisterAttempt(DuelActor.Player, false, playerA, playerB), Is.False);
        Assert.That(testimony.PublicCardCount, Is.Zero);

        Assert.That(testimony.RegisterAttempt(DuelActor.Opponent, false, opponentA, opponentB), Is.True);
        Assert.That(testimony.IsWindowArmed, Is.False);
        Assert.That(testimony.PublicCardCount, Is.EqualTo(2));
        Assert.That(testimony.IsPublic(opponentA), Is.True);
        Assert.That(testimony.IsPublic(opponentB), Is.True);
        testimony.ResolveMatched(opponentA, opponentB);
        Assert.That(testimony.PublicCardCount, Is.Zero);
    }

    [Test]
    public void TestimonyVisibilityClearsOnNewBoard()
    {
        var testimony = new PostgameTestimonyState();
        testimony.BeginBoard(1);
        testimony.ArmRound(1);
        var first = new DuelCardId(1, 4);
        var second = new DuelCardId(1, 5);
        testimony.RegisterAttempt(DuelActor.Opponent, false, first, second);
        Assert.That(testimony.PublicCardCount, Is.EqualTo(2));

        testimony.BeginBoard(2);
        Assert.That(testimony.PublicCardCount, Is.Zero);
        Assert.That(testimony.IsWindowArmed, Is.False);
        Assert.That(testimony.IsPublic(first), Is.False);
    }

    [Test]
    public void BoardBoundAbilityStateResetClearsRevisionAndVisibility()
    {
        var curtain = new PostgameCurtainState();
        curtain.BeginBoard(7);
        curtain.Arm(7, 1, DuelActor.Player);
        curtain.Reset();
        Assert.That(curtain.BoardRevision, Is.Zero);
        Assert.That(curtain.IsActive, Is.False);

        var testimony = new PostgameTestimonyState();
        testimony.BeginBoard(7);
        testimony.ArmRound(7);
        var first = new DuelCardId(7, 2);
        var second = new DuelCardId(7, 3);
        Assert.That(testimony.RegisterAttempt(DuelActor.Player, false, first, second), Is.True);
        testimony.Reset();
        Assert.That(testimony.BoardRevision, Is.Zero);
        Assert.That(testimony.IsWindowArmed, Is.False);
        Assert.That(testimony.PublicCardCount, Is.Zero);
        Assert.That(testimony.IsPublic(first), Is.False);
    }

    [Test]
    public void BrainCanForgetHiddenPositionsWithoutChangingBoardRevision()
    {
        var brain = new DuelOpponentBrain(new SystemDuelRandom(3), 0d);
        brain.BeginBoard(9);
        brain.Observe(new DuelCardId(9, 2), 1);
        brain.Observe(new DuelCardId(9, 5), 3);
        Assert.That(brain.KnownCardCount, Is.EqualTo(2));
        brain.ForgetObservedCards();
        Assert.That(brain.BoardRevision, Is.EqualTo(9));
        Assert.That(brain.KnownCardCount, Is.Zero);
    }

    [Test]
    public void CardMatchUiBindsOnlyDecidedPostgameAbility()
    {
        var host = new GameObject("postgame ability binding test");
        host.SetActive(false);
        var ui = host.AddComponent<CardMatchUI>();

        ui.ConfigureDuel(7, 123);
        Assert.That(ui.DuelOpponentAbilityId, Is.EqualTo(MementoPostgameAbilityId.NewHand));
        Assert.That(ui.DuelOpponentAbilityName, Is.EqualTo("Nueva mano"));

        ui.ConfigureDuel(8, 123);
        Assert.That(ui.DuelOpponentAbilityId, Is.EqualTo(MementoPostgameAbilityId.Unassigned));
        Assert.That(ui.DuelOpponentAbilityName, Is.Empty);

        ui.ConfigureDuel(9, 123);
        Assert.That(ui.DuelOpponentAbilityId, Is.EqualTo(MementoPostgameAbilityId.Unassigned));

        Object.DestroyImmediate(host);
    }
}
