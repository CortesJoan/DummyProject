using System.IO;
using System.Reflection;
using AnimalMemory.Progression;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// GameManager-level traversal of the released postgame («La Puerta Interior»)
/// state machine: start guards, active companion context, victory completion,
/// single-shot outcome consumption, the scripted Rei checkpoint through the
/// real public flow, and the true ending. The board itself is exercised by
/// MementoPostgameDuelIntegrationTests; what this proves is that the whole door
/// can be walked end to end with the flag on. The player's game.dat is never
/// touched (SaveSystem test override) and the release flag is always restored.
/// </summary>
public sealed class MementoPostgameTraversalTests
{
    private static readonly BindingFlags InstanceFlags =
        BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly BindingFlags StaticFlags =
        BindingFlags.Static | BindingFlags.NonPublic;

    private string saveOverridePath;

    [SetUp]
    public void SetUp()
    {
        MementoPostgameRelease.Enabled = true;
        saveOverridePath = Path.Combine(Path.GetTempPath(),
            "memento-postgame-traversal-" + System.Guid.NewGuid().ToString("N") + ".dat");
        typeof(SaveSystem).GetField("TestOnlySavePathOverride", StaticFlags)
            .SetValue(null, saveOverridePath);
    }

    [TearDown]
    public void TearDown()
    {
        MementoPostgameRelease.Enabled = false;
        typeof(SaveSystem).GetField("TestOnlySavePathOverride", StaticFlags)
            .SetValue(null, null);
        try { if (File.Exists(saveOverridePath)) File.Delete(saveOverridePath); }
        catch (IOException) { }
    }

    private static GameManager CreateManager(int campaignClearedMask, int postgameMask)
    {
        var host = new GameObject("Inactive postgame traversal");
        host.SetActive(false);
        var manager = host.AddComponent<GameManager>();
        // Awake never runs on an inactive host; savableObjects would stay null.
        typeof(GameManager).GetField("savableObjects", InstanceFlags)
            .SetValue(manager, new System.Collections.Generic.List<ISavable> { manager });
        var data = new GameData();
        data.SetData("ProgressionVersion", AnimalMemoryProgression.CurrentVersion);
        data.SetData("CampaignClearedMask", campaignClearedMask);
        data.SetData("PostgameVersion", 1);
        data.SetData("PostgameCompletedMask", postgameMask);
        manager.LoadData(data);
        return manager;
    }

    /// <summary>Mirrors StartPostgameEncounter's context without the board UI.</summary>
    private static void EnterEncounterContext(GameManager manager, int encounterId)
    {
        typeof(GameManager).GetField("activePostgameEncounter", InstanceFlags)
            .SetValue(manager, encounterId);
        typeof(GameManager).GetField("lastPostgameEncounter", InstanceFlags)
            .SetValue(manager, encounterId);
        typeof(GameManager).GetField("lastResultWasPostgame", InstanceFlags)
            .SetValue(manager, true);
        typeof(GameManager).GetField("lastResultWasCampaign", InstanceFlags)
            .SetValue(manager, false);
    }

    /// <summary>The exact handler OnWinGame runs for an active postgame duel.</summary>
    private static void WinActiveEncounter(GameManager manager) =>
        typeof(GameManager).GetMethod("FinishPostgameVictory", InstanceFlags)
            .Invoke(manager, null);

    private static int ReadSuppliesCoins(GameManager manager)
    {
        var data = new GameData();
        manager.SaveData(data);
        return data.GetData<int>("SuppliesCoins");
    }

