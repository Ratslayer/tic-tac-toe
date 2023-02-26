using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BB
{
	public sealed class GameInstaller : AbstractInstaller, IGameRules
	{
		[SerializeField]
		int _numRows, _numCols;

		public int NumRows => _numRows;

		public int NumColumns => _numCols;

		protected override void Install(IBinder binder)
		{
			binder.System<GameObjectPools>();
			binder.System<IPools, EntityPools>();
			binder.Data<IGameRules>(this);
			binder.System<IGrid, Grid>();
			binder.System<GridEntities>();
			binder.Event<HoverCellEvent>();
			binder.Event<ClickedCellEvent>();
			binder.Event<RedrawHintEvent>();
		}
	}
	public interface IGameRules
	{
		int NumRows { get; }
		int NumColumns { get; }
	}
}
