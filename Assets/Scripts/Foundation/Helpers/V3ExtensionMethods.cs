using UnityEngine;

public static class V3ExtensionMethods
{
    public static Vector3 Div(this Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x / v2.x, v1.y / v2.y, v1.z / v2.z);
    }
    public static Vector3 Mul(this Vector3 v1, Vector3 v2)
    {
        return new Vector3(v1.x * v2.x, v1.y * v2.y, v1.z * v2.z);
    }
    public static Vector3 Inv(this Vector3 v)
    {
        return new Vector3(1 / v.x, 1 / v.y, 1 / v.z);
    }
    public static Vector3 Clamp(this Vector3 v, Vector3 v1, Vector3 v2)
    {
        //return Vector3.Min(Vector3.Max(v, min), max);
        var result = new Vector3(ClampF(v.x, v1.x, v2.x), ClampF(v.y, v1.y, v2.y), ClampF(v.z, v1.z, v2.z));
        return result;
        float ClampF(float f1, float f2, float f3)
        {
            var clamped = f2 > f3 ? Mathf.Clamp(f1, f3, f2) : Mathf.Clamp(f1, f2, f3);
            return clamped;
        }
    }
    public static bool Approximately(this Vector3 v1, Vector3 v2) => Mathf.Approximately((v1 - v2).sqrMagnitude, 0);
    public static float AbsAngle(this Vector3 v1, Vector3 v2) => Mathf.Abs(v1.SignedAngle(v2));
    public static float SignedAngle(this Vector3 v1, Vector3 v2) => Vector3.SignedAngle(v1, v2, Vector3.up);
    public static float AngleClockwise(this Vector3 v1, Vector3 v2)
    {
        var signedAngle = v1.SignedAngle(v2);
        var result = signedAngle > 0 ? signedAngle : 360 + signedAngle;
        return result;
    }
    public static Vector3 Saturate(this Vector3 v)
    {
        return v.Clamp(Vector3.zero, Vector3.one);
    }
    public static Vector3 ProjectOnLine(this Vector3 v, Vector3 lineDir, Vector3 origin)
    {
        var dir = v - origin;
        var proj = Vector3.Project(dir, lineDir);
        var result = proj + origin;
        return result;
    }
}
