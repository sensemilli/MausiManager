using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using BendDataBase.Enums;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Dies;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Profiles;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Punches;
using pncommon.WiCAM.Pn4000.Helpers.ObservableCollectionHelper;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.Base.SpatialDataStructures;
using WiCAM.Pn4000.BendModel.GeometryGenerators;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.BendTable;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Interfaces;
using WiCAM.Pn4000.PN3D.BendSimulation;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Pn4000.PN3D.Unfold;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.SubBendTable;

public class BendDataBaseViewModel : ViewModelBase, ICopyTab, IDeleteTab, IAddTab, IAnyTab
{
	private bool _isThicknessValid;

	private bool _isAngleValid;

	private bool _isRadiusValid;

	private bool _isKFValid;

	private bool _isBAValid;

	private bool _isBDValid;

	private bool _isDinValid;

	private bool _isBendLengthMinValid;

	private bool _isBendLengthMaxValid;

	private bool _isVWidthValid;

	private bool _isCornerRadiusValid;

	private bool _isVAngleValid;

	private bool _isPunchRadiusValid;

	private readonly IDoc3d _doc;

	private readonly IMaterialManager _materialManager;

	private readonly IConfigProvider _configProvider;

	private readonly IPnPathService _pnPathService;

	private ObservableCollection<PreferredProfileViewModel> _fittingProfiles;

	private PreferredProfileViewModel _profileToUse;

	private ObservableCollection<string> _punches;

	private ObservableCollection<string> _dies;

	private ObservableCollection<VisualBendZoneDataBaseItem> _items;

	private VisualBendZoneDataBaseItem _selectedItem;

	private ToolConfigModel _toolConfigModel;

	private Visibility _selectPreferredProfileVisible = Visibility.Collapsed;

	private Visibility _selectPreferredProfileNotVisible;

	private Visibility _machineIsSelectedVisibility = Visibility.Collapsed;

	private bool _machineIsSelected;

	private FrameworkElement _imagePunchProfile;

	private FrameworkElement _imageDieProfile;

	private bool _bAEnable;

	private bool _bDEnable;

	private bool _dinEnable;

	private bool _isCopyButtonEnabled;

	private bool _isDeleteButtonEnabled;

	private bool _isAddButtonEnabled;

	private bool _isSelectToolSetButtonEnabled;

	private bool _changeMat = true;

	private ICommand _selectDoubleClick;

	private ICommand _clearToolSetCommand;

	private ICommand _keyDownDelete;

	private bool _singleSelected = true;

	private readonly IDrawToolProfiles _drawToolProfiles;

	private const double ToolImageHeight = 358.0;

	private const double ToolImageWidth = 359.0;

	private DieProfile _primaryDie;

	private PunchProfile _primaryPunch;

	private Material3DGroupViewModel _selectedGroup;

	private ChangedConfigType _changed;

	private FrameworkElement _heightMap;

	private Func<BendParamType> _getFixedValue;

	private Image? _kfImage;

	private Image? _baImage;

	private Image? _bdImage;

	private Image? _dinImage;

	public Action<ChangedConfigType> DataChanged;

	private BendParamType _mainBendTabelValue;

	public bool IsThicknessValid
	{
		get
		{
			return _isThicknessValid;
		}
		set
		{
			_isThicknessValid = value;
			SetEditorEnableRules();
			NotifyPropertyChanged("IsThicknessValid");
		}
	}

	public bool IsAngleValid
	{
		get
		{
			return _isAngleValid;
		}
		set
		{
			_isAngleValid = value;
			SetEditorEnableRules();
			NotifyPropertyChanged("IsAngleValid");
		}
	}

	public bool IsRadiusValid
	{
		get
		{
			return _isRadiusValid;
		}
		set
		{
			_isRadiusValid = value;
			SetEditorEnableRules();
			NotifyPropertyChanged("IsRadiusValid");
		}
	}

	public bool IsKFValid
	{
		get
		{
			return _isKFValid;
		}
		set
		{
			_isKFValid = value;
			SetEditorEnableRules();
			NotifyPropertyChanged("IsKFValid");
		}
	}

	public bool IsBAValid
	{
		get
		{
			return _isBAValid;
		}
		set
		{
			_isBAValid = value;
			SetEditorEnableRules();
			NotifyPropertyChanged("IsBAValid");
		}
	}

	public bool IsBDValid
	{
		get
		{
			return _isBDValid;
		}
		set
		{
			_isBDValid = value;
			SetEditorEnableRules();
			NotifyPropertyChanged("IsBDValid");
		}
	}

	public bool IsDinValid
	{
		get
		{
			return _isDinValid;
		}
		set
		{
			_isDinValid = value;
			SetEditorEnableRules();
			NotifyPropertyChanged("IsDinValid");
		}
	}

	public bool IsBendLengthMinValid
	{
		get
		{
			return _isBendLengthMinValid;
		}
		set
		{
			_isBendLengthMinValid = value;
			SetEditorEnableRules();
			NotifyPropertyChanged("IsBendLengthMinValid");
		}
	}

	public bool IsBendLengthMaxValid
	{
		get
		{
			return _isBendLengthMaxValid;
		}
		set
		{
			_isBendLengthMaxValid = value;
			SetEditorEnableRules();
			NotifyPropertyChanged("IsBendLengthMaxValid");
		}
	}

	public bool IsVWidthValid
	{
		get
		{
			return _isVWidthValid;
		}
		set
		{
			_isVWidthValid = value;
			SetEditorEnableRules();
			NotifyPropertyChanged("IsVWidthValid");
		}
	}

	public bool IsCornerRadiusValid
	{
		get
		{
			return _isCornerRadiusValid;
		}
		set
		{
			_isCornerRadiusValid = value;
			SetEditorEnableRules();
			NotifyPropertyChanged("IsCornerRadiusValid");
		}
	}

	public bool IsVAngleValid
	{
		get
		{
			return _isVAngleValid;
		}
		set
		{
			_isVAngleValid = value;
			SetEditorEnableRules();
			NotifyPropertyChanged("IsVAngleValid");
		}
	}

	public bool IsPunchRadiusValid
	{
		get
		{
			return _isPunchRadiusValid;
		}
		set
		{
			_isPunchRadiusValid = value;
			SetEditorEnableRules();
			NotifyPropertyChanged("IsPunchRadiusValid");
		}
	}

	public ObservableCollection<Material3DGroupViewModel> Material3DGroups { get; set; }

