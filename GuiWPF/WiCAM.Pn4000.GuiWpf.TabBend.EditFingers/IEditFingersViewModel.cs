using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.GuiContracts.Popups;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditFingers;

public interface IEditFingersViewModel : ISubViewModel, IPopupViewModel
{
	bool IsVisible { get; }

	IFingerStop SelectedFinger { get; set; }

	IFingerStopCombination SelectedFingerCombination { get; set; }

	bool SnapActive { get; set; }

	double RUi { get; set; }

	double XUi { get; set; }

	double ZUi { get; set; }

	void Activate();

	void Dispose();

	void Init();

	void SelectFinger(Model model);
}
