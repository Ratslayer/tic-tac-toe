using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.ExceptionServices;
using UnityEngine;
namespace BB
{
	public abstract class AbstractGSAttribute : Attribute
	{
		protected object Target;
		public virtual void Copy(AbstractGSAttribute attribute, MemberInfo info, object target)
		{
			Target = target;
		}
	}
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
	public abstract class AbstractClassAttribute : AbstractGSAttribute
	{

	}
	[AttributeUsage(AttributeTargets.Method, Inherited = true)]
	public abstract class AbstractMethodAttribute : AbstractGSAttribute
	{
		public MethodInfo Method;
		public override void Copy(AbstractGSAttribute attribute, MemberInfo info, object target)
		{
			base.Copy(attribute, info, target);
			Method = (MethodInfo)info;
			Copy((AbstractMethodAttribute)attribute);
		}
		public abstract void Copy(AbstractMethodAttribute attribute);
		protected MethodType GetMethod<MethodType>(MethodInfo methodInfo, object target) where MethodType : class
		{
			MethodType result = default;
			try
			{
				result = Delegate.CreateDelegate(typeof(MethodType), target, methodInfo) as MethodType;
			}
			catch (ArgumentException e)
			{
				Assert.Throw.Log("'{0}' method can't be a {1}: {2}", methodInfo.Name, GetType().Name, e.Message);
			}
			return result;
		}
	}
	[AttributeUsage(AttributeTargets.Field, Inherited = true)]
	public abstract class AbstractFieldAttribute : AbstractGSAttribute
	{
		public FieldInfo Field;
		public override void Copy(AbstractGSAttribute attribute, MemberInfo info, object target)
		{
			base.Copy(attribute, info, target);
			Field = (FieldInfo)info;
			Copy((AbstractFieldAttribute)attribute);
		}
		public abstract void Copy(AbstractFieldAttribute attribute);
		protected object GetValue()
		{
			return Field.GetValue(Target);
		}
		protected FieldType GetValue<FieldType>(FieldInfo info, object target) where FieldType : class
		{
			var result = info.GetValue(target);
			return result as FieldType;
		}
	}
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, Inherited = true)]
	public abstract class AbstractFieldOrPropertyAttribute : AbstractGSAttribute
	{
		public object GetValue(MemberInfo info, object target)
		{
			object result = default;
			if (info is PropertyInfo property)
				result = property.GetValue(target);
			else if (info is FieldInfo field)
				result = field.GetValue(target);
			return result;
		}
		public void SetValue(MemberInfo info, object target, object value)
		{
			if (info is PropertyInfo property)
				property.SetValue(target, value);
			else if (info is FieldInfo field)
				field.SetValue(target, value);
		}
		public Type GetMemberType(MemberInfo info)
		{
			Type result = default;
			if (info is PropertyInfo property)
				result = property.PropertyType;
			else if (info is FieldInfo field)
				result = field.FieldType;
			return result;

		}
	}
	public static class CollectionUtils
	{
		public static void SafeSetAt<T>(this List<T> list, int index, T element)
		{
			while (index >= list.Count)
				list.Add(default);
			list[index] = element;
		}
		public static T SafeGetAt<T>(this List<T> list, int index) => list.Count > index ? list[index] : default;
		public static void Truncate<T>(this List<T> list, int numElements)
		{
			while (list.Count > numElements)
				list.RemoveLast();
		}
		public static bool TryGetFirstOfType<ParentType, ChildType>(this IEnumerable<ParentType> collection, out ChildType element)
			where ParentType : class
			where ChildType : class
		{
			element = collection.FirstOrDefaultOfType<ParentType, ChildType>();
			return element != null;
		}
		public static ChildType FirstOrDefaultOfType<ParentType, ChildType>(this IEnumerable<ParentType> collection)
			where ParentType : class
			where ChildType : class
		  => collection.FirstOrDefault(p => p is ChildType) as ChildType;
		public static List<ResultType> Aggregate<CollectionType, ResultType>(this IEnumerable<CollectionType> collection, Func<CollectionType, IEnumerable<ResultType>> getter)
		{
			var result = new List<ResultType>();
			foreach (var element in collection)
			{
				var targets = getter(element);
				if (targets != null)
					result.AddRange(targets);
			}
			return result;
		}
		public static List<ResultType> AggregateUniques<CollectionType, ResultType>(this IEnumerable<CollectionType> collection, Func<CollectionType, IEnumerable<ResultType>> getter)
			where ResultType : class
		{
			var result = new List<ResultType>();
			foreach (var element in collection)
			{
				var targets = getter(element);
				if (targets != null)
					result.AddUniqueNonNullRange(targets);
			}
			return result;
		}
		public static IEnumerable<string> ToStringCollection<T>(this IEnumerable<T> ts, Func<T, string> stringGetter)
		{
			foreach (var t in ts)
				yield return stringGetter(t);
		}
		public static string Join<T>(this IEnumerable<T> ts, string separator, Func<T, string> stringGetter) => string.Join(separator, ts.ToStringCollection(stringGetter));
		public static T RemoveLast<T>(this List<T> list)
		{
			var result = list.Last();
			list.RemoveAt(list.Count - 1);
			return result;
		}
		public static void RemoveRange<T>(this List<T> list, IEnumerable<T> removed)
		{
			foreach (var r in removed)
				list.Remove(r);
		}
		public static void RemoveAll<T>(this List<T> list, Predicate<T> predicate)
		{
			var deadElements = list.FindAll(predicate);
			list.RemoveRange(deadElements);
		}
		public static T Remove<T>(this List<T> list, Predicate<T> predicate)
		{
			for (int i = 0; i < list.Count; i++)
			{
				var value = list[i];
				if (predicate(value))
				{
					list.RemoveAt(i);
					return value;
				}
			}
			return default;
		}
		public static void RemoveAllOfType<T>(this IList list)
		{
			for (int i = list.Count - 1; i >= 0; i--)
				if (list[i] is T)
					list.RemoveAt(i);
		}
		public static T RemoveFirstOfType<T>(this List<T> list)
		{
			for (int i = 0; i < list.Count; i++)
				if (list[i] is T t)
				{
					list.RemoveAt(i);
					return t;
				}
			return default;
		}
		public static void Shuffle<T>(this List<T> list)
		{
			for (var i = list.Count; i > 0; i--)
				list.Swap(0, Rand.GetInt(0, i));
		}

		public static void Swap<T>(this List<T> list, int i, int j)
		{
			var temp = list[i];
			list[i] = list[j];
			list[j] = temp;
		}
		public static void AddOrSet<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key, TValue value)
		{
			if (dictionary.ContainsKey(key))
				dictionary[key] = value;
			else
				dictionary.Add(key, value);
		}
		public static TValue GetOrAddNew<TKey, TValue>(this Dictionary<TKey, TValue> dictionary, TKey key) where TValue : new()
		{
			if (!dictionary.TryGetValue(key, out var value))
			{
				value = new();
				dictionary.Add(key, value);
			}
			return value;
		}
		public static TValue GetOrAddNew<TKey, TValue, TValueImpl>(this Dictionary<TKey, TValue> dictionary, TKey key)
			where TValueImpl : TValue, new()
		{
			if (!dictionary.TryGetValue(key, out var value))
			{
				value = new TValueImpl();
				dictionary.Add(key, value);
			}
			return value;
		}
		public static void AddUnique<T>(this List<T> list, T value) where T : class
		{
			if (!list.Contains(value))
				list.Add(value);
		}
		public static void AddUniqueNonNull<T>(this List<T> list, T value) where T : class
		{
			if (value != null && !list.Contains(value))
				list.Add(value);
		}
		public static void AddUniqueNonNullRange<T>(this List<T> list, IEnumerable<T> values) where T : class
		{
			if (values != null)
				foreach (var value in values)
					AddUniqueNonNull(list, value);
		}
	}
	public static class CSHelper
	{
		#region Invoke
		#region Actions
		public static void InvokeAll(this Action allActions)
		{
			if (allActions != null)
			{
				foreach (Action a in allActions.GetInvocationList())
				{
					a();
				}
			}
		}
		public static void SafeInvoke(this Action action)
		{
			if (action != null)
			{
				action.Invoke();
			}
		}
		public static void SafeInvoke<T>(this Action<T> action, T param)
		{
			if (action != null)
			{
				action.Invoke(param);
			}
		}
		public static void Invoke(params Action[] actions)
		{
			foreach (Action action in actions)
			{
				if (action != null)
				{
					InvokeAll(action);
				}
			}
		}

		public static void LayeredInvoke<T>(IEnumerable<T> objects, params Action<T>[] actions)
		{
			foreach (var action in actions)
			{
				foreach (var obj in objects)
					action.Invoke(obj);
			}
		}
		public static void Invoke<T>(T info, params Action<T>[] actions) where T : class
		{
			foreach (var action in actions)
			{
				if (action != null)
				{
					foreach (Action<T> a in action.GetInvocationList())
					{
						a(info);
					}
				}
			}
		}
		#endregion
		#region Funcs
		public static T Invoke<T>(Func<T> func)
		{
			return func != null ? func() : default;
		}
		#endregion
		public static object SafeInvoke(this MethodInfo info, object target, object[] args)
		{
			var result = default(object);
			try
			{
				result = info.Invoke(target, args);
			}
			catch (TargetInvocationException ex)
			{
				ExceptionDispatchInfo.Capture(ex.InnerException).Throw();
			}
			return result;
		}
		#endregion

		public static T SafePeek<T>(this System.Collections.Generic.Stack<T> stack)
		{
			return stack.Count > 0 ? stack.Peek() : default;
		}
		public static T[] Init<T>(this T[] array, T value)
		{
			for (int i = 0; i < array.Length; i++)
				array[i] = value;
			return array;
		}
		public static void Select<T>(this bool value, T trueSelection, T falseSelection, out T selection, out T nonSelection)
		{
			if (value)
			{
				selection = trueSelection;
				nonSelection = falseSelection;
			}
			else
			{
				selection = falseSelection;
				nonSelection = trueSelection;
			}
		}
		public static int CombineHash(params object[] objs)
		{
			var hash = 17;
			foreach (var obj in objs)
				hash = hash * 31 + obj.GetHashCode();
			return hash;
		}
		public static bool SafeEquals(object left, object right)
		{
			bool result = left is null ? right is null : left.Equals(right);
			return result;
		}

		public static void Swap<T>(ref T v1, ref T v2)
		{
			T temp = v2;
			v2 = v1;
			v1 = temp;
		}
		public static bool OptionalMatch<T>(this T? n, T v) where T : struct
		{
			return !n.HasValue || n.Value.Equals(v);
		}
		public static T GetOther<T>(T value, T option1, T option2)
		{
			return value.Equals(option1) ? option2 : option1;
		}
		public static bool AnyTrue<T>(this IEnumerable<T> values, Predicate<T> condition)
		{
			var result = false;
			if (values != null)
				foreach (var v in values)
				{
					if (condition(v))
					{
						result = true;
						break;
					}
				}
			return result;
		}
		public static T[] CreateArray<T>(params T[] array) => array;
		public static List<T> GetUniqueValues<T>(this IEnumerable<T> values) where T : class
		{
			var result = new List<T>();
			foreach (var value in values)
			{
				if (!result.Contains(value))
				{
					result.Add(value);
				}
			}
			return result;
		}
		public static List<T2> GetUniqueValues<T1, T2>(this IEnumerable<T1> values, Func<T1, T2> converter)
		{
			var result = new List<T2>();
			foreach (var v1 in values)
			{
				var v2 = converter(v1);
				if (!result.Contains(v2))
					result.Add(v2);
			}
			return result;
		}
		public static bool NoE<T>(this IEnumerable<T> e) => e == null || e.Count() == 0;
		public static void AddRangeParams<T>(this List<T> list, params T[] values) => list.AddRange(values);

		public static void AddUniqueObject<T>(this List<T> list, T value) where T : UnityEngine.Object
		{
			if (value && !list.Contains(value))
				list.Add(value);
		}

		public static bool HasEnumFlag<T>(this T value, T flag) where T : Enum
		{
			return value.HasFlag(flag);
		}
		public static bool Empty<T>(this IEnumerable<T> e) => e.Count() == 0;
		public static bool IsAny<T>(this T e) where T : Enum => e.ToString() == "Any";
		public static bool IsNone<T>(this T e) where T : Enum => e.ToString() == "None";
		public static bool Matches<T>(this T e1, T e2) where T : Enum => e1.IsAny() || e2.IsAny() || e1.Equals(e2);
		public static bool MatchesAny<T>(this T e, IEnumerable<T> es) where T : Enum => e.IsAny() || es.Count() == 0 && e.IsNone() || es.Contains(e);
		public static IEnumerable<T> GetAllEnumValues<T>() where T : Enum
		{
			return Enum.GetValues(typeof(T)).Cast<T>().ToArray();
		}
		public static List<T> FindAll<T>(this IEnumerable<T> collection, Predicate<T> predicate)
		{
			var result = new List<T>();
			foreach (var e in collection)
				if (predicate(e))
					result.Add(e);
			return result;
		}
		public static T FindOfType<T>(this IEnumerable collection) where T : class
		{
			T result = default;
			foreach (var obj in collection)
				if (obj is T t)
				{
					result = t;
					break;
				}
			return result;
		}
		public static List<T> FindAllOfType<T>(this IEnumerable collection, Predicate<T> predicate)
		{
			var result = new List<T>();
			foreach (var obj in collection)
			{
				if (obj is T t && predicate(t))
					result.Add(t);
			}
			return result;
		}
		public static List<T> FindAllOfType<T>(this IEnumerable collection) where T : class
		{
			var result = new List<T>();
			foreach (var obj in collection)
			{
				if (obj is T t)
					result.Add(t);
			}
			return result;
		}
		public static bool AreBothEqual<T1, T2>(IEnumerable<T1> e1, IEnumerable<T2> e2, Func<T1, T2, bool> comparator)
		{
			bool result = true;
			foreach (var pair in e1.Zip(e2, Tuple.Create))
			{
				if (!comparator(pair.Item1, pair.Item2))
				{
					result = false;
					break;
				}
			}
			return result;
		}
		public static Stack<T> Clone<T>(this System.Collections.Generic.Stack<T> stack)
		{
			var result = new System.Collections.Generic.Stack<T>();
			foreach (var e in stack.Reverse())
			{
				result.Push(e);
			}
			return result;
		}
		public static bool DefaultEquals<T>(T t1, T t2)
		{
			return EqualityComparer<T>.Default.Equals(t1, t2);
		}
		public static bool Contains<TFrom,TTo>(
			this IEnumerable<TFrom> values,
			out TTo value,
			Predicate<TFrom> condition,
			Func<TFrom,TTo> conversion)
		{
			if(!values.Find(out var v, out _, condition))
			{
				value = default;
				return false;
			}
			value = conversion(v);
			return true;
			
		}
		public static bool Contains<T>(this IEnumerable<T> values, T v)
		{
			var result = false;
			foreach (var value in values)
			{
				if (DefaultEquals(value, v))
				{
					result = true;
					break;
				}
			}
			return result;
		}
		public static bool Contains<T>(this IEnumerable<T> values, T v, out int index) => values.Contains(out _, out index, (v1) => DefaultEquals(v1, v));
		public static bool ContainsAny<T>(this IEnumerable<T> values, IEnumerable<T> other)
		{
			var result = false;
			foreach (var value in values)
				if (other.Contains(value))
				{
					result = true;
					break;
				}
			return result;
		}
		public static bool Contains<T>(this IEnumerable<T> values, Predicate<T> condition)
		{
			return values.TryFind(out _, condition);
		}
		public static T FindBest<T>(this IEnumerable<T> values, params Predicate<T>[] predicates)
		{
			var vs = values;
			foreach (var p in predicates)
			{
				var newVs = vs.FindAll(p);
				if (newVs.Count != 0)
				{
					vs = newVs;
				}
			}
			var result = vs.FirstOrDefault();
			return result;
		}
		public static bool Find<T>(this IEnumerable<T> values, out T value, out int index, Predicate<T> predicate)
		{
			index = 0;
			value = default;
			var result = false;
			foreach (var v in values)
			{
				if (predicate(v))
				{
					value = v;
					result = true;
					break;
				}
				index++;
			}
			if (!result)
			{
				index = -1;
			}
			return result;
		}
		public static void AddIfNotNull<T>(this List<T> list, T value) where T : class
		{
			if (value != null)
				list.Add(value);
		}
		public static bool IsValidIndex<T>(this IEnumerable<T> values, int id)
		{
			return id >= 0 && id < values.Count();
		}
		public static bool IsValidIndex<T>(this IEnumerable<T> values, int id, out T value)
		{
			var result = values.IsValidIndex(id);
			value = result ? values.ElementAt(id) : default;
			return result;
		}
		public static T MinElement<T>(this IEnumerable<T> e, Func<T, float> selector)
		{
			MinElement(e, out var result, out _, selector);
			return result;
		}
		public static void MinElement<T>(this IEnumerable<T> e, out T element, out float value, Func<T, float> selector)
		{
			if (e.Count() > 0)
			{
				element = e.FirstOrDefault();
				value = selector(element);
				foreach (var el in e)
				{
					var newValue = selector(el);
					if (newValue < value)
					{
						element = el;
						value = newValue;
					}
				}
			}
			else
			{
				element = default;
				value = Mathf.Infinity;
			}
		}
		public static T MaxElement<T>(this IEnumerable<T> e, Func<T, float> selector)
		{
			T result;
			if (e != null && e.Count() > 0)
			{
				result = e.FirstOrDefault();
				float value = selector(result);
				foreach (var el in e)
				{
					var newValue = selector(el);
					if (newValue > value)
					{
						result = el;
						value = newValue;
					}
				}
			}
			else
				result = default;
			return result;
		}
		public static void GetBoundingElements<T>(this IEnumerable<T> elements, out T min, out T max, float centerValue, Func<T, float> valueGetter)
		{
			min = max = default;
			var minBound = Mathf.NegativeInfinity;
			var maxBound = Mathf.Infinity;
			foreach (var e in elements)
			{
				var value = valueGetter(e);
				if (value <= centerValue && value > minBound)
				{
					minBound = value;
					min = e;
				}
				if (value >= centerValue && value < maxBound)
				{
					maxBound = value;
					max = e;
				}
			}
		}
		/// <summary>
		/// Compares each element with its predecessor using the comparator.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="e"></param>
		/// <param name="comparator"></param>
		/// <returns>True if all comparisons are true.</returns>
		public static bool CompareSequentially<T>(this IEnumerable<T> e, Func<T, T, bool> comparator) where T : class
		{
			var result = true;
			T prev = null, next = null;
			foreach (var el in e)
			{
				prev = next;
				next = el;
				if (prev != null && next != null && !comparator(prev, next))
				{
					result = false;
					break;
				}
			}
			return result;
		}
		public static void PopMultiple<T>(this System.Collections.Generic.Stack<T> stack, int numPops)
		{
			for (int i = 0; i < numPops; i++)
				stack.Pop();
		}
		public static string[] GetAllFiles(string folderPath)
		{
			var result = Directory.GetFiles(folderPath);
			result.ConvertSlashes();
			return result;
		}
		public static string[] GetAllFolders(string folderPath)
		{
			var result = Directory.GetDirectories(folderPath);
			result.ConvertSlashes();
			return result;
		}
		public static void ConvertSlashes(this string[] strings)
		{
			for (int i = 0; i < strings.Length; i++)
			{
				var newStr = strings[i].Replace("\\", "/");
				strings[i] = newStr;
			}
		}
		//public static T Instantiate<T>(this Type type) where T : class
		//{
		//    var result = Activator.CreateInstance(type) as T;
		//    Assert.Throw.IfNull(result, "Could not instantiate class. {0} must extend {1}", type.Name, typeof(T).Name);
		//    return result;
		//}
		//public static T Instantiate<T>(string typeName) where T : class
		//{
		//    return Type.GetType(typeName).Instantiate<T>();
		//}
		#region Arrays
		// taken from https://www.dotnetperls.com/array-slice
		public static T[] Slice<T>(this T[] source, int start, int end)
		{
			// Handles negative ends.
			if (end < 0)
			{
				end = source.Length + end;
			}
			int len = end - start;

			// Return new array.
			T[] res = new T[len];
			for (int i = 0; i < len; i++)
			{
				res[i] = source[i + start];
			}
			return res;
		}
		public static T[] Initialize<T>(this T[] array, Func<T> initializer)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = initializer();
			}
			return array;
		}
		public static T[] TrimEnd<T>(this T[] source, int numElements)
		{
			return Sub(source, 0, source.Length - 2);
		}
		public static T[] Sub<T>(this T[] source, int start, int end)
		{
			var result = new T[end - start + 1];
			for (int i = 0; i < result.Length; i++)
			{
				result[i] = source[i + start];
			}
			return result;
		}
		public static T[] Initialize<T>(this T[] array, Func<int, T> initializer)
		{
			for (int i = 0; i < array.Length; i++)
			{
				array[i] = initializer(i);
			}
			return array;
		}
		public static bool WithinBounds<T>(this T[] array, int i)
		{
			return i >= 0 && i < array.Length;
		}
		public static bool WithinBounds<T>(this List<T> list, int i)
		{
			return i >= 0 && i < list.Count;
		}
		public static int Clamp<T>(this IEnumerable<T> e, int index) => Mathf.Clamp(index, 0, e.Count() - 1);
		public static void Clamp<T>(this IEnumerable<T> e, ref int index) => index = e.Clamp(index);
		public static void Increment<T>(this IEnumerable<T> e, ref int index) => index = e.Clamp(index + 1);
		public static void Decrement<T>(this IEnumerable<T> e, ref int index) => index = e.Clamp(index - 1);
		public static T GetAtClampedId<T>(this IEnumerable<T> e, int index) => e.ElementAt(e.Clamp(index));
		public static T GetNextElement<T>(this IEnumerable<T> e, T element) where T : class
		{
			T result = null;
			T lastElement = null;
			if (element != null)
				foreach (var el in e)
				{
					if (lastElement == element)
					{
						result = el;
						break;
					}
					else
						lastElement = el;
				}
			return result;
		}
		public static T SafeGet<T>(this T[] array, int i)
		{
			return array.WithinBounds(i) ? array[i] : default;
		}
		public static void CopyFields<T>(T from, T to, params string[] fieldNames)
		{
			var type = typeof(T);
			foreach (var name in fieldNames)
			{
				var field = type.GetField(name);
				if (Assert.Error.NotNull(field, $"{type} has no field named {name}"))
				{
					field.SetValue(to, field.GetValue(from));
				}
			}
		}
		public delegate void ArrayProcessor<T>(ref T element);
		public static T[] ProcessArray<T>(T[] array, ArrayProcessor<T> processor, Predicate<T> filter, bool breakOnFirst = true)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (filter(array[i]))
				{
					processor(ref array[i]);
					if (breakOnFirst)
						break;
				}
			}
			return array;
		}
		public static List<T2> ConvertUniques<T1, T2>(this IEnumerable<T1> collection, Func<T1, T2> converter)
		{
			var result = new List<T2>();
			foreach (var e in collection)
			{
				var e2 = converter(e);
				if (!result.Contains(e2))
				{
					result.Add(e2);
				}
			}
			return result;
		}
		public static void CopyFrom<T1, T2>(this T1[] array1, T2[] array2, Func<int, T2, T1> processor)
		{
			Assert.Throw.SameLength(array1, array2, $"Arrays must be same length to copy, not {array1.Length} and {array2.Length}.");
			for (int i = 0; i < array1.Length; i++)
			{
				array1[i] = processor(i, array2[i]);
			}
		}
		public static T2[] Convert<T1, T2>(this T1[] array, Func<int, T1, T2> processor)
		{
			var result = new T2[array.Length];
			result.CopyFrom(array, processor);
			return result;
		}
		public static T[] ProcessArray<T>(T[] array, ArrayProcessor<T> processor)
		{
			return ProcessArray(array, processor, (e) => true, false);
		}
		public static string ExpandToString<T>(this T[] array, Func<T, string> toString)
		{
			var result = "";
			if (array.Length > 0)
			{
				result = toString(array[0]);
				for (int i = 1; i < array.Length; i++)
				{
					result += $",{toString(array[i])}";
				}
			}
			return result;
		}
		public static T First<T>(this T[] array)
		{
			return array.SafeGet(0);
		}
		public static T Last<T>(this T[] array)
		{
			return array.SafeGet(array.Length - 1);
		}
		public static int IndexOf<T1, T2>(this T1[] array, T2 element, Func<T1, T2, bool> equalizer)
		{
			var result = -1;
			for (int i = 0; i < array.Length; i++)
				if (equalizer(array[i], element))
				{
					result = i;
					break;
				}
			return result;
		}
		public static int IndexOf<T>(this IEnumerable<T> collection, Func<T, bool> comparator)
		{
			var result = -1;
			for (int i = 0; i < collection.Count(); i++)
			{
				if (comparator(collection.ElementAt(i)))
				{
					result = i;
					break;
				}
			}
			return result;
		}
		public static bool Contains<T>(this IEnumerable<T> e, out T element, out int index, Func<T, bool> comparator)
		{
			index = 0;
			element = default;
			var result = false;
			foreach (var el in e)
			{
				if (comparator(el))
				{
					element = el;
					result = true;
					break;
				}
				else
					index++;
			}
			return result;
		}
		public static T GetNextElementWrapped<T>(this IEnumerable<T> collection, T element)
			=> collection.Contains(element, out var index) ? collection.ElementAt(collection.GetNextIndexWrapped(index)) : collection.FirstOrDefault();
		public static int GetNextIndexWrapped<T>(this IEnumerable<T> collection, int i) => (i + 1) % collection.Count();
		public static (int min, int max) GetMinMax<T>(this IEnumerable<T> collection, Func<T, int> valueGetter)
		{
			int min, max;
			if (collection.Count() > 0)
			{
				min = max = valueGetter(collection.First());
				foreach (var t in collection)
				{
					var f = valueGetter(t);
					min = Mathf.Min(min, f);
					max = Mathf.Max(max, f);
				}
			}
			else
				min = max = 0;
			return (min, max);
		}
		public static TResult[] Cast<T, TResult>(this T[] array)
		{
			return array.Cast<TResult>().ToArray();
		}
		public static T Find<T>(this T[] array, Predicate<T> predicate)
		{
			return Array.Find(array, predicate);
		}
		public static bool Set<T>(ref T value, T newValue)
		{
			var result = !Equals(value, newValue);
			value = newValue;
			return result;
		}
		public static void Swap(ref object obj1, ref object obj2)
		{
			var temp = obj2;
			obj2 = obj1;
			obj1 = temp;
		}
		public static T[] With<T>(this T[] left, params T[] right)
		{
			var result = new List<T>();
			result.AddRange(left);
			result.AddRange(right);
			return result.ToArray();
		}
		#endregion
		#region IEnumerable
		public static void ForEach<T>(this IEnumerable<T> ie, Action<T, int> action)
		{
			var i = 0;
			foreach (var e in ie)
				action(e, i++);
		}
		public static void ForEach<T>(this IEnumerable<T> ie, Action<T> action)
		{
			foreach (var e in ie)
				action(e);
		}
		public static void ForEach<T1, T2>(IEnumerable<T1> e1, IEnumerable<T2> e2, Action<T1, T2, int> action)
		{
			Assert.Throw.SameLength(e1, e2, $"Can not simultaneously iterate through collections of different sizes ({e1.Count()} and {e2.Count()}");
			var er1 = e1.GetEnumerator();
			var er2 = e2.GetEnumerator();
			int i = 0;
			while (er1.MoveNext() && er2.MoveNext())
			{
				action(er1.Current, er2.Current, i++);
			}
		}
		public static (TLeft, TRight)[] Merge<TLeft, TRight>(this IEnumerable<TLeft> left, IEnumerable<TRight> right)
		{
			Assert.Throw.SameLength(left, right, $"Can not merge collections of different sizes ({left.Count()} and {right.Count()}");
			var result = new (TLeft, TRight)[right.Count()];
			ForEach(left, right, (l, r, i) => result[i] = (l, r));
			return result;
		}
		public static bool TryFind<T1, T2>(this IEnumerable<T1> e, out T2 value) where T2 : T1
		{
			var result = TryFind(e, out var uncastValue, v => v is T2);
			value = (T2)uncastValue;
			return result;
		}
		public static bool TryGetValue<T1, T2, T3>(this Dictionary<T1, T2> dictionary, T1 key, out T3 value) where T2 : T3
		{
			var result = dictionary.TryGetValue(key, out var v2);
			value = v2;
			return result;
		}
		public static bool TryFind<T>(this IEnumerable<T> e, out T value, Predicate<T> match)
		{
			var result = false;
			value = default;
			foreach (var v in e)
			{
				if (match(v))
				{
					value = v;
					result = true;
					break;
				}
			}
			return result;
		}
		//public static bool TryFind<T1,T2>(this IEnumerable<T1> e, out T2 value, Predicate<T1> match, Func<T1, T2> getter)
		//{
		//    var result = TryFind(e, out var temp, match);
		//    value = result ? getter(temp) : default;
		//    return result;
		//}
		public static bool TryFind<ElementType, ReturnType>(this IEnumerable<ElementType> e, out ReturnType value, Predicate<ElementType> match, Func<ElementType, ReturnType> converter)
		{
			var result = e.TryFind(out var element, match);
			value = result ? converter(element) : default;
			return result;
		}
		#endregion

		public static void Repeat(this int numTimes, Action action)
		{
			for (int i = 0; i < numTimes; i++)
				action();
		}
		public static void Repeat(this int numTimes, Action<int> action)
		{
			for (int i = 0; i < numTimes; i++)
				action(i);
		}
		public static T[] GetOneOfEachChildType<T>()
		{
			var type = typeof(T);
			var childTypes = type.GetAllChildTypes();
			var result = childTypes.Convert((_, t) => (T)Activator.CreateInstance(t));
			return result;
		}
		public static object GetDefaultValue(this Type type)
		{
			var result = type.IsValueType ? Activator.CreateInstance(type) : null;
			return result;
		}
		public static bool Intersects<T>(this IEnumerable<T> e1, IEnumerable<T> e2)
		{
			var result = false;
			foreach (var i1 in e1)
			{
				if (e2.Contains(i1))
				{
					result = true;
					break;
				}
			}
			return result;
		}
		public static bool ContainsAll<T>(this IEnumerable<T> e1, IEnumerable<T> e2)
		{
			var result = true;
			foreach (var i2 in e2)
			{
				if (!e1.Contains(i2))
				{
					result = false;
					break;
				}
			}
			return result;
		}
		#region Parsing
		public delegate bool StringParser<T>(string s, out T result);
		public static bool TryParseArray<T>(string s, char separator, StringParser<T> elementParser, out T[] result, int numElements = 0)
		{
			var hasParsed = true;
			var parts = s.Split(separator);
			result = new T[parts.Length];
			for (int i = 0; i < result.Length; i++)
				if (!elementParser(parts[i], out result[i]))
				{
					hasParsed = false;
					result = default;
					break;
				}
			return hasParsed && (numElements == 0 || numElements == result.Length);
		}
		#endregion
		public static T GetFieldOrPropertyValue<T>(object target, string name)
		{
			var type = target.GetType();
			var field = type.GetField(name);
			object result;
			if (field == null)
			{
				var property = type.GetProperty(name);
				if (property != null)
					result = property.GetValue(target);
				else
					result = null;
			}
			else
				result = field.GetValue(target);
			return (T)result;
		}
		public static int RandomInt(int min, int max)
		{
			var diff = max - min + 1;
			var value = UnityEngine.Random.value * diff;
			var result = (int)(value + min);
			return result;
		}
		public static T RandomElement<T>(this IEnumerable<T> collection)
		{
			var count = collection.Count();
			if (count <= 0)
				return default;
			var index = RandomInt(0, count - 1);
			var result = collection.ElementAt(index);
			return result;
		}
		public static T RandomEnum<T>(params T[] exclude) where T : Enum
		{
			var allValues = Enum.GetValues(typeof(T));
			var selectedValues = new List<T>();
			foreach (var value in allValues)
			{
				var e = (T)value;
				if (!exclude.Contains(e))
					selectedValues.Add(e);
			}
			var result = selectedValues.RandomElement();
			return result;
		}
	}
}