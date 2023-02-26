using System;
using System.Collections.Generic;
using UnityEngine;

namespace BB.Logic.Serialized
{
	[Serializable]
	public sealed class IfAllTrue : Decorator
	{
		[SerializeReference]
		List<Condition> _conditions = new();
		public override ILogicNode Create(IResolver resolver)
			=> SerializedEventUtils.Create<EvaluateAll>(resolver, _conditions);
	}
	public static class SerializedEventUtils
	{
		public static ILogicNode Create<T>(IResolver resolver, IEnumerable<SerializedNodeFactory> events)
			where T : CompositeNode, new()
		{
			var result = new T();
			foreach(var e in events)
				result.AddChild(e.Create(resolver));
			return result;
		}
	}
}