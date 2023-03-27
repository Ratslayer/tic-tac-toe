using Sirenix.OdinInspector;
using UnityEngine;

namespace BB
{
	public sealed class T3Engine : EntityAsset
	{
		protected override void Install(IBinder binder) => binder.T3Engine();
	}
	public static class T3Extensions
	{
		public static void T3Engine(this IBinder binder)
		{
			binder.System<T3Manager>();
			binder.System<GridTeams>();
			binder.System<T3RuleChecker>();
			binder.System<ShowHintOnHover>();
		}
	}
}
