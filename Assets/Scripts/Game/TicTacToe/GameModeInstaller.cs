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
			_grid = Installer._ui.Spawn(new(null), out var ui);
			Button("Restart", () => Publish(new RestartGameEvent()));
			Button("Exit", Exit);
			Publish(new ResizeGridEvent(Rules.Value.NumColumns));
			Publish(new RestartGameEvent());
			void Exit()
			{
				Entity.Despawn();
				MainMenu.Value.Spawn(new(null));
			}
			void Button(string name, Action action) => Style.Value.ButtonPrefab.SpawnButton(ui._buttons, name, action);
		}
	}
}
