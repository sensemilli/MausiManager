using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Motions;
using WiCAM.Pn4000.BendModel.BendTools;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IAngleMeasurementSystem
{
	string FilePath { get; }

	string Name { get; set; }

	Model AngleMeasurmentModel { get; set; }

	PartRole Type { get; }

	Vector3d Origin { get; }

	ModelByLinearAxesMotionData MotionData { get; }

	List<Model> MoveToInitialPosition();
}
