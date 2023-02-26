using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
namespace BB
{
	public enum AssertType
	{
		None,
		Error,
		Throw,
		Exit
	}
	public abstract class AssertHandler : NullIsFalse
	{
		private UnityEngine.Object _context;
		public abstract void Log(UnityEngine.Object _context, string msg, params object[] args);
		public void Log(string msg, params object[] args)
		{
			Log(_context, msg, args);
		}
		public AssertHandler(UnityEngine.Object context)
		{
			_context = context;
		}
		public AssertHandler SetContext(UnityEngine.Object context)
		{
			_context = context;
			return this;
		}
		private bool FailIf(Func<bool> asserter, string msg, params object[] args)
			=> FailIf(asserter(), msg);
		private bool FailIf(bool value, string msg)
		{
			if (value)
				Log(_context, msg);
			return !value;
		}
		public static AssertHandler GetHandler(AssertType type)
		{
			var result = default(AssertHandler);
			switch (type)
			{
				case AssertType.Error:
					result = Assert.Error;
					break;
				case AssertType.Exit:
					result = Assert.Exit;
					break;
				case AssertType.Throw:
					result = Assert.Throw;
					break;
				case AssertType.None:
					result = Assert.DoNothing;
					break;
			}
			return result;
		}
		public T GetComponentInParent<T>(ref bool success) where T : class
		{
			T result = default;
			if (_context is Component comp)
			{
				result = comp.GetComponentInParent<T>();
				if (!NotNull(result, $"{comp.name} does not have a parent with component of type {typeof(T)}."))
					success = false;
			}
			return result;
		}
		public bool Null(object obj, string msg, params object[] args)
		{
			return FailIf(() => obj != null, msg, args);
		}
		public bool NotNull(object obj, string msg, params object[] args)
		{
			return FailIf(() => obj == null, msg, args);
		}
		public bool Assigned(params UnityEngine.Object[] objects)
		{
			return FailIf(()
				=> objects.Contains(obj => !obj), $"Critical references are unassigned.");
		}
		public bool ValidReference(params object[] objects) => FailIf(()
			 => objects.Contains(obj => obj is UnityEngine.Object ueo && ueo || obj != null), $"Critical references are unassigned.");
		public bool Unassigned(UnityEngine.Object obj, string msg) => FailIf(() => obj, msg);
		public bool Assigned(UnityEngine.Object obj, string message) => FailIf(() => !obj, message);
		public bool OfType<T>(object obj, string msg, params object[] args)
		{
			return FailIf(() => !(obj is T), msg, args);
		}
		public bool False(bool value, string msg, params object[] args)
		{
			return FailIf(() => value, msg, args);
		}
		public bool NotNOE(string str, string msg, params object[] args)
		{
			return FailIf(() => str.NoE(), msg, args);
		}
		public bool True(bool value, string msg, params object[] args)
		{
			return FailIf(() => !value, msg, args);
		}
		public bool NotEmpty<T>(IEnumerable<T> value, string msg, params object[] args)
		{
			return FailIf(() => value.Count() == 0, msg, args);
		}
		public bool Approximately(float f1, float f2, string msg, params object[] args)
		{
			return FailIf(() => !Mathf.Approximately(f1, f2), msg, args);
		}
		public bool DoesNotContain<T1, T2>(IEnumerable<T1> container, T2 value, string msg)
			where T2 : T1
			=> FailIf(() => container.Contains(value), msg);
		public bool Contain<T1, T2>(IEnumerable<T1> container, T2 value, string msg)
			where T2 : T1
			=> FailIf(() => !container.Contains(value), msg);
		public bool LengthNot<T>(T[] value, int length, string msg, params object[] args)
		{
			return FailIf(() => value.Length == length, msg, args);
		}
		public bool LengthIs<T>(T[] value, int length, string msg, params object[] args)
		{
			return FailIf(() => value.Length != length, msg, args);
		}
		public bool SameLength<T1, T2>(IEnumerable<T1> left, IEnumerable<T2> right, string msg, params object[] args)
		{
			return FailIf(() => left.Count() != right.Count(), msg, args);
		}
		public bool ValidIndex<T>(IEnumerable<T> collection, int index, string msg, params object[] args)
		{
			return FailIf(() => index < 0 || index >= collection.Count(), msg, args);
		}
		public bool ValidIndex<T>(IEnumerable<T> collection, int index, out T value, string msg, params object[] args)
		{
			var result = ValidIndex(collection, index, msg, args);
			value = !result ? collection.ElementAt(index) : default;
			return result;
		}
		public bool DoesNotInherit<T>(Type type, string message)
		{
			return FailIf(() => typeof(T).IsAssignableFrom(type), message);
		}
		public bool Inherits<T>(Type type, string message)
		{
			return FailIf(() => !typeof(T).IsAssignableFrom(type), message);
		}
		public bool Is<T>(object obj, out T castObj, string message) where T : class
		{
			castObj = obj as T;
			return NotNull(castObj, message);
		}
		public void Rethrow(Exception exception)
		{
			var message = $"{exception.GetType()} rethrown: {exception.Message}\n{exception.StackTrace}\n\n";
			Log(_context, message);
		}
	}
}