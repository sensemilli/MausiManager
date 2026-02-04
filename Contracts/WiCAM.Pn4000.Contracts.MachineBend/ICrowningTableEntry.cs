using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface ICrowningTableEntry
{
	int MaterialGroupId { get; set; }

	double Thickness { get; set; }

	List<double> Values { get; set; }
}
