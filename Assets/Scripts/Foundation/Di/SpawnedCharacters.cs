using MessagePipe;
using System.Collections.Generic;
namespace BB
{
	public sealed record SpawnedCharacter(IEntity Entity);
	public sealed record DespawnedCharacter(IEntity Entity);
	public sealed record SpawnedCharacters : EntitySystem
	{
		readonly List<IEntity> _characters = new();
		public IEnumerable<IEntity> Entities => _characters;
		[Subscribe]
		void OnSpawnCharacter(SpawnedCharacter msg) => _characters.Add(msg.Entity);
		[Subscribe]
		void OnDespawnCharacter(DespawnedCharacter msg) => _characters.Remove(msg.Entity);
	}
	public sealed record BroadcastCharacterSpawnEvents(
		IPublisher<SpawnedCharacter> Spawned,
		IPublisher<DespawnedCharacter> Despawned)
		: EntitySystem, IOnSpawn, IOnDespawn
	{
		public void OnDespawn() => Despawned.Publish(new(Entity));

		public void OnSpawn() => Spawned.Publish(new(Entity));
	}
}