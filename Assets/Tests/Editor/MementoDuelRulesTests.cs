using System.Collections;
using System.Collections.Generic;
using AnimalMemory.Progression;
using MementoMatch.Duel;
using NUnit.Framework;
using System.Reflection;
using UnityEngine;

public sealed class MementoDuelRulesTests
{
    [Test]
    public void DuelAlternatesAfterMatchesAndMisses()
    {
        DuelState state = new DuelState();
        state.Begin(AnimalMemoryContentIds.MikaGuide);

        state.CompleteTurn(DuelActor.Player, true);
        Assert.That(state.PlayerPairs, Is.EqualTo(1));
        Assert.That(state.ActiveActor, Is.EqualTo(DuelActor.Opponent));

        state.CompleteTurn(DuelActor.Opponent, false);
        Assert.That(state.OpponentPairs, Is.Zero);
        Assert.That(state.ActiveActor, Is.EqualTo(DuelActor.Player));
    }

    [Test]
    public void MikaDuelEndsAtStrictMajority()
    {
        DuelState state = new DuelState();
        state.Begin(AnimalMemoryContentIds.MikaGuide);

        for (int pair = 0; pair < 4; pair++)
        {
            state.CompleteTurn(DuelActor.Player, true);
            state.CompleteTurn(DuelActor.Opponent, false);
        }

        Assert.That(state.Winner, Is.Null);
        state.CompleteTurn(DuelActor.Player, true);
        Assert.That(state.Winner, Is.EqualTo(DuelActor.Player));
        Assert.That(state.TargetPairs, Is.EqualTo(5));
    }

    [Test]
    public void YoruDuelRequiresSixPairs()
    {
        DuelState state = new DuelState();
        state.Begin(AnimalMemoryContentIds.YoruGuide);
        Assert.That(state.TargetPairs, Is.EqualTo(6));
    }

    [Test]
    public void AkiDuelIsAValidFourHealthStoryBattle()
    {
        DuelState state = new DuelState();
        state.Begin(AnimalMemoryContentIds.AkiGuide);
        Assert.That(state.TargetPairs, Is.EqualTo(4));
    }

    [Test]
    public void UndoPlayerPairRestoresDuelHealthWithoutChangingTurn()
    {
        DuelState state = new DuelState();
        state.Begin(AnimalMemoryContentIds.AkiGuide);
        state.CompleteTurn(DuelActor.Player, true);
        state.CompleteTurn(DuelActor.Opponent, false);

        Assert.That(state.OpponentHealth, Is.EqualTo(3));
        Assert.That(state.UndoPlayerPair(), Is.True);
        Assert.That(state.PlayerPairs, Is.Zero);
        Assert.That(state.OpponentHealth, Is.EqualTo(4));
        Assert.That(state.ActiveActor, Is.EqualTo(DuelActor.Player));
        Assert.That(state.Winner, Is.Null);
    }

    [Test]
    public void DuelSeedDerivationIsDeterministicAndSeparatesStreams()
    {
        int boardSeed = DuelRules.DeriveBoardSeed(12345, 1);
        Assert.That(DuelRules.DeriveBoardSeed(12345, 1), Is.EqualTo(boardSeed));
        Assert.That(DuelRules.DeriveBoardSeed(12345, 2), Is.Not.EqualTo(boardSeed));
        Assert.That(DuelRules.DeriveBrainSeed(12345, 1), Is.Not.EqualTo(boardSeed));

        SystemDuelRandom boardA = new SystemDuelRandom(boardSeed);
        SystemDuelRandom boardB = new SystemDuelRandom(boardSeed);
        for (int i = 0; i < 8; i++)
            Assert.That(boardA.Range(0, 1000), Is.EqualTo(boardB.Range(0, 1000)));
    }

    [Test]
    public void BrainDropsMemoryOnNewBoardRevisionAndRejectsStaleCardIds()
    {
        DuelOpponentBrain brain = new DuelOpponentBrain(
            new FixedRandom(new[] { 0 }),
            0d);
        DuelCardId firstBoardCard = new DuelCardId(1, 2);
        brain.BeginBoard(1);
        brain.Observe(firstBoardCard, 4);
        Assert.That(brain.KnownCardCount, Is.EqualTo(1));

        brain.BeginBoard(2);
        Assert.That(brain.KnownCardCount, Is.Zero);
        Assert.Throws<System.InvalidOperationException>(
            () => brain.Observe(firstBoardCard, 4));

        brain.Observe(new DuelCardId(2, 2), 4);
        brain.BeginBoard(2);
        Assert.That(brain.KnownCardCount, Is.EqualTo(1));
    }

