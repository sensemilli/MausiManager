using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.GeometryGenerators;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Controls.InteractionsModes;
using WiCAM.Pn4000.ScreenD3D.Extensions;

namespace WiCAM.Pn4000.GuiWpf.GeneralSubWindow;

public class GeometryMeasureToolsViewModel : SubViewModelBase
{
	[Flags]
	private enum Mode
	{
		PickingMode = 1,
		PathMode = 2,
		PerpendicularMode = 4
	}

	private readonly IMainWindowBlock _windowBlock;

	private readonly IScreen3DMain _screen3DMain;

	private readonly IUnitConverter _unitConverter;

	private readonly ITranslator _translator;

	private IDoc3d _currentDoc;

	private Vector3d _latestInteractivePoint;

	private Vector3d? _latestInteractiveNormal;

	private Vector3d _interactivePoint1;

	private Vector3d? _interactiveNormal1;

	private List<WiCAM.Pn4000.BendModel.GeometryGenerators.Line> _oldLines;

	private WiCAM.Pn4000.BendModel.GeometryGenerators.Line _currentLine;

	private Screen3D Screen3D => _screen3DMain.Screen3D;

	public GeometryMeasureToolsViewModel(IScreen3DMain screen3D, IMainWindowBlock windowBlock, IUnitConverter unitConverter, ITranslator translator)
	{
		_windowBlock = windowBlock;
		_screen3DMain = screen3D;
		_unitConverter = unitConverter;
		_translator = translator;
	}

	public void Init(IDoc3d currentDoc)
	{
		_currentDoc = currentDoc;
	}

	public void SelectPoint3D()
	{
		if (_currentDoc != null)
		{
			_latestInteractivePoint = new Vector3d(double.MaxValue, double.MaxValue, double.MaxValue);
			Screen3D.InteractionMode = new SelectPointInteractionMode(Screen3D, InteractivePointSelection, EndInteractivePointSelection, SelectPointSelectionMode.Interactive);
			_windowBlock.BlockUI_Block(_currentDoc);
			Screen3D.InteractionMode?.Unblock();
		}
	}

	private void InteractivePointSelection(SelectPointResult result)
	{
		Vector3d vector3d = (_latestInteractivePoint = result.Point);
		_latestInteractiveNormal = result.pointNormal;
		List<string> list = new List<string> { $"{_unitConverter.Length.ToUi(vector3d.X, 4)} {_unitConverter.Length.ToUi(vector3d.Y, 4)} {_unitConverter.Length.ToUi(vector3d.Z, 4)}" };
		Mode mode = (((Keyboard.Modifiers & ModifierKeys.Control) > ModifierKeys.None) ? Mode.PickingMode : ((Mode)0));
		list.Add((mode.HasFlag(Mode.PickingMode) ? "->" : "") + _translator.Translate("SubView.GeometryMeasureTools.PickingMode"));
		Screen3D.SetScreenInfoText(list);
	}

	private void EndInteractivePointSelection(bool done)
	{
		Close();
	}

	public void Distance3D()
	{
		if (_currentDoc != null)
		{
			_oldLines = new List<WiCAM.Pn4000.BendModel.GeometryGenerators.Line>();
			_latestInteractivePoint = new Vector3d(double.MaxValue, double.MaxValue, double.MaxValue);
			Screen3D.InteractionMode = new SelectPointInteractionMode(Screen3D, InteractivePoint1DistanceMeasureSelection, EndInteractivePoint1DistanceMeasureSelection, SelectPointSelectionMode.Interactive);
			_windowBlock.BlockUI_Block(_currentDoc);
			Screen3D.InteractionMode?.Unblock();
		}
	}

	private void InteractivePoint1DistanceMeasureSelection(SelectPointResult result)
	{
		InteractivePointSelection(result);
	}

	private void EndInteractivePoint1DistanceMeasureSelection(bool done)
	{
		if (!done || _latestInteractivePoint.X == double.MaxValue)
		{
			Close();
			return;
		}
		_interactivePoint1 = _latestInteractivePoint;
		_interactiveNormal1 = _latestInteractiveNormal;
		Screen3D.InteractionMode = new SelectPointInteractionMode(Screen3D, InteractivePoint2DistanceMeasureSelection, EndInteractivePoint2DistanceMeasureSelection, SelectPointSelectionMode.Interactive);
	}

