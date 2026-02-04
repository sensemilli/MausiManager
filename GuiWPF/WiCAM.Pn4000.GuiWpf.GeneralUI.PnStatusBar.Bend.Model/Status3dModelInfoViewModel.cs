using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.Telerik;
using WiCAM.Pn4000.GuiContracts.Billboards;
using WiCAM.Pn4000.GuiWpf.Services;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Enums;
using WiCAM.Pn4000.PN3D.Pipes;
using WiCAM.Pn4000.ScreenControls.Controls.Billboards;
using WiCAM.Pn4000.ScreenControls.Controls.Billboards.Contents;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend.Model;

internal class Status3dModelInfoViewModel : ViewModelBase, IStatus3dModelInfoViewModel, IPnStatusViewModel
{
	private const string GlyphWarning = "&#xe403;";

	private readonly ITranslator _translator;

	private readonly IUnitCurrentLanguage _unitCurrentLanguage;

	private readonly IPN3DDocPipe _docPipe;

	private readonly IScreen3DMain _screen3D;

	private readonly IRadGlyphConverter _radGlyphConverter;

	private readonly IStyleProvider _styleProvider;

	private bool _billboardsActive;

	private readonly ICollection<IBillboard> _billboards = new List<IBillboard>();

	private IDoc3d? _doc;

	private bool _isActive;

	public string Dimensions { get; private set; }

	public string ModelType { get; private set; }

	public Brush ModelTypeBackground { get; private set; }

	public string ComponentCounts { get; private set; }

	public WiCAM.Pn4000.BendModel.Model? CurrentModel { get; private set; }

	private Brush ColorOk => Brushes.White;

	private Brush ColorError => Brushes.Red;

	public IDoc3d? Doc
	{
		get
		{
			return _doc;
		}
		set
		{
			if (_doc != value)
			{
				if (_doc != null)
				{
					_doc.UpdateGeneralInfoAutoEvent -= _doc_UpdateGeneralInfoAutoEvent;
					_doc.UpdateGeneralInfoEvent -= _doc_UpdateGeneralInfoEvent;
				}
				_doc = value;
				if (_doc != null)
				{
					_doc.UpdateGeneralInfoAutoEvent += _doc_UpdateGeneralInfoAutoEvent;
					_doc.UpdateGeneralInfoEvent += _doc_UpdateGeneralInfoEvent;
				}
				NotifyPropertyChanged("Doc");
			}
			UpdateData(null);
		}
	}

	public ModelViewMode? CurrentViewMode { get; set; }

	private void _doc_UpdateGeneralInfoEvent(IDoc3d doc, ModelViewMode mode)
	{
		CurrentViewMode = mode;
		UpdateData(mode);
	}

	private void _doc_UpdateGeneralInfoAutoEvent(IDoc3d doc)
	{
		UpdateData(null);
	}

	public Status3dModelInfoViewModel(ITranslator translator, IUnitCurrentLanguage unitCurrentLanguage, IPN3DDocPipe docPipe, IScreen3DMain screen3D, IRadGlyphConverter radGlyphConverter, IStyleProvider styleProvider)
	{
		_translator = translator;
		_unitCurrentLanguage = unitCurrentLanguage;
		_docPipe = docPipe;
		_screen3D = screen3D;
		_radGlyphConverter = radGlyphConverter;
		_styleProvider = styleProvider;
		_translator.ResourcesChangedStrong += delegate
		{
			UpdateTranslations();
		};
		UpdateTranslations();
	}

	private void UpdateTranslations()
	{
	}

	public void SetActive(bool isActive)
	{
		_isActive = isActive;
		if (isActive)
		{
			UpdateData(null);
		}
	}

	private void UpdateData(ModelViewMode? viewMode)
	{
		if (!_isActive)
		{
			return;
		}
		IDoc3d doc = Doc;
		if (!viewMode.HasValue)
		{
			viewMode = CurrentViewMode;
			if (!viewMode.HasValue)
			{
				viewMode = ((!_docPipe.IsShowingUnfold) ? ModelViewMode.OriginalEntryModel : ModelViewMode.UnfoldModel);
			}
		}
		WiCAM.Pn4000.BendModel.Model model = GetModel(doc, viewMode.Value);
		if (model != null && model.SubModels.Count == 0 && model.Shells.Count == 0)
		{
			model = null;
		}
		CurrentModel = model;
		UpdateDimensions(model);
		UpdateModelType(model);
		UpdateFaceAndBendCount(model);
		NotifyPropertyChanged("Dimensions");
		NotifyPropertyChanged("ModelType");
		NotifyPropertyChanged("ModelTypeBackground");
		NotifyPropertyChanged("ComponentCounts");
		NotifyPropertyChanged("Doc");
		NotifyPropertyChanged("CurrentModel");
		NotifyPropertyChanged("CurrentViewMode");
	}

	private void UpdateDimensions(WiCAM.Pn4000.BendModel.Model? model)
	{
		string format = "0.##";
		Vector3d? vector3d = model?.PartInfo.Dimensions;
		if (vector3d.HasValue)
		{
			Dimensions = _unitCurrentLanguage.ConvertMmToCurrentUnit(vector3d.Value.X, addUnit: false, format) + " x " + _unitCurrentLanguage.ConvertMmToCurrentUnit(vector3d.Value.Y, addUnit: false, format) + " x " + _unitCurrentLanguage.ConvertMmToCurrentUnit(vector3d.Value.Z, addUnit: true, format);
		}
		else
		{
			Dimensions = "";
		}
	}

