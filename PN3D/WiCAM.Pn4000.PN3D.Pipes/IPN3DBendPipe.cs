using System;
using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendSimulation;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.pn4.Interfaces;

namespace WiCAM.Pn4000.PN3D.Pipes;

public interface IPN3DBendPipe
{
	void SelectMachineRecentlyUsed(string name, string fullPath, int machineNumber, IDoc3d doc);

	F2exeReturnCode SelectBendMachine(string machineNumber, IDoc3d doc);

	F2exeReturnCode SelectBendMachineById(int id, IDoc3d doc);

	F2exeReturnCode AutoBend(IDoc3d doc);

	F2exeReturnCode SetBendMachine(bool usePreferredTools, Dictionary<ICombinedBendDescriptorInternal, IPreferredProfile> preferredProfiles, IDoc3d doc);

	F2exeReturnCode SetFingers(IDoc3d doc);

	F2exeReturnCode CalculateSimulation(IDoc3d doc, bool calculateFingerPos, bool backToStart = true);

	void CheckCalcSimulation(IDoc3d doc, Action actionBefore = null, Action actionAfter = null);

	void BendProgrammableVariables(IDoc3d doc, IMainWindowDataProvider mainWindowDataProvider);

	F2exeReturnCode CheckDocProgress(IDoc3d doc, DocState requiredState, bool showErrorMessage);

	void SetBendMachineForVisualization(IDoc3d doc, IGlobals globals, IMainWindowDataProvider mainWindowDataProvider);

	F2exeReturnCode ValidateSimulation(bool visible, IDoc3d doc, bool AutoContinueIfOk);

	ISimulationThread CreateNewValidationSim(IDoc3d doc);

	List<ICombinedBendDescriptor> ValidateSimulationToolSafetyWarningBends(double safetyDistance, ISimulationThread sim, IDoc3d doc);
}
