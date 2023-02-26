using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

namespace BB
{
	public sealed class StateAsset : AbstractStateProvider
	{
		[SerializeField, FormerlySerializedAs("_states")]
		private List<AbstractStateComponent> _components = new();
		[SerializeField]
		private List<StateAsset> _inherit = new();
		public override IEnumerable<IDiFactory<IDataOverride>> GetStates()
		{
			foreach (var state in _inherit)
				if (state)
					foreach (var data in state.GetStates())
						yield return data;
			foreach (var state in _components)
				if (state)
					foreach (var data in state.GetStates())
						yield return data;
		}
	}
	public abstract class AbstractStateProvider : BaseScriptableObject, IStateProvider
	{
		public abstract IEnumerable<IDiFactory<IDataOverride>> GetStates();
	}
	public abstract class AbstractStateProvider<TContract, TValue> : AbstractStateComponent
		where TContract : OverridableData<TValue>
	{
		[SerializeField]
		protected TValue _value;
		public override IDataOverride Create(IResolver resolver)
			=> new DataOverride<TContract, TValue>(_value);
	}
	public abstract class AbstractStateComponent : AbstractStateProvider, IDiFactory<IDataOverride>
	{
		public abstract IDataOverride Create(IResolver resolver);
		public override IEnumerable<IDiFactory<IDataOverride>> GetStates()
		{
			yield return this;
		}
	}
}