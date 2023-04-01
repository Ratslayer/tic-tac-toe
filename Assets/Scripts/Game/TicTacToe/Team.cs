using UnityEngine;

namespace BB
{
	//public enum Team
	//{
	//	None = 0,
	//	X = 1,
	//	O = 2
	//}
	public sealed class Team : BaseScriptableObject
	{
		public string _name;
		public GameObject _icon, _hint;
	}
}
