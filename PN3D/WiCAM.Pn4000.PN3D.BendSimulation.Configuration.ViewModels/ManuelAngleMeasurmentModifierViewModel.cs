using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.Motions;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Doc3d;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes;
using WiCAM.Pn4000.ScreenD3D.Extensions;

namespace WiCAM.Pn4000.PN3D.BendSimulation.Configuration.ViewModels;

public class ManuelAngleMeasurmentModifierViewModel : ScreenControlBaseViewModel, ISubViewModel
{
	private readonly IDoc3d _doc;

	private readonly IScreen3D _screen;

	private Matrix4d _obj2WorldStart;

	private readonly ICombinedBendDescriptor _activeBend;

	public event Action<ISubViewModel, Triangle, Model, double, double, Vector3d, MouseButtonEventArgs> Closed;

	public event Action RequestRepaint;

	public ManuelAngleMeasurmentModifierViewModel(IDoc3d doc, ICombinedBendDescriptor combinedBend, Screen3D screen, Model model, Vector3d anchorPoint, IArrowFactory arrowFactory, IGlobals globals)
		: base(screen, model, anchorPoint)
	{
		ManuelAngleMeasurmentModifierViewModel manuelAngleMeasurmentModifierViewModel = this;
		this._doc = doc;
		this._screen = screen;
		this._activeBend = combinedBend;
		base.SetOpacityMin(globals.ConfigProvider.InjectOrCreate<GeneralUserSettingsConfig>().DialogOpacity);
		HashSet<Model> selectableModels = new HashSet<Model> { model };
		MotionLinearAxis motionLinearAxis = this._doc.BendMachine.Geometry.LinearAxesByModel[model].First();
		Matrix4d worldMatrix = model.Parent.WorldMatrix;
		Vector3d v = motionLinearAxis.Start;
		worldMatrix.TransformInPlace(ref v);
		Vector3d v2 = motionLinearAxis.End;
		worldMatrix.TransformInPlace(ref v2);
		MoveModelInteractionMode moveModelInteractionMode = new MoveModelInteractionMode(screen, selectableModels, arrowFactory.CreateDoubleArrow(), null, null, v.X, v2.X, -1.0, 1.0, -1.0, 1.0)
		{
			UseWorldCoordinates = true,
			RenderMotion = false
		};
		moveModelInteractionMode.ObjectSelected += InteractionModeOnObjectSelected;
		moveModelInteractionMode.ObjectMoved += InteractionModeOnObjectMoved;
		moveModelInteractionMode.ObjectMoving += InteractionModeOnObjectMoving;
		screen.InteractionMode = moveModelInteractionMode;
		this.InteractionModeOnObjectSelected(moveModelInteractionMode, model);
		Vector3d v3 = anchorPoint;
		worldMatrix.TransformInPlace(ref v3);
		moveModelInteractionMode.ShowArrows(model, v3);
		moveModelInteractionMode.ObjectMoving += delegate
		{
			manuelAngleMeasurmentModifierViewModel.OnViewChanged();
		};
		moveModelInteractionMode.ObjectMoved += delegate
		{
			manuelAngleMeasurmentModifierViewModel.OnViewChanged();
		};
		moveModelInteractionMode.ResetArrows += delegate(MoveModelInteractionMode mode, Model selectedModel, Vector3d point)
		{
			manuelAngleMeasurmentModifierViewModel.AnchorPoint3d = point;
			manuelAngleMeasurmentModifierViewModel.Model = model;
			manuelAngleMeasurmentModifierViewModel.OnViewChanged();
		};
		this._doc.BendSimulation.GoToBend(this._activeBend.Order);
	}

	private void InteractionModeOnObjectSelected(MoveModelInteractionMode sender, Model obj)
	{
		this._obj2WorldStart = obj.Parent.WorldMatrix;
	}

	private void InteractionModeOnObjectMoved(MoveModelInteractionMode sender, Model obj)
	{
		double x = obj.Parent.WorldMatrix.TranslationVector.X - this._obj2WorldStart.TranslationVector.X;
		obj.Transform *= Matrix4d.Translation(x, 0.0, 0.0);
		this._screen.ScreenD3D.UpdateAllModelTransform();
		this._obj2WorldStart = obj.Parent.WorldMatrix;
		this._doc.BendSimulation.UpdateWAxesPos(this._activeBend.Order, obj.Transform.TranslationVector.X, this._doc.BendMachine.Geometry.AngleMeasurmentSystem);
	}

	private void InteractionModeOnObjectMoving(MoveModelInteractionMode arg1, Model obj, Vector3d position)
	{
		double x = obj.Parent.WorldMatrix.TranslationVector.X - this._obj2WorldStart.TranslationVector.X;
		obj.Transform *= Matrix4d.Translation(x, 0.0, 0.0);
		this._screen.ScreenD3D.UpdateAllModelTransform();
		this._obj2WorldStart = obj.Parent.WorldMatrix;
	}

	public void MouseMove(object sender, MouseEventArgs e)
	{
	}

	public void ColorModelParts(IPaintTool paintTool)
	{
	}

	public void KeyUp(object sender, IPnInputEventArgs e)
	{
	}

	public void MouseSelectTriangle(object sender, ITriangleEventArgs e)
	{
		if (e.Model.PartRole != PartRole.AngleMeasurement)
		{
			this.Close();
		}
	}

	public override bool Close()
	{
		this.Closed?.Invoke(this, null, null, 0.0, 0.0, Vector3d.Zero, null);
		return base.Close();
	}

	public void SetActive(bool active)
	{
	}
}
