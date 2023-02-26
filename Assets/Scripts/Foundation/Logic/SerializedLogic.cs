using System;
using System.Collections.Generic;
using UnityEngine;

namespace BB.Logic.Serialized
{
	[Serializable]
	public sealed class SerializedLogic
	{
		[SerializeReference]
		List<SerializedEntityComponent> _components = new();
		public void Append(IResolver resolver)
		{
			try
			{
				foreach (var component in _components)
					component?.Append(resolver);
			}
			catch (Exception ex)
			{
				InstallerUtils.LogInstallException(ex, resolver);
			}
		}
	}
}
