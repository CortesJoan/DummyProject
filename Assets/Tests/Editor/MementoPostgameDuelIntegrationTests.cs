using System.Reflection;
using AnimalMemory.Progression;
using MementoMatch.Duel;
using NUnit.Framework;
using UnityEngine;

/// <summary>Isolated runtime boundaries: never initializes a live manager or writes SaveSystem.</summary>
public sealed class MementoPostgameDuelIntegrationTests
{
    [TestCase(6)] [TestCase(7)] [TestCase(8)] [TestCase(9)] [TestCase(10)]
    public void NewOpponent_UsesOwnProfile_AndStillResolvesExhaustedBoards(int id)
    {
        Assert.That(MementoPostgameEncounters.TryGetOpponent(id, out var profile), Is.True);
        var challenge = new GuideChallengeState();
        challenge.Begin(id);
        Assert.That(challenge.OpponentGuideId, Is.EqualTo(id));
        Assert.That(challenge.IsActive, Is.True);
        Assert.That(DuelRules.GetSkillInterval(id), Is.EqualTo(profile.SkillInterval));
        Assert.That(DuelRules.GetInitialMemoryLimit(id), Is.EqualTo(profile.MemoryLimit));
        Assert.That(DuelRules.GetForgetChance(id), Is.EqualTo(profile.ForgetChance).Within(.00001));
        var state = new DuelState();
        state.Begin(id, 18);
        Assert.That(state.OpponentGuideId, Is.EqualTo(id));
        Assert.That(state.TargetPairs, Is.EqualTo(profile.Health));
        state.CompleteTurn(DuelActor.Player, true);
        state.CompleteTurn(DuelActor.Opponent, true, damage: 99);
        Assert.That(state.PlayerHealth, Is.EqualTo(profile.Health - 1), "Rivals do not inherit the protagonist passive.");
        state.ResolveBoardExhausted(DuelActor.Opponent);
        Assert.That(DuelRules.GetTerminalAction(state), Is.EqualTo(DuelTerminalAction.SuddenDeath));
        state.BeginSuddenDeathRound();
        Assert.That(state.ActiveActor, Is.EqualTo(DuelActor.Player));
        Assert.That(state.IsActive, Is.True);
    }

    [TestCase(6)] [TestCase(7)] [TestCase(8)] [TestCase(9)] [TestCase(10)]
    public void MissingNewVoice_NeverPlaysOrSubtitlesRei(int id)
    {
        int events = 0;
        System.Action<MementoMatchVoiceCue> listener = cue => events++;
        MementoMatchVoicePlayer.LineStarted += listener;
        try
        {
            Assert.That(MementoMatchVoicePlayer.PlayMenu(id), Is.Zero);
            Assert.That(MementoMatchVoicePlayer.PlayLocked(id), Is.Zero);
            Assert.That(MementoMatchVoicePlayer.PlayTurn(id), Is.Zero);
            Assert.That(MementoMatchVoicePlayer.PlayDamage(id), Is.Zero);
            Assert.That(MementoMatchVoicePlayer.PlaySkill(id), Is.Zero);
            foreach (bool won in new[] { false, true })
            {
                Assert.That(MementoMatchVoicePlayer.PlayOutcome(id, won), Is.Zero);
                Assert.That(MementoMatchVoicePlayer.GetOutcomeDuration(id, won), Is.Zero);
                Assert.That(MementoMatchVoicePlayer.GetOutcomeSubtitle(id, won), Is.Empty);
            }
            Assert.That(events, Is.Zero);
        }
        finally { MementoMatchVoicePlayer.LineStarted -= listener; }
    }

