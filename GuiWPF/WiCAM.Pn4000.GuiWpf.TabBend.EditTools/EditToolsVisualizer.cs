using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.BendSimulation;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiContracts.Billboards;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.GuiWpf.Services;
using WiCAM.Pn4000.GuiWpf.TabBend.OrderBillboards;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.ScreenControls.Controls.Billboards;
using WiCAM.Pn4000.ScreenControls.Controls.Billboards.Contents;
using WiCAM.Pn4000.ScreenD3D;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

internal class EditToolsVisualizer : SubViewModelBase, IEditToolsVisualizer, ISubViewModel, IStyleReceiver
{
	private readonly IEditToolsSelection _toolSelection;

	private readonly IToolsToMachineModel _toolsToMachine;

	private readonly ToolCalculationMode _toolCalculationMode;

	private readonly IShortcutSettingsCommon _shortcutSettingsCommon;

	private readonly IEditToolsBillboards _editToolsBillboards;

	private readonly IBendSelection _bendSelection;

	private readonly IOrderBillboardsViewModel _orderBillboardsViewModel;

	private readonly IEditToolsViewModel _editToolsViewModel;

	private readonly IModelStyleProvider _modelStyles;

	private readonly IStyleProvider _styleProvider;

	private readonly IDoc3d _doc;

	private readonly IScreen3DDoc _screenDoc;

	private readonly IUnitConverter _unitConverter;

	private readonly Dictionary<IToolPiece, IBillboard> _activePieceBillboards = new Dictionary<IToolPiece, IBillboard>();

	private readonly Dictionary<IToolPiece, IBillboard> _pieceToBillboard = new Dictionary<IToolPiece, IBillboard>();

	private bool _showBendModel;

	private bool _isActive;

	private ScreenD3D11? _screen => _screenDoc.Screen?.ScreenD3D;

	private Dictionary<Model, IToolPiece> _modelToPieces => _toolSelection.ModelToPieces;

	private Dictionary<IToolPiece, Model> _piecesToModel => _toolSelection.PiecesToModel;

	public EditToolsVisualizer(IEditToolsSelection toolSelection, IToolsToMachineModel toolsToMachine, ToolCalculationMode toolCalculationMode, IShortcutSettingsCommon shortcutSettingsCommon, IEditToolsBillboards editToolsBillboards, IDoc3d doc, IScreen3DDoc screenDoc, IModelStyleProvider modelStyles, IBendSelection bendSelection, IOrderBillboardsViewModel orderBillboardsViewModel, IEditToolsViewModel editToolsViewModel, IUnitConverter unitConverter, IStyleProvider styleProvider)
	{
		_toolSelection = toolSelection;
		_toolsToMachine = toolsToMachine;
		_toolCalculationMode = toolCalculationMode;
		_shortcutSettingsCommon = shortcutSettingsCommon;
		_editToolsBillboards = editToolsBillboards;
		_doc = doc;
		_screenDoc = screenDoc;
		_modelStyles = modelStyles;
		_bendSelection = bendSelection;
		_orderBillboardsViewModel = orderBillboardsViewModel;
		_editToolsViewModel = editToolsViewModel;
		_unitConverter = unitConverter;
		_styleProvider = styleProvider;
		_bendSelection.CurrentBendChanged += BendSelection_CurrentBendChanged;
		_toolSelection.SelectionChanged += _toolSelection_SelectionChanged;
		_toolSelection.DataRefreshed += _toolSelection_DataRefreshed;
		_editToolsBillboards.RequestRepaint += base.RaiseRequestRepaint;
		_styleProvider.RegisterBillboardStilesChanged(this);
		_doc.BendSimulationChanged += _doc_BendSimulationChanged;
		_doc.BendMachineChanged += _doc_BendMachineChanged;
	}

	private void _doc_BendMachineChanged()
	{
		VisualizeTools();
	}

