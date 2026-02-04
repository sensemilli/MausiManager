using System.Windows.Input;
using WiCAM.Pn4000.GuiContracts.EventArgs;

namespace WiCAM.Pn4000.GuiWpf.EventTokens;

internal class ShortcutSettingsCommon : IShortcutSettingsCommon
{
	public IShortcut Cancel { get; }

	public IShortcut Commit { get; }

	public ShortcutSettingsCommon()
	{
		Cancel = new ShortcutKey(Key.Escape);
		Commit = new ShortcutKey(Key.Return);
	}
}