	public Material3DGroupViewModel SelectedGroup
	{
		get
		{
			return _selectedGroup;
		}
		set
		{
			if (_selectedGroup != value)
			{
				_selectedGroup = value;
				MaterialGroupChanged();
				NotifyPropertyChanged("SelectedGroup");
			}
		}
	}

	public ObservableCollection<PreferredProfileViewModel> FittingProfiles
	{
		get
		{
			return _fittingProfiles;
		}
		set
		{
			_fittingProfiles = value;
			NotifyPropertyChanged("FittingProfiles");
		}
	}

	public PreferredProfileViewModel ProfileToUse
	{
		get
		{
			return _profileToUse;
		}
		set
		{
			_profileToUse = value;
			NotifyPropertyChanged("ProfileToUse");
			UsedProfileChanged();
		}
	}

	public bool IsToolEditorMode => SelectPreferredProfileVisible == Visibility.Visible;

	public Visibility SelectPreferredProfileVisible
	{
		get
		{
			return _selectPreferredProfileVisible;
		}
		set
		{
			_selectPreferredProfileVisible = value;
			if (_selectPreferredProfileVisible == Visibility.Visible)
			{
				SelectPreferredProfileNotVisible = Visibility.Collapsed;
			}
			else
			{
				SelectPreferredProfileNotVisible = Visibility.Visible;
			}
			NotifyPropertyChanged("SelectPreferredProfileVisible");
		}
	}

	public Visibility SelectPreferredProfileNotVisible
	{
		get
		{
			return _selectPreferredProfileNotVisible;
		}
		set
		{
			_selectPreferredProfileNotVisible = value;
			NotifyPropertyChanged("SelectPreferredProfileNotVisible");
		}
	}

	public bool MachineIsSelected
	{
		get
		{
			return _machineIsSelected;
		}
		set
		{
			_machineIsSelected = value;
			NotifyPropertyChanged("MachineIsSelected");
		}
	}

	public Visibility MachineIsSelectedVisibility
	{
		get
		{
			return _machineIsSelectedVisibility;
		}
		set
		{
			_machineIsSelectedVisibility = value;
			NotifyPropertyChanged("MachineIsSelectedVisibility");
		}
	}

	public ObservableCollection<PreferredProfileViewModel> PreferredTools => _toolConfigModel.PreferredProfiles;

	public ObservableCollection<string> Punches
	{
		get
		{
			return _punches;
		}
		set
		{
			_punches = value;
			NotifyPropertyChanged("Punches");
		}
	}

	public ObservableCollection<string> Dies
	{
		get
		{
			return _dies;
		}
		set
		{
			_dies = value;
			NotifyPropertyChanged("Dies");
		}
	}

	public bool ItemsLoading => Items == null;

	public ObservableCollection<VisualBendZoneDataBaseItem> Items
	{
		get
		{
			return _items;
		}
		set
		{
			if (_items != value)
			{
				_items = value;
				NotifyPropertyChanged("Items");
				NotifyPropertyChanged("ItemsLoading");
			}
		}
	}

	public VisualBendZoneDataBaseItem SelectedItem
	{
		get
		{
			return _selectedItem;
		}
		set
		{
			_selectedItem = value;
			if (_selectedItem != null)
			{
				_selectedItem.IsSelected = true;
			}
			Parallel.ForEach(Items, delegate(VisualBendZoneDataBaseItem item)
			{
				if (item != _selectedItem)
				{
					item.IsSelected = false;
				}
			});
			NotifyPropertyChanged("SelectedItem");
			if (SingleSelected)
			{
				ItemSelectionChangeNotify();
			}
		}
	}

	public ObservableCollection<object> SelectedItems { get; internal set; }

	public FrameworkElement ImagePunchProfile
	{
		get
		{
			return _imagePunchProfile;
		}
		set
		{
			_imagePunchProfile = value;
			NotifyPropertyChanged("ImagePunchProfile");
		}
	}

	public FrameworkElement ImageDieProfile
	{
		get
		{
			return _imageDieProfile;
		}
		set
		{
			_imageDieProfile = value;
			NotifyPropertyChanged("ImageDieProfile");
		}
	}

	public IBendMachineSimulation Machine { get; set; }

	public Image KfImage => _kfImage ?? (_kfImage = GetImage("BDB_KFactor.png"));

	public Image BaImage => _baImage ?? (_baImage = GetImage("BDB_BA.png"));

	public Image BdImage => _bdImage ?? (_bdImage = GetImage("BDB_BD.png"));

	public Image DinImage => _dinImage ?? (_dinImage = GetImage("BDB_DinLength.png"));

	public ICommand SelectToolSetCommand { get; set; }

	public ICommand ClearToolSetCommand => _clearToolSetCommand = new RelayCommand<object>(delegate
	{
		SelectedItem.VWidth = "*";
		SelectedItem.VAngle = "*";
		SelectedItem.CornerRadius = "*";
		SelectedItem.PunchRadius = "*";
		SelectedItem.PunchAngle = "*";
		ProfileToUse = null;
		ItemSelectionChangeNotify();
		PreferredProfileChanged();
	});

	public ICommand SelectDoubleClick => _selectDoubleClick ?? (_selectDoubleClick = new RelayCommand((Action<object>)delegate
	{
		IsDeleteButtonEnabled = true;
		IsCopyButtonEnabled = true;
		IsAddButtonEnabled = true;
		DieGroup dieGroup = Machine.BendMachine.Dies.DieGroups.FirstOrDefault((DieGroup x) => x.ID == ProfileToUse.DieGroupId);
		PunchGroup punchGroup = Machine.BendMachine.Punches.PunchGroups.FirstOrDefault((PunchGroup x) => x.ID == ProfileToUse.PunchGroupId);
		DieProfile dieProfile = Machine.BendMachine.Dies.DieProfiles.FirstOrDefault((DieProfile p) => p.ID == dieGroup?.PrimaryToolId);
		PunchProfile punchProfile = Machine.BendMachine.Punches.PunchProfiles.FirstOrDefault((PunchProfile p) => p.ID == punchGroup?.PrimaryToolId);
		if (dieProfile != null)
		{
			SelectedItem.VWidth = dieProfile.VWidth.ToString(CultureInfo.InvariantCulture);
			SelectedItem.VAngle = dieProfile.VAngle.ToString(CultureInfo.InvariantCulture);
			SelectedItem.CornerRadius = dieProfile.CornerRadius.ToString(CultureInfo.InvariantCulture);
		}
		else
		{
			SelectedItem.VWidth = "*";
			SelectedItem.VAngle = "*";
			SelectedItem.CornerRadius = "*";
		}
		if (punchProfile != null)
		{
			SelectedItem.PunchRadius = punchProfile.Radius.ToString(CultureInfo.InvariantCulture);
			SelectedItem.PunchAngle = punchProfile.Angle.ToString(CultureInfo.InvariantCulture);
		}
		else
		{
			SelectedItem.PunchRadius = "*";
			SelectedItem.PunchAngle = "*";
		}
		ItemSelectionChangeNotify();
		PreferredProfileChanged();
		SelectPreferredProfileVisible = Visibility.Collapsed;
	}));

