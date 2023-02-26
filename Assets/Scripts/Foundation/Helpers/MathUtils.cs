using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
namespace BB
{
	public static class QuaternionExtensionMethods
	{
		public static Quaternion Inverse(this Quaternion q) => Quaternion.Inverse(q);
		public static void GetDirections(this Quaternion q, out Vector3 forward, out Vector3 right, out Vector3 up)
		{
			forward = q * Vector3.forward;
			right = q * Vector3.right;
			up = q * Vector3.up;
		}
		public static Quaternion UpRot(this Vector3 newUp)
			=> Quaternion.FromToRotation(Vector3.up, newUp);
	}
	public static class MathUtils
	{
		public static float ToSign(this bool value) => value ? 1f : -1f;
		#region Single
		public static float Remap(this float value, float oldMin, float oldMax, float newMin, float newMax)
		{
			return (value - oldMin) / (oldMax - oldMin) * (newMax - newMin) + newMin;
		}
		public static float Remap(this float value, float min, float max)
		{
			return (max - min) * value + min * Mathf.Sign(value);
		}
		public static float Min0(this float value) => Mathf.Max(value, 0);
		public static int Min0(this int value) => Mathf.Max(value, 0);
		/// <summary>
		/// This mod works with negative numbers
		/// </summary>
		public static int Mod(this int n, int m)
		{
			return (n % m + m) % m;
		}
		public static int Sign(int n)
		{
			return n > 0 ? 1 : n < 0 ? -1 : 0;
		}
		public static bool IsEven(this int n)
		{
			return Mod(n, 2) == 0;
		}
		public static bool IsOdd(this int n)
		{
			return !n.IsEven();
		}
		public static bool IsBetween(this int value, int min, int max) => min < max ? value >= min && value <= max : value >= max && value <= min;
		public static bool IsBetween(this float value, float min, float max) => min < max ? value >= min && value <= max : value >= max && value <= min;
		public static bool Approximately(this float value, float v2) => Mathf.Approximately(value, v2);
		public static bool IsZero(this float value) => value <= 0.001f && value >= -0.001f;
		//private static bool IsBetweenDynamic(dynamic value, dynamic min, dynamic max)
		//{
		//	bool result;
		//	if (min < max)
		//	{
		//		result = value >= min && value <= max;
		//	}
		//	else
		//		result = value >= max && value <= min;
		//	return result;
		//}
		public static void MinMax(int value, ref int min, ref int max)
		{
			min = Math.Min(value, min);
			max = Math.Max(value, max);
		}
		public static void MinMax(float value, ref float min, ref float max)
		{
			min = Math.Min(value, min);
			max = Math.Max(value, max);
		}
		public static float MinDistance(float v1, float v2)
		{
			var result = Mathf.Abs(v1) < Mathf.Abs(v2) ? v1 : v2;
			return result;
		}
		public static float MaxDistance(float v1, float v2)
		{
			var result = Mathf.Abs(v1) > Mathf.Abs(v2) ? v1 : v2;
			return result;
		}
		public static int Clamp(int value, int min, int max)
		{
			return Math.Max(Math.Min(value, max), min);
		}
		public static float ClampDistance(float value, float min, float max)
		{
			var result = Mathf.Clamp(Mathf.Abs(value), min, max) * Mathf.Sign(value);
			return result;
		}
		public static float Decimal(float value)
		{
			return value - (float)Math.Truncate(value);
		}
		public static float ClipBetween0And360(float angle)
		{
			float result = angle > 0 ? angle : 360 + angle;
			return result;
		}
		public static float RemoveExtraRotationsOnAngle(float angle)
		{
			float result = angle - Mathf.Floor(angle / 360f) * 360;
			return result;
		}
		public static bool ApproximatelyEquals(float a, float b, float threshold = float.Epsilon)
		{
			return Mathf.Abs(a - b) <= threshold;
		}
		public static bool IsZero(float value, float threshold = float.Epsilon)
		{
			return value < threshold && value > -threshold;
		}
		public static float Scale(this float value, float min, float max)
		{
			return (value - min) / (max - min);
		}
		public static float ScaledClamp(this float value, float min, float max)
		{
			return Mathf.Clamp(Scale(value, min, max), 0, 1);
		}
		public static int Cycle(this int n, int maxExclusive) => (n + 1).Mod(maxExclusive);
		#endregion
		#region Vector3

