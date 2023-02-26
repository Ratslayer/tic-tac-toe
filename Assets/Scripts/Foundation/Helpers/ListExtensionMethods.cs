using System.Collections.Generic;
namespace BB
{
	public static class ListExtensionMethods
	{
		public static bool TryRemoveAt<T>(this List<T> list, int index, out T element)
		{
			var result = list.IsValidIndex(index);
			if (result)
			{
				element = list[index];
				list.RemoveAt(index);
			}
			else element = default;
			return result;
		}
		public static bool TryRemoveLast<T>(this List<T> list, out T element)
			=> list.TryRemoveAt(list.Count - 1, out element);
		public static IEnumerable<T> InReverse<T>(this List<T> list)
		{
			for (int i = list.Count - 1; i >= 0; i--)
				yield return list[i];
		}
		public static T BoundedElementAt<T>(this List<T> list, int i)
			=> i >= list.Count ? list[^1] : i <= 0 ? list[0] : list[i];
	}
}