using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

internal interface IEditToolsVisualizer : ISubViewModel
{
	void VisualizeTools();

	bool StartSubMenu(object sender, ITriangleEventArgs e);
}
