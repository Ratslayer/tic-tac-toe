using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Threading;

namespace BB
{
	public sealed record CancellationTokenManager : EntitySystem, IOnDespawn
	{
		readonly List<CancellationTokenSource> _tokens = new();
		public CancellationTokenSource CreateToken()
		{
			var token = new CancellationTokenSource();
			_tokens.Add(token);
			return token;
		}
		public void OnDespawn()
		{
			foreach (var token in _tokens)
			{
				token.Cancel();
				token.Dispose();
			}
			_tokens.Clear();
		}
	}
	public readonly struct TimeData
	{
		public readonly float _delay;
		public readonly bool _ignoreTimeScale;
		public TimeData(float delay, bool ignoreTimeScale)
		{
			_delay = delay;
			_ignoreTimeScale = ignoreTimeScale;
		}
		public static implicit operator TimeData(float delay) => new(delay, false);
	}
	public static class AsyncSystemExtensions
	{
		public static CancellationTokenSource CreateCancellationToken(this IEntity entity)
			=> entity.Resolver.Resolve<CancellationTokenManager>().CreateToken();
		public static CancellationTokenSource DelayedInvoke(this IEntity entity, TimeData delay, Action action)
		{
			var token = entity.CreateCancellationToken();
			TimerUtils.DelayedInvoke(delay._delay, action, delay._ignoreTimeScale, token.Token);
			return token;
		}
		public static void Despawn(this IEntity entity, TimeData delay)
			=> entity.DelayedInvoke(delay, entity.Despawn);
		public static void DespawnOnFrameEnd(this IEntity entity)
			=>TimerUtils.InvokeOnFrameEnd(entity.Despawn);
	}
}