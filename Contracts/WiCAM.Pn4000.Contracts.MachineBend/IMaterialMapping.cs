namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IMaterialMapping
{
	int MaterialId { get; }

	string? PpId { get; }

	string? PpIdentifier { get; }
}
