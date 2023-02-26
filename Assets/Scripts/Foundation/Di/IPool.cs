using System;
using System.Collections.Generic;
using UnityEngine;

namespace BB
{
	public interface IPool
	{
		bool HasUnspawnedEntity(out IEntity entity);
		void AddCreatedEntity(IEntity entity);
		int NumInstances { get; }
	}
	public abstract record AbstractPool : IPool
	{
		readonly List<IEntity> _instances = new();
		public int NumInstances => _instances.Count;
		public bool HasUnspawnedEntity(out IEntity entity)
			=> _instances.TryFind(out entity, e => !e.IsSpawned);
		public virtual void AddCreatedEntity(IEntity entity)
		{
			_instances.Add(entity);
			entity.Append<RemoveFromPoolOnDispose>(this);
		}
		sealed record RemoveFromPoolOnDispose(AbstractPool Pool)
			: EntitySystem, IDisposable
		{
			public void Dispose()
			{
				Pool._instances.Remove(Entity);
			}
		}
	}
	public sealed record Pool : AbstractPool;
	public sealed record GoPool(GameObject Root) : AbstractPool
	{
		public override void AddCreatedEntity(IEntity entity)
		{
			entity.Append<ReparentToPoolOnDespawn>(Root);
			base.AddCreatedEntity(entity);
		}
		sealed record ReparentToPoolOnDespawn(GameObject Root) : EntitySystem, IOnDespawn
		{
			public void OnDespawn()
			{
				if (Entity.HasGameObject(out var go))
					go.SetParent(Root);
			}
		}
	}
}