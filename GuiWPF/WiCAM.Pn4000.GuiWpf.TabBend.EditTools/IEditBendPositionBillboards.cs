using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

public interface IEditBendPositionBillboards : ISubViewModel
{
	bool StartSubMenu(object sender, ITriangleEventArgs e);

	void ShowBillboards(Vector3d position);

	void HideBillboards();
}
