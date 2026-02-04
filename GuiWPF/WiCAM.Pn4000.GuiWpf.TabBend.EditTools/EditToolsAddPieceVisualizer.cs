using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.Contracts.ToolCalculation;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiWpf.Services;
using WiCAM.Pn4000.ScreenD3D;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Pn4000.ToolCalculation.Implementations;
using WiCAM.Pn4000.ToolCalculation.Interfaces;
using WiCAM.Pn4000.ToolCalculationMediator;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

internal class EditToolsAddPieceVisualizer : IEditToolsAddPieceVisualizer
{
	private readonly IScreen3DMain _screen3D;

	private readonly IEditToolsSelection _toolsSelection;

	private readonly IToolsToMachineModel _toolsToModel;

	private readonly IModelStyleProvider _modelStyles;

	private readonly IToolCalculations _toolCalculations;

	private readonly IToolOperator _toolOperator;

	private readonly IPnBndDoc _doc;

	private Model _ghostModel;

	private Model _dynamicSubModel;

	private Model _transparentSubModel;

	private readonly ICollection<Model> _invisibleModels = new List<Model>();

	private readonly Dictionary<IAdapterProfile, Model> _adapterModels = new Dictionary<IAdapterProfile, Model>();

	private readonly Dictionary<IDieFoldExtentionProfile, Model> _extensionModels = new Dictionary<IDieFoldExtentionProfile, Model>();

	private ScreenD3D11 ScreenD3D => _screen3D.ScreenD3D;

	public bool IsActive { get; private set; }

	public EditToolsAddPieceVisualizer(IScreen3DMain screen3D, IEditToolsSelection toolsSelection, IToolsToMachineModel toolsToModel, IModelStyleProvider modelStyles, IToolCalculations toolCalculations, IToolOperator toolOperator, IPnBndDoc doc)
	{
		_screen3D = screen3D;
		_toolsSelection = toolsSelection;
		_toolsToModel = toolsToModel;
		_modelStyles = modelStyles;
		_toolCalculations = toolCalculations;
		_toolOperator = toolOperator;
		_doc = doc;
	}

	public void ColorModelParts(IPaintTool painter)
	{
		foreach (Model invisibleModel in _invisibleModels)
		{
			painter.SetModelVisibility(invisibleModel, visible: false);
		}
	}

	public void StartAddingPieces(bool addLeft)
	{
		ICollection<IToolPiece> source;
		ICollection<IToolPiece> collection;
		switch (_toolsSelection.SelectionMode)
		{
		default:
			return;
		case EditToolSelectionModes.ToolPiece:
			(source, collection) = GetPiecesForAddPiece(_toolsSelection.SelectedPieces.First(), addLeft);
			break;
		case EditToolSelectionModes.ToolSection:
			source = _toolsSelection.SelectedSections.First().Pieces;
			collection = new List<IToolPiece>();
			break;
		case EditToolSelectionModes.Cluster:
			return;
		}
		if (IsActive)
		{
			return;
		}
		IsActive = true;
		_invisibleModels.Clear();
		_ghostModel = new Model();
		_dynamicSubModel = new Model(_ghostModel);
		foreach (IToolPiece item in collection)
		{
			Model model = _toolsSelection.PiecesToModel[item];
			_invisibleModels.Add(model);
			new Model(_dynamicSubModel)
			{
				Shell = model.Shell,
				Transform = model.WorldMatrix,
				FaceColor = _modelStyles.ToolPieceFaceColor,
				EdgeColor = _modelStyles.ToolPieceEdgeColor
			};
		}
		if (addLeft)
		{
			IToolPiece toolPiece = source.First();
			Model model2 = _toolsSelection.PiecesToModel[toolPiece];
			_transparentSubModel = new Model(_dynamicSubModel)
			{
				Transform = (toolPiece.Flipped ? Matrix4d.Translation(model2.WorldMatrix.TranslationVector - new Vector3d(toolPiece.Length, 0.0, 0.0)) : model2.WorldMatrix)
			};
		}
		else
		{
			IToolPiece toolPiece2 = source.Last();
			Model model3 = _toolsSelection.PiecesToModel[toolPiece2];
			_transparentSubModel = new Model(_ghostModel)
			{
				Transform = (toolPiece2.Flipped ? Matrix4d.Translation(model3.WorldMatrix.TranslationVector) : (Matrix4d.Translation(toolPiece2.Length, 0.0, 0.0) * model3.WorldMatrix))
			};
		}
		ScreenD3D.AddModel(_ghostModel, render: false);
	}

