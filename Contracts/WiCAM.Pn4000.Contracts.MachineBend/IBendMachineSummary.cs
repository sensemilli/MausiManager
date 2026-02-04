namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IBendMachineSummary
{
	string Name { get; }

	string NCName { get; set; }

	int MachineNo { get; }

	string MachinePath { get; }

	string Manufacturer { get; }

	string Remark1 { get; }

	string Remark2 { get; }

	string PressBrakeDataType { get; }

	string PressBrakeInfoType { get; }
}
