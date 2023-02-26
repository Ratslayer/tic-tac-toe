using System;

namespace BB
{
	public interface ITimedActor
	{
		TimeType TimeType => TimeType.Game;
	}
	public interface IOnUpdate : ITimedActor
	{
		void OnUpdate(float deltaTime);
	}
	public interface IOnFixedUpdate : ITimedActor
	{
		void OnFixedUpdate(float deltaTime);
	}
	public interface IOnLateUpdate : ITimedActor
	{
		void OnLateUpdate(float deltaTime);
	}
	public interface IPolledActor : ITimedActor
	{
		float PollRate
		{
			get;
		}
		void Poll(float deltaTime);
	}
	public interface ITickSystem
	{
		void Register(ITimedActor obj);
		void Unregister(ITimedActor obj);
		void InvokeAfterTick(Action action);
	}
}