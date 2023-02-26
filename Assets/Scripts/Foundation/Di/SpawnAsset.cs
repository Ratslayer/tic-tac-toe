using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;

namespace BB
{
	public sealed class SpawnAsset : EntityAsset
	{
		[SerializeField, Required]
		GameObject _prefab;
		[SerializeField]
		AbstractInstaller _installer;
		public GameObject Prefab => _prefab;
		protected override IEntity Create(IResolver parent)
			=> parent.CreateChildGameObject(_prefab, true, _installer != null ? _installer.InstallBindings : null);
	}
	//public interface ISpawnData
	//{
	//	GameObject Prefab { get; }
	//	IEnumerable<IBaseInstaller> GetInstallers();
	//	string Name => Prefab.name;
	//}
	public static class SpawnUtils
	{
		//static GameObject Instantiate(GameObject prefab, SpawnPosition pos)
		//	=> UnityEngine.Object.Instantiate(prefab, pos._position, pos._rotation, pos._parent);
		//public static IEntity CreateEntity(IEntityFactory data, IResolver parent, SpawnPosition pos)
		//	=> CreateEntity(data.Prefab, parent, pos, data.GetInstallers());
		//public static IEntity CreateEntity(GameObject prefab, IResolver parent, SpawnPosition pos,  IEnumerable<IBaseInstaller> installers)
		//{
		//	var instance = prefab.Instantiate(pos);
		//	instance.name = prefab.name;
		//	var entity = InstallerUtils.CreateGoEntity(instance, parent, installers);
		//	return entity;
		//}
		//public static IEntity CreateEntity(GameObject instance, IResolver parent, IEnumerable<IBaseInstaller> installers)
		//{
		//	var entity = InstallerUtils.BindEntity(instance, parent);
		//	entity.AddInstaller(new GameObjectEntityInstaller(instance.transform, instance.transform.parent));
		//	entity.AddInstallers(installers);
		//	entity.Install();
		//	return entity;
		//}
		//public static IEntity CreateEntity(IResolver parent, IEnumerable<IBaseInstaller> installers)
		//{
		//	var entity = InstallerUtils.CreateEntity(parent);
		//	entity.AddInstallers(installers);
		//	entity.Install();
		//	return entity;
		//}
	}
}