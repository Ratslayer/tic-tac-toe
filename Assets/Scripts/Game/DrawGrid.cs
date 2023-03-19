using System.Collections.Generic;
using UnityEngine;

namespace BB
{
	public sealed record ResizeGridEvent(IEntity CanvasEntity, int Rows, int Cols);
	public sealed record GridRect(
		GameStyle Style,
		GameObjectPools Pools,
		GridEntities Entities) : EntitySystem
	{
		public RectTransform Transform => Entity.GetTransform().GetRectTransform();
		readonly List<IEntity> _lines = new();
		public Vector2 BottomLeft => Transform.position + new Vector3(Transform.rect.xMin, Transform.rect.yMin, 0);
		public Vector2 Size => Transform.rect.size;
		public int NumRows { get; private set; } = 1;
		public int NumCols { get; private set; } = 1;
		public float GetCellSize() => Size.x / NumCols;
		public Vector2 GetCellCenter(CellData cell)
		{
			var cellSize = GetCellSize();
			var pos = BottomLeft + new Vector2(cellSize * (cell.X + 0.5f), cellSize * (cell.Y + 0.5f));
			return pos;
		}
		[Subscribe]
		void OnResize(ResizeGridEvent msg)
		{
			if (msg.CanvasEntity != Entity)
				return;
			_lines.DespawnAndClear();
			//Transform = msg.CanvasEntity.GetTransform().GetRectTransform();
			NumRows = msg.Rows;
			NumCols = msg.Cols;
			Entities.Init(NumCols, NumRows);
			var size = Size;
			var pos = BottomLeft;
			var cellSize = GetCellSize();
			foreach (var i in 0..NumCols)
				DrawLine(new(0, size.y), Vector2.right, i);
			foreach (var i in 0..NumRows)
				DrawLine(new(size.x, 0), Vector2.up, i);
			void DrawLine(Vector2 to, Vector2 dir, int multiplier)
			{
				var offset = pos + multiplier * cellSize * dir;
				SpawnLine(offset, to + offset);
			}
		}
		void SpawnLine(Vector2 from, Vector2 to)
		{
			var style = Style.Value;
			var line = Pools.Spawn(style.LinePrefab, new(Transform));
			_lines.Add(line);
			var dir = to - from;
			var center = (from + to) * 0.5f;
			var angle = Vector3.Angle(Vector3.right, dir);
			var scale = new Vector3(dir.magnitude, style.LineWidth, 1f);
			var t = line.GetTransform();
			t.SetPositionAndRotation(center, Quaternion.Euler(0, 0, angle));
			t.localScale = scale;
		}
	}
}
