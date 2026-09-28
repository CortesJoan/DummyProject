using NUnit.Framework;
using UnityEngine;
using System.Reflection;

[TestFixture]
public class DuelOpponentTintTests
{
    [TestCase(true, 0.96f)]
    [TestCase(false, 1f)]
    public void RestingColor_PresentationOrientation_RestoresExpectedOpacity(bool portrait, float alpha)
    {
        var method = typeof(DuelOpponentFieldView).GetMethod("RestingColor",
            BindingFlags.Static | BindingFlags.NonPublic);
        Assert.That(method, Is.Not.Null);
        var colour = (Color)method.Invoke(null, new object[] { portrait });
        Assert.That(colour.a, Is.EqualTo(alpha).Within(0.001f));
        Assert.That(colour.r, Is.EqualTo(1f));
        Assert.That(colour.g, Is.EqualTo(1f));
        Assert.That(colour.b, Is.EqualTo(1f));
    }
}
