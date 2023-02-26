using System;
using UnityEngine;

namespace BB
{
	public interface ITimeScaleConfig
	{
		float DefaultFixedDelta { get; }
	}
	public sealed record TimeScale(ITimeScaleConfig Config) : EntitySystem
	{
		public void SetValue(float value)
		{
			Time.timeScale = value;
			if (value > 0f)
				Time.fixedDeltaTime = Config.DefaultFixedDelta * value;
			else Time.fixedDeltaTime = 1e6f;
		}
	}
	//namespace States.Serialized
	//{
	//	[Serializable]
	//	public sealed class TimeScale : SerializedState<BB.TimeScale, float> { }
	//}
}
