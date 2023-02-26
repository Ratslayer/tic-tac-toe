using MessagePipe;
using UnityEngine;

namespace BB
{
	public enum Team
	{
		X,
		O
	}
	public sealed record T3Manager(
		GridEntitiesList Entities,
		TicTacToeInstaller Installer,
		GameObjectPools Pools,
		IGrid Grid,
		IPublisher<PlayedTurnEvent> Played,
		IPublisher<RedrawHintEvent> RedrawHint) : EntitySystem
	{
		public Team Team { get; private set; }
		[Subscribe]
		void OnGridClick(ClickedCellEvent msg)
		{
			if (msg.Cell == null)
				return;
			if (Entities.Get(msg.Cell) != null)
				return;
			var prefab = Team == Team.X ? Installer._xPrefab : Installer._oPrefab;
			var pos = Grid.GetCellCenter(msg.Cell);
			var entity = Pools.Spawn(prefab, new(Grid.Transform, pos, Quaternion.identity));
			var size = Grid.GetCellSize();
			entity.GetTransform().localScale = new Vector3(size, size, 1);
			Entities.Set(msg.Cell, entity);
			Played.Publish(new(msg.Cell, Team));
			Team = Team == Team.X ? Team.O : Team.X;
			RedrawHint.Publish();
		}
	}
	public sealed record PlayedTurnEvent(CellData Cell, Team Team);
}
