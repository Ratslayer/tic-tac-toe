using MessagePipe;
using Zenject;

namespace BB
{
	public interface IOverridableData
	{
		void ResetToInitialValue();
	}
	public interface IEventData { }
	public abstract record OverridableData<TValue> : IOverridableData
	{
		public TValue Value { get; private set; }
		public TValue InitialValue { get; private set; }
		public TValue PreviousValue { get; private set; }
		public virtual void SetValue(TValue value)
		{
			PreviousValue = Value;
			Value = value;
		}
		public void Init(TValue value)
		{
			InitialValue = value;
			Value = value;
		}
		public void ResetToInitialValue() => SetValue(InitialValue);
	}
	public abstract record OverridableDataWithEvent<TData, TValue>
		: OverridableData<TValue>, IEventData
		where TData : OverridableDataWithEvent<TData, TValue>
	{
		[Inject]
		IPublisher<TData> _publisher;
		public override void SetValue(TValue value)
		{
			base.SetValue(value);
			_publisher.Publish((TData)this);
		}
	}
	public static class DataExtensions
	{
		public static bool HasValue<T>(this OverridableData<T> data, out T value)
			where T : class
		{
			value = data.Value;
			return value != null;
		}
	}
}