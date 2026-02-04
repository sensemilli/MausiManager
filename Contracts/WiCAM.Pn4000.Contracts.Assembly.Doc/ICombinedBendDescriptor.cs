using System;
using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;

namespace WiCAM.Pn4000.Contracts.Assembly.Doc;

public interface ICombinedBendDescriptor
{
	IBendDescriptor this[int i] { get; }

	int Count { get; }

	IEnumerable<IBendDescriptor> Enumerable { get; }

	IEnumerable<IBendDescriptor> Enumerable2 { get; }

	IEnumerable<IBendDescriptor> BendOrderUnfoldModel { get; }

	IEnumerable<IBendDescriptor> BendOrderBendModel { get; }

	int Order { get; }

	bool ToolsFound { get; }

	bool IsIncluded { get; set; }

	CombinedBendType BendType { get; }

	int? UserForcedUpperToolProfile { get; set; }

	int? UserForcedLowerToolProfile { get; set; }

	ToolSelectionType ToolSelectionAlgorithm { get; set; }

	bool UseLiftingAid { get; }

	LiftingAidEnum UseLeftFrontLiftingAid { get; set; }

	LiftingAidEnum UseRightFrontLiftingAid { get; set; }

	LiftingAidEnum UseLeftBackLiftingAid { get; set; }

	LiftingAidEnum UseRightBackLiftingAid { get; set; }

	bool UseAngleMeasurement { get; set; }

	[Obsolete("Not Implemented yet.")]
	double? AngleMeasurementPositionWorld { get; set; }

	double? AngleMeasurementPositionRel { get; set; }

	double? LeftFrontLiftingAidHorizontalCoordinar { get; set; }

	double? LeftFrontLiftingAidVerticalCoordinar { get; set; }

	double? LeftFrontLiftingAidRotationCoordinar { get; set; }

	double? RightFrontLiftingAidHorizontalCoordinar { get; set; }

	double? RightFrontLiftingAidVerticalCoordinar { get; set; }

	double? RightFrontLiftingAidRotationCoordinar { get; set; }

	double? LeftBackLiftingAidHorizontalCoordinar { get; set; }

	double? LeftBackLiftingAidVerticalCoordinar { get; set; }

	double? LeftBackLiftingAidRotationCoordinar { get; set; }

	double? RightBackLiftingAidHorizontalCoordinar { get; set; }

	double? RightBackLiftingAidVerticalCoordinar { get; set; }

	double? RightBackLiftingAidRotationCoordinar { get; set; }

	double ProgressStart { get; set; }

	double ProgressStop { get; set; }

	[Obsolete("Probably wrong definition. see BendAngleAbsStart")]
	double StartProductAngleAbs { get; }

	[Obsolete("Probably wrong definition. see BendAngleAbsStop")]
	double StopProductAngleAbs { get; }

	[Obsolete("Probably wrong definition. see BendAngleAbsStop")]
	double StopProductAngleSigned { get; }

	[Obsolete("Probably wrong definition. see BendAngleAbsStart")]
	double StartBendAngleAbs { get; }

	[Obsolete("Probably wrong definition. see BendAngleAbsStop")]
	double StopBendAngleAbs { get; }

	double BendAngleAbsStart { get; }

	double BendAngleAbsStop { get; }

	double TotalLength { get; }

	double TotalLengthWithoutGaps { get; }

	IReadOnlyList<ICombinedBendDescriptor> SplitPredecessors { get; }

	IReadOnlyList<ICombinedBendDescriptor> SplitSuccessors { get; }

	int SplitBendCount { get; }

	int SplitBendOrder { get; }

	IBendPositioningInfo PositioningInfo { get; }

	IFingerStopPoint? SelectedStopPointLeft { get; }

	IFingerStopPoint? SelectedStopPointRight { get; }

	FingerPositioningMode FingerPositioningMode { get; set; }

	FingerStability FingerStability { get; set; }

	double? XLeftRetractAuto { get; set; }

	double? XLeftRetractUser { get; set; }

	double? XRightRetractAuto { get; set; }

	double? XRightRetractUser { get; set; }

	double? RLeftRetractAuto { get; set; }

	double? RLeftRetractUser { get; set; }

	double? RRightRetractAuto { get; set; }

	double? RRightRetractUser { get; set; }

	double? ZLeftRetractAuto { get; set; }

	double? ZLeftRetractUser { get; set; }

	double? ZRightRetractAuto { get; set; }

	double? ZRightRetractUser { get; set; }

	double? ReleasePointUser { get; set; }

	double PressForce => this.UserPressForce ?? this.AutoPressForce;

	double AutoPressForce { get; }

	double? UserPressForce { get; set; }

	int BranchLength { get; }

	MachinePartInsertionDirection MachinePartInsertionDirection { get; set; }

	string Comment { get; set; }

	int PreferredProfilePrio { get; set; }

	int? UserStepChangeMode { get; set; }

	void UnfoldBendInModifiedEntryModel(double step, bool relative, bool noGeometryChange = false);

	void UnfoldBendInUnfoldModel(double step, bool relative, bool noGeometryChange = false);

	void UnfoldBendInBendModel(double step, bool relative, bool noGeometryChange = false);

	void UnfoldBendInModel(double step, bool relative, UiModelType uiModelType, bool noGeometryChange = false);

	Pair<Vector3d, Vector3d> GetEndPointsInWorldCoords(Func<IBendDescriptor, (FaceGroup fg, Model model)> modelSelector);

	bool IsSplitableBend();

	(HashSet<FaceGroup> left, HashSet<FaceGroup> right) GetAllNeighboursInBendModel();

	(HashSet<FaceGroup> left, HashSet<FaceGroup> right) GetAllNeighboursInUnfoldModel();

	(HashSet<FaceGroup> left, HashSet<FaceGroup> right) GetLegsOfBendInBendModel();

	(HashSet<FaceGroup> left, HashSet<FaceGroup> right) GetLegsOfBendInUnfoldModel();

	void ResetMachineSpecificData();

	void ResetStopPoints();

	void ResetTools();

	void UpdatePositioningInfo();

	void ActivateAndAutoPositionAngleMeasurementSystem(bool active, bool recalcSim);
}
