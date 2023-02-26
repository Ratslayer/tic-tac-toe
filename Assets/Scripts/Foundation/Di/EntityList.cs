using System.Collections.Generic;

namespace BB
{
	public abstract record EntityList<T>()
		where T : EntityData
	{
		readonly List<T> _datas = new();
		public void Add(T data) => _datas.Add(data);
		public void Remove(IEntity entity) => _datas.RemoveAll(d => d.Entity == entity);
		public void Clear() => _datas.Clear();
	}
}