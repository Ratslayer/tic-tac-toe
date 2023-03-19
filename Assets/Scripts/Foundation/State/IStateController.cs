using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Zenject;

namespace BB
{
	public interface IStateController
	{
		void Enter(IMachine machine, IStateProvider source);
	}

	public interface IStateProvider
	{
		IEnumerable<IDiFactory<IDataOverride>> GetStates();
	}
	public interface IMachine { }
	public sealed class ConstMachine : IMachine { }
	public sealed record OverrideController(
		IStateProvider DefaultState,
		StateContainer Container)
		: EntitySystem, IStateController, IOnStart
	{
		private class DefaultMachine : IMachine { }
		private static readonly DefaultMachine DEFAULT_MACHINE = new();
		private class Slot
		{
			private readonly IOverridableData _data;
			private readonly List<(IMachine machine, IDataOverride component)> _components = new();
			private IDataOverride _component;
			public Slot(IOverridableData data)
			{
				_data = data;
			}
			public void Set(IMachine machine, IDataOverride value)
			{
				Unset(machine);
				_components.Add((machine, value));
			}
			public void Unset(IMachine machine) => _components.Remove(i => i.machine == machine);
			public void TriggerChanged()
			{
				var newComponent = _components.Count > 0 ? _components.LastOrDefault().component : default;
				if (Equals(_component, newComponent))
					return;
				_component = newComponent;
				if (_component != null)
					_component.ApplyValue(_data);
				else _data.ResetToInitialValue();
			}
		}
		private readonly Dictionary<Type, Slot> _slots = new();
		private readonly Dictionary<IMachine, IStateProvider> _states = new();
		private readonly List<Slot> _dirtySlots = new();
		public void Enter(IMachine machine, IStateProvider source)
			=> ChangeStates(machine, source);
		private void ChangeStates(IMachine machine, IStateProvider to)
		{
			//copy values
			machine ??= DEFAULT_MACHINE;
			if (_states.TryGetValue(machine, out var from))
				_states[machine] = to;
			else
			{
				from = null;
				_states.Add(machine, to);
			}
			//update states
			_dirtySlots.Clear();
			if (from != null)
				foreach (var factory in from.GetStates())
				{
					var state = Container.GetOrCreate(factory);
					if (HasOrCreateSlot(state, out var slot))
					{
						slot.Unset(machine);
						_dirtySlots.AddUnique(slot);
					}
				}
			if (to != null)
				foreach (var factory in to.GetStates())
				{
					var state = Container.GetOrCreate(factory);
					if (HasOrCreateSlot(state, out var slot))
					{
						slot.Set(machine, state);
						_dirtySlots.AddUnique(slot);
					}
				}
			//update stacks
			foreach (var stack in _dirtySlots)
				stack.TriggerChanged();
		}
		private bool HasOrCreateSlot(IDataOverride state, out Slot stack)
		{
			stack = default;
			if (state == null)
				return false;
			//if stack exists
			var type = state.DataType;
			if (_slots.TryGetValue(type, out stack))
				return true;
			//check
			if (Resolver.TryResolve(type, out var data))
			{
				if (data is not IOverridableData d)
				{
					Debug.LogError($"{type.Name} is overriding {data.GetType().Name} " +
						$"which does not inherit IData");
					return false;
				}
				stack = new Slot(d);
				_slots.Add(type, stack);
				return true;
			}
			else
			{
				Debug.LogError($"No {type.Name} data found to override.");
				return false;
			}
			//foreach (var gt in type.GetInterfaces())
			//	if (gt.IsGenericType && gt.GetGenericTypeDefinition() == dataType)
			//	{
			//		var stateType = gt.GetGenericArguments()[0];
			//		var stackType = typeof(Link<>).MakeGenericType(stateType);
			//		stack = _container.Instantiate(stackType) as ISlotLink;
			//		return true;
			//	}
			//return false;
		}

		public void OnStart()
		{
			Enter(DEFAULT_MACHINE, DefaultState);
		}
	}
	//public static class StateControllerExtensions
	//{

	//	public static void StateMachine(this DiContainer container, IStateProvider provider = null)
	//	{
	//		container.Bind<IStateController>().To<StateController>().AsSingle().OnInstantiated<IStateController>(Instatiate).NonLazy();
	//		void Instatiate(InjectContext context, IStateController controller)
	//		{
	//			if (provider != null)
	//				controller.Enter(DEFAULT_MACHINE, provider);
	//		}
	//	}
	//}
}