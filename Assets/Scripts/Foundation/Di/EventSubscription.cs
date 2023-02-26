using MessagePipe;
using System;

namespace BB
{
	public interface ISubscription
	{
		void Init();
		void Subscribe();
		void Unsubscribe();
	}
	public sealed record EventSubscription<T>(Action<T> Action, IResolver Resolver, Type SystemType) : ISubscription
	{
		ISubscriber<T> _subscriber;
		IDisposable _disposable;
		public void Init() => _subscriber = Resolver.Resolve<ISubscriber<T>>();

		public void Unsubscribe()
		{
			if (_disposable != null)
			{
				_disposable.Dispose();
				_disposable = null;
			}
		}
		public void Subscribe()
		{
			if (_disposable != null)
				return;
			if (_subscriber != null)
				_disposable = _subscriber.Subscribe(Action);
			else Log.Error($"Error at {Resolver.Name}/{SystemType.Name}: subscriber of type {typeof(T).Name} not initialized.");
		}
	}
}