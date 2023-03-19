namespace BB
{
	public abstract class EntityAsset : BaseScriptableObject, IEntityFactory
	{
		public string Name => name;
		public IEntity Create(IResolver parent)
		{
			var result = CreateEntity(parent);
			if (result != null)
				result.Name = name;
			return result;
		}
		protected abstract IEntity CreateEntity(IResolver parent);
	}
}
