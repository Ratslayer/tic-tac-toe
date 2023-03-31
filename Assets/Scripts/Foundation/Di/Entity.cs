using MessagePipe;
using System;
using System.Collections.Generic;

namespace BB
{
	public sealed class DeltaTime
	{
		public float _scaled;
		public float _unscaled;
	}
	public interface IEntity : IDisposable
	{
		string Name { get => Resolver.Name; set => Resolver.Name = value; }
		IResolver Resolver { get; }
		EntityState State { get; }
		bool IsSpawned => State.CurrentState == EntityState.State.Spawned;
		bool IsDestroyed => State.CurrentState == EntityState.State.Disposed;
		void Spawn();
		void Despawn();
		//void AddSubscription(ISubscription subscription);
		//void CallAtInit(Action action);
	}
	public sealed record EntityState(
		IPublisher<SpawnEvent> _spawned,
		IPublisher<DespawnEvent> _despawned,
		IPublisher<DisposeEvent> _disposed,
		IPublisher<StartEvent> Started)
	{
		public enum State
		{
			Uninitialized,
			Spawned,
			Despawned,
			Disposed
		}
		readonly List<ISubscription> _subscriptions = new();
		readonly List<Action> _initActions = new();
		//readonly IPublisher<SpawnEvent> _spawned;
		//readonly IPublisher<DespawnEvent> _despawned;
		//readonly IPublisher<DisposeEvent> _disposed;
		//readonly IBinder _binder;
		//readonly Action<IBinder> _install;
		public State CurrentState { get; private set; } = State.Uninitialized;
		public IResolver Resolver { get; private set; }
		//public EntityState(IBinder binder, Action<IBinder> install)
		//{
		//	_binder = binder;
		//	_install = install;
		//	CurrentState 
		//	Init(ref _spawned);
		//	Init(ref _despawned);
		//	Init(ref _disposed);
		//	void Init<T>(ref IPublisher<T> publisher) => publisher = binder.Resolve<IPublisher<T>>();
		//}
		public void Spawn()
		{
			if (CurrentState == State.Spawned)
				return;
			//init if needed
			if (CurrentState == State.Uninitialized)
			{
				foreach (var action in _initActions)
					action();
				foreach (var subscription in _subscriptions)
					subscription.Init();
				SubscribeAll();
				Started.Publish();
			}
			else SubscribeAll();
			//spawn
			CurrentState = State.Spawned;
			_spawned.Publish(default);
		}
		public void Despawn()
		{
			if (CurrentState == State.Despawned)
				return;
			CurrentState = State.Despawned;
			_despawned.Publish();
			UnsubscribeAll();
		}
		public void Dispose()
		{
			if (CurrentState == State.Disposed)
				return;
			CurrentState = State.Disposed;
			_despawned.Publish();
			_disposed.Publish();
			UnsubscribeAll();
		}
		void SubscribeAll()
		{
			foreach (var subscription in _subscriptions)
				subscription.Subscribe();
		}
		void UnsubscribeAll()
		{
			foreach (var subscription in _subscriptions)
				subscription.Unsubscribe();
		}
		public void AddSubscription(ISubscription sub)
		{
			_subscriptions.Add(sub);
			//resolve subscription if this has been appended after installation
			if (CurrentState != State.Uninitialized)
				sub.Init();
			//subscribe if entity has already spawned
			if (CurrentState == State.Spawned)
				sub.Subscribe();
		}
	}
	//queue for injection does not work with records
	//this has to be a class
	public sealed class Entity : IEntity
	{
		public IResolver Resolver { get; private set; }

		public EntityState State { get; private set; }

		//public bool IsSpawned { get; private set; }
		//public bool IsDestroyed { get; private set; }
		//bool _initialized = false;
		public Entity(IResolver resolver, EntityState state)
		{
			Resolver = resolver;
			State = state;
		}
		//public void CallAtInit(Action action)
		//{
		//	if (_initialized)
		//		action();
		//	else _initActions.Add(action);
		//}
		public void Spawn() => State.Spawn();
		//{
		//	if (IsSpawned)
		//		return;
		//	IsSpawned = true;
		//	InvokeAfterInstall(SpawnMethod);
		//	void SpawnMethod()
		//	{
		//		//call init methods
		//		if (!_initialized)
		//		{
		//			_initialized = true;
		//			//systems can append on start, so cycle until the list is empty
		//			//_initActions.ProcessAndClearAllElements(a => a.Invoke());
		//			foreach (var action in _initActions)
		//				action();
		//			foreach (var subscription in _subscriptions)
		//				subscription.Init();
		//		}
		//		//subscribe
		//		foreach (var subscription in _subscriptions)
		//			subscription.Subscribe();
		//		//spawn
		//		_spawned.Publish(default);
		//	}
		//}
		public void Despawn() => State.Despawn();
		//{
		//	DespawnInternal();
		//	UnsubscribeAll();
		//}
		//void DespawnInternal()
		//{
		//	if (!IsSpawned)
		//		return;
		//	IsSpawned = false;
		//	_despawned.Publish(default);
		//}
		//void UnsubscribeAll()
		//{
		//	foreach (var subscription in _subscriptions)
		//		subscription.Unsubscribe();
		//}
		public void Dispose() => State.Dispose();
		//{
		//	IsDestroyed = true;
		//	DespawnInternal();
		//	_disposed.Publish(default);
		//	UnsubscribeAll();
		//}

		//public void AddSubscription(ISubscription subscription)
		//	=> _subscriptions.Add(subscription);
		//void InvokeAfterInstall(Action action)
		//{
		//	if (Resolver.Parent != null)
		//		Resolver.Parent.InvokeAfterInstall(action);
		//	else action();
		//}
		public override string ToString() => Resolver.GetPath();


	}
	public interface IOnShow
	{
		void OnShow();
	}
	public interface IOnHide
	{
		void OnHide();
	}
	public interface IOnInstall
	{
		void OnStart();
	}
	public interface IOnSpawn
	{
		void OnSpawn();
	}
	public interface IOnDespawn
	{
		void OnDespawn();
	}
}