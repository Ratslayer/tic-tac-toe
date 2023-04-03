using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace BB
{
	public sealed class GameModeInstaller : EntityAsset
	{
		[Required]
		public AbstractStateProvider _state;
		[Required]
		public GameUi _ui;
		protected override void Install(IBinder binder)
		{
			binder.Data(this);
			binder.System<InitGameUi>();
		}
	}
	public sealed record InitGameUi(
		GameModeInstaller Installer,
		IStateController Controller,
		GameRules Rules,
		GameStyle Style,
		MainMenuPrefab MainMenu) : EntitySystem, IOnSpawn, IOnDespawn
	{
		IEntity _grid;

		public void OnDespawn()
		{
			_grid.Despawn();
		}

		public void OnSpawn()
		{
			Controller.Enter(Installer._state);
			_grid = Installer._ui.gameObject.Spawn(new(null));
			Publish(new ResizeGridEvent(Rules.Value.NumColumns));
			Publish(new RestartGameEvent());
		}
	}
}
