using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MementoMatch.Duel;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.TestTools;
using UnityEngine.UI;

/// <summary>Runs real card coroutines in play mode with no GameManager/save listeners.</summary>
public sealed class MementoDuelBoardIntegrationTests
{
    private GameObject host;
    private CardMatchUI board;
    private bool previousBackground;
    private int victories;
    private int defeats;
    private static readonly BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;

    [UnitySetUp]
    public IEnumerator Enter()
    {
        yield return new EnterPlayMode();
        previousBackground = Application.runInBackground;
        Application.runInBackground = true;
        host = new GameObject("Isolated duel regression", typeof(RectTransform));
        host.SetActive(false);
        host.GetComponent<RectTransform>().sizeDelta = new Vector2(900, 700);
        host.AddComponent<ComboSystem>(); // Avoid finding the real scene's combo component.
        board = host.AddComponent<CardMatchUI>();
        var gridObject = new GameObject("Test grid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridObject.transform.SetParent(host.transform, false);
        Set("cardPrefab", AssetDatabase.LoadAssetAtPath<Card>("Assets/CardMatchPrototype/CardRelated/Prefabs/Card.prefab"));
        Set("gridLayout", gridObject.GetComponent<GridLayoutGroup>());
        Set("gridRect", gridObject.GetComponent<RectTransform>());
        board.onWinEvent = new UnityEvent();
        board.onDuelLost = new UnityEvent();
        board.onWinEvent.AddListener(() => victories++);
        board.onDuelLost.AddListener(() => defeats++);
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
    public IEnumerator ExhaustedTiedBoard_DealsNewPairs_AndAcceptsNextClick()
    {
        yield return Begin(2, 2);
        State.Begin(0, 4); // Two pairs remain in a duel with four maximum health.
        RememberBoard();
        ClickPair();
        yield return Until(() => !board.IsBusy && State.PlayerPairs == 1 && State.OpponentPairs == 1);
        Assert.That(victories + defeats, Is.Zero);
        Assert.That(board.IsPlayerTurn, Is.True);
        Assert.That(board.DuelPlayerHealth, Is.EqualTo(3));
        Assert.That(board.DuelOpponentHealth, Is.EqualTo(3));
        Assert.That(States.Values.Count(s => s == CardState.FaceDown), Is.EqualTo(4));
        Card next = States.Keys.First();
        next.GetComponent<Button>().onClick.Invoke();
        Assert.That(States[next], Is.EqualTo(CardState.FaceUp), "The replacement board must really accept input.");
    }

    [UnityTest]
    public IEnumerator LethalOpponentMatch_ContinuesOnce_PreservingActualRemainingCards()
    {
        yield return Begin(4, 4);
        RememberBoard();
        for (int turn = 0; turn < 4; turn++)
        {
            int expected = turn + 1;
            ClickMismatch();
            yield return Until(() => !board.IsBusy && State.OpponentPairs == expected);
        }
        Assert.That(defeats, Is.EqualTo(1));
        Assert.That(board.CanContinueDefeat, Is.True);
        var remaining = States.Where(p => p.Value == CardState.FaceDown).Select(p => p.Key).ToArray();
        Assert.That(remaining.Length, Is.EqualTo(8));
        int turns = board.Turns;
        Assert.That(board.TryContinueDefeat(), Is.True);
        Assert.That(board.DuelPlayerHealth, Is.EqualTo(2));
        Assert.That(board.DuelOpponentPairs, Is.EqualTo(4));
        Assert.That(board.Turns, Is.EqualTo(turns));
        Assert.That(States.Where(p => p.Value == CardState.FaceDown).Select(p => p.Key), Is.EquivalentTo(remaining));
        Assert.That(board.TryContinueDefeat(), Is.False);
        Card next = remaining[0];
        next.GetComponent<Button>().onClick.Invoke();
        Assert.That(States[next], Is.EqualTo(CardState.FaceUp));
    }

    [UnityTest]
    public IEnumerator OpponentBonusConsumesLastPair_EmptyBoardContinuationCreatesPlayableCards()
    {
        yield return Begin(2, 2);
        State.Begin(0, 4);
        State.CompleteTurn(DuelActor.Player, false);
        // This is the actual automatic-pair coroutine used by Rei's Hana technique.
        yield return (IEnumerator)typeof(CardMatchUI).GetMethod("OpponentSkillBloom", Fields).Invoke(board, null);
        yield return (IEnumerator)typeof(CardMatchUI).GetMethod("OpponentSkillBloom", Fields).Invoke(board, null);
        Assert.That(States.Values.All(s => s == CardState.Matched), Is.True);
        Assert.That(State.Winner, Is.EqualTo(DuelActor.Opponent));
        Assert.That(board.TryContinueDefeat(), Is.True);
        yield return Until(() => !board.IsBusy);
        Assert.That(board.IsDuelActive && board.IsPlayerTurn, Is.True);
        Assert.That(board.DuelOpponentPairs, Is.EqualTo(2));
        Assert.That(States.Values.Count(s => s == CardState.FaceDown), Is.EqualTo(4));
        ClickPair();
        yield return Until(() => !board.IsBusy);
        Assert.That(board.Matches, Is.EqualTo(1));
    }

    [UnityTest]
    public IEnumerator Vision_WaitsForAnnouncement_ThenRevealsAnActualPair()
    {
        yield return Begin(2, 2);
        var guide = new AnimalMemory.Progression.AnimalMemoryGuideRunState();
        guide.Reset(2);
        board.ConfigureProgression(0, guide);
        guide.PowerPresentationRequested += (id, resolved) => board.HoldForSkillPresentation(0.35f);
        Assert.That(board.TryUseGuidePower(), Is.True);
        Assert.That(board.IsBusy, Is.True);
        yield return new WaitForSecondsRealtime(0.12f);
        Assert.That(States.Keys.Count(c => c.CurrentSprite == c.OriginalSprite), Is.Zero,
            "Vision must not disappear underneath its announcement.");
        yield return new WaitForSecondsRealtime(0.35f);
        var visible = States.Keys.Where(c => c.CurrentSprite == c.OriginalSprite).ToArray();
        Assert.That(visible.Length, Is.EqualTo(2));
        Assert.That(visible[0].OriginalSprite, Is.SameAs(visible[1].OriginalSprite));
    }

    [UnityTest]
    public IEnumerator CancelDuringAnnouncement_ClearsHoldAndPendingReveal()
    {
        yield return Begin(2, 2);
        var guide = new AnimalMemory.Progression.AnimalMemoryGuideRunState();
        guide.Reset(2);
        board.ConfigureProgression(0, guide);
        guide.PowerPresentationRequested += (id, resolved) => board.HoldForSkillPresentation(0.25f);
        Assert.That(board.TryUseGuidePower(), Is.True);
        board.CancelCurrentGame();
        Assert.That(board.IsBusy, Is.False);
        yield return new WaitForSecondsRealtime(0.4f);
        Assert.That(States.Keys.Count(c => c.CurrentSprite == c.OriginalSprite), Is.Zero);
    }


    [UnityTest]
    public IEnumerator SkipAnnouncement_ReleasesRealVision_WithoutSkippingItsPair()
    {
        yield return Begin(2, 2);
        var guide = new AnimalMemory.Progression.AnimalMemoryGuideRunState();
        guide.Reset(2);
        board.ConfigureProgression(0, guide);
        guide.PowerPresentationRequested += (id, resolved) => board.HoldForSkillPresentation(8f);
        Assert.That(board.TryUseGuidePower(), Is.True);
        board.SkipSkillPresentation();
        Assert.That(board.IsBusy, Is.True, "Dismissal must not leak a click to a card.");
        yield return new WaitForSecondsRealtime(.3f);
        var visible = States.Keys.Where(c => c.CurrentSprite == c.OriginalSprite).ToArray();
        Assert.That(visible.Length, Is.EqualTo(2));
        Assert.That(visible[0].OriginalSprite, Is.SameAs(visible[1].OriginalSprite));
        Assert.That(guide.IsPowerReady, Is.False);
        Assert.That(board.Turns, Is.Zero);
    }

    [UnityTest]
    public IEnumerator SupplyRow_RevealsOnlyChosenRow_WithoutResolvingTurn()
    {
        yield return Begin(4, 4);
        int turns = board.Turns;
        var indices = Get<Dictionary<Card,int>>("cardIndices");
        Assert.That(board.TryRevealSupplyLine(AnimalMemory.Progression.MementoSupplyKind.Row, 2), Is.True);
        var visible = States.Keys.Where(c => c.CurrentSprite == c.OriginalSprite).ToArray();
        Assert.That(visible.Length, Is.EqualTo(4));
        Assert.That(visible.All(c => indices[c] / 4 == 2), Is.True);
        Assert.That(board.SupplyUsesRemaining, Is.EqualTo(2));
        Assert.That(board.TryRevealSupplyLine(AnimalMemory.Progression.MementoSupplyKind.Column, 0), Is.False);
        yield return new WaitForSecondsRealtime(3.2f);
        Assert.That(States.Keys.Count(c => c.CurrentSprite == c.OriginalSprite), Is.Zero);
        Assert.That(board.Turns, Is.EqualTo(turns));
        Assert.That(board.Matches, Is.Zero);
        Assert.That(board.IsBusy, Is.False);
        Assert.That(board.IsPlayerTurn, Is.True);
    }

    [UnityTest]
    public IEnumerator SupplyColumn_SelectsCorrectIndices_AndCancelDoesNotLeaveBusy()
    {
        yield return Begin(4, 4);
        var indices = Get<Dictionary<Card,int>>("cardIndices");
        Assert.That(board.TryRevealSupplyLine(AnimalMemory.Progression.MementoSupplyKind.Column, 1), Is.True);
        Assert.That(States.Keys.Count(c => c.CurrentSprite == c.OriginalSprite), Is.EqualTo(4));
        Assert.That(States.Keys.Where(c => c.CurrentSprite == c.OriginalSprite).All(c => indices[c] % 4 == 1), Is.True);
        board.CancelCurrentGame();
        Assert.That(board.IsBusy, Is.False);
        Assert.That(board.SupplyUsesRemaining, Is.EqualTo(3));
    }

    [UnityTest]
    public IEnumerator SupplyInvalidLinePartialSelectionAndAttemptLimit_DoNotConsumeUses()
    {
        yield return Begin(2, 2);
        Assert.That(board.TryRevealSupplyLine(AnimalMemory.Progression.MementoSupplyKind.Row, 9), Is.False);
        Assert.That(board.SupplyUsesRemaining, Is.EqualTo(3));
        Card first = States.Keys.First();
        first.GetComponent<Button>().onClick.Invoke();
        Assert.That(board.CanUseSupplies, Is.False);
        Assert.That(board.TryRevealSupplyLine(AnimalMemory.Progression.MementoSupplyKind.Row, 0), Is.False);
        typeof(CardMatchUI).GetMethod("CancelPartialSelection", Fields).Invoke(board, null);
        Set("suppliesUsed", 3);
        Assert.That(board.CanUseSupplies, Is.False);
        Assert.That(board.TryRevealSupplyLine(AnimalMemory.Progression.MementoSupplyKind.Row, 0), Is.False);
    }


    [UnityTest]
    public IEnumerator OpponentSkill_SkipInPresentationCallback_DoesNotRearmMinimumWait()
    {
        yield return Begin(2, 2);
        board.OpponentDuelSkillActivated += id => board.SkipSkillPresentation();
        var coroutine = (IEnumerator)typeof(CardMatchUI).GetMethod("OpponentSkillPeek", Fields)
            .Invoke(board, new object[] { false, true });
        board.StartCoroutine(coroutine);
        yield return new WaitForSecondsRealtime(.3f);
        Assert.That(States.Keys.Count(c => c.CurrentSprite == c.OriginalSprite), Is.GreaterThan(0),
            "The enemy reveal must start after skipping, not wait another 3.6 seconds.");
    }


    [UnityTest]
    public IEnumerator PostgameAllyMatch_DoesNotBorrowProtagonistComboDamage()
    {
        yield return Begin(4, 4);
        board.ConfigurePlayerRole(1);
        typeof(ComboSystem).GetField("currentCombo", Fields).SetValue(host.GetComponent<ComboSystem>(), 3);
        ClickPair();
        yield return Until(() => State.PlayerPairs == 1);
        Assert.That(board.LastPlayerDuelDamage, Is.EqualTo(1));
        Assert.That(State.OpponentHealth, Is.EqualTo(3));
        board.CancelCurrentGame();

        yield return Begin(4, 4);
        board.ConfigurePlayerRole(-1);
        typeof(ComboSystem).GetField("currentCombo", Fields).SetValue(host.GetComponent<ComboSystem>(), 3);
        ClickPair();
        yield return Until(() => State.PlayerPairs == 1);
        Assert.That(board.LastPlayerDuelDamage, Is.EqualTo(3));
        Assert.That(State.OpponentHealth, Is.EqualTo(1));
    }

    [UnityTest]
    public IEnumerator PostgameCustodian_UsesFixedTechnique_WithItsOwnIdentity()
    {
        board.ConfigureDuel(6);
        board.SetupGame(4, 4, Color.black, 0);
        yield return Until(() => !board.IsBusy);
        int calls = 0;
        board.OpponentDuelSkillActivated += id =>
        {
            calls++;
            Assert.That(id, Is.EqualTo(6));
            Assert.That(board.DuelOpponentTechniqueGuideId, Is.Zero);
            board.SkipSkillPresentation();
        };
        for (int i = 0; i < 3; i++)
        {
            yield return (IEnumerator)typeof(CardMatchUI).GetMethod("UltimateMasterSkill", Fields).Invoke(board, null);
            Assert.That(Get<bool>("ultimateMasterUndoPending"), Is.True);
        }
        Assert.That(calls, Is.EqualTo(3));
    }

    [UnityTest]
    public IEnumerator Creator_CyclesTechniques_WithoutImpersonatingTheirOwners()
    {
        board.ConfigureDuel(10);
        board.SetupGame(4, 4, Color.black, 0);
        yield return Until(() => !board.IsBusy);
        State.CompleteTurn(DuelActor.Player, false);
        var techniques = new List<int>();
        board.OpponentDuelSkillActivated += id =>
        {
            Assert.That(id, Is.EqualTo(10));
            techniques.Add(board.DuelOpponentTechniqueGuideId);
            board.SkipSkillPresentation();
        };
        for (int i = 0; i < 6; i++)
            yield return (IEnumerator)typeof(CardMatchUI).GetMethod("UltimateMasterSkill", Fields).Invoke(board, null);
        Assert.That(techniques, Is.EqualTo(new[] { 0, 1, 2, 3, 4, 0 }));
        Assert.That(State.OpponentPairs, Is.EqualTo(1), "The copied Bloom must actually resolve a pair.");
    }

    private IEnumerator Begin(int rows, int columns)
    {
        board.ConfigureDuel(0);
        board.SetupGame(rows, columns, Color.black, 0);
        yield return Until(() => !board.IsBusy);
    }

    private void RememberBoard()
    {
        var brain = new DuelOpponentBrain(new SystemDuelRandom(41), 0);
        // A replacement brain must join the current board revision before observing,
        // exactly like the one CreateGrid builds, or stale-id guards reject its memory.
        brain.BeginBoard(board.BoardRevision);
        var indices = Get<Dictionary<Card, int>>("cardIndices");
        foreach (var pair in Get<Dictionary<Card, int>>("cardPairIds"))
            brain.Observe(indices[pair.Key], pair.Value);
        Set("duelBrain", brain);
    }

    private void ClickPair()
    {
        var ids = Get<Dictionary<Card, int>>("cardPairIds");
        var pair = States.Where(p => p.Value == CardState.FaceDown).Select(p => p.Key)
            .GroupBy(c => ids[c]).First(g => g.Count() == 2).ToArray();
        pair[0].GetComponent<Button>().onClick.Invoke();
        pair[1].GetComponent<Button>().onClick.Invoke();
    }

    private void ClickMismatch()
    {
        var ids = Get<Dictionary<Card, int>>("cardPairIds");
        var hidden = States.Where(p => p.Value == CardState.FaceDown).Select(p => p.Key).ToArray();
        Card first = hidden[0], second = hidden.First(c => ids[c] != ids[first]);
        first.GetComponent<Button>().onClick.Invoke();
        second.GetComponent<Button>().onClick.Invoke();
    }

    private static IEnumerator Until(Func<bool> ready)
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
