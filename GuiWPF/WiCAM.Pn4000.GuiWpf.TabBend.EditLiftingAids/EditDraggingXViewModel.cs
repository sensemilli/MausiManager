using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiContracts.Billboards;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.GuiContracts.Popups;
using WiCAM.Pn4000.GuiWpf.TabBend.EditTools.Interafces;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Pn4000.ToolCalculation.Interfaces;
using WiCAM.Pn4000.ToolCalculationMediator;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditLiftingAids;

internal abstract class EditDraggingXViewModel : SubViewModelBase, IEditLiftingAidsViewModel, ISubViewModel, IPopupViewModel
{
	private readonly IShortcutSettingsCommon _shortcutSettingsCommon;

	private readonly IBillboardFactory _billboardFactory;

	private readonly IModelDragVisualizerX _modelDragVisualizerX;

	protected readonly IBendSelection _bendSelection;

	private readonly ISnapPointsCalculator _snapPointsCalculator;

	private readonly IToolCalculations _toolCalculations;

	protected readonly IPnBndDoc _doc;

	private readonly IntervalOperator<IRange> _intervalOperator;

	protected Model _draggingModel;

	private readonly BillboardStackPanel _billboardStackPanel;

	private readonly IScreen3DMain _screen;

	private List<IButtonBillboard> _billboards = new List<IButtonBillboard>();

	private bool _isDragging;

	private Model? _ghost;

	private Model? _dynamic;

	public EditDraggingXViewModel(IShortcutSettingsCommon shortcutSettingsCommon, IBillboardFactory billboardFactory, IScreen3DMain screen3DMain, IModelDragVisualizerX modelDragVisualizerX, IBendSelection bendSelection, ISnapPointsCalculator snapPointsCalculator, IToolCalculations toolCalculations, IPnBndDoc doc, IntervalOperator<IRange> intervalOperator)
	{
		_shortcutSettingsCommon = shortcutSettingsCommon;
		_billboardFactory = billboardFactory;
		_modelDragVisualizerX = modelDragVisualizerX;
		_bendSelection = bendSelection;
		_snapPointsCalculator = snapPointsCalculator;
		_toolCalculations = toolCalculations;
		_doc = doc;
		_intervalOperator = intervalOperator;
		_screen = screen3DMain;
		_billboardStackPanel = new BillboardStackPanel(screen3DMain.ScreenD3D);
		CreateBillboards();
	}

	private void CreateBillboards()
	{
		IButtonBillboard buttonBillboard = _billboardFactory.CreateButtonGlyph("&#xe141;", UpdateScreen);
		buttonBillboard.OnClick += CmdMoveClick;
		_billboards.Add(buttonBillboard);
		buttonBillboard = _billboardFactory.CreateButtonGlyph("&#xe551;", UpdateScreen);
		buttonBillboard.OnClick += CmdActivateClick;
		_billboards.Add(buttonBillboard);
	}

	private void UpdateScreen(IButtonBillboard b)
	{
		_screen.ScreenD3D.UpdateBillboardAppearance(b);
	}

	public void SelectModel(Model model)
	{
		_draggingModel = model;
		RaiseRequestRepaint();
	}

	private void ShowBillboards()
	{
		_billboardStackPanel.SetBillboards(_billboards, _draggingModel);
	}

	private void HideBillboards()
	{
		_billboardStackPanel.RemoveBillboards();
	}

	public override void ColorModelParts(IPaintTool paintTool)
	{
		base.ColorModelParts(paintTool);
		if (_isDragging)
		{
			paintTool.SetModelVisibility(_draggingModel, visible: false, applyToSubModels: true);
		}
		else
		{
			paintTool.SetModelFaceColor(_draggingModel, Color.Yellow, applyToSubModels: true);
		}
	}

	public override void MouseMove(object sender, MouseEventArgs e)
	{
		base.MouseMove(sender, e);
		if (!e.Handled)
		{
			_screen.Screen3D.RecalculateWpFPointToPixelPoint(e.GetPosition(_screen.Screen3D), out var X, out var Y);
			_modelDragVisualizerX.Drag(new Vector2f(X, Y));
		}
	}

