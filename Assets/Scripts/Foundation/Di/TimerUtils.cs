using Cysharp.Threading.Tasks;
using System;
using System.Threading;

namespace BB
{
	public static class TimerUtils
	{
		public static UniTask Delay(float time, bool ignoreTimeScale = false, CancellationToken token = default) 
			=> UniTask.Delay((int)(time * 1000), ignoreTimeScale, cancellationToken: token);
		public static void DelayedInvoke(float time, Action action, bool ignoreTimeScale = false, CancellationToken token = default)
			=> DelayedInvokeTask(time, action, ignoreTimeScale, token).Forget();
		public static void DelayedInvoke(float time, Action action, bool ignoreTimeScale, ref CancellationTokenSource source)
		{
			source?.Dispose();
			source = new();
			DelayedInvoke(time, action, ignoreTimeScale, source.Token);
		}
		public static async UniTaskVoid DelayedInvokeTask(float time, Action action, bool ignoreTimeScale = false, CancellationToken token = default)
		{
			await Delay(time, ignoreTimeScale, token);
			action?.Invoke();
		}
		public static YieldAwaitable WaitForEndOfFrame() 
			=> UniTask.Yield(PlayerLoopTiming.LastPostLateUpdate);
		public static void InvokeOnFrameEnd(Action action)
		{
			Invoke().Forget();
			async UniTaskVoid Invoke()
			{
				await WaitForEndOfFrame();
				action?.Invoke();
			}
		}
	}
}