using System;
using UnityEngine;
using Zenject;

namespace BB
{
	public abstract record EntitySystem : AbstractSystem
	{
		IResolver _resolver;
		IEntity _entity;
		protected IResolver Resolver => _resolver;
		public IEntity Entity => _entity;
		[Inject]
		private void Construct(IEntity entity, IResolver resolver)
		{
			_entity = entity;
			_resolver = resolver;
			OnInstall();
		}
		protected EventSubscription<T> Subscribe<T>(Action<T> action)
		{
			var result = new EventSubscription<T>(action, Resolver, GetType());
			AddSubscription(result);
			return result;
		}
		protected void AddSubscription(ISubscription subscription)
			=> Entity.State.AddSubscription(subscription);
		protected EventSubscription<T> Subscribe<T>(Action action)
			=> Subscribe<T>(t => action());
		protected void Publish<T>(T msg) => Entity.Resolver.Publish(msg);
		void OnInstall()
		{
			//if (this is IOnInstall start)
			//	Entity.CallAtInit(start.OnStart);
			SubscribeAction<IOnInstall, StartEvent>(l => l.OnStart());
			SubscribeAction<IOnSpawn, SpawnEvent>(l => l.OnSpawn());
			SubscribeAction<IOnDespawn, DespawnEvent>(l => l.OnDespawn());
			SubscribeAction<IDisposable, DisposeEvent>(l => l.Dispose());
			SubscribeToTick<IOnUpdate, UpdateEvent>((time, tick) => tick.OnUpdate(time._scaled));
			SubscribeToTick<IOnFixedUpdate, FixedUpdateEvent>((time, tick) => tick.OnFixedUpdate(time._scaled));
			SubscribeToTick<IOnLateUpdate, LateUpdateEvent>((time, tick) => tick.OnLateUpdate(time._scaled));
			SubscribeMarkedWithAttributes();
		}
		void SubscribeAction<TContract, TEvent>(Action<TContract> action)
		{
			if (this is TContract c)
				Subscribe<TEvent>(_ => action(c));
		}
		void SubscribeToTick<TTick, TEvent>(Action<DeltaTime, TTick> action)
		{
			if (this is TTick tick
				&& Resolver.TryResolve(out DeltaTime time))
				Subscribe<TEvent>(_ => action(time, tick));
		}
		void SubscribeMarkedWithAttributes()
		{
			var selfType = GetType();
			foreach (var info
				in ReflectionUtils.GetAllMethodsWithAttribute<SubscribeAttribute>(GetType(), typeof(EntitySystem)))
			{
				var args = info.GetParameters();
				if (args.Length != 1)
				{
					Debug.LogError($"{info.Name} has {args.Length} arguments. Can't subscribe it.");
					continue;
				}
				var type = args[0].ParameterType;
				//create delegate
				var delegateType = typeof(Action<>).MakeGenericType(type);
				var action = info.CreateDelegate(delegateType, this);
				//create subscription
				var subscriptionType = typeof(EventSubscription<>).MakeGenericType(type);
				var constructor = subscriptionType.GetConstructors()[0];
				var subscription = constructor.Invoke(new object[] { action, Resolver, selfType });
				AddSubscription((ISubscription)subscription);
			}
		}
	}
	public sealed class SubscribeAttribute : Attribute { }
}