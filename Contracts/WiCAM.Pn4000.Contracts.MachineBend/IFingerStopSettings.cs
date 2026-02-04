using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IFingerStopSettings
{
	bool AllowCornerStops { get; set; }

	bool AllowCylinderStops { get; set; }

	double FixedLeftZPosition { get; set; }

	double FixedRightZPosition { get; set; }

	double HemFingerX1 { get; set; }

	double HemFingerX2 { get; set; }

	bool IgnoreBentUpFacesAsStopPosition { get; set; }

	bool IgnoreFingerStopsNextToTools { get; set; }

	bool IgnoreHolesFacesAsStopPosition { get; set; }

	bool IgnoreNoneFlatFacesAsStopPosition { get; set; }

	bool IgnoreNonParallelEdges { get; set; }

	bool IgnoreStability { get; set; }

	bool IgnoreZPositions { get; set; }

	double MinDistanceForVerticalSupport { get; set; }

	double MinEdgeLength { get; set; }

	double MinFingerDistance { get; set; }

	int MinimizeZAxesMovements { get; set; }

	double MinRetractDistance { get; set; }

	double MinRetractThreshold { get; set; }

	double MinXDistanceForHemming { get; set; }

	StopSide PrefConfigurationSide { get; set; }

	int PrefCornerStop { get; set; }

	double PrefDistanceFromCorner { get; set; }

	double PrefRetractSafetyDistance { get; set; }

	double PrefRetractDistance { get; set; }

	int PrefNumberOfCornerStops { get; set; }

	double PrefSafetyDistanceAboveDie { get; set; }

	Vertical PrefSheetFingerstopFaceAlignment { get; set; }

	double PrefSheetFingerstopFaceAlignmentCorrection { get; set; }

	bool RetractWithSameValue { get; set; }

	bool UseHemFingerX { get; set; }

	int VerticalSupport { get; set; }

	List<Pair<double, double>> RetractThresholdByThickness { get; set; }
}
