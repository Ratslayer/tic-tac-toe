using System.Collections.Generic;

namespace BB.Logic
{
	public interface ILogicNode
	{
		bool CanEvaluate();
		void Evaluate();
	}
	public abstract record LeafNode : ILogicNode
	{
		public virtual bool CanEvaluate() => true;
		public virtual void Evaluate() { }
	}
	public abstract class CompositeNode : ILogicNode
	{
		public virtual bool CanEvaluate() => true;
		public virtual void Evaluate() { }
		protected readonly List<ILogicNode> _children = new();
		public void AddChild(ILogicNode node) => _children.Add(node);
	}
	public sealed class EvaluateAll : CompositeNode
	{
		public override bool CanEvaluate()
		{
			var result = !_children.Contains(Invalid);
			return result;
			static bool Invalid(ILogicNode node)
			{
				var valid = node.CanEvaluate();
				return !valid;
			}
		}
		public override void Evaluate()
		{
			foreach (var child in _children)
				child.Evaluate();
		}
	}
	public sealed class EvaluateAny : CompositeNode
	{
		public override bool CanEvaluate() => _children.Contains(c => c.CanEvaluate());
		public override void Evaluate()
		{
			foreach (var child in _children)
				if (child.CanEvaluate())
					child.Evaluate();
		}
	}
	public sealed class EvaluateFirst : CompositeNode
	{
		public override bool CanEvaluate() => _children.Contains(c => c.CanEvaluate());
		public override void Evaluate()
		{
			foreach (var child in _children)
				if (child.CanEvaluate())
				{
					child.Evaluate();
					break;
				}
		}
	}
	public static class LogicNodeExtensions
	{
		public static void EvaluateIfCan(this ILogicNode node)
		{
			if (node.CanEvaluate())
				node.Evaluate();
		}
	}
}