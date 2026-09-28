using AnimalMemory.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

public sealed class MementoTutorialFlowTests
{
    [TestCase(false, 0, true)]
    [TestCase(false, 1, false)]
    [TestCase(false, -1, false)]
    [TestCase(true, 0, false)]
    public void TryBegin_OnlyPendingFirstLevel_StartsBasicLesson(
        bool completed, int level, bool expected)
    {
        var flow = new MementoTutorialFlow();

        Assert.That(flow.TryBegin(completed, level), Is.EqualTo(expected));
    }

    [Test]
    public void ObservePlayerMove_HintsAreNeverDismissed_TwoPairsCompleteBasicsWithoutGuideLesson()
    {
        var flow = new MementoTutorialFlow();
        flow.TryBegin(false, 0);

        Assert.That(flow.ObservePlayerMove(false), Is.False);
        Assert.That(flow.ObservePlayerMove(true), Is.True);
        Assert.That(flow.Stage, Is.EqualTo(MementoTutorialStage.ComboLesson));
        Assert.That(flow.ObservePlayerMove(true), Is.True);
        Assert.That(flow.Stage, Is.EqualTo(MementoTutorialStage.Complete));
        Assert.That(flow.IsOverlayVisible, Is.False);
    }

    [Test]
    public void Continue_DismissesOnlyHint_GameplayStillAdvancesBasicLesson()
    {
        var flow = new MementoTutorialFlow();
        flow.TryBegin(false, 0);

        Assert.That(flow.Continue(), Is.False);
        Assert.That(flow.IsOverlayVisible, Is.False);
        Assert.That(flow.IsWaitingForMove, Is.True);
        Assert.That(flow.ObservePlayerMove(true), Is.True);
        Assert.That(flow.Continue(), Is.False);
        Assert.That(flow.Stage, Is.EqualTo(MementoTutorialStage.FindSecondPair));
        Assert.That(flow.ObservePlayerMove(false), Is.False);
        Assert.That(flow.ObservePlayerMove(true), Is.True);
        Assert.That(flow.Stage, Is.EqualTo(MementoTutorialStage.Complete));
    }

    [TestCase(false, -1, false, true, false)]
    [TestCase(false, 0, false, true, false)]
    [TestCase(false, 0, true, false, false)]
    [TestCase(true, 0, true, true, false)]
    [TestCase(false, 0, true, true, true)]
    [TestCase(false, 4, true, true, true)]
    public void TryBeginGuideLesson_RequiresUnseenEquippedUnlockedUsablePower(
        bool completed, int equippedGuide, bool unlocked, bool usable, bool expected)
    {
        var flow = new MementoTutorialFlow();

        Assert.That(
            flow.TryBeginGuideLesson(completed, equippedGuide, unlocked, usable),
            Is.EqualTo(expected));
    }

    [Test]
    public void TryBeginGuideLesson_BasicsInProgress_DoesNotReplaceMatchingHint()
    {
        var flow = new MementoTutorialFlow();
        flow.TryBegin(false, 0);

        Assert.That(flow.TryBeginGuideLesson(false, 0, true, true), Is.False);
        Assert.That(flow.Stage, Is.EqualTo(MementoTutorialStage.Introduction));
    }

    [Test]
    public void TryBeginGuideLesson_BasicsAlreadyCompleted_UsesIndependentCompletionFlag()
    {
        var flow = new MementoTutorialFlow();
        Assert.That(flow.TryBegin(true, 0), Is.False);

        Assert.That(flow.TryBeginGuideLesson(false, 0, true, true), Is.True);
        Assert.That(flow.Stage, Is.EqualTo(MementoTutorialStage.GuideLesson));
        Assert.That(flow.Continue(), Is.True);
        flow.Reset();
        Assert.That(flow.TryBeginGuideLesson(true, 0, true, true), Is.False);
    }

    [Test]
    public void Reset_InterruptedLesson_DoesNotMarkAbilityCompleted()
    {
        var flow = new MementoTutorialFlow();
        flow.TryBeginGuideLesson(false, 0, true, true);

        flow.Reset();

        Assert.That(flow.Stage, Is.EqualTo(MementoTutorialStage.Inactive));
        Assert.That(flow.IsOverlayVisible, Is.False);
        Assert.That(flow.IsWaitingForMove, Is.False);
        Assert.That(flow.TryBeginGuideLesson(false, 0, true, true), Is.True);
    }

    [Test]
    public void TutorialVisualTree_IsConfinedToHeader_AndHasNoBlockingShade()
    {
        var asset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
            "Assets/AnimalMemory/UI/AnimalMemoryMenu.uxml");
        Assert.That(asset, Is.Not.Null);
        var root = asset.CloneTree();
        var hint = root.Q<VisualElement>("tutorial-overlay");

        Assert.That(hint, Is.Not.Null);
        Assert.That(hint.parent.ClassListContains("hud-top"), Is.True);
        Assert.That(hint.pickingMode, Is.EqualTo(PickingMode.Ignore));
        Assert.That(root.Q<VisualElement>(className: "tutorial-shade"), Is.Null);
        hint.Query<VisualElement>().ForEach(element =>
        {
            if (!(element is Button))
                Assert.That(element.pickingMode, Is.EqualTo(PickingMode.Ignore),
                    "Tutorial decoration must not intercept card input: " + element.name);
        });
    }
}
