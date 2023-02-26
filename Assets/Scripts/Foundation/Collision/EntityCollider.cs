using System;
using UnityEngine;

namespace BB
{
	public sealed class EntityCollider : MonoBehaviour
	{
		public Action<Collision> OnEnter;
		private void OnCollisionEnter(Collision collision)
		{
			OnEnter?.Invoke(collision);
		}
	}
}