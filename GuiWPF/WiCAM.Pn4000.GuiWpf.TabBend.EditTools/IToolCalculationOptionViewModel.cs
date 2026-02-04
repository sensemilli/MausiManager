using WiCAM.Pn4000.GuiContracts.Popups;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

internal interface IToolCalculationOptionViewModel : ISubViewModel, IPopupViewModel
{
	void ImportOptions();
}
