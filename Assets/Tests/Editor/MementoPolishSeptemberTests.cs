using System;
using System.Collections.Generic;
using System.Linq;
using AnimalMemory.Progression;
using AnimalMemory.UI;
using EHKP.GameAnalytics;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

public sealed class MementoPolishSeptemberTests
{
    [Test]
    public void Encore_FirstMismatchProtectsCombo_ThenConsumesExactlyOnce()
    {
        var state = new AnimalMemoryGuideRunState();
        state.Reset(AnimalMemoryContentIds.MomoGuide);
        var phases = new List<bool>();
        state.PowerPresentationRequested += (_, proc) => phases.Add(proc);
        Assert.That(state.TryActivatePower(), Is.True);
        Assert.That(state.TryProtectCombo(3), Is.True);
        Assert.That(state.MomoEncoreArmed, Is.True);
        Assert.That(phases, Is.EqualTo(new[] { false }));
        Assert.That(state.TryConsumeEncore(), Is.True);
        Assert.That(state.TryProtectCombo(3), Is.False);
        Assert.That(state.TryConsumeEncore(), Is.False);
        Assert.That(phases, Is.EqualTo(new[] { false, true }));
        Assert.That(state.PowerCooldownRemaining, Is.EqualTo(AnimalMemoryGuideRunState.MomoCooldownTurns));
    }

    [TestCase(0)]
    [TestCase(9)]
    [TestCase(41)]
    [TestCase(108)]
    public void PracticeBoard_HasTwoDistinctPairs_AndCompletesOnlyOnMatches(int seed)
    {
        var board = new MementoPracticeBoard(seed);
        Assert.That(Enumerable.Range(0, 4).Select(board.Identity).GroupBy(x => x).All(g => g.Count() == 2), Is.True);
        Assert.That(board.TryFlip(-1), Is.False);
        for (int identity = 0; identity < 2; identity++)
        {
            int[] pair = Enumerable.Range(0, 4).Where(i => board.Identity(i) == identity).ToArray();
            Assert.That(board.TryFlip(pair[0]), Is.True);
            Assert.That(board.TryFlip(pair[0]), Is.False);
            Assert.That(board.TryFlip(pair[1]), Is.True);
            Assert.That(board.TryFlip((pair[1] + 1) % 4), Is.False);
            Assert.That(board.Resolve(), Is.True);
        }
        Assert.That(board.IsComplete, Is.True);
        Assert.That(board.Pairs, Is.EqualTo(2));
    }

    [Test]
    public void PracticeBoard_MismatchReclosesCards_WithoutAwardingPair()
    {
        var board = new MementoPracticeBoard(10);
        int a = 0, b = Enumerable.Range(1, 3).First(i => board.Identity(i) != board.Identity(a));
        board.TryFlip(a); board.TryFlip(b);
        Assert.That(board.Resolve(), Is.False);
        Assert.That(board.Pairs, Is.Zero);
        Assert.That(board.IsVisible(a), Is.False);
        Assert.That(board.TryFlip(a), Is.True);
    }

    [Test]
    public void OutcomeFlags_SeparateRivalsAndWinLoss()
    {
        var bits = new HashSet<int>();
        for (int guide = 0; guide < 6; guide++)
        {
            bits.Add(MementoDefeatPresentation.OutcomeBit(guide, true));
            bits.Add(MementoDefeatPresentation.OutcomeBit(guide, false));
        }
        Assert.That(bits.Count, Is.EqualTo(12));
        Assert.That(MementoDefeatPresentation.OutcomeBit(-1, false), Is.Zero);
    }

    [Test]
    public void Achievements_HaveVerticalScroll_AndKeepCloseOutside()
    {
        var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>("Assets/AnimalMemory/UI/AnimalMemoryMenu.uxml");
        var root = asset.CloneTree();
        var scroll = root.Q<ScrollView>("achievements-scroll");
        Assert.That(scroll, Is.Not.Null);
        Assert.That(scroll.Q<VisualElement>("achievement-list"), Is.Not.Null);
        Assert.That(scroll.Q<Button>("achievements-close"), Is.Null);
        Assert.That(root.Q<Button>("achievements-close"), Is.Not.Null);
    }

    [Test]
    public void MikaChallenge_ExplainsAkiComboSynergy()
    {
        string text = string.Join(" ", MementoMatchCampaignRules.GetStoryScene(4).Beats.Select(x => x.Text));
        Assert.That(text, Does.Contain("Deshacer de Aki").And.Contain("daño máximo").And.Contain("combo anterior"));
    }

    private static TelemetryEvent Entry(long time, string name = "level_start") => new TelemetryEvent
    {
        name = name, utcSeconds = time, eventId = Guid.NewGuid().ToString("N"),
        installationId = Guid.NewGuid().ToString("N"), sessionId = Guid.NewGuid().ToString("N"),
        build = "test", platform = "test"
    };

    [Test]
    public void Telemetry_WithoutConsentDoesNotCollect_RevocationClears()
    {
        var queue = new TelemetryQueue(() => 2000000);
        Assert.That(queue.Enqueue(Entry(2000000)), Is.False);
        queue.SetConsent(true); Assert.That(queue.Enqueue(Entry(2000000)), Is.True);
        queue.SetConsent(false); Assert.That(queue.Count, Is.Zero);
    }

    [Test]
    public void Telemetry_QueueBounded_DeduplicatesAndAcknowledgesOnlyGivenIds()
    {
        var queue = new TelemetryQueue(() => 2000000); queue.SetConsent(true);
        var first = Entry(2000000);
        queue.Enqueue(first);
        Assert.That(queue.Enqueue(first), Is.False);
        for (int i = 0; i < 520; i++) queue.Enqueue(Entry(2000000));
        Assert.That(queue.Count, Is.EqualTo(512));
        Assert.That(queue.Dropped, Is.EqualTo(9));
        var batch = queue.Peek(100);
        Assert.That(batch.Length, Is.EqualTo(32));
        queue.Acknowledge(batch.Take(3).Select(x => x.eventId));
        Assert.That(queue.Count, Is.EqualTo(509));
    }

    [Test]
    public void Telemetry_ExpiresAndRejectsBadEvents()
    {
        long now = 2000000;
        var queue = new TelemetryQueue(() => now); queue.SetConsent(true);
        Assert.That(queue.Enqueue(Entry(now, "player name")), Is.False);
        Assert.That(queue.Enqueue(Entry(now - TelemetryQueue.RetentionSeconds - 1)), Is.False);
        Assert.That(queue.Enqueue(Entry(now + 600)), Is.False);
        queue.Enqueue(Entry(now));
        now += TelemetryQueue.RetentionSeconds + 1;
        Assert.That(queue.Peek(32), Is.Empty);
    }

    [TestCase("http://example.com/events", false)]
    [TestCase("https://example.com/events", true)]
    [TestCase("https://user:secret@example.com/events", false)]
    [TestCase("https://example.com/events?token=secret", false)]
    [TestCase("", false)]
    public void Telemetry_EndpointRequiresHttpsWithoutEmbeddedSecrets(string endpoint, bool expected)
    {
        Assert.That(TelemetryQueue.ValidEndpoint(endpoint), Is.EqualTo(expected));
    }
}
