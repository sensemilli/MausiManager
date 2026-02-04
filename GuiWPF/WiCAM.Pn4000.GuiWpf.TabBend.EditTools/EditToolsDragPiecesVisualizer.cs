using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiWpf.GeneralUI.Docking;
using WiCAM.Pn4000.GuiWpf.Services;
using WiCAM.Pn4000.GuiWpf.TabBend.EditTools.Interafces;
using WiCAM.Pn4000.GuiWpf.TabBend.SnapPointSettings;
using WiCAM.Pn4000.ScreenD3D;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Pn4000.ToolCalculation.Interfaces;
using WiCAM.Pn4000.ToolCalculationMediator;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

internal class EditToolsDragPiecesVisualizer : IEditToolsDragPiecesVisualizer, IEditToolsDragModelVisualizer
{
	private readonly IScreen3DMain _screen3D;

	private readonly IEditToolsSelection _toolsSelection;

	private readonly IToolsToMachineModel _toolsToModel;

	private readonly IModelStyleProvider _modelStyles;

	private readonly IModelDragVisualizerX _modelDragVisualizerX;

	private readonly ISnapPointsCalculator _snapPointsCalculator;

	private readonly IToolCalculations _toolCalculations;

	private readonly IBendSelection _bendSelection;

	private readonly IPnBndDoc _doc;

	private readonly IScopedFactorio _factorio;

	private readonly IDockingService _dockingService;

	private Model? _ghostModel;

	private readonly ICollection<Model> _invisibleModels = new List<Model>();

	private readonly List<Pair<double, Model>> _tmpModels = new List<Pair<double, Model>>();

	private Model _defaultTmpModel;

	private Model? _activeTmpModel;

	private ISnapPointViewModel _snapVm;

	private SnapOptions _options;

	private ScreenD3D11 ScreenD3D => _screen3D.ScreenD3D;

	public bool IsDragging => _modelDragVisualizerX.IsDragging;

	public EditToolsDragPiecesVisualizer(IScreen3DMain screen3D, IEditToolsSelection toolsSelection, IToolsToMachineModel toolsToModel, IModelStyleProvider modelStyles, IModelDragVisualizerX modelDragVisualizerX, ISnapPointsCalculator snapPointsCalculator, IToolCalculations toolCalculations, IBendSelection bendSelection, IPnBndDoc doc, IScopedFactorio factorio, IDockingService dockingService)
	{
		_screen3D = screen3D;
		_toolsSelection = toolsSelection;
		_toolsToModel = toolsToModel;
		_modelStyles = modelStyles;
		_modelDragVisualizerX = modelDragVisualizerX;
		_snapPointsCalculator = snapPointsCalculator;
		_toolCalculations = toolCalculations;
		_bendSelection = bendSelection;
		_doc = doc;
		_factorio = factorio;
		_dockingService = dockingService;
		_modelDragVisualizerX.DistanceChanged += SetActiveTmpModel;
	}

	public void ColorModelParts(IPaintTool painter)
	{
		foreach (Model invisibleModel in _invisibleModels)
		{
			painter.SetModelVisibility(invisibleModel, visible: false, applyToSubModels: true);
		}
		_modelDragVisualizerX.SnapPointVisualization();
	}

	public void MouseMove(object sender, MouseEventArgs e)
	{
		if (!e.Handled && IsDragging)
		{
			_screen3D.Screen3D.RecalculateWpFPointToPixelPoint(e.GetPosition(_screen3D.Screen3D), out var X, out var Y);
			_modelDragVisualizerX.Drag(new Vector2f(X, Y));
		}
	}

