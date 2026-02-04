using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;
using SharpDX;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.Contracts.ToolCalculation;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiWpf.Services;
using WiCAM.Pn4000.ScreenControls.Controls.Billboards;
using WiCAM.Pn4000.ScreenControls.Controls.Billboards.Contents;
using WiCAM.Pn4000.ScreenD3D;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Pn4000.ToolCalculation.Implementations;
using WiCAM.Pn4000.ToolCalculation.Interfaces;
using WiCAM.Pn4000.ToolCalculationMediator;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

internal class EditToolsExtendPiecesVisualizer : IEditToolsExtendPiecesVisualizer
{
	private readonly IScreen3DMain _screen3D;

	private readonly IEditToolsSelection _toolsSelection;

	private readonly IToolsToMachineModel _toolsToModel;

	private readonly IModelStyleProvider _modelStyles;

	private readonly IToolOperator _toolOperator;

	private readonly IPnBndDoc _doc;

	private readonly IToolCalculations _toolCalculations;

	private readonly IUnitConverter _unitConverter;

	private readonly IStyleProvider _styleProvider;

	private Model? _ghostModel;

	private readonly ICollection<Model> _invisibleModels = new List<Model>();

	private Vector3d _lastPosition;

	private Vector3d? _startingPosition;

	private WiCAM.Pn4000.BendModel.Base.Plane? _draggingPlane;

	private List<IToolSection> _selectedSections;

	private readonly List<List<Interval>> _intervals = new List<List<Interval>>();

	private readonly List<Dictionary<double, Model>> _models = new List<Dictionary<double, Model>>();

	private readonly List<Model> _parents = new List<Model>();

	private readonly Dictionary<IMultiToolProfile, Dictionary<IAliasPieceProfile, int>> _amounts = new Dictionary<IMultiToolProfile, Dictionary<IAliasPieceProfile, int>>();

	private IBendMachineGeometry? _bendMachineGeometry => _toolsSelection.CurrentMachine?.Geometry;

	private ScreenD3D11 ScreenD3D => _screen3D.ScreenD3D;

	public bool IsExtendingLeft { get; private set; }

	public bool IsActive { get; private set; }

	public EditToolsExtendPiecesVisualizer(IScreen3DMain screen3D, IEditToolsSelection toolsSelection, IToolsToMachineModel toolsToModel, IModelStyleProvider modelStyles, IToolOperator toolOperator, IPnBndDoc doc, IToolCalculations toolCalculations, IUnitConverter unitConverter, IStyleProvider styleProvider)
	{
		_screen3D = screen3D;
		_toolsSelection = toolsSelection;
		_toolsToModel = toolsToModel;
		_modelStyles = modelStyles;
		_toolOperator = toolOperator;
		_doc = doc;
		_toolCalculations = toolCalculations;
		_unitConverter = unitConverter;
		_styleProvider = styleProvider;
	}

	public void MouseMove(object sender, MouseEventArgs e)
	{
		if (!e.Handled && IsActive)
		{
			_screen3D.Screen3D.RecalculateWpFPointToPixelPoint(e.GetPosition(_screen3D.Screen3D), out var X, out var Y);
			Extending(new Vector2f(X, Y));
		}
	}

	public void ColorModelParts(IPaintTool painter)
	{
		foreach (Model invisibleModel in _invisibleModels)
		{
			painter.SetModelVisibility(invisibleModel, visible: false);
		}
	}

