using UnityEngine;

namespace BB.UI
{
	public sealed record DrawGrid(
		GridInstaller GridUi,
		GameObjectPools Pools,
		IGrid Grid) : EntitySystem, IOnSpawn
	{
		public void OnSpawn()
		{
			Grid.SetGrid(Entity);
			var size = Grid.Size;
			var pos = Grid.BottomLeft;
			var rows = Grid.NumRows;
			var cols = Grid.NumColumns;
			var cellSize = Grid.GetCellSize();
			foreach (var i in 0..cols)
				DrawLine(new(0, size.y), Vector2.right, i);
			foreach (var i in 0..rows)
				DrawLine(new(size.x, 0), Vector2.up, i);
			void DrawLine(Vector2 to, Vector2 dir, int multiplier)
			{
				var offset = pos + multiplier * cellSize * dir;
				SpawnLine(offset, to + offset);
			}
		}
		void SpawnLine(Vector2 from, Vector2 to)
		{
			var line = Pools.Spawn(GridUi._linePrefab, new(GridUi.transform));
			var dir = to - from;
			var center = (from + to) * 0.5f;
			var angle = Vector3.Angle(Vector3.right, dir);
			var scale = new Vector3(dir.magnitude, GridUi._lineThickness, 1f);
			var t = line.GetTransform();
			t.SetPositionAndRotation(center, Quaternion.Euler(0, 0, angle));
			t.localScale = scale;
		}
	}
}
