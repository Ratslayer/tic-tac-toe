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
			binder.System<EntityPools>();
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
	public static class GlobalSystems
	{
		static GameObjectPools _goPools;
		static EntityPools _entityPools;
		public static GameObjectPools GoPools => GetSingleton(ref _goPools);
		public static EntityPools EntityPools => GetSingleton(ref _entityPools);
		static T GetSingleton<T>(ref T value)
		{
			if (value == null)
				value = DiServices.Root.Resolve<T>();
			return value;
		}
	}
}
