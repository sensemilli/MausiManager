using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.SpatialDataStructures;
using WiCAM.Pn4000.BendModel.BendTools.Validations;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Common.Wpf.UnitConversion;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Popup.Information;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.Popup.Enums;

namespace WiCAM.Pn4000.GuiWpf.Ui3D;

public class PopupUnfoldInfoViewModel : PopupViewModelBase
{
	private bool _punchGroupIdColumnVisible;

	private bool _punchGroupNameColumnVisible;

	private bool _dieGroupIdColumnVisible;

	private bool _dieGroupNameColumnVisible;

	private bool _vWidthColumnVisible;

	private bool _vAngleColumnVisible;

	private bool _cornerRadiusColumnVisible;

	private bool _punchRadiusColumnVisible;

	private readonly PopupUnfoldInfoModel _popupUnfoldInfoModel;

	private readonly IMaterialManager _materials;

	private readonly ITranslator _translator;

	private ICommand _modification;

	private VisualBendInfoItems _selectedBend;

	private Canvas _imageUnfoldModel;

	private Dictionary<int, HashSet<Polygon>> _faceDict;

	private Dictionary<int, HashSet<Polygon>> _holeDict;

	private ICombinedBendDescriptorInternal _mouseActiveBend;

	private double _scale;

	private double _dx;

	private double _dy;

	private double _dWidth;

	private double _dHeight;

	private readonly Brush _b1 = Brushes.LightSteelBlue;

	private readonly Brush _b2 = Brushes.SteelBlue;

	private readonly Brush _b3 = Brushes.Blue;

	private readonly Brush _b4 = Brushes.Green;

	private readonly Brush _l1 = Brushes.Black;

	private readonly Brush _l2 = Brushes.DarkBlue;

	private readonly Brush _l3 = Brushes.DarkGreen;

	private ObservableCollection<VisualBendInfoItems> _infoItems;

	private Dictionary<IBendDescriptor, Label> _bendZoneLabelDict;

	private Visibility _loadingGeometryVisibility;

	private bool _isBackgroundLoadingCompleted;

	private Thread _loadThread;

	public Canvas ImageUnfoldModel
	{
		get
		{
			return _imageUnfoldModel;
		}
		set
		{
			_imageUnfoldModel = value;
			OnPropertyChanged("ImageUnfoldModel");
		}
	}

	public VisualBendInfoItems SelectedBend
	{
		get
		{
			return _selectedBend;
		}
		set
		{
			_selectedBend = value;
			HighLightLabels();
			OnPropertyChanged("SelectedBend");
		}
	}

