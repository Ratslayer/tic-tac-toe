using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace BB
{
	public sealed class InputConfig : BaseScriptableObject
	{
		[Required]
		public InputActionAsset _asset;
		public string _mapName = "Default";
		public string _moveInputName = "Move";
		public string _lookInputName = "Look";
		public List<InputButton> _buttons = new();
	}
}