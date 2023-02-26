namespace BB
{
	public sealed record GridEntitiesList(IGrid Grid) : EntitySystem, IOnSpawn
	{
		IEntity[,] _entities;

		public void OnSpawn()
		{
			_entities = new IEntity[Grid.NumColumns, Grid.NumRows];
		}
		public void Set(int x, int y, IEntity entity) => _entities[x, y] = entity;
		public void Set(CellData data, IEntity entity) => _entities[data.X, data.Y] = entity;
		public void Clear()
		{
			foreach (var i in _entities.GetLength(0))
				foreach (var j in _entities.GetLength(1))
					_entities[i, j] = null;
		}
		public IEntity Get(int x, int y) => _entities[x, y];
		public IEntity Get(CellData data) => Get(data.X, data.Y);
	}
}
