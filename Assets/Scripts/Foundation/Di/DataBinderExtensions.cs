using UnityEngine;

namespace BB
{
	public abstract class AbstractData<T>
	{
		T _value;
		public T Value => _value;
		public void Set(T value) => _value = value;
	}
	public interface IConstData { }
	public abstract record ConstData<T>(T Value) : IConstData;
	public abstract record ChildComponent<T> : EntitySystem, IOnStart
	{
		public T Value { get; private set; }
		public void OnStart()
		{
			if (!Entity.HasGameObject(out var go))
			{
				Debug.LogError($"Child Component systems can only be found on game object entities.");
				return;
			}
			Value = go.GetComponentInChildren<T>();
			if (Value == null)
				Debug.LogError($"{Entity.Name} does not have a child component of type {typeof(T).Name}");
		}
	}
	public interface IVarData { }
	public abstract class VarData<T> : IVarData
	{
		public T Value { get; set; }
	}
	public static class DataBinderExtensions
	{
		public static void Const<T>(this IBinder binder, T value)
			where T : IConstData
			=> binder.BindInstance(value);
		public static void Var<T>(this IBinder binder)
			where T : IVarData, new()
			=> binder.BindInstance<T>(new());
		public static void Data<T>(this IBinder installer)
			where T : new()
			=> installer.BindInstance(new T());
		public static void EventData<T>(this IBinder installer)
		{
			installer.Event<T>();
			installer.Bind<T, T>();
		}
		public static void Data<T>(this IBinder installer, T value)
			=> installer.BindInstance(value);
		private static void EventIfData<TData>(this IBinder binder)
		{
			if (typeof(IEventData).IsAssignableFrom(typeof(TData)))
				binder.Event<TData>();
		}
		public static void Over<TData>(this IBinder binder)
			where TData : IOverridableData
		{
			binder.EventIfData<TData>();
			binder.Bind<TData, TData>();
		}
		public static void Over<TData, TValue>(this IBinder binder, TValue value)
			where TData : OverridableData<TValue>, new()
		{
			binder.EventIfData<TData>();
			var data = new TData();
			data.Init(value);
			binder.BindAndInjectInstance(data);
		}
	}
}