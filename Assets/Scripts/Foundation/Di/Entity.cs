using MessagePipe;
using System;
using System.Collections.Generic;
using Zenject;

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
		bool IsSpawned { get; }
		bool IsDestroyed { get; }
		void Spawn();
		void Despawn();
		void AddSubscription(ISubscription subscription);
		void CallAtInit(Action action);
	}
	//queue for injection does not work with records
	//this has to be a class
	public sealed class Entity : IEntity
	{
		public IResolver Resolver { get; private set; }
		readonly List<ISubscription> _subscriptions = new();
		readonly List<Action> _initActions = new();
		//IPublisher<StartEvent> _started;
		IPublisher<SpawnEvent> _spawned;
		IPublisher<DespawnEvent> _despawned;
		IPublisher<DisposeEvent> _disposed;

		public bool IsSpawned { get; private set; }
		public bool IsDestroyed { get; private set; }
		bool _initialized = false;
		public Entity(IResolver resolver)
		{
			Resolver = resolver;
		}
		[Inject]
		void Construct(
			//IPublisher<StartEvent> started,
			IPublisher<SpawnEvent> spawned,
			IPublisher<DespawnEvent> despawned,
			IPublisher<DisposeEvent> disposed)
		{
			//_started = started;
			_spawned = spawned;
			_despawned = despawned;
			_disposed = disposed;
		}
		public void CallAtInit(Action action)
		{
			if (_initialized)
				action();
			else _initActions.Add(action);
		}
		public void Spawn()
		{
			if (IsSpawned)
				return;
			IsSpawned = true;
			InvokeAfterInstall(SpawnMethod);
			void SpawnMethod()
			{
				//call init methods
				if (!_initialized)
				{
					_initialized = true;
					//systems can append on start, so cycle until the list is empty
					//_initActions.ProcessAndClearAllElements(a => a.Invoke());
					foreach (var action in _initActions)
						action();
					foreach (var subscription in _subscriptions)
						subscription.Init();
				}
				//subscribe
				foreach (var subscription in _subscriptions)
					subscription.Subscribe();
				//spawn
				_spawned.Publish(default);
			}
		}
		public void Despawn()
		{
			DespawnInternal();
			UnsubscribeAll();
		}
		void DespawnInternal()
		{
			if (!IsSpawned)
				return;
			IsSpawned = false;
			_despawned.Publish(default);
		}
		void UnsubscribeAll()
		{
			foreach (var subscription in _subscriptions)
				subscription.Unsubscribe();
		}
		public void Dispose()
		{
			IsDestroyed = true;
			DespawnInternal();
			_disposed.Publish(default);
			UnsubscribeAll();
		}

		public void AddSubscription(ISubscription subscription)
			=> _subscriptions.Add(subscription);
		void InvokeAfterInstall(Action action)
		{
			if (Resolver.Parent != null)
				Resolver.Parent.InvokeAfterInstall(action);
			else action();
		}
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
	public interface IOnStart
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