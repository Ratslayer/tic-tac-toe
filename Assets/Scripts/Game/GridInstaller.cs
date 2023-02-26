using UnityEngine;
using BB.UI;
using Sirenix.OdinInspector;

namespace BB
{
	public sealed class GridInstaller : AbstractInstallerBehaviour
	{
		public float _lineThickness = 0.1f;
		[Required]
		public GameObject _linePrefab;
		public override void InstallBindings(IBinder binder)
		{
			binder.Component<GridInstaller>();
			binder.System<DrawGrid>();
			binder.System<PublishGridEvents>();
			binder.System<ShowHintOnHover>();
			binder.Pointer();
			binder.Hover();
		}
	}
	public sealed record CellData(int X, int Y);
}
