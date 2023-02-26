using UnityEngine;

public static class V2ExtensionMethods
{
    public static Vector2 Convert(this Vector3 v, Vector3 xDir, Vector3 yDir) => new Vector2(Vector3.Dot(v, xDir), Vector3.Dot(v, yDir));
    public static Vector2 Rotate(this Vector2 v, float angle) => Quaternion.Euler(0, 0, angle) * v;
    public static Vector2 RotateToward(this Vector2 current, Vector2 target, float maxDelta)
    {
        var targetAngle = Vector2.SignedAngle(current, target);
        var angle = Mathf.MoveTowards(0, targetAngle, maxDelta);
        var result = current.Rotate(angle);
        return result;
    }
}
