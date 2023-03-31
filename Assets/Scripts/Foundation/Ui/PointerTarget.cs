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
		public event Action<PointerEventData> Pressed, Released, Clicked;
		public event Action Selected, Deselected;

		public void OnDeselect(BaseEventData eventData)
		{
			Deselected?.Invoke();
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			Clicked?.Invoke(eventData);
		}
		public void OnPointerDown(PointerEventData eventData)
		{
			Pressed?.Invoke(eventData);
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			Released?.Invoke(eventData);
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
		: EntitySystem, IDisposable, IOnInstall
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
		void OnPress(PointerEventData data) => Pressed.Publish(new(data));
		void OnRelease(PointerEventData data) => Released.Publish(new(data));
		void OnClick(PointerEventData data) => Clicked.Publish(new(data));
		void OnSelected() => Selected.Publish(new());
		void OnDeselected() => Deselected.Publish(new());
	}
	public readonly struct PointerClicked {
		public readonly PointerEventData _data;
		public PointerClicked(PointerEventData data) => _data = data;
	}
	public readonly struct PointerReleased {
		public readonly PointerEventData _data;
		public PointerReleased(PointerEventData data) => _data = data;
	}
	public readonly struct PointerPressed {
		public readonly PointerEventData _data;
		public PointerPressed(PointerEventData data) => _data = data;
	}
	public readonly struct UiElementSelected { }
	public readonly struct UiElementDeselected { }
}