using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace BB
{
	public sealed class SpawnGameObject : AbstractSpawnAsset
	{
		[SerializeField, Required]
		GameObject _prefab;
		[SerializeField]
		AbstractInstaller _installer;
		public GameObject Prefab => _prefab;
		protected override IEntity CreateEntity(IResolver parent)
			=> parent.CreateChildGameObject(_prefab, true, _installer != null ? _installer.InstallBindings : null);
	}
}