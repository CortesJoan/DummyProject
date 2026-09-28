using System.Reflection;
using Minigames.Core;
using NUnit.Framework;
using UnityEngine;

/// <summary>Exercises the actual manager result/menu wiring without invoking Awake or SaveSystem.</summary>
public sealed class MementoMemorySessionIntegrationTests
{
    private GameObject host;
    private GameManager manager;
    private const BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic;

    [SetUp]
    public void SetUp()
    {
        host = new GameObject("Inactive minigame integration");
        host.SetActive(false);
        manager = host.AddComponent<GameManager>();
    }
    [TearDown] public void TearDown() => Object.DestroyImmediate(host);
    private void Invoke(string name, params object[] args) =>
        typeof(GameManager).GetMethod(name, Private).Invoke(manager, args);

    [Test]
    public void Defeat_ThroughFramework_PresentsResultExactlyOnce()
    {
        int results = 0;
        manager.ResultChanged += () => results++;
        Invoke("BeginMemorySession", new object[] { null });
        Assert.That(manager.MemorySessionStatus, Is.EqualTo(MinigameStatus.Playing));
        Invoke("FinishChallengeDefeat");
        Invoke("FinishChallengeDefeat");
        Assert.That(manager.MemorySessionStatus, Is.EqualTo(MinigameStatus.Ended));
        Assert.That(manager.IsResultOverlayOpen, Is.True);
        Assert.That(manager.LastResultWasVictory, Is.False);
        Assert.That(results, Is.EqualTo(1));
    }

    [Test]
    public void Menu_AbortsSession_RejectsLateVictory()
    {
        Invoke("BeginMemorySession", new object[] { null });
        int wins = manager.ActualWins;
        manager.ShowMainMenu();
        Invoke("OnWinGame");
        Assert.That(manager.MemorySessionStatus, Is.EqualTo(MinigameStatus.Ended));
        Assert.That(manager.IsMainMenuOpen, Is.True);
        Assert.That(manager.IsResultOverlayOpen, Is.False);
        Assert.That(manager.ActualWins, Is.EqualTo(wins));
    }

    [Test]
    public void NewAttempt_AfterDefeat_CanResolveAgain()
    {
        Invoke("BeginMemorySession", new object[] { null });
        Invoke("FinishChallengeDefeat");
        manager.ShowMainMenu();
        Invoke("BeginMemorySession", new object[] { null });
        Assert.That(manager.MemorySessionStatus, Is.EqualTo(MinigameStatus.Playing));
        Invoke("FinishChallengeDefeat");
        Assert.That(manager.MemorySessionStatus, Is.EqualTo(MinigameStatus.Ended));
    }
}
