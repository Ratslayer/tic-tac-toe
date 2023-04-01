using UnityEngine;

namespace BB
{
	public interface IGameStyle
	{
		float LineWidth { get; }
		GameObject LinePrefab { get; }
		GameObject ButtonPrefab { get; }
		//GameObject GetHintPrefab(Team team);
		//GameObject GetTilePrefab(Team team);
	}
	public sealed class GameStyle : OverridableData<IGameStyle> { }
	
}