	private void InteractivePoint2DistanceMeasureSelection(SelectPointResult result)
	{
		Vector3d vector3d = result.Point;
		if ((Keyboard.Modifiers & ModifierKeys.Alt) > ModifierKeys.None && !_interactiveNormal1.HasValue)
		{
			return;
		}
		if ((Keyboard.Modifiers & ModifierKeys.Alt) > ModifierKeys.None && _interactiveNormal1.HasValue)
		{
			double num = (vector3d - _interactivePoint1).Dot(_interactiveNormal1.Value);
			vector3d = _interactivePoint1 + num * _interactiveNormal1.Value;
		}
		_latestInteractivePoint = vector3d;
		_latestInteractiveNormal = result.pointNormal;
		Vector3d vector3d2 = _latestInteractivePoint - _interactivePoint1;
		double length = vector3d2.Length;
		if (_currentLine != null)
		{
			Screen3D.ScreenD3D.RemoveModel(_currentLine);
		}
		if (Math.Abs(length) < 0.0001)
		{
			InteractivePointSelection(result);
			return;
		}
		List<string> list;
		if (_oldLines != null && _oldLines.Count > 0)
		{
			double d2 = length + _oldLines.Sum((WiCAM.Pn4000.BendModel.GeometryGenerators.Line x) => (x.EndPoint - x.StartPoint).Length);
			list = new List<string>();
			list.AddRange(new List<string>
			{
				string.Format(CultureInfo.InvariantCulture, "DTotal = {0} ", ConvertN(d2)),
				string.Format(CultureInfo.InvariantCulture, "D = {0} ", ConvertN(length)),
				string.Format(CultureInfo.InvariantCulture, "DX = {0} ", ConvertN(vector3d2.X)),
				string.Format(CultureInfo.InvariantCulture, "DY = {0} ", ConvertN(vector3d2.Y)),
				string.Format(CultureInfo.InvariantCulture, "DZ = {0} ", ConvertN(vector3d2.Z))
			});
			int num2 = 1;
			foreach (WiCAM.Pn4000.BendModel.GeometryGenerators.Line oldLine in _oldLines)
			{
				list.Add(string.Format(CultureInfo.InvariantCulture, "P{0} {1}", num2++, ConvertV(oldLine.StartPoint)));
			}
			list.Add(string.Format(CultureInfo.InvariantCulture, "P{0} {1}", num2++, ConvertV(_interactivePoint1)));
			list.Add(string.Format(CultureInfo.InvariantCulture, "P{0} {1}", num2++, ConvertV(vector3d)));
			AddControlManual(list);
		}
		else
		{
			list = new List<string>
			{
				string.Format(CultureInfo.InvariantCulture, "D = {0} ", ConvertN(length)),
				string.Format(CultureInfo.InvariantCulture, "DX = {0} ", ConvertN(vector3d2.X)),
				string.Format(CultureInfo.InvariantCulture, "DY = {0} ", ConvertN(vector3d2.Y)),
				string.Format(CultureInfo.InvariantCulture, "DZ = {0} ", ConvertN(vector3d2.Z)),
				string.Format(CultureInfo.InvariantCulture, "P1 {0}", ConvertV(_interactivePoint1)),
				string.Format(CultureInfo.InvariantCulture, "P2 {0}", ConvertV(vector3d))
			};
			AddControlManual(list);
		}
		Screen3D.SetScreenInfoText(list);
		WiCAM.Pn4000.BendModel.GeometryGenerators.Line line = new WiCAM.Pn4000.BendModel.GeometryGenerators.Line(_interactivePoint1, vector3d, new Color(1f, 0f, 0f, 1f), 1E-05, 5f)
		{
			ModelType = ModelType.System
		};
		Screen3D.ScreenD3D.AddModel(line);
		_currentLine = line;
		string ConvertN(double d)
		{
			d = _unitConverter.Length.ToUi(d, 4);
			return d.ToString();
		}
		string ConvertV(Vector3d v)
		{
			return ConvertN(v.X) + " " + ConvertN(v.Y) + " " + ConvertN(v.Z);
		}
	}

	private void AddControlManual(List<string> info)
	{
		Mode mode = (Mode)0;
		mode = (Mode)((int)mode | (((Keyboard.Modifiers & ModifierKeys.Alt) > ModifierKeys.None) ? 4 : 0));
		mode = (Mode)((int)mode | (((Keyboard.Modifiers & ModifierKeys.Shift) > ModifierKeys.None) ? 2 : 0));
		mode = (Mode)((int)mode | (((Keyboard.Modifiers & ModifierKeys.Control) > ModifierKeys.None) ? 1 : 0));
		info.Add((mode.HasFlag(Mode.PerpendicularMode) ? "->" : "") + _translator.Translate("SubView.GeometryMeasureTools.PerpendicularMode"));
		info.Add((mode.HasFlag(Mode.PathMode) ? "->" : "") + _translator.Translate("SubView.GeometryMeasureTools.PathMode"));
		info.Add((mode.HasFlag(Mode.PickingMode) ? "->" : "") + _translator.Translate("SubView.GeometryMeasureTools.PickingMode"));
	}

	private void EndInteractivePoint2DistanceMeasureSelection(bool done)
	{
		if ((Keyboard.Modifiers & ModifierKeys.Shift) > ModifierKeys.None)
		{
			_oldLines.Add(_currentLine);
			_interactivePoint1 = _currentLine.EndPoint;
			_currentLine = null;
			Screen3D.InteractionMode = new SelectPointInteractionMode(Screen3D, InteractivePoint2DistanceMeasureSelection, EndInteractivePoint2DistanceMeasureSelection, SelectPointSelectionMode.Interactive);
		}
		else
		{
			Close();
		}
	}

	public override bool Close()
	{
		if (_currentLine != null)
		{
			Screen3D.ScreenD3D.RemoveModel(_currentLine);
		}
		if (_oldLines != null)
		{
			foreach (WiCAM.Pn4000.BendModel.GeometryGenerators.Line oldLine in _oldLines)
			{
				Screen3D.ScreenD3D.RemoveModel(oldLine);
			}
			_oldLines = null;
		}
		_windowBlock.BlockUI_Unblock(_currentDoc);
		Screen3D.HideScreenInfoText();
		return base.Close();
	}

	public override void MouseSelectTriangle(object sender, ITriangleEventArgs e)
	{
		base.MouseSelectTriangle(sender, e);
		e.Args.Handle();
	}
}