	public ICommand KeyDownDelete => _keyDownDelete ?? (_keyDownDelete = new RelayCommand(DeleteButtonClick));

	public bool BAEnable
	{
		get
		{
			return _bAEnable;
		}
		set
		{
			_bAEnable = value;
			NotifyPropertyChanged("BAEnable");
		}
	}

	public bool BDEnable
	{
		get
		{
			return _bDEnable;
		}
		set
		{
			_bDEnable = value;
			NotifyPropertyChanged("BDEnable");
		}
	}

	public bool DinEnable
	{
		get
		{
			return _dinEnable;
		}
		set
		{
			_dinEnable = value;
			NotifyPropertyChanged("DinEnable");
		}
	}

	public bool IsSelectToolSetButtonEnabled
	{
		get
		{
			return _isSelectToolSetButtonEnabled;
		}
		set
		{
			_isSelectToolSetButtonEnabled = value;
			NotifyPropertyChanged("IsSelectToolSetButtonEnabled");
		}
	}

	public bool IsCopyButtonEnabled
	{
		get
		{
			if (_isCopyButtonEnabled)
			{
				return !IsToolEditorMode;
			}
			return false;
		}
		set
		{
			_isCopyButtonEnabled = value;
			NotifyPropertyChanged("IsCopyButtonEnabled");
		}
	}

	public bool IsDeleteButtonEnabled
	{
		get
		{
			if (_isDeleteButtonEnabled)
			{
				return !IsToolEditorMode;
			}
			return false;
		}
		set
		{
			_isDeleteButtonEnabled = value;
			NotifyPropertyChanged("IsDeleteButtonEnabled");
		}
	}

	public bool IsAddButtonEnabled
	{
		get
		{
			if (_isAddButtonEnabled)
			{
				return !IsToolEditorMode;
			}
			return false;
		}
		set
		{
			_isAddButtonEnabled = value;
			NotifyPropertyChanged("IsAddButtonEnabled");
		}
	}

	public bool SingleSelected
	{
		get
		{
			return _singleSelected;
		}
		set
		{
			_singleSelected = value;
			NotifyPropertyChanged("SingleSelected");
		}
	}

	public DieProfile PrimaryDie
	{
		get
		{
			return _primaryDie;
		}
		set
		{
			_primaryDie = value;
			NotifyPropertyChanged("PrimaryDie");
		}
	}

	public PunchProfile PrimaryPunch
	{
		get
		{
			return _primaryPunch;
		}
		set
		{
			_primaryPunch = value;
			NotifyPropertyChanged("PrimaryPunch");
		}
	}

	public FrameworkElement HeightMap
	{
		get
		{
			return _heightMap;
		}
		set
		{
			_heightMap = value;
			NotifyPropertyChanged("HeightMap");
		}
	}

	public BendParamType MainBendTabelValue
	{
		get
		{
			return _mainBendTabelValue;
		}
		set
		{
			if (_mainBendTabelValue != value)
			{
				General3DConfig general3DConfig = _configProvider.InjectOrCreate<General3DConfig>();
				general3DConfig.P3D_BendTableValueShowType = (int)value;
				_configProvider.Push(general3DConfig);
				_configProvider.Save<General3DConfig>();
				_mainBendTabelValue = value;
				NotifyPropertyChanged("MainBendTabelValue");
			}
		}
	}

	public bool IsOkButtonEnabled => true;

	public bool IsSaveButtonEnabled => true;

	public bool IsCancelButtonEnabled => !IsToolEditorMode;

	public BendDataBaseViewModel(IDoc3d doc, IMaterialManager materialManager, IConfigProvider configProvider, IPnPathService pnPathService)
	{
		_doc = doc;
		_materialManager = materialManager;
		_configProvider = configProvider;
		_pnPathService = pnPathService;
		General3DConfig general3DConfig = configProvider.InjectOrCreate<General3DConfig>();
		MainBendTabelValue = (BendParamType)general3DConfig.P3D_BendTableValueShowType;
	}

	public BendDataBaseViewModel Init(ToolConfigModel toolConfigModel, IBendMachineSimulation bendMachineSimulation)
	{
		_toolConfigModel = toolConfigModel;
		_getFixedValue = () => MainBendTabelValue;
		Machine = bendMachineSimulation;
		MachineIsSelected = Machine != null;
		MachineIsSelectedVisibility = ((Machine == null) ? Visibility.Collapsed : Visibility.Visible);
		IsSelectToolSetButtonEnabled = Machine != null && SelectedItem != null;
		SelectToolSetCommand = new RelayCommand<object>(SelectToolSetClick, CanSelectToolSetClick);
		FittingProfiles = new ObservableCollection<PreferredProfileViewModel>();
		ImagePunchProfile = new Canvas
		{
			Height = 358.0,
			Width = 359.0
		};
		ImageDieProfile = new Canvas
		{
			Height = 358.0,
			Width = 359.0
		};
		if (Machine != null)
		{
			CheckForPreferredTools();
		}
		new Thread((ThreadStart)delegate
		{
			FillMaterialCombos();
			Items = GetVisualTable(_doc.GetApplicableBendTable(out var _));
		}).Start();
		HeightMap = new Screen3D();
		(HeightMap as Screen3D).ShowNavigation(show: true);
		(HeightMap as Screen3D).MouseWheelInverted = true;
		HeightMap.Loaded += HeightMapOnLoaded;
		SetDefaultEditorValues();
		return this;
	}

	private ObservableCollection<VisualBendZoneDataBaseItem> GetVisualTable(IBendTable trable)
	{
		throw new NotImplementedException();
	}