	private void _styleProvider_BillboardStilesChanged()
	{
		AddOrUpdateBillboardsForPieces(updateAll: true);
		_screen?.Render(skipQueuedFrames: false);
	}

	private void _doc_BendSimulationChanged(ISimulationThread arg1, ISimulationThread arg2)
	{
		VisualizeTools();
	}

	private void BendSelection_CurrentBendChanged(IBendPositioning? obj)
	{
		VisualizeTools();
		bool flag = obj != null;
		if (_showBendModel != flag)
		{
			_showBendModel = flag;
			if (_showBendModel)
			{
				_screen?.RemoveModel(_doc.BendModel3D, render: false);
				_screen?.AddModel(_doc.BendModel3D);
				_editToolsBillboards.SetActive(active: true);
			}
			else
			{
				_screen?.RemoveModel(_doc.BendModel3D);
			}
		}
	}

	private void _toolSelection_DataRefreshed()
	{
		VisualizeTools();
	}

	private void _toolSelection_SelectionChanged()
	{
		RaiseRequestRepaint();
	}

	public void VisualizeTools()
	{
		if (!_isActive)
		{
			return;
		}
		IBendMachine bendMachine = _doc.BendSimulation?.State.MachineConfig;
		Model model = _doc.BendSimulation?.State.Part;
		_modelToPieces.Clear();
		_piecesToModel.Clear();
		_activePieceBillboards.Clear();
		if (_bendSelection.CurrentBend == null)
		{
			model = null;
			bendMachine = _doc.BendMachine?.CopyStructure();
			if (bendMachine != null)
			{
				foreach (IToolPiece allPiecesInSetup in _toolSelection.AllPiecesInSetups)
				{
					Model model2 = _toolsToMachine.AddToolPiece(allPiecesInSetup, bendMachine);
					_modelToPieces.Add(model2, allPiecesInSetup);
					_piecesToModel.Add(allPiecesInSetup, model2);
				}
			}
		}
		else
		{
			foreach (KeyValuePair<IToolPiece, Model> item in (_doc.BendSimulation?.State.ToolPieceToModel).EmptyIfNull())
			{
				_piecesToModel.Add(item.Key, item.Value);
				_modelToPieces.Add(item.Value, item.Key);
			}
		}
		Model model3 = bendMachine?.Geometry.Root;
		_toolSelection.CurrentMachine = bendMachine;
		_screen?.RemoveAllModels(render: false);
		_orderBillboardsViewModel.RemoveBillboards(render: false);
		if (model3 != null)
		{
			_screen?.AddModel(model3, render: false);
		}
		if (model != null)
		{
			_screen?.AddModel(model, render: false);
		}
	}

	public override bool Close()
	{
		_editToolsBillboards.HideBillboards();
		_editToolsViewModel.Close();
		_toolSelection.UnselectAll();
		return base.Close();
	}

	public override void SetActive(bool active)
	{
		base.SetActive(active);
		_isActive = active;
		if (active)
		{
			VisualizeTools();
			_editToolsBillboards.SetActive(active);
		}
	}

	public override void KeyUp(object sender, IPnInputEventArgs e)
	{
		base.KeyUp(sender, e);
		_editToolsBillboards?.KeyUp(sender, e);
		if (e.Handled || !_toolCalculationMode.UseNewToolCalculation || _shortcutSettingsCommon.Cancel.IsShortcut(e))
		{
			return;
		}
		KeyEventArgs? keyEventArgs = e.KeyEventArgs;
		if (keyEventArgs != null && keyEventArgs.Key == Key.R)
		{
			VisualizeTools();
			Model model = _toolSelection.CurrentMachine?.Geometry.Root;
			if (model != null)
			{
				_screen?.RemoveModel(model, render: false);
				_screen?.AddModel(model);
				_screen?.UpdateAllModelTransform(render: false);
				_screen?.UpdateAllModelGeometry(render: false);
				_screen?.UpdateAllModelAppearance(render: true);
			}
			e.Handle();
		}
	}

