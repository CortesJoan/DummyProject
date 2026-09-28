using System;
using Minigames.Core;
using NUnit.Framework;

public sealed class MementoMemorySessionTests
{
    private MementoMemorySession session;
    [SetUp] public void SetUp() => session = new MementoMemorySession(42);
    [TearDown] public void TearDown() => session.Dispose();

    [Test]
    public void Start_UsesOriginalFramework_InvokesBoardOnce()
    {
        int starts = 0;
        Assert.That(session.Minigame, Is.TypeOf<Minigame>());
        Assert.That(session.Status, Is.EqualTo(MinigameStatus.Ready));
        Assert.That(session.Start(() => starts++), Is.True);
        Assert.That(session.Start(() => starts++), Is.False);
        Assert.That(starts, Is.EqualTo(1));
        Assert.That(session.Status, Is.EqualTo(MinigameStatus.Playing));
    }

    [TestCase(MinigameResult.Win)]
    [TestCase(MinigameResult.Lose)]
    [TestCase(MinigameResult.Draw)]
    [TestCase(MinigameResult.Aborted)]
    public void Complete_RepeatedOutcome_NotifiesExactlyOnce(MinigameResult result)
    {
        int completions = 0;
        MinigameResult received = MinigameResult.None;
        session.Completed += value => { completions++; received = value; };
        session.Start(null);
        Assert.That(session.TryComplete(result), Is.True);
        Assert.That(session.TryComplete(result), Is.False);
        Assert.That(completions, Is.EqualTo(1));
        Assert.That(received, Is.EqualTo(result));
        Assert.That(session.Status, Is.EqualTo(MinigameStatus.Ended));
    }

    [Test]
    public void Dispose_ActiveAttempt_DoesNotGrantAnOutcome()
    {
        int completions = 0;
        session.Completed += _ => completions++;
        session.Start(null);
        session.Dispose();
        session.Dispose();
        Assert.That(session.TryComplete(MinigameResult.Win), Is.False);
        Assert.That(session.Status, Is.EqualTo(MinigameStatus.Ended));
        Assert.That(completions, Is.Zero);
    }

    [Test]
    public void Start_BoardThrows_DisposesWithoutReward()
    {
        int completions = 0;
        session.Completed += _ => completions++;
        Assert.Throws<InvalidOperationException>(() => session.Start(() => throw new InvalidOperationException()));
        Assert.That(session.Status, Is.EqualTo(MinigameStatus.Ended));
        Assert.That(session.TryComplete(MinigameResult.Win), Is.False);
        Assert.That(completions, Is.Zero);
    }

    [Test]
    public void Complete_BeforeStartOrWithoutResult_IsRejected()
    {
        Assert.That(session.TryComplete(MinigameResult.Win), Is.False);
        session.Start(null);
        Assert.That(session.TryComplete(MinigameResult.None), Is.False);
        Assert.That(session.Status, Is.EqualTo(MinigameStatus.Playing));
    }

    [Test]
    public void Continue_AfterDefeat_NewAttemptCanWinWithoutRedealing()
    {
        session.Start(null);
        session.TryComplete(MinigameResult.Lose);
        session.Dispose();
        session = new MementoMemorySession();
        int wins = 0;
        session.Completed += result => { if (result == MinigameResult.Win) wins++; };
        session.Start(null);
        Assert.That(session.TryComplete(MinigameResult.Win), Is.True);
        Assert.That(wins, Is.EqualTo(1));
    }
}
