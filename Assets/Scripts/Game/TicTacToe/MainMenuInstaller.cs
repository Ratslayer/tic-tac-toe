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

		public override void InstallBindings(IBinder binder)
		{
			binder.Data(this);
			binder.System<SpawnMainMenu>();
		}
	}
	public sealed record SpawnMainMenu(MainMenuInstaller Installer, GameObjectPools Pools) : EntitySystem, IOnSpawn
	{
		public void OnSpawn()
		{
			AddButton("Begin", null);
			AddButton("Porn", null);
			AddButton("Exit", null);
		}
		void AddButton(string name, Action action)
		{
			var entity = Pools.Spawn(Installer._buttonPrefab, new SpawnPosition(Installer._buttonsRoot));
			entity.Publish(new UI.InitButton(name, action));
		}
	}
}
