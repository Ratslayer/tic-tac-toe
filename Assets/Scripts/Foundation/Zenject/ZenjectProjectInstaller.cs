using UnityEngine;
using Zenject;
using MessagePipe;

namespace BB
{
	public sealed class ZenjectProjectInstaller : MonoInstaller
	{
		[SerializeField]
		private AbstractInstaller _installerAsset;
		IBinder _binder;
		IEntity _entity;
		public override void InstallBindings()
		{
			_binder = new GoZenjectBinder(Container, null, gameObject);
			name = "Game";
			DiServices.SetRoot(_binder);
			DiServices.SetGame(_binder);
			//message pipe must be configured on the container first
			if (!Container.IsValidating)
				GlobalMessagePipe.SetProvider(Container.AsServiceProvider());
			_entity = InstallerUtils.BindGoEntity(_binder, gameObject, true);
			_installerAsset.InstallBindings(_binder);
		}
		public override void Start()
		{
			_entity.Resolver.EndInstall();
			_entity.Spawn();
		}
		//before splash screen does not work properly in build
		//on first scene load everything gets cleared
		//so call on first scene load instead
		static bool _initialized = false;
		//reset initialized on splash screen to allow disabling domain reload
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSplashScreen)]
		static void ResetInitialize() => _initialized = false;
		[RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
		static void InstallProject()
		{
			if (_initialized)
				return;
			_initialized = true;
			ProjectContext.Instance.EnsureIsInitialized();
		}
	}
}