using MessagePipe;
using System;
namespace BB.Logic
{
	public abstract record TriggerNode<TEvent>(
		Action Action)
		: EntitySystem,IOnInstall
	{
		public void OnStart()
		{
			Subscribe<TEvent>(OnEvent);
		}
		void OnEvent(TEvent _) => Action();
	}
}
