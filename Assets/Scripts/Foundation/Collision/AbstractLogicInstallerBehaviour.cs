using UnityEngine;

namespace BB
{
	public abstract class AbstractLogicInstallerBehaviour<TSelf, TEvents> : AbstractInstallerBehaviour
		where TSelf : AbstractLogicInstallerBehaviour<TSelf, TEvents>
		where TEvents : SerializedEntityComponent, new()
	{
		[SerializeField]
		TEvents _logic = new();
		public override void InstallBindings(IBinder binder)
		{
			binder.Data((TSelf)this);
			binder.System<AppendLogic>();
		}
		sealed record AppendLogic(TSelf Behaviour) : EntitySystem,IOnStart
		{
			public void OnStart()
			{
				Behaviour._logic.Append(Resolver);
			}
		}
	}
}