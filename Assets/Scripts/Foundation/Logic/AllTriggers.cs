using System;
using System.Collections.Generic;
using UnityEngine;

namespace BB.Logic.Serialized
{
	[Serializable]
	public abstract class AbstractEvent<TTrigger, TActions> : SerializedLogicActions<TActions>
		where TTrigger : ISerializedTrigger
		where TActions : IDiFactory<ILogicNode>
	{
		[SerializeReference]
		List<TTrigger> _triggers = new();
		public override void Append(IResolver resolver)
		{
			var action = CreateNode(resolver);
			foreach (var e in _triggers)
				e.Create(resolver, action.EvaluateIfCan);
		}
	}
	[Serializable]
	public abstract class SerializedLogicActions<TActions> : SerializedEntityComponent
		where TActions : IDiFactory<ILogicNode>
	{
		[SerializeField]
		List<TActions> _actions = new();
		protected ILogicNode CreateNode(IResolver resolver)
		{
			var action = new EvaluateAll();
			foreach (var a in _actions)
				action.AddChild(a.Create(resolver));
			return action;
		}
	}
	[Serializable]
	public abstract class SerializedEvents<TTriggers> : SerializedEntityComponent
		where TTriggers : SerializedEntityComponent
	{
		[SerializeField]
		List<TTriggers> _events = new();
		public override void Append(IResolver resolver)
		{
			foreach (var e in _events)
				e.Append(resolver);
		}
	}
	[Serializable]
	public sealed class AllEvent:AbstractEvent<SerializedTrigger, SerializedActions> { }
	[Serializable]
	public sealed class AllEvents : SerializedEvents<AllEvent> { }
}
