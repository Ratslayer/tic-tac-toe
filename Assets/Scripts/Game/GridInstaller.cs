using UnityEngine;
using BB.UI;
using Sirenix.OdinInspector;
using MessagePipe;

namespace BB
{
	public sealed class GridInstaller : AbstractInstallerBehaviour
	{
		public float _lineThickness = 0.1f;
		[Required]
		public GameObject _linePrefab;
		public override void InstallBindings(IBinder binder)
		{
			binder.Component<GridInstaller>();
			binder.System<GridRect>();
			binder.System<PublishGridEvents>();
			binder.System<GridEntities>();
			binder.System<SpawnGridEntity>();
			binder.Pointer();
			binder.Hover();
		}
	}
	public sealed record CellData(int X, int Y, GridEntities Entities);
	public sealed record SpawnGridEntityEvent(CellData Cell, GameObject Prefab);
	public sealed record DespawnGridEntityEvent(CellData Cell);
	public sealed record GridEntitiesChangedEvent(GridEntities Entities);
	public sealed record SpawnGridEntity(
		GridEntities Entities,
		GridRect Rect,
		GameObjectPools Pools) : EntitySystem
	{
		[Subscribe]
		void SpawnEntity(SpawnGridEntityEvent msg)
		{
			var pos = Rect.GetCellCenter(msg.Cell);
			var entity = Pools.Spawn(msg.Prefab, new(Rect.Transform, pos, Quaternion.identity));
			var size = Rect.GetCellSize();
			entity.GetTransform().localScale = new Vector3(size, size, 1);
			Entities.Set(msg.Cell, entity);
		}
		[Subscribe]
		void DespawnEntity(DespawnGridEntityEvent msg)
		{
			var entity = Entities.Get(msg.Cell);
			if (entity == null)
				return;
			entity.Despawn();
			Entities.Set(msg.Cell, null);
		}
	}
}
