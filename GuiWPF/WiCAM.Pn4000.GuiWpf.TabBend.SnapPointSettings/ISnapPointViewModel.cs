using System;
using WiCAM.Pn4000.GuiContracts.Popups;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.ToolCalculation.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.TabBend.SnapPointSettings;

internal interface ISnapPointViewModel : ISubViewModel, IPopupViewModel
{
	Action OnChanged { get; set; }

	void Init(SnapOptions options);
}