	public bool StartSubMenu(object sender, ITriangleEventArgs e)
	{
		return MouseSelectTriangleValidObject(sender, e) == true;
	}

	public override void MouseSelectTriangle(object sender, ITriangleEventArgs e)
	{
		base.MouseSelectTriangle(sender, e);
		_editToolsBillboards?.MouseSelectTriangle(this, e);
		if (MouseSelectTriangleValidObject(sender, e) == false)
		{
			Close();
		}
	}

	public bool? MouseSelectTriangleValidObject(object sender, ITriangleEventArgs e)
	{
		if (e.Args.Handled)
		{
			return null;
		}
		if (e.Model == null)
		{
			return false;
		}
		IPnInputEventArgs args = e.Args;
		if (args != null && args.MouseButtonEventArgs?.ChangedButton == MouseButton.Left)
		{
			if (!e.Args.ControlModifier && !e.Args.ShiftModifier)
			{
				_toolSelection.UnselectAll();
			}
			if (_modelToPieces.TryGetValue(e.Model, out IToolPiece value))
			{
				switch (_toolSelection.SelectionMode)
				{
				case EditToolSelectionModes.ToolPiece:
					_toolSelection.ToggleSelection(value);
					break;
				case EditToolSelectionModes.ToolSection:
					_toolSelection.ToggleSelection(value.ToolSection);
					break;
				case EditToolSelectionModes.Cluster:
					_toolSelection.ToggleSelection(value.ToolSection.Cluster);
					break;
				}
				e.Args.Handle();
				return true;
			}
			return false;
		}
		IPnInputEventArgs args2 = e.Args;
		if (args2 != null && args2.MouseButtonEventArgs?.ChangedButton == MouseButton.Right)
		{
			if (_modelToPieces.TryGetValue(e.Model, out IToolPiece value2))
			{
				if (!_toolSelection.SelectionAsPieces.Contains(value2))
				{
					_toolSelection.UnselectAll();
					switch (_toolSelection.SelectionMode)
					{
					case EditToolSelectionModes.ToolPiece:
						_toolSelection.ToggleSelection(value2);
						break;
					case EditToolSelectionModes.ToolSection:
						_toolSelection.ToggleSelection(value2.ToolSection);
						break;
					case EditToolSelectionModes.Cluster:
						_toolSelection.ToggleSelection(value2.ToolSection.Cluster);
						break;
					}
				}
				_editToolsBillboards.ShowBillboards(e.HitPoint);
				e.Args.Handle();
				return true;
			}
			return false;
		}
		return null;
	}

	public override void MouseMove(object sender, MouseEventArgs e)
	{
		base.MouseMove(sender, e);
		_editToolsBillboards?.MouseMove(this, e);
	}

	public override void ColorModelParts(IPaintTool paintTool)
	{
		base.ColorModelParts(paintTool);
		if (_toolSelection.CurrentSetups == null)
		{
			return;
		}
		HashSet<IToolPiece> badTools = _toolSelection.BadTools;
		foreach (IToolPiece item in _toolSelection.CurrentSetups.AllSections.SelectMany((IToolSection x) => x.Pieces))
		{
			if (_piecesToModel.TryGetValue(item, out Model value))
			{
				(Color?, Color?) colors = GetColors(highlight: false, badTools.Contains(item), item.Flipped);
				paintTool.SetModelColors(value, colors.Item2, colors.Item1, null);
			}
		}
		foreach (IToolPiece selectionAsPiece in _toolSelection.SelectionAsPieces)
		{
			if (_piecesToModel.TryGetValue(selectionAsPiece, out Model value2))
			{
				(Color?, Color?) colors2 = GetColors(highlight: true, badTools.Contains(selectionAsPiece), selectionAsPiece.Flipped);
				paintTool.SetModelColors(value2, colors2.Item2, colors2.Item1, _modelStyles.ToolPieceHighlightEdgeWidth);
			}
		}
		AddOrUpdateBillboardsForPieces();
		_editToolsBillboards.ColorModelParts(paintTool);
	}