    [Test]
    public void FullTraversal_FlagTrue_WalksTheDoorEndToEnd()
    {
        GameManager manager = CreateManager((1 << 21) - 1, 0);
        try
        {
            Assert.That(manager.IsPostgameAvailable, Is.True);
            Assert.That(manager.RecommendedPostgameEncounter, Is.EqualTo(0));

            // Encuentros 0..3: duelos de compañeras contra las Custodias.
            for (int id = 0; id < 4; id++)
            {
                Assert.That(manager.IsPostgameEncounterUnlocked(id), Is.True);
                Assert.That(manager.IsPostgameEncounterUnlocked(id + 1), Is.False,
                    "El siguiente encuentro no puede abrirse antes de tiempo.");
                EnterEncounterContext(manager, id);
                Assert.That(manager.IsPostgameActive, Is.True);
                Assert.That(manager.ActivePlayerGuideId,
                    Is.EqualTo(MementoPostgameEncounters.Get(id).PlayerGuideId),
                    "El control debe pertenecer a la compañera del encuentro.");
                WinActiveEncounter(manager);
                Assert.That(manager.PostgameCompletedMask, Is.EqualTo((1 << (id + 1)) - 1));
                Assert.That(manager.LastResultWasVictory, Is.True);
                Assert.That(manager.LastResultTitle, Is.EqualTo("CUSTODIA SUPERADA"));
                // El resultado pendiente se consume exactamente una vez.
                Assert.That(manager.TryConsumePostgameOutcome(), Is.True);
                Assert.That(manager.TryConsumePostgameOutcome(), Is.False);
                Assert.That(manager.RecommendedPostgameEncounter, Is.EqualTo(id + 1));
            }

            // Encuentro 4: checkpoint guionizado de Rei por la vía pública real.
            int requested = -1;
            manager.PostgameScriptedEncounterRequested += encounterId => requested = encounterId;
            Assert.That(manager.StartPostgameEncounter(4), Is.True);
            Assert.That(requested, Is.EqualTo(4));
            Assert.That(manager.IsPostgameActive, Is.True);
            Assert.That(manager.ActivePlayerGuideId, Is.EqualTo(5), "Rei acompaña su decisión.");
            Assert.That(manager.CompletePostgameScriptedEncounter(), Is.True);
            Assert.That((manager.PostgameCompletedMask >> 4) & 1, Is.EqualTo(1));
            Assert.That(manager.RecommendedPostgameEncounter, Is.EqualTo(5));

            // Encuentro 5: duelo final contra el Creador.
            EnterEncounterContext(manager, 5);
            WinActiveEncounter(manager);
            Assert.That(manager.LastResultTitle, Is.EqualTo("EL DESEO NOS PERTENECE"));
            Assert.That(manager.IsTrueEndingUnlocked, Is.True);
            Assert.That(manager.RecommendedPostgameEncounter, Is.EqualTo(-1));
            Assert.That(manager.TryConsumePostgameOutcome(), Is.True);
        }
        finally
        {
            Object.DestroyImmediate(manager.gameObject);
        }
    }

    [Test]
    public void ReplayOfCompletedEncounter_DoesNotDuplicateSupplyRewards()
    {
        GameManager manager = CreateManager((1 << 21) - 1, 0);
        try
        {
            EnterEncounterContext(manager, 0);
            WinActiveEncounter(manager);
            int coinsAfterFirstClear = ReadSuppliesCoins(manager);

            // Rejugada del mismo encuentro ya completado.
            EnterEncounterContext(manager, 0);
            WinActiveEncounter(manager);
            Assert.That(ReadSuppliesCoins(manager), Is.EqualTo(coinsAfterFirstClear),
                "La rejugada no vuelve a otorgar monedas de primera victoria.");
            Assert.That(manager.LastResultSummary, Does.Contain("REPLAY COMPLETADO"));
            Assert.That(manager.PostgameCompletedMask, Is.EqualTo(1),
                "La máscara no avanza por rejugadas.");
        }
        finally
        {
            Object.DestroyImmediate(manager.gameObject);
        }
    }

    [Test]
    public void NewEncounterStart_ClearsStaleOutcomeFromPreviousRun()
    {
        GameManager manager = CreateManager((1 << 21) - 1, 0);
        try
        {
            // Victoria sin consumir (p. ej. el jugador volvió a iniciar directo).
            EnterEncounterContext(manager, 0);
            WinActiveEncounter(manager);
            Assert.That(manager.TryConsumePostgameOutcome(), Is.True);

            // Una segunda victoria seguida sin nueva partida queda pendiente…
            EnterEncounterContext(manager, 1);
            WinActiveEncounter(manager);
            // …y un inicio posterior (aquí el checkpoint guionizado tras
            // completar 0..3) la sustituye en vez de acumularla.
            for (int id = 2; id < 4; id++)
            {
                EnterEncounterContext(manager, id);
                WinActiveEncounter(manager);
                Assert.That(manager.TryConsumePostgameOutcome(), Is.True);
            }
            manager.PostgameScriptedEncounterRequested += _ => { };
            Assert.That(manager.StartPostgameEncounter(4), Is.True);
            Assert.That(manager.TryConsumePostgameOutcome(), Is.False,
                "Iniciar un encuentro nuevo nunca presenta la victoria caduca del anterior.");
        }
        finally
        {
            Object.DestroyImmediate(manager.gameObject);
        }
    }

    [Test]
    public void FlagTrue_WithoutCompletedCampaign_KeepsTheDoorClosed()
    {
        GameManager manager = CreateManager(0, 0);
        try
        {
            Assert.That(manager.IsPostgameAvailable, Is.False);
            Assert.That(manager.RecommendedPostgameEncounter, Is.EqualTo(-1));
            Assert.That(manager.StartPostgameEncounter(0), Is.False,
                "Sin campaña completada ningún encuentro arranca aunque el flag esté activo.");
            Assert.That(manager.IsPostgameActive, Is.False);
            Assert.That(manager.PostgameCompletedMask, Is.EqualTo(0));
        }
        finally
        {
            Object.DestroyImmediate(manager.gameObject);
        }
    }
}
