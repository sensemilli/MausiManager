using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;

namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface ISimulationLiftingAidInfo
{
	bool UseLiftingAid { get; }

	LiftingAidEnum UseLeftFrontLiftingAid { get; }

	LiftingAidEnum UseRightFrontLiftingAid { get; }

	LiftingAidEnum UseLeftBackLiftingAid { get; }

	LiftingAidEnum UseRightBackLiftingAid { get; }

	double? LeftFrontLiftingAidHorizontalPos { get; }

	double? LeftFrontLiftingAidVerticalPos { get; }

	double? LeftFrontLiftingAidRotation { get; }

	double? RightFrontLiftingAidHorizontalPos { get; }

	double? RightFrontLiftingAidVerticalPos { get; }

	double? RightFrontLiftingAidRotation { get; }

	double? LeftBackLiftingAidHorizontalPos { get; }

	double? LeftBackLiftingAidVerticalPos { get; }

	double? LeftBackLiftingAidRotation { get; }

	double? RightBackLiftingAidHorizontalPos { get; }

	double? RightBackLiftingAidVerticalPos { get; }

	double? RightBackLiftingAidRotation { get; }
}
