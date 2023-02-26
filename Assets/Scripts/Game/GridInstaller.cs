using UnityEngine;

namespace BB
{
	public sealed class GridInstaller : AbstractInstallerBehaviour
	{
		public float _lineThickness = 0.1f;
		public GameObject _linePrefab;
		public override void InstallBindings(IBinder binder)
		{
			binder.Component<GridInstaller>();
			binder.System<DrawGrid>();
		}
	}
	public sealed record DrawGrid(
		GridInstaller Grid,
		GameObjectPools Pools,
		IGameRules Rules) : EntitySystem, IOnSpawn
	{
		public void OnSpawn()
		{
			var transform = Entity.GetComponent<RectTransform>();
			var size = transform.rect.size;
			var rows = Rules.NumRows;
			var cols = Rules.NumColumns;
			var cellSize = size.x / cols;
			foreach (var i in 0..cols)
				DrawLine(new(0, size.y), Vector2.right, i);
			foreach (var i in 0..rows)
				DrawLine(new(size.x, 0), Vector2.up, i);
			void DrawLine(Vector2 to, Vector2 dir, int multiplier)
			{
				var offset = multiplier * cellSize * dir - size * 0.5f;
				SpawnLine(offset, to + offset);
			}
		}
		void SpawnLine(Vector2 from, Vector2 to)
		{
			var line = Pools.Spawn(Grid._linePrefab, new(Grid.transform));
			var dir = to - from;
			var center = (from + to) * 0.5f;
			var angle = Vector3.Angle(Vector3.right, dir);
			var scale = new Vector3(dir.magnitude, Grid._lineThickness, 1f);
			var t = line.GetTransform();
			t.SetLocalTRS(center, Quaternion.Euler(0, 0, angle), scale);
		}
	}
}
