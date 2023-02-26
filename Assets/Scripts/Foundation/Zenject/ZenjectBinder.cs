using System;
using UnityEngine;
using Zenject;
using MessagePipe;
using System.Collections.Generic;

namespace BB
{
	public abstract class AbstractZenjectBinder : IBinder
	{
		readonly DiContainer _container;
		readonly MessagePipeOptions _options;
		string _name;
		event Action InstallEnded;
		public virtual string Name { get => _name; set => _name = value; }
		public AbstractZenjectBinder(DiContainer container, IResolver parent, string name)
		{
			_name = name;
			_container = container;
			Parent = parent;
			_options = _container.BindMessagePipe(CustomizeOptions);
			BindInstance<IResolver>(this);
		}
		void CustomizeOptions(MessagePipeOptions options)
		{
			//options.AddGlobalMessageHandlerFilter(typeof(LoggingFilter<>), -10000);
		}

		public IResolver Parent { get; private set; }

		public bool Installed { get; private set; }

		public void QueueForInject(object instance)
			=> _container.QueueForInject(instance);
		public void Bind(Type contractType, Type instanceType)
			=> _container.Bind(contractType).To(instanceType).AsSingle().NonLazy();
		public void BindLazy(Type contractType, Type instanceType)
			=> _container.Bind(contractType).To(instanceType).AsSingle();
		public void BindInstance(Type contractType, object instance)
			=> _container.Bind(contractType).FromInstance(instance).AsSingle().NonLazy();

		public void BindAllInterfaces(object instance)
			=> _container.BindInterfacesTo(instance.GetType()).FromInstance(instance).AsSingle().NonLazy();

		public void BindInstance<TContract>(TContract instance)
			=> _container.BindInstance(instance).AsSingle().NonLazy();

		public void Event<T>()
			=> _container.BindMessageBroker<T>(_options);

		public void DontDestroyOnLoad(GameObject obj)
		{
			if (!_container.IsValidating)
				UnityEngine.Object.DontDestroyOnLoad(obj);
		}

		public void BasicInject(object instance)
		{
			_container.Inject(instance);
		}

		public object Resolve(Type type) => _container.Resolve(type);

		public bool TryResolve(Type type, out object obj)
		{
			obj = _container.TryResolve(type);
			return obj != null;
		}
		readonly List<TypeValuePair> _args = new();
		public object CreateExplicit(Type type, ValueTuple<Type, object>[] args)
		{
			_args.Clear();
			foreach (var arg in args)
				_args.Add(new(arg.Item1, arg.Item2));
			return _container.InstantiateExplicit(type, _args);
		}
		//public object Create(Type type, object[] args)
		//{
		//	if (args.Contains(default(object)))
		//		throw new ArgumentNullException($"Null arg passed at creation of {type.Name}." +
		//			$"\nUse CreateExplicit if that's the desired behaviour.");
		//	_args.Clear();
		//	foreach (var arg in args)
		//		_args.Add(new(arg.GetType(), arg));
		//	return _container.InstantiateExplicit(type, _args);
		//}

		public IBinder CreateChildBinder(GameObject instance)
		{
			var container = _container.CreateSubContainer();
			var binder = new GoZenjectBinder(container, this, instance);
			return binder;
		}
		public IBinder CreateChildBinder(string name)
		{
			var container = _container.CreateSubContainer();
			var binder = new ZenjectBinder(container, this, name);
			return binder;
		}

		public void InvokeAfterInstall(Action action)
		{
			if (Installed)
				action();
			else InstallEnded += action;
		}

		public void ResolveRoots()
		{
			_container.ResolveRoots();

		}

		public void EndInstall()
		{
			Installed = true;
			//resolve append queues
			foreach (var appendix in _appendices)
				CreateExplicit(appendix.Type, appendix.Args);
			_appendices.Clear();
			foreach (var extension in _extensions)
				extension.Append(this);
			_extensions.Clear();
			InstallEnded?.Invoke();
			InstallEnded = null;
		}
		sealed record Appendix(Type Type, ValueTuple<Type, object>[] Args);
		readonly List<Appendix> _appendices = new();
		readonly List<IEntityExtension> _extensions = new();
		public void AppendExplicit(Type type, params ValueTuple<Type, object>[] args)
		{
			if (Installed)
				CreateExplicit(type, args);
			else _appendices.Add(new Appendix(type, args));
		}
		public void Append(IEntityExtension extension)
		{
			if (extension == null)
				return;
			if (Installed)
				extension.Append(this);
			else _extensions.Add(extension);
		}
	}
	public sealed class ZenjectBinder : AbstractZenjectBinder
	{
		public ZenjectBinder(DiContainer container, IResolver parent, string name)
			: base(container, parent, name)
		{
		}
	}
	public sealed class GoZenjectBinder : AbstractZenjectBinder, IGameObject
	{
		readonly GameObject _instance;
		public override string Name { get => _instance.name; set => _instance.name = value; }
		public GoZenjectBinder(DiContainer container, IResolver parent, GameObject instance)
			: base(container, parent, instance.name)
		{
			_instance = instance;
		}

		public GameObject Instance => _instance;
	}
}