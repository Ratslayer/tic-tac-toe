using System;
using UnityEngine;

namespace BB
{
	public static class ResolverCreateExtensions
	{
		public static IEntity CreateChildGameObject(this IResolver parent, GameObject prefab, bool isRoot, Action<IBinder> install)
		{
			var instance = UnityEngine.Object.Instantiate(prefab);
			var entity = parent.BindChildGameObject(instance, isRoot, install);
			return entity;
		}
		public static IEntity CreateChildGameObject(this IResolver parent, string name, Action<IBinder> install)
		{
			var instance = new GameObject(name);
			var entity = parent.BindChildGameObject(instance, false, install);
			return entity;
		}
		public static IEntity BindChildGameObject(this IResolver parent, GameObject instance, bool isRoot, Action<IBinder> install)
			=> InstallerUtils.CreateGoEntity(instance, parent, install, isRoot);
		public static IEntity CreateChild(this IResolver parent, string name, Action<IBinder> install)
			=> InstallerUtils.CreateEntity(name, parent, install);
		public static IResolver CreateChildResolver(this IResolver parent, string name, Action<IBinder> install)
			=> InstallerUtils.CreateChildResolver(name, parent, install);
	}
}