	private void HeightMapOnLoaded(object sender, RoutedEventArgs e)
	{
		UpdateTableVisualization();
	}

	private void UpdateTableVisualization()
	{
		if (!double.TryParse(SelectedItem?.Thickness ?? "0", NumberStyles.Any, CultureInfo.InvariantCulture, out var thickness))
		{
			thickness = 0.0;
		}
		int materialId = _materialManager.GetMaterial3DGroupIdByName(SelectedItem?.Material3DGroup);
		double num = 1.0;
		double num2 = 1.0;
		double num3 = 1.0;
		bool isMachineSpecific;
		List<IGrouping<double, IBendTableItem>> list = (from x in _doc.GetApplicableBendTable(out isMachineSpecific).GetEntries()
			where x.Material3DGroupID.Value == materialId
			where Math.Abs(x.Thickness.Value - thickness) < 1E-06
			group x by x.R.Value into g
			orderby g.Key
			select g).ToList();
		Model model = new Model();
		Shell shell2 = (model.Shell = new Shell(model));
		double num4 = 0.0;
		AABBTree<Vector2d, Triple<IBendTableItem, IBendTableItem, AABB<Vector2d>>> aABBTree = new AABBTree<Vector2d, Triple<IBendTableItem, IBendTableItem, AABB<Vector2d>>>();
		List<Triple<IBendTableItem, IBendTableItem, AABB<Vector2d>>> list2 = new List<Triple<IBendTableItem, IBendTableItem, AABB<Vector2d>>>();
		for (int i = 0; i < list.Count; i++)
		{
			double key = list[i].Key;
			double radiusTolerance = Unfold.GetRadiusTolerance(thickness, key, _configProvider.InjectOrCreate<General3DConfig>());
			double num5 = key - radiusTolerance;
			double num6 = key + radiusTolerance;
			num4 = Math.Max(num6, num4);
			List<IBendTableItem> list3 = list[i].OrderBy((IBendTableItem x) => x.Angle.Value).ToList();
			for (int j = 0; j < list3.Count; j++)
			{
				IBendTableItem bendTableItem = list3[j];
				IBendTableItem bendTableItem2 = ((j < list3.Count - 1) ? list3[j + 1] : list3[j]);
				List<Vector2d> list4 = new List<Vector2d>
				{
					new Vector2d(bendTableItem.Angle.Value - 1E-06, num5),
					new Vector2d(bendTableItem2.Angle.Value + 1E-06, num5),
					new Vector2d(bendTableItem2.Angle.Value + 1E-06, num6),
					new Vector2d(bendTableItem.Angle.Value - 1E-06, num6)
				};
				list2.Add(new Triple<IBendTableItem, IBendTableItem, AABB<Vector2d>>(bendTableItem, bendTableItem2, new AABB<Vector2d>(list4)));
			}
			for (int k = 0; k < list3.Count; k++)
			{
				IBendTableItem bendTableItem3 = list3[k];
				IBendTableItem bendTableItem4 = ((k < list3.Count - 1) ? list3[k + 1] : list3[k]);
				_ = new Vector3d[4]
				{
					new Vector3d(bendTableItem3.Angle.Value * num, num5 * num2, -1.0),
					new Vector3d(bendTableItem4.Angle.Value * num, num5 * num2, -1.0),
					new Vector3d(bendTableItem4.Angle.Value * num, num6 * num2, -1.0),
					new Vector3d(bendTableItem3.Angle.Value * num, num6 * num2, -1.0)
				};
			}
		}
		aABBTree.Build(list2, (Triple<IBendTableItem, IBendTableItem, AABB<Vector2d>> triple) => triple.Item3);
		num2 = 180.0 / num4 * 1.0;
		Vertex[] array3;
		for (int l = 0; l < list2.Count; l++)
		{
			Triple<IBendTableItem, IBendTableItem, AABB<Vector2d>> triple2 = list2[l];
			List<Triple<IBendTableItem, IBendTableItem, AABB<Vector2d>>> source = (from x in aABBTree.RectQuery(triple2.Item3)
				where x != triple2
				select x).ToList();
			double r = (triple2.Item3.Min + triple2.Item3.Max).Y * 0.5;
			double radiusTolerance2 = Unfold.GetRadiusTolerance(thickness, r, _configProvider.InjectOrCreate<General3DConfig>());
			double x2 = triple2.Item3.Min.X;
			double x3 = triple2.Item3.Max.X;
			double num7 = BendDataCalculator.BendDeductionFromBendAllowance(thickness, triple2.Item1.Angle.Value, r, BendDataCalculator.BendAllowanceFromKFactor(thickness, triple2.Item1.Angle.Value, r, triple2.Item1.KFactor.Value));
			double num8 = BendDataCalculator.BendDeductionFromBendAllowance(thickness, triple2.Item2.Angle.Value, r, BendDataCalculator.BendAllowanceFromKFactor(thickness, triple2.Item2.Angle.Value, r, triple2.Item2.KFactor.Value));
			List<Triple<IBendTableItem, IBendTableItem, AABB<Vector2d>>> source2 = (from x in source
				where (x.Item3.Min + x.Item3.Max).Y * 0.5 < r
				orderby x.Item3.Min.X
				select x).ToList();
			List<Triple<IBendTableItem, IBendTableItem, AABB<Vector2d>>> source3 = (from x in source
				where (x.Item3.Min + x.Item3.Max).Y * 0.5 > r
				orderby x.Item3.Min.X
				select x).ToList();
			List<Tuple<double, bool, bool, double>> source4 = (from e in new List<Tuple<double, bool, bool, double>>
				{
					new Tuple<double, bool, bool, double>(x2, item2: true, item3: true, r - radiusTolerance2),
					new Tuple<double, bool, bool, double>(x2, item2: false, item3: true, r + radiusTolerance2)
				}.Concat(source2.Select((Triple<IBendTableItem, IBendTableItem, AABB<Vector2d>> e) => new Tuple<double, bool, bool, double>(e.Item1.Angle.Value - 1E-06, item2: true, item3: true, e.Item3.Max.Y))).Concat(source2.Select((Triple<IBendTableItem, IBendTableItem, AABB<Vector2d>> e) => new Tuple<double, bool, bool, double>(e.Item2.Angle.Value + 1E-06, item2: true, item3: false, e.Item3.Max.Y))).Concat(source3.Select((Triple<IBendTableItem, IBendTableItem, AABB<Vector2d>> e) => new Tuple<double, bool, bool, double>(e.Item1.Angle.Value - 1E-06, item2: false, item3: true, e.Item3.Min.Y)))
					.Concat(source3.Select((Triple<IBendTableItem, IBendTableItem, AABB<Vector2d>> e) => new Tuple<double, bool, bool, double>(e.Item2.Angle.Value + 1E-06, item2: false, item3: false, e.Item3.Min.Y)))
				orderby e.Item1
				select e).ToList();
			List<double> list5 = new List<double>();
			List<double> list6 = new List<double>();
			double item = source4.First((Tuple<double, bool, bool, double> e) => e.Item2 && e.Item3).Item4;
			double item2 = source4.First((Tuple<double, bool, bool, double> e) => !e.Item2 && e.Item3).Item4;
			double num9 = item;
			double num10 = item2;
			double num11 = Math.Min(source4.First((Tuple<double, bool, bool, double> e) => e.Item2 && e.Item3).Item1, source4.First((Tuple<double, bool, bool, double> e) => !e.Item2 && e.Item3).Item1);
			foreach (IGrouping<double, Tuple<double, bool, bool, double>> item3 in from x in source4
				group x by x.Item1)
			{
				foreach (Tuple<double, bool, bool, double> item4 in item3)
				{
					if (item4.Item2)
					{
						if (item4.Item3)
						{
							list5.Add(item4.Item4);
						}
						else
						{
							list5.Remove(item4.Item4);
						}
					}
					else if (item4.Item3)
					{
						list6.Add(item4.Item4);
					}
					else
					{
						list6.Remove(item4.Item4);
					}
				}
				double num12 = (list5.Any() ? list5.Max() : item);
				double num13 = (list6.Any() ? list6.Min() : item2);
				if (!(Math.Abs(num12 - num9) > 1E-06) && !(Math.Abs(num13 - num10) > 1E-06))
				{
					continue;
				}
				double num14 = Math.Min(r, (num9 + r - radiusTolerance2) * 0.5);
				double num15 = Math.Max(r, (num10 + r + radiusTolerance2) * 0.5);
				double num16 = MathExt.Clamp(num11, x2, x3);
				double num17 = MathExt.Clamp(item3.Key, x2, x3);
				double num18 = num7 + (num16 - x2) / (x3 - x2) * (num8 - num7);
				double num19 = num7 + (num17 - x2) / (x3 - x2) * (num8 - num7);
				_ = num17 - num16;
				_ = 0.01;
				Vector3d[] array = new Vector3d[4]
				{
					new Vector3d(num16 * num, num14 * num2, num18 * num3),
					new Vector3d(num17 * num, num14 * num2, num19 * num3),
					new Vector3d(num17 * num, num15 * num2, num19 * num3),
					new Vector3d(num16 * num, num15 * num2, num18 * num3)
				};
				Vertex[] array2 = new Vertex[4];
				for (int m = 0; m < array.Length; m++)
				{
					Vector3d pos = array[m];
					if (!shell2.VertexCache.TryGetValue(pos, out var value))
					{
						value = new Vertex(ref pos);
						shell2.VertexCache.Add(pos, value);
					}
					array2[m] = value;
				}
				Face face = new Face(shell2);
				shell2.Faces.Add(face);
				face.Mesh.Add(new Triangle(face, array2[0], array2[1], array2[3]));
				face.Mesh.Add(new Triangle(face, array2[1], array2[2], array2[3]));
				FaceHalfEdge faceHalfEdge = new FaceHalfEdge(face, EdgeType.PolyLine)
				{
					Color = new Color(0f, 0f, 0f, 1f),
					Width = 4f
				};
				faceHalfEdge.AddVertexRange(array2);
				faceHalfEdge.AddVertex(array2.First());
				face.BoundaryEdgesCcw.Add(faceHalfEdge);
				face.Color = new Color(1f, 0f, 0f, 1f);
				Vector3d calculatedTriangleNormal = face.Mesh.First().CalculatedTriangleNormal;
				array3 = array2;
				foreach (Vertex key2 in array3)
				{
					if (!face.SurfaceDerivatives.ContainsKey(key2))
					{
						face.SurfaceDerivatives.Add(key2, new SurfaceDerivatives(calculatedTriangleNormal));
					}
				}
				num11 = item3.Key;
				num9 = num12;
				num10 = num13;
				num4 = Math.Max(num10, num4);
			}
			if (!(num11 < x3))
			{
				continue;
			}
			double num20 = Math.Min(r, (num9 + r - radiusTolerance2) * 0.5);
			double num21 = Math.Max(r, (num10 + r + radiusTolerance2) * 0.5);
			double num22 = MathExt.Clamp(num11, x2, x3);
			double num23 = MathExt.Clamp(x3, x2, x3);
			double num24 = num7 + (num22 - x2) / (x3 - x2) * (num8 - num7);
			double num25 = num7 + (num23 - x2) / (x3 - x2) * (num8 - num7);
			Vector3d[] array4 = new Vector3d[4]
			{
				new Vector3d(num22 * num, num20 * num2, num24 * num3),
				new Vector3d(num23 * num, num20 * num2, num25 * num3),
				new Vector3d(num23 * num, num21 * num2, num25 * num3),
				new Vector3d(num22 * num, num21 * num2, num24 * num3)
			};
			Vertex[] array5 = new Vertex[4];
			for (int num26 = 0; num26 < array4.Length; num26++)
			{
				Vector3d pos2 = array4[num26];
				if (!shell2.VertexCache.TryGetValue(pos2, out var value2))
				{
					value2 = new Vertex(ref pos2);
					shell2.VertexCache.Add(pos2, value2);
				}
				array5[num26] = value2;
			}
			Face face2 = new Face(shell2);
			shell2.Faces.Add(face2);
			face2.Mesh.Add(new Triangle(face2, array5[0], array5[1], array5[3]));
			face2.Mesh.Add(new Triangle(face2, array5[1], array5[2], array5[3]));
			FaceHalfEdge faceHalfEdge2 = new FaceHalfEdge(face2, EdgeType.PolyLine)
			{
				Color = new Color(0f, 0f, 0f, 1f),
				Width = 4f
			};
			faceHalfEdge2.AddVertexRange(array5);
			faceHalfEdge2.AddVertex(array5.First());
			face2.BoundaryEdgesCcw.Add(faceHalfEdge2);
			face2.Color = new Color(1f, 0f, 0f, 1f);
			Vector3d calculatedTriangleNormal2 = face2.Mesh.First().CalculatedTriangleNormal;
			array3 = array5;
			foreach (Vertex key3 in array3)
			{
				if (!face2.SurfaceDerivatives.ContainsKey(key3))
				{
					face2.SurfaceDerivatives.Add(key3, new SurfaceDerivatives(calculatedTriangleNormal2));
				}
			}
		}
		Vector3d[] array6 = new Vector3d[4]
		{
			new Vector3d(0.0, 0.0 * num2, 0.0),
			new Vector3d(180.0 * num, 0.0 * num2, 0.0),
			new Vector3d(180.0 * num, num4 * num2, 0.0),
			new Vector3d(0.0, num4 * num2, 0.0)
		};
		Vertex[] array7 = new Vertex[4];
		for (int num27 = 0; num27 < array6.Length; num27++)
		{
			Vector3d pos3 = array6[num27];
			if (!shell2.VertexCache.TryGetValue(pos3, out var value3))
			{
				value3 = new Vertex(ref pos3);
				shell2.VertexCache.Add(pos3, value3);
			}
			array7[num27] = value3;
		}
		Face face3 = new Face(shell2);
		shell2.Faces.Add(face3);
		face3.Mesh.Add(new Triangle(face3, array7[0], array7[1], array7[3]));
		face3.Mesh.Add(new Triangle(face3, array7[1], array7[2], array7[3]));
		FaceHalfEdge faceHalfEdge3 = new FaceHalfEdge(face3, EdgeType.PolyLine)
		{
			Color = new Color(0f, 0f, 1f, 1f),
			Width = 4f
		};
		faceHalfEdge3.AddVertexRange(array7);
		faceHalfEdge3.AddVertex(array7.First());
		face3.BoundaryEdgesCcw.Add(faceHalfEdge3);
		face3.Color = new Color(0.8f, 0.8f, 1f, 0.5f);
		Vector3d calculatedTriangleNormal3 = face3.Mesh.First().CalculatedTriangleNormal;
		array3 = array7;
		foreach (Vertex key4 in array3)
		{
			if (!face3.SurfaceDerivatives.ContainsKey(key4))
			{
				face3.SurfaceDerivatives.Add(key4, new SurfaceDerivatives(calculatedTriangleNormal3));
			}
		}
		Vector3d[] array8 = new Vector3d[4]
		{
			new Vector3d(0.0, 0.0 * num2, 0.0),
			new Vector3d(0.0, 0.0 * num2, 1.0 * num3),
			new Vector3d(0.0, num4 * num2, 1.0 * num3),
			new Vector3d(0.0, num4 * num2, 0.0)
		};
		Vertex[] array9 = new Vertex[4];
		for (int num28 = 0; num28 < array8.Length; num28++)
		{
			Vector3d pos4 = array8[num28];
			if (!shell2.VertexCache.TryGetValue(pos4, out var value4))
			{
				value4 = new Vertex(ref pos4);
				shell2.VertexCache.Add(pos4, value4);
			}
			array9[num28] = value4;
		}
		Face face4 = new Face(shell2);
		shell2.Faces.Add(face4);
		face4.Mesh.Add(new Triangle(face4, array9[0], array9[1], array9[3]));
		face4.Mesh.Add(new Triangle(face4, array9[1], array9[2], array9[3]));
		FaceHalfEdge faceHalfEdge4 = new FaceHalfEdge(face4, EdgeType.PolyLine)
		{
			Color = new Color(0f, 1f, 0f, 1f),
			Width = 4f
		};
		faceHalfEdge4.AddVertexRange(array9);
		faceHalfEdge4.AddVertex(array9.First());
		face4.BoundaryEdgesCcw.Add(faceHalfEdge4);
		face4.Color = new Color(0.8f, 1f, 0.8f, 0.5f);
		Vector3d calculatedTriangleNormal4 = face4.Mesh.First().CalculatedTriangleNormal;
		array3 = array9;
		foreach (Vertex key5 in array3)
		{
			if (!face4.SurfaceDerivatives.ContainsKey(key5))
			{
				face4.SurfaceDerivatives.Add(key5, new SurfaceDerivatives(calculatedTriangleNormal4));
			}
		}
		shell2.CreateCollisionTree();
		(HeightMap as Screen3D)?.ScreenD3D?.RemoveModel(null);
		(HeightMap as Screen3D)?.ScreenD3D?.RemoveBillboard(null);
		(HeightMap as Screen3D)?.ScreenD3D?.AddModel(model);
		(HeightMap as Screen3D)?.ScreenD3D?.AddModel(new Sphere(default(Vector3d), 2.0, 20, 20, new Color(0f, 0f, 1f, 1f)));
		try
		{
			double num29 = double.Parse(SelectedItem?.Angle ?? "0", CultureInfo.InvariantCulture);
			double num30 = double.Parse(SelectedItem?.Radius ?? "0", CultureInfo.InvariantCulture);
			double num31 = double.Parse(SelectedItem?.BD ?? "0", CultureInfo.InvariantCulture);
			(HeightMap as Screen3D)?.ScreenD3D?.AddModel(new Sphere(new Vector3d(num29 * num, num30 * num2, num31 * num3), 1.0, 20, 20, new Color(1f, 1f, 0f, 1f)));
			(HeightMap as Screen3D)?.ScreenD3D?.AddModel(new Sphere(new Vector3d(num29 * num, num30 * num2, num31 * num3), 5.0, 20, 20, new Color(1f, 1f, 0f, 0.5f)));
		}
		catch
		{
		}
	}

