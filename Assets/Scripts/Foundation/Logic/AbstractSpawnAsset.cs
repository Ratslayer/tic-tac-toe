namespace BB
{
	public abstract class AbstractSpawnAsset : BaseScriptableObject, IEntityFactory
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
	public abstract class EntityAsset : AbstractSpawnAsset
	{
		protected override IEntity CreateEntity(IResolver parent) => parent.CreateChild(Name, Install);
		protected abstract void Install(IBinder binder);
	}
}
