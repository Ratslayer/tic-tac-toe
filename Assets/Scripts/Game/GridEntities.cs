namespace BB
{
	public abstract record AbstractGridTable<T>(IGrid Grid) : EntitySystem, IOnSpawn
	{
		protected T[,] _values;

		public void OnSpawn()
		{
			_values = new T[Grid.NumColumns, Grid.NumRows];
		}
		public int X => _values.GetLength(0);
		public int Y => _values.GetLength(1);
		public void Set(int x, int y, T value) => _values[x, y] = value;
		public void Set(CellData data, T value) => _values[data.X, data.Y] = value;
		public void Clear()
		{
			foreach (var i in X)
				foreach (var j in Y)
					_values[i, j] = default;
		}
		public T Get(int x, int y) => _values[x, y];
		public T Get(CellData data) => Get(data.X, data.Y);
	}
	public sealed record GridEntities(IGrid Grid) : AbstractGridTable<IEntity>(Grid)
	{
		public void DespawnAndClearAll()
		{
			foreach (var i in X)
				foreach (var j in Y)
				{
					_values[i, j]?.Despawn();
					_values[i, j] = default;
				}
		}
	}
}
