using System;
using UnityEngine;
using Zenject;

namespace BB
{
	public static class DiExtensionMethods
	{
		public static IfNotBoundBinder BindNewComponent<TContract, TComponent>(this DiContainer container, GameObject instance)
			where TComponent : TContract
			=> container.Bind<TContract>().To<TComponent>().FromNewComponentOn(instance).AsSingle().NonLazy();
		public static IfNotBoundBinder System<TContract, TInstance>(this DiContainer container)
			where TInstance : TContract
			=> container.Bind<TContract>().To<TInstance>().AsSingle().NonLazy();
		public static IfNotBoundBinder Bind<TContract, TInstance>(this DiContainer container, TInstance instance)
			where TInstance : TContract
			=> container.Bind<TContract>().To<TInstance>().FromInstance(instance).AsSingle();
		public static IfNotBoundBinder Bind<TContract, TInstance>(this DiContainer container)
			where TInstance : TContract
			=> container.Bind<TContract>().To<TInstance>().AsSingle();
		public static T Create<T>(this DiContainer container)
			where T : new()
		{
			var result = new T();
			container.QueueForInject(result);
			return result;
		}
		public static IfNotBoundBinder BindNewComponent<TComponent>(this DiContainer container, GameObject instance)
			=> container.Bind<TComponent>().FromNewComponentOn(instance).AsSingle().NonLazy();
		public static IfNotBoundBinder BindNewComponent<TContract, TComponent>(this DiContainer container, GameObject instance, Action<TComponent> initializer)
			where TComponent : TContract
			=> container.Bind<TContract>().To<TComponent>().FromNewComponentOn(instance).AsSingle().OnInstantiated<TComponent>((inj, c) => initializer(c)).NonLazy();
		public static ConcreteIdArgConditionCopyNonLazyBinder Component<TContract, TComponent>(this DiContainer container, GameObject instance)
			where TComponent : TContract
			=> container.Bind<TContract>().To<TComponent>().FromComponentOn(instance).AsSingle();
		public static void AddComponent<TComponent>(this DiContainer container, GameObject instance)
			where TComponent : Component
			=> container.QueueForInject(instance.AddComponent<TComponent>());
		public static void BindSingleton<TContact, TComponent>(this DiContainer container)
			where TComponent : Component, TContact
			=> container.Bind<TContact>().To<TComponent>().FromNewComponentOnNewGameObject().AsSingle().NonLazy();
	}
}