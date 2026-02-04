using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools.Validations;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PaintTools;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.PN3D.Converter;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Enums;
using WiCAM.Pn4000.PN3D.Extensions;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.TabUnfold;

public class ValidationViewModel : SubViewModelBase, IValidationViewModel, ISubViewModel
{
	public class ValidationErrorIntrinsic : ViewModelBase
	{
		private bool _isSelected;

		public string Type { get; set; }

		public string ErrorDesc { get; set; }

		public HashSet<FaceHalfEdge> Edges { get; set; }

		public HashSet<Face> Faces { get; set; }

		public bool IsSelected
		{
			get
			{
				return _isSelected;
			}
			set
			{
				if (_isSelected != value)
				{
					_isSelected = value;
					NotifyPropertyChanged("IsSelected");
				}
			}
		}
	}

	public class ValidationErrorDistance : ViewModelBase
	{
		private bool _isSelected;

		public string Type1 { get; set; }

		public string Type2 { get; set; }

		public HashSet<FaceHalfEdge> Edges1 { get; set; }

		public HashSet<Face> Faces1 { get; set; }

		public HashSet<FaceHalfEdge> Edges2 { get; set; }

		public HashSet<Face> Faces2 { get; set; }

		public bool IsSelected
		{
			get
			{
				return _isSelected;
			}
			set
			{
				if (_isSelected != value)
				{
					_isSelected = value;
					NotifyPropertyChanged("IsSelected");
				}
			}
		}
	}

	private readonly IScreen3DMain _screen3d;

	private readonly ITranslator _translator;

	private IDoc3d _currentDoc;

	private Color _highlightColor;

	private bool _isActive;

	private HashSet<FaceHalfEdge> _highlightedEdges = new HashSet<FaceHalfEdge>();

	private bool _valiClickmode;

	private Visibility _visibilityValidationResults = Visibility.Collapsed;

	private bool _showValiResultDetails;

	private Visibility _visibilityValiResultDetails = Visibility.Collapsed;

	private ModelViewMode _currentViewMode;

	public int ValidationErrorCount { get; set; }

	public Visibility VisibilityValidationResults
	{
		get
		{
			return _visibilityValidationResults;
		}
		set
		{
			if (_visibilityValidationResults != value)
			{
				_visibilityValidationResults = value;
				NotifyPropertyChanged("VisibilityValidationResults");
				if (_visibilityValidationResults == Visibility.Visible && _showValiResultDetails)
				{
					VisibilityValiResultDetails = Visibility.Visible;
				}
				else
				{
					VisibilityValiResultDetails = Visibility.Collapsed;
				}
			}
		}
	}

	public Visibility VisibilityValiResultDetails
	{
		get
		{
			return _visibilityValiResultDetails;
		}
		set
		{
			if (_visibilityValiResultDetails != value)
			{
				_visibilityValiResultDetails = value;
				NotifyPropertyChanged("VisibilityValiResultDetails");
			}
		}
	}

	public Visibility VisibilityValiOk { get; set; } = Visibility.Collapsed;

	public Visibility VisibilityValiWarning { get; set; } = Visibility.Collapsed;

	public Visibility SelfCollisionDetected { get; set; } = Visibility.Collapsed;

	public ObservableCollection<ValidationErrorIntrinsic> ValiIntrinsicErrors { get; set; } = new ObservableCollection<ValidationErrorIntrinsic>();

	public ObservableCollection<ValidationErrorDistance> ValiDistancesErrors { get; set; } = new ObservableCollection<ValidationErrorDistance>();

	public RelayCommand CmdExpandValiResults { get; set; }

	public ValidationViewModel(IScreen3DMain screen3d, ITranslator translator, IDoc3d currentDoc, IConfigProvider configProvider)
	{
		_screen3d = screen3d;
		_translator = translator;
		CmdExpandValiResults = new RelayCommand(ToggleExpandValiResults);
		Color highlightColor = configProvider.InjectOrCreate<ModelColors3DConfig>().SelectedObjectHighlightBorderColorSecondary.ToBendColor();
		_highlightColor = highlightColor;
		_currentDoc = currentDoc;
		_currentDoc.ValidationResultChanged += ValidationResultsChanged;
		ValidationResultsChanged(null, _currentDoc?.ValidationResults);
		SetActive(active: true);
	}

	public void Init(IDoc3d currentDoc, Color highlightColor, ModelViewMode viewMode)
	{
		_currentViewMode = viewMode;
		_highlightColor = highlightColor;
		_currentDoc = currentDoc;
		_currentDoc.ValidationResultChanged += ValidationResultsChanged;
		ValidationResultsChanged(null, _currentDoc?.ValidationResults);
		SetActive(active: true);
	}

