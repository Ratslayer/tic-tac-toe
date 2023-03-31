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
			binder.System<InitSystems>();
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
		sealed record InitSystems(
			GameObjectPools GoPools,
			EntityPools EntityPools) : EntitySystem, GlobalSystems.ISystems, IOnInstall
		{
			public void OnStart()
			{
				GlobalSystems.Systems = this;
			}
		}
	}
	public static class GlobalSystems
	{
		public interface ISystems
		{
			GameObjectPools GoPools { get; }
			EntityPools EntityPools { get; }
		}
		public static ISystems Systems { get; set; }
		public static GameObjectPools GoPools => Systems.GoPools; //GetSingleton(ref _goPools);
		public static EntityPools EntityPools => Systems.EntityPools;// GetSingleton(ref _entityPools);
		//static T GetSingleton<T>(ref T value)
		//{
		//	if (value == null)
		//		value = DiServices.Root.Resolve<T>();
		//	return value;
		//}
	}
}
