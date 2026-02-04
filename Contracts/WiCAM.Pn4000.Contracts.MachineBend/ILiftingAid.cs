using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Motions;
using WiCAM.Pn4000.BendModel.BendTools;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface ILiftingAid
{
	ModelByLinearAxesMotionData BaseMotionData { get; }

	string FilePath { get; }

	Model LiftingAidBaseModel { get; set; }

	Model LiftingAidRotationModel { get; set; }

	Model LiftingAidTableModel { get; set; }

	Model LiftingAidTransformModel { get; set; }

	Model Root => this.LiftingAidBaseModel;

	string Name { get; set; }

	Vector3d Origin { get; }

	ModelByRotationAxesMotionData RotationData { get; }

	ModelByLinearAxesMotionData TableMotionData { get; }

	PartRole Type { get; }

	List<Model> MoveToInitialPosition();
}
