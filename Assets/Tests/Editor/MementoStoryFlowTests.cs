using System.Collections.Generic;
using AnimalMemory.Progression;
using AnimalMemory.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public sealed class MementoStoryFlowTests
{
    private static readonly string[] BackgroundPaths =
    {
        "Assets/AnimalMemory/Art/StoryBackgrounds/story-bg-forest-v1.png",
        "Assets/AnimalMemory/Art/StoryBackgrounds/story-bg-coral-v1.png",
        "Assets/AnimalMemory/Art/StoryBackgrounds/story-bg-night-v1.png",
        "Assets/AnimalMemory/Art/StoryBackgrounds/story-bg-garden-v1.png",
        "Assets/AnimalMemory/Art/StoryBackgrounds/story-bg-sweets-v1.png"
    };

    [Test]
    public void EveryWorldHasARealSixteenByNineStoryBackground()
    {
        Assert.That(BackgroundPaths.Length,
            Is.EqualTo(MementoMatchCampaignRules.WorldCount));

        foreach (string path in BackgroundPaths)
        {
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(path);
            Assert.That(texture, Is.Not.Null, $"Missing story background: {path}");
            Assert.That(texture.width, Is.GreaterThan(texture.height));
            Assert.That(
                (float)texture.width / texture.height,
                Is.EqualTo(16f / 9f).Within(0.02f),
                $"{path} is not a 16:9 background.");
        }
    }

    [Test]
    public void RecruitSceneOffersLoadoutChoiceButReplayDoesNotChangeProgress()
    {
        MementoMatchStoryScene recruit =
            MementoMatchCampaignRules.GetStoryScene(2);
        MementoMatchStoryScene intro =
            MementoMatchCampaignRules.GetStoryScene(0);

        Assert.That(
            MementoStoryFlowRules.ShouldOfferGuideEquip(recruit),
            Is.True);
        Assert.That(
            MementoStoryFlowRules.ShouldOfferGuideEquip(intro),
            Is.False);

        HashSet<int> seen = new HashSet<int> { intro.Id };
        Assert.That(
            MementoStoryFlowRules.IsReplaySceneAvailable(
                intro.Id,
                seen.Contains),
            Is.True);
        Assert.That(
            MementoStoryFlowRules.IsReplaySceneAvailable(
                recruit.Id,
                seen.Contains),
            Is.False);
        Assert.That(
            MementoStoryFlowRules.IsReplayWorldAvailable(
                0,
                seen.Contains),
            Is.True);
        Assert.That(seen, Is.EquivalentTo(new[] { intro.Id }),
            "Checking replay availability must not mutate progression.");
    }

    [Test]
    public void StoryUiContainsBackgroundLoadoutAndReplayControls()
    {
        VisualTreeAsset template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/AnimalMemory/UI/AnimalMemoryMenu.uxml");
        Assert.That(template, Is.Not.Null);

        TemplateContainer root = template.Instantiate();
        Assert.That(root.Q<VisualElement>("story-background"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("guide-equip-overlay"), Is.Not.Null);
        Assert.That(root.Q<Button>("guide-equip-confirm"), Is.Not.Null);
        Assert.That(root.Q<Button>("guide-equip-keep"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("story-replay-overlay"), Is.Not.Null);
        Assert.That(root.Q<Button>("story-replay-open"), Is.Not.Null);
        Assert.That(root.Q<Button>("story-replay-scene-0"), Is.Not.Null);
        Assert.That(root.Q<Button>("story-replay-epilogue"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("tutorial-overlay"), Is.Not.Null);
        Assert.That(root.Q<Button>("tutorial-continue"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>("final-celebration-overlay"), Is.Not.Null);
        Assert.That(root.Q<Button>("final-equip-archive"), Is.Not.Null);
        Assert.That(root.Q<Button>("final-credits"), Is.Not.Null);
    }

    [Test]
    public void FinalCelebrationRequiresTheCompletedFinalCampaignLevel()
    {
        int finalLevel = MementoMatchCampaignRules.LevelCount - 1;

        Assert.That(
            MementoStoryFlowRules.ShouldPresentFinalCelebration(
                true,
                true,
                finalLevel),
            Is.True);
        Assert.That(
            MementoStoryFlowRules.ShouldPresentFinalCelebration(
                false,
                true,
                finalLevel),
            Is.False);
        Assert.That(
            MementoStoryFlowRules.ShouldPresentFinalCelebration(
                true,
                false,
                finalLevel),
            Is.False);
        Assert.That(
            MementoStoryFlowRules.ShouldPresentFinalCelebration(
                true,
                true,
                finalLevel - 1),
            Is.False);
    }

    [Test]
    public void ReplaySceneIdsMapEveryWorldToItsThreeChapters()
    {
        for (int world = 0; world < MementoMatchCampaignRules.WorldCount; world++)
        {
            for (int phase = 0; phase < 3; phase++)
            {
                Assert.That(
                    MementoStoryFlowRules.GetReplaySceneId(world, phase),
                    Is.EqualTo(world * 3 + phase));
            }
        }
    }
}
