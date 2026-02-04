using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface ISimulationBendInfo
{
	int Order { get; }

	bool IsIncluded { get; set; }

	CombinedBendType BendType { get; }

	double AngleProgressStop { get; }

	double AngleProgressStart { get; }

	IToolCluster? Anchor { get; }

	Vector3d Offset { get; }

	double OffsetWorldX { get; }

	bool IsReversedGeometry { get; }

	MachinePartInsertionDirection MachineInsertDirection { get; }

	Vector3d PositionWorld => this.Offset + (this.Anchor?.OffsetWorld ?? Vector3d.Zero);

	IEnumerable<IRange> BendingZonesOrientated { get; }

	int PrimaryFaceGroupId { get; }

	IEnumerable<int> FaceGroupIds { get; }

	double KFactor { get; }

	double? SpringBack { get; }

	ISimulationFingerPosInfo? FingerPosInfo { get; }

	ISimulationLiftingAidInfo? LiftingAidInfo { get; }

	ISimulationAngleMeasurementInfo? AngleMeasurementSysInfo { get; }

	double? ReleasePointUser { get; }

	IPunchProfile? PunchProfile { get; }

	IDieProfile? DieProfile { get; }

	IToolProfile? SpringAdapter { get; }

	IDieProfile? SpringVProfile { get; }

	double WorkingHeightDie { get; }

	double WorkingHeightPunch { get; }

	ISimulationBendInfo NextInfo { get; set; }

	ISimulationBendInfo PrevInfo { get; set; }

	Vector3d LegBox { get; set; }
}
