using TMPro;

namespace BB.UI
{
	public static class UiInstallerUtils
	{
		public static void Pointer(this IBinder binder)
		{
			binder.AddComponent<PointerTarget>();
			binder.Event<PointerClicked>();
			binder.Event<PointerPressed>();
			binder.Event<PointerReleased>();
			binder.Event<UiElementSelected>();
			binder.Event<UiElementDeselected>();
			binder.System<BroadcastPointerEvents>();
		}
		public static void Text(this IBinder binder)
		{
			binder.ComponentInChildren<TextMeshProUGUI>();
			binder.Event<SetText>();
			binder.System<ChangeTextOnEvent>();
		}
		sealed record ChangeTextOnEvent(TextMeshProUGUI Text) : EntitySystem
		{
			[Subscribe]
			void OnSetText(SetText msg) => Text.text = msg.Value;
		}
	}
	public sealed record SetText(string Value);
}