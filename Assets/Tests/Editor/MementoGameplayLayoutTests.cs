using System.Collections.Generic;
using System.Reflection;
using NUnit.Framework;
using UnityEngine.UI;
using UnityEngine;

public sealed class MementoGameplayLayoutTests
{
    [TestCase(390, 844, 4, 4, false)]
    [TestCase(390, 844, 6, 5, true)]
    [TestCase(844, 390, 6, 5, true)]
    [TestCase(1920, 1080, 4, 4, true)]
    [TestCase(1080, 1920, 6, 5, true)]
    [TestCase(768, 1024, 4, 5, false)]
    [TestCase(320, 568, 6, 5, true)]
    public void BoardArea_ReservesHudAndFitsEveryCell(
        int width, int height, int rows, int columns, bool duel)
    {
        Rect safe = new Rect(0f, 20f, width, height - 40f);
        float header = safe.height * 0.16f;
        float footer = safe.height * (duel ? 0.22f : 0.1f);
        Rect area = MementoGameplayLayout.BoardArea(safe, header, footer, duel);
        var padding = new RectOffset(3, 3, 3, 3);
        Vector2 spacing = new Vector2(4f, 4f);
        Vector2 cells = MementoGameplayLayout.FitCells(area.size, rows, columns, spacing, padding);
        Vector2 grid = MementoGameplayLayout.GridSize(cells, rows, columns, spacing, padding);

        Assert.That(area.yMax, Is.LessThanOrEqualTo(safe.yMax - header));
        Assert.That(area.yMin, Is.GreaterThanOrEqualTo(safe.yMin + footer));
        Assert.That(area.xMin, Is.GreaterThanOrEqualTo(safe.xMin));
        Assert.That(area.xMax, Is.LessThanOrEqualTo(safe.xMax));
        Assert.That(grid.x, Is.LessThanOrEqualTo(area.width + 0.01f));
        Assert.That(grid.y, Is.LessThanOrEqualTo(area.height + 0.01f));
        Assert.That(cells.x / cells.y, Is.EqualTo(0.75f).Within(0.001f));
    }

    [Test]
    public void BoardArea_RotateAndReturn_RestoresGeometryWithoutHistory()
    {
        Rect portrait = new Rect(0, 24, 390, 796);
        Rect initial = MementoGameplayLayout.BoardArea(portrait, 140, 160, true);
        Rect landscape = MementoGameplayLayout.BoardArea(new Rect(24, 0, 796, 390), 55, 55, true);
        Rect final = MementoGameplayLayout.BoardArea(portrait, 140, 160, true);

        Assert.That(final, Is.EqualTo(initial));
        Assert.That(landscape.width, Is.GreaterThan(initial.width));
    }

    [Test]
    public void ValidSafeArea_NotchChangesWithoutOrientation_UpdatesUsableBounds()
    {
        Vector2 screen = new Vector2(844, 390);
        Rect leftNotch = MementoGameplayLayout.ValidSafeArea(screen, new Rect(40, 10, 804, 380));
        Rect rightNotch = MementoGameplayLayout.ValidSafeArea(screen, new Rect(0, 10, 804, 380));

        Assert.That(leftNotch.xMin, Is.EqualTo(40));
        Assert.That(rightNotch.xMin, Is.Zero);
        Assert.That(rightNotch.xMax, Is.EqualTo(804));
    }

    [TestCase(390, 844, true, 1f)]
    [TestCase(844, 390, false, 0.5f)]
    [TestCase(600, 600, false, 0.5f)]
    public void ScalePolicy_UsesViewportShapeRatherThanPlatformOrientation(
        int width, int height, bool portrait, float match)
    {
        Vector2 screen = new Vector2(width, height);
        Assert.That(MementoGameplayLayout.IsPortrait(screen), Is.EqualTo(portrait));
        Assert.That(MementoGameplayLayout.ScaleMatch(screen), Is.EqualTo(match));
    }

