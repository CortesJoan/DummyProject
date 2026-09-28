using System;

/// <summary>One rewarded continuation per run; stale, duplicate and cancelled callbacks fail closed.</summary>
public sealed class RewardedContinueGate
{
    private long sequence;
    private long pending;
    public bool Used { get; private set; }
    public bool IsPending => pending != 0;
    public long BeginRequest(bool eligible, bool ready)
    {
        if (!eligible || !ready || Used || IsPending) return 0;
        pending = ++sequence;
        return pending;
    }
    public bool Complete(long request, bool earned)
    {
        if (request == 0 || request != pending) return false;
        pending = 0;
        if (!earned) return false;
        Used = true;
        return true;
    }
    public void CancelPending() { pending = 0; }
    public void BeginRun() { pending = 0; Used = false; }
}

/// <summary>Only Rewarded means the SDK confirmed earning the reward AND the full-screen ad has closed.</summary>
public enum RewardedContinueOutcome { Unavailable, Cancelled, Failed, Rewarded }

/// <summary>Implement with a licensed/configured SDK later. No default fake rewards or network requests.</summary>
public interface IRewardedContinueProvider
{
    bool IsReady { get; }
    void Show(Action<RewardedContinueOutcome> completed);
}
