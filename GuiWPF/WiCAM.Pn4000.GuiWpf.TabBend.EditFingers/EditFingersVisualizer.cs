using System.Linq;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendServices;
using WiCAM.Pn4000.Contracts.BendSimulation;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.FingerCalculation;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.FingerStopCalculationMediator;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.GuiWpf.TabBend.EditFingers.Interfaces;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditFingers;

internal class EditFingersVisualizer : SubViewModelBase, IEditFingersVisualizer, ISubViewModel
{
	private readonly IDoc3d _doc;

	private readonly IScreen3DMain _screen;

	private readonly IBendSelection _bendSelection;

	private readonly IFingerStopModifier _fingerModifier;

	private readonly IEditFingersBillboards _editFingersBillboards;

	private readonly IFingers2DDragVisualizerR _dragFingersVisualizerR;

	private readonly IFingers2DDragVisualizerXZ _dragFingersVisualizerXZ;

	private readonly IShortcutSettingsCommon _shortcutSettingsCommon;

	private readonly IFingerStopCalculationMediator _fingerStopCalculationMediator;

	private readonly IUndo3dDocService _undo3d;

	private readonly ITranslator _translator;

	private bool _showBendModel;

	private bool _isActive;

	private bool _isAlive;

	public IEditFingersViewModel EditFingersViewModel { get; }

	private Model FingerModel
	{
		get
		{
			return EditFingersViewModel.SelectedFinger?.FingerModel;
		}
		set
		{
			EditFingersViewModel.SelectFinger(value);
		}
	}

	public EditFingersVisualizer(IDoc3d doc, IScreen3DMain screen, IBendSelection bendSelection, IFingerStopModifier fingerModifier, IEditFingersViewModel editFingersViewModel, IEditFingersBillboards editFingersBillboards, IFingers2DDragVisualizerR dragFingersVisualizerR, IFingers2DDragVisualizerXZ dragFingersVisualizerXZ, IShortcutSettingsCommon shortcutSettingsCommon, IFingerStopCalculationMediator fingerStopCalculationMediator, IUndo3dDocService undo3d, ITranslator translator)
	{
		_doc = doc;
		_screen = screen;
		_bendSelection = bendSelection;
		_fingerModifier = fingerModifier;
		EditFingersViewModel = editFingersViewModel;
		_editFingersBillboards = editFingersBillboards;
		_dragFingersVisualizerR = dragFingersVisualizerR;
		_dragFingersVisualizerXZ = dragFingersVisualizerXZ;
		_shortcutSettingsCommon = shortcutSettingsCommon;
		_fingerStopCalculationMediator = fingerStopCalculationMediator;
		_undo3d = undo3d;
		_translator = translator;
		_bendSelection.CurrentBendChanged += BendSelection_CurrentBendChanged;
		_editFingersBillboards.RequestRepaint += base.RaiseRequestRepaint;
		_editFingersBillboards.CmdMoveR += MoveR;
		_editFingersBillboards.CmdMoveXZ += MoveXZ;
		_editFingersBillboards.CmdSnapUp += SnapUp;
		_editFingersBillboards.CmdSnapLeft += SnapLeft;
		_editFingersBillboards.CmdSnapRight += SnapRight;
		_dragFingersVisualizerXZ.RaiseColorModelParts += base.RaiseRequestRepaint;
	}

	private void BendSelection_CurrentBendChanged(IBendPositioning? obj)
	{
	}

	public override void SetActive(bool active)
	{
		_isActive = active;
		base.SetActive(active);
		_editFingersBillboards.SetActive(active);
	}

	public override bool Close()
	{
		_editFingersBillboards.HideBillboards();
		FingerModel = null;
		_isAlive = false;
		EditFingersViewModel.Close();
		_dragFingersVisualizerR.Stop();
		_dragFingersVisualizerXZ.Stop();
		return base.Close();
	}

	public override void KeyUp(object sender, IPnInputEventArgs e)
	{
		base.KeyUp(sender, e);
		if (!e.Handled && _shortcutSettingsCommon.Cancel.IsShortcut(e))
		{
			Close();
			e.Handle();
		}
	}