	public override void KeyUp(object sender, IPnInputEventArgs e)
	{
		base.KeyUp(sender, e);
		if (!e.Handled && _shortcutSettingsCommon.Cancel.IsShortcut(e))
		{
			Cancel();
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
		KeyUp(sender, e.Args);
		if (e.Args.Handled)
		{
			return;
		}
		if (_isDragging)
		{
			Vector3d modelOrigin = _modelDragVisualizerX.ModelOrigin;
			double distance = _modelDragVisualizerX.Stop();
			MouseButtonEventArgs? mouseEventArgs = e.MouseEventArgs;
			if (mouseEventArgs != null && mouseEventArgs.ChangedButton == MouseButton.Left)
			{
				CommitDragging(distance, modelOrigin);
			}
			StopDragging();
			return;
		}
		bool? flag = MouseSelectTriangleValidObject(sender, e);
		if (flag.HasValue)
		{
			if (flag == true)
			{
				e.Args.Handle();
			}
			else
			{
				Cancel();
			}
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
		for (Model model = e.Model; model != null; model = model.Parent)
		{
			if (IsValidModel(model))
			{
				SelectModel(model);
				_billboardStackPanel.SetAnchor(e.HitPoint);
				MouseButtonEventArgs? mouseButtonEventArgs = e.Args.MouseButtonEventArgs;
				if (mouseButtonEventArgs != null && mouseButtonEventArgs.ChangedButton == MouseButton.Right)
				{
					ShowBillboards();
				}
				else
				{
					HideBillboards();
				}
				return true;
			}
		}
		return false;
	}

	public override bool Close()
	{
		_billboardStackPanel.RemoveBillboards();
		StopDragging();
		return base.Close();
	}

	private void StopDragging()
	{
		if (_isDragging)
		{
			_isDragging = false;
		}
		if (_ghost != null)
		{
			_screen.ScreenD3D.RemoveModel(_ghost);
			_ghost = null;
			_dynamic = null;
		}
		_modelDragVisualizerX.Stop();
	}

	public void Cancel()
	{
		Close();
	}

	private void CmdMoveClick(IPnInputEventArgs e, IBillboard arg2)
	{
		e.Handle();
		HideBillboards();
		_isDragging = true;
		Model model = _draggingModel.CopyStructure();
		_dynamic = _draggingModel.CopyStructure();
		foreach (Model item in model.GetAllSubModelsWithSelf())
		{
			item.Opacity = 0.6000000238418579;
		}
		foreach (Model item2 in _dynamic.GetAllSubModelsWithSelf())
		{
			item2.FaceColor = Color.Yellow;
		}
		_ghost = new Model();
		_ghost.SubModels.Add(model);
		Model model2 = new Model(_ghost);
		model2.SubModels.Add(_dynamic);
		model.Parent = _ghost;
		_dynamic.Parent = model2;
		_ghost.Transform = _draggingModel.WorldMatrix;
		double x = _ghost.Transform.Transform(Vector3d.Zero).X;
		_ghost.Transform *= Matrix4d.Translation(0.0, 0.0, 0.0);
		model.Transform = Matrix4d.Translation(0.0, -0.1, 0.1);
		model2.Transform = Matrix4d.Translation(0.0, 0.0, 0.0);
		_dynamic.Transform = Matrix4d.Identity;
		SnapOptions options = new SnapOptions
		{
			CalculationOptions = _toolCalculations.CreateDefaultOptions(_doc)
		};
		double startPositionForPlacingTools = _doc.BendMachine.LowerToolSystem.StartPositionForPlacingTools;
		double endPositionForPlacingTools = _doc.BendMachine.LowerToolSystem.EndPositionForPlacingTools;
		IEnumerable<ISnapPoint> snapPointsBendCenter = _snapPointsCalculator.GetSnapPointsBendCenter(_bendSelection.CurrentBend, options, x);
		List<IRange> blockedIntervals = new List<IRange>
		{
			_intervalOperator.CreateRange(double.NegativeInfinity, startPositionForPlacingTools - x),
			_intervalOperator.CreateRange(endPositionForPlacingTools - x, double.PositiveInfinity)
		};
		_modelDragVisualizerX.Start(_dynamic, _ghost, blockedIntervals, snapPointsBendCenter.ToList() ?? new List<ISnapPoint>());
		_screen.ScreenD3D.AddModel(_ghost);
		RaiseRequestRepaint();
	}

	protected abstract bool IsValidModel(Model m);

	protected abstract void CmdActivateClick(IPnInputEventArgs e, IBillboard arg2);

	protected abstract void CommitDragging(double distance, Vector3d origin);
}