	void IEditToolsDragPiecesVisualizer.Start()
	{
		if (IsDragging)
		{
			return;
		}
		List<IToolPiece> selectedPieces = _toolsSelection.SelectionAsPieces.ToList();
		if (!selectedPieces.Any())
		{
			return;
		}
		_options = new SnapOptions
		{
			CalculationOptions = _toolCalculations.CreateDefaultOptions(_doc)
		};
		_invisibleModels.Clear();
		_tmpModels.Clear();
		_activeTmpModel = null;
		_ghostModel = new Model();
		Model model = new Model(_ghostModel);
		foreach (IToolPiece item in selectedPieces)
		{
			Model model2 = _toolsSelection.PiecesToModel[item];
			_invisibleModels.Add(model2);
			Matrix4d worldMatrix = model2.WorldMatrix;
			new Model(model)
			{
				Shell = model2.Shell,
				Transform = worldMatrix * Matrix4d.Scale(1.0, 1.01, 1.0),
				FaceColor = _modelStyles.ToolPieceHighlightFaceColor,
				EdgeColor = _modelStyles.ToolPieceHighlightEdgeColor,
				EdgeWidth = _modelStyles.ToolPieceHighlightEdgeWidth
			};
			if (selectedPieces.Count > 1)
			{
				new Model(_ghostModel)
				{
					Shell = model2.Shell,
					Transform = worldMatrix * Matrix4d.Scale(1.0, 0.99, 1.0),
					FaceColor = _modelStyles.ToolPieceFaceColor,
					EdgeColor = _modelStyles.ToolPieceEdgeColor,
					Opacity = 0.3
				};
			}
		}
		if (selectedPieces.Count == 1)
		{
			IToolPiece toolPiece = selectedPieces.First();
			IToolSection toolSection = toolPiece.ToolSection;
			_defaultTmpModel = new Model(_ghostModel)
			{
				SubModelsVisible = false
			};
			foreach (IToolPiece piece in toolSection.Pieces)
			{
				Model model3 = _toolsSelection.PiecesToModel[piece];
				Matrix4d worldMatrix2 = model3.WorldMatrix;
				_invisibleModels.Add(model3);
				Model model4 = new Model(_defaultTmpModel)
				{
					Shell = model3.Shell,
					Transform = worldMatrix2,
					FaceColor = _modelStyles.ToolPieceFaceColor,
					EdgeColor = _modelStyles.ToolPieceEdgeColor
				};
				if (piece == toolPiece)
				{
					model4.Opacity = 0.3;
					model4.Transform = worldMatrix2 * Matrix4d.Scale(1.0, 0.99, 1.0);
				}
			}
			int num = toolSection.Pieces.IndexOf(toolPiece);
			for (int i = 0; i < toolSection.Pieces.Count; i++)
			{
				Model model5 = new Model(_ghostModel)
				{
					SubModelsVisible = (i == num)
				};
				for (int j = 0; j < toolSection.Pieces.Count; j++)
				{
					if (j != num)
					{
						double x2 = 0.0;
						if (i < num && j < num && j >= i)
						{
							x2 = toolPiece.Length;
						}
						else if (i > num && j > num && j <= i)
						{
							x2 = 0.0 - toolPiece.Length;
						}
						Model model6 = _toolsSelection.PiecesToModel[toolSection.Pieces[j]];
						Matrix4d worldMatrix3 = model6.WorldMatrix;
						new Model(model5)
						{
							Shell = model6.Shell,
							Transform = worldMatrix3 * Matrix4d.Translation(x2, 0.0, 0.0),
							FaceColor = _modelStyles.ToolPieceFaceColor,
							EdgeColor = _modelStyles.ToolPieceEdgeColor
						};
					}
				}
				IToolPiece toolPiece2 = toolSection.Pieces[i];
				double num2 = toolPiece2.OffsetLocal.X - toolPiece.OffsetLocal.X;
				if (i > num)
				{
					num2 -= toolPiece.Length;
					num2 += toolPiece2.Length;
				}
				_tmpModels.Add(new Pair<double, Model>(num2, model5));
			}
		}
		IToolSetups currentSetup = _toolsSelection.CurrentSetups;
		IBendPositioning currentBend = _bendSelection.CurrentBend;
		_snapVm = _factorio.Resolve<ISnapPointViewModel>();
		_dockingService.Show(_snapVm, _factorio);
		_snapVm.Init(_options);
		_snapVm.OnChanged = SetSnapPointsPiece;
		_modelDragVisualizerX.Start(model, _toolsToModel.GetToolSystemModel(_doc, upper: false), new List<IRange>(), new List<ISnapPoint>());
		SetSnapPointsPiece();
		ScreenD3D.AddModel(_ghostModel, render: false);
		void SetSnapPointsPiece()
		{
			List<IRange> blockedIntervals = (from x in _snapPointsCalculator.GetBlockedIntervals(selectedPieces, currentSetup, _options, (from x in _bendSelection.CurrentBend.ToIEnumerable()
					where x != null
					select x).ToList(), _doc)
				orderby x.Start
				select x).ToList();
			List<ISnapPoint> snapPoints = _snapPointsCalculator.GetSnapPoints(selectedPieces, currentSetup, currentBend, _options).ToList();
			_modelDragVisualizerX.SetSnapPoints(blockedIntervals, snapPoints);
		}
	}

