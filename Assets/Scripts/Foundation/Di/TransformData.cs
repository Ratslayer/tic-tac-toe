using System;
using UnityEngine;
namespace BB
{
    [Flags]
    public enum MovementType
    {
        Position = 1,
        Rotation = 2,
        Scale = 4
    }
    [Serializable]
    public struct LocalTransformData
    {
        public Vector3 _position, _scale;
        public Quaternion _rotation;
        public LocalTransformData(Transform t)
        {
            _position = t.localPosition;
            _rotation = t.localRotation;
            _scale = t.localScale;
        }
        public LocalTransformData(Vector3 pos, Quaternion rot, Vector3 scale)
        {
            _position = pos;
            _rotation = rot;
            _scale = scale;
        }
        public LocalTransformData(int _ = 0)
        {
            _position = Vector3.zero;
            _rotation = Quaternion.identity;
            _scale = Vector3.one;
        }
        public static implicit operator LocalTransformData(Transform t) => new(t);
        public void Apply(Transform t)
        {
            t.localPosition = _position;
            t.localRotation = _rotation;
            t.localScale = _scale;
        }
        public void Apply(Transform t, MovementType type)
        {
            if (type.HasEnumFlag(MovementType.Position))
                t.localPosition = _position;
            if (type.HasEnumFlag(MovementType.Rotation))
                t.localRotation = _rotation;
            if (type.HasEnumFlag(MovementType.Scale))
                t.localScale = _scale;
        }
    }
    [Serializable]
    public struct TransformData
    {
        public Vector3 Position;
        public Quaternion Rotation;
        public TransformData(Vector3 pos, Quaternion rot)
        {
            Position = pos;
            Rotation = rot;
        }
        public void GetLocalData(Transform t)
        {
            Position = t.localPosition;
            Rotation = t.localRotation;
        }
        public void SetLocalData(Transform t)
        {
            t.localPosition = Position;
            t.localRotation = Rotation;
        }
        public void LerpLocalData(Transform t, float value)
        {
            t.localPosition = Vector3.Lerp(t.localPosition, Position, value);
            t.localRotation = Quaternion.Slerp(t.localRotation, Rotation, value);
        }
        public static TransformData ToLocalData(Transform t)
        {
            var result = new TransformData();
            result.GetLocalData(t);
            return result;
        }
        public void GetWorldData(Transform t)
        {
            Position = t.position;
            Rotation = t.rotation;
        }
        public void SetWorldData(Transform t)
        {
            t.position = Position;
            t.rotation = Rotation;
        }
        public void LerpWorldData(Transform t, float value)
        {
            t.position = Vector3.Lerp(t.position, Position, value);
            t.rotation = Quaternion.Slerp(t.rotation, Rotation, value);
        }
        public static TransformData ToWorldData(Transform t)
        {
            var result = new TransformData();
            result.GetWorldData(t);
            return result;
        }
        public TransformData ApplyParent(TransformData parent)
        {
            var rotation = parent.Rotation * Rotation;
            var position = parent.Position + rotation * Position;
            return new TransformData(position, rotation);
        }
        public TransformData ApplyParent(Transform parent) => ApplyParent(parent.ToWorldData());
        public Vector3 Transform(Vector3 point)
        {
            var result = Position + Rotation * point;
            return result;
        }
        public Vector3 Transform(Vector3 point, Transform parent) => parent.TransformPoint(Transform(point));
        public Vector3 InverseTransform(Vector3 point)
        {
            var result = Rotation.Inverse() * (point - Position);
            return result;
        }
        public Vector3 InverseTransform(Vector3 point, Transform parent) => InverseTransform(parent.InverseTransformPoint(point));
        public static TransformData GetWorldParentToAlignChild(Transform parent, Transform child, Vector3 position, Quaternion rotation)
        {
            var localPos = parent.InverseTransformPoint(child.position);
            var localRot = parent.rotation.Inverse() * child.rotation;
            var worldRot = localRot.Inverse() * rotation;
            var worldPos = position - worldRot * localPos;
            return new TransformData(worldPos, worldRot);
        }
    }
}