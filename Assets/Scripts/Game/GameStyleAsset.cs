using Sirenix.OdinInspector;
using UnityEngine;

namespace BB
{
	public sealed class GameStyleAsset : AbstractStateComponent, IGameStyle
	{
		[SerializeField]
		float _lineThickness = 3f;
		[SerializeField, Required]
		GameObject _linePrefab, _buttonPrefab, _xPrefab, _oPrefab, _xHintPrefab, _oHintPrefab;

		public float LineWidth => _lineThickness;

		public GameObject LinePrefab => _linePrefab;
		public GameObject ButtonPrefab => _buttonPrefab;
		public override IDataOverride Create(IResolver resolver)
			=> new DataOverride<GameStyle, IGameStyle>(this);
		//public GameObject GetHintPrefab(Team team)
		//		=> GetPrefab(team, _xHintPrefab, _oHintPrefab);
		//public GameObject GetTilePrefab(Team team)
		//	=> GetPrefab(team, _xPrefab, _oPrefab);
		//GameObject GetPrefab(Team team, GameObject x, GameObject o) => team == Team.X ? x : o;
	}

}
