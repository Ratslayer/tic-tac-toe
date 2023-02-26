using MessagePipe;
using BB.UI;

namespace BB
{
	public sealed record PublishGridEvents(
		IGrid Grid,
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
			var cell = Grid.GetCellData(msg._data.position);
			if (cell == _lastCellData)
				return;
			_lastCellData = cell;
			CellHovered.Publish(new(cell));
		}
		[Subscribe]
		void OnClick(PointerClicked msg)
		{
			var cell = Grid.GetCellData(msg._data.position);
			CellClicked.Publish(new(cell));
		}
	}
	public sealed record HoverCellEvent(CellData Cell);
	public sealed record ClickedCellEvent(CellData Cell);
}
