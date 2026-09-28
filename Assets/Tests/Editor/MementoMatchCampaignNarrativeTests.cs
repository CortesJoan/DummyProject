using AnimalMemory.Progression;
using NUnit.Framework;

public sealed class MementoMatchCampaignNarrativeTests
{
    [Test]
    public void PersistentScenes_ExplainEachStepWithReadableBeats()
    {
        for (int sceneId = 0; sceneId <= MementoMatchCampaignRules.EpilogueSceneId; sceneId++)
        {
            MementoMatchStoryScene scene = MementoMatchCampaignRules.GetStoryScene(sceneId);
            Assert.That(scene.Beats.Count, Is.GreaterThanOrEqualTo(5),
                $"Scene {sceneId} needs enough beats to explain its objective and consequence.");

            for (int beatIndex = 0; beatIndex < scene.Beats.Count; beatIndex++)
            {
                MementoMatchStoryBeat beat = scene.Beats[beatIndex];
                Assert.That(beat.Speaker, Is.Not.Null.And.Not.Empty,
                    $"Scene {sceneId}, beat {beatIndex} has no speaker.");
                Assert.That(beat.Text, Is.Not.Null.And.Not.Empty,
                    $"Scene {sceneId}, beat {beatIndex} has no text.");
                Assert.That(beat.Text.Length, Is.LessThanOrEqualTo(240),
                    $"Scene {sceneId}, beat {beatIndex} should stay readable on mobile.");
            }
        }
    }

    [Test]
    public void WorldIntroductions_LetTheProtagonistExplainBeforeGuardianAppears()
    {
        for (int worldId = 0; worldId < MementoMatchCampaignRules.WorldCount; worldId++)
        {
            MementoMatchStoryScene scene =
                MementoMatchCampaignRules.GetStoryScene(worldId * 3);

            int firstGuardianBeat = -1;
            int firstProtagonistBeat = -1;
            for (int beatIndex = 0; beatIndex < scene.Beats.Count; beatIndex++)
            {
                if (firstProtagonistBeat < 0 &&
                    scene.Beats[beatIndex].Kind == MementoMatchStoryBeatKind.Protagonist)
                    firstProtagonistBeat = beatIndex;
                if (scene.Beats[beatIndex].GuideId >= 0)
                {
                    firstGuardianBeat = beatIndex;
                    break;
                }
            }

            int earliestAllowed = worldId == 0 ? 0 : 1;
            Assert.That(firstProtagonistBeat, Is.GreaterThanOrEqualTo(earliestAllowed));
            Assert.That(firstGuardianBeat, Is.GreaterThan(firstProtagonistBeat),
                $"World {worldId} introduces its guardian before the route and player goal are clear.");
        }
    }

    [Test]
    public void Prologue_DefinesTheArchiveAndMakesTheFirstLossConcrete()
    {
        MementoMatchStoryScene prologue =
            MementoMatchCampaignRules.GetStoryScene(0);

        string completeText = string.Empty;
        for (int beatIndex = 0; beatIndex < prologue.Beats.Count; beatIndex++)
        {
            MementoMatchStoryBeat beat = prologue.Beats[beatIndex];
            completeText += " " + beat.Text;
            Assert.That(beat.Speaker, Is.Not.EqualTo("RELATOR DEL ARCHIVO"));
        }

        // Jo: the world rules reach the player in plain words before the loss.
        bool protagonistEarly = false;
        int earlyWindow = prologue.Beats.Count < 3 ? prologue.Beats.Count : 3;
        for (int beatIndex = 0; beatIndex < earlyWindow; beatIndex++)
            protagonistEarly |= prologue.Beats[beatIndex].Kind == MementoMatchStoryBeatKind.Protagonist;
        Assert.That(protagonistEarly, Is.True,
            "The protagonist must introduce herself within the first beats.");
        Assert.That(completeText, Does.Contain("Archivo").And.Contain("tablero").IgnoreCase);
        Assert.That(completeText, Does.Contain("recuerdo").IgnoreCase);
        Assert.That(completeText, Does.Contain("Deseo"));
        Assert.That(completeText, Does.Contain("Zona Cero"));
        Assert.That(completeText, Does.Contain("Un minuto").IgnoreCase,
            "The first loss must feel concrete and timed.");
        Assert.That(completeText, Does.Contain("campana").IgnoreCase);
    }

