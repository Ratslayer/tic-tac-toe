﻿using MessagePipe;
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
	public sealed record RestartGameEvent;
	public sealed record T3Manager(
		GridTeams Teams,
		GameStyle Style,
		IPublisher<PlayedTurnEvent> Played,
		IPublisher<SpawnGridEntityEvent> SpawnGridEntity,
		IPublisher<DespawnGridEntityEvent> DespawnGridEntity) : EntitySystem
	{
		public Team Team { get; private set; }
		public int Turn { get; private set; }
		[Subscribe]
		void OnGridResize(ResizeGridEvent msg) => Teams.Init(msg.Cols, msg.Rows);
		[Subscribe]
		void OnGameRestart(RestartGameEvent _)
		{
			Team = Team.X;
			Turn = 0;
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
			SpawnGridEntity.Publish(new(msg.Cell, prefab));
			Teams.Set(msg.Cell, Team);
			Turn++;
			var oldTeam = Team;
			Team = Team == Team.X ? Team.O : Team.X;
			Played.Publish(new(msg.Cell, oldTeam, Turn));
		}
	}
	public sealed record T3RuleChecker(
		GameRules Rules,
		GridTeams Teams,
		IPublisher<RestartGameEvent> Restart) : EntitySystem
	{
		[Subscribe]
		void OnPlayTurn(PlayedTurnEvent msg)
		{
			var entities = msg.Cell.Entities;
			if (!IsGameOver(msg.Team, out var team))
				return;
			Debug.Log($"Team {team} has won on turn {msg.TurnNumber}!");
			Restart.Publish();
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
					won = Team.None;
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