	public void Start(bool extendLeft)
	{
		if (IsActive)
		{
			return;
		}
		IsExtendingLeft = extendLeft;
		_selectedSections = _toolsSelection.SelectedSections.ToList();
		if (!_selectedSections.Any())
		{
			return;
		}
		IsActive = true;
		_invisibleModels.Clear();
		_intervals.Clear();
		_parents.Clear();
		_amounts.Clear();
		_models.Clear();
		foreach (IMultiToolProfile item in _selectedSections.Select((IToolSection x) => x.MultiToolProfile).Distinct().ToList())
		{
			Dictionary<IAliasPieceProfile, int> value = _toolOperator.CountToolProfileAmount(_toolsSelection.CurrentSetups.Root, item);
			_amounts.Add(item, value);
		}
		_ghostModel = new Model();
		foreach (IToolSection selectedSection in _selectedSections)
		{
			Model model = new Model(_ghostModel);
			Matrix4d transform = ((!selectedSection.IsUpperSection) ? _bendMachineGeometry?.LowerToolSystemTools.WorldMatrix : _bendMachineGeometry?.UpperToolSystemTools.WorldMatrix) ?? Matrix4d.Identity;
			model.Transform = transform;
			Dictionary<IAliasPieceProfile, int> usedAmounts = _amounts[selectedSection.MultiToolProfile];
			foreach (IToolPiece piece in selectedSection.Pieces)
			{
				_doc.BendMachine.ToolConfig.ToolManager.ToolListManager.AddUsedAmount(piece.PieceProfile, usedAmounts, -1);
			}
			_parents.Add(model);
		}
		for (int i = 0; i < _selectedSections.Count; i++)
		{
			Model model2 = new Model(_parents[i]);
			IToolSection toolSection = _selectedSections[i];
			foreach (IToolPiece piece2 in toolSection.Pieces)
			{
				Model model3 = _toolsSelection.PiecesToModel[piece2];
				_invisibleModels.Add(model3);
				new Model(model2)
				{
					Shell = model3.Shell,
					Transform = piece2.Transform,
					FaceColor = _modelStyles.ToolPieceFaceColor,
					EdgeColor = _modelStyles.ToolPieceEdgeColor
				};
			}
			_models.Add(new Dictionary<double, Model> { { toolSection.Length, model2 } });
			IToolSection toolSection2 = CreateDummySection(toolSection, toolSection.Length + 0.5, SelectionStrategy.GreaterEqual);
			CreateModelForSection(toolSection2, i);
			List<List<Interval>> intervals = _intervals;
			int num = 1;
			List<Interval> list = new List<Interval>(num);
			CollectionsMarshal.SetCount(list, num);
			Span<Interval> span = CollectionsMarshal.AsSpan(list);
			int index = 0;
			span[index] = new Interval(toolSection.Length, toolSection2.Length);
			intervals.Add(list);
		}
		Vector3d vector3d = (_bendMachineGeometry?.LowerToolSystemTools.WorldMatrix ?? Matrix4d.Identity).Transform(Vector3d.Zero);
		_draggingPlane = new WiCAM.Pn4000.BendModel.Base.Plane(vector3d, Vector3d.UnitY);
		_startingPosition = vector3d;
		ScreenD3D.AddModel(_ghostModel, render: false);
	}

	public IDictionary<IToolSection, double> Stop()
	{
		Dictionary<IToolSection, double> dictionary = new Dictionary<IToolSection, double>();
		if (!IsActive)
		{
			return dictionary;
		}
		IsActive = false;
		double num = 0.0;
		if (_startingPosition.HasValue)
		{
			num = _lastPosition.X - _startingPosition.Value.X;
		}
		_lastPosition = Vector3d.Zero;
		_startingPosition = null;
		_draggingPlane = null;
		_invisibleModels.Clear();
		if (_ghostModel != null)
		{
			ScreenD3D.RemoveModel(_ghostModel, render: false);
		}
		for (int i = 0; i < _selectedSections.Count; i++)
		{
			IToolSection toolSection = _selectedSections[i];
			List<Interval> list = _intervals[i];
			double x2 = toolSection.OffsetWorld.X;
			double length = Math.Max(IsExtendingLeft ? (x2 + toolSection.Length - num) : (num - x2), 0.0);
			int index = list.TakeWhile((Interval x) => x.Start < length + 1E-06).Count() - 1;
			Interval interval = list[index];
			bool flag = Math.Abs(interval.Start - length) < Math.Abs(interval.End - length);
			double value = ((interval.Start < 1E-06) ? interval.End : (flag ? interval.Start : interval.End));
			dictionary.Add(toolSection, value);
		}
		return dictionary;
	}

	private void Extending(Vector2f pos)
	{
		if (!IsActive)
		{
			return;
		}
		Matrix transformation = ScreenD3D.Renderer.Root.Transform ?? Matrix.Identity;
		ScreenD3D.CreateRay(pos.X, pos.Y, ref transformation, out var eyePos, out var eyeDir);
		Vector3d? startingPosition = _draggingPlane?.IntersectRay(eyePos, eyeDir);
		if (!startingPosition.HasValue)
		{
			return;
		}
		_lastPosition = startingPosition.Value;
		Vector3d? startingPosition2 = _startingPosition;
		if (!startingPosition2.HasValue)
		{
			_startingPosition = startingPosition;
		}
		double num = _lastPosition.X - _startingPosition.Value.X;
		for (int i = 0; i < _selectedSections.Count; i++)
		{
			Model model = _parents[i];
			foreach (Model subModel in model.SubModels)
			{
				ScreenD3D.RemoveModel(subModel, render: false);
			}
			model.SubModels.Clear();
			double sectionLength;
			Model model2 = GetModel(num, i, out sectionLength);
			model.SubModels.Add(model2);
			ScreenD3D.AddModel(model2, model, render: false);
			IToolSection section = _selectedSections[i];
			Billboard billboard = CreateBillboard(section, num, sectionLength);
			ScreenD3D.AddBillboard(billboard, model2, render: false);
		}
		ScreenD3D.Render(skipQueuedFrames: false);
	}

