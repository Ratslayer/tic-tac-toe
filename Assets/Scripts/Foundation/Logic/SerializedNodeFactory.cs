using BB.Logic;
using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace BB.Logic.Serialized
{
	[Serializable]
	public abstract class SerializedNodeFactory : IDiFactory<ILogicNode>
	{
		public abstract ILogicNode Create(IResolver resolver);
	}
}