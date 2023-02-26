using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BB.Logic.Serialized
{
	[Serializable]
	public abstract class SerializedAction : SerializedNodeFactory { }
	[Serializable]
	public abstract class AbstractActions<TAction> : SerializedNodeFactory
		where TAction : SerializedNodeFactory
	{
		[SerializeReference]
		List<TAction> _actions = new();
		[SerializeReference, PropertyOrder(1)]
		List<Decorator> _decorators = new();
		public override ILogicNode Create(IResolver resolver)
		{
			var result = new EvaluateAll();
			if (_actions.Count == 0)
				return result;
			foreach (var es in GetEvents())
				foreach (var e in es)
					result.AddChild(e.Create(resolver));
			return result;
		}
		protected virtual IEnumerable<IEnumerable<SerializedNodeFactory>> GetEvents()
		{
			yield return _decorators;
			yield return _actions;
		}
	}
	[Serializable]
	public sealed class SerializedActions : AbstractActions<SerializedAction> { }
}