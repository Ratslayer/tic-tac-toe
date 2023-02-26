using System;

namespace BB
{
	public sealed class VolumeEvents : AbstractLogicInstallerBehaviour<VolumeEvents, Logic.Serialized.SerializedVolumeEvents>
	{
		public override void InstallBindings(IBinder binder)
		{
			base.InstallBindings(binder);
			binder.Trigger();
		}
	}
	namespace Logic.Serialized
	{
		public interface IVolumeTrigger : ISerializedTrigger { }
		[Serializable]
		public sealed class SerializedVolumeEvent : AbstractEvent<IVolumeTrigger, SerializedActions> { }
		[Serializable]
		public sealed class SerializedVolumeEvents : SerializedEvents<SerializedVolumeEvent> { }
		[Serializable]
		public sealed class OnVolumeEnter : SerializedTrigger<TriggerEntered>, IVolumeTrigger { }
	}
}