	private IToolSection CreateDummySection(IToolSection refSection, double length, SelectionStrategy strategy)
	{
		Dictionary<IAliasPieceProfile, int> amounts = _amounts[refSection.MultiToolProfile].ToDictionary<KeyValuePair<IAliasPieceProfile, int>, IAliasPieceProfile, int>((KeyValuePair<IAliasPieceProfile, int> x) => x.Key, (KeyValuePair<IAliasPieceProfile, int> x) => x.Value);
		IToolSection toolSection = _toolOperator.CreateDummySection(refSection, length, strategy, amounts, _doc.ToolManager, _toolCalculations.CreateDefaultOptions(_doc));
		if (IsExtendingLeft)
		{
			double x2 = refSection.OffsetLocalX + refSection.Length - toolSection.Length;
			Vector3d offsetLocal = refSection.OffsetLocal;
			offsetLocal.X = x2;
			toolSection.OffsetLocal = offsetLocal;
		}
		else
		{
			toolSection.OffsetLocal = refSection.OffsetLocal;
		}
		return toolSection;
	}

	private Model GetModel(double x, int i, out double sectionLength)
	{
		IToolSection toolSection = _selectedSections[i];
		List<Interval> list = _intervals[i];
		double x2 = toolSection.OffsetWorld.X;
		double length = Math.Max(IsExtendingLeft ? (x2 + toolSection.Length - x) : (x - x2), 0.0);
		int num = list.TakeWhile((Interval x) => x.Start < length + 1E-06).Count() - 1;
		if (num < 0 || list[num].End < length - 1E-06)
		{
			IToolSection toolSection2 = CreateDummySection(toolSection, length, SelectionStrategy.LessEqual);
			IToolSection toolSection3 = CreateDummySection(toolSection, length + 0.5, SelectionStrategy.GreaterEqual);
			CreateModelForSection(toolSection2, i);
			CreateModelForSection(toolSection3, i);
			num++;
			list.Insert(num, new Interval(toolSection2.Length, toolSection3.Length));
		}
		Interval interval = list[num];
		bool flag = Math.Abs(interval.Start - length) < Math.Abs(interval.End - length);
		double key = (sectionLength = ((interval.Start < 1E-06) ? interval.End : (flag ? interval.Start : interval.End)));
		return _models[i][key];
	}

	private void CreateModelForSection(IToolSection section, int i)
	{
		Dictionary<double, Model> dictionary = _models[i];
		if (dictionary.ContainsKey(section.Length))
		{
			return;
		}
		ToolPieceSortingStrategies strategy = ((!IsExtendingLeft) ? ToolPieceSortingStrategies.SmallPiecesRight : ToolPieceSortingStrategies.SmallPiecesLeft);
		_toolOperator.OrderPiecesInSection(section, strategy);
		Model model = new Model
		{
			Parent = _parents[i]
		};
		foreach (IToolPiece piece in section.Pieces)
		{
			Model modelForProfile = _toolsToModel.GetModelForProfile(_doc.BendMachine, piece.PieceProfile);
			modelForProfile.Transform = piece.Transform;
			modelForProfile.Parent = model;
			modelForProfile.FaceColor = _modelStyles.ToolPieceFaceColor;
			modelForProfile.EdgeColor = _modelStyles.ToolPieceEdgeColor;
			model.SubModels.Add(modelForProfile);
		}
		dictionary.Add(section.Length, model);
	}

	private Billboard CreateBillboard(IToolSection section, double offsetX, double newSectionLength)
	{
		bool flag = _screen3D.ScreenD3D.IsCamaraInFront();
		int num = (section.IsUpperSection ? 1 : (-1));
		int num2 = ((!(IsExtendingLeft ^ flag)) ? 1 : (-1));
		double z = (section.ZMin - section.ZMax).Value * 0.5 * (double)num;
		string plainText = _unitConverter.Length.ToUi(newSectionLength, 0).ToString(CultureInfo.CurrentCulture) + " " + _unitConverter.Length.Unit;
		TextContent textContent = new TextContent
		{
			PlainText = plainText,
			TextStyle = _styleProvider.BillboardTextStyle,
			Background = _styleProvider.BillboardBackgroundStyle
		};
		double num3 = 10 * num2;
		Vector2d offset = new Vector2d(textContent.Extends.X * 0.5 * (double)num2 + num3, 0.0);
		return new Billboard
		{
			Content = textContent,
			Offset = offset,
			Center = new Vector3d(offsetX, 0.0, z)
		};
	}
}
