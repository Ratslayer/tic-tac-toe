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
		public GameModeInstaller _mode1, _mode2;
		//public StateMachine _machine;
		//[Required]
		//public AbstractStateProvider _m3State, _m4State;
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
		void Match3() => BeginGame(Installer._mode1);
		void Match4() => BeginGame(Installer._mode2);
		void BeginGame(AbstractSpawnAsset asset)
		{
			asset.Spawn();
			Entity.Despawn();
			//Override.Enter(Installer._machine, state);
			//var grid = Installer._grid.GetComponent<EntityBehaviour>().Entity;
			//Resize.Publish(new(Rules.Value.NumRows, Rules.Value.NumColumns));
			//Restart.Publish();
		}
		void AddButton(string name, Action action) => Installer._buttonPrefab.SpawnButton(Installer._buttonsRoot, name, action);
	}
	public static class UiUtils
	{
		public static IEntity SpawnButton(this GameObject prefab, Transform root, string name, Action onClick)
		{
			var entity = GlobalSystems.GoPools.Spawn(prefab, new SpawnPosition(root));
			entity.Publish(new UI.InitButton(name, onClick));
			return entity;
		}
		public static IEntity Spawn(this GameObject go, SpawnPosition pos) => GlobalSystems.GoPools.Spawn(go, pos);
		public static IEntity Spawn<T>(this T prefab, SpawnPosition pos, out T instance)
			where T : Component
		{
			var result = prefab.gameObject.Spawn(pos);
			instance = result.GetComponent<T>();
			return result;
		}
		public static IEntity Spawn(this IEntityFactory factory) => GlobalSystems.EntityPools.Spawn(factory);
	}
}
