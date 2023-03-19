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
		public IEntity GetEntity(int x, int y);
	}
	public sealed record Grid(GameRules Rules, GridEntities Entities) : EntitySystem, IGrid
	{
		RectTransform _rect;
		public void SetGrid(IEntity entity) => _rect = entity.GetTransform().GetRectTransform();
		public Vector2 BottomLeft => _rect.position + new Vector3(_rect.rect.xMin, _rect.rect.yMin, 0);

		public Vector2 Size => _rect.rect.size;

		public int NumRows => Rules.Value.NumRows;

		public int NumColumns => Rules.Value.NumColumns;

		public Transform Transform => _rect.transform;
		public IEntity GetEntity(int x, int y) => Entities.Get(x, y);
	}
}
