using UnityEngine;
using BB.UI;
using Sirenix.OdinInspector;

namespace BB
{
	public sealed class GridInstaller : AbstractInstallerBehaviour
	{
		public float _lineThickness = 0.1f;
		[Required]
		public GameObject _linePrefab, _hintPrefab;
		public override void InstallBindings(IBinder binder)
		{
			binder.Component<GridInstaller>();
			binder.System<DrawGrid>();
			binder.System<ShowHintOnHover>();
			binder.System<PublishGridEvents>();
			binder.Pointer();
			binder.Hover();
		}
	}
	public sealed record ShowHintOnHover(
		GridInstaller Ui,
		IGrid Grid,
		GameObjectPools Pools) : EntitySystem
	{
		IEntity _instance;
		[Subscribe]
		void OnHover(HoverCellEvent msg)
		{
			if (msg.Cell == null)
			{
				_instance?.Despawn();
				_instance = null;
				return;
			}
			if (_instance == null)
			{
				_instance = Pools.Spawn(Ui._hintPrefab, new(Entity.GetTransform()));
				var cellSize = Grid.GetCellSize();
				_instance.GetTransform().localScale = new Vector3(cellSize, cellSize, 1f);
			}
			_instance.GetTransform().position = Grid.GetCellCenter(msg.Cell);
		}
		[Subscribe]
		void OnClick(ClickedCellEvent msg)
		{
			if (msg.Cell == null)
				return;
			var click = Pools.Spawn(Ui._hintPrefab, new(Entity.GetTransform()));
			var cellSize = Grid.GetCellSize();
			click.GetTransform().localScale = new Vector3(cellSize, cellSize, 1f);
			click.GetTransform().position = Grid.GetCellCenter(msg.Cell);
		}
	}
	public sealed record CellData(int X, int Y);
}
