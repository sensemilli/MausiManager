using System.Collections.Generic;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base.Enum;
using WiCAM.Pn4000.BendModel.Base.Motions;

namespace WiCAM.Pn4000.Contracts.MachineBend;

public interface IBendMachineGeometry
{
	Model Root => this.MachineModel;

	Model MachineModel { get; set; }

	Model MainFrame { get; }

	Model Beam { get; }

	Model BeamLeft { get; }

	Model BeamRight { get; }

	Model UpperToolSystem { get; }

	Model UpperToolSystemTools { get; }

	Model LowerToolSystem { get; }

	Model LowerToolSystemTools { get; }

	Model TotalFingerSupport { get; }

	Model LeftFingerSupport { get; }

	Model RightFingerSupport { get; }

	IFingerStop LeftFinger { get; }

	IFingerStop RightFinger { get; }

	ILiftingAid? LeftFrontLiftingAid { get; }

	ILiftingAid? RightFrontLiftingAid { get; }

	ILiftingAid? LeftBackLiftingAid { get; }

	ILiftingAid? RightBackLiftingAid { get; }

	IAngleMeasurementSystem AngleMeasurmentSystem { get; }

	Dictionary<AxisType, MotionLinearAxis> LinearAxesByType { get; }

	Dictionary<Model, List<MotionLinearAxis>> LinearAxesByModel { get; }

	Dictionary<AxisType, MotionRotationAxis> RotationAxisByType { get; }

	Dictionary<Model, List<MotionRotationAxis>> RotationAxisByModel { get; }

	double BeamGapMin { get; }

	double BeamGapMax { get; }

	string BasePath { get; }

	IBendMachineGeometry CopyStructure();

	Model LoadGeometry(string filename);

	bool SaveGeometry(string filename, Model model);

	IEnumerable<string> GetGeometryNames();
}
