using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditFingers;

internal interface IEditFingersVisualizer : ISubViewModel
{
	IEditFingersViewModel EditFingersViewModel { get; }

	bool StartSubMenu(object sender, ITriangleEventArgs e);
}
