using System.Collections.Generic;
using UnityEngine;

namespace BB
{
	public sealed class GameRulesAsset : AbstractStateComponent, IGameRules
	{
		[SerializeField]
		int _numRows = 3, _numColumns = 3, _winSize = 3;
		[SerializeField]
		List<Team> _teams = new List<Team>();
		public int NumRows => _numRows;
		public int NumColumns => _numColumns;
		public int WinSize => _winSize;

		public IEnumerable<Team> Teams => _teams;

		public override IDataOverride Create(IResolver resolver)
			=> new DataOverride<GameRules, IGameRules>(this);
	}
}
