using MessagePipe;
using BB.UI;
using UnityEngine;

namespace BB
{
	public sealed record PublishGridEvents(
		GridRect Rect,
		GridEntities Entities,
		IPublisher<HoverCellEvent> CellHovered,
		IPublisher<ClickedCellEvent> CellClicked)
		: EntitySystem
	{
		CellData _lastCellData;
		[Subscribe]
		void OnHoverBegin(HoverEntered msg)
		{
		}
		[Subscribe]
		void OnHoverEnd(HoverExited msg)
		{
			CellHovered.Publish(new(null));
		}
		[Subscribe]
		void OnHoverMove(HoverMoved msg)
		{
			var cell = GetCellData(msg._data.position);
			if (cell == _lastCellData)
				return;
			_lastCellData = cell;
			CellHovered.Publish(new(cell));
		}
		[Subscribe]
		void OnClick(PointerClicked msg)
		{
			var cell = GetCellData(msg._data.position);
			CellClicked.Publish(new(cell));
		}
		public CellData GetCellData(Vector2 pointerPosition)
		{
			var localPos = pointerPosition - Rect.BottomLeft;
			var size = Rect.Size;
			if (!(localPos.x.IsBetween(0f, size.x) && localPos.y.IsBetween(0f, size.y)))
				return null;
			var normalizedSize = localPos.Div(size);
			var x = (int)(normalizedSize.x * Rect.NumCols);
			var y = (int)(normalizedSize.y * Rect.NumRows);
			var result = new CellData(x, y, Entities);
			return result;
		}
		
	}
	public sealed record HoverCellEvent(CellData Cell);
	public sealed record ClickedCellEvent(CellData Cell);
}