    [Test]
    public void BrainDoesNotKnowUnobservedCards()
    {
        DuelOpponentBrain brain = new DuelOpponentBrain(
            new FixedRandom(new[] { 0 }),
            0d);

        Assert.That(brain.KnownCardCount, Is.Zero);
        int choice = brain.ChooseFirst(new[] { 3, 7, 9 });

        Assert.That(choice, Is.EqualTo(3));
        Assert.That(brain.KnownCardCount, Is.Zero);
        Assert.That(brain.KnowsCard(3), Is.False);
    }

    [Test]
    public void BrainPrioritizesOnlyAnObservedPair()
    {
        DuelOpponentBrain brain = new DuelOpponentBrain(
            new FixedRandom(new[] { 0 }),
            0d);
        brain.Observe(2, 4);
        brain.Observe(6, 4);
        brain.Observe(9, 1);

        Assert.That(brain.ChooseFirst(new[] { 2, 6, 9 }), Is.EqualTo(2));
        Assert.That(brain.ChooseSecond(2, new[] { 2, 6, 9 }), Is.EqualTo(6));
    }

    [Test]
    public void InitialMemoryKeepsSingletonsInsteadOfReadyPairs()
    {
        DuelOpponentBrain brain = new DuelOpponentBrain(
            new FixedRandom(new[] { 0, 0, 0 }),
            0d);
        List<int> available = new List<int>();
        for (int card = 0; card < 12; card++)
        {
            brain.Observe(card, card / 2);
            available.Add(card);
        }

        brain.TrimInitialMemory(3);

        Assert.That(brain.KnownCardCount, Is.EqualTo(3));
        Assert.That(brain.TryFindKnownPair(available, out _, out _), Is.False);
    }

    [Test]
    public void InitialMemoryNeverStartsWithSolvedPairsAcrossManySeeds()
    {
        List<int> available = new List<int>();
        for (int card = 0; card < 20; card++)
            available.Add(card);

        for (int seed = 0; seed < 100; seed++)
        {
            DuelOpponentBrain brain = new DuelOpponentBrain(
                new SystemDuelRandom(seed),
                0d);
            for (int card = 0; card < available.Count; card++)
                brain.Observe(card, card / 2);

            brain.TrimInitialMemory(6);

            Assert.That(
                brain.TryFindKnownPair(available, out _, out _),
                Is.False,
                $"Seed {seed} retained a solved pair.");
        }
    }

    [Test]
    public void PeekTargetsAreUniqueAndLegal()
    {
        DuelOpponentBrain brain = new DuelOpponentBrain(
            new FixedRandom(new[] { 1, 0, 0 }),
            0d);

        IReadOnlyList<int> targets = brain.ChoosePeekTargets(new[] { 2, 4, 8 }, 3);

        Assert.That(targets, Is.EquivalentTo(new[] { 2, 4, 8 }));
        Assert.That(new HashSet<int>(targets).Count, Is.EqualTo(3));
    }

    [Test]
    public void BeginCapsTargetToActualBoardPairs()
    {
        DuelState state = new DuelState();
        state.Begin(AnimalMemoryContentIds.MomoGuide, 3);
        Assert.That(state.TargetPairs, Is.EqualTo(3));
    }

    [Test]
    public void ExhaustedTieRequestsSuddenDeathAndNeverAwardsDefeat()
    {
        DuelState state = new DuelState();
        state.Begin(AnimalMemoryContentIds.MikaGuide, 2);
        state.CompleteTurn(DuelActor.Player, true);
        state.CompleteTurn(
            DuelActor.Opponent,
            true,
            boardExhausted: true);

        Assert.That(state.Winner, Is.Null);
        Assert.That(state.RequiresSuddenDeath, Is.True);
        Assert.Throws<System.InvalidOperationException>(
            () => state.CompleteTurn(DuelActor.Player, false));

        state.BeginSuddenDeathRound();
        Assert.That(state.RequiresSuddenDeath, Is.False);
        Assert.That(state.ActiveActor, Is.EqualTo(DuelActor.Player));
    }

    [Test]
    public void FinalCompletedTurnResolvesExhaustedBoardWithoutSecondRulesCall()
    {
        DuelState state = new DuelState();
        state.Begin(AnimalMemoryContentIds.MikaGuide, 3);
        state.CompleteTurn(DuelActor.Player, true);
        state.CompleteTurn(DuelActor.Opponent, true);
        state.CompleteTurn(DuelActor.Player, false);
        state.CompleteTurn(
            DuelActor.Opponent,
            true,
            boardExhausted: true);

        Assert.That(state.PlayerHealth, Is.EqualTo(1));
        Assert.That(state.OpponentHealth, Is.EqualTo(2));
        Assert.That(state.Winner, Is.EqualTo(DuelActor.Opponent));
        Assert.That(
            DuelRules.GetTerminalAction(state),
            Is.EqualTo(DuelTerminalAction.PlayerDefeat));
    }

