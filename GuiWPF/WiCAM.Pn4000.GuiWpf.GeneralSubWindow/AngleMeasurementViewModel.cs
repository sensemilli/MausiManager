using System;
using System.Collections.Generic;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.PN3D.Extensions;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.GeneralSubWindow;

internal class AngleMeasurementViewModel : SubViewModelBase
{
	private readonly IScreen3DMain _screen;

	private readonly IMainWindowBlock _mainWindowBlock;

	private readonly ITranslator _translator;

	private readonly IConfigProvider _configProvider;

	private Color _highlightColor;

	private Vector3d _normal1;

	private Face? _face1;

	private Model? _model1;

	private Face? _face2;

	private Model? _model2;

	private const string TextKeySelectFace1 = "SubView.AngleMeasurement.SelectFace1";

	private const string TextKeySelectFace2 = "SubView.AngleMeasurement.SelectFace2";

	private const string TextKeyHintCancel = "SubView.AngleMeasurement.HintCancel";

	private const string TextKeyResult = "SubView.AngleMeasurement.Result";

	private List<string> _currentMessages = new List<string>();

	public AngleMeasurementViewModel(IScreen3DMain screen, IMainWindowBlock mainWindowBlock, ITranslator translator, IConfigProvider configProvider)
	{
		_screen = screen;
		_mainWindowBlock = mainWindowBlock;
		_translator = translator;
		_configProvider = configProvider;
	}

	private void ShowMessage()
	{
		_screen.Screen3D.SetScreenInfoText(_currentMessages);
	}

	public void MeasureAngle()
	{
		ModelColors3DConfig modelColors3DConfig = _configProvider.InjectOrCreate<ModelColors3DConfig>();
		_highlightColor = modelColors3DConfig.SelectionHighlightFaceColor.ToBendColor();
		_currentMessages = new List<string>
		{
			_translator.Translate("SubView.AngleMeasurement.SelectFace1"),
			_translator.Translate("SubView.AngleMeasurement.HintCancel")
		};
		ShowMessage();
	}

	public override bool Close()
	{
		_face1 = null;
		_model1 = null;
		_face2 = null;
		_model2 = null;
		_screen.Screen3D.HideScreenInfoText();
		RaiseRequestRepaint();
		return base.Close();
	}

	public override void MouseSelectTriangle(object sender, ITriangleEventArgs e)
	{
		base.MouseSelectTriangle(sender, e);
		if (e.Args.Handled)
		{
			return;
		}
		MouseButtonEventArgs? mouseEventArgs = e.MouseEventArgs;
		if (mouseEventArgs != null && mouseEventArgs.ChangedButton == MouseButton.Right)
		{
			Close();
		}
		else
		{
			MouseButtonEventArgs? mouseEventArgs2 = e.MouseEventArgs;
			if (mouseEventArgs2 != null && mouseEventArgs2.ChangedButton == MouseButton.Left && e.Tri != null && e.Model != null)
			{
				if (_face1 != null && _face2 == null)
				{
					Vector3d normalized = e.Tri.Face.Shell.GetWorldMatrix(e.Model).TransformNormal(e.Tri.TriangleNormal).Normalized;
					_face2 = e.Tri.Face;
					_model2 = e.Model;
					double num = _normal1.UnsignedAngle(normalized) * 180.0 / Math.PI;
					_currentMessages = new List<string>
					{
						_translator.Translate("SubView.AngleMeasurement.Result", num),
						_translator.Translate("SubView.AngleMeasurement.SelectFace1"),
						_translator.Translate("SubView.AngleMeasurement.HintCancel")
					};
					ShowMessage();
				}
				else
				{
					_normal1 = e.Tri.Face.Shell.GetWorldMatrix(e.Model).TransformNormal(e.Tri.TriangleNormal).Normalized;
					_face1 = e.Tri.Face;
					_model1 = e.Model;
					_face2 = null;
					_model2 = null;
					_currentMessages = new List<string>
					{
						_translator.Translate("SubView.AngleMeasurement.SelectFace2"),
						_translator.Translate("SubView.AngleMeasurement.HintCancel")
					};
					ShowMessage();
				}
				RaiseRequestRepaint();
			}
		}
		e.Args.Handle();
	}

	public override void SetActive(bool active)
	{
		base.SetActive(active);
		if (active && _currentMessages.Count > 0)
		{
			ShowMessage();
		}
	}

	public override void ColorModelParts(IPaintTool paintTool)
	{
		base.ColorModelParts(paintTool);
		if (_face1 != null)
		{
			paintTool.SetFaceColor(_face1, _model1, _highlightColor);
		}
		if (_face2 != null)
		{
			paintTool.SetFaceColor(_face2, _model2, _highlightColor);
		}
	}
}
