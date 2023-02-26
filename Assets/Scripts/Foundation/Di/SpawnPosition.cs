using System.Collections.Generic;
using UnityEngine;

namespace BB
{
	public readonly struct SpawnPosition
	{
		public readonly Transform _parent;
		public readonly Vector3 _position, _scale;
		public readonly Quaternion _rotation;
		public Vector3 Forward => _rotation * Vector3.forward;
		public SpawnPosition(
			Transform parent,
			Vector3 position,
			Quaternion rotation,
			Vector3 scale)
		{
			_parent = parent;
			_position = position;
			_rotation = rotation;
			_scale = scale;
		}
		public SpawnPosition(Transform parent, Vector3 position, Quaternion rotation, float scale)
			: this(parent, position, rotation, Vector3.one * scale) { }
		public SpawnPosition(Transform parent, Vector3 position, Quaternion rotation)
			: this(parent, position, rotation, Vector3.one) { }
		public SpawnPosition(Transform parent)
			: this(parent, Vector3.zero, Quaternion.identity, Vector3.one) { }
		public SpawnPosition(Transform parent, Transform point)
			: this(parent, point.position, point.rotation, Vector3.one) { }
		public static SpawnPosition Local(Transform parent, Vector3 pos, Quaternion rot)
			=> new(parent, pos + parent.position, rot * parent.rotation);
	}
	public static class SpawnPositionExtensions
	{
		public static GameObject Instantiate(this GameObject go, SpawnPosition position)
			=> GameObject.Instantiate(go, position._position, position._rotation, position._parent);
		public static void Apply(this GameObject go, SpawnPosition position)
		{
			go.transform.SetParent(position._parent, false);
			go.transform.SetPositionAndRotation(position._position, position._rotation);
			go.transform.localScale = go.transform.localScale.Mul(position._scale);
		}
	}
}