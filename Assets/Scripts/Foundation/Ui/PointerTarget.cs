using MessagePipe;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BB.UI
{
	public sealed class PointerTarget
		: MonoBehaviour,
		IPointerDownHandler,
		IPointerUpHandler,
		IPointerClickHandler,
		ISelectHandler,
		IDeselectHandler
	{
		public event Action Pressed, Released, Clicked, Selected, Deselected;

		public void OnDeselect(BaseEventData eventData)
		{
			Deselected?.Invoke();
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			Clicked?.Invoke();
		}
		public void OnPointerDown(PointerEventData eventData)
		{
			Pressed?.Invoke();
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			Released?.Invoke();
		}

		public void OnSelect(BaseEventData eventData)
		{
			Selected?.Invoke();
		}
	}
	public sealed record BroadcastPointerEvents(
		PointerTarget Target,
		IPublisher<PointerClicked> Clicked,
		IPublisher<PointerPressed> Pressed,
		IPublisher<PointerReleased> Released,
		IPublisher<UiElementSelected> Selected,
		IPublisher<UiElementDeselected> Deselected)
		: EntitySystem, IDisposable, IOnStart
	{
		public void OnStart()
		{
			Target.Clicked += OnClick;
			Target.Pressed += OnPress;
			Target.Released += OnRelease;
			Target.Selected += OnSelected;
			Target.Deselected += OnDeselected;
		}
		public void Dispose()
		{
			Target.Clicked -= OnClick;
			Target.Pressed -= OnPress;
			Target.Released -= OnRelease;
			Target.Selected -= OnSelected;
			Target.Deselected -= OnDeselected;
		}
		void OnPress() => Pressed.Publish(new());
		void OnRelease() => Released.Publish(new());
		void OnClick() => Clicked.Publish(new());
		void OnSelected() => Selected.Publish(new());
		void OnDeselected() => Deselected.Publish(new());
	}
	public readonly struct PointerClicked { }
	public readonly struct PointerReleased { }
	public readonly struct PointerPressed { }
	public readonly struct UiElementSelected { }
	public readonly struct UiElementDeselected { }
}