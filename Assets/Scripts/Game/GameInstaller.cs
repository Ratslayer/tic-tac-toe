using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BB
{
	public sealed record MainMenuPrefab(GameObject Value) : ConstData<GameObject>(Value);
	public sealed class GameInstaller : AbstractProjectInstaller
	{
		
		[Required, SerializeField]
		GameObject _mainMenu;
		protected override void Install(IBinder binder)
		{
			base.Install(binder);
			binder.Const<MainMenuPrefab>(new(_mainMenu));
			binder.Var<GameOver>();
			binder.Over<GameRules>();
			binder.Over<GameStyle>();
			//events
			binder.Event<ResizeGridEvent>();
			binder.Event<StopGameEvent>();
			binder.Event<RestartGameEvent>();
			binder.Event<GameLogEvent>();
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