	public override void MouseSelectTriangle(object sender, ITriangleEventArgs e)
	{
		base.MouseSelectTriangle(sender, e);
		if (MouseSelectTriangleValidObject(sender, e) == false)
		{
			Close();
			RaiseRequestRepaint();
		}
	}

	public bool? MouseSelectTriangleValidObject(object sender, ITriangleEventArgs e)
	{
		if (_dragFingersVisualizerR.IsDragging)
		{
			MouseButtonEventArgs? mouseButtonEventArgs = e.Args.MouseButtonEventArgs;
			if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
			{
				e.Args.Handle();
				double num = _dragFingersVisualizerR.Stop();
				IFingerStopModifier fingerModifier = _fingerModifier;
				PartRole partRole = FingerModel.PartRole;
				Vector3d translationVector = _doc.BendSimulation.State.LeftFinger.FingerModel.WorldMatrix.TranslationVector;
				Vector3d vector3d = ((FingerModel.PartRole != PartRole.LeftFinger) ? Vector3d.Zero : (num * Vector3d.UnitZ));
				Vector3d left = translationVector + vector3d;
				Vector3d translationVector2 = _doc.BendSimulation.State.RightFinger.FingerModel.WorldMatrix.TranslationVector;
				Vector3d vector3d2 = ((FingerModel.PartRole != PartRole.RightFinger) ? Vector3d.Zero : (num * Vector3d.UnitZ));
				fingerModifier.ApplyPositions(partRole, left, translationVector2 + vector3d2, snapLeft: true, snapRight: true, _doc.BendSimulation, out IFingerStopPointInternal leftFingerPos, out IFingerStopPointInternal rightFingerPos);
				_fingerModifier.UpdateStopFaceCombinations(_doc, leftFingerPos, rightFingerPos);
				ICombinedBendDescriptorInternal cbd = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _doc.BendSimulation.State.ActiveStep.BendInfo.Order);
				SetFingerPosByUser(cbd, leftFingerPos, rightFingerPos);
				SaveUndoAndRecreateSimulation();
				RaiseRequestRepaint();
				return false;
			}
			MouseButtonEventArgs? mouseButtonEventArgs2 = e.Args.MouseButtonEventArgs;
			if ((mouseButtonEventArgs2 != null && mouseButtonEventArgs2.ChangedButton == MouseButton.Right) || _shortcutSettingsCommon.Cancel.IsShortcut(e.Args))
			{
				e.Args.Handle();
				_dragFingersVisualizerR.Stop();
				return false;
			}
		}
		if (_dragFingersVisualizerXZ.IsDragging)
		{
			MouseButtonEventArgs? mouseButtonEventArgs3 = e.Args.MouseButtonEventArgs;
			if (mouseButtonEventArgs3 != null && mouseButtonEventArgs3.ChangedButton == MouseButton.Left)
			{
				e.Args.Handle();
				(double, double, Vector3d?) tuple = _dragFingersVisualizerXZ.Stop();
				ICombinedBendDescriptorInternal combinedBendDescriptorInternal = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _bendSelection.CurrentBend?.Order);
				if (FingerModel.PartRole == PartRole.LeftFinger)
				{
					_fingerModifier.ApplyPositions(FingerModel.PartRole, _doc.BendSimulation.State.LeftFinger.FingerModel.WorldMatrix.TranslationVector + tuple.Item1 * Vector3d.UnitX + tuple.Item2 * Vector3d.UnitY, tuple.Item3 ?? _doc.BendSimulation.State.RightFinger.FingerModel.WorldMatrix.TranslationVector, combinedBendDescriptorInternal.LeftFingerSnap, combinedBendDescriptorInternal.RightFingerSnap, _doc.BendSimulation, out IFingerStopPointInternal leftFingerPos2, out IFingerStopPointInternal rightFingerPos2);
					_fingerModifier.UpdateStopFaceCombinations(_doc, leftFingerPos2, rightFingerPos2);
					ICombinedBendDescriptorInternal cbd2 = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _doc.BendSimulation.State.ActiveStep.BendInfo.Order);
					SetFingerPosByUser(cbd2, leftFingerPos2, rightFingerPos2);
				}
				else
				{
					_fingerModifier.ApplyPositions(FingerModel.PartRole, tuple.Item3 ?? _doc.BendSimulation.State.LeftFinger.FingerModel.WorldMatrix.TranslationVector, _doc.BendSimulation.State.RightFinger.FingerModel.WorldMatrix.TranslationVector + tuple.Item1 * Vector3d.UnitX + tuple.Item2 * Vector3d.UnitY, combinedBendDescriptorInternal.LeftFingerSnap, combinedBendDescriptorInternal.RightFingerSnap, _doc.BendSimulation, out IFingerStopPointInternal leftFingerPos3, out IFingerStopPointInternal rightFingerPos3);
					_fingerModifier.UpdateStopFaceCombinations(_doc, leftFingerPos3, rightFingerPos3);
					ICombinedBendDescriptorInternal cbd3 = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _doc.BendSimulation.State.ActiveStep.BendInfo.Order);
					SetFingerPosByUser(cbd3, leftFingerPos3, rightFingerPos3);
				}
				SaveUndoAndRecreateSimulation();
				RaiseRequestRepaint();
				return false;
			}
			MouseButtonEventArgs? mouseButtonEventArgs4 = e.Args.MouseButtonEventArgs;
			if ((mouseButtonEventArgs4 != null && mouseButtonEventArgs4.ChangedButton == MouseButton.Right) || _shortcutSettingsCommon.Cancel.IsShortcut(e.Args))
			{
				e.Args.Handle();
				_dragFingersVisualizerXZ.Stop();
				return false;
			}
		}
		if (e.Args.Handled)
		{
			return null;
		}
		Model? model = e.Model;
		if (model == null || model.PartRole != PartRole.LeftFinger)
		{
			Model? model2 = e.Model;
			if (model2 == null || model2.PartRole != PartRole.RightFinger)
			{
				IPnInputEventArgs args = e.Args;
				if (args != null && args.MouseButtonEventArgs?.ChangedButton == MouseButton.Left)
				{
					return false;
				}
				IPnInputEventArgs args2 = e.Args;
				if (args2 != null && args2.MouseButtonEventArgs?.ChangedButton == MouseButton.Right)
				{
					return false;
				}
				return null;
			}
		}
		IPnInputEventArgs args3 = e.Args;
		if (args3 != null && args3.MouseButtonEventArgs?.ChangedButton == MouseButton.Right)
		{
			ISimulationStep curStep = _doc.BendSimulation.State.ActiveStep;
			int num2 = _doc.BendSimulation.SimulationsSteps.FindIndex((ISimulationStep step) => step.BendInfo.Order == curStep.BendInfo.Order && step is IStepMain);
			_doc.BendSimulation.GotoStep(num2);
			_editFingersBillboards.SetBillboardLocation(e.HitPoint, e.Model.PartRole);
			_editFingersBillboards.ShowBillboards();
		}
		else
		{
			_editFingersBillboards.HideBillboards();
		}
		e.Args.Handle();
		EditFingersViewModel.Activate();
		EditFingersViewModel.Init();
		FingerModel = e.Model;
		RaiseRequestRepaint();
		return true;
	}

	private void SaveUndoAndRecreateSimulation()
	{
		_undo3d.Save(_translator.Translate("Undo3d.FingerUser"));
		_doc.RecalculateSimulation();
		_screen.ScreenD3D.RemoveModel(null);
		_screen.ScreenD3D.AddModel(_doc.BendSimulation.State.Part, render: false);
		_screen.ScreenD3D.AddModel(_doc.BendSimulation.State.Machine, render: false);
	}

	public bool StartSubMenu(object sender, ITriangleEventArgs e)
	{
		return MouseSelectTriangleValidObject(sender, e) == true;
	}

	public override void MouseMove(object sender, MouseEventArgs e)
	{
		if (_dragFingersVisualizerXZ.IsDragging)
		{
			_dragFingersVisualizerXZ.MouseMove(sender, e);
		}
		if (_dragFingersVisualizerR.IsDragging)
		{
			_dragFingersVisualizerR.MouseMove(sender, e);
		}
	}

	public override void ColorModelParts(IPaintTool paintTool)
	{
		base.ColorModelParts(paintTool);
		if (_dragFingersVisualizerR.IsDragging)
		{
			_dragFingersVisualizerR.ColorModelParts(paintTool);
		}
		if (_dragFingersVisualizerXZ.IsDragging)
		{
			_dragFingersVisualizerXZ.ColorModelParts(paintTool);
		}
		paintTool.SetModelEdgeColor(FingerModel, new Color(1f, 1f, 0f), 3f);
	}

	private void SetFingerPosByUser(ICombinedBendDescriptorInternal cbd, IFingerStopPointInternal leftFingerPos, IFingerStopPointInternal rightFingerPos)
	{
		_doc.SetFingerPos(cbd, leftFingerPos, rightFingerPos, FingerPositioningMode.User);
		_fingerStopCalculationMediator.CalculateRetractForFingers(_doc.BendSimulation, cbd);
	}

	private void MoveR(IPnInputEventArgs args)
	{
		if (!args.Handled)
		{
			MouseButtonEventArgs? mouseButtonEventArgs = args.MouseButtonEventArgs;
			if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
			{
				args.Handle();
				_editFingersBillboards.HideBillboards();
				_dragFingersVisualizerR.Start(FingerModel.PartRole);
				RaiseRequestRepaint();
			}
		}
	}

	private void MoveXZ(IPnInputEventArgs args)
	{
		if (!args.Handled)
		{
			MouseButtonEventArgs? mouseButtonEventArgs = args.MouseButtonEventArgs;
			if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
			{
				args.Handle();
				_editFingersBillboards.HideBillboards();
				_dragFingersVisualizerXZ.Start(FingerModel.PartRole);
				RaiseRequestRepaint();
			}
		}
	}

	private void SnapUp(IPnInputEventArgs args)
	{
		if (args.Handled)
		{
			return;
		}
		MouseButtonEventArgs? mouseButtonEventArgs = args.MouseButtonEventArgs;
		if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
		{
			args.Handle();
			_fingerModifier.SnapFingerUp(FingerModel.PartRole, _doc.BendSimulation, out IFingerStopPointInternal leftFingerPos, out IFingerStopPointInternal rightFingerPos);
			ICombinedBendDescriptorInternal cbd = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _doc.BendSimulation.State.ActiveStep.BendInfo.Order);
			SetFingerPosByUser(cbd, leftFingerPos, rightFingerPos);
			SaveUndoAndRecreateSimulation();
			RaiseRequestRepaint();
		}
	}

	private void SnapLeft(IPnInputEventArgs args)
	{
		if (_screen.ScreenD3D.IsCamaraInFront())
		{
			SnapXNeg(args);
		}
		else
		{
			SnapXPos(args);
		}
	}

	private void SnapXNeg(IPnInputEventArgs args)
	{
		if (args.Handled)
		{
			return;
		}
		MouseButtonEventArgs? mouseButtonEventArgs = args.MouseButtonEventArgs;
		if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
		{
			args.Handle();
			_fingerModifier.SnapFingerLeft(FingerModel.PartRole, _doc.BendSimulation, out IFingerStopPointInternal leftFingerPos, out IFingerStopPointInternal rightFingerPos);
			ICombinedBendDescriptorInternal cbd = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _doc.BendSimulation.State.ActiveStep.BendInfo.Order);
			SetFingerPosByUser(cbd, leftFingerPos, rightFingerPos);
			SaveUndoAndRecreateSimulation();
			RaiseRequestRepaint();
		}
	}

	private void SnapRight(IPnInputEventArgs args)
	{
		if (_screen.ScreenD3D.IsCamaraInFront())
		{
			SnapXPos(args);
		}
		else
		{
			SnapXNeg(args);
		}
	}

	private void SnapXPos(IPnInputEventArgs args)
	{
		if (args.Handled)
		{
			return;
		}
		MouseButtonEventArgs? mouseButtonEventArgs = args.MouseButtonEventArgs;
		if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Left)
		{
			args.Handle();
			_fingerModifier.SnapFingerRight(FingerModel.PartRole, _doc.BendSimulation, out IFingerStopPointInternal leftFingerPos, out IFingerStopPointInternal rightFingerPos);
			ICombinedBendDescriptorInternal cbd = _doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _doc.BendSimulation.State.ActiveStep.BendInfo.Order);
			SetFingerPosByUser(cbd, leftFingerPos, rightFingerPos);
			SaveUndoAndRecreateSimulation();
			RaiseRequestRepaint();
		}
	}
}
