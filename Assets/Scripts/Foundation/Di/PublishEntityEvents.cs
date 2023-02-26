using MessagePipe;

namespace BB
{
	public sealed record PublishEntityEvents(
		EntityEvents Events,
		DeltaTime Delta,
		IPublisher<UpdateEvent> OnUpdate,
		IPublisher<FixedUpdateEvent> OnFixedUpdate,
		IPublisher<LateUpdateEvent> OnLateUpdate)
		: EntitySystem, IOnSpawn, IOnDespawn
	{
		public void OnSpawn()
		{
			Events.OnUpdate += Update;
			Events.OnFixedUpdate += FixedUpdate;
			Events.OnLateUpdate += LateUpdate;
			Events.Destroyed += OnDestroy;
		}
		public void OnDespawn()
		{
			Events.OnUpdate -= Update;
			Events.OnFixedUpdate -= FixedUpdate;
			Events.OnLateUpdate -= LateUpdate;
			Events.Destroyed -= OnDestroy;

		}
		void OnDestroy() => Entity.Dispose();
		void Publish<T>(IPublisher<T> publisher, float delta, float unscaled)
			where T : new()
		{
			Delta._scaled = delta;
			Delta._unscaled = unscaled;
			publisher.Publish(new());
		}
		void Update(float delta, float unscaled)
			=> Publish(OnUpdate, delta, unscaled);
		void FixedUpdate(float delta, float unscaled)
			=> Publish(OnFixedUpdate, delta, unscaled);
		void LateUpdate(float delta, float unscaled)
			=> Publish(OnLateUpdate, delta, unscaled);
	}
	public struct UpdateEvent { }
	public struct FixedUpdateEvent { }
	public struct LateUpdateEvent { }
}