	public void StartAddingAdapters()
	{
		if (IsActive)
		{
			return;
		}
		IsActive = true;
		IToolSection toolSection = _toolsSelection.SelectedSections.First();
		IEnumerable<IToolPiece> piecesForAddAdapter = GetPiecesForAddAdapter(toolSection);
		_invisibleModels.Clear();
		_adapterModels.Clear();
		_extensionModels.Clear();
		Model toolSystemModel = _toolsToModel.GetToolSystemModel(_doc, toolSection.IsUpperSection);
		_ghostModel = new Model
		{
			Transform = toolSystemModel.WorldMatrix
		};
		_dynamicSubModel = new Model(_ghostModel);
		foreach (IToolPiece item in piecesForAddAdapter)
		{
			Model model = _toolsSelection.PiecesToModel[item];
			_invisibleModels.Add(model);
			new Model(_dynamicSubModel)
			{
				Shell = model.Shell,
				Transform = item.Transform,
				FaceColor = _modelStyles.ToolPieceFaceColor,
				EdgeColor = _modelStyles.ToolPieceEdgeColor
			};
		}
		_transparentSubModel = new Model(_ghostModel);
		ScreenD3D.AddModel(_ghostModel, render: false);
	}

	public void StartAddingExtensions(IToolProfile adapter)
	{
		if (!IsActive && adapter != null)
		{
			IsActive = true;
			IToolSection toolSection = _toolsSelection.SelectedSections.First();
			GetPiecesForAddAdapter(toolSection);
			_invisibleModels.Clear();
			_adapterModels.Clear();
			_extensionModels.Clear();
			_ghostModel = new Model
			{
				Transform = _toolsSelection.CurrentMachine.Geometry.LowerToolSystemTools.WorldMatrix * Matrix4d.Translation(0.0, toolSection.OffsetLocal.Y, toolSection.OffsetLocal.Z)
			};
			_dynamicSubModel = new Model(_ghostModel);
			_transparentSubModel = new Model(_ghostModel);
			ScreenD3D.AddModel(_ghostModel, render: false);
		}
	}

	private static (ICollection<IToolPiece>, ICollection<IToolPiece>) GetPiecesForAddPiece(IToolPiece piece, bool addLeft)
	{
		List<IToolPiece> pieces = piece.ToolSection.Pieces;
		int num = pieces.IndexOf(piece);
		if (addLeft)
		{
			List<IToolPiece> item = pieces.Skip(num).ToList();
			List<IToolPiece> item2 = pieces.Take(num).ToList();
			return (item, item2);
		}
		List<IToolPiece> item3 = pieces.Take(num + 1).ToList();
		List<IToolPiece> item4 = pieces.Skip(num + 1).ToList();
		return (item3, item4);
	}

	private static IEnumerable<IToolPiece> GetPiecesForAddAdapter(IToolSection refSection)
	{
		IToolCluster cluster = refSection.Cluster;
		bool isUpper = refSection.IsUpperSection;
		IEnumerable<IToolSection> enumerable = cluster.Sections.Where((IToolSection x) => x != refSection && x.IsUpperSection == isUpper);
		Interval interval = new Interval(refSection.OffsetLocalX, refSection.OffsetLocalX + refSection.Length);
		double anchorPointAbs = refSection.MultiToolProfile.AnchorPointAbs;
		List<IToolPiece> list = new List<IToolPiece>();
		list.AddRange(refSection.Pieces);
		foreach (IToolSection item in enumerable)
		{
			if (item.OffsetLocalX < interval.End && item.OffsetLocalX + item.Length > interval.Start && ((isUpper && item.OffsetLocal.Z < refSection.OffsetLocal.Z - anchorPointAbs + 1E-06) || (!isUpper && item.OffsetLocal.Z > refSection.OffsetLocal.Z + anchorPointAbs - 1E-06)))
			{
				list.AddRange(item.Pieces);
			}
		}
		return list;
	}

