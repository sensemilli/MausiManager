using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Motions;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Contracts.MachineBend;

namespace WiCAM.Pn4000.PN3D.Machine;

public class AngleMeasurementSystem : IAngleMeasurementSystem
{
	private readonly IBendMachineGeometry _machineGeometry;

	public string FilePath { get; }

	public string Name { get; set; }

	public Model AngleMeasurmentModel { get; set; }

	public PartRole Type { get; private set; }

	public Vector3d Origin { get; private set; }

	public ModelByLinearAxesMotionData MotionData { get; private set; }

	public AngleMeasurementSystem(Model model, PartRole role, IBendMachineGeometry machineGeometry)
	{
		this._machineGeometry = machineGeometry;
		this.Type = role;
		this.AngleMeasurmentModel = model.GetAllSubModelsWithSelf().FirstOrDefault((Model m) => m.PartRole == role);
		this.FilePath = model.FileName;
		this.MotionData = new ModelByLinearAxesMotionData(this.AngleMeasurmentModel, this._machineGeometry.LinearAxesByModel);
	}

	public List<Model> MoveToInitialPosition()
	{
		Dictionary<MotionLinearAxis, double> dictionary = this.MotionData.AllAxesByModel.SelectMany((KeyValuePair<Model, List<MotionLinearAxis>> kvp) => kvp.Value).ToDictionary((MotionLinearAxis axis) => axis, (MotionLinearAxis axis) => axis.InitialPosition);
		new MotionByLinearAxes(this.MotionData.AllAxesByModel, dictionary, dictionary, 1.0).GoToEnd();
		return this.MotionData.AllAxesByModel.Keys.ToList();
	}
}
