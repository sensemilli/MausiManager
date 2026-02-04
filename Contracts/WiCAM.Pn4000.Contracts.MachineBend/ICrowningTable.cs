using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface ICrowningTable
{
	IEnumerable<double> Lengths { get; }

	IEnumerable<ICrowningTableEntry> Entries { get; }

	double GetInterpolatedValue(int mat, double thickness, double length);

	void SetData(IEnumerable<double> lengths, IEnumerable<ICrowningTableEntry> entries);
}
