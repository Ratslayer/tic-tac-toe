using UnityEngine;
using System.Collections.Generic;

namespace BB
{
	public sealed class EntityBehaviour : MonoBehaviour
	{
		public IEnumerable<IInstaller> GetInstallers() => GetComponents<IInstaller>();
		public IEntity Entity { get; set; }
		private void Start()
		{
			Install();
		}
		public void Install()
		{
			if (Entity != null)
				return;
			//if this is reached, it means that this object is not pooled
			var parentRoot = transform.parent ? transform.parent.GetComponentInParent<EntityBehaviour>() : default;
			IResolver parentResolver = default;
			if (parentRoot)
			{
				parentRoot.Install();
				parentResolver = parentRoot.Entity.Resolver;
			}
			InstallerUtils.CreateGoEntity(gameObject, parentResolver, null, true);
			InitChildren();
			Entity.Append<DestroyOnDespawn>();
			Entity.Spawn();
		}
		public void InitChildren()
		{
			foreach (var comp in GetComponentsInChildren<IEntityComponent>(true))
				comp.Entity = Entity;
		}
		sealed record DestroyOnDespawn(EntityTransform Transform)
			: EntitySystem, IOnDespawn
		{
			public void OnDespawn()
			{
				Destroy(Transform.Value.gameObject);
			}
		}
	}
	public static class ComponentUtils
	{
		public static bool TryGetComponentInParent<T>(this GameObject obj, out T comp)
		{
			comp = obj.GetComponentInParent<T>();
			return comp != null;
		}
		public static bool HasEntity(this GameObject obj, out IEntity entity)
		{
			if (obj && obj.TryGetComponentInParent(out EntityBehaviour eb))
			{
				entity = eb.Entity;
				return true;
			}
			entity = default;
			return false;
		}
		public static bool HasEntity(this Component comp, out IEntity entity)
			=> HasEntity(comp ? comp.gameObject : default, out entity);
		public static TComponent GetInstallerRootComponent<TComponent>(this MonoBehaviour b)
		{
			var root = b.GetComponentInParent<EntityBehaviour>();
			return root ? root.GetComponent<TComponent>() : default;
		}
	}
	public interface IEntityComponent
	{
		IEntity Entity { get; set; }
	}
}