using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface ISimulationMultiCreationParameters
{
	Model PartModel { get; }

	double Thickness { get; }

	IMaterialArt Material { get; }

	IBendMachine? BendMachine { get; }

	List<ISimulationBendInfo> Bends { get; }

	List<IToolSetups> ToolSetups { get; }

	ISimulationStepFilter SimulationStepFilter { get; }

	bool IgnoreCollisionsCompletly { get; }
}
