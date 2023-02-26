using MessagePipe;
using System;
using Zenject;

namespace BB
{
	public interface IDataOverride
	{
		Type DataType { get; }
		void ApplyValue(object data);
	}
	public sealed class DataOverride<TData, TValue> : IDataOverride
		where TData : OverridableData<TValue>
	{
		readonly TValue _value;
		public DataOverride(TValue value) => _value = value;
		public Type DataType => typeof(TData);
		public void ApplyValue(object data)
		{
			var d = (TData)data;
			d.SetValue(_value);
		}
	}
	public struct DataChanged<TData>
	{
		public readonly TData _data;
		public DataChanged(TData data) => _data = data;
	}
}