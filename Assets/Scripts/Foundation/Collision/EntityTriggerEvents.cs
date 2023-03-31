using System;
using System.Collections.Generic;
using UnityEngine;

namespace BB
{
	public sealed class EntityTriggerEvents : MonoBehaviour
	{
		readonly List<IEntity> _entities = new();
		public event Action<IEntity> OnEntityEnter, OnEntityLeave;
		private void Awake()
		{
			foreach (var trigger in GetComponentsInChildren<EntityTrigger>())
			{
				trigger.OnEnter = Enter;
				trigger.OnExit = Exit;
			}
		}
		void Enter(Collider collider)
		{
			if (collider.TryGetComponentInParent(out EntityBehaviour entity))
			{
				if (!_entities.Contains(entity))
					OnEntityEnter?.Invoke(entity);
				_entities.Add(entity);
			}
		}
		void Exit(Collider collider)
		{
			if (collider && collider.TryGetComponentInParent(out EntityBehaviour entity))
			{
				_entities.Remove(entity);
				if (!_entities.Contains(entity))
					OnEntityEnter?.Invoke(entity);
			}
		}
	}
}