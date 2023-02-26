using System;
using UnityEngine;

namespace BB
{
	public sealed class EntityColliderEvents : MonoBehaviour
	{
		public event Action<Collision> OnCollisionEnter;
		private void Awake()
		{
			foreach (var collider in GetComponentsInChildren<EntityCollider>())
				collider.OnEnter = OnEnter;
		}
		void OnEnter(Collision collision) => OnCollisionEnter?.Invoke(collision);
	}
}