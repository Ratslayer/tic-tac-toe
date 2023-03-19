using MessagePipe;
using UnityEngine;

namespace BB
{
	public enum Team
	{
		None = 0,
		X = 1,
		O = 2
	}
	public sealed record GridTeams : AbstractGridTable<Team> { }
	public sealed record T3Manager(
		GridTeams Teams,
		GameStyle Style,
		IPublisher<PlayedTurnEvent> Played,
		IPublisher<RedrawHintEvent> RedrawHint,
		IPublisher<SpawnGridEntityEvent> SpawnGridEntity,
		IPublisher<DespawnGridEntityEvent> DespawnGridEntity) : EntitySystem
	{
		public Team Team { get; private set; }
		[Subscribe]
		void OnGridResize(ResizeGridEvent msg)
		{
			Teams.Init(msg.Cols, msg.Rows);
		}
		[Subscribe]
		void OnGridClick(ClickedCellEvent msg)
		{
			if (msg.Cell == null)
				return;
			if (Teams.Get(msg.Cell) != Team.None)
				return;
			DespawnGridEntity.Publish(new(msg.Cell));
			var prefab = Style.Value.GetTilePrefab(Team);
			//Team == Team.X ? Installer._xPrefab : Installer._oPrefab;
			SpawnGridEntity.Publish(new(msg.Cell, prefab));
			Teams.Set(msg.Cell, Team);
			Played.Publish(new(msg.Cell, Team));
			Team = Team == Team.X ? Team.O : Team.X;
			RedrawHint.Publish();
		}
	}
	public sealed record T3RuleChecker(
		GameRules Rules,
		GridTeams Teams) : EntitySystem
	{
		[Subscribe]
		void OnPlayTurn(PlayedTurnEvent msg)
		{
			var entities = msg.Cell.Entities;
			if (!HasTotallyWon())
				return;
			Debug.Log($"Team {msg.Team} has won!");
			entities.Clear();
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
				foreach (var i in 1..(Rules.Value.WinSize - 1))
				{
					var x = msg.Cell.X + i * xDir;
					var y = msg.Cell.Y + i * yDir;
					if (!Rules.Value.IsValidIndex(x, y)
						|| entities.Get(x, y) == null
						|| Teams.Get(x, y) != msg.Team)
						return false;
				}
				return true;
			}
		}
	}
	public sealed record PlayedTurnEvent(CellData Cell, Team Team);
}