	private void FillMaterialCombos()
	{
		Material3DGroups = new ObservableCollection<Material3DGroupViewModel>
		{
			new Material3DGroupViewModel(null)
			{
				Name = "*",
				Number = -1
			}
		};
		foreach (IMaterialUnf material3DGroup in _materialManager.Material3DGroups)
		{
			Material3DGroups.Add(new Material3DGroupViewModel(material3DGroup));
		}
	}

	private void SetDefaultEditorValues()
	{
		if (_doc.Material != null)
		{
			SelectedGroup = Material3DGroups.FirstOrDefault((Material3DGroupViewModel g) => g.Material3DGroup?.Number == _doc.Material.MaterialGroupForBendDeduction);
		}
		else
		{
			SelectedGroup = Material3DGroups?.FirstOrDefault();
		}
		BAEnable = true;
		BDEnable = true;
		DinEnable = true;
		SetEditorEnableRules();
	}

	public void SetEditorEnableRules()
	{
		ObservableCollection<object> selectedItems = SelectedItems;
		SingleSelected = selectedItems == null || selectedItems.Count <= 1;
		if (SelectedItem == null)
		{
			BAEnable = false;
			BDEnable = false;
			DinEnable = false;
			IsCopyButtonEnabled = false;
			IsAddButtonEnabled = !IsToolEditorMode;
			IsDeleteButtonEnabled = false;
		}
		else
		{
			bool flag = IsDoubleValue(SelectedItem.Thickness);
			bool flag2 = IsDoubleValue(SelectedItem.Angle);
			bool flag3 = IsDoubleValue(SelectedItem.Radius);
			BAEnable = flag && flag2 && flag3;
			BDEnable = BAEnable;
			DinEnable = BAEnable;
			IsCopyButtonEnabled = SelectedItem != null && SingleSelected;
			IsDeleteButtonEnabled = SelectedItem != null;
			IsAddButtonEnabled = !IsToolEditorMode;
			IsSelectToolSetButtonEnabled = Machine != null && SelectedItem != null && SingleSelected;
		}
	}

