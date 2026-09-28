using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using AnimalMemory.Progression;
using MementoMatch.Duel;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>
/// GOD final duel wiring: phase transition, structural rebuild and turn ownership.
/// Runs real card coroutines in play mode with no GameManager/save listeners.
/// </summary>
public sealed class MementoGodDuelIntegrationTests
{
    private GameObject host;
    private CardMatchUI board;
    private bool previousBackground;
    private static readonly BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;

    [UnitySetUp]
    public IEnumerator Enter()
    {
        yield return new EnterPlayMode();
        previousBackground = Application.runInBackground;
        Application.runInBackground = true;
        host = new GameObject("Isolated GOD regression", typeof(RectTransform));
        host.SetActive(false);
        host.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 700);
        host.AddComponent<ComboSystem>();
        board = host.AddComponent<CardMatchUI>();
        var gridObject = new GameObject("Test grid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridObject.transform.SetParent(host.transform, false);
        Set("cardPrefab", AssetDatabase.LoadAssetAtPath<Card>("Assets/CardMatchPrototype/CardRelated/Prefabs/Card.prefab"));
        Set("gridLayout", gridObject.GetComponent<GridLayoutGroup>());
        Set("gridRect", gridObject.GetComponent<RectTransform>());
        board.onWinEvent = new UnityEvent();
        board.onDuelLost = new UnityEvent();
        host.SetActive(true);
        yield return null;
    }

    [UnityTearDown]
    public IEnumerator Leave()
    {
        if (board != null) board.CancelCurrentGame();
        if (host != null) UnityEngine.Object.Destroy(host);
        Application.runInBackground = previousBackground;
        yield return null;
        yield return new ExitPlayMode();
    }

    [UnityTest]
    public IEnumerator GodDuel_PhaseTransitionFiresOnceAtHalfHealth()
    {
        yield return BeginGodDuel(2, 2);
        int transitions = 0;
        board.GodPhaseTransitioned += () => transitions++;
        Assert.That(board.GodPhase, Is.EqualTo(GodDuelPhase.PhaseOne));
        Assert.That(board.DuelOpponentHealth, Is.EqualTo(2));

        ClickPair();
        yield return Until(() => !board.IsBusy);

        Assert.That(board.GodPhase, Is.EqualTo(GodDuelPhase.PhaseTwo), "Half of the initial health crosses the phase.");
        Assert.That(transitions, Is.EqualTo(1), "The transition happens exactly once.");
        Assert.That(board.IsGodPhaseTransitionPresented, Is.True, "The visible transition plays exactly once.");
        Assert.That(board.IsGodJudgementArmed, Is.False, "Phase one marks never survive the transition.");
    }

    [UnityTest]
    public IEnumerator GodDuel_PlayerExhaustionRebuilds_KeepsHealthAndReturnsTheTurn()
    {
        yield return BeginGodDuel(2, 2);
        MakeOpponentRememberTheBoard();
        int playerHealth = board.DuelPlayerHealth;
        int opponentHealth = board.DuelOpponentHealth;

        ClickPair();
        yield return Until(() => board.GodRebuildSerial == 1 && !board.IsBusy);

        Assert.That(State.RequiresSuddenDeath, Is.False, "The GOD duel never falls into sudden death.");
        Assert.That(State.Winner, Is.Null);
        Assert.That(board.IsPlayerTurn, Is.True, "A rebuild ends with a playable player turn.");
        Assert.That(board.DuelPlayerHealth, Is.EqualTo(playerHealth - 1), "The rebuild keeps real damage; it never restores health.");
        Assert.That(board.DuelOpponentHealth, Is.EqualTo(opponentHealth - 1));
        Assert.That(States.Values.Count(s => s == CardState.FaceDown), Is.EqualTo(4), "A full paired board is dealt again.");
        Assert.That(board.BoardRevision, Is.EqualTo(2));
        Assert.That(board.IsGodEraseArmed, Is.False, "Old marks never survive a rebuild.");
        Assert.That(board.IsGodJudgementArmed, Is.False);
    }

