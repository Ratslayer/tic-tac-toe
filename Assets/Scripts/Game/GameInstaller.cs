using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BB
{
	public sealed class GameInstaller : AbstractInstaller
	{
		[Required, SerializeField]
		AbstractStateProvider _defaultState;
		protected override void Install(IBinder binder)
		{
			binder.StateMachine(_defaultState);
			binder.System<GameObjectPools>();
			binder.System<IPools, EntityPools>();
			binder.Over<GameRules>();
			binder.Over<GameStyle>();
			//events
			binder.Event<ResizeGridEvent>();
			binder.Event<RestartGameEvent>();
			binder.Event<HoverCellEvent>();
			binder.Event<ClickedCellEvent>();
			binder.Event<RedrawHintEvent>();
			binder.Event<PlayedTurnEvent>();
			binder.Event<SpawnGridEntityEvent>();
			binder.Event<DespawnGridEntityEvent>();
			binder.T3Engine();
		}
	}
}
