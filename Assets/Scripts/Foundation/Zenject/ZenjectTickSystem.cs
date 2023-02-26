using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

namespace BB
{
	public sealed class ZenjectTickSystem : ITickSystem, ITickable, IFixedTickable, ILateTickable
	{
		private readonly List<IOnUpdate> _ticked = new();
		private readonly List<IOnFixedUpdate> _fixedTicked = new();
		private readonly List<IOnLateUpdate> _lateTicked = new();
		private readonly List<ITimedActor> _newObjects = new(), _deadObjects = new();
		private bool _isTicking;
		public void FixedTick()
		{
			var deltaTime = Time.fixedDeltaTime;
			Update(_fixedTicked, t => t.OnFixedUpdate(deltaTime));
		}

		public void InvokeAfterTick(Action action)
		{
			throw new NotImplementedException();
		}

		public void LateTick()
		{
			var deltaTime = Time.deltaTime;
			Update(_lateTicked, t => t.OnLateUpdate(deltaTime));
		}
		public void Tick()
		{
			var deltaTime = Time.deltaTime;
			Update(_ticked, t => t.OnUpdate(deltaTime));
		}
		public void Register(ITimedActor obj)
		{
			if (_isTicking)
			{
				if (!_deadObjects.Remove(obj))
					_newObjects.Add(obj);
				return;
			}
			if (obj is IOnUpdate ticked)
				_ticked.Add(ticked);
		}



		public void Unregister(ITimedActor obj)
		{
			if (_isTicking)
			{
				if (!_newObjects.Remove(obj))
					_deadObjects.Add(obj);
				return;
			}
			if (obj is IOnUpdate ticked)
				_ticked.Remove(ticked);
		}
		private void Update<T>(List<T> actors, Action<T> process)
		{
			_isTicking = true;
			foreach (var actor in actors)
			{
				try
				{
					process(actor);
				}
				catch (Exception ex)
				{
					Debug.LogException(ex);
				}
			}
			_isTicking = false;
			foreach (var dead in _deadObjects)
				if (dead is T t)
					actors.Remove(t);
			foreach (var actor in _newObjects)
				if (actor is T t)
					actors.Add(t);
			_deadObjects.Clear();
			_newObjects.Clear();
		}
	}
}