    [UnityTest]
    public IEnumerator GodDuel_OpponentExhaustionRebuilds_InTheSameClosure()
    {
        yield return BeginGodDuel(2, 2);

        // Leave a single live pair and make the AI remember it, so its attempt
        // really empties the board during GOD's own action.
        var states = Get<Dictionary<Card, CardState>>("cardStates");
        var pairIds = Get<Dictionary<Card, int>>("cardPairIds");
        int doomedPair = pairIds.Values.GroupBy(id => id).OrderBy(g => g.Count()).First().Key;
        foreach (Card card in pairIds.Keys.Where(c => pairIds[c] != doomedPair).ToArray())
            states[card] = CardState.Matched;

        MakeOpponentRememberTheBoard();

        int playerHealthBefore = board.DuelPlayerHealth;
        State.CompleteTurn(DuelActor.Player, false);
        yield return (IEnumerator)typeof(CardMatchUI).GetMethod("RunOpponentTurn", Fields).Invoke(board, null);
        yield return Until(() => !board.IsBusy);

        Assert.That(board.GodRebuildSerial, Is.EqualTo(1), "GOD rebuilds instead of resolving a tie by sudden death.");
        Assert.That(State.RequiresSuddenDeath, Is.False);
        Assert.That(State.Winner, Is.Null);
        Assert.That(board.IsPlayerTurn, Is.True);
        Assert.That(board.DuelPlayerHealth, Is.EqualTo(playerHealthBefore - 1), "The rebuild keeps the real damage; it never restores health.");
        Assert.That(board.DuelOpponentHealth, Is.EqualTo(2));
        Assert.That(States.Values.Count(s => s == CardState.FaceDown), Is.EqualTo(4));
        Assert.That(board.BoardRevision, Is.EqualTo(2));
    }

    [UnityTest]
    public IEnumerator GodDuel_RebuildDoesNotRepeatForTheSameRevision()
    {
        yield return BeginGodDuel(4, 4);
        int playerHealth = board.DuelPlayerHealth;

        State.CompleteTurn(DuelActor.Player, false);
        yield return (IEnumerator)typeof(CardMatchUI).GetMethod("RebuildGodBoard", Fields)
            .Invoke(board, new object[] { false });
        yield return Until(() => !board.IsBusy);
        Assert.That(board.GodRebuildSerial, Is.EqualTo(1));
        int revisionAfterFirst = board.BoardRevision;

        // A stale board never deals twice: the guard rejects the same revision.
        var godState = Get<GodDuelState>("godDuel");
        Assert.That(godState.BeginRebuild(revisionAfterFirst, out _), Is.False);
        Assert.That(board.GodRebuildSerial, Is.EqualTo(1));
        Assert.That(board.DuelPlayerHealth, Is.EqualTo(playerHealth));
    }

    private void MakeOpponentRememberTheBoard()
    {
        var brain = new DuelOpponentBrain(new SystemDuelRandom(41), 0);
        brain.BeginBoard(board.BoardRevision);
        var indices = Get<Dictionary<Card, int>>("cardIndices");
        foreach (KeyValuePair<Card, int> entry in Get<Dictionary<Card, int>>("cardPairIds"))
            brain.Observe(indices[entry.Key], entry.Value);
        Set("duelBrain", brain);
    }

    [UnityTest]
    public IEnumerator GodDuel_JudgementArmsAfterFiveOwnerTurns_AndStrikesOnTheNextTurn()
    {
        yield return BeginGodDuel(4, 4);
        MakeOpponentForget();
        int armedPairs = 0;
        int struckPairs = 0;
        board.GodJudgementArmed += pairs => armedPairs = pairs;
        board.GodJudgementResolved += pairs => struckPairs = pairs;

        for (int turn = 0; turn < GodDuelRules.JudgementCooldownTurns; turn++)
        {
            State.CompleteTurn(DuelActor.Player, false);
            yield return RunOpponentTurnOnce();
        }

        Assert.That(board.GodPhase, Is.EqualTo(GodDuelPhase.PhaseOne));
        Assert.That(board.IsGodJudgementArmed, Is.True, "Judgement arms on its own cooldown.");
        Assert.That(armedPairs, Is.InRange(1, GodDuelRules.JudgementMaxPairs));
        int markedPairs = board.GodJudgementPairCount;

        int playerHealth = board.DuelPlayerHealth;
        State.CompleteTurn(DuelActor.Player, false);
        yield return RunOpponentTurnOnce();

        Assert.That(board.IsGodJudgementArmed, Is.False);
        Assert.That(struckPairs, Is.EqualTo(markedPairs), "Every still-valid marked pair is struck.");
        Assert.That(board.DuelPlayerHealth, Is.EqualTo(playerHealth - markedPairs), "Normal damage per pair, no player combo.");
    }

