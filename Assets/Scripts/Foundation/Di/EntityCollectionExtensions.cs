using System.Collections.Generic;

namespace BB
{
	public static class EntityCollectionExtensions
	{
		public static void DespawnAndClear(this List<IEntity> entities)
		{
			foreach (var entity in entities)
				entity.Despawn();
			entities.Clear();
		}
		public static void DespawnAndClear<T>(this List<T> systems)
			where T : ISystem
		{
			foreach (var system in systems)
				system.Entity.Despawn();
			systems.Clear();
		}
		public static void SpawnAndAddAll(this List<IEntity> entities, IPools pools, IEnumerable<IEntityFactory> factories)
		{
			if (factories != null)
				foreach (var factory in factories)
					entities.Add(pools.Spawn(factory));
		}
	}
}