using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface INcValues
{
	IReadOnlyCollection<INcBendValues> NcBendValues { get; }

	INcBendValues GetNcBendValues(int order);
}
