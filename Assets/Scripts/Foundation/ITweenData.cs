using Cinemachine;
using DG.Tweening;
using DG.Tweening.Core;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static BB.TweenUtils;
namespace BB
{
    public interface ITweenData
    {
        Tween Apply(Tween tween);
        Sequence Apply(Sequence sequence);
        float Duration { get; }
    }
    [Serializable]
    public class CurveData : NullIsFalse, ITweenData
    {
        [HideLabel]
        public AnimationCurve _curve;
        public float _duration;
        public float Duration => _duration;
        public Tween Apply(Tween tween) => tween.SetEase(_curve);
        public Sequence Apply(Sequence sequence) => sequence.SetEase(_curve);
    }
    [Serializable, InlineProperty]
    public class TweenData : NullIsFalse, ITweenData
    {
        [HorizontalGroup, HideLabel]
        public Ease _ease;
        [HorizontalGroup(35), HideLabel]
        public float _duration = 1f;
        public TweenData()
        {
            _duration = 1f;
        }
        public TweenData(Ease ease, float duration)
        {
            _ease = ease;
            _duration = duration;
        }

        public float Duration => _duration;
        public Tween Apply(Tween tween) => tween.SetEase(_ease);
        public Sequence Apply(Sequence sequence) => sequence.SetEase(_ease);
    }
    public static class TweenUtils
    {
        public static Tweener Blend(TweenData data, float from, float to, DOSetter<float> setter) => DOTween.To(setter, from, to, data._duration).SetEase(data._ease);
        public static void KillConflicts(object target)
        {
            //var numKilled = DOTween.Kill(target);
            //if (numKilled > 0)
            //    Debug.Log($"Killed {numKilled} tweens: " + target.ToString());
        }
    }
    public static class TweenExtensionMethods
    {
        private static Tween Apply(this Tween tween, ITweenData data) => data.Apply(tween);
        private static Sequence Apply(this Sequence sequence, ITweenData data) => data.Apply(sequence);
        //public static Tween Alpha(this CanvasGroup group, float value)
        //{
        //    KillConflicts(group);
        //    return Alpha(group, value, UI.UIData.Asset._blendAlphaCurve);
        //}
        public static Tween Alpha(this CanvasGroup group, float value, TweenData curve)
        {
            KillConflicts(group);
            return group.DOFade(value, curve._duration).Apply(curve).SetUpdate(true);
        }
		public static Tween Alpha(this TextMeshPro text, float value, TweenData curve)
		{
			KillConflicts(text);
			return DOTween.ToAlpha(() => text.color, c => text.color = c, value, curve.Duration).Apply(curve);
		}
        public static Tween Move(this Transform t, Vector3 position, TweenData data)
        {
            KillConflicts(t);
            return t.DOMove(position, data.Duration).Apply(data);
        }
        public static Tween MoveY(this Transform t, float height, TweenData data)
        {
            KillConflicts(t);
            return t.DOMoveY(height, data.Duration).Apply(data);
        }
        public static Tween MoveHorizontal(this Transform t, Vector3 position, TweenData data)
        {
            KillConflicts(t);
            var result = DOTween.Sequence(t).Apply(data);
            result.Join(t.DOMoveX(position.x, data.Duration));
            result.Join(t.DOMoveZ(position.z, data.Duration));
            return result;
        }
        public static Tween MoveHVSplit(this Transform t, Vector3 position, float duration, Ease horizontalEase, Ease verticalEase)
        {
            KillConflicts(t);
            var result = DOTween.Sequence(t);
            result.Join(t.DOMoveX(position.x, duration).SetEase(horizontalEase));
            result.Join(t.DOMoveY(position.y, duration).SetEase(verticalEase));
            result.Join(t.DOMoveZ(position.z, duration).SetEase(horizontalEase));
            return result;
        }
        public static Tween LocalMove(this Transform t, Vector3 position, TweenData data)
        {
            KillConflicts(t);
            return t.DOLocalMove(position, data.Duration).Apply(data);
        }
        public static Tween LocalTransform(this Transform t, LocalTransformData data, TweenData curve)
        {
            KillConflicts(t);
            var result = DOTween.Sequence().Apply(curve);
            result.Join(t.LocalMove(data._position, curve));
            result.Join(t.LocalRotate(data._rotation, curve));
            return result;
        }
        public static Tween Rotate(this Transform t, Vector3 rotation, TweenData data)
        {
            KillConflicts(t);
            return t.DORotate(rotation, data.Duration).Apply(data);
        }
        public static Tween LocalRotate(this Transform t, Vector3 rotation, TweenData data)
        {
            KillConflicts(t);
            return t.DOLocalRotate(rotation, data.Duration).Apply(data);
        }
        public static Tween LocalRotate(this Transform t, Quaternion rotation, TweenData data)
        {
            KillConflicts(t);
            return t.DOLocalRotateQuaternion(rotation, data.Duration).Apply(data);
        }
        public static Tween FaceHorizontal(this Transform t, float angle, TweenData data) => Rotate(t, angle * Vector3.up, data);
        public static Tween FaceHorizontal(this Transform t, Vector3 dir, TweenData data)
        {
            var angle = dir.sqrMagnitude > 0f ? Vector3.SignedAngle(Vector3.forward, dir, Vector3.up) : t.eulerAngles.y;
            return t.FaceHorizontal(angle, data);
        }
        public static Tween FOV(this CinemachineVirtualCamera camera, float fov, TweenData data)
        {
            KillConflicts(camera);
            return DOTween.To(SetFov, camera.m_Lens.FieldOfView, fov, data._duration).SetEase(data._ease);
            void SetFov(float value)
            {
                var lens = camera.m_Lens;
                lens.FieldOfView = value;
                camera.m_Lens = lens;
            }
        }
        public static void SafeKill(this Tween tween)
        {
            if (tween != null)
                tween.Kill();
        }
        public static Tween Time(this Tween tween, TimeType time)
        {
            if (time == TimeType.Unscaled)
                tween.SetUpdate(true);
            return tween;
        }
    }
    namespace Tweens
    {

