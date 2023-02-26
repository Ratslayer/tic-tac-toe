using TMPro;

namespace BB.UI
{
	public static class UiInstallerUtils
	{
		public static void Pointer(this IBinder binder)
		{
			binder.AddComponent<PointerTarget>();
			binder.System<BroadcastPointerEvents>();
			binder.Event<PointerClicked>();
			binder.Event<PointerPressed>();
			binder.Event<PointerReleased>();
			binder.Event<UiElementSelected>();
			binder.Event<UiElementDeselected>();
		}
		public static void Hover(this IBinder binder)
		{
			binder.AddComponent<HoverTarget>();
			binder.System<BroadcastHoverEvents>();
			binder.Event<HoverEntered>();
			binder.Event<HoverExited>();
			binder.Event<HoverMoved>();
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