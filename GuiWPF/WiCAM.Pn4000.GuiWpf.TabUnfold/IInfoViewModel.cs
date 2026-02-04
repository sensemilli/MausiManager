using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.PN3D.Enums;

namespace WiCAM.Pn4000.GuiWpf.TabUnfold;

public interface IInfoViewModel : ISubViewModel
{
	void SetActiveModelType(ModelViewMode newMode);
}
