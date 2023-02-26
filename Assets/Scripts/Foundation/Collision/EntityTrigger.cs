using System;
using System.Collections.Generic;
using UnityEngine;

namespace BB
{
	public sealed class EntityTrigger : MonoBehaviour
	{
		public Action<Collider> OnEnter, OnExit;
		readonly List<Collider> _colliders = new();
		private void OnTriggerEnter(Collider other)
		{
			_colliders.Add(other);
			if (isActiveAndEnabled)
				Enter(other);
		}
		private void OnTriggerExit(Collider other)
		{
			_colliders.Remove(other);
			if (isActiveAndEnabled)
				Exit(other);
		}
		private void OnEnable()
		{
			foreach (var collider in _colliders)
				Enter(collider);
		}
		private void OnDisable()
		{
			foreach (var collider in _colliders)
				Exit(collider);
		}
		void Enter(Collider other) => OnEnter?.Invoke(other);
		void Exit(Collider other) => OnExit?.Invoke(other);
	}
}