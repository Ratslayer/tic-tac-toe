using System;
namespace BB
{
	[Serializable]
	public abstract class SerializedEntityComponent
	{
		public abstract void Append(IResolver resolver);
	}
}