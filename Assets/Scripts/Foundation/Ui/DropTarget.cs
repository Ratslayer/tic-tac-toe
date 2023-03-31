using MessagePipe;
using System;
using UnityEngine;
using UnityEngine.EventSystems;

namespace BB.UI
{
	public sealed class DropTarget : MonoBehaviour, IDropHandler
	{
		public event Action<IEntity> ReceivedDrop;
		public void OnDrop(PointerEventData eventData)
		{
			var go = eventData.pointerDrag;
			if (!go)
				return;
			if (go.TryGetComponent(out DragTarget drag) && TryGetComponent(out EntityBehaviour root))
				drag.OnDrop(root);
			if (go.TryGetComponent(out root))
				ReceivedDrop?.Invoke(root);
		}
	}
}