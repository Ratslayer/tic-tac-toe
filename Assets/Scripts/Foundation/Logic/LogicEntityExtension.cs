using BB.Logic.Serialized;
using UnityEngine;

namespace BB
{
	public sealed class LogicEntityExtension : AbstractEntityExtension
	{
		[SerializeField]
		SerializedLogic _logic = new();
		public override void Append(IResolver resolver)
			=> _logic.Append(resolver);
	}
}