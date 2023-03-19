using Sirenix.OdinInspector;
using UnityEngine;
namespace BB
{
	public sealed class SpawnOnStart : AbstractInstallerBehaviour
	{
		[SerializeField, Required]
		SpawnAsset _data;
		public override void InstallBindings(IBinder binder)
		{
			binder.Data(_data);
			binder.System<SpawnOnStartSystem>();
		}
		sealed record SpawnOnStartSystem(
		EntityTransform T,
		SpawnAsset Data,
		GameObjectPools Pooling) : EntitySystem, IOnSpawn
		{
			//[Subscribe]
			//void OnLevelLoaded(LevelLoaded _)
			//{
			//	Pooling.Spawn(Data, new(null, T.Value));
			//}
			public void OnSpawn()
			{
				Pooling.Spawn(Data, new(null, T.Value));
			}
		}
	}
}