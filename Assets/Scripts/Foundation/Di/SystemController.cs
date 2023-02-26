using System.Collections.Generic;
using System.Linq;
using Zenject;

namespace BB
{
	public abstract class AbstractSystemStack
	{
		readonly List<StackedSystem> _stack = new();
		public void Add(StackedSystem system)
		{
			_stack.Add(system);
			UpdateActive();
		}
		public void Remove(StackedSystem system)
		{
			_stack.Remove(system);
			system.RegisterOnStack(false);
			UpdateActive();
		}
		void UpdateActive()
		{
			_stack.SortByPriority();
			if (_stack.Count == 0)
				return;
			_stack[^1].RegisterOnStack(true);
			if (_stack.Count == 1)
				return;
			for (int i = 0; i < _stack.Count - 1; i++)
				_stack[i].RegisterOnStack(false);
		}
	}
	public abstract record StackedSystem : EntitySystem, IPriority
	{
		public int Priority => 0;
		public bool Active { get; private set; }
		public void RegisterOnStack(bool value) => Active = value;
		public abstract void SetActive(bool value);
	}
	public abstract record StackedSystem<TStack> : StackedSystem
		where TStack : AbstractSystemStack
	{
		TStack _stack;
		bool _enabled;
		protected virtual bool AddAutomatically => false;
		[Inject]
		void RegisterInStack(TStack stack)
		{
			_stack = stack;
			if (AddAutomatically)
				SetActive(true);
		}
		public override void SetActive(bool value)
		{
			if (value == _enabled)
				return;
			if (value)
				_stack.Add(this);
			else _stack.Remove(this);
			_enabled = value;
		}
	}
}