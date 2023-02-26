using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BB
{
	public interface IBinder : IResolver
	{
		void Bind(Type contactType, Type instanceType);
		void BindLazy(Type contactType, Type instanceType);
		void BindInstance(Type contactType, object instance);
		void BindAllInterfaces(object instance);
		void QueueForInject(object instance);
		void Event<T>();
		void DontDestroyOnLoad(GameObject obj);
	}
	public static class BinderExtensions
	{
		public static void BindInstance<T>(this IBinder binder, T instance)
			=> binder.BindInstance(typeof(T), instance);
		public static void BindAndInjectInstance<T>(this IBinder binder, T instance)
		{
			binder.BindInstance(instance);
			binder.QueueForInject(instance);
		}
		static void AssertIsGoBinder(IBinder binder, out GameObject instance)
		{
			if (binder is not IGameObject go)
				throw new Exception($"{binder.Name} is not a Game Object binder and can't add/get components.");
			instance = go.Instance;
		}
		public static void Component<T>(this IBinder binder)
		{
			AssertIsGoBinder(binder, out var instance);
			var component = instance.GetComponent<T>();
			if (component == null)
				Debug.LogError($"{instance.name} has no {typeof(T).Name} component.");
			binder.BindInstance(component);
		}
		public static void ComponentInChildren<T>(this IBinder binder)
		{
			AssertIsGoBinder(binder, out var instance);
			var component = instance.GetComponentInChildren<T>();
			if(component==null)
				Debug.LogError($"{instance.name} has no child {typeof(T).Name} component.");
			binder.BindInstance(component);
		}
		public static void AddComponent<TContract, TComponent>(this IBinder binder)
			where TComponent : Component, TContract
		{
			AssertIsGoBinder(binder, out var instance);
			binder.BindAndInjectInstance<TContract>(instance.GetOrCreateComponent<TComponent>());
		}
		public static void AddComponent<TComponent>(this IBinder binder)
			where TComponent : Component
			=> binder.AddComponent<TComponent, TComponent>();
		public static void Bind<TContract, TInstance>(this IBinder binder)
			where TInstance : TContract
			=> binder.Bind(typeof(TContract), typeof(TInstance));
		public static void Bind<TContract>(this IBinder binder)
			=> binder.Bind(typeof(TContract), typeof(TContract));
		public static void BindLazy<TContract, TInstance>(this IBinder binder)
			=> binder.BindLazy(typeof(TContract), typeof(TInstance));
		public static void BindLazy<TContract>(this IBinder binder)
			=> binder.BindLazy<TContract, TContract>();

		public static void System<TContract, TInstance>(this IBinder installer)
			where TInstance : TContract
			=> installer.Bind<TContract, TInstance>();
		public static void System<T>(this IBinder installer)
			=> installer.Bind<T, T>();
		public static void Stack<T>(this IBinder binder)
			where T : AbstractSystemStack
			=> binder.Bind<T, T>();
		public static void StateMachine(this IBinder installer, IStateProvider defaultState)
		{
			installer.System<StateContainer>();
			installer.Data(defaultState);
			installer.System<IStateController, OverrideController>();
		}

	}
}