	private static bool IsDoubleValue(string txt)
	{
		double value;
		return IsDoubleValue(txt, out value);
	}

	private static bool IsDoubleValue(string txt, out double value)
	{
		value = 0.0;
		if (string.IsNullOrEmpty(txt))
		{
			return false;
		}
		txt = txt.Trim().Replace(',', '.');
		if (txt == string.Empty || txt == "*" || txt == "---")
		{
			return false;
		}
		return double.TryParse(txt, NumberStyles.Any, CultureInfo.InvariantCulture, out value);
	}

	private void CheckForPreferredTools()
	{
		LoadTools();
		if (_doc != null)
		{
			LoadFittingProfile();
		}
	}

	public bool CheckForDuplicates(IEnumerable<object> list)
	{
		foreach (VisualBendZoneDataBaseItem item in list)
		{
			if (GetDuplicate(item) != null)
			{
				return true;
			}
		}
		return false;
	}

	public VisualBendZoneDataBaseItem GetDuplicate(VisualBendZoneDataBaseItem o)
	{
		foreach (VisualBendZoneDataBaseItem item in Items)
		{
			if (item.IsEqualInKeyProperties(o))
			{
				return item;
			}
		}
		return null;
	}

	private Image GetImage(string name)
	{
		string pFileImagePath = _pnPathService.GetPFileImagePath(name);
		if (!File.Exists(pFileImagePath))
		{
			return null;
		}
		try
		{
			BitmapImage bitmapImage = new BitmapImage();
			bitmapImage.BeginInit();
			bitmapImage.UriSource = new Uri(pFileImagePath);
			bitmapImage.EndInit();
			return new Image
			{
				Source = bitmapImage
			};
		}
		catch
		{
			return null;
		}
	}

