using Sirenix.OdinInspector;
using System;
using TMPro;
using UnityEngine;

namespace BB.UI
{
	public sealed class ButtonInstaller : AbstractInstallerBehaviour
	{
		[SerializeField, Required]
		TextMeshProUGUI _nameText;
		public override void InstallBindings(IBinder binder)
		{
			binder.Data(this);
			binder.Pointer();
			binder.Event<InitButton>();
			binder.System<ButtonSystem>();
		}
		sealed record ButtonSystem(ButtonInstaller Installer) : EntitySystem
		{
			Action _onClick;
			[Subscribe]
			void Init(InitButton msg)
			{
				Installer._nameText.text = msg.Name;
				_onClick = msg.OnClick;
			}
			[Subscribe]
			void OnClick(PointerClicked msg)
			{
				_onClick?.Invoke();
			}
		}
	}
	public sealed record InitButton(string Name, Action OnClick);
}