    [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)] [TestCase(5)]
    public void EncounterBoard_HasEnoughDistinctRuntimeCardFaces(int id)
    {
        var host = new GameObject("Inactive postgame card capacity");
        host.SetActive(false);
        try
        {
            var board = host.AddComponent<CardMatchUI>();
            var encounter = MementoPostgameEncounters.Get(id);
            var sprites = board.GetCardSetPreview(encounter.SetId);
            Assert.That(sprites, Is.Not.Null);
            Assert.That(sprites.Count, Is.GreaterThanOrEqualTo(encounter.Rows * encounter.Columns / 2));
            var unique = new System.Collections.Generic.HashSet<Sprite>(sprites);
            Assert.That(unique.Contains(null), Is.False);
            Assert.That(unique.Count, Is.EqualTo(sprites.Count));
        }
        finally { Object.DestroyImmediate(host); }
    }

    [Test]
    public void ClosedRelease_RejectsAllEncounterStarts_WithoutMutatingProfileOrRequestingPresentation()
    {
        var host = new GameObject("Inactive postgame entry regression");
        host.SetActive(false);
        try
        {
            var manager = host.AddComponent<GameManager>();
            var data = new GameData();
            data.SetData("ProgressionVersion", AnimalMemoryProgression.CurrentVersion);
            data.SetData("CampaignClearedMask", (1 << 21) - 1);
            data.SetData("PostgameVersion", 1);
            data.SetData("PostgameCompletedMask", 63);
            data.SetData("PawStars", 90);
            manager.LoadData(data);
            int requests = 0;
            manager.PostgameScriptedEncounterRequested += id => requests++;
            foreach (int id in new[] { -1, 0, 1, 2, 3, 4, 5, 6, int.MaxValue })
                Assert.That(manager.StartPostgameEncounter(id), Is.False);
            Assert.That(requests, Is.Zero);
            Assert.That(manager.IsPostgameActive, Is.False);
            Assert.That(manager.LastResultWasPostgame, Is.False);
            Assert.That(manager.LastPostgameEncounter, Is.EqualTo(-1));
            Assert.That(manager.PostgameCompletedMask, Is.EqualTo(63));
            Assert.That(manager.PawStars, Is.EqualTo(90));
            Assert.That(manager.CampaignEndingUnlocked, Is.True);
        }
        finally { Object.DestroyImmediate(host); }
    }

    [Test]
    public void ReleaseSwitch_ClosesAndReopensTheDoorExactly()
    {
        var host = new GameObject("Inactive postgame release cycle");
        host.SetActive(false);
        try
        {
            var manager = host.AddComponent<GameManager>();
            var data = new GameData();
            data.SetData("ProgressionVersion", AnimalMemoryProgression.CurrentVersion);
            data.SetData("CampaignClearedMask", (1 << 21) - 1);
            data.SetData("PostgameVersion", 1);
            data.SetData("PostgameCompletedMask", 0);
            manager.LoadData(data);

            // El valor de entrega mantiene la puerta cerrada...
            Assert.That(MementoPostgameRelease.Enabled, Is.False);
            Assert.That(manager.IsPostgameAvailable, Is.False);
            Assert.That(manager.IsTrueEndingUnlocked, Is.False);
            Assert.That(manager.RecommendedPostgameEncounter, Is.EqualTo(-1));

            // ...una actualizacion futura la abre con el unico interruptor...
            MementoPostgameRelease.Enabled = true;
            Assert.That(manager.IsPostgameAvailable, Is.True);
            Assert.That(manager.IsPostgameEncounterUnlocked(0), Is.True);
            Assert.That(manager.RecommendedPostgameEncounter, Is.EqualTo(0));

            // ...y volver a cerrarla no deja ningun estado a medio abrir. No se
            // afirma StartPostgameEncounter en la fase abierta: en un host
            // inactivo no hay dificultad ni tablero, y esa guarda es previa.
            MementoPostgameRelease.Enabled = false;
            Assert.That(manager.IsPostgameAvailable, Is.False);
            Assert.That(manager.IsTrueEndingUnlocked, Is.False);
            Assert.That(manager.RecommendedPostgameEncounter, Is.EqualTo(-1));
            for (int id = 0; id < MementoPostgameEncounters.Count; id++)
            {
                Assert.That(manager.IsPostgameEncounterUnlocked(id), Is.False, "Encuentro " + id);
                Assert.That(manager.StartPostgameEncounter(id), Is.False, "Encuentro " + id);
            }

            Assert.That(manager.IsPostgameActive, Is.False);
            Assert.That(manager.PostgameCompletedMask, Is.EqualTo(0),
                "Abrir y cerrar la puerta no puede otorgar progreso postgame.");
        }
        finally
        {
            MementoPostgameRelease.Enabled = false;
            Object.DestroyImmediate(host);
        }
    }

    [Test]
    public void InvalidMainRequests_DoNotEraseAnActivePostgameContext()
    {
        var host = new GameObject("Inactive invalid request regression");
        host.SetActive(false);
        try
        {
            var manager = host.AddComponent<GameManager>();
            const BindingFlags fields = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(GameManager).GetField("activePostgameEncounter", fields).SetValue(manager, 2);
            typeof(GameManager).GetField("lastResultWasPostgame", fields).SetValue(manager, true);
            Assert.That(manager.StartCampaignLevel(20), Is.False);
            Assert.That(manager.StartGuideChallenge(4), Is.False);
            Assert.That(manager.ActivePostgameEncounter, Is.EqualTo(2));
            Assert.That(manager.LastResultWasPostgame, Is.True);
        }
        finally { Object.DestroyImmediate(host); }
    }
}
