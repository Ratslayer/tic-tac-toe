using MessagePipe;
using System;
namespace BB.Logic.Serialized
{
	public interface ISerializedTrigger
	{
		void Create(IResolver resolver, Action action);
	}
	[Serializable]
	public abstract class SerializedTrigger : ISerializedTrigger
	{
		public abstract void Create(IResolver resolver, Action action);
	}
	[Serializable]
	public abstract class SerializedTrigger<TEvent> : SerializedTrigger
	{
		public override void Create(IResolver resolver, Action action)
			=> resolver.Append<Node>(action);
		sealed record Node(Action Action)
			: TriggerNode<TEvent>(Action);
	}
	[Serializable]
	public abstract class BroadcastEmptyEventAction<TEvent> : SerializedAction
		where TEvent : new()
	{
		public override ILogicNode Create(IResolver resolver)
			=> resolver.Create<Node>();
		sealed record Node(IPublisher<TEvent> Publisher) : LeafNode
		{
			public override void Evaluate()
				=> Publisher.Publish();
		}
	}
}