    [Test]
    public void LethalDamageWinsBeforeExhaustionCanRequestSuddenDeath()
    {
        DuelState state = new DuelState();
        state.Begin(AnimalMemoryContentIds.MikaGuide, 1);

        state.CompleteTurn(
            DuelActor.Player,
            true,
            boardExhausted: true);

        Assert.That(state.Winner, Is.EqualTo(DuelActor.Player));
        Assert.That(state.RequiresSuddenDeath, Is.False);
        Assert.That(
            DuelRules.GetTerminalAction(state),
            Is.EqualTo(DuelTerminalAction.PlayerVictory));
    }

    [Test]
    public void OpponentBonusPairCanCloseAnExhaustedBoardAtomically()
    {
        DuelState state = new DuelState();
        state.Begin(AnimalMemoryContentIds.UltimateMasterOpponent, 2);
        state.CompleteTurn(DuelActor.Player, false);

        state.ApplyOpponentBonusPair(boardExhausted: true);

        Assert.That(state.PlayerHealth, Is.EqualTo(1));
        Assert.That(state.OpponentHealth, Is.EqualTo(2));
        Assert.That(state.Winner, Is.EqualTo(DuelActor.Opponent));
    }

    [Test]
    public void UltimateMasterEmptyBoardRoutesToDefeatWhenPlayerTakesLastPairButTrails()
    {
        DuelState state = new DuelState();
        state.Begin(AnimalMemoryContentIds.UltimateMasterOpponent, 15);

        // Rei claims thirteen pairs. The player claims the fifteenth and final
        // board pair, but still trails 2-13 with neither health bar at zero.
        state.CompleteTurn(DuelActor.Player, true);
        state.CompleteTurn(DuelActor.Opponent, true);
        for (int pair = 0; pair < 12; pair++)
        {
            state.CompleteTurn(DuelActor.Player, false);
            state.CompleteTurn(DuelActor.Opponent, true);
        }
        state.CompleteTurn(DuelActor.Player, true);

        Assert.That(state.PlayerPairs, Is.EqualTo(2));
        Assert.That(state.OpponentPairs, Is.EqualTo(13));
        Assert.That(state.PlayerHealth, Is.EqualTo(2));
        Assert.That(state.OpponentHealth, Is.EqualTo(13));
        Assert.That(state.Winner, Is.Null);

        state.ResolveBoardExhausted(DuelActor.Player);

        Assert.That(state.Winner, Is.EqualTo(DuelActor.Opponent));
        Assert.That(DuelRules.GetTerminalAction(state),
            Is.EqualTo(DuelTerminalAction.PlayerDefeat));
    }

    [Test]
    public void CardMatchUiPresentsResolvedDefeatInsteadOfStartingAnotherTurn()
    {
        GameObject host = new GameObject("Duel terminal presentation test");
        host.SetActive(false);
        CardMatchUI ui = host.AddComponent<CardMatchUI>();
        bool? presentedAsPlayerWin = null;
        ui.DuelOutcomePresentationRequested +=
            (_, playerWon) => presentedAsPlayerWin = playerWon;
        MethodInfo present = typeof(CardMatchUI).GetMethod(
            "PresentDuelOutcome",
            BindingFlags.Instance | BindingFlags.NonPublic);

        IEnumerator routine = (IEnumerator)present.Invoke(
            ui,
            new object[] { DuelTerminalAction.PlayerDefeat });
        Assert.That(routine.MoveNext(), Is.True);
        Assert.That(presentedAsPlayerWin, Is.False);
        Assert.That(ui.DuelOpponentPose, Is.EqualTo(4));

        Object.DestroyImmediate(host);
    }

    [Test]
    public void TerminalRoutingKeepsTrueTieInSuddenDeath()
    {
        DuelState state = new DuelState();
        state.Begin(AnimalMemoryContentIds.AkiGuide, 4);
        state.CompleteTurn(DuelActor.Player, true);
        state.CompleteTurn(DuelActor.Opponent, true);
        state.ResolveBoardExhausted(DuelActor.Opponent);

        Assert.That(DuelRules.GetTerminalAction(state),
            Is.EqualTo(DuelTerminalAction.SuddenDeath));
    }

