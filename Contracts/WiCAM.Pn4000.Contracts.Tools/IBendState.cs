namespace WiCAM.Pn4000.Contracts.Tools;

public interface IBendState
{
	int FaceGroupId { get; set; }

	double Angle { get; set; }

	double KFactor { get; set; }

	double FinalRadius { get; set; }
}
