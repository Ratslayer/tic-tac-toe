//using System;
//using System.Collections.Generic;
//using System.Linq;
//using Zenject;

//namespace BB
//{
//	public interface IState
//	{
//		void SetActive(bool active);
//	}
//	public interface IStateStack<TState> where TState : IState
//	{
//		TState State { get; }
//		void Enter(IFactory<TState> factory);
//		void Exit(IFactory<TState> factory);
//		void ExitCurrent();
//	}
//	public sealed class StateStack<TState> : IStateStack<TState> where TState : IState
//	{
//		[Inject]
//		private IStateContainer<TState> _container;
//		private TState _state;
//		private IFactory<TState> _factory;
//		private readonly List<IFactory<TState>> _stack = new();
//		public TState State => _state;

//		private TState GetState(IFactory<TState> factory) => factory != null ? _container.GetOrCreate(factory) : default;
//		public void Enter(IFactory<TState> factory)
//		{
//			if (_factory == factory)
//				return;
//			//rewind if state is already on stack
//			for (int i = 0; i < _stack.Count; i++)
//				if (_stack[i] == factory)
//				{
//					_stack.RemoveRange(i, _stack.Count - i);
//					break;
//				}
//			_stack.Add(factory);
//			var oldState = _state;
//			_state = GetState(factory);
//			_factory = factory;
//			OnStateChange(oldState, _state);
//		}
//		public void Exit(IFactory<TState> factory)
//		{
//			_stack.Remove(factory);
//			if (_factory != factory)
//				return;
//			_factory = _stack.LastOrDefault();
//			var oldState = _state;
//			_state = GetState(_factory);
//			OnStateChange(oldState, _state);
//		}
//		private void OnStateChange(TState oldState, TState newState)
//		{
//			oldState?.SetActive(false);
//			newState?.SetActive(true);
//		}

//		public void ExitCurrent() => Exit(_stack.LastOrDefault());
//	}
//}