using System;
using UnityEngine;

namespace BB.Logic
{
	public abstract record ConditionNode(bool Inverted) : LeafNode
	{
		protected abstract bool IsTrue();
		public override bool CanEvaluate() => IsTrue() ^ Inverted;
	}
	namespace Serialized
	{
		[Serializable]
		public abstract class Condition : SerializedNodeFactory
		{
			[SerializeField]
			protected bool _inverted;
		}
	}
}