	public string ValidationSummary
	{
		get
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (_popupUnfoldInfoModel.Doc == null)
			{
				stringBuilder.AppendLine("");
			}
			if (_popupUnfoldInfoModel.Doc.ValidationResults == null)
			{
				stringBuilder.AppendLine(_translator.Translate("l_popup.PopupUnfoldInfo.NoValidation"));
			}
			else if (_popupUnfoldInfoModel.Doc.ValidationResults.Count == 0)
			{
				stringBuilder.AppendLine(_translator.Translate("l_popup.PopupUnfoldInfo.ValidationOK"));
			}
			else
			{
				int num = _popupUnfoldInfoModel.Doc.ValidationResults.Count((WiCAM.Pn4000.BendModel.BendTools.Validations.ValidationResult x) => x.IntrinsicErrors != null || x.DistanceErrors != null);
				stringBuilder.AppendLine(_translator.Translate("l_popup.PopupUnfoldInfo.ValidationErrors") + " " + num);
			}
			if (!_popupUnfoldInfoModel.Doc.PnMaterialByUser)
			{
				stringBuilder.AppendLine(_translator.Translate("l_popup.PopupUnfoldInfo.NoMaterialByUser"));
			}
			if (_popupUnfoldInfoModel.Doc.KFactorWarningsError)
			{
				stringBuilder.AppendLine(_translator.Translate("l_popup.PopupUnfoldInfo.KFactorWarning"));
			}
			return stringBuilder.ToString();
		}
	}

	public ICommand ModificationCommand => _modification ?? (_modification = new RelayCommand((Action<object>)delegate
	{
		Modification();
	}));

	public ObservableCollection<VisualBendInfoItems> InfoItems
	{
		get
		{
			return _infoItems;
		}
		set
		{
			_infoItems = value;
			OnPropertyChanged("InfoItems");
		}
	}

	public bool PunchGroupIdColumnVisible
	{
		get
		{
			return _punchGroupIdColumnVisible;
		}
		set
		{
			_punchGroupIdColumnVisible = value;
			OnPropertyChanged("PunchGroupIdColumnVisible");
		}
	}

	public bool PunchGroupNameColumnVisible
	{
		get
		{
			return _punchGroupNameColumnVisible;
		}
		set
		{
			_punchGroupNameColumnVisible = value;
			OnPropertyChanged("PunchGroupNameColumnVisible");
		}
	}

	public bool DieGroupIdColumnVisible
	{
		get
		{
			return _dieGroupIdColumnVisible;
		}
		set
		{
			_dieGroupIdColumnVisible = value;
			OnPropertyChanged("DieGroupIdColumnVisible");
		}
	}

	public bool DieGroupNameColumnVisible
	{
		get
		{
			return _dieGroupNameColumnVisible;
		}
		set
		{
			_dieGroupNameColumnVisible = value;
			OnPropertyChanged("DieGroupNameColumnVisible");
		}
	}

	public bool VWidthColumnVisible
	{
		get
		{
			return _vWidthColumnVisible;
		}
		set
		{
			_vWidthColumnVisible = value;
			OnPropertyChanged("VWidthColumnVisible");
		}
	}

	public bool VAngleColumnVisible
	{
		get
		{
			return _vAngleColumnVisible;
		}
		set
		{
			_vAngleColumnVisible = value;
			OnPropertyChanged("VAngleColumnVisible");
		}
	}

	public bool CornerRadiusColumnVisible
	{
		get
		{
			return _cornerRadiusColumnVisible;
		}
		set
		{
			_cornerRadiusColumnVisible = value;
			OnPropertyChanged("CornerRadiusColumnVisible");
		}
	}

	public bool PunchRadiusColumnVisible
	{
		get
		{
			return _punchRadiusColumnVisible;
		}
		set
		{
			_punchRadiusColumnVisible = value;
			OnPropertyChanged("PunchRadiusColumnVisible");
		}
	}

	public InchConversion Thickness { get; set; }

	public string Material3DGroupName { get; set; }

	public PopupUnfoldInfoModel PopupUnfoldInfoModel => _popupUnfoldInfoModel;

	public Visibility LoadingGeometryVisibility
	{
		get
		{
			return _loadingGeometryVisibility;
		}
		set
		{
			if (_loadingGeometryVisibility != value)
			{
				_loadingGeometryVisibility = value;
				OnPropertyChanged("LoadingGeometryVisibility");
			}
		}
	}

	public bool IsBackgroundLoadingCompleted
	{
		get
		{
			return _isBackgroundLoadingCompleted;
		}
		set
		{
			_isBackgroundLoadingCompleted = value;
			OnPropertyChanged("IsBackgroundLoadingCompleted");
			if (_isBackgroundLoadingCompleted)
			{
				LoadingGeometryVisibility = Visibility.Collapsed;
			}
			else
			{
				LoadingGeometryVisibility = Visibility.Visible;
			}
		}
	}

	public Image KfImage { get; }

	public Image BdImage { get; }

	public Image DinImage { get; }

	public ToolTip KFactorAlgorithmToolTip { get; }

	public ToolTip ToolSelectionAlgorithmToolTip { get; }

	public PopupUnfoldInfoViewModel(PopupUnfoldInfoModel popupUnfoldInfoModel, IMaterialManager materials, ITranslator translator)
	{
		_popupUnfoldInfoModel = popupUnfoldInfoModel;
		_materials = materials;
		_translator = translator;
		base.Button16_OkClick = new RelayCommand<object>(OkButtonClick, CanOkButtonClick);
		KfImage = popupUnfoldInfoModel.KfImage;
		BdImage = popupUnfoldInfoModel.BdImage;
		DinImage = popupUnfoldInfoModel.DinImage;
		KFactorAlgorithmToolTip = popupUnfoldInfoModel.KFactorAlgorithmToolTip;
		ToolSelectionAlgorithmToolTip = popupUnfoldInfoModel.ToolSelectionAlgorithmToolTip;
	}

	public void Init(IDoc3d doc)
	{
		_popupUnfoldInfoModel.Init(doc);
		SetImage();
		LoadData(_materials);
	}

	private void LoadData(IMaterialManager materials)
	{
		if (_popupUnfoldInfoModel.Doc != null)
		{
			Material3DGroupName = materials.GetMaterial3DGroupName(_popupUnfoldInfoModel.Doc.Material.MaterialGroupForBendDeduction);
		}
		Thickness = ((_popupUnfoldInfoModel.Doc != null) ? new InchConversion(_popupUnfoldInfoModel.Doc.Thickness) : new InchConversion(0.0));
		InfoItems = new ObservableCollection<VisualBendInfoItems>();
		foreach (ICombinedBendDescriptorInternal combinedBendDescriptor in _popupUnfoldInfoModel.Doc.CombinedBendDescriptors)
		{
			var (upperTool, lowerTool, value) = _popupUnfoldInfoModel.Doc.PreferredProfileStore.GetBestToolProfiles(combinedBendDescriptor);
			InfoItems.Add(new VisualBendInfoItems(combinedBendDescriptor, upperTool, lowerTool, value));
		}
		if (InfoItems.Count > 0)
		{
			PunchGroupIdColumnVisible = InfoItems.Any((VisualBendInfoItems i) => !string.IsNullOrEmpty(i.PunchGroupId));
			PunchGroupNameColumnVisible = InfoItems.Any((VisualBendInfoItems i) => !string.IsNullOrEmpty(i.PunchGroupName));
			DieGroupIdColumnVisible = InfoItems.Any((VisualBendInfoItems i) => !string.IsNullOrEmpty(i.DieGroupId));
			DieGroupNameColumnVisible = InfoItems.Any((VisualBendInfoItems i) => !string.IsNullOrEmpty(i.DieGroupName));
			VWidthColumnVisible = InfoItems.Any((VisualBendInfoItems i) => Convert.ToDouble(i.VWidth.Converted) > 0.0);
			VAngleColumnVisible = InfoItems.Any((VisualBendInfoItems i) => Math.Abs(i.VAngle) > 0.0);
			CornerRadiusColumnVisible = InfoItems.Any((VisualBendInfoItems i) => Convert.ToDouble(i.CornerRadius.Converted) > 0.0);
			PunchRadiusColumnVisible = InfoItems.Any((VisualBendInfoItems i) => Convert.ToDouble(i.PunchRadius.Converted) > 0.0);
		}
	}

	private void SetImage()
	{
		ImageUnfoldModel = new Canvas
		{
			Width = 746.0,
			Height = 656.0
		};
	}

	public void Draw()
	{
		Freeze();
		GetCurrentGeometries();
	}

	private void GetCurrentGeometries()
	{
		ImageUnfoldModel.Children.Clear();
		_faceDict = new Dictionary<int, HashSet<Polygon>>();
		_holeDict = new Dictionary<int, HashSet<Polygon>>();
		if (_popupUnfoldInfoModel.Doc == null)
		{
			return;
		}
		List<(Face, Model)> list = _popupUnfoldInfoModel.Doc.UnfoldModel3D.GetAllFaceModelsWithFaceGroup().ToList();
		if (list.Count < 1)
		{
			return;
		}
		CalculateCurrentScreenParameters();
		foreach (var item3 in list)
		{
			Face item = item3.Item1;
			Model item2 = item3.Item2;
			Matrix4d worldMatrix = item.Shell.GetWorldMatrix(item2);
			HashSet<Polygon> hashSet = new HashSet<Polygon>();
			HashSet<Polygon> hashSet2 = new HashSet<Polygon>();
			Brush currentBrushForFace = GetCurrentBrushForFace(item, _popupUnfoldInfoModel.Doc.VisibleFaceGroupId);
			Polygon polygon = new Polygon
			{
				StrokeThickness = 1.0,
				Fill = currentBrushForFace,
				Stroke = currentBrushForFace,
				StrokeMiterLimit = 0.0,
				StrokeLineJoin = PenLineJoin.Miter
			};
			foreach (Vertex item4 in item.BoundaryEdgesCcw.SelectMany((FaceHalfEdge e) => e.Vertices))
			{
				Vector3d v = item4.Pos;
				worldMatrix.TransformInPlace(ref v);
				polygon.Points.Add(GetPointProjection(v.X, v.Y));
			}
			ImageUnfoldModel.Children.Add(polygon);
			hashSet.Add(polygon);
			foreach (List<FaceHalfEdge> item5 in item.HoleEdgesCw)
			{
				Polygon polygon2 = new Polygon
				{
					StrokeThickness = 1.0,
					Fill = new SolidColorBrush(Colors.White),
					Stroke = currentBrushForFace,
					StrokeMiterLimit = 0.0,
					StrokeLineJoin = PenLineJoin.Miter
				};
				foreach (Vertex item6 in item5.SelectMany((FaceHalfEdge e) => e.Vertices))
				{
					Vector3d v2 = item6.Pos;
					worldMatrix.TransformInPlace(ref v2);
					polygon2.Points.Add(GetPointProjection(v2.X, v2.Y));
				}
				ImageUnfoldModel.Children.Add(polygon2);
				hashSet2.Add(polygon2);
			}
			_faceDict.Add(item.ID, hashSet);
			_holeDict.Add(item.ID, hashSet2);
			foreach (FaceHalfEdge item7 in item.BoundaryEdgesCcw)
			{
				Polyline polyline = new Polyline
				{
					Stroke = _l1
				};
				foreach (Vertex vertex in item7.Vertices)
				{
					Vector3d v3 = vertex.Pos;
					worldMatrix.TransformInPlace(ref v3);
					polyline.Points.Add(GetPointProjection(v3.X, v3.Y));
				}
				ImageUnfoldModel.Children.Add(polyline);
			}
		}
		FillLabels();
	}

	private Brush GetCurrentBrushForFace(Face face, int visibleFaceGroupId)
	{
		if (face.FaceGroup.IsBendingZone)
		{
			if (!(Math.Round(face.FaceGroup.ConvexAxis.OpeningAngle * 180.0 / Math.PI, 6) > 0.0))
			{
				return _b3;
			}
			return _b4;
		}
		if (face.FaceGroup.ID == visibleFaceGroupId)
		{
			return _b2;
		}
		return _b1;
	}

	private Point GetPointProjection(double x, double y)
	{
		return new Point((int)(x * _scale + _dx), (int)(_dHeight - y * _scale - _dy));
	}

	private void FillLabels()
	{
		_bendZoneLabelDict = new Dictionary<IBendDescriptor, Label>();
		if (_popupUnfoldInfoModel.Doc.CombinedBendDescriptors.Count == 0)
		{
			return;
		}
		foreach (ICombinedBendDescriptorInternal combinedBendDescriptor in _popupUnfoldInfoModel.Doc.CombinedBendDescriptors)
		{
			foreach (IBendDescriptor item in combinedBendDescriptor.Enumerable)
			{
				if (_bendZoneLabelDict.TryGetValue(item, out Label value))
				{
					value.Content = (string)value.Content + $", {combinedBendDescriptor.Order + 1}";
					continue;
				}
				Label label = new Label
				{
					Foreground = new SolidColorBrush(Colors.Black),
					Content = $"{combinedBendDescriptor.Order + 1}",
					Background = new SolidColorBrush(Colors.Wheat),
					Margin = new Thickness(5.0),
					BorderThickness = new Thickness(1.0),
					BorderBrush = new SolidColorBrush(Colors.Black)
				};
				label.MouseEnter += Text_MouseEnter;
				label.MouseLeave += Text_MouseLeave;
				_bendZoneLabelDict.Add(item, label);
				Shell? shell = item.BendParams.UnfoldFaceGroupModel.Shells.FirstOrDefault();
				AABB<Vector3d> boundingBox = shell.AABBTree.Root.BoundingBox;
				Vector3d v = boundingBox.Min;
				Vector3d v2 = boundingBox.Max;
				Matrix4d worldMatrix = shell.GetWorldMatrix(item.BendParams.UnfoldFaceGroupModel);
				worldMatrix.TransformInPlace(ref v);
				worldMatrix.TransformInPlace(ref v2);
				TextCanvasCenter(label, GetPointProjection((v.X + v2.X) / 2.0, (v.Y + v2.Y) / 2.0));
				ImageUnfoldModel?.Children.Add(label);
			}
		}
	}

	private static void TextCanvasCenter(UIElement T, Point p)
	{
		T.Measure(new Size(double.MaxValue, double.MaxValue));
		Canvas.SetLeft(T, p.X - T.DesiredSize.Width / 2.0);
		Canvas.SetTop(T, p.Y - T.DesiredSize.Height / 2.0);
	}

	private void CalculateCurrentScreenParameters()
	{
		_dWidth = ImageUnfoldModel.ActualWidth - 20.0;
		_dHeight = ImageUnfoldModel.ActualHeight - 20.0;
		Pair<Vector3d, Vector3d> boundary = _popupUnfoldInfoModel.Doc.UnfoldModel3D.GetBoundary(Matrix4d.Identity);
		Vector3d item = boundary.Item1;
		Vector3d item2 = boundary.Item2;
		double scale = _dWidth / Math.Abs(item2.X - item.X);
		double num = _dHeight / Math.Abs(item2.Y - item.Y);
		_scale = scale;
		if (num < _scale)
		{
			_scale = num;
		}
		double num2 = item.X * _scale;
		double num3 = item.Y * _scale;
		double num4 = item2.X * _scale;
		double num5 = item2.Y * _scale;
		_dx = _dWidth / 2.0 - (num2 + (num4 - num2) / 2.0) + 10.0;
		_dy = _dHeight / 2.0 - (num3 + (num5 - num3) / 2.0) - 10.0;
	}

	private void Freeze()
	{
		_b1.Freeze();
		_b2.Freeze();
		_b3.Freeze();
		_b4.Freeze();
		_l1.Freeze();
		_l2.Freeze();
		_l3.Freeze();
	}

	private void Text_MouseLeave(object sender, MouseEventArgs e)
	{
		if (SelectedBend?.CommonBendFace?.Order != _mouseActiveBend?.Order)
		{
			UpdateAllColorsAndTexts();
			HighLightLabels();
			_mouseActiveBend = null;
		}
	}

	private void Text_MouseEnter(object sender, MouseEventArgs e)
	{
		_mouseActiveBend = GetBendForLabel(sender as Label);
		if (_mouseActiveBend == null)
		{
			return;
		}
		foreach (IBendDescriptor item in _mouseActiveBend.Enumerable)
		{
			_bendZoneLabelDict[item].Background = _b4;
		}
	}

	private ICombinedBendDescriptorInternal GetBendForLabel(Label label)
	{
		return _popupUnfoldInfoModel.Doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal cbf) => cbf.Enumerable.Any((IBendDescriptor bz) => _bendZoneLabelDict[bz] == label));
	}

	private void HighLightLabels()
	{
		if (_faceDict == null || SelectedBend == null)
		{
			return;
		}
		UpdateAllColorsAndTexts();
		foreach (IBendDescriptor item in SelectedBend.CommonBendFace.Enumerable)
		{
			_bendZoneLabelDict[item].Background = _b4;
		}
	}

	private void UpdateAllColorsAndTexts()
	{
		foreach (Face item in _popupUnfoldInfoModel.Doc.UnfoldModel3D.GetAllFacesWithFaceGroup())
		{
			SetColorForAllPolygonsAtFace(item, GetCurrentBrushForFace(item, _popupUnfoldInfoModel.Doc.VisibleFaceGroupId), new SolidColorBrush(Colors.White));
		}
		FillLabels();
		foreach (ICombinedBendDescriptorInternal combinedBendDescriptor in _popupUnfoldInfoModel.Doc.CombinedBendDescriptors)
		{
			foreach (IBendDescriptor item2 in combinedBendDescriptor.Enumerable)
			{
				_bendZoneLabelDict[item2].Background = new SolidColorBrush(Colors.Wheat);
				_bendZoneLabelDict[item2].Content = $"{combinedBendDescriptor.Order + 1}";
			}
		}
	}

	private void SetColorForAllPolygonsAtFace(Face face, Brush faceBrush, Brush holeBrush)
	{
		foreach (KeyValuePair<int, HashSet<Polygon>> item in _faceDict)
		{
			if (item.Key != face.ID)
			{
				continue;
			}
			foreach (Polygon item2 in item.Value)
			{
				item2.Fill = faceBrush;
				item2.Stroke = faceBrush;
			}
			break;
		}
		foreach (KeyValuePair<int, HashSet<Polygon>> item3 in _holeDict)
		{
			if (item3.Key != face.ID)
			{
				continue;
			}
			{
				foreach (Polygon item4 in item3.Value)
				{
					item4.Fill = holeBrush;
					item4.Stroke = faceBrush;
				}
				break;
			}
		}
	}

	private void Modification()
	{
		GetBendFaceByOrder();
	}

	private void GetBendFaceByOrder()
	{
		if (_mouseActiveBend == null)
		{
			return;
		}
		SelectedBend = InfoItems.FirstOrDefault((VisualBendInfoItems i) => i.CommonBendFace == _popupUnfoldInfoModel.Doc.CombinedBendDescriptors.FirstOrDefault((ICombinedBendDescriptorInternal x) => x.Order == _mouseActiveBend.Order));
	}

	private void OkButtonClick(object obj)
	{
		CloseLikeOk();
	}

	private bool CanOkButtonClick(object obj)
	{
		return true;
	}

	private void CloseLikeOk()
	{
		ImageUnfoldModel = null;
		Thread loadThread = _loadThread;
		if (loadThread != null && loadThread.IsAlive)
		{
			_loadThread.Abort();
		}
		_loadThread = null;
		CloseView();
	}

	public override void ViewCloseAction(EPopupCloseReason reason)
	{
		if (reason == EPopupCloseReason.EnterKey || reason == EPopupCloseReason.MouseButton3 || reason == EPopupCloseReason.System)
		{
			ImageUnfoldModel = null;
			CloseLikeOk();
		}
	}

	public void LoadGeometryInBackground()
	{
		_loadThread = new Thread((ThreadStart)delegate
		{
			Application.Current.Dispatcher.BeginInvoke((Action)delegate
			{
				Draw();
				IsBackgroundLoadingCompleted = true;
			});
		})
		{
			Priority = ThreadPriority.Normal,
			IsBackground = true,
			Name = "GeometryLoaderThread"
		};
		_loadThread.Start();
	}
}
