using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Motions;
using WiCAM.Pn4000.BendModel.Base.SpatialDataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.MachineBend.Enums;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IFingerStop
{
	string Name { get; set; }

	Model FingerModel { get; set; }

	IFingerStopCombination SelectedCombination { get; set; }

	List<IFingerStopPointInternal> StopPoints { get; set; }

	IFingerStop DependsOn { get; set; }

	IFingerStop OtherFinger { get; set; }

	string FilePath { get; }

	ModelByLinearAxesMotionData MotionData { get; }

	IFingerStopSettings Settings { get; set; }

	IEnumerable<MotionBase> GetMotionToStopPoint(Vector3d stopPoint);

	List<Model> MoveToInitialPosition();

	IEnumerable<MotionBase> GetMotion(Vector3d moveDistances);

	MotionByLinearAxes SetDieAxis(double workingHeightDie);

	bool GetMotionAabb(out AABB<Vector3d> aabb, IFingerStop overrideDependsOnFinger = null, bool ignoreDependsOnFinger = false);

	Model? GetStopFaceModel(StopCombinationType type);

	IEnumerable<Pair<StopCombinationType, Model>> GetAllStopFaceModels();
}
