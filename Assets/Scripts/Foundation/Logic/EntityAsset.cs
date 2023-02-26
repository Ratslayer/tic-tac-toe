namespace BB
{
	public abstract class EntityAsset : BaseScriptableObject, IEntityFactory
	{
		public IEntity CreateEntity(IResolver parent)
		{
			var result = Create(parent);
			if (result != null)
				result.Name = name;
			return result;
		}
		protected abstract IEntity Create(IResolver parent);
	}
}
