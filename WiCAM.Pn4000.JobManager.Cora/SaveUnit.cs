using System;

namespace WiCAM.Pn4000.JobManager.Cora;

[Serializable]
public class SaveUnit
{
	public int ProductionStatus { get; set; }

	public string Path { get; set; } = string.Empty;


	public PlateProductionInfo Plate { get; set; }
}
