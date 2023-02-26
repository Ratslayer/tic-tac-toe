using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BB
{
	public interface IInstaller
	{
		void InstallBindings(IBinder binder);
	}
	public abstract class AbstractInstaller : BaseScriptableObject, IInstaller
	{
		[SerializeField]
		List<AbstractEntityExtension> _extensions = new();
		[SerializeField]
		List<AbstractInstaller> _installers = new();
		public void InstallBindings(IBinder binder)
		{
			Install(binder);
			foreach(var installer in _installers)
				installer.InstallBindings(binder);
			foreach(var extension in _extensions)
				extension.Append(binder);
		}
		protected abstract void Install(IBinder binder);
	}
	public readonly struct DisposeEvent { }
	public readonly struct StartEvent { }
	public readonly struct SpawnEvent { }
	public readonly struct DespawnEvent { }
}