	public void AddPiece(IToolPieceProfile profile, bool addLeft)
	{
		if (!IsActive)
		{
			return;
		}
		double x = (addLeft ? (0.0 - profile.Length) : profile.Length);
		_dynamicSubModel.Transform = Matrix4d.Translation(x, 0.0, 0.0);
		ScreenD3D.UpdateModelTransform(_dynamicSubModel, render: false);
		foreach (Model subModel in _transparentSubModel.SubModels)
		{
			ScreenD3D.RemoveModel(subModel, render: false);
		}
		Model modelForProfile = _toolsToModel.GetModelForProfile(_doc.BendMachine, profile);
		modelForProfile.Opacity = 0.3;
		modelForProfile.FaceColor = _modelStyles.ToolPieceFaceColor;
		modelForProfile.EdgeColor = _modelStyles.ToolPieceEdgeColor;
		_transparentSubModel.SubModels.Clear();
		_transparentSubModel.SubModels.Add(modelForProfile);
		ScreenD3D.AddModel(modelForProfile, _transparentSubModel);
	}

	public void AddAdapter(IAdapterProfile profile)
	{
		if (!IsActive)
		{
			return;
		}
		IToolSection toolSection = _toolsSelection.SelectedSections.First();
		double z = (toolSection.IsUpperSection ? (0.0 - profile.WorkingHeight) : profile.WorkingHeight);
		_dynamicSubModel.Transform = Matrix4d.Translation(0.0, 0.0, z);
		ScreenD3D.UpdateModelTransform(_dynamicSubModel, render: false);
		foreach (Model subModel in _transparentSubModel.SubModels)
		{
			ScreenD3D.RemoveModel(subModel, render: false);
		}
		_transparentSubModel.SubModels.Clear();
		if (!_adapterModels.TryGetValue(profile, out Model value))
		{
			IToolCalculationOption option = _toolCalculations.CreateDefaultOptions(_doc);
			IToolSection toolSection2 = _toolOperator.AddAdapters(toolSection, profile, _doc.ToolManager, option);
			value = new Model
			{
				Parent = _transparentSubModel
			};
			foreach (IToolPiece piece in toolSection2.Pieces)
			{
				Model modelForProfile = _toolsToModel.GetModelForProfile(_doc.BendMachine, piece.PieceProfile);
				modelForProfile.Transform = piece.Transform;
				modelForProfile.Opacity = 0.3;
				modelForProfile.FaceColor = _modelStyles.ToolPieceFaceColor;
				modelForProfile.EdgeColor = _modelStyles.ToolPieceEdgeColor;
				modelForProfile.Parent = value;
				value.SubModels.Add(modelForProfile);
			}
			_adapterModels.Add(profile, value);
			_toolOperator.RemoveSectionAndMovePossibleChildren(toolSection2);
		}
		_transparentSubModel.SubModels.Add(value);
		ScreenD3D.AddModel(value, _transparentSubModel);
	}

	public void AddExtension(IDieFoldExtentionProfile profile)
	{
		if (!IsActive)
		{
			return;
		}
		IToolSection refSection = _toolsSelection.SelectedSections.First();
		foreach (Model subModel in _transparentSubModel.SubModels)
		{
			ScreenD3D.RemoveModel(subModel, render: false);
		}
		_transparentSubModel.SubModels.Clear();
		if (!_extensionModels.TryGetValue(profile, out Model value))
		{
			IToolCalculationOption option = _toolCalculations.CreateDefaultOptions(_doc);
			IToolSection toolSection = _toolOperator.AddExtensions(refSection, profile, _doc.ToolManager, option);
			value = new Model
			{
				Parent = _transparentSubModel
			};
			foreach (IToolPiece piece in toolSection.Pieces)
			{
				Model modelForProfile = _toolsToModel.GetModelForProfile(_doc.BendMachine, piece.PieceProfile);
				modelForProfile.Transform = piece.Transform;
				modelForProfile.Opacity = 0.3;
				modelForProfile.FaceColor = _modelStyles.ToolPieceFaceColor;
				modelForProfile.EdgeColor = _modelStyles.ToolPieceEdgeColor;
				modelForProfile.Parent = value;
				value.SubModels.Add(modelForProfile);
			}
			_extensionModels.Add(profile, value);
			_toolOperator.RemoveSectionAndMovePossibleChildren(toolSection);
		}
		_transparentSubModel.SubModels.Add(value);
		ScreenD3D.AddModel(value, _transparentSubModel);
	}

	public void Stop()
	{
		if (IsActive)
		{
			IsActive = false;
			_invisibleModels.Clear();
			ScreenD3D.RemoveModel(_ghostModel, render: false);
		}
	}
}
