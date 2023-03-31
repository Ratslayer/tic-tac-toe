using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BB
{
	public static class GameObjectExtensions
	{
		public static GameObject CreateChild(this GameObject parent, string name)
		{
			var result = new GameObject(name);
			result.SetParent(parent);
			return result;
		}
		public static T GetOrCreateComponent<T>(this GameObject go)
			where T : Component
			=> go.TryGetComponent(out T result) ? result : go.AddComponent<T>();
	}
	public static class InstallerUtils
	{
		public static IEntity CreateEntity(
			string name,
			IResolver parent,
			Action<IBinder> install)
		{
			var binder = CreateBinder(name, parent);
			try
			{
				BindEntity(binder);
				install(binder);
				return binder.Resolve<IEntity>();
			}
			catch (Exception ex)
			{
				LogInstallException(ex, binder);
				return null;
			}
			//return CreateAndInstallEntity(binder, BindEntity, install);
		}
		public static IEntity CreateGoEntity(
			this IEntity parent,
			string name,
			Action<IBinder> install)
		{
			if (!parent.HasGameObject(out var go))
				throw new Exception($"{parent.Name} does not have a game object and can't create child objects.");
			var instance = go.CreateChild(name);
			return CreateGoEntity(instance, parent.Resolver, install, false);
		}
		public static EntityBehaviour CreateGoEntity(
			GameObject instance,
			IResolver parent,
			Action<IBinder> install,
			bool isRoot)
		{
			if (instance.TryGetComponent(out EntityBehaviour eb)
				&& eb.Resolver != null)
				return eb;
			var binder = CreateBinder(instance, parent);
			try
			{
				BindGoEntity(binder, instance, isRoot);
				foreach (var installer in instance.GetComponents<IInstaller>())
					installer.InstallBindings(binder);
				install?.Invoke(binder);
				binder.Install();
				//get entity
				var entity = instance.GetOrCreateComponent<EntityBehaviour>();
				entity.Install(binder, isRoot);
				return entity;
			}
			catch (Exception ex)
			{
				LogInstallException(ex, binder);
				return null;
			}
			//var entity = CreateAndInstallEntity(binder, Bind, Install);
			//root.Entity = entity;
			//root.InitChildren();
			//return entity;
			//void Install(IBinder binder)
			//{
			//	install?.Invoke(binder);
			//	foreach (var installer in root.GetInstallers())
			//		installer.InstallBindings(binder);
			//}
		}
		//public static void Append(this IResolver resolver, string name, params IInstaller[] installers)
		//{
		//	var container = resolver.CreateChildBinder(name);
		//	foreach (var installer in installers)
		//		installer.InstallBindings(container);
		//	resolver.InvokeAfterInstall(container.ResolveRoots);
		//}
		public static void BindGoEntity(IBinder binder, GameObject instance, bool root)
		{
			BindEntity(binder);
			binder.Const<EntityTransform>(new(instance.transform));
			//if (root)
			//	binder.Const<RootEntity>(new(entity));
			binder.System<DisableGameObjectOnDespawn>();
			binder.AddComponent<EntityEvents>();
			//update
			binder.Data<DeltaTime>();
			binder.Event<UpdateEvent>();
			binder.Event<FixedUpdateEvent>();
			binder.Event<LateUpdateEvent>();
			binder.System<PublishEntityEvents>();
			//return entity;
		}
		static void BindEntity(IBinder binder)
		{
			//var entity = new Entity(binder);
			//binder.BindInstance<IEntity>(entity);
			//binder.QueueForInject(entity);
			binder.Bind<IEntity, Entity>();
			binder.Bind<EntityState>();
			//core systems
			binder.System<ExternalSubscriptions>();
			binder.System<CancellationTokenManager>();
			//events
			binder.Event<StartEvent>();
			binder.Event<DisposeEvent>();
			binder.Event<SpawnEvent>();
			binder.Event<DespawnEvent>();
		}
		static IResolver GetRootParentIfNull(IResolver parent) => parent ?? DiServices.Root;
		static IBinder CreateBinder(GameObject instance, IResolver parent)
			=> GetRootParentIfNull(parent).CreateChildBinder(instance);
		public static IBinder CreateBinder(string name, IResolver parent)
			=> GetRootParentIfNull(parent).CreateChildBinder(name);
		public static IResolver CreateChildResolver(string name, IResolver parent, Action<IBinder> install)
		{
			var child = CreateBinder(name, parent);
			try
			{
				install?.Invoke(child);
				child.Install();
			}
			catch (Exception e)
			{
				LogInstallException(e, child);
			}
			return child;
		}
		//static IEntity CreateAndInstallEntity(
		//	IBinder binder,
		//	Func<IBinder, Action<IBinder>, IEntity> bindEntity,
		//	Action<IBinder> install)
		//{
		//	try
		//	{
		//		var entity = bindEntity(binder, install);
		//		entity.Install();
		//		return entity;
		//	}
		//	catch (Exception e)
		//	{
		//		LogInstallException(e, binder);
		//		return null;
		//	}
		//}
		public static void LogInstallException(Exception e, IResolver resolver)
		{
			Debug.LogError($"Install error at {resolver.GetPath()}");
			Debug.LogException(e);
		}
	}
	public sealed record DisableGameObjectOnDespawn : EntitySystem, IOnSpawn, IOnDespawn
	{
		public void OnSpawn()
		{
			if (Entity.HasGameObject(out var go))
				go.SetActive(true);
		}
		public void OnDespawn()
		{
			if (Entity.HasGameObject(out var go))
				go.SetActive(false);
		}
	}
	public sealed record RootEntity(IEntity Value) : ConstData<IEntity>(Value);
}