	void IEditToolsDragModelVisualizer.Start()
	{
		if (IsDragging)
		{
			return;
		}
		_invisibleModels.Clear();
		_ghostModel = new Model();
		Model model = new Model(_ghostModel);
		Model parent = new Model(_ghostModel);
		Model bendModel3D = _doc.BendModel3D;
		_invisibleModels.Add(bendModel3D);
		foreach (Model item in bendModel3D.GetAllSubModelsWithSelf())
		{
			if (item.Shell != null)
			{
				Matrix4d worldMatrix = item.WorldMatrix;
				new Model(model)
				{
					Shell = item.Shell,
					Transform = worldMatrix * Matrix4d.Translation(0.0, 0.0, 0.05),
					ModelType = ModelType.Part
				};
				new Model(parent)
				{
					Shell = item.Shell,
					Transform = worldMatrix * Matrix4d.Translation(0.0, 0.0, -0.05),
					ModelType = ModelType.Part,
					Opacity = 0.3
				};
			}
		}
		IToolSetups currentSetup = _toolsSelection.CurrentSetups;
		IBendPositioning currentBend = _bendSelection.CurrentBend;
		SnapOptions options = new SnapOptions
		{
			CalculationOptions = _toolCalculations.CreateDefaultOptions(_doc)
		};
		_snapVm = _factorio.Resolve<ISnapPointViewModel>();
		_dockingService.Show(_snapVm, _factorio);
		_snapVm.Init(options);
		_snapVm.OnChanged = SetSnapPoints;
		List<IRange> blockedIntervals = (from x in _snapPointsCalculator.GetBlockedIntervals(currentBend, currentSetup, options, _doc)
			orderby x.Start
			select x).ToList();
		List<ISnapPoint> snapPoints = _snapPointsCalculator.GetSnapPoints(currentBend, currentSetup, options).ToList();
		_modelDragVisualizerX.Start(model, _toolsToModel.GetToolSystemModel(_doc, upper: false), blockedIntervals, snapPoints);
		ScreenD3D.AddModel(_ghostModel, render: false);
		void SetSnapPoints()
		{
			List<IRange> blockedIntervals2 = (from x in _snapPointsCalculator.GetBlockedIntervals(currentBend, currentSetup, options, _doc)
				orderby x.Start
				select x).ToList();
			List<ISnapPoint> snapPoints2 = _snapPointsCalculator.GetSnapPoints(currentBend, currentSetup, options).ToList();
			_modelDragVisualizerX.SetSnapPoints(blockedIntervals2, snapPoints2);
		}
	}

	public double Stop()
	{
		if (!IsDragging)
		{
			return 0.0;
		}
		double result = _modelDragVisualizerX.Stop();
		_invisibleModels.Clear();
		if (_ghostModel != null)
		{
			ScreenD3D.RemoveModel(_ghostModel, render: false);
		}
		ISnapPointViewModel snapVm = _snapVm;
		if (snapVm != null)
		{
			snapVm.Close();
			return result;
		}
		return result;
	}

	private void SetActiveTmpModel(double distance)
	{
		if (_activeTmpModel != null)
		{
			_activeTmpModel.SubModelsVisible = false;
		}
		int? indexForDistance = GetIndexForDistance(distance);
		_activeTmpModel = (indexForDistance.HasValue ? _tmpModels[indexForDistance.Value].Item2 : _defaultTmpModel);
		if (_activeTmpModel != null)
		{
			_activeTmpModel.SubModelsVisible = true;
		}
	}

	public int? GetIndexForDistance(double distance)
	{
		SnapOptions options = _options;
		if (options == null || !options.MovePieceInSection)
		{
			return null;
		}
		if (_tmpModels.Count == 0 || distance < _tmpModels[0].Item1)
		{
			return null;
		}
		for (int i = 0; i < _tmpModels.Count - 1; i++)
		{
			int num = i + 1;
			Pair<double, Model> pair = _tmpModels[i];
			Pair<double, Model> pair2 = _tmpModels[num];
			if (!(distance > pair2.Item1))
			{
				return (Math.Abs(distance - pair.Item1) < Math.Abs(distance - pair2.Item1)) ? i : num;
			}
		}
		return null;
	}
}
