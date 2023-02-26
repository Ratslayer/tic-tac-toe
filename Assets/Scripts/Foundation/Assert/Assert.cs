using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
namespace BB
{
	public class CustomAssertHandler : AssertHandler
	{
		private Action<string> _action;
		public CustomAssertHandler(Action<string> action) : base(null)
		{
			_action = action;
		}

		public override void Log(UnityEngine.Object _context, string msg, params object[] args)
		{
			_action.Invoke(msg);
		}
	}
	public class GUIBoxHandler : AssertHandler
	{
		private MessageType type;
		public GUIBoxHandler(MessageType type, UnityEngine.Object context = null) : base(context)
		{
			this.type = type;
		}

		public override void Log(UnityEngine.Object context, string msg, params object[] args)
		{
			EditorGUILayout.HelpBox(msg, type);
		}
	}
	public static partial class Assert
	{
		public static readonly AssertHandler GUIError = new GUIBoxHandler(MessageType.Error);
		public static readonly AssertHandler GUIWarning = new GUIBoxHandler(MessageType.Warning);
		public static readonly AssertHandler GUIInfo = new GUIBoxHandler(MessageType.Info);
	}
}
#endif
namespace BB
{
	public static partial class Assert
	{
		public static readonly AssertHandler Error = new DebugLogHandler(Debug.LogErrorFormat);
		public static readonly AssertHandler Warn = new DebugLogHandler(Debug.LogWarningFormat);
		public static readonly AssertHandler Info = new DebugLogHandler(Debug.LogFormat);
		public static readonly AssertHandler Throw = new ThrowExceptionHandler();
		public static readonly AssertHandler Exit = new ExitGameHandler();
		//private static readonly DisableBehaviourHandler _disableBehaviour = new();
		public static AssertHandler Disable(MonoBehaviour mb) => new DisableBehaviourHandler(mb);//_disableBehaviour.SetTargetBehaviour(mb);
		/// <summary>
		/// This is in case the assert handler is undefined and can be null.
		/// This way you don't have to break the logic - the handler still evaluates the condition, it just outputs nothing.
		/// </summary>
		public static readonly AssertHandler DoNothing = new DebugLogHandler(null);
	}
	// public class DoNothingHandler : AssertHandler
	// {
	//     public DoNothingHandler(UnityEngine.Object context = null) : base(context)
	//     {
	//     }

	//     public override void Log(UnityEngine.Object context, string msg, params object[] args)
	//     {
	//     }
	// }
	public class DebugLogHandler : AssertHandler
	{
		public delegate void Logger(UnityEngine.Object context, string msg, params object[] args);
		private Logger _logger;
		public DebugLogHandler(Logger logger) : base(null)
		{
			_logger = logger;
		}
		public override void Log(UnityEngine.Object context, string msg, params object[] args)
		{
			_logger?.Invoke(context, msg, args);
		}
	}
	public class LogErrorHandler : AssertHandler
	{
		public LogErrorHandler(UnityEngine.Object context = null) : base(context) { }
		public override void Log(UnityEngine.Object context, string msg, params object[] args)
		{
			Debug.LogErrorFormat(context, msg, args);
		}
	}
	public class ThrowExceptionHandler : AssertHandler
	{
		public ThrowExceptionHandler(UnityEngine.Object context = null) : base(context) { }
		public override void Log(UnityEngine.Object context, string msg, params object[] args)
		{
			throw new GSException(msg, args);
		}
	}
	public class ExitGameHandler : AssertHandler
	{
		public ExitGameHandler(UnityEngine.Object context = null) : base(context) { }
		public override void Log(UnityEngine.Object context, string msg, params object[] args)
		{
			Debug.LogErrorFormat(context, msg, args);
#if UNITY_EDITOR
			UnityEditor.EditorApplication.isPlaying = false;
#endif
		}
	}
	public class DisableBehaviourHandler : AssertHandler
	{
		public DisableBehaviourHandler(UnityEngine.Object context = null) : base(context)
		{
		}
		public override void Log(UnityEngine.Object context, string msg, params object[] args)
		{
			Debug.LogErrorFormat(context, msg, args);
			if (context is MonoBehaviour mb)
				mb.enabled = false;
		}
	}
	// public class LogWarningHandler : AssertHandler
	// {
	//     public LogWarningHandler(UnityEngine.Object context = null) : base(context) { }
	//     public override void Log(UnityEngine.Object context, string msg, params object[] args)
	//     {
	//         Debug.LogWarningFormat(context, msg, args);
	//     }
	// }
}