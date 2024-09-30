using System;
using System.Collections.Generic;

namespace WiCAM.Pn4000.JobManager;

public class ReadStrategyInfo
{
	public Type DatType { get; set; }

	public List<ReadPropertyInfo> Properties { get; set; } = new List<ReadPropertyInfo>();

}
