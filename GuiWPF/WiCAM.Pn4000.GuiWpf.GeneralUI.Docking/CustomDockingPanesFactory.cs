using Telerik.Windows.Controls;
using Telerik.Windows.Controls.Docking;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.Docking;

internal class CustomDockingPanesFactory : DockingPanesFactory
{
	private readonly DockingContainerXaml _dockingContainer;

	public CustomDockingPanesFactory(DockingContainerXaml dockingContainer)
	{
		_dockingContainer = dockingContainer;
	}

	protected override void AddPane(RadDocking radDocking, RadPane pane)
	{
		pane.TitleTemplate = _dockingContainer.GetDataTemplate("MyTitleTemplate");
		base.AddPane(radDocking, pane);
	}
}
