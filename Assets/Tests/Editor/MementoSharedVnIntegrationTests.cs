using System;
using AnimalMemory.Narrative;
using AnimalMemory.Progression;
using NUnit.Framework;
using UnityEditor.PackageManager;
using VN;

[TestFixture]
public sealed class MementoSharedVnIntegrationTests
{
    [Test]
    public void DialogueEngine_ResolvesFromOriginalSharedUpmPackage()
    {
        var package = PackageInfo.FindForAssembly(typeof(DialogueEngine).Assembly);
        Assert.That(package, Is.Not.Null);
        Assert.That(package.name, Is.EqualTo("com.studio.vn-engine"));
    }

    [Test]
    public void Runner_PlayEntireScene_PreservesBeatMetadataAndStopsAtEnd()
    {
        var scene = MementoMatchCampaignRules.GetStoryScene(0);
        var runner = new MementoNarrativeRunner();
        runner.Begin(scene);
        for (int i = 0; i < scene.Beats.Count; i++)
        {
            Assert.That(runner.BeatIndex, Is.EqualTo(i));
            Assert.That(runner.CurrentBeat.Text, Is.EqualTo(scene.Beats[i].Text));
            Assert.That(runner.CurrentBeat.IsSilhouette, Is.EqualTo(scene.Beats[i].IsSilhouette));
            Assert.That(runner.MoveNext(), Is.EqualTo(i + 1 < scene.Beats.Count));
        }
        Assert.That(runner.MoveNext(), Is.False);
        runner.End();
        Assert.That(runner.IsRunning, Is.False);
        Assert.Throws<InvalidOperationException>(() => { var ignored = runner.CurrentBeat; });
    }

    [Test]
    public void SharedEnginePackage_ContainsNoNarrativeImagesOrVoiceClips()
    {
        string[] scope = { "Packages/com.studio.vn-engine" };
        Assert.That(UnityEditor.AssetDatabase.FindAssets("t:Texture2D", scope), Is.Empty);
        Assert.That(UnityEditor.AssetDatabase.FindAssets("t:AudioClip", scope), Is.Empty);
    }

    [Test]
    public void Runner_DialogueOnly_DoesNotCreateLegacySceneServices()
    {
        int storyStates = UnityEngine.Resources.FindObjectsOfTypeAll<StoryState>().Length;
        int dayManagers = UnityEngine.Resources.FindObjectsOfTypeAll<DayManager>().Length;
        int saveServices = UnityEngine.Resources.FindObjectsOfTypeAll<SaveLoadService>().Length;
        int audioServices = UnityEngine.Resources.FindObjectsOfTypeAll<AudioService>().Length;
        var runner = new MementoNarrativeRunner();
        runner.Begin(MementoMatchCampaignRules.GetStoryScene(0));
        while (runner.MoveNext()) { }
        runner.End();
        Assert.That(UnityEngine.Resources.FindObjectsOfTypeAll<StoryState>().Length, Is.EqualTo(storyStates));
        Assert.That(UnityEngine.Resources.FindObjectsOfTypeAll<DayManager>().Length, Is.EqualTo(dayManagers));
        Assert.That(UnityEngine.Resources.FindObjectsOfTypeAll<SaveLoadService>().Length, Is.EqualTo(saveServices));
        Assert.That(UnityEngine.Resources.FindObjectsOfTypeAll<AudioService>().Length, Is.EqualTo(audioServices));
    }

    [Test]
    public void Runner_ReplayAndReplacement_StartAtFirstBeat()
    {
        var runner = new MementoNarrativeRunner();
        runner.Begin(MementoMatchCampaignRules.GetStoryScene(0));
        runner.MoveNext();
        runner.Begin(MementoMatchCampaignRules.GetStoryScene(1));
        Assert.That(runner.BeatIndex, Is.Zero);
        Assert.That(runner.Scene.Id, Is.EqualTo(1));
        runner.End();
        runner.Begin(MementoMatchCampaignRules.GetStoryScene(0));
        Assert.That(runner.BeatIndex, Is.Zero);
        runner.End();
    }

    [Test]
    public void Runner_DoesNotTriggerUnrelatedVnAudioOrSaveServices()
    {
        int broadcasts = 0;
        Action<Command> handler = _ => broadcasts++;
        DialogueEngine.OnCommandExecuted += handler;
        try
        {
            var runner = new MementoNarrativeRunner();
            runner.Begin(MementoMatchCampaignRules.GetStoryScene(0));
            while (runner.MoveNext()) { }
            runner.End();
            Assert.That(broadcasts, Is.Zero);
        }
        finally { DialogueEngine.OnCommandExecuted -= handler; }
    }
}
