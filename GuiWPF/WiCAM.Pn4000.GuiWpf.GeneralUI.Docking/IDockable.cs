using System;
using System.ComponentModel;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.Docking;

public interface IDockable
{
	string Header { get; }

	event Action CloseView;

	void ViewClosing(object? sender, CancelEventArgs e);
}
