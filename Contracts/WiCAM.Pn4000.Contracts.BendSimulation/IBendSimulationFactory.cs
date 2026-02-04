using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;

namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface IBendSimulationFactory
{
	ISimulationThread CreateSimulation(ISimulationCreationParameters creationParameters);

	ISimulationThread CreateSimulation(ISimulationMultiCreationParameters creationParameters);

	ITransformState CreateTransformStateFlat(IEnumerable<ISimulationBendInfo> allBends, Model part);
}
