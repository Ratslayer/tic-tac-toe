//using System;
//using System.Collections.Generic;
//using UnityEngine;

//namespace BB.Logic.Serialized
//{
//	[Serializable]
//	public sealed class TriggerAction
//	{
//		[SerializeReference]
//		List<SerializedTrigger> _triggers = new();
//		[SerializeReference]
//		List<SerializedEvent> _actions = new();
//		[SerializeReference]
//		List<Decorator> _decorators = new();
//		public void Create(IResolver resolver)
//		{
//			var root = new EvaluateAll();
//			foreach (var action in _actions)
//				root.AddChild(action.Create(resolver));
//			foreach (var decorator in _decorators)
//				root.AddChild(decorator.Create(resolver));
//			//var result = new CompositeTrigger();
//			foreach (var trigger in _triggers)
//				trigger.Create(resolver, root.EvaluateIfCan);
//			//return result;
//		}

//	}
//	[Serializable]
//	public sealed class TriggerActionsOld
//	{
//		[SerializeField]
//		List<TriggerAction> _actions = new();
//		public void Create(IResolver resolver)
//		{
//			foreach (var action in _actions)
//				action.Create(resolver);
//		}
//	}
//	//public sealed class CompositeTrigger : ITrigger
//	//{
//	//	readonly List<ITrigger> _triggers = new();
//	//	public void Add(ITrigger trigger) => _triggers.Add(trigger);
//	//	public void Subscribe()
//	//	{
//	//		foreach (var trigger in _triggers)
//	//			trigger.Subscribe();
//	//	}
//	//	public void Unsubscribe()
//	//	{
//	//		foreach (var trigger in _triggers)
//	//			trigger.Unsubscribe();
//	//	}
//	//}
//}