    [Test]
    public void RecruitScenes_ExplainAllianceAbilityAndNextDestination()
    {
        for (int worldId = 0; worldId < MementoMatchCampaignRules.WorldCount; worldId++)
        {
            MementoMatchStoryScene scene =
                MementoMatchCampaignRules.GetStoryScene(worldId * 3 + 2);

            bool mentionsAlliance = false;
            bool explainsAbility = false;
            for (int beatIndex = 0; beatIndex < scene.Beats.Count; beatIndex++)
            {
                MementoMatchStoryBeat beat = scene.Beats[beatIndex];
                mentionsAlliance |= beat.Text.IndexOf("alianza", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    beat.Text.IndexOf("equipo", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    beat.Text.IndexOf("unidas", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    beat.Text.IndexOf("compañera", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    beat.Text.IndexOf("me uno", System.StringComparison.OrdinalIgnoreCase) >= 0 ||
                    beat.Text.IndexOf("a nuestro lado", System.StringComparison.OrdinalIgnoreCase) >= 0;
                explainsAbility |= beat.Kind == MementoMatchStoryBeatKind.Ability;
            }

            Assert.That(mentionsAlliance, Is.True,
                $"World {worldId} recruitment does not explain why the alliance changes.");
            Assert.That(explainsAbility, Is.True, $"World {worldId} recruitment does not explain its ability.");
            Assert.That(scene.Beats[scene.Beats.Count - 1].Kind,
                Is.EqualTo(MementoMatchStoryBeatKind.Narration),
                $"World {worldId} recruitment should end by revealing the next route.");
        }
    }

    [Test]
    public void RetryScene_ExplainsFailureCorrectionAndRestart()
    {
        MementoMatchStoryScene retry = MementoMatchCampaignRules.GetRetryScene(0);

        Assert.That(retry.Beats.Count, Is.GreaterThanOrEqualTo(4));
        Assert.That(retry.Beats[retry.Beats.Count - 1].Text,
            Does.Contain("vuelve a empezar"));
    }

    [Test]
    public void FirstChallenge_ContainsAnExplicitProposalAndExclusivePlayerPassive()
    {
        var challenge = MementoMatchCampaignRules.GetStoryScene(1);
        bool passive = false;
        foreach (var beat in challenge.Beats)
        {
            if (beat.Kind != MementoMatchStoryBeatKind.Protagonist)
                continue;
            passive |= beat.Text.Contains("Pulso del Relevo") &&
                beat.Text.Contains("sólo mía");
        }
        Assert.That(passive, Is.True, "The combo advantage belongs to the player alone.");

        // The alliance offer happens in the recruit scene, before Aki accepts.
        var recruit = MementoMatchCampaignRules.GetStoryScene(2);
        bool proposal = false;
        foreach (var beat in recruit.Beats)
        {
            if (beat.Kind != MementoMatchStoryBeatKind.Protagonist)
                continue;
            proposal |= beat.Text.Contains("No te pido tu bosque");
        }
        Assert.That(proposal, Is.True,
            "Aki must hear an offer that renounces her territory before joining.");
    }

    [Test]
    public void Epilogue_AllFiveAlliesSpeakBeforeWishActivation()
    {
        var scene = MementoMatchCampaignRules.GetStoryScene(
            MementoMatchCampaignRules.EpilogueSceneId);
        int allies = 0;
        bool activated = false;
        foreach (var beat in scene.Beats)
        {
            if (beat.Text.Contains("se encienden en dorado"))
            {
                Assert.That(allies, Is.EqualTo((1 << AnimalMemoryContentIds.GuideCount) - 1));
                activated = true;
            }
            if (beat.GuideId >= 0 && beat.GuideId < AnimalMemoryContentIds.GuideCount)
                allies |= 1 << beat.GuideId;
        }
        Assert.That(activated, Is.True);
    }

    [Test]
    public void Epilogue_ResolvesTheWishAndPlayerGoal()
    {
        MementoMatchStoryScene epilogue =
            MementoMatchCampaignRules.GetStoryScene(MementoMatchCampaignRules.EpilogueSceneId);

        string completeText = string.Empty;
        for (int beatIndex = 0; beatIndex < epilogue.Beats.Count; beatIndex++)
            completeText += " " + epilogue.Beats[beatIndex].Text;

        Assert.That(completeText, Does.Contain("regla").IgnoreCase);
        Assert.That(completeText, Does.Contain("seis").IgnoreCase);
        Assert.That(completeText, Does.Contain("juntas").IgnoreCase);
        Assert.That(completeText, Does.Contain("Zona Cero"));
        Assert.That(completeText, Does.Contain("{PLAYER}"));
    }
}
