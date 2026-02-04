using System;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

public interface IGenerateMachineViewModel
{
	void Init(BendMachine machine, IDoc3d doc, Action<IGenerateMachineViewModel> closeAction);

	void Dispose();
}
