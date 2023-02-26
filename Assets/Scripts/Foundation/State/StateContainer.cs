using System;
using System.Collections.Generic;
using Zenject;
namespace BB
{
	public sealed record StateContainer : EntitySystem, IOnUpdate
	{
		readonly Dictionary<object, object> _states = new();
		readonly List<IOnUpdate> _ticked = new();
		public TState GetOrCreate<TState>(IDiFactory<TState> factory)
			=> GetOrCreate(factory, () => factory.Create(Resolver));
		private TState GetOrCreate<TState>(object key, Func<TState> constructor)
		{
			if (!_states.TryGetValue(key, out var state))
			{
				state = constructor();
				if (state is IOnUpdate ticked)
					_ticked.Add(ticked);
				_states.Add(key, state);
			}
			return (TState)state;
		}
		public void OnUpdate(float deltaTime)
		{
			foreach (var ticked in _ticked)
				ticked.OnUpdate(deltaTime);
		}
	}
	public static class StateContainerExtensions
	{
		//public static IEnumerable<T> ToStates<T>(this IEnumerable<IFactory<T>> factories, StateContainer container)
		//{
		//	foreach (var factory in factories)
		//		yield return container.GetOrCreate(factory);
		//}
		public static IEnumerable<T> ToStates<T>(this IEnumerable<IDiFactory<T>> factories, StateContainer container)
		{
			foreach (var factory in factories)
				yield return container.GetOrCreate(factory);
		}
	}
}
