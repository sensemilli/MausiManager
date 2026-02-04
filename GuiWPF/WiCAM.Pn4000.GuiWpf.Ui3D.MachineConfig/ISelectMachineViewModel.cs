using System;
using System.Collections.Generic;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public interface ISelectMachineViewModel
{
	bool UseDefaultTools { get; set; }

	ISelectMachineViewModel Init(IDoc3d doc, IEnumerable<BendMachine> machines, IEnumerable<IBendMachineSummary> machineSummaries, Action<ISelectMachineViewModel> closeAction = null);

	Dictionary<ICombinedBendDescriptorInternal, IPreferredProfile> GetPreferedToolsForCombinedBends();

	void Dispose();

	IBendMachineSummary GetSelectedMachine();
}
