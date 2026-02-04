using System;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.Docking;

public interface IClosableView
{
	event Action CloseRequested;

	void ViewClosing()
	{
	}
}
