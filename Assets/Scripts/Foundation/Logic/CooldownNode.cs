using System;
using UnityEngine;

namespace BB.Logic
{
	public sealed record CooldownNode(float Cooldown) : LeafNode
	{
		bool _onCooldown;
		public override bool CanEvaluate() => !_onCooldown;
		public override void Evaluate()
		{
			_onCooldown = true;
			TimerUtils.DelayedInvoke(Cooldown, () => _onCooldown = false);
		}
	}
	namespace Serialized
	{
		[Serializable]
		public sealed class Cooldown : Decorator
		{
			[SerializeField]
			private float _cooldown;
			public override ILogicNode Create(IResolver resolver)
				=> resolver.Create<CooldownNode>(_cooldown);
		}
	}
}