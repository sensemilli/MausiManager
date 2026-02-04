using System.Collections.Generic;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SCombinedBendDescriptor
{
	public List<int> BendDescriptors { get; set; } = new List<int>();

	public int Order { get; set; }

	public bool IsIncluded { get; set; }

	public CombinedBendType BendType { get; set; }

	public (int punchProfileId, int dieProfileId)? UserForcedToolProfiles { get; set; }

	public int? UserForcedUpperToolProfile { get; set; }

	public int? UserForcedLowerToolProfile { get; set; }

	public int? ToolSetupId { get; set; }

	public ToolSelectionType ToolSelectionAlgorithm { get; set; }

	public int? UpperToolSetId { get; set; }

	public int? LowerToolSetId { get; set; }

	public int? UpperAdapterSetId { get; set; }

	public int? LowerAdapterSetId { get; set; }

	public double ToolStationOffset { get; set; }

	public SToolPresenceVector ToolPresenceVector { get; set; }

	public double ProgressStart { get; set; }

	public double ProgressStop { get; set; }

	public List<int> SplitPredecessors { get; set; }

	public SFingerStopPoint SelectedStopPointLeft { get; set; }

	public SFingerStopPoint SelectedStopPointRight { get; set; }

	public List<SFingerStopPoint> StopPointsLeft { get; set; }

	public List<SFingerStopPoint> StopPointsRight { get; set; }

	public FingerPositioningMode FingerPositioningMode { get; set; }

	public FingerStability FingerStability { get; set; }

	public double? XLeftRetractAuto { get; set; }

	public double? XLeftRetractUser { get; set; }

	public double? XRightRetractAuto { get; set; }

	public double? XRightRetractUser { get; set; }

	public double? RLeftRetractAuto { get; set; }

	public double? RLeftRetractUser { get; set; }

	public double? RRightRetractAuto { get; set; }

	public double? RRightRetractUser { get; set; }

	public double? ZLeftRetractAuto { get; set; }

	public double? ZLeftRetractUser { get; set; }

	public double? ZRightRetractAuto { get; set; }

	public double? ZRightRetractUser { get; set; }

	public bool LeftFingerSnap { get; set; } = true;

	public bool RightFingerSnap { get; set; } = true;

	public double? ReleasePointUser { get; set; }

	public bool UseAngleMeasurement { get; set; }

	public double? AngleMeasurementPosition { get; set; }

	public double? AngleMeasurementPositionRel { get; set; }

	public double? LeftFrontLiftingAidHorizontalCoordinar { get; set; }

	public double? LeftFrontLiftingAidVerticalCoordinar { get; set; }

	public double? LeftFrontLiftingAidRotationCoordinar { get; set; }

	public double? RightFrontLiftingAidHorizontalCoordinar { get; set; }

	public double? RightFrontLiftingAidVerticalCoordinar { get; set; }

	public double? RightFrontLiftingAidRotationCoordinar { get; set; }

	public double? LeftBackLiftingAidHorizontalCoordinar { get; set; }

	public double? LeftBackLiftingAidVerticalCoordinar { get; set; }

	public double? LeftBackLiftingAidRotationCoordinar { get; set; }

	public double? RightBackLiftingAidHorizontalCoordinar { get; set; }

	public double? RightBackLiftingAidVerticalCoordinar { get; set; }

	public double? RightBackLiftingAidRotationCoordinar { get; set; }

	public SBendPositioningInfo PositioningInfo { get; set; }

	public MachinePartInsertionDirection MachinePartInsertionDirection { get; set; }

	public string Comment { get; set; }

	public LiftingAidEnum UseLeftFrontLiftingAid { get; set; }

	public LiftingAidEnum UseRightFrontLiftingAid { get; set; }

	public LiftingAidEnum UseLeftBackLiftingAid { get; set; }

	public LiftingAidEnum UseRightBackLiftingAid { get; set; }

	public int? UserStepChangeMode { get; set; }
}
