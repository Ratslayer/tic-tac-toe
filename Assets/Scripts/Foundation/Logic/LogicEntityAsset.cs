using BB.Logic.Serialized;
using UnityEngine;
namespace BB
{
	public abstract class LogicEntityAsset : AbstractSpawnAsset
	{
		[SerializeField]
		SerializedLogic _logic = new();
		protected override IEntity CreateEntity(IResolver parent)
		{
			var entity = parent.CreateChild(name, null);
			_logic.Append(entity.Resolver);
			return entity;
		}
	}
}
