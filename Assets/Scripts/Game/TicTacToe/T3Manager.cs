using MessagePipe;
using UnityEngine;

namespace BB
{
	public enum Team
	{
		X,
		O
	}
	public sealed record GridTeams(IGrid Grid) : AbstractGridTable<Team>(Grid);
	public sealed record T3Manager(
		GridEntities Entities,
		GridTeams Teams,
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
			Teams.Set(msg.Cell, Team);
			Played.Publish(new(msg.Cell, Team));
			Team = Team == Team.X ? Team.O : Team.X;
			RedrawHint.Publish();
		}
	}
	public sealed record T3RuleChecker(
		TicTacToeInstaller Installer,
		IGrid Grid,
		GridEntities Entities,
		GridTeams Teams) : EntitySystem
	{
		[Subscribe]
		void OnPlayTurn(PlayedTurnEvent msg)
		{
			if (!HasTotallyWon())
				return;
			Debug.Log($"Team {msg.Team} has won!");
			Entities.DespawnAndClearAll();
			bool HasTotallyWon()
			{
				for (int i = -1; i <= 1; i++)
					for (int j = -1; j <= 1; j++)
						if (HasWon(i, j))
							return true;
				return false;
			}
			bool HasWon(int xDir, int yDir)
			{
				if (xDir == 0 && yDir == 0)
					return false;
				foreach (var i in 1..(Installer._winSize - 1))
				{
					var x = msg.Cell.X + i * xDir;
					var y = msg.Cell.Y + i * yDir;
					if (!Grid.IsValidIndex(x, y) 
						|| Entities.Get(x, y) == null 
						|| Teams.Get(x, y) != msg.Team)
						return false;
				}
				return true;
			}
		}
	}
	public sealed record PlayedTurnEvent(CellData Cell, Team Team);
}
