using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;
namespace BB
{
	public static class ReflectionUtils
	{
		public const BindingFlags PrivateFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
		public const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
		public static bool HasAttribute<T>(this Type type) => Attribute.GetCustomAttribute(type, typeof(T)) != null;
		public static FieldInfo[] GetAllFields(object target)
		{
			return GetAllFields(target.GetType());
		}
		private static readonly List<FieldInfo> _readonlyInfos = new();
		public static List<FieldInfo> GetFieldsIncludingReadonly(object target, Type cutoffType)
		{
			_readonlyInfos.Clear();
			var type = target.GetType();
			while (type != null)
			{
				_readonlyInfos.AddRange(type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly));
				if (type == cutoffType)
					break;
				type = type.BaseType;
			}
			return _readonlyInfos;
		}
		private static IEnumerable<FieldInfo> SelectFieldsThatInherit<T>(IEnumerable<FieldInfo> infos)
		{
			var result = from info in infos
						 where typeof(T).IsAssignableFrom(info.FieldType)
						 select info;
			return result;
		}
		private static IEnumerable<T> SelectFieldValues<T>(IEnumerable<FieldInfo> infos, object target) where T : class
		{
			var result = from info in infos
						 select info.GetValue(target) as T;
			return result;
		}
		public static MethodInfo GetStaticMethodInBaseTypes(Type type, string methodName)
		{
			MethodInfo result;
			var currentType = type;
			do
			{
				result = currentType.GetMethod(methodName, StaticFlags);
				if (result != null)
					break;
				currentType = currentType.BaseType;
			}
			while (currentType != null);
			return result;
		}
		public static object InvokeStaticMethod(Type type, string methodName, params object[] parameters)
		{
			object result = null;
			if (type != null)
			{
				var method = type.GetMethod(methodName, StaticFlags);
				if (method != null)
					result = method.Invoke(null, parameters);
			}
			return result;
		}
		public static object InvokeStaticMethod(string typeName, string methodName, params object[] parameters)
			=> InvokeStaticMethod(Type.GetType(typeName), methodName, parameters);
		private static FieldInfo[] GetAllFields(Type type)
		{
			return type.GetFields(PrivateFlags);
		}
		public static FieldInfo[] GetAllPublicFields(object target)
		{
			return target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
		}
		public static IEnumerable<T> GetFieldValuesIncludingReadonlyThatInherit<T>(object target, Type cutoffType) where T : class => SelectFieldValues<T>(SelectFieldsThatInherit<T>(GetFieldsIncludingReadonly(target, cutoffType)), target);
		public static IEnumerable<FieldInfo> GetFieldsThatInherit<T>(object target) where T : class => SelectFieldsThatInherit<T>(GetAllFields(target));
		public static IEnumerable<T> GetFieldValuesThatInherit<T>(object target) where T : class => SelectFieldValues<T>(GetFieldsThatInherit<T>(target), target);
		public static FieldInfo GetFieldWithValue(object target, object value)
		{
			FieldInfo result = null;
			foreach (var field in GetAllFields(target))
				if (field.GetValue(target) == value)
				{
					result = field;
					break;
				}
			return result;
		}
		public static object InvokeDefaultConstructor(Type type)
		{
			var ctor = type.GetConstructor(new Type[0]);
			var instance = ctor.Invoke(new object[0]);
			return instance;
		}
		public static List<T> Plus<T>(this List<T> list, params T[] addedElements)
		{
			list.AddRange(addedElements);
			return list;
		}
		private static List<T> AggregateCollectionValues<T>(object target, IEnumerable<FieldInfo> infos) where T : class
		{
			var result = new List<T>();
			var tType = typeof(T);
			foreach (var info in infos)
			{
				var type = info.FieldType;
				if (tType.IsAssignableFrom(type))
				{
					if (info.GetValue(target) is T t)
						result.Add(t);
				}
				else if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
				{
					var itemType = type.GetGenericArguments()[0];
					if (tType.IsAssignableFrom(itemType))
					{
						var list = info.GetValue(target) as IList;
						foreach (var item in list)
						{
							if (item != null)
								result.Add(item as T);
						}
					}
				}
			}
			return result;
		}
		public static List<T> AggregateCollectionValues<T>(object target) where T : class
		{
			var infos = GetAllFields(target);
			var result = AggregateCollectionValues<T>(target, infos);
			return result;
		}
		public static List<T> AggregateAllCollectionValues<T, CutoffT>(object target) where T : class
		{
			var infos = GetFieldsIncludingReadonly(target, typeof(CutoffT));
			var result = AggregateCollectionValues<T>(target, infos);
			return result;
		}
		public static string GetClassName(params string[] names)
		{
			var result = names.Join("+");
			return result;
		}
		public struct ValueFieldPairs
		{
			public ValueFieldPairs(string name, object value)
			{
				FieldName = name;
				Value = value;
			}
			public string FieldName;
			public object Value;
		}
		public static void SetFields(object obj, List<ValueFieldPairs> values)
		{
			var fields = GetAllFields(obj);
			var type = obj.GetType();
			foreach (var pair in values)
			{
				var field = fields.Find((f) => f.Name == pair.FieldName);
				if (Assert.Error.NotNull(field, $"Could not find '{pair.FieldName}' field in '{type.Name}' instance."))
				{
					if (!Assert.Error.True(field.FieldType.IsAssignableFrom(pair.Value.GetType()), $"Can't assign {pair.Value.GetType()} to {field.FieldType.Name}"))
						field.SetValue(obj, pair.Value);
				}
			}
		}
		public static object GetFieldValue(object target, string fieldName, BindingFlags flags = PrivateFlags)
		{
			object result;
			if (target != null)
			{
				var type = target.GetType();
				var field = type.GetField(fieldName, flags);
				result = Assert.Error.NotNull(field, $"{type.Name} has no field named '{fieldName}'") ? field.GetValue(target) : default;
			}
			else
				result = default;
			return result;
		}
		public static object GetPropertyValue(object target, string propertyName, BindingFlags flags = PrivateFlags)
		{
			var property = GetProperty(target?.GetType(), propertyName);
			var result = property?.GetValue(target);
			return result;
		}
		public static void SetPropertyValue(object target, string propertyName, object value, BindingFlags flags = PrivateFlags)
		{
			var property = GetProperty(target?.GetType(), propertyName);
			property?.SetValue(target, value);
		}
		public static object GetStaticPropertyValue(Type type, string propertyName, BindingFlags flags = PrivateFlags)
		{
			var property = GetProperty(type, propertyName);
			var result = property.GetValue(null);
			return result;

		}
		public static object GetStaticPropertyValue<T>(string propertyName, BindingFlags flags = PrivateFlags)
		{
			return GetStaticPropertyValue(typeof(T), propertyName, flags);
		}
		public static PropertyInfo GetProperty(Type type, string name, BindingFlags flags = PrivateFlags)
		{
			var result = GetInfo(type, name, type.GetProperty, flags);
			return result;
		}
		public static MethodInfo GetMethod(Type type, string methodName)
		{
			var result = type.GetMethod(methodName, PrivateFlags);
			return result;
		}
		public static MethodInfo GetMethodWithArgs(Type type, string methodName, params Type[] argTypes)
		{
			var result = type.GetMethod(methodName, PrivateFlags, null, argTypes, null);
			return result;
		}
		public static FieldInfo GetField(Type type, string name, BindingFlags flags = PrivateFlags)
		{
			var result = GetInfo(type, name, type.GetField, flags);
			return result;
		}
		private static T GetInfo<T>(Type type, string name, Func<string, BindingFlags, T> getter, BindingFlags flags) where T : MemberInfo
		{
			T result;
			if (type != null)
			{
				result = getter(name, flags);
				AssertInfoNotNull(result, type, name, typeof(T));
			}
			else
				result = null;
			return result;
		}
		private static void AssertInfoNotNull(MemberInfo info, Type type, string name, Type memberType)
		{
			if (info == null)
			{
				var infoTypeName = memberType.Name;
				Debug.LogError($"{type.Name} has no {infoTypeName} named '{name}'");
			}
		}
		public static object InvokeMethod(object target, string methodName, params object[] args)
		{
			var method = GetMethod(target.GetType(), methodName);
			return method.Invoke(target, args);
		}
		public static object InvokeStaticMethod(Type type, string methodName, bool matchArgTypes, params object[] args)
		{
			MethodInfo method;
			if (matchArgTypes)
			{
				var types = args.Convert((i, a) => a.GetType());
				method = GetMethodWithArgs(type, methodName, types);
			}
			else
				method = GetMethod(type, methodName);
			return method.Invoke(null, args);
		}
		public static object InvokeStaticMethod<T>(string methodName, params object[] args)
		{
			var type = typeof(T);
			return InvokeStaticMethod(type, methodName, args);
		}
		//taken from https://stackoverflow.com/questions/91778/how-to-remove-all-event-handlers-from-an-event/91853#91853
		public static void ClearEventInvocations(object obj, string eventName)
		{
			var t = obj.GetType();
			var fs = t.GetFields(PrivateFlags);
			var ms = t.GetMethods(PrivateFlags);
			var ps = t.GetProperties(PrivateFlags);
			var es = t.GetEvents(PrivateFlags);
			var name = "m_On" + eventName.Capitalize();
			var field = t.GetField(name, PrivateFlags);
			return;
		}
		#region Attributes
		public class GSAttributeWrapper<T> where T : AbstractGSAttribute
		{
			public T Attribute;
			public MemberInfo Info;
			public object Target;
		}
		public static IEnumerable<FieldInfo> GetAllDeclaredFields(Type t, BindingFlags flags)
		{
			return t != null ? t.GetFields(flags).Concat(GetAllDeclaredFields(t.BaseType, flags)) : Enumerable.Empty<FieldInfo>();
		}
		public static IEnumerable<PropertyInfo> GetAllDeclaredProperties(Type t, BindingFlags flags)
		{
			return t != null ? t.GetProperties(flags).Concat(GetAllDeclaredProperties(t.BaseType, flags)) : Enumerable.Empty<PropertyInfo>();
		}
		public static List<MemberInfo> GetAllFieldsAndProperties(object target)
		{
			var type = target.GetType();
			var flags = BindingFlags.Public | BindingFlags.NonPublic |
								BindingFlags.Instance | BindingFlags.DeclaredOnly;

			var result = new List<MemberInfo>();
			result.AddRange(GetAllDeclaredFields(type, flags));
			result.AddRange(GetAllDeclaredProperties(type, flags));
			return result;
		}
		public static List<GSAttributeWrapper<T>> GetAttributes<T>(object target, IEnumerable<MemberInfo> infos) where T : AbstractGSAttribute
		{
			var result = new List<GSAttributeWrapper<T>>();
			foreach (var info in infos)
			{
				var attributes = info.GetCustomAttributes(typeof(T), true);
				foreach (var attribute in attributes)
				{
					result.Add(new GSAttributeWrapper<T>()
					{
						Attribute = attribute as T,
						Info = info,
						Target = target
					});
				}
			}
			return result;
		}
		private static List<AttributeType> FetchAttributes<AttributeType, MemberType>(object target, MemberType[] members, Type attributeType)
			where AttributeType : AbstractGSAttribute
			where MemberType : MemberInfo
		{
			var result = new List<AttributeType>();
			foreach (var methodInfo in members)
			{
				var attributes = methodInfo.GetCustomAttributes(true);
				foreach (var attribute in attributes)
				{
					if (attribute is AttributeType)
					{
						AttributeType newAttribute = Activator.CreateInstance(attribute.GetType(), true) as AttributeType;
						Assert.Throw.NotNull(newAttribute, $"{attributeType.Name} has no default constructor.");
						newAttribute.Copy((AttributeType)attribute, methodInfo, target);
						result.Add(newAttribute);
					}
				}
			}
			return result;
		}
		private static List<AttributeType> FetchAttributes<AttributeType, MemberType>(object target, MemberType[] members, Type[] attributeTypes)
			where AttributeType : AbstractGSAttribute
			where MemberType : MemberInfo
		{
			PopulateIfEmpty<AttributeType>(ref attributeTypes);
			var result = new List<AttributeType>();
			foreach (var type in attributeTypes)
			{
				var attributes = FetchAttributes<AttributeType, MemberType>(target, members, type);
				result.AddRange(attributes);
			}
			return result;
		}
		private static void PopulateIfEmpty<T>(ref Type[] types)
		{
			if (types == null || types.Length == 0)
			{
				types = new Type[] { typeof(T) };
			}
		}
		public static void ForeachOfType<T>(this IEnumerable e, Action<T> action)
		{
			foreach (var element in e)
				if (element is T t)
					action(t);
		}
		public static List<T> FetchMethodAttributes<T>(object target, params Type[] types) where T : AbstractGSAttribute
		{
			var methods = target.GetType().GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var result = FetchAttributes<T, MethodInfo>(target, methods, types);
			return result;
		}
		public static List<T> FetchFieldAttributes<T>(object target, params Type[] types) where T : AbstractGSAttribute
		{
			var methods = target.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			var result = FetchAttributes<T, FieldInfo>(target, methods, types);
			return result;
		}

