using System;
using System.Collections.Concurrent;
using UnityEngine;

/// <summary>Optional monetization boundary. Gameplay and views never depend on an ad SDK.</summary>
public sealed class MementoRewardedContinue : MonoBehaviour
{
    private readonly RewardedContinueGate gate = new RewardedContinueGate();
    private readonly ConcurrentQueue<Action> callbacks = new ConcurrentQueue<Action>();
    private IRewardedContinueProvider provider;
    private GameManager game;
    private float deadline;
    public bool IsPending => gate.IsPending;
    public bool WasUsed => gate.Used;
    public string Status { get; private set; } = "";
    public bool CanOffer => game != null && game.CanContinueDefeat && !gate.Used && ProviderReady;

    private bool ProviderReady
    {
        get { try { return provider != null && provider.IsReady; } catch { return false; } }
    }

    /// <summary>Provider must handle consent, age restrictions, test IDs, ad lifecycle and main-thread SDK access.</summary>
    public void Configure(GameManager owner, IRewardedContinueProvider adProvider)
    {
        gate.CancelPending();
        game = owner;
        provider = adProvider;
        Status = "";
    }

    public void BeginRun() { gate.BeginRun(); Status = ""; }
    public void CancelPending() { gate.CancelPending(); Status = ""; }

    public void Request()
    {
        long token = gate.BeginRequest(game != null && game.CanContinueDefeat, ProviderReady);
        if (token == 0) return;
        Status = "ESPERANDO AL ANUNCIO…";
        deadline = Time.realtimeSinceStartup + 180f;
        try
        {
            provider.Show(outcome => callbacks.Enqueue(() => Receive(token, outcome)));
        }
        catch
        {
            Receive(token, RewardedContinueOutcome.Failed);
        }
    }

    private void Receive(long token, RewardedContinueOutcome outcome)
    {
        bool earned = outcome == RewardedContinueOutcome.Rewarded;
        bool wasPending = gate.IsPending;
        if (!gate.Complete(token, earned))
        {
            // Ignore stale/duplicate callbacks rather than overwrite a newer request's status.
            if (wasPending && !gate.IsPending)
                Status = outcome == RewardedContinueOutcome.Cancelled
                    ? "Anuncio cerrado sin recompensa. Puedes reintentar gratis."
                    : "El anuncio no está disponible. Puedes reintentar gratis.";
            return;
        }
        Status = game != null && game.TryContinueDefeat()
            ? ""
            : "La partida ya no está disponible. Puedes reintentar gratis.";
    }

    private void Update()
    {
        Action callback;
        int count = 0;
        while (count++ < 32 && callbacks.TryDequeue(out callback)) callback();
        if (gate.IsPending && (game == null || !game.CanContinueDefeat))
            CancelPending();
        if (gate.IsPending && Time.realtimeSinceStartup >= deadline)
        {
            gate.CancelPending();
            Status = "El anuncio no respondió. Puedes reintentar gratis.";
        }
    }

    private void OnDisable() { CancelPending(); }
    private void OnDestroy()
    {
        CancelPending(); provider = null; game = null;
        Action ignored;
        while (callbacks.TryDequeue(out ignored)) { }
    }
}