	private void UpdateModelType(WiCAM.Pn4000.BendModel.Model? model)
	{
		if (model?.PartInfo != null)
		{
			ModelType = model.PartInfo.PartType.ToString();
			if ((model.PartInfo.PartType.HasFlag(PartType.FlatSheetMetal) || model.PartInfo.PartType.HasFlag(PartType.UnfoldableSheetMetal) || model.PartInfo.PartType.HasFlag(PartType.Tube) || model.PartInfo.PartType.HasFlag(PartType.RollOffPlate)) && !model.PartInfo.PartType.HasFlag(PartType.SmallPart))
			{
				ModelTypeBackground = ColorOk;
			}
			else
			{
				ModelTypeBackground = ColorError;
			}
		}
		else
		{
			ModelTypeBackground = ColorOk;
			ModelType = "";
		}
	}

	private void UpdateFaceAndBendCount(WiCAM.Pn4000.BendModel.Model? model)
	{
		if (model != null)
		{
			ComponentCounts = $"FACES: {model.GetFaceCount()}   BENDS: {model.GetBendCount()}";
		}
		else
		{
			ComponentCounts = "";
		}
	}

	private WiCAM.Pn4000.BendModel.Model? GetModel(IDoc3d? doc, ModelViewMode viewMode)
	{
		if (doc == null)
		{
			return null;
		}
		return viewMode switch
		{
			ModelViewMode.InputModel => doc?.InputModel3D, 
			ModelViewMode.UnfoldModel => doc?.UnfoldModel3D, 
			ModelViewMode.ModifiedEntryModel => doc?.ModifiedEntryModel3D, 
			ModelViewMode.OriginalEntryModel => doc?.EntryModel3D, 
			_ => doc?.EntryModel3D, 
		};
	}

	public void ToggleBillboards()
	{
		_billboardsActive = !_billboardsActive;
		if (!_billboardsActive)
		{
			foreach (IBillboard billboard in _billboards)
			{
				_screen3D.ScreenD3D.RemoveBillboard(billboard, render: false);
			}
			_billboards.Clear();
			_screen3D.ScreenD3D.Render(skipQueuedFrames: false);
		}
		else
		{
			if (!CurrentViewMode.HasValue)
			{
				return;
			}
			WiCAM.Pn4000.BendModel.Model model = GetModel(Doc, CurrentViewMode.Value);
			if (model == null)
			{
				return;
			}
			BackgroundStyle billboardBackgroundStyle = _styleProvider.BillboardBackgroundStyle;
			billboardBackgroundStyle.BorderThickness = 0f;
			billboardBackgroundStyle.IsCircular = true;
			billboardBackgroundStyle.Padding = -1f;
			BackgroundStyle backgroundStyle = billboardBackgroundStyle;
			GlyphStyle billboardGlyphStyle = _styleProvider.BillboardGlyphStyle;
			billboardGlyphStyle.Color = WiCAM.Pn4000.BendModel.Base.Color.Red;
			GlyphStyle glyphStyle = billboardGlyphStyle;
			GlyphContent content = new GlyphContent(_radGlyphConverter)
			{
				Glyph = "&#xe403;",
				GlyphStyle = glyphStyle,
				Background = backgroundStyle
			};
			GlyphContent obj = new GlyphContent(_radGlyphConverter)
			{
				Glyph = "&#xe403;",
				GlyphStyle = glyphStyle
			};
			billboardBackgroundStyle = backgroundStyle;
			WiCAM.Pn4000.BendModel.Base.Color color = backgroundStyle.Color;
			color.A = 1f;
			billboardBackgroundStyle.Color = color;
			obj.Background = billboardBackgroundStyle;
			GlyphContent hoverContent = obj;
			FaceGroupModelMapping faceGroupModelMapping = new FaceGroupModelMapping(model);
			foreach (KeyValuePair<FaceGroup, List<Face>> notConformFace in model.PartInfo.NotConformFaces)
			{
				notConformFace.Deconstruct(out var key, out var value);
				FaceGroup fg = key;
				List<Face> list = value;
				WiCAM.Pn4000.BendModel.Model fgModel = faceGroupModelMapping.GetModel(fg);
				foreach (Face face in list)
				{
					Vector3d center = face.Mesh.Select((Triangle x) => x.Center).Average();
					ButtonBillboard buttonBillboard = new ButtonBillboard(delegate(IButtonBillboard x)
					{
						_screen3D.ScreenD3D.UpdateBillboardAppearance(x);
					})
					{
						Center = center,
						Content = content,
						HoverContent = hoverContent
					};
					buttonBillboard.OnMouseEnter += delegate
					{
						face.HighlightColor = face.Color.Modulate(1.1f, 0.2f);
						foreach (FaceHalfEdge item in face.BoundaryEdgesCcw)
						{
							item.HighlightColor = WiCAM.Pn4000.BendModel.Base.Color.Yellow;
							item.HighlightWidth = 3f;
						}
						_screen3D.ScreenD3D.UpdateModelAppearance(fgModel);
					};
					buttonBillboard.OnMouseLeave += delegate
					{
						face.HighlightColor = null;
						foreach (FaceHalfEdge item2 in face.BoundaryEdgesCcw)
						{
							item2.HighlightColor = null;
							item2.HighlightWidth = null;
						}
						_screen3D.ScreenD3D.UpdateModelAppearance(fgModel);
					};
					_screen3D.ScreenD3D.AddBillboard(buttonBillboard, fgModel, render: false);
					_billboards.Add(buttonBillboard);
				}
			}
			_screen3D.ScreenD3D.Render(skipQueuedFrames: false);
		}
	}
}
