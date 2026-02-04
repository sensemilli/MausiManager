using System;
using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.Contracts.BendSimulation;

public interface ISimulationBendInfoOld
{
	double AngleProgressStart { get; }

	double AngleProgressStop { get; }

	IBendPositioning? Bend { get; }

	int BendNumber { get; }

	Vector3d BendOffsetWorld { get; }

	IBendPositioningInfo BendPositioningInfo { get; }

	CombinedBendType BendType { get; }

	ICombinedBendDescriptorInternal CombinedBendDescriptor { get; }

	IDieProfile DieProfile { get; }

	List<FaceGroup> FaceGroups { get; }

	bool FlippedDieProfile { get; }

	bool FlippedPunchProfile { get; }

	Func<IFingerStopPointInternal> GetLeftFingerStop { get; }

	Func<double> GetRetractDistanceLeftFunc { get; }

	Func<double> GetRetractDistanceRightFunc { get; }

	Func<IFingerStopPointInternal> GetRightFingerStop { get; }

	bool IsIncluded { get; set; }

	bool IsReversedGeometry { get; }

	bool IsSimulated { get; }

	double KFactor { get; }

	Vector3d LegBox { get; set; }

	ISimulationBendInfoOld NextInfo { get; set; }

	ISimulationBendInfoOld PrevInfo { get; set; }

	IPunchProfile PunchProfile { get; }

	double? SpringBack { get; }

	double? SpringBackCalculated { get; set; }

	double WorkingHeightDie { get; }

	double WorkingHeightPunch { get; }
}
