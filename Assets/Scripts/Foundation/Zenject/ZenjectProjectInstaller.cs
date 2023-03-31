using UnityEngine;
using Zenject;
using MessagePipe;

namespace BB
{
	public sealed class ZenjectProjectInstaller : BaseScriptableObject
	{
		public const string RESOURCE_NAME = "ProjectInstaller";
		public const string CONTAINER_NAME = "Project";
		[SerializeField]
		private AbstractInstaller _installerAsset;
		IBinder _binder;
		void Install()
		{
			var container = new DiContainer(StaticContext.Container);
			////message pipe must be configured on the container first
			//var serviceProvider = container.AsServiceProvider();
			//GlobalMessagePipe.SetProvider(serviceProvider);
			//if (!container.IsValidating)
			//create instance
			var instance = new GameObject(RESOURCE_NAME);
			DontDestroyOnLoad(instance);
			//install binder
			_binder = new GoZenjectBinder(container, null, instance);
			InstallerUtils.BindGoEntity(_binder, instance, true);
			_installerAsset.InstallBindings(_binder);
			_binder.Install();
			//register as this binder as root
			DiServices.SetRoot(_binder);
			//spawn
			var entity = _binder.Resolve<IEntity>();
			entity.Spawn();
		}
		//before splash screen does not work properly in build
		//on first scene load everything gets cleared
		//so call on first scene load instead
		//static bool _initialized = false;
		////reset initialized on splash screen to allow disabling domain reload
		//[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		//static void ResetInitialize() => _initialized = false;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		static void InstallProject()
		{
			//if (_initialized)
			//	return;
			//_initialized = true;
			var installer = Resources.Load<ZenjectProjectInstaller>(RESOURCE_NAME);
			if (!Log.AssertNotNull(installer, $"No installer '{RESOURCE_NAME}' found in resources."))
				return;
			installer.Install();
			//ProjectContext.Instance.EnsureIsInitialized();
		}
	}
}