using MessagePipe;
using UnityEngine;

namespace BB
{
	public sealed record ShowHintOnHover(
		GameStyle Style,
		GameObjectPools Pools,
		T3Manager Manager,
		GridTeams Teams,
		IPublisher<SpawnGridEntityEvent> Spawn,
		IPublisher<DespawnGridEntityEvent> Despawn) : EntitySystem
	{
		CellData _cell;
		[Subscribe]
		void OnHover(HoverCellEvent msg) => UpdateHint(msg.Cell);
		[Subscribe]
		void OnRedrawHint(RedrawHintEvent _) => UpdateHint(_cell);
		[Subscribe]
		void OnDespawn(DespawnGridEntityEvent msg)
		{
			if (_cell == msg.Cell)
				_cell = null;
		}
		[Subscribe]
		void OnSpawn(SpawnGridEntityEvent msg)
		{
			if (msg.Cell == _cell)
				_cell = null;
		}
		void ClearHint()
		{
			if (_cell == null)
				return;
			Despawn.Publish(new(_cell));
			_cell = null;
		}
		void UpdateHint(CellData cell)
		{
			ClearHint();
			if (cell == null || cell.Entities.Get(cell) != null)
				return;
			var prefab = Style.Value.GetHintPrefab(Manager.Team);
			Spawn.Publish(new(cell, prefab));
			_cell = cell;
		}
	}
	public sealed record RedrawHintEvent;
}
