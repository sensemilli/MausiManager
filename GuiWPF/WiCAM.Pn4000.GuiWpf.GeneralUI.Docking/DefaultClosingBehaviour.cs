using System;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.Docking;

[Flags]
public enum DefaultClosingBehaviour
{
	Never = 0,
	OnFocusLost = 1,
	OnScopeChanged = 2,
	OnContextChanged = 4
}