    [UnityTest]
    public IEnumerator GodDuel_JudgementCancelsPairsThePlayerAlreadyRemoved()
    {
        yield return BeginGodDuel(4, 4);
        MakeOpponentForget();
        for (int turn = 0; turn < GodDuelRules.JudgementCooldownTurns; turn++)
        {
            State.CompleteTurn(DuelActor.Player, false);
            yield return RunOpponentTurnOnce();
        }
        Assert.That(board.IsGodJudgementArmed, Is.True);
        int markedPairs = board.GodJudgementPairCount;
        Assert.That(markedPairs, Is.GreaterThanOrEqualTo(1));

        // The player removes one marked pair before the strike: that ray is cancelled.
        GodBoardCard marked = Get<GodJudgementState>("godJudgement").Targets[0];
        var pairIds = Get<Dictionary<Card, int>>("cardPairIds");
        var states = Get<Dictionary<Card, CardState>>("cardStates");
        foreach (Card card in pairIds.Keys.Where(c => pairIds[c] == marked.PairId).ToArray())
            states[card] = CardState.Matched;

        int struckPairs = 0;
        board.GodJudgementResolved += pairs => struckPairs = pairs;
        int playerHealth = board.DuelPlayerHealth;
        State.CompleteTurn(DuelActor.Player, false);
        yield return RunOpponentTurnOnce();

        Assert.That(struckPairs, Is.EqualTo(markedPairs - 1), "The removed pair is never substituted.");
        Assert.That(board.DuelPlayerHealth, Is.EqualTo(playerHealth - struckPairs));
    }

    [UnityTest]
    public IEnumerator GodDuel_EraseSavedWhenPlayerResolvesTheTarget()
    {
        yield return BeginGodDuel(4, 4);
        MakeOpponentForget();
        yield return EnterPhaseTwo();
        yield return ArmErase();
        Assert.That(board.IsGodEraseArmed, Is.True, "Borrado arms on its own cooldown in phase two.");
        int targetOrdinal = board.GodEraseTargetOrdinal;
        Assert.That(targetOrdinal, Is.GreaterThanOrEqualTo(0));

        Card target = FindCardByOrdinal(targetOrdinal);
        Assert.That(target, Is.Not.Null);
        var pairIds = Get<Dictionary<Card, int>>("cardPairIds");
        var states = Get<Dictionary<Card, CardState>>("cardStates");
        int targetPair = pairIds[target];
        foreach (Card card in pairIds.Keys.Where(c => pairIds[c] == targetPair).ToArray())
            states[card] = CardState.Matched;

        bool saved = false;
        board.GodEraseSaved += () => saved = true;
        int playerHealth = board.DuelPlayerHealth;
        State.CompleteTurn(DuelActor.Player, false);
        yield return RunOpponentTurnOnce();

        Assert.That(saved, Is.True, "A ray whose target is gone fails instead of redirecting.");
        Assert.That(board.GodErasedCardCount, Is.Zero);
        Assert.That(board.GodOrphanedCardCount, Is.Zero);
        Assert.That(board.DuelPlayerHealth, Is.EqualTo(playerHealth), "A failed ray never damages.");
    }

    [UnityTest]
    public IEnumerator GodDuel_EraseFiresWithoutScoring_AndLeavesAnUnselectableOrphan()
    {
        yield return BeginGodDuel(4, 4);
        MakeOpponentForget();
        yield return EnterPhaseTwo();
        yield return ArmErase();
        Assert.That(board.IsGodEraseArmed, Is.True);
        int targetOrdinal = board.GodEraseTargetOrdinal;

        int playerHealth = board.DuelPlayerHealth;
        int opponentPairs = State.OpponentPairs;
        State.CompleteTurn(DuelActor.Player, false);
        yield return RunOpponentTurnOnce();

        Assert.That(board.IsGodEraseArmed, Is.False);
        Assert.That(board.GodErasedCardCount, Is.EqualTo(1));
        Assert.That(board.GodOrphanedCardCount, Is.EqualTo(1), "The partner is orphaned, never matched.");
        Assert.That(board.DuelPlayerHealth, Is.EqualTo(playerHealth), "Erasing never damages.");
        Assert.That(State.OpponentPairs, Is.EqualTo(opponentPairs), "Erasing never scores a pair for GOD.");

        Card erased = FindCardByOrdinal(targetOrdinal);
        Assert.That(erased, Is.Not.Null);
        Assert.That(erased.GetComponent<Button>().interactable, Is.False, "An erased card is never selectable again.");
        Assert.That(erased.VisualTint, Is.Not.EqualTo(Color.white), "The invalidated card keeps an unmistakable tint.");
        Assert.That(erased.CurrentSprite, Is.SameAs(erased.OriginalSprite), "It stays visible instead of looking matched.");

        var pairIds = Get<Dictionary<Card, int>>("cardPairIds");
        Card orphan = pairIds.Keys.First(c => c != erased && pairIds[c] == pairIds[erased]);
        Assert.That(orphan.GetComponent<Button>().interactable, Is.False, "The orphan is never selectable again.");
        Assert.That(orphan.VisualTint, Is.Not.EqualTo(Color.white));
        Assert.That(orphan.VisualTint, Is.Not.EqualTo(erased.VisualTint), "Erased and orphan carry different signals.");
    }

