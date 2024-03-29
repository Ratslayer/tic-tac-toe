﻿using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace BB
{
	public interface IGameRules
	{
		int NumRows { get; }
		int NumColumns { get; }
		int WinSize { get; }
		IEnumerable<Team> Teams { get; }
	}
	public sealed class GameRules : OverridableData<IGameRules> { }
	public static class RulesExtensions
	{
		public static bool IsValidIndex(this IGameRules grid, int x, int y) => x >= 0 && x < grid.NumColumns && y >= 0 && y < grid.NumRows;
		public static Team GetNextTeam(this IGameRules rules, Team team) => rules.Teams.GetNextCyclic(team);
	}
}
