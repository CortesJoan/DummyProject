using MementoMatch.Duel;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

public sealed class MementoRewardedContinueTests
{
    // Editor-only provider for deterministic runtime QA. Never compiled into a player.
    public sealed class FakeProvider : IRewardedContinueProvider
    {
        public bool IsReady { get; set; } = true;
        public System.Action<RewardedContinueOutcome> Completion;
        public void Show(System.Action<RewardedContinueOutcome> completed) { Completion = completed; }
    }

    [TestCase(false, true)]
    [TestCase(true, false)]
    [TestCase(false, false)]
    public void Gate_UnavailableOrIneligible_DoesNotBegin(bool eligible, bool ready)
    {
        var gate = new RewardedContinueGate();
        Assert.That(gate.BeginRequest(eligible, ready), Is.Zero);
        Assert.That(gate.IsPending, Is.False);
    }

    [Test]
    public void Gate_RewardIsGrantedExactlyOnce_PerRun()
    {
        var gate = new RewardedContinueGate();
        long id = gate.BeginRequest(true, true);
        Assert.That(gate.BeginRequest(true, true), Is.Zero);
        Assert.That(gate.Complete(id, true), Is.True);
        Assert.That(gate.Complete(id, true), Is.False);
        Assert.That(gate.BeginRequest(true, true), Is.Zero);
        gate.BeginRun();
        Assert.That(gate.BeginRequest(true, true), Is.GreaterThan(id));
    }

    [Test]
    public void Gate_CancelledFailedAndOldCallbacks_NeverRewardANewRun()
    {
        var gate = new RewardedContinueGate();
        long old = gate.BeginRequest(true, true);
        Assert.That(gate.Complete(old, false), Is.False);
        Assert.That(gate.Used, Is.False);
        long cancelled = gate.BeginRequest(true, true);
        gate.CancelPending();
        long current = gate.BeginRequest(true, true);
        Assert.That(gate.Complete(cancelled, true), Is.False);
        Assert.That(gate.IsPending, Is.True);
        gate.BeginRun();
        Assert.That(gate.Complete(current, true), Is.False);
        Assert.That(gate.Used, Is.False);
    }

    private static DuelState Defeated(int guide)
    {
        var state = new DuelState();
        state.Begin(guide, 15);
        state.CompleteTurn(DuelActor.Player, true);
        state.CompleteTurn(DuelActor.Opponent, false);
        while (state.IsActive)
        {
            state.CompleteTurn(DuelActor.Player, false);
            state.CompleteTurn(DuelActor.Opponent, true);
        }
        return state;
    }

    [TestCase(0)]
    [TestCase(1)]
    [TestCase(2)]
    [TestCase(3)]
    [TestCase(4)]
    [TestCase(5)]
    public void Continue_PreservesPairCountersAndRivalDamage_RestoresHalfHealthAndPlayerTurn(int guide)
    {
        var state = Defeated(guide);
        int playerPairs = state.PlayerPairs, opponentPairs = state.OpponentPairs, hp = state.OpponentHealth;
        Assert.That(state.TryContinueDefeat(false), Is.True);
        Assert.That(state.PlayerHealth, Is.EqualTo((state.TargetPairs + 1) / 2));
        Assert.That(state.PlayerPairs, Is.EqualTo(playerPairs));
        Assert.That(state.OpponentPairs, Is.EqualTo(opponentPairs));
        Assert.That(state.OpponentHealth, Is.EqualTo(hp));
        Assert.That(state.ActiveActor, Is.EqualTo(DuelActor.Player));
        Assert.That(state.Winner, Is.Null);
        Assert.That(state.TryContinueDefeat(false), Is.False);
        while (state.IsActive)
        {
            state.CompleteTurn(DuelActor.Player, false);
            state.CompleteTurn(DuelActor.Opponent, true);
        }
        Assert.That(state.CanContinueDefeat, Is.False);
    }

    [Test]
    public void Continue_EmptyBoardRequestsNewRound_WithoutResettingHealthOrCounters()
    {
        var state = Defeated(5);
        state.TryContinueDefeat(true);
        Assert.That(DuelRules.GetTerminalAction(state), Is.EqualTo(DuelTerminalAction.SuddenDeath));
        int health = state.PlayerHealth;
        state.BeginSuddenDeathRound();
        Assert.That(state.PlayerHealth, Is.EqualTo(health));
        Assert.That(state.PlayerPairs, Is.EqualTo(1));
        Assert.That(DuelRules.GetTerminalAction(state), Is.EqualTo(DuelTerminalAction.Continue));
    }

    [Test]
    public void Continue_ActiveAndWonDuelsAreNotEligible_NewAttemptResetsAllowance()
    {
        var state = new DuelState(); state.Begin(0);
        Assert.That(state.TryContinueDefeat(false), Is.False);
        state.CompleteTurn(DuelActor.Player, true, false, 99);
        Assert.That(state.TryContinueDefeat(false), Is.False);
        state.Begin(0);
        while(state.IsActive) { state.CompleteTurn(DuelActor.Player,false); state.CompleteTurn(DuelActor.Opponent,true); }
        Assert.That(state.TryContinueDefeat(false), Is.True);
    }

    [Test]
    public void Result_UnconfiguredProviderCannotOffer_AndBothFreeExitsRemain()
    {
        var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/AnimalMemory/UI/AnimalMemoryMenu.uxml");
        var root = asset.CloneTree();
        Assert.That(root.Q<VisualElement>("result-continue-offer"), Is.Not.Null);
        var owner = new UnityEngine.GameObject("reward-test");
        try { Assert.That(owner.AddComponent<MementoRewardedContinue>().CanOffer, Is.False); }
        finally { UnityEngine.Object.DestroyImmediate(owner); }
        Assert.That(root.Q<Button>("result-retry"), Is.Not.Null);
        Assert.That(root.Q<Button>("result-menu"), Is.Not.Null);
        Assert.That(root.Q<Button>("result-continue-ad").text, Does.Contain("ANUNCIO"));
    }
}
