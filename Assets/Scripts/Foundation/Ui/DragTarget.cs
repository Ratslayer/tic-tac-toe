using MessagePipe;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BB.UI
{
	public sealed class DragTarget : MonoBehaviour,
		IBeginDragHandler,
		IEndDragHandler,
		IDragHandler
	{
		public event Action DragBegan, DragEnded;
		public event Action<IEntity> Dropped;
		public event Action<Vector2> Dragged;
		public void OnBeginDrag(PointerEventData eventData)
		{
			DragBegan?.Invoke();
		}
		public void OnDrag(PointerEventData eventData)
		{
			Dragged?.Invoke(eventData.delta);
		}
		public void OnEndDrag(PointerEventData eventData)
		{
			DragEnded?.Invoke();
		}
		public void OnDrop(IEntity entity) => Dropped?.Invoke(entity);
	}
	public sealed record BroadcastDragEvents(
		IPublisher<UiBeginDrag> BeganDrag,
		IPublisher<UiEndDrag> EndedDrag,
		IPublisher<UiDropped> Dropped,
		DragTarget Draggable) : EntitySystem, IDisposable, IOnStart
	{
		public void OnStart()
		{
			Draggable.DragBegan += BeginDrag;
			Draggable.DragEnded += EndDrag;
			Draggable.Dropped += Drop;
		}
		public void Dispose()
		{
			Draggable.DragBegan -= BeginDrag;
			Draggable.DragEnded -= EndDrag;
			Draggable.Dropped -= Drop;
		}
		void Drop(IEntity target)
		{
			Dropped.Publish(new() { _target = target });
		}
		void BeginDrag()
		{
			BeganDrag.Publish(new());
		}
		void EndDrag()
		{
			EndedDrag.Publish(new());
		}


	}
	public abstract record UiDragSystem : EntitySystem, IOnStart
	{
		protected abstract void OnBeginDrag();
		protected abstract void OnEndDrag();
		void Begin(UiBeginDrag begin)
		{
			OnBeginDrag();
		}
		void End(UiEndDrag end)
		{
			OnEndDrag();
		}

		public void OnStart()
		{
			Subscribe<UiBeginDrag>(Begin);
			Subscribe<UiEndDrag>(End);
		}
	}
	public sealed record DisableRaycastBlockOnDrag(UiElement Element) : UiDragSystem
	{
		protected override void OnBeginDrag()
		{
			Element.BlocksRaycasts = false;
		}
		protected override void OnEndDrag()
		{
			Element.BlocksRaycasts = true;
		}
	}
	public sealed record ForegroundOnDrag(
		UiLayout Layout,
		EntityTransform T,
		EntityParent Parent) : UiDragSystem
	{
		protected override void OnBeginDrag()
		{
			T.Value.SetParent(Layout._foreground);
		}
		protected override void OnEndDrag()
		{
			T.Value.SetParent(Parent.Value);
		}
	}
	public sealed record MoveOnDrag(
		EntityTransform T,
		DragTarget draggable)
		: EntitySystem, IDisposable, IOnStart
	{
		readonly DragTarget _draggable;
		public void OnStart()
		{
			draggable.Dragged += Drag;
		}
		public void Dispose()
		{

			_draggable.Dragged -= Drag;
		}
		void Drag(Vector2 delta)
		{
			T.Value.position += (Vector3)delta;
		}


	}
	public struct UiEndDrag { }
	public struct UiBeginDrag { }
	public struct UiDropped { public IEntity _target; }

}