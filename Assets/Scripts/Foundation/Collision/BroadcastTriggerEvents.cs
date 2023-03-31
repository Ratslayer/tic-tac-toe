using MessagePipe;
using System;

namespace BB
{
	public sealed record BroadcastTriggerEvents(
		EntityTriggerEvents Events,
		IPublisher<TriggerEntered> Entered,
		IPublisher<TriggerExited> Exited)
		: EntitySystem, IDisposable,IOnInstall
	{
		public void OnStart()
		{
			Events.OnEntityEnter += OnEnter;
			Events.OnEntityLeave += OnExit;
		}
		public void Dispose()
		{
			Events.OnEntityEnter -= OnEnter;
			Events.OnEntityLeave -= OnExit;
		}
		void OnEnter(IEntity entity) => Entered.Publish(new(entity));
		void OnExit(IEntity entity) => Exited.Publish(new(entity));
		
	}
	public sealed record TriggerEntered(IEntity Entity);
	public sealed record TriggerExited(IEntity Entity);
	public static class TriggerInstallerUtils
	{
		public static void Trigger(this IBinder binder)
		{
			binder.AddComponent<EntityTriggerEvents>();
			binder.Event<TriggerEntered>();
			binder.Event<TriggerExited>();
			binder.System<BroadcastTriggerEvents>();
		}
	}
}