namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IToolSystem
{
	int IDClampSystemInToolDatabase { get; set; }

	string NameInPNBEND { get; set; }

	string NameOrNumberInController { get; set; }

	string Dimension { get; set; }

	string ReferenceToGeometryFile { get; set; }

	double StartPositionForPlacingTools { get; set; }

	double EndPositionForPlacingTools { get; set; }

	double Length { get; set; }

	double AccuracyToPositionTools { get; set; }

	int DirectionOfToolPositions { get; set; }

	int NumberOfClampParts { get; set; }

	int WorkingHeight { get; set; }
}
