using System;
using NUnit.Framework;
using UnityEngine;

public class MementoIllustratedAtlasLayoutTests
{
    [Test]
    public void ReadingOrderStartsAtTopLeftWithoutRotating()
    {
        var pixels = new Color32[100];
        foreach (int index in new[] { 22, 27, 72, 77 })
            pixels[index] = new Color32(255, 255, 255, 255);
        var rects = MementoIllustratedAtlasLayout.BuildRects(pixels, 10, 10, 2, 2);
        Assert.That(rects[0], Is.EqualTo(new RectInt(0, 5, 5, 5)));
        Assert.That(rects[1], Is.EqualTo(new RectInt(5, 5, 5, 5)));
        Assert.That(rects[2], Is.EqualTo(new RectInt(0, 0, 5, 5)));
        Assert.That(pixels[72].a, Is.EqualTo(255));
    }

    [Test]
    public void ShiftsBoundaryToAvoidPaint()
    {
        var pixels = new Color32[400];
        pixels[5 * 20 + 10] = new Color32(255, 255, 255, 255);
        pixels[5 * 20 + 15] = new Color32(255, 255, 255, 255);
        pixels[5 * 20 + 3] = new Color32(255, 255, 255, 255);
        var rects = MementoIllustratedAtlasLayout.BuildRects(pixels, 20, 20, 2, 1);
        Assert.That(rects[0].xMax, Is.EqualTo(9));
        Assert.That(rects[1].xMin, Is.EqualTo(9));
    }

    [Test]
    public void RejectsAtlasWithoutGutters()
    {
        var pixels = new Color32[100];
        for (int i = 0; i < pixels.Length; i++) pixels[i].a = 255;
        Assert.Throws<InvalidOperationException>(() =>
            MementoIllustratedAtlasLayout.BuildRects(pixels, 10, 10, 2, 2));
    }

    [Test]
    public void RejectsEmptyCell()
    {
        Assert.Throws<InvalidOperationException>(() =>
            MementoIllustratedAtlasLayout.BuildRects(new Color32[100], 10, 10, 2, 2));
    }

    [Test]
    public void StaggeredRows_CutsEachColumnWithoutLosingPaint()
    {
        var pixels = new Color32[400];
        for (int y = 2; y <= 12; y++) pixels[y * 20 + 3].a = 255;
        for (int y = 15; y <= 18; y++) pixels[y * 20 + 3].a = 255;
        for (int y = 2; y <= 6; y++) pixels[y * 20 + 16].a = 255;
        for (int y = 9; y <= 18; y++) pixels[y * 20 + 16].a = 255;
        // Left gap13 exceeds allowed2px radius at20/2; use gap12 instead.
        pixels[12 * 20 + 3].a = 0;
        var rects = MementoIllustratedAtlasLayout.BuildRects(pixels, 20, 20, 2, 2);
        Assert.That(rects[0].yMin, Is.EqualTo(12));
        Assert.That(rects[1].yMin, Is.EqualTo(8));
        int found = 0;
        foreach (var rect in rects)
            for (int y = rect.yMin; y < rect.yMax; y++)
            for (int x = rect.xMin; x < rect.xMax; x++)
                if (pixels[y * 20 + x].a != 0) found++;
        Assert.That(found, Is.EqualTo(System.Array.FindAll(pixels, p => p.a != 0).Length));
    }

    [Test]
    public void RejectsInvalidDimensions()
    {
        Assert.Throws<ArgumentException>(() =>
            MementoIllustratedAtlasLayout.BuildRects(new Color32[10], 10, 10, 2, 2));
    }
}