        public static class TransformTweenUtils
        {
            public static MovementType AllMovements => MovementType.Position | MovementType.Rotation | MovementType.Scale;
            public static Sequence Append(this Sequence seq, List<Tween> tweens)
            {
                if (tweens.Count > 0)
                {
                    seq.Append(tweens[0]);
                    for (int i = 1; i < tweens.Count; i++)
                        seq.Join(tweens[i]);
                }
                return seq;
            }
            public static List<Tween> CreateTweens<T>(MovementType flags, Func<T> position, Func<T> rotation, Func<T> scale) where T : Tween
            {
                var result = new List<Tween>();
                Add(MovementType.Position, position);
                Add(MovementType.Rotation, rotation);
                Add(MovementType.Scale, scale);
                return result;
                void Add(MovementType type, Func<Tween> getter)
                {
                    if (getter != null && flags.HasEnumFlag(type))
                        result.Add(getter());
                }
            }
            public static Tween JoinTweens(List<Tween> tweens, TweenData data)
            {
                var result = DOTween.Sequence();
                result.Append(tweens);
                foreach (var tween in tweens)
                    tween.SetEase(data._ease);
                return result;
            }
            public static Sequence CreateSequence(TweenData data, Transform target) => DOTween.Sequence(target).SetEase(data._ease);
            public static Tween CreateLocalMoveTween(MovementType type, Transform target, LocalTransformData from, LocalTransformData to, TweenData data)
            {
                KillConflicts(target);
                var result = CreateSequence(data, target);
                result.Append(CreateLocalMoveTweens(type, target, from, 0));
                result.Append(CreateLocalMoveTweens(type, target, to, data._duration));
                return result;
            }
            private static List<Tween> CreateLocalMoveTweens(MovementType type, Transform target, LocalTransformData data, float duration)
                => CreateLocalMoveTransformTweens(type, target, data._position, data._rotation, data._scale, duration);
            public static List<Tween> CreateLocalMoveTransformTweens(MovementType movement, Transform target, Vector3 position, Quaternion rotation, Vector3 scale, float duration)
                => CreateTweens<Tween>(movement,
                                () => target.DOLocalMove(position, duration),
                                () => target.DOLocalRotateQuaternion(rotation, duration),
                                () => target.DOScale(scale, duration));
            public static List<Tween> CreateMoveTransformTweens(MovementType flags, Transform target, Transform to, bool local, float duration) =>
                local ? CreateLocalMoveTransformTweens(flags, target, to.localPosition, to.localRotation, to.localScale, duration)
                : CreateTweens<Tween>(flags,
                    () => target.DOMove(to.position, duration),
                    () => target.DORotateQuaternion(to.rotation, duration),
                    () => target.DOScale(to.localScale, duration));
        }
    }
}