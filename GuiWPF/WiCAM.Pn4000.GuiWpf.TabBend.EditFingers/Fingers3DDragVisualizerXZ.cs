using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.GuiWpf.TabBend.EditFingers.Interfaces;
using WiCAM.Pn4000.GuiWpf.Ui3D.InteractiveMotions;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ToolCalculation.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditFingers;

internal class Fingers3DDragVisualizerXZ : ModelDragVisualizer<Vector2d>, IFingers3DDragVisualizerXZ
{
	private double _minX;

	private double _maxX;

	private double _minY;

	private double _maxY;

	public Fingers3DDragVisualizerXZ(IPnBndDoc doc, IScreen3DMain screen3D)
		: base(doc, screen3D, Vector3d.UnitX, (Vector3d?)Vector3d.UnitY)
	{
	}

	public void Start(Model model, Model referenceSystemModel, double minX, double maxX, double minY, double maxY)
	{
		_minX = minX;
		_maxX = maxX;
		_minY = minY;
		_maxY = maxY;
		Start(model, referenceSystemModel);
	}

	protected override (double distanceOnPrimaryDir, double distanceOnSecondaryDir) ApplyBlockedIntervals(double distanceOnPrimaryDir, double distanceOnSecondaryDir)
	{
		return (distanceOnPrimaryDir: MathExt.Clamp(distanceOnPrimaryDir, _minX, _maxX), distanceOnSecondaryDir: MathExt.Clamp(distanceOnSecondaryDir, _minY, _maxY));
	}

	protected override (ISnapPoint? snapPointPrimary, ISnapPoint? snapPointSecondary) ApplySnapPoints(double distanceOnPrimaryDir, double distanceOnSecondaryDir)
	{
		return (snapPointPrimary: null, snapPointSecondary: null);
	}
}
