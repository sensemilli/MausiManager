using System.Collections.Generic;

namespace WiCAM.Pn4000.Contracts.Tools;

public interface ISensorDisk
{
	int Id { get; }

	IPunchProfile PunchProfile { get; }

	ISensorDiskProfile Disk1 { get; }

	ISensorDiskProfile Disk2 { get; }

	string TypeName { get; set; }

	int Type { get; set; }

	List<ISensorDiskMinFlangeLength> MinFlangeLength { get; set; }

	List<ISensorDiskMeasuringRange> MeasuringRanges { get; set; }
}