	private void ToggleExpandValiResults(object obj)
	{
		_showValiResultDetails = !_showValiResultDetails;
		if (_visibilityValidationResults == Visibility.Visible && _showValiResultDetails)
		{
			VisibilityValiResultDetails = Visibility.Visible;
		}
		else
		{
			VisibilityValiResultDetails = Visibility.Collapsed;
		}
	}

	public override bool Close()
	{
		if (_isActive)
		{
			_screen3d.Screen3D.HideScreenInfoText();
		}
		_currentDoc.ValidationResultChanged -= ValidationResultsChanged;
		return base.Close();
	}

	public override void SetActive(bool active)
	{
		base.SetActive(active);
		if (active != _isActive)
		{
			_isActive = active;
			_ = _isActive;
		}
	}

	public override void MouseSelectTriangle(object sender, ITriangleEventArgs e)
	{
		base.MouseSelectTriangle(sender, e);
		if (e.MouseEventArgs.ChangedButton == MouseButton.Left)
		{
			ValidationFaceClicked(e.Tri?.Face);
			e.MouseEventArgs.Handled = true;
		}
	}

	public override void ColorModelParts(IPaintTool paintTool)
	{
		base.ColorModelParts(paintTool);
		if (!_isActive)
		{
			return;
		}
		foreach (FaceHalfEdge highlightedEdge in _highlightedEdges)
		{
			paintTool.SetEdgeColorInShell(highlightedEdge, _highlightColor, 5f);
		}
	}

	private string Convertmm(double value, bool appendUnit, int decimalPlace = 2)
	{
		string text = "mm";
		if (SystemConfiguration.UseInch)
		{
			value = Convert.MmToInch(value);
			text = "inch";
			decimalPlace += 2;
		}
		if (decimalPlace < 0)
		{
			decimalPlace = 0;
		}
		string text2 = string.Empty.PadLeft(decimalPlace, '#');
		string text3 = string.Format("{0:0." + text2 + "}", value).Replace(',', '.');
		if (appendUnit)
		{
			text3 = text3 + " " + text;
		}
		return text3;
	}

	private string TranslateMacro(string macroName)
	{
		return _translator.Translate("l_popup.PnInterfaceSettings.Macro" + macroName) ?? macroName;
	}

	private string Translate(string key)
	{
		return _translator.Translate(key) ?? key;
	}

	public string ValidationResultTranslator(string key)
	{
		return ((string)Application.Current.FindResource("UnfoldView." + key)) ?? "Error";
	}

	private void ValidationFaceClicked(Face face)
	{
		_valiClickmode = true;
		foreach (ValidationErrorIntrinsic valiIntrinsicError in ValiIntrinsicErrors)
		{
			valiIntrinsicError.IsSelected = valiIntrinsicError.Faces.Contains(face);
		}
		foreach (ValidationErrorDistance valiDistancesError in ValiDistancesErrors)
		{
			valiDistancesError.IsSelected = valiDistancesError.Faces1.Contains(face);
		}
		_valiClickmode = false;
		ValidationSelectedChanged(originUser: false);
	}

	public void ValidationSelectedChanged(bool originUser)
	{
		if (_valiClickmode)
		{
			return;
		}
		_highlightedEdges.Clear();
		IEnumerable<FaceHalfEdge> first = ValiIntrinsicErrors.Where((ValidationErrorIntrinsic x) => x.IsSelected).SelectMany((ValidationErrorIntrinsic x) => x.Edges);
		IEnumerable<FaceHalfEdge> second = ValiDistancesErrors.Where((ValidationErrorDistance x) => x.IsSelected).SelectMany((ValidationErrorDistance x) => x.Edges1.Concat(x.Edges2));
		foreach (FaceHalfEdge item in first.Concat(second))
		{
			_highlightedEdges.Add(item);
		}
		if (!originUser)
		{
			return;
		}
		if (_highlightedEdges.Count > 0)
		{
			List<Vector3d> source = _highlightedEdges.SelectMany(delegate(FaceHalfEdge x)
			{
				Matrix4d transform = GetWorldMatrix(x.Face.Shell);
				return x.Vertices.Select(delegate(Vertex v)
				{
					Vector3d pos = v.Pos;
					return transform.Transform(pos);
				});
			}).ToList();
			Vector3d bbMin = source.Min();
			Vector3d bbMax = source.Max();
			_screen3d.ScreenD3D.ZoomExtend(render: true, bbMin, bbMax, 1.0, fitFront: false, null);
		}
		RaiseRequestRepaint();
	}

	private Matrix4d GetWorldMatrix(Shell shell)
	{
		foreach (Model item in _currentDoc.UnfoldModel3D.GetAllSubModelsWithSelf())
		{
			if (item.Shells.Contains(shell))
			{
				return shell.GetWorldMatrix(item);
			}
		}
		return Matrix4d.Identity;
	}

