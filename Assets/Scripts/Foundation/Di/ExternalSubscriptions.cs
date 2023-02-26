using MessagePipe;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BB
{
	public sealed record ExternalSubscriptions : EntitySystem, IOnDespawn
	{
		List<IDisposable> _subscriptions = new();
		public void OnDespawn()
		{
			foreach (var subscription in _subscriptions)
				subscription.Dispose();
			_subscriptions.Clear();
		}

		public new void Subscribe<T>(Action<T> action)
		{
			if (Resolver.TryResolve(out ISubscriber<T> subscriber))
				_subscriptions.Add(subscriber.Subscribe(action));
			else Debug.LogError($"{Entity.Name} does not receive {typeof(T).Name} events. " +
				$"External subscription will be ignored.");
		}
	}
	
}