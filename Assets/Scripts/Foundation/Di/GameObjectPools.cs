using MessagePipe;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BB
{
	public interface IPools
	{
		IEntity GetOrCreateUnspawnedEntity(IEntityFactory factory);
	}
	public sealed record EntityTransform(Transform Value) : ConstData<Transform>(Value);
	public sealed record EntityParent(Transform Value) : ConstData<Transform>(Value);
	public sealed record PrefabToEntityFactoryMap : EntitySystem
	{
		readonly Dictionary<GameObject, PrefabFactory> _factories = new();
		public IEntityFactory GetOrCreate(GameObject prefab)
		{
			if (!_factories.TryGetValue(prefab, out var factory))
			{
				factory = new PrefabFactory(prefab);
				_factories.Add(prefab, factory);
			}
			return factory;
		}
		sealed record PrefabFactory(GameObject Prefab) : IEntityFactory
		{
			public string Name => Prefab.name;
			public IEntity Create(IResolver parent)
			{
				var instance = UnityEngine.Object.Instantiate(Prefab);
				var entity = InstallerUtils.CreateGoEntity(instance, parent, null, true);
				return entity;
			}
		}
	}
	public sealed record EntityPools : AbstractPools
	{
		protected override IPool CreatePool() => new Pool();
	}
	public abstract record AbstractPools : EntitySystem, IPools
	{
		readonly Dictionary<IEntityFactory, IPool> _factoryPools = new();
		protected abstract IPool CreatePool();
		protected IPool GetOrCreate<TKey>(Dictionary<TKey, IPool> pools, TKey key)
		{
			if (!pools.TryGetValue(key, out var result))
			{
				result = CreatePool();
				pools.Add(key, result);
			}
			return result;
		}
		IPool GetOrCreatePool(IEntityFactory factory)
		{
			if (factory == null)
				throw new Exception($"Can't pool a null factory.");
			return GetOrCreate(_factoryPools, factory);
		}
		public IEntity GetOrCreateUnspawnedEntity(IEntityFactory factory)
		{
			var pool = GetOrCreatePool(factory);
			if (!pool.HasUnspawnedEntity(out var entity))
			{
				entity = factory.Create(Resolver);
				pool.AddCreatedEntity(entity);
				entity.Name = $"{factory.Name} {pool.NumInstances}";
			}
			return entity;
		}
	}
	public sealed record GameObjectPools(
		EntityTransform Transform) : AbstractPools, IOnStart
	{
		//readonly Dictionary<IEntityFactory, IPool> _factoryPools = new();
		readonly Dictionary<GameObject, IPool> _prefabPools = new();
		GameObject _parent;
		public void OnStart()
		{
			_parent = new GameObject(GetType().Name);
			_parent.transform.parent = Transform.Value;
		}
		protected override IPool CreatePool() => new GoPool(_parent);
		IPool GetOrCreatePool(GameObject prefab)
		{
			if (!prefab)
				throw new Exception($"Can't pool a null prefab.");
			return GetOrCreate(_prefabPools, prefab);
		}
		public IEntity GetOrCreateUnspawnedEntity(GameObject prefab)
		{
			var pool = GetOrCreatePool(prefab);
			if (!pool.HasUnspawnedEntity(out var entity))
			{
				var instance = UnityEngine.Object.Instantiate(prefab);
				entity = InstallerUtils.CreateGoEntity(instance, Resolver, null, true);
				pool.AddCreatedEntity(entity);
				entity.Name = instance.name = $"{prefab.name} {pool.NumInstances}";
			}
			return entity;
		}


		//public IEntity GetOrCreateUnspawnedEntity(IEntityFactory factory)
		//{
		//	var pool = GetOrCreatePool(factory);
		//	if (!pool.HasUnspawnedEntity(out var entity))
		//	{
		//		entity = factory.CreateEntity(Resolver);
		//		pool.AddCreatedEntity(entity);
		//	}
		//	return entity;
		//}
	}
	public static class PoolsExtensions
	{
		public static IEntity SetParent(this IEntity entity, Transform parent)
		{
			if (entity.HasGameObject(out var obj))
				obj.transform.parent = parent;
			return entity;
		}
		public static IEntity SetTransform(this IEntity entity, SpawnPosition pos)
		{
			if (entity.HasGameObject(out var obj))
				obj.Apply(pos);
			return entity;
		}
		public static Transform GetTransform(this IEntity entity)
		{
			if (!entity.HasGameObject(out var obj))
			{
				Debug.LogError($"{entity} has no game object and therefor has no transform.");
				return null;
			}
			return obj.transform;
		}
		public static T GetComponent<T>(this IEntity entity)
			=> entity.HasGameObject(out var go) ? go.GetComponent<T>() : default;
		public static IPublisher<T> GetEvent<T>(this IEntity entity)
			=> entity.Resolver.Resolve<IPublisher<T>>();
		public static bool HasGameObject(this IEntity entity, out GameObject obj)
		{
			obj = default;
			if (entity.IsDestroyed)
				return false;
			if (entity.Resolver is IGameObject go)
				obj = go.Instance;
			else Debug.LogError($"{entity.Name} is not a GoEntity and can't be moved with SpawnPosition.");
			return obj;
		}
		public static Vector3 GetPosition(this IEntity entity)
			=> HasGameObject(entity, out var go) ? go.transform.position : Vector3.zero;
		public static Quaternion GetRotation(this IEntity entity)
			=> HasGameObject(entity, out var go) ? go.transform.rotation : Quaternion.identity;
		public static void SetPosition(this IEntity entity, Vector3 position)
		{
			if (entity.HasGameObject(out var go))
				go.transform.position = position;
		}
		public static IEntity Spawn(this IPools pools, IEntityFactory factory)
		{
			if (!factory.CreatesEntity)
				return null;
			var entity = pools.GetOrCreateUnspawnedEntity(factory);
			entity.Spawn();
			return entity;
		}
		public static IEntity Spawn(this GameObjectPools pools, IEntityFactory factory, SpawnPosition pos)
		{
			var entity = pools.GetOrCreateUnspawnedEntity(factory);
			entity.SetTransform(pos);
			entity.Spawn();
			return entity;
		}
		public static IEntity Spawn(this GameObjectPools pools, GameObject prefab, SpawnPosition pos)
		{
			var entity = pools.GetOrCreateUnspawnedEntity(prefab);
			entity.SetTransform(pos);
			entity.Spawn();
			return entity;
		}
		public static IEntity Spawn<T>(this GameObjectPools pools, T prefab, SpawnPosition pos, out T instance)
			where T : Component
		{
			if (!prefab)
			{
				instance = null;
				return null;
			}
			var result = Spawn(pools, prefab.gameObject, pos);
			instance = result.GetComponent<T>();
			return result;
		}
	}
}