	private void ValidationResultsChanged(IDoc3d doc3d, List<ValidationResult> validationResults)
	{
		ValiIntrinsicErrors.Clear();
		ValiDistancesErrors.Clear();
		SelfCollisionDetected = Visibility.Collapsed;
		if (validationResults != null)
		{
			int validationErrorCount = validationResults.Count((ValidationResult x) => x.IntrinsicErrors != null || x.DistanceErrors != null || x.Type == ValidationResult.ResultTypes.SelfCollision);
			ValidationErrorCount = validationErrorCount;
			NotifyPropertyChanged("ValidationErrorCount");
			foreach (ValidationResult validationResult in validationResults)
			{
				if (validationResult.IntrinsicErrors != null)
				{
					foreach (ValidationResultIntrinsic intrinsicError in validationResult.IntrinsicErrors)
					{
						ValiIntrinsicErrors.Add(new ValidationErrorIntrinsic
						{
							Type = GetTypeString(validationResult),
							ErrorDesc = intrinsicError.Translate(ValidationResultTranslator),
							Faces = GetFaces(validationResult),
							Edges = GetEdges(validationResult)
						});
					}
				}
				if (validationResult.DistanceErrors != null)
				{
					foreach (ValidationResult distanceError in validationResult.DistanceErrors)
					{
						ValiDistancesErrors.Add(new ValidationErrorDistance
						{
							Type1 = GetTypeString(validationResult),
							Type2 = GetTypeString(distanceError),
							Faces1 = GetFaces(validationResult),
							Faces2 = GetFaces(distanceError),
							Edges1 = GetEdges(validationResult),
							Edges2 = GetEdges(distanceError)
						});
					}
				}
				if (validationResult.Type == ValidationResult.ResultTypes.SelfCollision)
				{
					SelfCollisionDetected = Visibility.Visible;
				}
			}
		}
		RefreshValiResultVisiblity();
		static HashSet<FaceHalfEdge> GetEdges(ValidationResult obj)
		{
			if (obj.Type == ValidationResult.ResultTypes.BendingGroup)
			{
				return new HashSet<FaceHalfEdge>(GetFhe(obj.BendingGroup.Side0.Concat(obj.BendingGroup.Side1).Concat(obj.BendingGroup.ConnectingFaces)));
			}
			if (obj.Type == ValidationResult.ResultTypes.Macro)
			{
				return new HashSet<FaceHalfEdge>(GetFhe(obj.Macro.Faces));
			}
			return new HashSet<FaceHalfEdge> { obj.Fhe };
		}
		static HashSet<Face> GetFaces(ValidationResult obj)
		{
			if (obj.Type == ValidationResult.ResultTypes.BendingGroup)
			{
				return new HashSet<Face>(obj.BendingGroup.Side0.Concat(obj.BendingGroup.Side1).Concat(obj.BendingGroup.ConnectingFaces));
			}
			if (obj.Type == ValidationResult.ResultTypes.Macro)
			{
				return new HashSet<Face>(obj.Macro.Faces);
			}
			return new HashSet<Face>();
		}
		static IEnumerable<FaceHalfEdge> GetFhe(IEnumerable<Face> faces)
		{
			return faces.SelectMany((Face f) => f.BoundaryEdgesCcw.Concat(f.HoleEdgesCw.SelectMany((List<FaceHalfEdge> x) => x)));
		}
		string GetTypeString(ValidationResult obj)
		{
			if (obj.Type == ValidationResult.ResultTypes.BendingGroup)
			{
				return Translate("l_popup.PopupUnfoldSetting.ValiGroupBending") + " " + obj.BendingGroup.ID;
			}
			if (obj.Type == ValidationResult.ResultTypes.Macro)
			{
				return TranslateMacro(obj.Macro.GetType().Name) + " " + obj.Macro.ID;
			}
			return Translate("l_popup.PopupUnfoldSetting.ValiGroupEdges");
		}
	}

	public void SetActiveModelType(ModelViewMode newMode)
	{
		_currentViewMode = newMode;
		RefreshValiResultVisiblity();
	}

	private void RefreshValiResultVisiblity()
	{
		if (_currentDoc?.ValidationResults != null && _currentViewMode == ModelViewMode.UnfoldModel)
		{
			VisibilityValidationResults = Visibility.Visible;
			if (_currentDoc.ValidationResults.Count > 0)
			{
				VisibilityValiWarning = Visibility.Visible;
				VisibilityValiOk = Visibility.Collapsed;
			}
			else
			{
				VisibilityValiOk = Visibility.Visible;
				VisibilityValiWarning = Visibility.Collapsed;
			}
		}
		else
		{
			VisibilityValidationResults = Visibility.Collapsed;
			VisibilityValiOk = Visibility.Collapsed;
			VisibilityValiWarning = Visibility.Collapsed;
		}
		NotifyPropertyChanged("VisibilityValiOk");
		NotifyPropertyChanged("VisibilityValiWarning");
		NotifyPropertyChanged("SelfCollisionDetected");
	}
}
