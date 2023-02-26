using UnityEngine;

namespace BB
{
	public interface IHintRules
	{
		GameObject GetPrefab(CellData cell);
	}
	public sealed record ShowHintOnHover(
		IHintRules Rules,
		IGrid Grid,
		GameObjectPools Pools) : EntitySystem
	{
		IEntity _instance;
		CellData _cell;
		[Subscribe]
		void OnHover(HoverCellEvent msg)
		{
			_cell = msg.Cell;
			UpdateHint();
		}
		[Subscribe]
		void OnRedrawHint(RedrawHintEvent _) => UpdateHint();
		void ClearHint()
		{
			_instance?.Despawn();
			_instance = null;
		}
		void UpdateHint()
		{
			if (_cell == null)
			{
				ClearHint();
				return;
			}
			var hintPrefab = Rules.GetPrefab(_cell);
			if (hintPrefab == null)
			{
				ClearHint();
				return;
			}
			if (_instance == null)
			{
				_instance = Pools.Spawn(hintPrefab, new(Grid.Transform));
				var cellSize = Grid.GetCellSize();
				_instance.GetTransform().localScale = new Vector3(cellSize, cellSize, 1f);
			}
			_instance.GetTransform().position = Grid.GetCellCenter(_cell);
		}
	}
	public sealed record RedrawHintEvent;
}
