using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

public sealed class MementoSkillReadingTests
{
    [TestCase(0f, 3.6f)]
    [TestCase(5f, 5.3f)]
    [TestCase(20f, 8f)]
    public void Announcement_HasReadableMinimumAndBoundedVoiceDuration(float voice, float expected)
    {
        Assert.That(MementoSkillPresentationRules.Duration(voice, ""), Is.EqualTo(expected).Within(.001f));
    }
    [Test]
    public void LongCaption_GetsMoreTime()
    {
        Assert.That(MementoSkillPresentationRules.Duration(0, new string('a', 120)), Is.GreaterThan(5f));
    }
    [TestCase(0)] [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)]
    public void EveryPower_HasAnExplicitEffectCaption(int guide)
    {
        Assert.That(MementoSkillPresentationRules.Effect(guide, false, false), Is.Not.Empty);
        Assert.That(MementoSkillPresentationRules.Effect(guide, true, false), Is.Not.Empty);
        Assert.That(MementoSkillPresentationRules.Effect(guide, true, true), Is.Not.Empty);
    }
    [Test]
    public void CutIn_HasIntegratedSubtitle_AndNoUnexplainedSpeedLines()
    {
        var root = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/AnimalMemory/UI/AnimalMemoryMenu.uxml").CloneTree();
        Assert.That(root.Q<Label>("skill-cut-in-quote"), Is.Not.Null);
        Assert.That(root.Q<VisualElement>(className: "cut-in-speed-line"), Is.Null);
    }
}