		public static List<T> FetchFieldAttributesThatInherit<T>(object target) where T : AbstractGSAttribute
		{
			var types = GetAllChildTypes(typeof(T));
			return FetchFieldAttributes<T>(target, types);
		}
		public static List<T> FetchMethodAttributesThatInherit<T>(object target) where T : AbstractGSAttribute
		{
			var types = GetAllChildTypes(typeof(T));
			return FetchMethodAttributes<T>(target, types);
		}

		public static T GetCustomAttribute<T>(this FieldInfo field, bool inherit = true) where T : Attribute
		{
			return field.GetCustomAttributes(typeof(T), inherit).FirstOrDefault() as T;
		}
		public static bool HasCustomAttribute<T>(this FieldInfo field, bool inherit = true) where T : Attribute => field.GetCustomAttribute<T>(inherit) != null;
		private static readonly List<FieldInfo> _fieldInfos = new();
		public static List<FieldInfo> GetAllFieldsWithAttributeNonAlloc<AttributeType>(object target, Type cutoffType)
			where AttributeType : Attribute
		{
			_fieldInfos.Clear();
			var infos = GetFieldsIncludingReadonly(target, cutoffType);
			for (int i = 0; i < infos.Count; i++)
				if (infos[i].HasCustomAttribute<AttributeType>())
					_fieldInfos.Add(infos[i]);
			return _fieldInfos;
		}
		#endregion
		public static string[] GetAllChildTypeNames<T>()
		{
			return GetAllChildTypeNames(typeof(T));
		}
		public static string[] GetAllChildTypeNames(this Type type)
		{
			var types = GetAllChildTypes(type);
			var names = types.Convert((_, t) => t.Name);
			return names;
		}
		public static Type[] GetAllChildTypes<T>()
		{
			return GetAllChildTypes(typeof(T));
		}
		public static void ForeachMemberWithAttribute<TInfo>(object target, Func<Type, IEnumerable<TInfo>> getter, Action<TInfo, Type> processor)
			where TInfo : MemberInfo
		{
			var fields = getter(target.GetType());
			foreach (var field in fields)
				foreach (var data in field.CustomAttributes)
					processor(field, data.AttributeType);
		}
		public static IEnumerable<MethodInfo> GetAllMethodsWithAttribute<TAttibute>(Type targetType, Type cutoffType)
			where TAttibute : Attribute
		{
			var attributeType = typeof(TAttibute);
			foreach (var method in GetAllMethods(targetType, cutoffType))
				foreach (var attribute in method.CustomAttributes)
					if (attribute.AttributeType.IsAssignableFrom(attributeType))
						yield return method;
		}
		public static void ForeachFieldWithAttribute(object target, Type cutoffType, Action<FieldInfo, Type> processor)
			=> ForeachMemberWithAttribute(target, type => GetAllFields(type, cutoffType), processor);
		public static void ForeachPropertyWithAttribute(object target, Type cutoffType, Action<PropertyInfo, Type> processor)
			=> ForeachMemberWithAttribute(target, type => GetAllProperties(type, cutoffType), processor);
		public delegate bool FieldAndPropertySetter(Type memberType, Type attributeType, out object value);
		private static readonly List<FieldInfo> _fields = new();
		private static readonly List<PropertyInfo> _properties = new();
		static readonly List<MethodInfo> _methods = new();
		public static IEnumerable<MethodInfo> GetAllMethods(Type type, Type cutoffType)
			=> GetAllInfos(type, cutoffType, _methods, (t, f) => t.GetMethods(f));
		public static IEnumerable<FieldInfo> GetAllFields(Type type, Type cutoffType)
			=> GetAllInfos(type, cutoffType, _fields, (t, f) => t.GetFields(f));
		public static IEnumerable<PropertyInfo> GetAllProperties(Type type, Type cutoffType)
			=> GetAllInfos(type, cutoffType, _properties, (t, f) => t.GetProperties(f));
		private static IEnumerable<TInfo> GetAllInfos<TInfo>(Type type, Type cutoffType, List<TInfo> cache, Func<Type, BindingFlags, TInfo[]> getter)
		{
			cache.Clear();
			do
			{
				cache.AddRange(getter(type, PrivateFlags));
				type = type.BaseType;
			}
			while (type != null && type != cutoffType);
			return cache;
		}
		public static void SetFieldsAndPropertiesWithAttributes(object target, Type cutoffType, FieldAndPropertySetter setter)
		{
			ForeachFieldWithAttribute(target, cutoffType, SetField);
			ForeachPropertyWithAttribute(target, cutoffType, SetProperty);
			void SetField(FieldInfo info, Type attributeType)
			{
				if (setter(info.FieldType, attributeType, out var value))
					info.SetValue(target, value);
			}
			void SetProperty(PropertyInfo info, Type attributeType)
			{
				if (setter(info.PropertyType, attributeType, out var value))
					info.SetValue(target, value);
			}
		}
		public static Type[] GetAllChildTypes(this Type tType)
		{
			var predicate = tType.IsGenericType ? (Predicate<Type>)InheritsGeneric : Inherits;
			var allTypes = from type
						 in Assembly.GetAssembly(tType).GetTypes()
						   where predicate(type)
						   select type;
			return allTypes.ToArray();
			bool Inherits(Type child)
			{
				var result = NotAbstract(child) && tType.IsAssignableFrom(child);
				return result;
			}
			bool InheritsGeneric(Type child)
			{
				var parent = child.BaseType;
				var result = NotAbstract(child) && parent != null && parent.IsGenericType && parent.GetGenericTypeDefinition() == tType;
				return result;
			}
			bool NotAbstract(Type child)
			{
				var result = !child.IsAbstract && !child.IsInterface;
				return result;
			}
		}
	}
}