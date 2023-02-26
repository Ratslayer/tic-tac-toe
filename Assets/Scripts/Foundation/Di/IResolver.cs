using MessagePipe;
using System;
using UnityEngine;

namespace BB
{
	public interface IGameObject
	{
		GameObject Instance { get; }
		Transform Transform => Instance.transform;
	}
	public interface IResolver
	{
		IResolver Parent { get; }
		string Name { get; set; }
		bool Installed { get; }
		void BasicInject(object instance);
		object Resolve(Type type);
		bool TryResolve(Type type, out object obj);
		object CreateExplicit(Type type, ValueTuple<Type, object>[] args);
		IBinder CreateChildBinder(GameObject instance);
		IBinder CreateChildBinder(string name);
		void InvokeAfterInstall(Action action);
		void ResolveRoots();
		void EndInstall();
		void AppendExplicit(Type type, params ValueTuple<Type, object>[] args);
		void Append(IEntityExtension extension);
	}
	public static class ResolverExtensions
	{
		public static T Resolve<T>(this IResolver resolver)
			=> (T)resolver.Resolve(typeof(T));
		public static bool TryResolve<T>(this IResolver resolver, out T value)
		{
			var result = resolver.TryResolve(typeof(T), out var obj);
			value = (T)obj;
			return result;
		}
		public static bool TryResolver<T>(this IResolver resolver, out T value, string errorMsg)
			=> Log.Assert(resolver.TryResolve(out value), errorMsg);
		public static void Inject(this IResolver resolver, object instance)
		{
			resolver.BasicInject(instance);
			if (instance is IOnInject injected)
				injected.OnInject(resolver);
		}
		public static T CreateExplicit<T>(this IResolver resolver, params ValueTuple<Type, object>[] args)
			=> (T)resolver.CreateExplicit(typeof(T), args);
		public static T Create<T>(this IResolver resolver, params object[] args)
			=> (T)resolver.CreateExplicit(typeof(T), GetExplicitArgs(typeof(T), args));
		public static void AppendExplicit<T>(this IResolver resolver, params ValueTuple<Type, object>[] args)
			=> resolver.AppendExplicit(typeof(T), args);
		public static void Append<T>(this IResolver resolver, params object[] args)
			=> resolver.AppendExplicit(typeof(T), GetExplicitArgs(typeof(T), args));
		static ValueTuple<Type, object>[] GetExplicitArgs(Type type, object[] args)
		{
			var result = new ValueTuple<Type, object>[args.Length];
			foreach (var i in args.Length)
				if (args[i] != null)
					result[i] = (args[i].GetType(), args[i]);
				else throw new ArgumentNullException(
					$"Null arg passed at creation of {type.Name}." +
					$"\nUse CreateExplicit if that's the desired behaviour.");
			return result;
		}
		public static bool TryPublish<T>(this IResolver to, T msg)
		{
			if (to.TryResolve(out IPublisher<T> publisher))
				publisher.Publish(msg);
			return publisher != null;
		}
		public static void Publish<T>(this IResolver to, T msg)
		{
			if (!to.TryPublish(msg))
				Debug.LogError($"{to.Name} does not have a {typeof(T).Name} publisher. This message will be ignored.");
		}
	}
	public interface IOnInject
	{
		void OnInject(IResolver resolver);
	}
}