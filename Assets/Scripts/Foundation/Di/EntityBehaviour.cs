using UnityEngine;
using System.Collections.Generic;
using System;

namespace BB
{
	public sealed class EntityBehaviour : MonoBehaviour, IEntity
	{
		public IEnumerable<IInstaller> GetInstallers() => GetComponents<IInstaller>();
		readonly List<EntityBehaviour> _children = new List<EntityBehaviour>();

		public IResolver Resolver { get; private set; }
		public EntityState State { get; private set; }
		public bool IsPooled { get; private set; } = true;
		public bool Installed => Resolver != null;
		private void Start()
		{
			Install();
		}
		void Install()
		{
			if (Resolver != null)
			{
				this.Append<DespawnChildrenOnDespawn>();
				return;
			}
			//if this is reached, it means that this object is not pooled
			IsPooled = false;
			var parentRoot = transform.parent ? transform.parent.GetComponentInParent<EntityBehaviour>() : default;
			IResolver parentResolver = default;
			if (parentRoot)
			{
				parentRoot.Install();
				parentResolver = parentRoot.Resolver;
			}
			InstallerUtils.CreateGoEntity(gameObject, parentResolver, null, true);
			this.Append<DestroyOnDespawn>();
			this.Spawn();
		}
		public void Install(IBinder binder, bool root)
		{
			Resolver = binder;
			State = Resolver.Resolve<EntityState>();
			
			foreach (var installer in GetComponentsInChildren<AbstractInstallerBehaviour>())
			{
				if (installer.GetComponent<EntityBehaviour>().Installed)
					continue;
				var entity = InstallerUtils.CreateGoEntity(installer.gameObject, binder, null, false);
				_children.Add(entity);
			}
			//foreach (var comp in GetComponentsInChildren<IEntityComponent>(true))
			//	comp.Entity = Entity;
		}
		void InvokeForAll(Action<EntityBehaviour> action)
		{
			action(this);
			foreach (var child in _children)
				action(child);
		}
		public void Spawn()
		{
			State.Spawn();
			foreach (var child in _children)
				child.Spawn();
		}

		public void Despawn() => InvokeForAll(e => e.State.Despawn());
		public void Dispose() => InvokeForAll(e => e.State.Dispose());
		static void DespawnChildren(EntityTransform t)
		{
			foreach (var child in t.Value.GetComponentsInChildren<EntityBehaviour>())
				if (child.IsPooled)
					child.Despawn();
		}
		sealed record DespawnChildrenOnDespawn(EntityTransform Transform)
			: EntitySystem, IOnDespawn
		{
			public void OnDespawn()
			{
				DespawnChildren(Transform);
			}
		}
		sealed record DestroyOnDespawn(EntityTransform Transform)
			: EntitySystem, IOnDespawn
		{
			public void OnDespawn()
			{
				DespawnChildren(Transform);
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
				entity = eb;
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