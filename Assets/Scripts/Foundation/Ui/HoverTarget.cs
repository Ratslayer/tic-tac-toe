using MessagePipe;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BB.UI
{
	public sealed class HoverTarget : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
	{
		public event Action<PointerEventData> Entered, Exited, Moved;
		public void OnPointerEnter(PointerEventData eventData) => Entered?.Invoke(eventData);

		public void OnPointerExit(PointerEventData eventData) => Exited?.Invoke(eventData);

		public void OnPointerMove(PointerEventData eventData) => Moved?.Invoke(eventData);
	}
	public sealed record BroadcastHoverEvents(
		HoverTarget Target,
		IPublisher<HoverEntered> Entered,
		IPublisher<HoverExited> Exited,
		IPublisher<HoverMoved> Moved)
		: EntitySystem, IDisposable, IOnStart
	{
		public void OnStart()
		{
			Target.Entered += OnEnter;
			Target.Exited += OnExit;
			Target.Moved += OnMove;
		}
		public void Dispose()
		{
			Target.Entered -= OnEnter;
			Target.Exited -= OnExit;
			Target.Moved -= OnMove;
		}
		void OnEnter(PointerEventData data) => Entered.Publish(new(data));
		void OnExit(PointerEventData data) => Exited.Publish(new(data));
		void OnMove(PointerEventData data) => Moved.Publish(new(data));
	}
	public readonly struct HoverEntered
	{
		public readonly PointerEventData _data;
		public HoverEntered(PointerEventData data) => _data = data;
	}
	public readonly struct HoverExited
	{
		public readonly PointerEventData _data;
		public HoverExited(PointerEventData data) => _data = data;
	}
	public readonly struct HoverMoved
	{
		public readonly PointerEventData _data;
		public HoverMoved(PointerEventData data) => _data = data;
	}
}