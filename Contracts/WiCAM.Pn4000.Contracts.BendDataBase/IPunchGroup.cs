namespace WiCAM.Pn4000.Contracts.BendDataBase;

public interface IPunchGroup : IProfileGroup
{
	double Radius { get; set; }

	int PrimaryToolId { get; set; }

	string PrimaryToolName { get; set; }

	IPunchGroup Copy();
}
