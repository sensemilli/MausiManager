using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;

namespace WiCAM.Pn4000.PN3D.Interfaces;

public interface ITab
{
	ISubViewModel ActiveSubViewModel { get; set; }

	void SetActive(bool active);

	void RefreshScreen();
}
