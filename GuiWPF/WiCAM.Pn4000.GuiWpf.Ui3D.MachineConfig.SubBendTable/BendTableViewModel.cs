using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using Telerik.Windows.Data;
using WiCAM.Pn4000.BendTable;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiWpf.UiBasic;
using WiCAM.Pn4000.MachineAndTools.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.SubBendTable;

internal class BendTableViewModel : ViewModelBase, IBendTableViewModel
{
	private readonly IPnPathService _pnPathService;

	private readonly IBendDataCalculator _bendDataCalculator;

	private readonly IMaterialManager _materialManager;

	private readonly IUnitConverter _unitConverter;

	private readonly ITranslator _translator;

	private readonly IGlobalToolStorage _globalToolStorage;

	private readonly IMessageLogGlobal _messageLogGlobal;

	private BendTableEntry _selectedItem;

	private IBendMachine? _bendMachine;

	private Image? _kfImage;

	private Image? _baImage;

	private Image? _bdImage;

	private Image? _dinImage;

	private BendTableFixedValueTypes _fixedValueType;

	private double _defaultKFactor;

	private string _bendTableName;

	private bool _usePlot2D;

	private string _plotTitle = "";

	private bool _useCameraOrthographic;

	private IBendTable? _bendTable;

	public bool ItemsLoading { get; }

	public RadObservableCollection<BendTableEntry> Items { get; private set; }

	public BendTableEntry SelectedItem
	{
		get
		{
			return _selectedItem;
		}
		set
		{
			if (_selectedItem == value)
			{
				return;
			}
			_selectedItem = value;
			NotifyPropertyChanged("SelectedItem");
			if (_selectedItem == null)
			{
				PlotValues3D = new List<BendTableEntry>();
				PlotTitle = "Wähle einen Eintrag in der Tabelle aus, um für dieses Material und Blechdicke alle Einträge graphisch darzustellen.";
			}
			else
			{
				PlotValues3D = Items.Where((BendTableEntry x) => (x.Material3DGroupID == _selectedItem.Material3DGroupID || x.Material3DGroupID < 0) && (x.Thickness == _selectedItem.Thickness || !x.Thickness.HasValue)).ToList();
				PlotTitle = _translator.Translate("l_popup.PopupBendZoneData.MaterialGroup") + ": " + Materials.FirstOrDefault((ComboboxEntry<int> x) => x.Value == _selectedItem?.Material3DGroupID)?.Desc + " " + _translator.Translate("l_popup.PopupBendZoneData.Thickness") + ": " + (_selectedItem.Thickness?.ToString() ?? "*") + LengthUnit;
			}
			NotifyPropertyChanged("PlotValues3D");
			PlotValues2D = new List<PlotSerie2D<BendTableEntry>>();
			foreach (IGrouping<double?, BendTableEntry> item in from x in PlotValues3D
				group x by x.Angle)
			{
				PlotValues2D.Add(new PlotSerie2D<BendTableEntry>
				{
					Description = (item.Key?.ToString() ?? "-"),
					Data = item.ToList()
				});
			}
			NotifyPropertyChanged("PlotValues2D");
		}
	}

	public List<ComboboxEntry<CornerType>> CornerTypes { get; init; }

	public Image KfImage => _kfImage ?? (_kfImage = GetImage("BDB_KFactor.png"));

	public Image BaImage => _baImage ?? (_baImage = GetImage("BDB_BA.png"));

	public Image BdImage => _bdImage ?? (_bdImage = GetImage("BDB_BD.png"));

	public Image DinImage => _dinImage ?? (_dinImage = GetImage("BDB_DinLength.png"));

	public bool MachineIsSelected => _bendMachine != null;

	public BendTableFixedValueTypes FixedValueType
	{
		get
		{
			return _fixedValueType;
		}
		set
		{
			if (_fixedValueType != value)
			{
				_fixedValueType = value;
				NotifyPropertyChanged("FixedValueType");
				List<BendTableEntry> plotValues3D = PlotValues3D;
				PlotValues3D = null;
				NotifyPropertyChanged("PlotValues3D");
				PlotValues3D = plotValues3D;
				NotifyPropertyChanged("PlotValues3D");
				NotifyPropertyChanged("CurrentFixedValueTypeDesc");
			}
		}
	}

