using MessagePipe;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
namespace BB {
	public readonly struct InputPressed
	{
		public readonly InputButton _button;
		public readonly InputData _data;
		public InputPressed(InputButton button, InputData data)
		{
			_button = button;
			_data = data;
		}
	}
	public readonly struct InputReleased
	{
		public readonly InputButton _button;
		public readonly InputData _data;
		public InputReleased(InputButton button, InputData data)
		{
			_button = button;
			_data = data;
		}
	}
	public readonly struct InputUpdated
	{
		public readonly IEnumerable<InputButton> _buttons;
		public readonly InputData _data;
		public InputUpdated(IEnumerable<InputButton> buttons, InputData data)
		{
			_buttons = buttons;
			_data = data;
		}
	}
	public readonly struct InputData
	{
		public readonly float _deltaTime;
		public readonly Vector2 _move;
		public readonly Vector2 _look;
		public Vector3 CameraAlignedMove => CameraUtils.GetCameraAlignedHorizontal(_move);
		public InputData(float deltaTime, Vector2 move, Vector2 camera)
		{
			_deltaTime = deltaTime;
			_move = move;
			_look = camera;
		}
	}
	public sealed record InputManager(
		InputConfig Config,
		IPublisher<InputPressed> Pressed,
		IPublisher<InputReleased> Released,
		IPublisher<InputUpdated> Updated) 
		: EntitySystem, IOnUpdate, IOnInstall, IDisposable
	{
		InputActionMap _map;
		InputAction _moveAction;
		InputAction _lookAction;
		InputData _inputData;

		readonly List<BBInputAction> _actions = new();
		readonly List<InputButton> _activeButtons = new();

		private readonly struct BBInputAction
		{
			public readonly InputButton Button;
			public readonly InputAction Action;
			public readonly Action<CallbackContext> PressDelegate, ReleaseDelegate;
			public BBInputAction(InputButton button, InputAction action, Action<CallbackContext> press, Action<CallbackContext> release)
			{
				Button = button;
				Action = action;
				PressDelegate = press;
				ReleaseDelegate = release;
			}
		}
		public void OnStart()
		{
			_map = Config._asset.FindActionMap(Config._mapName, true).Clone();
			_moveAction = _map[Config._moveInputName];
			_lookAction = _map[Config._lookInputName];
			foreach (var button in Config._buttons)
				AddAction(button);
			_map.Enable();
			void AddAction(InputButton button)
			{
				var a = _map[button.ActionName];
				if (!_actions.TryFind(out var pa, (ra) => ra.Button == button))
				{
					a.started -= pa.PressDelegate;
					a.canceled -= pa.ReleaseDelegate;
					_actions.Remove(pa);
				}
				var newA = new BBInputAction(button, a, GetPressButtonProcessor(button), GetReleaseButtonProcessor(button));
				a.started += newA.PressDelegate;
				a.canceled += newA.ReleaseDelegate;
				_actions.Add(newA);
			}
		}
		public void Dispose()
		{
			_map?.Disable();
		}
		private Action<CallbackContext> GetPressButtonProcessor(InputButton button) => (context) => PressButton(button);
		private Action<CallbackContext> GetReleaseButtonProcessor(InputButton button) => (context) => ReleaseButton(button);
		public void OnUpdate(float deltaTime)
		{
			_inputData = new InputData(deltaTime, _moveAction.ReadValue<Vector2>(), _lookAction.ReadValue<Vector2>());
			Updated.Publish(new(_activeButtons, _inputData));
		}
		private void PressButton(InputButton button)
		{
			_activeButtons.Add(button);
			Pressed.Publish(new(button, _inputData));
		}
		private void ReleaseButton(InputButton button)
		{
			_activeButtons.Remove(button);
			Released.Publish(new(button, _inputData));
		}
	}
}