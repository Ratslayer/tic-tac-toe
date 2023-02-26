using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;

namespace BB
{
	public sealed class CompositeInstaller : AbstractInstallerBehaviour
	{
		[SerializeField]
		List<AbstractInstaller> _installers;
		public override void InstallBindings(IBinder binder)
		{
			foreach (var installer in _installers)
				installer.InstallBindings(binder);
		}
	}
	public abstract class AbstractInstallerBehaviour : MonoBehaviour, IInstaller
	{
		void Awake()
		{
			gameObject.GetOrCreateComponent<EntityBehaviour>();
		}
		public abstract void InstallBindings(IBinder binder);
	}
}