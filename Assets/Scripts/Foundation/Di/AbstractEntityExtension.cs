using System.Collections.Generic;

namespace BB
{
	public abstract class AbstractEntityExtension : BaseScriptableObject, IEntityExtension
	{
		public abstract void Append(IResolver resolver);
	}
	public interface IEntityExtension
	{
		void Append(IResolver resolver);
	}
	public sealed record AddExtensions(IEnumerable<IEntityExtension> Extensions)
		: EntitySystem, IOnStart
	{
		public void OnStart()
		{
			foreach (var extension in Extensions)
				extension.Append(Resolver);
		}
	}
	public static class EntityExtensionExtensions
	{
		public static void Append(this IResolver resolver, IEnumerable<IEntityExtension> extensions)
		{
			foreach (var extension in extensions)
				resolver.Append(extension);
		}
	}
}