	public double DefaultKFactor
	{
		get
		{
			return _defaultKFactor;
		}
		set
		{
			if (_defaultKFactor != value)
			{
				_defaultKFactor = value;
				NotifyPropertyChanged("DefaultKFactor");
			}
		}
	}

	public string BendTableName
	{
		get
		{
			return _bendTableName;
		}
		set
		{
			if (_bendTableName != value)
			{
				_bendTableName = value;
				NotifyPropertyChanged("BendTableName");
			}
		}
	}

	public bool UsePlot2D
	{
		get
		{
			return _usePlot2D;
		}
		set
		{
			if (_usePlot2D != value)
			{
				_usePlot2D = value;
				NotifyPropertyChanged("UsePlot2D");
				NotifyPropertyChanged("PlotVisibility2D");
				NotifyPropertyChanged("PlotVisibility3D");
			}
		}
	}

	public string PlotTitle
	{
		get
		{
			return _plotTitle;
		}
		set
		{
			if (_plotTitle != value)
			{
				_plotTitle = value;
				NotifyPropertyChanged("PlotTitle");
			}
		}
	}

	public Visibility PlotVisibility2D
	{
		get
		{
			if (!UsePlot2D)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public Visibility PlotVisibility3D
	{
		get
		{
			if (UsePlot2D)
			{
				return Visibility.Collapsed;
			}
			return Visibility.Visible;
		}
	}

	public IBendTable? BendTable
	{
		get
		{
			return _bendTable;
		}
		set
		{
			if (_bendTable != value)
			{
				if (MessageBox.Show(_translator.Translate("l_popup.BendTable.ChangeBendTable"), "", MessageBoxButton.OKCancel, MessageBoxImage.Question, MessageBoxResult.Cancel) == MessageBoxResult.OK)
				{
					UseNewBendTable(value);
				}
				NotifyPropertyChanged("BendTable");
			}
		}
	}

	public bool UseCameraOrthographic
	{
		get
		{
			return _useCameraOrthographic;
		}
		set
		{
			if (_useCameraOrthographic != value)
			{
				_useCameraOrthographic = value;
				if (_useCameraOrthographic)
				{
					Camera3d = new OrthographicCamera();
				}
				else
				{
					Camera3d = new PerspectiveCamera();
				}
				NotifyPropertyChanged("UseCameraOrthographic");
				NotifyPropertyChanged("Camera3d");
			}
		}
	}

	public Camera Camera3d { get; private set; } = new PerspectiveCamera();

	public List<BendTableEntry> PlotValues3D { get; private set; }

	public List<PlotSerie2D<BendTableEntry>> PlotValues2D { get; private set; } = new List<PlotSerie2D<BendTableEntry>>();

	public string LengthUnit => _unitConverter.Length.UnitSuffix;

	public string AngleUnit => _unitConverter.Angle.UnitSuffix;

	public string CurrentFixedValueTypeDesc => FixedValueTypes.First((ComboboxEntry<BendTableFixedValueTypes> x) => x.Value == FixedValueType).Desc;

	public Material PlotMaterial => new DiffuseMaterial(Brushes.Orange);

	public RadObservableCollection<ComboboxEntry<IBendTable>> AllBendTables { get; private set; }

	public RadObservableCollection<ComboboxEntry<int>> Materials { get; private set; }

	public RadObservableCollection<ComboboxEntry<BendTableFixedValueTypes>> FixedValueTypes { get; private set; }

	public BendTableViewModel(IPnPathService pnPathService, IBendDataCalculator bendDataCalculator, IMaterialManager materialManager, IUnitConverter unitConverter, ITranslator translator, IGlobalToolStorage globalToolStorage, IMessageLogGlobal messageLogGlobal)
	{
		_pnPathService = pnPathService;
		_bendDataCalculator = bendDataCalculator;
		_materialManager = materialManager;
		_unitConverter = unitConverter;
		_translator = translator;
		_globalToolStorage = globalToolStorage;
		_messageLogGlobal = messageLogGlobal;
		CornerTypes = translator.GetTranslatedComboboxEntries<CornerType>().ToList();
	}

	public void Init(IBendMachine? bendMachine)
	{
		_bendMachine = bendMachine;
		IBendTable bendTable = bendMachine?.BendTable;
		if (bendTable != null)
		{
			Items = new RadObservableCollection<BendTableEntry>();
			UseNewBendTable(bendTable);
			Items.CollectionChanged += Items_CollectionChanged;
			FixedValueTypes = new RadObservableCollection<ComboboxEntry<BendTableFixedValueTypes>>
			{
				new ComboboxEntry<BendTableFixedValueTypes>(_translator.Translate("l_popup.PopupBendZoneData.KFactorShort"), BendTableFixedValueTypes.KFactor),
				new ComboboxEntry<BendTableFixedValueTypes>(_translator.Translate("l_popup.PopupBendZoneData.BAShort"), BendTableFixedValueTypes.BendAllowance),
				new ComboboxEntry<BendTableFixedValueTypes>(_translator.Translate("l_popup.PopupBendZoneData.BDShort"), BendTableFixedValueTypes.BendDeduction),
				new ComboboxEntry<BendTableFixedValueTypes>(_translator.Translate("l_popup.PopupBendZoneData.DinShort"), BendTableFixedValueTypes.Din)
			};
			Materials = new RadObservableCollection<ComboboxEntry<int>>(_materialManager.Material3DGroup.Select<KeyValuePair<int, string>, ComboboxEntry<int>>((KeyValuePair<int, string> x) => new ComboboxEntry<int>(x.Value, x.Key)));
			Materials.Insert(0, new ComboboxEntry<int>("*", -1));
			AllBendTables = new RadObservableCollection<ComboboxEntry<IBendTable>>(from x in _globalToolStorage.GetAllBendTables()
				select new ComboboxEntry<IBendTable>(x.Name, x));
		}
	}

	private void UseNewBendTable(IBendTable bendTable)
	{
		_bendTable = bendTable;
		DefaultKFactor = bendTable.DefaultKFactor;
		BendTableName = bendTable.Name;
		Items.CollectionChanged -= Items_CollectionChanged;
		Items.Clear();
		Items.AddRange(bendTable.GetEntries().Select(CreateBendTableEntry));
		Items.CollectionChanged += Items_CollectionChanged;
		foreach (BendTableEntry item in Items)
		{
			item.Init(_bendDataCalculator, _unitConverter, this);
		}
	}

	private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		if (e.Action != 0)
		{
			return;
		}
		foreach (BendTableEntry newItem in e.NewItems)
		{
			newItem.Init(_bendDataCalculator, _unitConverter, this);
		}
	}

	private void ShowWarnings(IBendTable bendTable)
	{
		List<IBendTableItem> list = new List<IBendTableItem>();
		foreach (IBendTableItem entry in bendTable.GetEntries())
		{
			if (entry.PunchRadius.HasValue && entry.R.HasValue && entry.R.Value < entry.PunchRadius.Value - 1E-06)
			{
				list.Add(entry);
			}
		}
		if (list.Count > 0)
		{
			_messageLogGlobal.ShowTranslatedWarningMessage("l_popup.PopupMachineConfig.SaveWarningBendTableInvalidRadius", list.Count, list.First().PunchRadius.Value, list.First().R.Value);
		}
	}

	public void Save(IBendMachine? bendMachine)
	{
		IBendTable bendTable = _bendTable;
		if (bendTable != null)
		{
			bendTable.ImportEntries(Items.Select((BendTableEntry x) => x.ExportBendTableItem()), removeOldEntries: true);
			bendTable.DefaultKFactor = DefaultKFactor;
			bendTable.Name = BendTableName;
			ShowWarnings(bendTable);
		}
		if (bendMachine != null)
		{
			bendMachine.BendTable = _bendTable;
		}
	}

	public bool CanSave()
	{
		int num = 0;
		foreach (BendTableEntry item in Items)
		{
			if (!item.KFactor.HasValue || !item.Radius.HasValue || item.Radius <= 0.0)
			{
				num++;
			}
		}
		if (num > 0)
		{
			_messageLogGlobal.ShowTranslatedErrorMessage("l_popup.PopupMachineConfig.SaveErrorBendTableInvalidEntry", num);
			return false;
		}
		return true;
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

	private BendTableFixedValueTypes CurrentFixedValueType()
	{
		return FixedValueType;
	}

	public BendTableEntry CreateBendTableEntry(IBendTableItem entry)
	{
		return new BendTableEntry(_bendDataCalculator, _unitConverter).Init(this, entry);
	}

	public BendTableEntry CreateBendTableEntry()
	{
		return new BendTableEntry(_bendDataCalculator, _unitConverter).Init(this);
	}
}
