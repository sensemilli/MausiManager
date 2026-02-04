using System.Windows.Input;
using WiCAM.Pn4000.GuiContracts.EventArgs;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

public class ShortcutEditTools
{
	public IShortcut SelectionModePiece { get; }

	public IShortcut SelectionModeSection { get; }

	public IShortcut SelectionModeGroup { get; }

	public IShortcut Flip { get; }

	public IShortcut RemovePieces { get; }

	public IShortcut MoveLeft { get; }

	public IShortcut MoveRight { get; }

	public IShortcut CenterTools { get; }

	public ShortcutEditTools()
	{
		SelectionModePiece = new ShortcutKey(Key.D1, true);
		SelectionModeSection = new ShortcutKey(Key.D2, true);
		SelectionModeGroup = new ShortcutKey(Key.D3, true);
		Flip = new ShortcutKey(Key.F);
		RemovePieces = new ShortcutKey(Key.Delete);
		MoveLeft = new ShortcutKey(Key.A);
		MoveRight = new ShortcutKey(Key.D);
		CenterTools = new ShortcutKey(Key.C);
	}
}
