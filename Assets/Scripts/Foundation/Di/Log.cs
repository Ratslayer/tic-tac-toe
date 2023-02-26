using UnityEngine;

namespace BB
{
	public static class Log
	{
		public static void Error(string message) => Debug.LogError(message);
		public static void Warning(string message) => Debug.LogWarning(message);
		public static void Deb(string message) => Debug.Log(message);
		public static bool Assert(bool condition, string msg)
		{
			if (!condition)
				Error(msg);
			return condition;
		}
		public static bool AssertNotNull<T>(T obj, string msg)
			where T : class
			=> Assert(obj != null, msg);
	}
}