using Sirenix.OdinInspector;
using UnityEngine;

namespace BB
{
	public sealed class SpawnObjectInstaller : AbstractInstaller
	{
		[SerializeField, Required]
		GameObject _prefab;
		protected override void Install(IBinder binder)
		{
			binder.Data(_prefab);
		}
		sealed record SpawnSystem(GameObject Prefab, GameObjectPools Pools) : EntitySystem, IOnSpawn
		{
			public void OnSpawn()
			{
				Pools.Spawn(Prefab, new());
			}
		}
	}
}
