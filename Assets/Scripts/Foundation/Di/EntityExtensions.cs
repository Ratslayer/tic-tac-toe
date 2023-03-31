using MessagePipe;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BB
{
	public static class EntityExtensions
	{
		public static void Append<TSystem>(this IEntity entity, params object[] args)
			where TSystem : EntitySystem
			=> entity.Resolver.Create<TSystem>(args);
		public static void Install(this IResolver resolver)
		{
			if (resolver.Installed)
				return;
			resolver.ResolveRoots();
			resolver.EndInstall();
			//install?.Invoke(resolver as IBinder);
			//if (resolver.Parent != null)
			//	resolver.Parent.InvokeAfterInstall(InstallResolver);
			//else InstallResolver();
			//void InstallResolver()
			//{
				
			//}

		}
		public static IEntity GetEntity(this GameObject obj)
		{
			if (!Log.AssertNotNull(obj, "Null gameobject can't have an entity."))
				return null;
			if (Log.Assert(obj.TryGetComponent(out EntityBehaviour eb), $"Gameobject {obj.name} has no entity."))
				return null;
			return eb;
		}
		public static void InstallBindings(this IEnumerable<IInstaller> installers, IBinder binder)
		{
			foreach (var installer in installers)
				installer.InstallBindings(binder);
		}
		public static bool TryPublish<T>(this IEntity to, T msg) => to.Resolver.TryPublish(msg);
		public static void Publish<T>(this IEntity to, T msg) => to.Resolver.Publish(msg);
		public static void ExternalSubscribe<T>(this IEntity entity, Action<T> action)
			=> entity.Resolver.Resolve<ExternalSubscriptions>().Subscribe(action);
		public static void ExternalSubscribe<T>(this IEntity entity, Action action)
			=> ExternalSubscribe<T>(entity, t => action());
		public static void EnterState(this IEntity entity, IMachine machine, IStateProvider state)
			=> entity.Resolver.Resolve<IStateController>().Enter(machine, state);
		public static string GetPath(this IResolver root)
		{
			var objects = new List<string>();
			Add(root);
			return string.Join('/', objects);
			void Add(IResolver resolver)
			{
				if (resolver.Parent != null)
					Add(resolver.Parent);
				objects.Add(resolver.Name);
			}
		}
		public static void Move(this IEntity entity, SpawnPosition pos)
		{
			if (entity.HasGameObject(out var go))
				go.Apply(pos);
		}
	}
}