using MessagePipe;
using System.Linq;
using UnityEngine;

namespace BB
{
	public sealed record GridTeams : AbstractGridTable<Team> { }
	public sealed record RestartGameEvent;
	public sealed record StopGameEvent;
	public sealed class GameOver : VarData<bool> { }
	public sealed record T3Manager(
		GridTeams Teams,
		GameStyle Style,
		GameRules Rules,
		GameOver GameOver,
		IPublisher<PlayedTurnEvent> Played,
		IPublisher<SpawnGridEntityEvent> SpawnGridEntity,
		IPublisher<DespawnGridEntityEvent> DespawnGridEntity) : EntitySystem
	{
		public Team Team { get; private set; }
		public int Turn { get; private set; }
		[Subscribe]
		void OnGridResize(ResizeGridEvent msg) => Teams.Init(msg.Cols, msg.Cols);
		[Subscribe]
		void OnGameRestart(RestartGameEvent _)
		{
			Team = Rules.Value.GetNextTeam(null);
			Turn = 0;
			GameOver.Value = false;
			UpdateTeamName();
		}
		[Subscribe]
		void OnGridClick(ClickedCellEvent msg)
		{
			if (GameOver.Value)
				return;
			if (msg.Cell == null)
				return;
			if (Teams.Get(msg.Cell) != null)
				return;
			DespawnGridEntity.Publish(new(msg.Cell));
			SpawnGridEntity.Publish(new(msg.Cell, Team._icon));
			Teams.Set(msg.Cell, Team);
			Turn++;
			var oldTeam = Team;
			Team = Rules.Value.GetNextTeam(Team);
			UpdateTeamName();
			Played.Publish(new(msg.Cell, oldTeam, Turn));
		}
		void UpdateTeamName() => Publish(new GameLogEvent($"{Team} turn"));
	}
	public sealed record T3RuleChecker(
		GameRules Rules,
		GridTeams Teams,
		GameOver GameOver,
		IPublisher<RestartGameEvent> Restart) : EntitySystem
	{
		[Subscribe]
		void OnPlayTurn(PlayedTurnEvent msg)
		{
			var entities = msg.Cell.Entities;
			if (!IsGameOver(msg.Team, out var team))
				return;
			var text = team != null
				? $"Team {team} has won on turn {msg.TurnNumber}!"
				: "It's a draw!";
			Publish(new GameLogEvent(text));
			Publish(new StopGameEvent());
			GameOver.Value = true;
			//Restart.Publish();
			bool IsGameOver(Team team, out Team won)
			{
				for (int i = -1; i <= 1; i++)
					for (int j = -1; j <= 1; j++)
						if (HasWon(i, j))
						{
							won = team;
							return true;
						}
				if (msg.TurnNumber == entities.NumCells)
				{
					won = null;
					return true;
				}
				won = default;
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
	public sealed record PlayedTurnEvent(CellData Cell, Team Team, int TurnNumber);
}
