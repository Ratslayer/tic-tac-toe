using Sirenix.OdinInspector;
using System;
using System.Collections.Generic;
using UnityEngine;
namespace BB
{
	public sealed class State : AbstractStateProvider
	{
		[SerializeReference]
		List<States.Serialized.SerializedState> _states = new();
		[SerializeField]
		List<AbstractStateProvider> _inherit = new();
		public override IEnumerable<IDiFactory<IDataOverride>> GetStates()
		{
			foreach (var state in _inherit)
				if (state)
					foreach (var data in state.GetStates())
						yield return data;
			foreach (var state in _states)
				if (state != null)
					yield return state;
		}
	}
	namespace States.Serialized
	{
		[Serializable]
		public abstract class SerializedState : IDiFactory<IDataOverride>
		{
			public abstract IDataOverride Create(IResolver resolver);
		}
		[Serializable]
		public abstract class SerializedState<TContract, TValue> : SerializedState
			where TContract : OverridableData<TValue>
		{
			[SerializeField, Required]
			TValue _value;
			public override IDataOverride Create(IResolver resolver)
				=> new DataOverride<TContract, TValue>(_value);
		}
	}
}