    [UnityTest]
    public IEnumerator GodDuel_PhaseTwoUnlocksOnlyPresentAllyPowers()
    {
        yield return BeginGodDuel(4, 4);
        var run = new AnimalMemoryGuideRunState();
        run.Reset(2);
        board.ConfigureProgression(0, run);
        board.ConfigureAllianceRoster((1 << 0) | (1 << 1) | (1 << 2));
        Assert.That(run.IsAllianceActive, Is.False);
        Assert.That(board.TryUseAlliancePower(1), Is.False);
        yield return EnterPhaseTwo();
        yield return (IEnumerator)typeof(CardMatchUI)
            .GetMethod("PresentGodPhaseTransition", Fields).Invoke(board, null);
        yield return Until(() => !board.IsBusy);
        Assert.That(run.AllianceMask, Is.EqualTo((1 << 1) | (1 << 2)));
        Assert.That(board.TryUseAlliancePower(0), Is.False);
        Assert.That(board.TryUseAlliancePower(3), Is.False);
        Assert.That(board.TryUseAlliancePower(1), Is.True);
        Assert.That(board.TryUseAlliancePower(2), Is.False);
        Assert.That(run.MikaProtectionArmed, Is.True);
    }

    private IEnumerator EnterPhaseTwo()
    {
        // The transition itself has its own test; the phase is entered through the
        // real model contract so the erase path can be exercised deterministically.
        State.CompleteTurn(DuelActor.Player, true, false, 4);
        Assert.That(Get<GodDuelState>("godDuel").TryEnterPhaseTwo(board.DuelOpponentHealth, false), Is.True);
        State.CompleteTurn(DuelActor.Opponent, false);
        yield return null;
    }

    private IEnumerator ArmErase()
    {
        for (int turn = 0; turn < GodDuelRules.EraseCooldownTurns; turn++)
        {
            State.CompleteTurn(DuelActor.Player, false);
            yield return RunOpponentTurnOnce();
        }
    }

    private IEnumerator RunOpponentTurnOnce()
    {
        yield return (IEnumerator)typeof(CardMatchUI).GetMethod("RunOpponentTurn", Fields).Invoke(board, null);
        yield return Until(() => !board.IsBusy);
    }

    private void MakeOpponentForget()
    {
        var brain = new DuelOpponentBrain(new SystemDuelRandom(41), 0);
        brain.BeginBoard(board.BoardRevision);
        Set("duelBrain", brain);
    }

    private Card FindCardByOrdinal(int ordinal)
    {
        foreach (KeyValuePair<Card, int> entry in Get<Dictionary<Card, int>>("cardIndices"))
            if (entry.Value == ordinal)
                return entry.Key;
        return null;
    }

    private IEnumerator BeginGodDuel(int rows, int columns)
    {
        board.ConfigureDuel(MementoPostgameEncounters.LastOpponentId);
        board.SetupGame(rows, columns, Color.black, 0);
        yield return Until(() => !board.IsBusy);
        Assert.That(board.IsGodDuelConfigured, Is.True, "The Creator must configure the two-phase duel.");
    }

    private void ClickPair()
    {
        var ids = Get<Dictionary<Card, int>>("cardPairIds");
        var pair = States.Where(p => p.Value == CardState.FaceDown).Select(p => p.Key)
            .GroupBy(c => ids[c]).First(g => g.Count() == 2).ToArray();
        pair[0].GetComponent<Button>().onClick.Invoke();
        pair[1].GetComponent<Button>().onClick.Invoke();
    }

    private static IEnumerator Until(System.Func<bool> ready)
    {
        float deadline = Time.realtimeSinceStartup + 18f;
        yield return null;
        while (!ready() && Time.realtimeSinceStartup < deadline) yield return null;
        Assert.That(ready(), Is.True, "The actual card coroutine did not reach its terminal/interactive state.");
    }

    private DuelState State => Get<DuelState>("duelState");
    private Dictionary<Card, CardState> States => Get<Dictionary<Card, CardState>>("cardStates");
    private T Get<T>(string name) => (T)typeof(CardMatchUI).GetField(name, Fields).GetValue(board);
    private void Set(string name, object value) => typeof(CardMatchUI).GetField(name, Fields).SetValue(board, value);
}