    [Test]
    public void ReflowExistingGrid_PreservesSelectedAndMatchedCardsAndTurnState()
    {
        var owner = new GameObject("InactiveLayoutTest");
        owner.SetActive(false);
        var parentObject = new GameObject("TestCanvas", typeof(RectTransform), typeof(Canvas));
        var gridObject = new GameObject("Grid", typeof(RectTransform), typeof(GridLayoutGroup));
        try
        {
            var match = owner.AddComponent<CardMatchUI>();
            var parent = parentObject.GetComponent<RectTransform>();
            parentObject.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var grid = gridObject.GetComponent<RectTransform>();
            grid.SetParent(parent, false);
            grid.anchorMin = Vector2.zero;
            grid.anchorMax = Vector2.one;
            var layout = gridObject.GetComponent<GridLayoutGroup>();
            layout.spacing = new Vector2(5, 5);
            SetField(match, "gridRect", grid);
            SetField(match, "gridLayout", layout);
            SetField(match, "gridRows", 2);
            SetField(match, "gridColumns", 2);
            SetField(match, "matches", 1);
            SetField(match, "turns", 7);
            SetField(match, "isResolvingMatch", true);
            var cards = GetField<List<Card>>(match, "cards");
            var states = GetField<Dictionary<Card, CardState>>(match, "cardStates");
            var selected = GetField<List<Card>>(match, "flippedCards");
            for (int i = 0; i < 4; i++)
            {
                var cardObject = new GameObject("Card" + i, typeof(RectTransform), typeof(Image), typeof(Card));
                cardObject.transform.SetParent(grid, false);
                Card card = cardObject.GetComponent<Card>();
                SetField(card, "cardRenderer", cardObject.GetComponent<Image>());
                cards.Add(card);
                states.Add(card, i < 2 ? CardState.Matched : CardState.FaceDown);
            }
            states[cards[2]] = CardState.FaceUp;
            selected.Add(cards[2]);
            Card[] originalCards = cards.ToArray();
            MethodInfo reflow = typeof(CardMatchUI).GetMethod(
                "ApplyResponsiveGridLayout", BindingFlags.Instance | BindingFlags.NonPublic);

            match.SetPresentationViewport(
                new Rect(0, 0, Screen.width, Screen.height), 80, 120);
            reflow.Invoke(match, null);
            parent.localScale = new Vector3(0.6f, 0.6f, 1f);
            reflow.Invoke(match, null);
            parent.localScale = Vector3.one;
            reflow.Invoke(match, null);

            CollectionAssert.AreEqual(originalCards, cards);
            Assert.That(match.Matches, Is.EqualTo(1));
            Assert.That(match.Turns, Is.EqualTo(7));
            Assert.That(match.IsBusy, Is.True);
            Assert.That(states[cards[0]], Is.EqualTo(CardState.Matched));
            Assert.That(states[cards[2]], Is.EqualTo(CardState.FaceUp));
            Assert.That(selected, Has.Count.EqualTo(1));
            Assert.That(selected[0], Is.SameAs(originalCards[2]));
            Assert.That(grid.anchorMin, Is.EqualTo(grid.anchorMax));
            Assert.That(layout.cellSize.x, Is.GreaterThan(0f));
        }
        finally
        {
            Object.DestroyImmediate(owner);
            Object.DestroyImmediate(parentObject);
        }
    }

    private static void SetField(object instance, string field, object value)
    {
        instance.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(instance, value);
    }

    private static T GetField<T>(object instance, string field)
    {
        return (T)instance.GetType().GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(instance);
    }

    [Test]
    public void PlaceInParent_RemovesStretchAnchorsAndPreservesRequestedBounds()
    {
        var parentObject = new GameObject("LayoutTestParent", typeof(RectTransform));
        var childObject = new GameObject("LayoutTestGrid", typeof(RectTransform));
        try
        {
            var parent = parentObject.GetComponent<RectTransform>();
            var child = childObject.GetComponent<RectTransform>();
            parent.sizeDelta = new Vector2(600, 1000);
            child.SetParent(parent, false);
            child.anchorMin = Vector2.zero;
            child.anchorMax = Vector2.one;
            Rect requested = new Rect(-200, -300, 400, 500);

            MementoGameplayLayout.PlaceInParent(child, requested);
            parent.sizeDelta = new Vector2(1000, 600);

            Assert.That(child.anchorMin, Is.EqualTo(child.anchorMax));
            Assert.That(child.rect.size, Is.EqualTo(requested.size));
            Assert.That(child.anchoredPosition, Is.EqualTo(requested.center));
        }
        finally
        {
            Object.DestroyImmediate(parentObject);
        }
    }
}
