using MessagePipe;
using System;
using Zenject;

namespace BB
{
	public abstract class EventData<TData, TValue>
		where TData : EventData<TData, TValue>
	{
		[Inject]
		private IPublisher<TData> _publisher;
		TValue _value;
		public TValue Value
		{
			get => _value;
			set
			{
				if (Equals(_value, value))
					return;
				_value = value;
				_publisher.Publish((TData)this);
			}
		}
	}
	//public readonly struct Changed<TData, TValue>
	//{
	//	public readonly TValue _oldValue, _newValue;
	//	public Changed(TValue oldValue, TValue newValue)
	//	{
	//		_oldValue = oldValue;
	//		_newValue = newValue;
	//	}
	//}
}