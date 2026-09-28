using System.Collections.Generic;
using AnimalMemory.Progression;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

public sealed class MementoMenuCopyTests
{
    [TestCase("CIEN MEMORIAS")]
    [TestCase("cien memorias")]
    [TestCase("cinco sellos")]
    [TestCase("centésimo farol")]
    [TestCase("Capitán del Recuerdo")]
    [TestCase("último farol")]
    [TestCase("Cada sello estabiliza")]
    public void MenuCopy_DoesNotReintroduceRetiredCampaignPremise(string obsoleteText)
    {
        string copy = ReadStaticMenuCopy();

        Assert.That(copy, Does.Not.Contain(obsoleteText));
    }

    [Test]
    public void StoryPage_UsesCurrentCampaignTitleAndSixZoneGoal()
    {
        var root = LoadMenu();
        var story = root.Q<VisualElement>("page-story");

        Assert.That(story, Is.Not.Null);
        Assert.That(story.Q<Label>(className: "page-title").text.ToUpperInvariant(),
            Is.EqualTo(MementoMatchCampaignRules.CampaignTitle.ToUpperInvariant()));
        Assert.That(story.Q<Label>(className: "story-route-number").text, Is.EqualTo("6"));
        Assert.That(story.Q<Label>(className: "story-route-unit").text, Is.EqualTo("ZONAS"));
    }

    private static string ReadStaticMenuCopy()
    {
        var labels = new List<string>();
        LoadMenu().Query<Label>().ForEach(label => labels.Add(label.text));
        return string.Join("\n", labels);
    }

    private static TemplateContainer LoadMenu()
    {
        var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/AnimalMemory/UI/AnimalMemoryMenu.uxml");
        Assert.That(asset, Is.Not.Null);
        return asset.CloneTree();
    }
}
