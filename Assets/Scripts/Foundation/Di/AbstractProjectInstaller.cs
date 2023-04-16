using Sirenix.OdinInspector;
using UnityEngine;

namespace BB
{
	public abstract class AbstractProjectInstaller : AbstractInstaller
	{
		[Required, SerializeField]
		AbstractStateProvider _defaultState;
		protected override void Install(IBinder binder)
		{
			binder.StateMachine(_defaultState);
			binder.System<GameObjectPools>();
			binder.System<EntityPools>();
			binder.System<GlobalSystems.InitSystems>();
		}
	}
}