		public static Vector3 Replace(this Vector3 v, Vector3 replace, Axis axis)
		{
			var x = axis.HasFlag(Axis.X) ? replace.x : v.x;
			var y = axis.HasFlag(Axis.Y) ? replace.y : v.y;
			var z = axis.HasFlag(Axis.Z) ? replace.z : v.z;
			var result = new Vector3(x, y, z);
			return result;
		}
		//public static Axis GetClosestSingleAxis(Vector3 v)
		//{
		//    Axis result;
		//    var x = Mathf.Abs(v.x);
		//    var y = Mathf.Abs(v.y);
		//    var z = Mathf.Abs(v.z);
		//    if (x >= y && x >= z)
		//        result = Axis.X;
		//    else if (y >= z)
		//        result = Axis.Y;
		//    else
		//        result = Axis.Z;
		//    return result;
		//}
		public static void Split(this Vector3 v, Vector3 normalizedDir, out Vector3 along, out Vector3 remaining)
		{
			var factor = Vector3.Dot(v, normalizedDir);
			along = normalizedDir * factor;
			remaining = v - along;
		}
		#endregion
		#region Vector2
		public static Vector2 Mul(this Vector2 v1, Vector2 v2)
		{
			return new Vector2(v1.x * v2.x, v1.y * v2.y);
		}
		public static Vector2 Div(this Vector2 v1, Vector2 v2)
		{
			return new Vector2(v1.x / v2.x, v1.y / v2.y);
		}
		public static Vector2 DegreesToVector(float degrees)
		{
			float rads = degrees * Mathf.Deg2Rad;
			return new Vector2(Mathf.Cos(rads), Mathf.Sin(rads));
		}
		public static Vector2 Clamp(Vector2 value, Vector2 min, Vector2 max)
		{
			Vector2 result = Vector2.Min(max, Vector2.Max(value, min));
			return result;
		}
		public static Vector2 Clamp(Bounds bounds, Bounds externalBounds)
		{
			var actualBounds = new Bounds(externalBounds.center, externalBounds.size - bounds.size);
			return Clamp(bounds.center, actualBounds.min, actualBounds.max);
		}
		public static Vector3 Horizontal(this Vector2 v, float y = 0f) => new Vector3(v.x, y, v.y);
		public static Vector2 IntersectRays(Vector2 p1, Vector2 d1, Vector2 p2, Vector2 d2)
		{
			//the formula is (p1.y-p2.y-d1.y/d1.x*(p1.x-p1.x)) / (d2.y - d1.y/d1.x*d2.x)
			//those are temp values to simplify the expression
			Vector2 pDiff = p1 - p2;
			float tan_d1 = d1.y / d1.x;
			//calculate the result
			float s = (pDiff.y - tan_d1 * pDiff.x) / (d2.y - tan_d1 * d2.x);
			Vector2 result = p2 + d2 * s;
			return result;
		}
		public static bool AreVectorsParallel(Vector2 v1, Vector2 v2)
		{
			float tan1 = v1.y / v1.x;
			float tan2 = v2.y / v2.x;
			bool result = Mathf.Approximately(tan1, tan2);
			return result;
		}

		public static float GetNormalizedAngle(this Vector2 to)
		{
			return ClipBetween0And360(Vector2.right, to);
		}
		public static float GetNormalizedAngle(this Vector2 from, Vector2 to)
		{
			return ClipBetween0And360(from, to);
		}
		public static float SignedAngleClamp(float angle, float minAngle, float maxAngle)
		{
			var signedAngle = GetSignedAngle(angle);
			var result = Mathf.Clamp(signedAngle, minAngle, maxAngle);
			return ClipBetween0And360(result);
		}
		public static float GetSignedAngle(float value)
		{
			value -= 360 * (int)(value / 360);
			var result = value > 180 ? value - 360 : value < -180 ? value + 360 : value;
			return result;
		}
		public static float ClipBetween0And360(Vector2 from, Vector2 to)
		{
			float angle = Vector2.SignedAngle(from, to);
			return ClipBetween0And360(angle);
		}
		public static bool IsWithinAngle(Vector2 from, Vector2 to, float angle)
		{
			float diff = Vector2.Angle(from, to);
			bool result = diff < angle;
			return result;
		}
		/// <summary>
		/// Is the vector on the arc between 2 directions?
		/// </summary>
		/// <param name="value">Vector being checked</param>
		/// <param name="dir1">First arc bound</param>
		/// <param name="dir2">Second arc bound</param>
		/// <returns></returns>
		public static bool IsBetweenRadial(Vector2 value, Vector2 dir1, Vector2 dir2)
		{
			float valueAngle = Vector2.SignedAngle(dir1, value);
			float fullAngle = Vector2.SignedAngle(dir1, dir2);
			bool result = Mathf.Sign(valueAngle) == Mathf.Sign(fullAngle) && Mathf.Abs(valueAngle) < Mathf.Abs(fullAngle);
			return result;
		}
		public static bool IsBetweenRadial(Vector2 value, Vector2 pos1, Vector2 pos2, Vector2 center)
		{
			return IsBetweenRadial(value - center, pos1 - center, pos2 - center);
		}
		public static Vector2 StretchToSquare(Vector2 value)
		{
			var x = Mathf.Abs(value.x);
			var y = Mathf.Abs(value.y);
			var div = x > y ? x : y;
			var result = value / div;
			return result;
		}
		public static Vector3 GetClosestPointOnLine(Vector3 origin, Vector3 dir, Vector3 point)
		{
			dir.Normalize();
			var result = origin + dir * Vector3.Dot(point - origin, dir);
			return result;
		}
		public static Vector2 CenterOfMass(IEnumerable<Vector2> vs)
		{
			var total = Vector2.zero;
			foreach (var v in vs)
				total += v;
			var result = total / vs.Count();
			return result;
		}
		//public static Vector3 GetClosestPointOnSegment(Vector3 a, Vector3 b, Vector3 point)
		//{
		//    var dir = b - a;
		//    var p = GetClosestPointOnLine(a, b - a, point);

