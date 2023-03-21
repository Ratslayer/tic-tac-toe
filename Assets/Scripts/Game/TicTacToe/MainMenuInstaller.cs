using MessagePipe;
using Sirenix.OdinInspector;
using System;
using UnityEngine;

namespace BB
{
	public sealed class MainMenuInstaller : AbstractInstallerBehaviour
	{
		[Required]
		public Transform _buttonsRoot;
		[Required]
		public GameObject _buttonPrefab;
		[Required]
		public StateMachine _machine;
		[Required]
		public AbstractStateProvider _m3State, _m4State;
		public GameObject _grid;
		public override void InstallBindings(IBinder binder)
		{
			binder.Data(this);
			binder.System<SpawnMainMenu>();
		}
	}
	public sealed record SpawnMainMenu(
		MainMenuInstaller Installer, 
		GameObjectPools Pools,
		IPublisher<ResizeGridEvent> Resize,
		IPublisher<RestartGameEvent> Restart,
		IStateController Override,
		GameRules Rules) : EntitySystem, IOnSpawn
	{
		public void OnSpawn()
		{
			AddButton("Match 3", Match3);
			AddButton("Match 4", Match4);
			AddButton("Exit", Application.Quit);
		}
		void Match3() => BeginGame(Installer._m3State);
		void Match4() => BeginGame(Installer._m4State);
		void BeginGame(AbstractStateProvider state)
		{
			Override.Enter(Installer._machine, state);
			var grid = Installer._grid.GetComponent<EntityBehaviour>().Entity;
			Resize.Publish(new(grid, Rules.Value.NumRows, Rules.Value.NumColumns));
			Restart.Publish();
		}
		void AddButton(string name, Action action)
		{
			var entity = Pools.Spawn(Installer._buttonPrefab, new SpawnPosition(Installer._buttonsRoot));
			entity.Publish(new UI.InitButton(name, action));
		}
	}
}
