using System;
using UnityEngine;

namespace BB
{
	public abstract record AbstractGridTable<T> : EntitySystem
	{
		protected T[,] _values = new T[1, 1];
		public void Init(int cols, int rows) => _values = new T[cols, rows];
		public int X => _values.GetLength(0);
		public int Y => _values.GetLength(1);
		public int NumCells => X * Y;
		public void Set(int x, int y, T value)
		{
			Clear(_values[x, y]);
			_values[x, y] = value;
		}
		public void Set(CellData data, T value) => Set(data.X, data.Y, value);
		[Subscribe]
		void OnResize(ResizeGridEvent _) => Clear();
		[Subscribe]
		void OnRestart(RestartGameEvent _) => Clear();
		public void Clear()
		{
			foreach (var i in X)
				foreach (var j in Y)
				{
					Clear(_values[i, j]);
					_values[i, j] = default;
				}
		}
		protected virtual void Clear(T value) { }
		public T Get(int x, int y)
		{
			try
			{
				return _values[x, y];
			}
			catch (Exception e)
			{
				Debug.LogException(e);
			}
			return default;
		}
		public T Get(CellData data) => Get(data.X, data.Y);
	}
	public sealed record GridEntities : AbstractGridTable<IEntity>
	{
		protected override void Clear(IEntity value)
		{
			base.Clear(value);
			value?.Despawn();
		}
	}
}
