using UnityEngine;

namespace BB
{
	public interface IGrid
	{
		void SetGrid(IEntity entity);
		public Vector2 BottomLeft { get; }
		public Vector2 Size { get; }
		public int NumRows { get; }
		public int NumColumns { get; }
		public Transform Transform { get; }
	}
	public sealed record Grid(IGameRules Rules) : EntitySystem, IGrid
	{
		RectTransform _rect;
		public void SetGrid(IEntity entity) => _rect = entity.GetTransform().GetRectTransform();
		public Vector2 BottomLeft => _rect.position + new Vector3(_rect.rect.xMin, _rect.rect.yMin, 0);

		public Vector2 Size => _rect.rect.size;

		public int NumRows => Rules.NumRows;

		public int NumColumns => Rules.NumColumns;

		public Transform Transform => _rect.transform;
	}
	public static class GridExtensions
	{
		public static CellData GetCellData(this IGrid grid, Vector2 pointerPosition)
		{
			var localPos = pointerPosition - grid.BottomLeft;
			if (!(localPos.x.IsBetween(0f, grid.Size.x) && localPos.y.IsBetween(0f, grid.Size.y)))
				return null;
			var normalizedSize = localPos.Div(grid.Size);
			var result = new CellData((int)(normalizedSize.x * grid.NumColumns), (int)(normalizedSize.y * grid.NumRows));
			return result;
		}
		public static float GetCellSize(this IGrid grid) => grid.Size.x / grid.NumColumns;
		public static Vector2 GetCellCenter(this IGrid grid, CellData cell)
		{
			var cellSize = grid.GetCellSize();
			var pos = grid.BottomLeft + new Vector2(cellSize * (cell.X + 0.5f), cellSize * (cell.Y + 0.5f));
			return pos;
		}
		public static bool IsValidIndex(this IGrid grid, int x, int y) => x >= 0 && x < grid.NumColumns && y >= 0 && y < grid.NumRows;
	}
}
