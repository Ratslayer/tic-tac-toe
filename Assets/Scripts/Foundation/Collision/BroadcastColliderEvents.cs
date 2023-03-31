using MessagePipe;
using System;
using UnityEngine;

namespace BB
{
	public sealed record CollisionEntered(IEntity Entity, Collision Collision)
	{
		public Collider Collider => Collision.collider;
		public Vector3 Point => Collision.contacts[0].point;
		public Vector3 Normal => Collision.contacts[0].normal;
	}
	public sealed record BroadcastColliderEvents(
		EntityColliderEvents Events,
		IPublisher<CollisionEntered> Entered)
		: EntitySystem, IDisposable, IOnInstall
	{
		public void OnStart()
		{
			Events.OnCollisionEnter += OnEnter;
		}
		public void Dispose()
		{
			Events.OnCollisionEnter -= OnEnter;
		}
		void OnEnter(Collision collision)
		{
			var behaviour = collision.gameObject.GetComponentInParent<EntityBehaviour>();
			Entered.Publish(new(behaviour, collision));
		}
	}
	public static class CollisionInstallUtils
	{
		public static void Collision(this IBinder binder)
		{
			binder.AddComponent<EntityColliderEvents>();
			binder.Event<CollisionEntered>();
			binder.System<BroadcastColliderEvents>();
		}
	}
}