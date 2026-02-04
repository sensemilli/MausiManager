using System.Windows.Input;
using WiCAM.Pn4000.GuiContracts.EventArgs;

namespace WiCAM.Pn4000.GuiWpf.TabBend;

internal class ShortcutTabBend
{
	public IShortcut ToggleExpandSequence { get; }

	public IShortcut ToggleExpandSubView { get; }

	public ShortcutTabBend()
	{
		ToggleExpandSequence = new ShortcutKey(Key.B);
		ToggleExpandSubView = new ShortcutKey(Key.S);
	}
}
