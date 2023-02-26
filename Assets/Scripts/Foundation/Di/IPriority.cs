using System;
using System.Collections.Generic;
using System.Linq;

namespace BB
{
	public interface IPriority
	{
		int Priority { get; }
	}
	public static class PriorityExtensions
	{
		public static IOrderedEnumerable<T> OrderByPriority<T>(this IEnumerable<T> collection)
			=> collection.OrderBy(GetPriority);
		public static void SortByPriority<T>(this List<T> list)
			=> list.Sort((l, r) => l.GetPriority().CompareTo(r.GetPriority()));
		private static int GetPriority<T>(this T t) => t is IPriority p ? -p.Priority : 0;
	}
}