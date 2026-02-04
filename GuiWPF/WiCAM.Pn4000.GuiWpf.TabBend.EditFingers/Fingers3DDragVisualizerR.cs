using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.GuiWpf.TabBend.EditFingers.Interfaces;
using WiCAM.Pn4000.GuiWpf.Ui3D.InteractiveMotions;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ToolCalculation.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditFingers;

internal class Fingers3DDragVisualizerR : ModelDragVisualizer<Vector1d>, IFingers3DDragVisualizerR
{
	private double _minR;

	private double _maxR;

	public Fingers3DDragVisualizerR(IPnBndDoc doc, IScreen3DMain screen3D)
		: base(doc, screen3D, Vector3d.UnitZ, (Vector3d?)null)
	{
	}

	double IFingers3DDragVisualizerR.Stop()
	{
		return Stop().distanceOnPrimaryDir;
	}

	protected override (double distanceOnPrimaryDir, double distanceOnSecondaryDir) ApplyBlockedIntervals(double distanceOnPrimaryDir, double distanceOnSecondaryDir)
	{
		return (distanceOnPrimaryDir: MathExt.Clamp(distanceOnPrimaryDir, _minR, _maxR), distanceOnSecondaryDir: distanceOnSecondaryDir);
	}

	protected override (ISnapPoint? snapPointPrimary, ISnapPoint? snapPointSecondary) ApplySnapPoints(double distanceOnPrimaryDir, double distanceOnSecondaryDir)
	{
		return (snapPointPrimary: null, snapPointSecondary: null);
	}

	public void Start(Model model, Model referenceSystemModel, double minR, double maxR)
	{
		_minR = minR;
		_maxR = maxR;
		Start(model, referenceSystemModel);
	}
}