	private void LoadTools()
	{
		Punches = new ObservableCollection<string> { "*" };
		foreach (PunchProfile punchProfile in Machine.BendMachine.Punches.PunchProfiles)
		{
			Punches.Add(punchProfile.Name);
		}
		Dies = new ObservableCollection<string> { "*" };
		foreach (DieProfile dieProfile in Machine.BendMachine.Dies.DieProfiles)
		{
			Dies.Add(dieProfile.Name);
		}
	}

	private void LoadFittingProfile()
	{
		FittingProfiles.Clear();
		if (SelectedItem == null)
		{
			return;
		}
		int matGroupId = -1;
		if (SelectedItem.Material3DGroup != "*")
		{
			matGroupId = _materialManager.Material3DGroup.FirstOrDefault<KeyValuePair<int, string>>((KeyValuePair<int, string> x) => x.Value == SelectedItem.Material3DGroup).Key;
		}
		foreach (PreferredProfileViewModel preferredTool in PreferredTools)
		{
			if (preferredTool.PreferredProfile.IsValidProfile(matGroupId, SelectedItem.ThicknessDouble, 0.0001, SelectedItem.RadiusDouble, SelectedItem.AngleDouble))
			{
				FittingProfiles.Add(preferredTool);
			}
		}
		if (FittingProfiles.Count > 0)
		{
			ProfileToUse = FittingProfiles.First();
		}
	}

	private void PreferredProfileChanged()
	{
		if (ProfileToUse != null)
		{
			DieGroup dieGroup = Machine.BendMachine.Dies.DieGroups.FirstOrDefault((DieGroup x) => x.ID == ProfileToUse.DieGroupId);
			PunchGroup punchGroup = Machine.BendMachine.Punches.PunchGroups.FirstOrDefault((PunchGroup x) => x.ID == ProfileToUse.PunchGroupId);
			if (dieGroup != null)
			{
				SelectedItem.VWidth = dieGroup.VWidth.ToString(CultureInfo.InvariantCulture);
				SelectedItem.VAngle = dieGroup.VAngle.ToString(CultureInfo.InvariantCulture);
				SelectedItem.CornerRadius = dieGroup.Radius.ToString(CultureInfo.InvariantCulture);
			}
			if (punchGroup != null)
			{
				SelectedItem.PunchRadius = punchGroup.Radius.ToString(CultureInfo.InvariantCulture);
			}
			SetEditorEnableRules();
		}
	}

	private void UsedProfileChanged()
	{
		if (ProfileToUse == null)
		{
			((Canvas)ImageDieProfile).Children.Clear();
			((Canvas)ImagePunchProfile).Children.Clear();
			return;
		}
		DieGroup dieGroup = Machine.BendMachine.Dies.DieGroups.FirstOrDefault((DieGroup x) => x.ID == ProfileToUse.DieGroupId);
		PunchGroup punchGroup = Machine.BendMachine.Punches.PunchGroups.FirstOrDefault((PunchGroup x) => x.ID == ProfileToUse.PunchGroupId);
		PrimaryDie = Machine.BendMachine.Dies.DieProfiles.FirstOrDefault((DieProfile p) => p.ID == dieGroup?.PrimaryToolId);
		PrimaryPunch = Machine.BendMachine.Punches.PunchProfiles.FirstOrDefault((PunchProfile p) => p.ID == punchGroup?.PrimaryToolId);
		if (PrimaryDie != null && !string.IsNullOrEmpty(PrimaryDie.GeometryFile) && !PrimaryDie.GeometryFile.EndsWith(".n3d"))
		{
			_drawToolProfiles.LoadDiePreview2D(PrimaryDie, (Canvas)ImageDieProfile, Machine.BendMachine);
			NotifyPropertyChanged("ImageDieProfile");
		}
		if (PrimaryPunch != null && !string.IsNullOrEmpty(PrimaryPunch.GeometryFile) && !PrimaryPunch.GeometryFile.EndsWith(".n3d"))
		{
			_drawToolProfiles.LoadPunchPreview2D(PrimaryPunch, (Canvas)ImagePunchProfile, Machine.BendMachine);
			NotifyPropertyChanged("ImagePunchProfile");
		}
	}