		//}
		#endregion
		#region Quaternion
		//taken from https://stackoverflow.com/questions/3684269/component-of-a-quaternion-rotation-around-an-axis
		public static float GetAngleComponent(Quaternion quaternion, Vector3 axis, Vector3 othogonalAxis)
		{
			Vector3 transformed = quaternion * othogonalAxis;
			//project transformed vector onto plane
			Vector3 flattened = transformed - Vector3.Dot(transformed, axis) * axis;
			flattened.Normalize();
			//get angle between original vector and projected transform to get angle around normal
			float result = Vector3.SignedAngle(othogonalAxis, flattened, axis);
			return result;
		}
		public static float GetAngleComponent2D(this Quaternion quaternion)
		{
			float result = GetAngleComponent(quaternion, Vector3.forward, Vector3.up);
			return result;
		}
		public static Quaternion Get2DRotation(this Quaternion quaternion)
		{
			float angle = GetAngleComponent(quaternion, Vector3.forward, Vector3.up);
			Quaternion result = Quaternion.Euler(0, 0, angle);
			return result;
		}
		public static Vector3 ShortestEuler(this Quaternion quaternion)
		{
			var euler = quaternion.eulerAngles;
			var result = new Vector3(ShortestAngle(euler.x), ShortestAngle(euler.y), ShortestAngle(euler.z));
			return result;
			float ShortestAngle(float value)
			{
				return Mathf.DeltaAngle(0, value);
			}
		}
		public static Quaternion MirrorX(this Quaternion q)
		{
			var result = new Quaternion(q.x, -q.y, -q.z, q.w);
			return result;
		}
		public static Quaternion MirrorY(this Quaternion q)
		{
			var result = new Quaternion(-q.x, q.y, -q.z, q.w);
			return result;
		}
		public static Quaternion MirrorZ(this Quaternion q)
		{
			return new Quaternion(-q.x, -q.y, q.z, q.w);
		}
		#endregion

		#region Random
		private static readonly System.Random SystemRandom = new System.Random(DateTime.Now.Millisecond);
		public static float Random(float min, float max)
		{
			return UnityEngine.Random.Range(min, max);
		}
		public static int RandomInt(int min, int max)
		{
			int randomMax = max - min + 1;
			return SystemRandom.Next(randomMax) + min;
		}
		public static int RandomIntWithExclusion(int min, int max, int exclude)
		{
			Assert.Throw.False(exclude > max || exclude < min, "Exclude value ({0}) must be between min ({1}) and max ({2}).");
			var range = max - min;
			var randomRange = RandomInt(1, range);
			var normalizedExclude = exclude - min;
			var normalizedValue = Mod(randomRange + normalizedExclude, range + 1);
			var result = min + normalizedValue;
			return result;
		}
		#endregion
	}
	public enum Axis
	{
		X = 0,
		Y = 1,
		Z = 2
	}
	public static class AxisUtils
	{
		public static Vector3 GetAxis(this Transform t, Axis axis) => axis switch
		{
			Axis.X => t.right,
			Axis.Y => t.up,
			_ => t.forward
		};
	}
}