using Sirenix.OdinInspector;
using UnityEngine;

namespace BB
{
	public sealed class TicTacToeInstaller : AbstractInstaller
	{
		[Required]
		public GameObject _xPrefab, _xHintPrefab, _oPrefab, _oHintPrefab;
		public int _winSize = 3;
		protected override void Install(IBinder binder)
		{
			binder.Data(this);
			binder.System<IHintRules, TicTacToeHintRules>();
			binder.System<T3Manager>();
			binder.Event<PlayedTurnEvent>();
		}
	}
	public sealed record TicTacToeHintRules(
		TicTacToeInstaller Installer,
		GridEntitiesList Entities,
		T3Manager Manager) : EntitySystem, IHintRules
	{
		public GameObject GetPrefab(CellData cell)
		{
			var cellDweller = Entities.Get(cell);
			return cellDweller == null ? Manager.Team == Team.X ? Installer._xHintPrefab : Installer._oHintPrefab : null;
		}
	}
}
