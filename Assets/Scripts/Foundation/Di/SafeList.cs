using System;
using System.Collections;
using System.Collections.Generic;
namespace BB
{
	public interface IBaseEnumerable<T> : IEnumerable<T>
	{
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}
	public abstract class AbstractSafeList<StoredType, IdType> : IBaseEnumerable<StoredType>
	{
		protected readonly List<StoredType> _elements = new();
		protected readonly List<IdType> _newElements = new(), _deadElements = new();
		protected abstract void UpdateElementsInternal();
		protected abstract bool IsContainedInElements(IdType element);
		public IEnumerator<StoredType> GetEnumerator()
		{
			UpdateElements();
			return _elements.GetEnumerator();
		}
		private void UpdateElements()
		{
			UpdateElementsInternal();
			_newElements.Clear();
			_deadElements.Clear();
		}
		public void Clear()
		{
			_elements.Clear();
			_newElements.Clear();
			_deadElements.Clear();
		}
		public void ProcessAndClearAllElements(Action<StoredType> action)
		{
			while (!Empty())
			{
				foreach (var e in _elements)
					action(e);
				_elements.Clear();
			}
		}
		public bool Empty()
		{
			UpdateElements();
			return _elements.Count == 0;
		}
		public void Add(IdType element)
		{
			if (_deadElements.Contains(element))
				_deadElements.Remove(element);
			else if (!_newElements.Contains(element) && !IsContainedInElements(element))
				_newElements.Add(element);
		}
		public void Add(IEnumerable<IdType> elements)
		{
			foreach (var element in elements)
				Add(element);
		}
		public void Remove(IdType element)
		{
			if (_newElements.Contains(element))
				_newElements.Remove(element);
			else if (!_deadElements.Contains(element) && IsContainedInElements(element))
				_deadElements.Add(element);
		}
		public void Remove(IEnumerable<IdType> elements)
		{
			foreach (var e in elements)
				Remove(e);
		}
	}
	public class SafeList<T> : AbstractSafeList<T, T>
	{
		protected override bool IsContainedInElements(T element) => _elements.Contains(element);
		protected override void UpdateElementsInternal()
		{
			_elements.AddRange(_newElements);
			_elements.RemoveRange(_deadElements);

		}
	}
	public abstract class AbstractComplexSafeList<MainType, IdType> : AbstractSafeList<MainType, IdType>
		where MainType : class
		where IdType : class
	{
		protected abstract MainType CreateMainFromId(IdType e);
		protected abstract IdType GetIdFromMain(MainType e);
		protected override bool IsContainedInElements(IdType element) => _elements.Contains(item => GetIdFromMain(item) == element);
		protected override void UpdateElementsInternal()
		{
			foreach (var e in _newElements)
				_elements.Add(CreateMainFromId(e));
			_elements.RemoveAll(e => _deadElements.Contains(GetIdFromMain(e)));
		}
	}
}