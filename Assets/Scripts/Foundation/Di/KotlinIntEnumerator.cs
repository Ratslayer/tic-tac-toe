using System;

namespace BB
{
	public struct KotlinIntEnumerator
	{
		int _current;
		readonly int _end, _increment;
		public KotlinIntEnumerator(Range range, int stepSize)
		{
			_current = range.Start.Value - 1;
			_end = range.End.Value;
			_increment = _current < _end ? stepSize : -stepSize;
		}
		public KotlinIntEnumerator(Range range) : this(range, 1) { }
		public int Current => _current;
		public bool MoveNext()
		{
			if (_increment == 0)
				return false;
			_current += _increment;
			return _current <= _end;
		}
	}
	public static class KotlinIntEnumeratorExtensions
	{
		public static KotlinIntEnumerator GetEnumerator(this int range)
		{
			if (range > 0)
				return new(new(0, range - 1));
			else return new();
		}
		public static KotlinIntEnumerator GetEnumerator(this Range range)
			=> new(range);
		public static KotlinIntEnumerator GetEnumerator(this ValueTuple<Range, int> tuple)
			=> new(tuple.Item1, tuple.Item2);
	}
}