	private (Color? edge, Color? face) GetColors(bool highlight, bool error, bool flipped)
	{
		Color value;
		Color value2;
		if (highlight)
		{
			if (error)
			{
				Color toolPieceHighlightEdgeColorError = _modelStyles.ToolPieceHighlightEdgeColorError;
				value = _modelStyles.ToolPieceHighlightFaceColorError;
				value2 = toolPieceHighlightEdgeColorError;
			}
			else
			{
				Color toolPieceHighlightEdgeColor = _modelStyles.ToolPieceHighlightEdgeColor;
				value = _modelStyles.ToolPieceHighlightFaceColor;
				value2 = toolPieceHighlightEdgeColor;
			}
		}
		else if (error)
		{
			Color toolPieceEdgeColorError = _modelStyles.ToolPieceEdgeColorError;
			value = _modelStyles.ToolPieceFaceColorError;
			value2 = toolPieceEdgeColorError;
		}
		else
		{
			Color toolPieceEdgeColor = _modelStyles.ToolPieceEdgeColor;
			value = _modelStyles.ToolPieceFaceColor;
			value2 = toolPieceEdgeColor;
		}
		if (flipped)
		{
			float num = 0.6f;
			value2.R *= num;
			value2.G *= num;
			value2.B *= num;
			value.R *= num;
			value.G *= num;
			value.B *= num;
		}
		return (edge: value2, face: value);
	}

	private void AddOrUpdateBillboardsForPieces(bool updateAll = false)
	{
		if (_screen == null)
		{
			return;
		}
		if (updateAll)
		{
			foreach (IBillboard value3 in _activePieceBillboards.Values)
			{
				_screen.RemoveBillboard(value3, render: false);
			}
			_activePieceBillboards.Clear();
		}
		HashSet<IToolPiece> hashSet = _activePieceBillboards.Keys.ToHashSet();
		foreach (IToolPiece selectionAsPiece in _toolSelection.SelectionAsPieces)
		{
			if (!hashSet.Remove(selectionAsPiece) && _piecesToModel.TryGetValue(selectionAsPiece, out Model value))
			{
				IBillboard billboard = CreateBillboard(selectionAsPiece);
				_activePieceBillboards[selectionAsPiece] = billboard;
				_screen.AddBillboard(billboard, value, render: false);
			}
		}
		foreach (IToolPiece item in hashSet)
		{
			if (_activePieceBillboards.Remove(item, out IBillboard value2))
			{
				_screen.RemoveBillboard(value2, render: false);
			}
		}
	}

	private IBillboard CreateBillboard(IToolPiece piece)
	{
		if (_pieceToBillboard.TryGetValue(piece, out IBillboard value))
		{
			return value;
		}
		string plainText = _unitConverter.Length.ToUi(piece.Length, 0).ToString(CultureInfo.CurrentCulture);
		TextContent content = new TextContent
		{
			PlainText = plainText,
			TextStyle = _styleProvider.BillboardTextStyle,
			Background = _styleProvider.BillboardBackgroundStyle
		};
		int num = ((!piece.PieceProfile.MultiToolProfile.IsUpper) ? 1 : (-1));
		double z = (piece.ToolSection.ZMin - piece.ToolSection.ZMax).Value * 0.5 * (double)num;
		Vector3d center = new Vector3d(piece.Length * 0.5, 0.0, z);
		Billboard billboard = new Billboard
		{
			Content = content,
			Center = center
		};
		_pieceToBillboard.Add(piece, billboard);
		return billboard;
	}

	public void BillboardStylesChanged()
	{
		_styleProvider_BillboardStilesChanged();
	}
}
