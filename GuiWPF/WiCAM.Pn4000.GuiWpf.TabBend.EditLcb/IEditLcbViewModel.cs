using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.GuiContracts.Popups;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditLcb;

internal interface IEditLcbViewModel : ISubViewModel, IPopupViewModel
{
	bool StartSubMenu(object sender, ITriangleEventArgs e);

	void SelectModel(Model model);
}
