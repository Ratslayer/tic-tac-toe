namespace BB.Logic
{
	public sealed record DespawnNode(IEntity Entity) : LeafNode
	{
		public override void Evaluate() => Entity.Despawn();
	}
	namespace Serialized
	{
		public sealed class Despawn : SerializedAction
		{
			public override ILogicNode Create(IResolver resolver)
				=> resolver.Create<DespawnNode>();
		}
	}
}