namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IUnfoldConfig
{
	bool? IgnoreBendTable { get; set; }

	double? DefaultKFactor { get; set; }
}
