using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;

namespace BB
{
	public sealed class GameUi : AbstractInstallerBehaviour
	{
		[Required]
		public Transform _buttons;
		[Required]
		public TextMeshProUGUI _text;
		public override void InstallBindings(IBinder binder)
		{
			binder.Data(this);
			binder.System<DisplayMessage>();
			binder.System<InitGameUiButtons>();
		}
	}
	public sealed record GameLogEvent(string Message);
	public sealed record DisplayMessage(GameUi Ui)
		: EntitySystem
	{
		[Subscribe]
		void OnGameLog(GameLogEvent msg) => Ui._text.text = msg.Message;
	}
	public sealed record InitGameUiButtons(
		GameUi Ui,
		GameRules Rules,
		GameStyle Style,
		MainMenuPrefab MainMenu)
		: EntitySystem, IOnSpawn
	{
		public void OnSpawn()
		{
			Button("Restart", () => Publish(new RestartGameEvent()));
			Button("Exit", Exit);
			
			void Exit()
			{
				Entity.Despawn();
				MainMenu.Value.Spawn(new(null));
			}
			void Button(string name, Action action) => Style.Value.ButtonPrefab.SpawnButton(Ui._buttons, name, action);
		}
	}
}
