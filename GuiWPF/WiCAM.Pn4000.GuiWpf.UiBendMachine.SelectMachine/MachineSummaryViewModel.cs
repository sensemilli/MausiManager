using WiCAM.Pn4000.Contracts.MachineBend;

namespace WiCAM.Pn4000.GuiWpf.UiBendMachine.SelectMachine;

public class MachineSummaryViewModel
{
	public IBendMachineSummary Summary { get; }

	public int Number => Summary.MachineNo;

	public string Manufacturer => Summary.Manufacturer;

	public string Name => Summary.Name;

	public string PressBrakeDataType => Summary.PressBrakeDataType;

	public string PressBrakeInfoType => Summary.PressBrakeInfoType;

	public string Remark1 => Summary.Remark1;

	public string Remark2 => Summary.Remark2;

	public string MachinePath => Summary.MachinePath;

	public MachineSummaryViewModel(IBendMachineSummary summary)
	{
		Summary = summary;
	}
}
