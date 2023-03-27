using System.Collections.Generic;

namespace BB
{
	public interface IEntityFactory
	{
		bool CreatesEntity => true;
		string Name { get; }
		IEntity Create(IResolver parent);
	}
	//public sealed record SubEntityContainer : EntitySystem, IOnDespawn
	//{
	//	readonly Dictionary<IEntityFactory, IEntity> _entities = new();
	//	public IEntity GetOrCreate(IEntityFactory factory)
	//	{
	//		if (factory == null)
	//			return null;
	//		if (!HasEntity(factory, out var entity))
	//		{
	//			entity = factory.CreateEntity(Resolver);
	//			_entities.Add(factory, entity);
	//		}
	//		return entity;
	//	}
	//	public bool HasEntity(IEntityFactory factory, out IEntity entity)
	//	{
	//		if (factory != null)
	//			return _entities.TryGetValue(factory, out entity);
	//		entity = null;
	//		return false;
	//	}
	//	public void OnDespawn()
	//	{
	//		foreach (var entity in _entities.Values)
	//			entity?.Despawn();
	//	}
	//}
	//public static class SubEntityContainerExtensions
	//{
	//	public static bool TryCreateEntity(this IEntityFactory factory, IResolver parent, out IEntity entity)
	//	{
	//		entity = factory.CreateEntity(parent);
	//		return entity != null;
	//	}
	//	public static IEntity Get(this SubEntityContainer container, IEntityFactory factory)
	//		=> container.HasEntity(factory, out var result) ? result : default;
	//	public static IEntity Spawn(this SubEntityContainer container, IEntityFactory factory)
	//	{
	//		if (factory == null)
	//			return null;
	//		var entity = container.GetOrCreate(factory);
	//		entity?.Spawn();
	//		return entity;
	//	}
	//	public static IEntity Spawn(this SubEntityContainer container, IEntityFactory factory, SpawnPosition pos)
	//	{
	//		if (factory == null)
	//			return null;
	//		var entity = container.GetOrCreate(factory);
	//		entity?.SetTransform(pos);
	//		entity?.Spawn();
	//		return entity;
	//	}
	//	public static void Despawn(this SubEntityContainer container, IEntityFactory factory)
	//	{
	//		if (factory != null && container.HasEntity(factory, out var entity))
	//			entity?.Despawn();
	//	}
	//	public static void SpawnDifference(
	//		this SubEntityContainer container,
	//		IEnumerable<IEntityFactory> spawned,
	//		IEnumerable<IEntityFactory> despawned)
	//	{
	//		if (despawned == null)
	//		{
	//			if (spawned != null)
	//				foreach (var factory in spawned)
	//					container.Spawn(factory);
	//		}
	//		else if (spawned == null)
	//		{
	//			foreach (var factory in despawned)
	//				container.Despawn(factory);
	//		}
	//		else
	//		{
	//			foreach (var factory in despawned)
	//				if (!spawned.Contains(factory))
	//					container.Despawn(factory);
	//			foreach (var factory in spawned)
	//				if (!despawned.Contains(factory))
	//					container.Spawn(factory);
	//		}
	//	}
	//}
}