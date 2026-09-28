using UnityEngine;

public static class DuelAttackPath
{
    public static Vector2 EvaluateQuadratic(
        Vector2 start,
        Vector2 control,
        Vector2 target,
        float normalizedTime)
    {
        float t = Mathf.Clamp01(normalizedTime);
        float inverse = 1f - t;
        return (inverse * inverse * start) +
               (2f * inverse * t * control) +
               (t * t * target);
    }

    public static Vector2 BuildControlPoint(
        Vector2 start,
        Vector2 target,
        float lateralOffset,
        float lift)
    {
        Vector2 direction = target - start;
        Vector2 normal = direction.sqrMagnitude > 0.001f
            ? new Vector2(-direction.y, direction.x).normalized
            : Vector2.right;
        return Vector2.Lerp(start, target, 0.5f) +
               (normal * lateralOffset) +
               (Vector2.up * lift);
    }
}
