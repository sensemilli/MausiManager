using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Motions;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Contracts.MachineBend;

namespace WiCAM.Pn4000.PN3D.LiftingAid;

public class LiftingAid : ILiftingAid
{
	private readonly IBendMachineGeometry _machineGeometry;

	public string FilePath { get; }

	public string Name { get; set; }

	public Model LiftingAidBaseModel { get; set; }

	public Model LiftingAidTableModel { get; set; }

	public Model LiftingAidTransformModel { get; set; }

	public Model LiftingAidRotationModel { get; set; }

	public PartRole Type { get; private set; }

	public Vector3d Origin { get; private set; }

	public ModelByLinearAxesMotionData BaseMotionData { get; private set; }

	public ModelByLinearAxesMotionData TableMotionData { get; private set; }

	public ModelByRotationAxesMotionData RotationData { get; private set; }

	public LiftingAid(Model model, PartRole role, IBendMachineGeometry machineGeometry)
	{
		this._machineGeometry = machineGeometry;
		this.Type = role;
		this.LiftingAidBaseModel = model.GetAllSubModelsWithSelf().FirstOrDefault((Model m) => m.PartRole == role);
		bool isLeft = role == PartRole.LeftFrontLiftingAid || role == PartRole.LeftBackLiftingAid;
		if (role == PartRole.LeftFrontLiftingAid || role == PartRole.RightFrontLiftingAid)
		{
			this.LiftingAidTableModel = model.GetAllSubModels().FirstOrDefault((Model m) => m.PartRole == (PartRole)(isLeft ? 31 : 32));
		}
		else
		{
			this.LiftingAidTableModel = model.GetAllSubModels().FirstOrDefault((Model m) => m.PartRole == (PartRole)(isLeft ? 41 : 42));
		}
		this.LiftingAidRotationModel = model.GetAllSubModels().FirstOrDefault((Model m) => m.PartRole == (PartRole)(isLeft ? 34 : 36));
		this.LiftingAidTransformModel = model.GetAllSubModels().FirstOrDefault((Model m) => m.PartRole == (PartRole)(isLeft ? 33 : 35));
		this.FilePath = model.FileName;
		this.BaseMotionData = new ModelByLinearAxesMotionData(this.LiftingAidBaseModel, this._machineGeometry.LinearAxesByModel);
		this.TableMotionData = new ModelByLinearAxesMotionData(this.LiftingAidTransformModel, this._machineGeometry.LinearAxesByModel);
		this.RotationData = new ModelByRotationAxesMotionData(this.LiftingAidRotationModel, this._machineGeometry.RotationAxisByModel);
	}

	public List<Model> MoveToInitialPosition()
	{
		Dictionary<MotionLinearAxis, double> dictionary = this.TableMotionData.AllAxesByModel.SelectMany((KeyValuePair<Model, List<MotionLinearAxis>> kvp) => kvp.Value).ToDictionary((MotionLinearAxis axis) => axis, (MotionLinearAxis axis) => axis.InitialPosition);
		new MotionByLinearAxes(this.TableMotionData.AllAxesByModel, dictionary, dictionary, 1.0).GoToEnd();
		Dictionary<MotionRotationAxis, double> dictionary2 = this.RotationData.AllAxesByModel.SelectMany((KeyValuePair<Model, List<MotionRotationAxis>> kvp) => kvp.Value).ToDictionary((MotionRotationAxis axis) => axis, (MotionRotationAxis axis) => axis.InitialPosition);
		new MotionByRotationAxes(this.RotationData.AllAxesByModel, dictionary2, dictionary2, 1.0).GoToEnd();
		List<Model> list = this.BaseMotionData.AllAxesByModel.Keys.ToList();
		list.AddRange(this.TableMotionData.AllAxesByModel.Keys);
		list.AddRange(this.RotationData.AllAxesByModel.Keys);
		return list;
	}
}
