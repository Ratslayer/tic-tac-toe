using System;
using UnityEngine;

namespace BB
{
	public sealed class EntityEvents : MonoBehaviour 
	{
		public delegate void UpdateDelegate(float deltaTime, float unscaledDeltaTime);
		public event UpdateDelegate OnUpdate, OnFixedUpdate, OnLateUpdate;
		public event Action Destroyed;
		void Update()
		{
			OnUpdate?.Invoke(Time.deltaTime, Time.unscaledDeltaTime);
		}
		void FixedUpdate()
		{
			OnFixedUpdate?.Invoke(Time.fixedDeltaTime, Time.fixedUnscaledDeltaTime);
		}
		void LateUpdate()
		{
			OnLateUpdate?.Invoke(Time.deltaTime, Time.unscaledDeltaTime);
		}
		private void OnDestroy()
		{
			Destroyed?.Invoke();
		}
	}
}