using MessagePipe;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BB
{

	public static class EventSubscriptionUtils
	{
		//public static void Publish<T>(this IEntity target, IEntity source, T msg)
		//{
		//	if (target.Resolver.TryResolve<IPublisher<T>>(out var publisher))
		//		publisher.PublishWithTarget(msg, source, target);
		//}
		//static readonly List<IEntity> _targets = new(), _sources = new();
		//public static IEntity Target
		//{
		//	get
		//	{
		//		if (_targets.Count == 0)
		//		{
		//			Debug.LogError($"A targeted event has been published without using PublishWithTarget.");
		//			return null;
		//		}
		//		return _targets[^1];
		//	}
		//}
		//public static IEntity Source
		//{
		//	get
		//	{
		//		if (_sources.Count == 0)
		//		{
		//			Debug.LogError($"Event has been published without specifying the source.");
		//			return null;
		//		}
		//		return _sources[^1];
		//	}
		//}
		//public static void PublishWithSource<T>(this IPublisher<T> publisher, T msg, IEntity source)
		//{
		//	try
		//	{
		//		_sources.Add(source);
		//		publisher.Publish(msg);
		//	}
		//	catch (Exception ex)
		//	{
		//		Debug.LogException(ex);
		//	}
		//	finally
		//	{
		//		_sources.RemoveAt(_sources.Count - 1);
		//	}
		//}
		//public static void PublishWithTarget<T>(this IPublisher<T> publisher, T msg, IEntity source, IEntity target)
		//{
		//	_targets.Add(target);
		//	publisher.PublishWithSource(msg, source);
		//	_targets.RemoveAt(_targets.Count - 1);
		//}
		public static void Publish<T>(this IPublisher<T> publisher)
			where T : new()
			=> publisher.Publish(default);
		public static void PublishOnFrameEnd<T>(this IPublisher<T> publisher)
			where T : new()
			=> TimerUtils.InvokeOnFrameEnd(publisher.Publish);
	}
}