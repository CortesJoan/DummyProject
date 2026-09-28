using NUnit.Framework;
using UnityEngine;

public sealed class DuelAttackPathTests
{
    [Test]
    public void EvaluateQuadratic_ReturnsExactEndpoints()
    {
        Vector2 start = new Vector2(10f, 20f);
        Vector2 control = new Vector2(50f, 90f);
        Vector2 target = new Vector2(120f, 30f);

        Assert.That(DuelAttackPath.EvaluateQuadratic(start, control, target, 0f), Is.EqualTo(start));
        Assert.That(DuelAttackPath.EvaluateQuadratic(start, control, target, 1f), Is.EqualTo(target));
    }

    [Test]
    public void EvaluateQuadratic_ClampsTime()
    {
        Vector2 start = new Vector2(-4f, 7f);
        Vector2 control = Vector2.up;
        Vector2 target = new Vector2(8f, 3f);

        Assert.That(DuelAttackPath.EvaluateQuadratic(start, control, target, -2f), Is.EqualTo(start));
        Assert.That(DuelAttackPath.EvaluateQuadratic(start, control, target, 3f), Is.EqualTo(target));
    }

    [Test]
    public void BuildControlPoint_AddsVisibleArc()
    {
        Vector2 start = Vector2.zero;
        Vector2 target = new Vector2(100f, 0f);

        Vector2 control = DuelAttackPath.BuildControlPoint(start, target, 0f, 40f);

        Assert.That(control.x, Is.EqualTo(50f).Within(0.001f));
        Assert.That(control.y, Is.EqualTo(40f).Within(0.001f));
    }
}
