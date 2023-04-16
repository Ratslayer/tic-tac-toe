namespace BB
{
	public static class GlobalSystems
	{
		public sealed record InitSystems(
			GameObjectPools GoPools,
			EntityPools EntityPools) : EntitySystem, GlobalSystems.ISystems, IOnInstall
		{
			public void OnStart()
			{
				GlobalSystems.Systems = this;
			}
		}
		public interface ISystems
		{
			GameObjectPools GoPools { get; }
			EntityPools EntityPools { get; }
		}
		public static ISystems Systems { get; set; }
		public static GameObjectPools GoPools => Systems.GoPools; 
		public static EntityPools EntityPools => Systems.EntityPools;
	}
}