    [TestCase(0, 1)]
    [TestCase(1, 1)]
    [TestCase(2, 2)]
    [TestCase(3, 3)]
    [TestCase(8, 3)]
    public void ProtagonistComboDamageIsCappedAndPredictable(int combo, int damage)
    {
        Assert.That(DuelRules.GetPlayerComboDamage(combo), Is.EqualTo(damage));
    }

    [Test]
    public void PlayerComboScalesDamageButOpponentDamageStaysAtOne()
    {
        DuelState state = new DuelState();
        state.Begin(AnimalMemoryContentIds.YoruGuide);
        state.CompleteTurn(DuelActor.Player, true, false, damage: 3);
        Assert.That(state.OpponentHealth, Is.EqualTo(3));
        state.CompleteTurn(DuelActor.Opponent, true, false, damage: 99);
        Assert.That(state.PlayerHealth, Is.EqualTo(5));
    }

    [Test]
    public void MomoEncoreRetainsPlayerTurn()
    {
        DuelState state = new DuelState();
        state.Begin(AnimalMemoryContentIds.MomoGuide, 10);
        state.CompleteTurn(DuelActor.Player, true, true);
        Assert.That(state.PlayerPairs, Is.EqualTo(1));
        Assert.That(state.ActiveActor, Is.EqualTo(DuelActor.Player));
    }

    [TestCase(AnimalMemoryContentIds.HanaGuide, 6)]
    [TestCase(AnimalMemoryContentIds.MomoGuide, 7)]
    public void NewRivalsHaveDistinctTargets(int guideId, int target)
    {
        DuelState state = new DuelState();
        state.Begin(guideId);
        Assert.That(state.TargetPairs, Is.EqualTo(target));
    }

    [Test]
    public void UltimateMasterHanaBloomClaimsBonusPairWithoutConsumingTurn()
    {
        DuelState state = new DuelState();
        state.Begin(AnimalMemoryContentIds.UltimateMasterOpponent, 15);
        state.CompleteTurn(DuelActor.Player, false);

        state.ApplyOpponentBonusPair();

        Assert.That(state.OpponentPairs, Is.EqualTo(1));
        Assert.That(state.PlayerHealth, Is.EqualTo(14));
        Assert.That(state.OpponentTurns, Is.Zero);
        Assert.That(state.ActiveActor, Is.EqualTo(DuelActor.Opponent));
    }

    [Test]
    public void UltimateMasterUsesFifteenHealthOnTheThirtyCardBoard()
    {
        DuelState state = new DuelState();
        state.Begin(AnimalMemoryContentIds.UltimateMasterOpponent, 15);
        Assert.That(state.TargetPairs, Is.EqualTo(15));
        Assert.That(state.PlayerHealth, Is.EqualTo(15));
        Assert.That(state.OpponentHealth, Is.EqualTo(15));
        Assert.That(DuelRules.UltimateMasterSkillIntervalTurns, Is.EqualTo(2));
    }

    [Test]
    public void UndoAfterMismatchRestoresThePreviousCombo()
    {
        GameObject host = new GameObject("Combo undo test");
        ComboSystem combo = host.AddComponent<ComboSystem>();
        combo.ResetForNewGame();
        MethodInfo made = typeof(ComboSystem).GetMethod(
            "OnMatchMade", BindingFlags.Instance | BindingFlags.NonPublic);
        MethodInfo failed = typeof(ComboSystem).GetMethod(
            "OnMatchFailed", BindingFlags.Instance | BindingFlags.NonPublic);
        MethodInfo undone = typeof(ComboSystem).GetMethod(
            "OnMatchUndone", BindingFlags.Instance | BindingFlags.NonPublic);
        made.Invoke(combo, null);
        made.Invoke(combo, null);
        Assert.That(combo.CurrentCombo, Is.EqualTo(2));
        failed.Invoke(combo, null);
        Assert.That(combo.CurrentCombo, Is.Zero);
        undone.Invoke(combo, null);
        Assert.That(combo.CurrentCombo, Is.EqualTo(2));
        Object.DestroyImmediate(host);
    }

    private sealed class FixedRandom : IDuelRandom
    {
        private readonly Queue<int> integers;
        private readonly Queue<double> values;

        public FixedRandom(IEnumerable<int> integers, IEnumerable<double> values = null)
        {
            this.integers = new Queue<int>(integers);
            this.values = new Queue<double>(values ?? new[] { 1d });
        }

        public int Range(int minimumInclusive, int maximumExclusive)
        {
            int value = integers.Count > 0 ? integers.Dequeue() : 0;
            int width = maximumExclusive - minimumInclusive;
            return minimumInclusive + (value % width);
        }

        public double Value()
        {
            return values.Count > 0 ? values.Dequeue() : 1d;
        }
    }
}