	private void MaterialGroupChanged()
	{
		if (!_changeMat)
		{
			return;
		}
		Material3DGroupViewModel selectedGroup = SelectedGroup;
		if (selectedGroup != null && selectedGroup.Number < 0)
		{
			return;
		}
		if (SelectedItem != null)
		{
			SelectedItem.Material3DGroup = SelectedGroup.Name;
		}
		if (_materialManager.MaterialGroup == null || SelectedGroup == null)
		{
			return;
		}
		KeyValuePair<int, string> group = _materialManager.MaterialGroup.FirstOrDefault<KeyValuePair<int, string>>((KeyValuePair<int, string> g) => g.Key == SelectedGroup.Number);
		if (ProfileToUse != null && group.Value != ProfileToUse.Material3DGroupName)
		{
			ProfileToUse.MaterialGroup3D = new Material3DGroupViewModel(_materialManager.Material3DGroups.FirstOrDefault((IMaterialUnf m) => m.Number == group.Key));
			SelectedGroup = Material3DGroups.FirstOrDefault((Material3DGroupViewModel g) => g.Number == ProfileToUse.Material3DGroupID);
			NotifyPropertyChanged("ProfileToUse");
		}
	}

	private void ItemSelectionChangeNotify()
	{
		if (SelectedItem == null)
		{
			SetDefaultEditorValues();
			return;
		}
		_changeMat = false;
		SelectedGroup = Material3DGroups.FirstOrDefault((Material3DGroupViewModel g) => g.Name == SelectedItem.Material3DGroup);
		_changeMat = true;
		SetEditorEnableRules();
		UpdateTableVisualization();
	}

	private VisualBendZoneDataBaseItem CopyItem()
	{
		_changed = ChangedConfigType.BendTable;
		return new VisualBendZoneDataBaseItem(_getFixedValue)
		{
			IsSelected = true,
			Material3DGroup = SelectedGroup?.Name,
			Thickness = SelectedItem.Thickness,
			Angle = SelectedItem.Angle,
			Radius = SelectedItem.Radius,
			BendLengthMin = SelectedItem.BendLengthMin,
			BendLengthMax = SelectedItem.BendLengthMax,
			MinRadius = SelectedItem.MinRadius,
			MaxRadius = SelectedItem.MaxRadius,
			VWidth = SelectedItem.VWidth,
			VAngle = SelectedItem.VAngle,
			CornerRadius = SelectedItem.CornerRadius,
			PunchRadius = SelectedItem.PunchRadius,
			PunchAngle = SelectedItem.PunchAngle,
			Tag = SelectedItem.Tag,
			KFactor = SelectedItem.KFactor
		};
	}

	public void DeleteButtonClick()
	{
		if (SelectedItems != null && SelectedItems.Any())
		{
			int num = Items.IndexOf((VisualBendZoneDataBaseItem)SelectedItems.Last()) + 1 - SelectedItems.Count;
			Items = Items.Except(SelectedItems.Select((object x) => x as VisualBendZoneDataBaseItem)).ToObservableCollection();
			SetEditorEnableRules();
			if (num >= Items.Count)
			{
				num = Items.Count - 1;
			}
			if (num < 0)
			{
				SelectedItem = null;
			}
			else
			{
				SelectedItem = Items[num];
			}
			_changed = ChangedConfigType.BendTable;
		}
	}

	public void AddButtonClick()
	{
		VisualBendZoneDataBaseItem visualBendZoneDataBaseItem = new VisualBendZoneDataBaseItem(_getFixedValue);
		if (visualBendZoneDataBaseItem != null)
		{
			Items.Add(visualBendZoneDataBaseItem);
			SelectedItem = visualBendZoneDataBaseItem;
			_changed = ChangedConfigType.BendTable;
		}
	}

	public void CopyButtonClick()
	{
		VisualBendZoneDataBaseItem visualBendZoneDataBaseItem = CopyItem();
		if (visualBendZoneDataBaseItem != null)
		{
			Items.Add(visualBendZoneDataBaseItem);
			SelectedItem = Items.LastOrDefault();
			if (SelectedItem != null && IsDoubleValue(SelectedItem.KFactor, out var value))
			{
				SelectedItem.KFactor = (value + 0.1).ToString(CultureInfo.InvariantCulture);
				SelectedItem.KFactor = value.ToString(CultureInfo.InvariantCulture);
				NotifyPropertyChanged("SelectedItem");
			}
		}
	}

	public void CancelButtonClick()
	{
		if (SelectPreferredProfileVisible == Visibility.Visible)
		{
			IsDeleteButtonEnabled = true;
			IsCopyButtonEnabled = true;
			SelectPreferredProfileVisible = Visibility.Collapsed;
			IsAddButtonEnabled = !IsToolEditorMode;
		}
	}

	public void SetSelectedProfile()
	{
		IsDeleteButtonEnabled = true;
		IsCopyButtonEnabled = true;
		IsAddButtonEnabled = true;
		ItemSelectionChangeNotify();
		PreferredProfileChanged();
		SelectPreferredProfileVisible = Visibility.Collapsed;
		IsAddButtonEnabled = !IsToolEditorMode;
	}

	private void SelectToolSetClick(object obj)
	{
		IsDeleteButtonEnabled = false;
		IsCopyButtonEnabled = false;
		LoadFittingProfile();
		SelectPreferredProfileVisible = Visibility.Visible;
		IsAddButtonEnabled = !IsToolEditorMode;
	}

	private bool CanSelectToolSetClick(object obj)
	{
		return Machine != null;
	}

	public void Save()
	{
		throw new NotImplementedException();
	}

	public void Dispose()
	{
		(HeightMap as Screen3D)?.Dispose();
		HeightMap = null;
	}
}
