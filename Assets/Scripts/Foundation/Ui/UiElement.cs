using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace BB.UI
{
	public sealed class UiElement : MonoBehaviour
	{
		CanvasGroup _group;
		private void Awake()
		{
			_group = gameObject.GetOrCreateComponent<CanvasGroup>();
		}
		public float Alpha
		{
			get => _group.alpha;
			set => _group.alpha = value;
		}
		public bool BlocksRaycasts
		{
			get => _group.blocksRaycasts;
			set => _group.blocksRaycasts = value;
		}
		public Tween TweenAlpha(float value, TweenData curve)
			=> _group.Alpha(value, curve);
	}

}