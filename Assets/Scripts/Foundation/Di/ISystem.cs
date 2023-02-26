using System;
using System.Collections.Generic;
using UnityEngine;
namespace BB
{
	public interface ISystem
	{
		IEntity Entity { get; }
		string Name => Entity.Name;
	}
	//public static class SystemExtensions
	//{
	//	private static readonly List<Type> _types = new List<Type>();
	//	public static void System<TSystem>(this DiContainer container)
	//		where TSystem : AbstractSystem
	//	{
	//		_types.Clear();
	//		var type = typeof(TSystem);
	//		_types.Add(type);
	//		foreach (var t in type.GetInterfaces())
	//		{
	//			if (t == typeof(IDisposable))
	//				_types.Add(typeof(IDisposable));
	//			if (t == typeof(IInitializable))
	//				_types.Add(typeof(IInitializable));
	//		}
	//		container.Bind(_types).To<TSystem>().AsSingle().NonLazy();
	//	}
	//	public static IfNotBoundBinder Data<TContract, TInstance>(this DiContainer container)
	//		where TInstance : TContract
	//		=> container.Bind<TContract>().To<TInstance>().AsSingle();
	//	public static IfNotBoundBinder Data<TContract>(this DiContainer container)
	//		=> container.Bind<TContract>().AsSingle();
	//	public static IfNotBoundBinder Data<TContract>(this DiContainer container, TContract instance)
	//		=> container.Bind<TContract>().FromInstance(instance).AsSingle();
	//	public static ConcreteIdArgConditionCopyNonLazyBinder Component<TContract>(this DiContainer container, GameObject instance)
	//		=> container.Bind<TContract>().FromInterfaceComponentOn(instance).AsSingle();
	//	public static void Config<TData>(this DiContainer container, TData instance)
	//		=> container.BindInterfacesTo<TData>().FromInstance(instance).AsSingle();
	//}
}
