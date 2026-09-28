using AnimalMemory.UI;
using NUnit.Framework;

public sealed class MementoTutorialTypographyTests
{
    [TestCase(1920f, 1280, 720)]
    [TestCase(1920f, 800, 480)]
    [TestCase(1859f, 800, 480)]
    [TestCase(1920f, 1920, 1080)]
    public void ForLandscape_DownscaledPanel_PreservesPhysicalReadingSize(
        float panelWidth, int pixelWidth, int pixelHeight)
    {
        var metrics = MementoTutorialTypography.ForLandscape(panelWidth, pixelWidth);
        float pixelsPerUnit = pixelWidth / panelWidth;

        Assert.That(pixelWidth, Is.GreaterThan(pixelHeight));
        Assert.That(metrics.TitleSize * pixelsPerUnit, Is.GreaterThanOrEqualTo(13.99f));
        Assert.That(metrics.BodySize * pixelsPerUnit, Is.GreaterThanOrEqualTo(12.99f));
        Assert.That(metrics.HeaderHeight * pixelsPerUnit, Is.GreaterThanOrEqualTo(59.99f));
        Assert.That(metrics.HeaderHeight * pixelsPerUnit, Is.LessThanOrEqualTo(68.01f));
    }

    [TestCase(1920f, 1280)]
    [TestCase(1920f, 800)]
    [TestCase(1859f, 800)]
    [TestCase(1920f, 1920)]
    public void ForLandscape_ActionsHave44PixelTargets_AndReserveNonOverlappingMenuSpace(
        float panelWidth, int pixelWidth)
    {
        var metrics = MementoTutorialTypography.ForLandscape(panelWidth, pixelWidth);
        float pixelsPerUnit = pixelWidth / panelWidth;

        Assert.That(metrics.ActionSize * pixelsPerUnit, Is.GreaterThanOrEqualTo(43.99f));
        Assert.That(metrics.MenuWidth * pixelsPerUnit, Is.GreaterThanOrEqualTo(89.99f));
        Assert.That(metrics.MenuTextSize * pixelsPerUnit, Is.GreaterThanOrEqualTo(12.99f));
        Assert.That(metrics.ActionSize, Is.LessThan(metrics.HeaderHeight));
        Assert.That((metrics.ContentRightInset - metrics.MenuWidth) * pixelsPerUnit,
            Is.GreaterThanOrEqualTo(15.99f));
    }

    [TestCase(0f, 1280)]
    [TestCase(-10f, 1280)]
    [TestCase(float.NaN, 1280)]
    [TestCase(float.PositiveInfinity, 1280)]
    [TestCase(1920f, 0)]
    public void ForLandscape_UnresolvedGeometry_UsesFiniteBaseline(float panelWidth, int pixelWidth)
    {
        var metrics = MementoTutorialTypography.ForLandscape(panelWidth, pixelWidth);

        Assert.That(metrics.TitleSize, Is.EqualTo(22f));
        Assert.That(metrics.BodySize, Is.EqualTo(20f));
        Assert.That(metrics.HeaderHeight, Is.EqualTo(68f));
        Assert.That(metrics.ActionSize, Is.EqualTo(44f));
        Assert.That(metrics.MenuWidth, Is.EqualTo(100f));
    }

    [Test]
    public void ForLandscape_RepeatedResize_HasNoCachedOrientationOrScale()
    {
        var initial = MementoTutorialTypography.ForLandscape(1920f, 1280);
        MementoTutorialTypography.ForLandscape(1920f, 800);
        var returned = MementoTutorialTypography.ForLandscape(1920f, 1280);

        Assert.That(returned.TitleSize, Is.EqualTo(initial.TitleSize));
        Assert.That(returned.BodySize, Is.EqualTo(initial.BodySize));
        Assert.That(returned.HeaderHeight, Is.EqualTo(initial.HeaderHeight));
        Assert.That(returned.ActionSize, Is.EqualTo(initial.ActionSize));
        Assert.That(returned.MenuWidth, Is.EqualTo(initial.MenuWidth));
        Assert.That(returned.ContentRightInset, Is.EqualTo(initial.ContentRightInset));
    }
}
