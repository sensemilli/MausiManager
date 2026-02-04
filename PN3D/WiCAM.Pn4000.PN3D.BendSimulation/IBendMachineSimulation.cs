using System;
using System.Collections.Generic;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Interfaces;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.BendPP;
using WiCAM.Pn4000.Contracts.BendSimulation;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.PN3D.Doc;

namespace WiCAM.Pn4000.PN3D.BendSimulation;

public interface IBendMachineSimulation : IBendMachineSimulationBasic
{
	new BendMachine BendMachine { get; set; }

	IBendmachineDepricated IBendMachineSimulationBasic.BendMachine => this.BendMachine;

	IBendTable BendTable { get; }

	IMachineHelper MachineHelper { get; }

	IPostProcessor PostProcessor { get; }

	event Action<ISimulationThread, ISimulationThread> SimulationChangedEvent;

	IBendMachineSimulation Init(string path, Dictionary<ICombinedBendDescriptorInternal, IPreferredProfile> preferredProfilesDict, IDoc3d doc, out bool loadingError);

	void CalculateBendSteps(Model part, List<IBendPositioning> bendPositionings, bool calculateFingerPos, bool backToStart = true, bool toolConfigActive = false);
}
