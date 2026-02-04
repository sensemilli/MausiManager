using System;
using System.Collections.Frozen;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using pncommon.WiCAM.Pn4000.Helpers.ObservableCollectionHelper;
using Telerik.Windows.Data;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolItemViewModels;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.ToolViewModels;
using WiCAM.Pn4000.GuiWpf.UiBasic;
using WiCAM.Pn4000.MachineAndTools.Interfaces;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools;

public class MachineToolsViewModel : ViewModelBase
{
	private readonly IUnitConverter _unitConverter;

	private readonly IConfigProvider _configProvider;

	private readonly IGlobalToolStorage _toolStorage;

	private readonly IToolGeometryVerifier _toolGeometryVerifier;

	private bool changed;

	private bool changedMachine;

	private ToolListViewModel _selectedToolList;

	private List<UpperToolGroupViewModel> _deletedUpperGroups = new List<UpperToolGroupViewModel>();

	private List<LowerToolGroupViewModel> _deletedLowerGroups = new List<LowerToolGroupViewModel>();

	private List<UpperToolViewModel> _deletedUpperTools = new List<UpperToolViewModel>();

	private List<LowerToolViewModel> _deletedLowerTools = new List<LowerToolViewModel>();

	private List<UpperToolPieceViewModel> _deletedUpperPieces = new List<UpperToolPieceViewModel>();

	private List<LowerToolPieceViewModel> _deletedLowerPieces = new List<LowerToolPieceViewModel>();

	private List<LowerToolExtensionPieceViewModel> _deletedLowerToolExtensionPieces = new List<LowerToolExtensionPieceViewModel>();

	private List<MultiToolViewModel> _deletedMultiTools = new List<MultiToolViewModel>();

	private List<LowerAdapterViewModel> _deletedLowerAdapters = new List<LowerAdapterViewModel>();

	private List<UpperAdapterViewModel> _deletedUpperAdapters = new List<UpperAdapterViewModel>();

	private List<LowerToolExtensionViewModel> _deletedLowerToolsExtensions = new List<LowerToolExtensionViewModel>();

	private List<ToolListViewModel> _deletedToolLists = new List<ToolListViewModel>();

	private Dictionary<int, MultiToolViewModel> _multiToolsWithIDs;

	private int _lastFakeId = -1;

	public Action<ChangedConfigType> ChangedAction;

	private int _upperBeamMountTypeId;

	private int _lowerBeamMountTypeId;

	public IUnitConverter UnitConverter => _unitConverter;

	public IConfigProvider ConfigProvider => _configProvider;

	public IGlobalToolStorage ToolStorage => _toolStorage;

	public IMessageLogGlobal MessageLogGlobal { get; }

	public IToolGeometryVerifier ToolGeometryVerifier => _toolGeometryVerifier;

	public ToolListViewModel SelectedToolList
	{
		get
		{
			return _selectedToolList;
		}
		set
		{
			if (_selectedToolList != value)
			{
				_selectedToolList = value;
				ToolListSelectionChanged();
				NotifyPropertyChanged("SelectedToolList");
			}
		}
	}

	public List<MultiToolViewModel> AllMultiTools { get; private set; }

	public ObservableCollection<UpperToolGroupViewModel> UpperGroups { get; private set; }

	public ObservableCollection<LowerToolGroupViewModel> LowerGroups { get; private set; }

	public ObservableCollection<UpperAdapterViewModel> UpperAdapters { get; private set; }

	public ObservableCollection<LowerAdapterViewModel> LowerAdapters { get; private set; }

	public ObservableCollection<UpperToolViewModel> UpperTools { get; private set; }

	public ObservableCollection<LowerToolViewModel> LowerTools { get; private set; }

	public ObservableCollection<LowerToolExtensionViewModel> LowerToolsExtensions { get; private set; }

	public ObservableCollection<UpperToolPieceViewModel> UpperPieces { get; private set; }

	public ObservableCollection<LowerToolPieceViewModel> LowerPieces { get; private set; }

	public ObservableCollection<LowerToolExtensionPieceViewModel> LowerToolExtentionPieces { get; private set; }

	public ObservableCollection<ToolListViewModel> ToolLists { get; private set; }

	public ObservableCollection<ToolListViewModel> AllToolLists { get; private set; }

	public FrozenDictionary<int, ToolViewModel> StartToolsById { get; private set; }

	public FrozenDictionary<int, UpperToolGroupViewModel> StartUpperGroupsById { get; private set; }

	public FrozenDictionary<int, LowerToolGroupViewModel> StartLowerGroupsById { get; private set; }

	internal ObservableCollection<AliasMultiToolViewModel> AllAliasMultiTools { get; set; }

	internal ObservableCollection<AliasPieceViewModel> AllAliasPieces { get; set; }

	public int UpperBeamMountTypeId
	{
		get
		{
			return _upperBeamMountTypeId;
		}
		set
		{
			if (_upperBeamMountTypeId != value)
			{
				_upperBeamMountTypeId = value;
				NotifyPropertyChanged("UpperBeamMountTypeId");
			}
		}
	}

	public int LowerBeamMountTypeId
	{
		get
		{
			return _lowerBeamMountTypeId;
		}
		set
		{
			if (_lowerBeamMountTypeId != value)
			{
				_lowerBeamMountTypeId = value;
				NotifyPropertyChanged("LowerBeamMountTypeId");
			}
		}
	}

	public List<ComboboxEntry<AdapterDirections>> AllAdapterDirections { get; }

	public List<ComboboxEntry<AdapterXPositions>> AllAdapterXPositions { get; }

	public List<ComboboxEntry<AllowedFlippedStates>> AllAllowedFlippingStates { get; }

	public RadObservableCollection<MappingViewModel.MappingVm> MultiToolMappings { get; set; } = new RadObservableCollection<MappingViewModel.MappingVm>();

	public RadObservableCollection<MappingViewModel.MappingVm> ToolMappings { get; set; } = new RadObservableCollection<MappingViewModel.MappingVm>();

	public RadObservableCollection<MappingViewModel.MappingVm> MountIdMappings { get; set; } = new RadObservableCollection<MappingViewModel.MappingVm>();

	public List<MappingViewModel.ItemVm> MultiToolItemVms { get; } = new List<MappingViewModel.ItemVm>();

	public List<MappingViewModel.ItemVm> ToolItemVms { get; } = new List<MappingViewModel.ItemVm>();

	public List<MappingViewModel.ItemVm> MountIdItemVms { get; } = new List<MappingViewModel.ItemVm>();

	public string? LastEditGeometryPath { get; set; }

	public MachineToolsViewModel(IUnitConverter unitConverter, IConfigProvider configProvider, IGlobalToolStorage toolStorage, IMessageLogGlobal messageLogGlobal, ITranslator translator, IToolGeometryVerifier toolGeometryVerifier)
	{
		_unitConverter = unitConverter;
		_configProvider = configProvider;
		_toolStorage = toolStorage;
		_toolGeometryVerifier = toolGeometryVerifier;
		MessageLogGlobal = messageLogGlobal;
		AllMultiTools = new List<MultiToolViewModel>();
		_multiToolsWithIDs = new Dictionary<int, MultiToolViewModel>();
		AllAdapterDirections = translator.GetTranslatedComboboxEntries<AdapterDirections>().ToList();
		AllAdapterXPositions = translator.GetTranslatedComboboxEntries<AdapterXPositions>().ToList();
		AllAllowedFlippingStates = translator.GetTranslatedComboboxEntries<AllowedFlippedStates>().ToList();
	}

	public MachineToolsViewModel Init(IBendMachine bendMachine)
	{
		UpperBeamMountTypeId = bendMachine.UpperToolSystem.IDClampSystemInToolDatabase;
		LowerBeamMountTypeId = bendMachine.LowerToolSystem.IDClampSystemInToolDatabase;
		Dictionary<int, AliasMultiToolViewModel> aliasMultiTools = (from x in _toolStorage.GetAllAliasMultiToolProfiles()
			select new AliasMultiToolViewModel(x, this)).ToDictionary((AliasMultiToolViewModel x) => x.Id);
		Dictionary<int, MultiToolViewModel> multiTools = (from x in _toolStorage.GetAllMultiToolProfiles()
			select GetMultiTool(x, aliasMultiTools)).ToDictionary((MultiToolViewModel x) => x.ID.Value);
		Dictionary<int, AliasPieceViewModel> aliasPieces = (from x in _toolStorage.GetAllAliasPieceProfiles()
			select new AliasPieceViewModel(x, this)).ToDictionary((AliasPieceViewModel x) => x.Id);
		Dictionary<int, UpperToolGroupViewModel> upperGroups = (from x in _toolStorage.GetAllPunchGroups()
			select new UpperToolGroupViewModel(x, _unitConverter, _toolStorage)).ToDictionary((UpperToolGroupViewModel x) => x.ID.Value);
		Dictionary<int, LowerToolGroupViewModel> lowerGroups = (from x in _toolStorage.GetAllDieGroups()
			select new LowerToolGroupViewModel(x, _unitConverter, _toolStorage)).ToDictionary((LowerToolGroupViewModel x) => x.ID.Value);
		Dictionary<int, UpperToolViewModel> dictionary = (from x in _toolStorage.GetAllPunchProfiles()
			select new UpperToolViewModel(x, upperGroups[x.GroupID], _unitConverter, _toolStorage, multiTools[x.MultiToolProfile.ID])).ToDictionary((UpperToolViewModel x) => x.ID.Value);
		Dictionary<int, LowerToolViewModel> dictionary2 = (from x in _toolStorage.GetAllDieProfiles()
			select new LowerToolViewModel(x, lowerGroups[x.GroupID], _unitConverter, _toolStorage, multiTools[x.MultiToolProfile.ID])).ToDictionary((LowerToolViewModel x) => x.ID.Value);
		foreach (KeyValuePair<int, LowerToolViewModel> item in dictionary2)
		{
			item.Value.MachineToolsViewModel = this;
			item.Value.ValidateGeometry();
		}
		Dictionary<int, UpperAdapterViewModel> dictionary3 = (from x in _toolStorage.GetAllUpperAdapterProfiles()
			select new UpperAdapterViewModel(x, _unitConverter, _toolStorage, multiTools[x.MultiToolProfile.ID])).ToDictionary((UpperAdapterViewModel x) => x.ID.Value);
		Dictionary<int, LowerAdapterViewModel> dictionary4 = (from x in _toolStorage.GetAllLowerAdapterProfiles()
			select new LowerAdapterViewModel(x, _unitConverter, _toolStorage, multiTools[x.MultiToolProfile.ID])).ToDictionary((LowerAdapterViewModel x) => x.ID.Value);
		Dictionary<int, LowerToolExtensionViewModel> dictionary5 = (from x in _toolStorage.GetAllDieExtentionProfiles()
			select new LowerToolExtensionViewModel(x, _unitConverter, _toolStorage, multiTools[x.MultiToolProfile.ID], lowerGroups.GetValueOrDefault(x.GroupID))).ToDictionary((LowerToolExtensionViewModel x) => x.ID.Value);
		MultiToolItemVms.AddRange(bendMachine.ToolConfig.MultiToolProfiles.Select((IMultiToolProfile x) => new MappingViewModel.ItemVm
		{
			Id = x.ID,
			Desc = x.Name
		}));
		ToolItemVms.AddRange(from x in bendMachine.ToolConfig.UpperTools.OfType<IToolProfile>().Concat(bendMachine.ToolConfig.LowerTools.OfType<IToolProfile>())
			select new MappingViewModel.ItemVm
			{
				Id = x.ID,
				Desc = x.Name
			});
		MountIdItemVms.AddRange(from x in (from x in bendMachine.ToolConfig.UpperAdapterProfiles.Concat(bendMachine.ToolConfig.LowerAdapterProfiles).OfType<IAdapterProfile>()
				select x.Socket.Id into x
				where x.HasValue
				select x.Value).Concat(from x in bendMachine.ToolConfig.MultiToolProfiles
				where x.Plug.Id.HasValue
				select x.Plug.Id.Value).Append(bendMachine.LowerToolSystem.IDClampSystemInToolDatabase).Append(bendMachine.UpperToolSystem.IDClampSystemInToolDatabase)
				.Distinct()
			select new MappingViewModel.ItemVm
			{
				Id = x,
				Desc = bendMachine.PpMappings.ToolMapping.MountTypeIdMappings.FirstOrDefault<KeyValuePair<int, (string, string)>>((KeyValuePair<int, (string ppName, string desc)> y) => y.Key == x).Value.Item2
			});
		IToolMappings toolMapping = bendMachine.PpMappings.ToolMapping;
		Dictionary<int, MappingViewModel.ItemVm> multiToolItemVmsDict = MultiToolItemVms.ToDictionary((MappingViewModel.ItemVm x) => x.Id);
		MultiToolMappings.AddRange(from x in toolMapping.ToolIdMappings
			where multiToolItemVmsDict.ContainsKey(x.Key)
			select new MappingViewModel.MappingVm
			{
				Profile = multiToolItemVmsDict[x.Key],
				PpName = x.Value
			});
		Dictionary<int, MappingViewModel.ItemVm> toolItemVmsDict = ToolItemVms.ToDictionary((MappingViewModel.ItemVm x) => x.Id);
		ToolMappings.AddRange(from x in toolMapping.ToolIdMappings
			where toolItemVmsDict.ContainsKey(x.Key)
			select new MappingViewModel.MappingVm
			{
				Profile = toolItemVmsDict[x.Key],
				PpName = x.Value
			});
		Dictionary<int, MappingViewModel.ItemVm> mountIdItemVmsDict = MountIdItemVms.ToDictionary((MappingViewModel.ItemVm x) => x.Id);
		MountIdMappings.AddRange(from x in toolMapping.MountTypeIdMappings
			where mountIdItemVmsDict.ContainsKey(x.Key)
			select new MappingViewModel.MappingVm
			{
				Profile = mountIdItemVmsDict[x.Key],
				PpName = x.Value.ppName
			});
		AllAliasMultiTools = aliasMultiTools.Values.ToObservableCollection();
		AllAliasPieces = aliasPieces.Values.ToObservableCollection();
		UpperGroups = upperGroups.Values.ToObservableCollection();
		LowerGroups = lowerGroups.Values.ToObservableCollection();
		UpperTools = dictionary.Values.ToObservableCollection();
		LowerTools = dictionary2.Values.ToObservableCollection();
		UpperAdapters = dictionary3.Values.ToObservableCollection();
		LowerAdapters = dictionary4.Values.ToObservableCollection();
		LowerToolsExtensions = dictionary5.Values.ToObservableCollection();
		StartUpperGroupsById = UpperGroups.ToFrozenDictionary((UpperToolGroupViewModel x) => x.ID.Value);
		StartLowerGroupsById = LowerGroups.ToFrozenDictionary((LowerToolGroupViewModel x) => x.ID.Value);
		StartToolsById = ((IEnumerable<ToolViewModel>)UpperTools).Concat((IEnumerable<ToolViewModel>)LowerTools).Concat(LowerToolsExtensions).ToFrozenDictionary((ToolViewModel x) => x.ID.Value);
		UpperPieces = new ObservableCollection<UpperToolPieceViewModel>();
		LowerPieces = new ObservableCollection<LowerToolPieceViewModel>();
		LowerToolExtentionPieces = new ObservableCollection<LowerToolExtensionPieceViewModel>();
		foreach (IToolPieceProfile allPieceProfile in _toolStorage.GetAllPieceProfiles())
		{
			int totalAmount = 0;
			if (allPieceProfile is IUpperToolPieceProfile upperToolPieceProfile)
			{
				UpperPieces.Add(new UpperToolPieceViewModel(SelectedToolList, multiTools[upperToolPieceProfile.MultiToolProfile.ID], allPieceProfile.Aliases.Select((IAliasPieceProfile x) => aliasPieces[x.Id]), totalAmount, upperToolPieceProfile, this));
			}
			else if (allPieceProfile is ILowerToolExtensionPieceProfile)
			{
				LowerToolExtentionPieces.Add(new LowerToolExtensionPieceViewModel(SelectedToolList, multiTools[allPieceProfile.MultiToolProfile.ID], allPieceProfile.Aliases.Select((IAliasPieceProfile x) => aliasPieces[x.Id]), totalAmount, allPieceProfile, this));
			}
			else
			{
				LowerPieces.Add(new LowerToolPieceViewModel(SelectedToolList, multiTools[allPieceProfile.MultiToolProfile.ID], allPieceProfile.Aliases.Select((IAliasPieceProfile x) => aliasPieces[x.Id]), totalAmount, allPieceProfile, this));
			}
		}
		Dictionary<int, List<ToolPieceViewModel>> piecesToVm = (from x in ((IEnumerable<ToolPieceViewModel>)UpperPieces).Concat((IEnumerable<ToolPieceViewModel>)LowerPieces).Concat(LowerToolExtentionPieces)
			group x by x.AliasId.Value).ToDictionary((IGrouping<int, ToolPieceViewModel> g) => g.Key, (IGrouping<int, ToolPieceViewModel> g) => g.ToList());
		AllToolLists = (from x in _toolStorage.GetAllToolLists()
			select new ToolListViewModel(x, _toolStorage, piecesToVm)).ToObservableCollection();
		ObservableCollection<ToolListViewModel> observableCollection = new ObservableCollection<ToolListViewModel>();
		foreach (IToolListAvailable list in bendMachine.ToolLists)
		{
			ToolListViewModel toolListViewModel = AllToolLists.First((ToolListViewModel x) => x.ID == list.Id);
			toolListViewModel.UsedByMachine = true;
			observableCollection.Add(toolListViewModel);
		}
		ToolLists = observableCollection;
		foreach (ToolListViewModel allToolList in AllToolLists)
		{
			allToolList.PropertyChanged += ToolList_PropertyChanged;
		}
		SelectedToolList = ToolLists.FirstOrDefault();
		UpperGroups.CollectionChanged += delegate
		{
			changed = true;
		};
		LowerGroups.CollectionChanged += delegate
		{
			changed = true;
		};
		UpperTools.CollectionChanged += delegate
		{
			changed = true;
		};
		LowerTools.CollectionChanged += delegate
		{
			changed = true;
		};
		UpperPieces.CollectionChanged += delegate
		{
			changed = true;
		};
		LowerPieces.CollectionChanged += delegate
		{
			changed = true;
		};
		LowerToolExtentionPieces.CollectionChanged += delegate
		{
			changed = true;
		};
		UpperAdapters.CollectionChanged += delegate
		{
			changed = true;
		};
		LowerAdapters.CollectionChanged += delegate
		{
			changed = true;
		};
		changed = false;
		changedMachine = false;
		return this;
	}

	public void ToolList_PropertyChanged(object? sender, PropertyChangedEventArgs e)
	{
		if (!(sender is ToolListViewModel toolListViewModel) || !(e.PropertyName == "UsedByMachine"))
		{
			return;
		}
		if (toolListViewModel.UsedByMachine)
		{
			if (!ToolLists.Contains(toolListViewModel))
			{
				ToolLists.Add(toolListViewModel);
			}
		}
		else if (ToolLists.Contains(toolListViewModel))
		{
			ToolLists.Remove(toolListViewModel);
		}
	}

	public bool CanSave()
	{
		if (CanSaveAliasPieces())
		{
			return CanSaveToolProfiles();
		}
		return false;
	}

	public void Save(IBendMachineTools bendMachineTools, IBendMachine bendMachine)
	{
		if (UpperBeamMountTypeId != bendMachine.UpperToolSystem.IDClampSystemInToolDatabase)
		{
			bendMachine.UpperToolSystem.IDClampSystemInToolDatabase = UpperBeamMountTypeId;
			changedMachine = true;
		}
		if (LowerBeamMountTypeId != bendMachine.LowerToolSystem.IDClampSystemInToolDatabase)
		{
			bendMachine.LowerToolSystem.IDClampSystemInToolDatabase = LowerBeamMountTypeId;
			changedMachine = true;
		}
		SaveAliasMultiTools();
		SaveMultiTools();
		Dictionary<UpperToolGroupViewModel, IPunchGroup> groupDict = SaveUpperGroups();
		Dictionary<LowerToolGroupViewModel, IDieGroup> groupDict2 = SaveLowerGroups();
		SaveUpperTools(groupDict);
		SaveLowerTools(groupDict2);
		SaveLowerToolExtensions();
		SaveAliasPieces();
		SaveUpperPieces();
		SaveLowerPieces();
		SaveLowerAdapters();
		SaveUpperAdapters();
		SaveLowerToolExtensionPieces();
		SaveToolLists(bendMachine);
		SavaeMappings(bendMachine);
		ChangedConfigType changedConfigType = ChangedConfigType.NoChanges;
		if (changed)
		{
			changedConfigType |= ChangedConfigType.Tools | ChangedConfigType.ToolGroups;
		}
		if (changedMachine)
		{
			changedConfigType |= ChangedConfigType.MachineConfig;
		}
		ChangedAction(changedConfigType);
	}

	internal MultiToolViewModel GetMultiTool(IMultiToolProfile multiTool, Dictionary<int, AliasMultiToolViewModel> dict)
	{
		if (!_multiToolsWithIDs.TryGetValue(multiTool.ID, out MultiToolViewModel value))
		{
			value = new MultiToolViewModel(multiTool, _unitConverter, _toolStorage, dict[multiTool.Aliases.ID], this);
			_multiToolsWithIDs.Add(multiTool.ID, value);
			AllMultiTools.Add(value);
		}
		return value;
	}

	internal LowerToolGroupViewModel CreateLowerToolGroup()
	{
		LowerToolGroupViewModel lowerToolGroupViewModel = new LowerToolGroupViewModel(_unitConverter, _toolStorage)
		{
			Name = "HemGroup"
		};
		LowerGroups.Add(lowerToolGroupViewModel);
		return lowerToolGroupViewModel;
	}

	internal LowerToolGroupViewModel CreateLowerToolGroup(double vWidthMm, double vAngleRad, double? radiusMm, string name)
	{
		LowerToolGroupViewModel lowerToolGroupViewModel = CreateLowerToolGroup();
		lowerToolGroupViewModel.VWidthMm = vWidthMm;
		lowerToolGroupViewModel.VAngleRad = vAngleRad;
		if (radiusMm.HasValue)
		{
			lowerToolGroupViewModel.RadiusMm = radiusMm.Value;
		}
		lowerToolGroupViewModel.Name = name;
		return lowerToolGroupViewModel;
	}

	internal UpperToolGroupViewModel CreateUpperToolGroup(string name = "HemGroup")
	{
		UpperToolGroupViewModel upperToolGroupViewModel = new UpperToolGroupViewModel(_unitConverter, _toolStorage)
		{
			Name = name
		};
		UpperGroups.Add(upperToolGroupViewModel);
		return upperToolGroupViewModel;
	}

	internal UpperToolGroupViewModel CreateUpperToolGroup(string name, double punchRadiusMm)
	{
		UpperToolGroupViewModel upperToolGroupViewModel = CreateUpperToolGroup(name);
		upperToolGroupViewModel.RadiusMm = punchRadiusMm;
		return upperToolGroupViewModel;
	}

	internal AliasMultiToolViewModel CreateAliasMultiTool()
	{
		AliasMultiToolViewModel aliasMultiToolViewModel = new AliasMultiToolViewModel(this);
		AllAliasMultiTools.Add(aliasMultiToolViewModel);
		return aliasMultiToolViewModel;
	}

	internal AliasPieceViewModel CreateAliasPiece()
	{
		AliasPieceViewModel aliasPieceViewModel = new AliasPieceViewModel(this);
		AllAliasPieces.Add(aliasPieceViewModel);
		return aliasPieceViewModel;
	}

	internal MultiToolViewModel CreateMultiTool(string fileName, AliasMultiToolViewModel? alias = null)
	{
		if (alias == null)
		{
			alias = CreateAliasMultiTool();
		}
		MultiToolViewModel multiToolViewModel = new MultiToolViewModel(_toolStorage, _unitConverter, fileName, this, alias);
		AllMultiTools.Add(multiToolViewModel);
		return multiToolViewModel;
	}

	internal UpperToolViewModel CreateUpperToolProfile(MultiToolViewModel multiTool, UpperToolGroupViewModel upperGroup, double? radius, double? angleRad, double workingHeight, string name, double maxAllowableToolForcePerLengthUnit, double? hemOffsetX = 0.0, double? widthHemmingFace = 0.0)
	{
		UpperToolViewModel upperToolViewModel = new UpperToolViewModel(_unitConverter, _toolStorage, multiTool, upperGroup, angleRad, radius, hemOffsetX, widthHemmingFace, workingHeight)
		{
			Name = name,
			MaxAllowableToolForcePerLengthUnit = maxAllowableToolForcePerLengthUnit,
			Implemented = true
		};
		UpperTools.Add(upperToolViewModel);
		return upperToolViewModel;
	}

	internal LowerToolViewModel CreateLowerToolProfile(MultiToolViewModel multiTool, LowerToolGroupViewModel lowerGroup, VWidthTypes vWidthType, string name, double maxAllowableToolForcePerLengthUnit, double offsetInX = 0.0, double workingHeight = 0.0, double? vWidth = null, double? vAngle = null, double? vDepth = null, double? cornerRadius = null, double? foldGap = null, double? widthHemmingFace = null, double? springHeight = null, double? partFoldOffset = null)
	{
		LowerToolViewModel lowerToolViewModel = new LowerToolViewModel(vWidthType, _unitConverter, _toolStorage, multiTool, lowerGroup, offsetInX, workingHeight, vWidth, vAngle, vDepth, cornerRadius, foldGap, widthHemmingFace, springHeight, partFoldOffset)
		{
			Name = name,
			MaxAllowableToolForcePerLengthUnit = maxAllowableToolForcePerLengthUnit,
			Implemented = true
		};
		LowerTools.Add(lowerToolViewModel);
		return lowerToolViewModel;
	}

	internal UpperToolPieceViewModel CreateUpperToolPiece(ToolListViewModel currentToolList, MultiToolViewModel multiTool, string fileName, double length = 0.0, int amount = 0)
	{
		return new UpperToolPieceViewModel(this, currentToolList, multiTool, fileName, length, amount);
	}

	internal LowerToolPieceViewModel CreateLowerToolPiece(ToolListViewModel currentToolList, MultiToolViewModel multiTool, string fileName, double length = 0.0, int amount = 0)
	{
		return new LowerToolPieceViewModel(this, currentToolList, multiTool, fileName, length, amount);
	}

	internal LowerToolExtensionPieceViewModel CreateLowerToolExtensionPiece(ToolListViewModel currentToolList, MultiToolViewModel multiTool, string fileName, double length = 0.0, int amount = 0)
	{
		return new LowerToolExtensionPieceViewModel(this, currentToolList, multiTool, fileName, length, amount);
	}

	public void DeleteUpperGroup(UpperToolGroupViewModel upperGroup)
	{
		UpperGroups.Remove(upperGroup);
		_deletedUpperGroups.Add(upperGroup);
		UpperTools.Where((UpperToolViewModel x) => x.Group == upperGroup).ForEach(delegate(UpperToolViewModel x)
		{
			x.Group = UpperGroups.FirstOrDefault();
		});
	}

	public void DeleteLowerGroup(LowerToolGroupViewModel lowerGroup)
	{
		LowerGroups.Remove(lowerGroup);
		_deletedLowerGroups.Add(lowerGroup);
		LowerTools.Where((LowerToolViewModel x) => x.Group == lowerGroup).ForEach(delegate(LowerToolViewModel x)
		{
			x.Group = LowerGroups.FirstOrDefault();
		});
	}

	public void DeleteLowerPiece(LowerToolPieceViewModel lowerPiece)
	{
		LowerPieces.Remove(lowerPiece);
		_deletedLowerPieces.Add(lowerPiece);
		lowerPiece.IsDeleted = true;
		foreach (ToolListViewModel allToolList in AllToolLists)
		{
			if (lowerPiece.ID.HasValue)
			{
				allToolList.SetAmount(lowerPiece, 0);
			}
		}
	}

	public void DeleteLowerToolExtentionPiece(LowerToolExtensionPieceViewModel lowerPiece)
	{
		LowerToolExtentionPieces.Remove(lowerPiece);
		_deletedLowerToolExtensionPieces.Add(lowerPiece);
		lowerPiece.IsDeleted = true;
		foreach (ToolListViewModel allToolList in AllToolLists)
		{
			if (lowerPiece.ID.HasValue)
			{
				allToolList.SetAmount(lowerPiece, 0);
			}
		}
	}

	public void DeleteUpperPiece(UpperToolPieceViewModel upperPiece)
	{
		UpperPieces.Remove(upperPiece);
		_deletedUpperPieces.Add(upperPiece);
		upperPiece.IsDeleted = true;
		foreach (ToolListViewModel toolList in ToolLists)
		{
			if (upperPiece.ID.HasValue)
			{
				toolList.SetAmount(upperPiece, 0);
			}
		}
	}

	public void DeleteUpperTool(UpperToolViewModel upperTool)
	{
		UpperTools.Remove(upperTool);
		_deletedUpperTools.Add(upperTool);
		DeleteMultiToolIfUnused(upperTool.MultiTool);
	}

	public void DeleteLowerTool(LowerToolViewModel lowerTool)
	{
		LowerTools.Remove(lowerTool);
		_deletedLowerTools.Add(lowerTool);
		DeleteMultiToolIfUnused(lowerTool?.MultiTool);
	}

	public void DeleteLowerAdapter(LowerAdapterViewModel lowerAdapter)
	{
		LowerAdapters.Remove(lowerAdapter);
		_deletedLowerAdapters.Add(lowerAdapter);
		DeleteMultiToolIfUnused(lowerAdapter?.MultiTool);
	}

	public void DeleteUpperAdapter(UpperAdapterViewModel upperAdapter)
	{
		UpperAdapters.Remove(upperAdapter);
		_deletedUpperAdapters.Add(upperAdapter);
		DeleteMultiToolIfUnused(upperAdapter?.MultiTool);
	}

	public void DeleteLowerExtensionTool(LowerToolExtensionViewModel lowerExtensionTool)
	{
		LowerToolsExtensions.Remove(lowerExtensionTool);
		_deletedLowerToolsExtensions.Add(lowerExtensionTool);
		DeleteMultiToolIfUnused(lowerExtensionTool?.MultiTool);
	}

	public void DeleteToolList(ToolListViewModel toolList)
	{
		toolList.UsedByMachine = false;
		AllToolLists.Remove(toolList);
		_deletedToolLists.Add(toolList);
	}

	public UpperToolGroupViewModel GetMatchingUpperGroup(double radius)
	{
		UpperToolGroupViewModel upperToolGroupViewModel = UpperGroups.FirstOrDefault((UpperToolGroupViewModel x) => x.Radius == radius);
		if (upperToolGroupViewModel == null)
		{
			upperToolGroupViewModel = CreateUpperToolGroup("R" + radius, radius);
		}
		return upperToolGroupViewModel;
	}

	public UpperToolGroupViewModel GetMatchingUpperGroup(string name, double radius)
	{
		UpperToolGroupViewModel upperToolGroupViewModel = UpperGroups.FirstOrDefault((UpperToolGroupViewModel x) => x.Name == name);
		if (upperToolGroupViewModel == null)
		{
			upperToolGroupViewModel = CreateUpperToolGroup(name, radius);
		}
		return upperToolGroupViewModel;
	}

	public LowerToolGroupViewModel GetAnyLowerGroup()
	{
		if (LowerGroups.Any())
		{
			return LowerGroups.First();
		}
		LowerToolGroupViewModel lowerToolGroupViewModel = new LowerToolGroupViewModel(_unitConverter, _toolStorage);
		LowerGroups.Add(lowerToolGroupViewModel);
		return lowerToolGroupViewModel;
	}

	public LowerToolGroupViewModel GetMatchingLowerGroup(double vWidth, double vAngleDeg, CornerType cornerTyper = CornerType.Default)
	{
		double vAngleRad = vAngleDeg * Math.PI / 180.0;
		LowerToolGroupViewModel lowerToolGroupViewModel = LowerGroups.FirstOrDefault((LowerToolGroupViewModel x) => x.VWidthMm == vWidth && Math.Abs(x.VAngleRad - vAngleRad) < 1E-06);
		if (lowerToolGroupViewModel == null)
		{
			lowerToolGroupViewModel = CreateLowerToolGroup(vWidth, vAngleRad, null, "W" + vWidth + "/A" + vAngleDeg);
		}
		return lowerToolGroupViewModel;
	}

	public LowerToolGroupViewModel GetMatchingLowerGroup(string name, double vWidth, double vAngleDeg)
	{
		LowerToolGroupViewModel lowerToolGroupViewModel = LowerGroups.FirstOrDefault((LowerToolGroupViewModel x) => x.Name == name);
		if (lowerToolGroupViewModel == null)
		{
			lowerToolGroupViewModel = CreateLowerToolGroup(vWidth, vAngleDeg * Math.PI / 180.0, null, name);
		}
		return lowerToolGroupViewModel;
	}

	private void DeleteMultiToolIfUnused(MultiToolViewModel multiTool)
	{
		if ((0u | (LowerTools.Any((LowerToolViewModel x) => x.MultiTool == multiTool) ? 1u : 0u) | (UpperTools.Any((UpperToolViewModel x) => x.MultiTool == multiTool) ? 1u : 0u) | (LowerAdapters.Any((LowerAdapterViewModel x) => x.MultiTool == multiTool) ? 1u : 0u) | (UpperAdapters.Any((UpperAdapterViewModel x) => x.MultiTool == multiTool) ? 1u : 0u)) != 0)
		{
			return;
		}
		LowerPieces.Where((LowerToolPieceViewModel x) => x.MultiTool == multiTool).ToList().ForEach(DeleteLowerPiece);
		UpperPieces.Where((UpperToolPieceViewModel x) => x.MultiTool == multiTool).ToList().ForEach(DeleteUpperPiece);
		_deletedMultiTools.Add(multiTool);
		AllMultiTools.Remove(multiTool);
		if (multiTool == null)
		{
			return;
		}
		int? iD = multiTool.ID;
		if (iD.HasValue)
		{
			int valueOrDefault = iD.GetValueOrDefault();
			_multiToolsWithIDs.Remove(valueOrDefault);
		}
		foreach (LowerToolExtensionPieceViewModel item in multiTool.ToolPieces.OfType<LowerToolExtensionPieceViewModel>())
		{
			DeleteLowerToolExtentionPiece(item);
		}
	}

	private void SaveUpperTools(Dictionary<UpperToolGroupViewModel, IPunchGroup> groupDict)
	{
		foreach (UpperToolViewModel upperTool in UpperTools)
		{
			IPunchProfile punchProfile = ((!upperTool.ID.HasValue) ? null : _toolStorage.GetPunchProfile(upperTool.ID.Value));
			punchProfile = upperTool.Save(punchProfile, (upperTool.Group == null) ? null : groupDict[upperTool.Group]);
			changed = true;
		}
		foreach (UpperToolViewModel deletedUpperTool in _deletedUpperTools)
		{
			int? iD = deletedUpperTool.ID;
			if (iD.HasValue)
			{
				int valueOrDefault = iD.GetValueOrDefault();
				_toolStorage.DeletePunchProfile(valueOrDefault);
			}
		}
	}

	private void SaveLowerTools(Dictionary<LowerToolGroupViewModel, IDieGroup> groupDict)
	{
		foreach (LowerToolViewModel lowerTool in LowerTools)
		{
			IDieProfile dieProfile = ((!lowerTool.ID.HasValue) ? null : _toolStorage.GetDieProfile(lowerTool.ID.Value));
			IAdapterProfile frontAdapterProfile = ((!lowerTool.MountTypeExtensionFrontId.HasValue || !lowerTool.FrontAdapterID.HasValue) ? null : _toolStorage.TryGetLowerAdapter(lowerTool.FrontAdapterID.Value));
			IAdapterProfile backAdapterProfile = ((!lowerTool.MountTypeExtensionBackId.HasValue || !lowerTool.BackAdapterID.HasValue) ? null : _toolStorage.TryGetLowerAdapter(lowerTool.BackAdapterID.Value));
			dieProfile = lowerTool.Save(dieProfile, frontAdapterProfile, backAdapterProfile, (lowerTool.Group == null) ? null : groupDict[lowerTool.Group]);
			changed = true;
		}
		foreach (LowerToolViewModel deletedLowerTool in _deletedLowerTools)
		{
			int? num = deletedLowerTool?.ID;
			if (num.HasValue)
			{
				int valueOrDefault = num.GetValueOrDefault();
				_toolStorage.DeleteDieProfile(valueOrDefault);
			}
		}
	}

	private void SaveLowerToolExtensions()
	{
		foreach (LowerToolExtensionViewModel lowerToolsExtension in LowerToolsExtensions)
		{
			IDieFoldExtentionProfile dieProfile = ((!lowerToolsExtension.ID.HasValue) ? null : _toolStorage.GetDieExtensionProfile(lowerToolsExtension.ID.Value));
			dieProfile = lowerToolsExtension.Save(dieProfile);
			changed = true;
		}
		foreach (LowerToolExtensionViewModel deletedLowerToolsExtension in _deletedLowerToolsExtensions)
		{
			int? iD = deletedLowerToolsExtension.ID;
			if (iD.HasValue)
			{
				int valueOrDefault = iD.GetValueOrDefault();
				_toolStorage.DeleteDieExtensionProfile(valueOrDefault);
			}
		}
	}

	private void SaveUpperAdapters()
	{
		foreach (UpperAdapterViewModel upperAdapter in UpperAdapters)
		{
			IAdapterProfile adapterProfile = ((!upperAdapter.ID.HasValue) ? null : _toolStorage.GetUpperAdapter(upperAdapter.ID.Value));
			if (adapterProfile == null || upperAdapter.IsChanged)
			{
				upperAdapter.Save(adapterProfile);
				changed = true;
			}
		}
		foreach (UpperAdapterViewModel deletedUpperAdapter in _deletedUpperAdapters)
		{
			int? iD = deletedUpperAdapter.ID;
			if (iD.HasValue)
			{
				int valueOrDefault = iD.GetValueOrDefault();
				_toolStorage.DeleteUpperAdapterProfile(valueOrDefault);
			}
		}
	}

	private void SaveLowerAdapters()
	{
		foreach (LowerAdapterViewModel lowerAdapter in LowerAdapters)
		{
			IAdapterProfile adapterProfile = ((!lowerAdapter.ID.HasValue) ? null : _toolStorage.GetLowerAdapter(lowerAdapter.ID.Value));
			if (adapterProfile == null || lowerAdapter.IsChanged)
			{
				lowerAdapter.Save(adapterProfile);
				changed = true;
			}
		}
		foreach (LowerAdapterViewModel deletedLowerAdapter in _deletedLowerAdapters)
		{
			int? iD = deletedLowerAdapter.ID;
			if (iD.HasValue)
			{
				int valueOrDefault = iD.GetValueOrDefault();
				_toolStorage.DeleteLowerAdapterProfile(valueOrDefault);
			}
		}
	}

	private void SaveUpperPieces()
	{
		foreach (UpperToolPieceViewModel upperPiece in UpperPieces)
		{
			IToolPieceProfile toolPieceProfile = ((!upperPiece.ID.HasValue) ? null : _toolStorage.GetPieceProfile(upperPiece.ID.Value));
			if (toolPieceProfile == null || upperPiece.IsChanged)
			{
				upperPiece.Save(toolPieceProfile);
				changed = true;
			}
		}
		foreach (UpperToolPieceViewModel deletedUpperPiece in _deletedUpperPieces)
		{
			int? iD = deletedUpperPiece.ID;
			if (iD.HasValue)
			{
				int valueOrDefault = iD.GetValueOrDefault();
				_toolStorage.DeletePiece(valueOrDefault);
			}
		}
	}

	private void SaveLowerPieces()
	{
		foreach (LowerToolPieceViewModel lowerPiece in LowerPieces)
		{
			IToolPieceProfile toolPieceProfile = ((!lowerPiece.ID.HasValue) ? null : _toolStorage.GetPieceProfile(lowerPiece.ID.Value));
			if (toolPieceProfile == null || lowerPiece.IsChanged)
			{
				lowerPiece.Save(toolPieceProfile);
				changed = true;
			}
		}
		foreach (LowerToolPieceViewModel deletedLowerPiece in _deletedLowerPieces)
		{
			int? iD = deletedLowerPiece.ID;
			if (iD.HasValue)
			{
				int valueOrDefault = iD.GetValueOrDefault();
				_toolStorage.DeletePiece(valueOrDefault);
			}
		}
	}

	private void SaveLowerToolExtensionPieces()
	{
		foreach (LowerToolExtensionPieceViewModel lowerToolExtentionPiece in LowerToolExtentionPieces)
		{
			IToolPieceProfile toolPieceProfile = ((!lowerToolExtentionPiece.ID.HasValue) ? null : _toolStorage.GetPieceProfile(lowerToolExtentionPiece.ID.Value));
			if (toolPieceProfile == null || lowerToolExtentionPiece.IsChanged)
			{
				lowerToolExtentionPiece.Save(toolPieceProfile);
				changed = true;
			}
		}
		foreach (LowerToolExtensionPieceViewModel deletedLowerToolExtensionPiece in _deletedLowerToolExtensionPieces)
		{
			int? iD = deletedLowerToolExtensionPiece.ID;
			if (iD.HasValue)
			{
				int valueOrDefault = iD.GetValueOrDefault();
				_toolStorage.DeletePiece(valueOrDefault);
			}
		}
	}

	private Dictionary<UpperToolGroupViewModel, IPunchGroup> SaveUpperGroups()
	{
		Dictionary<UpperToolGroupViewModel, IPunchGroup> dictionary = new Dictionary<UpperToolGroupViewModel, IPunchGroup>();
		foreach (UpperToolGroupViewModel upperGroup in UpperGroups)
		{
			IPunchGroup punchGroup = ((!upperGroup.ID.HasValue) ? null : _toolStorage.GetPunchGroup(upperGroup.ID.Value));
			punchGroup = upperGroup.Save(punchGroup);
			changed = true;
			dictionary.Add(upperGroup, punchGroup);
		}
		foreach (UpperToolGroupViewModel deletedUpperGroup in _deletedUpperGroups)
		{
			int? iD = deletedUpperGroup.ID;
			if (iD.HasValue)
			{
				int valueOrDefault = iD.GetValueOrDefault();
				_toolStorage.DeletePunchGroup(valueOrDefault);
			}
		}
		return dictionary;
	}

	private Dictionary<LowerToolGroupViewModel, IDieGroup> SaveLowerGroups()
	{
		Dictionary<LowerToolGroupViewModel, IDieGroup> dictionary = new Dictionary<LowerToolGroupViewModel, IDieGroup>();
		foreach (LowerToolGroupViewModel lowerGroup in LowerGroups)
		{
			IDieGroup dieGroup = ((!lowerGroup.ID.HasValue) ? null : _toolStorage.GetDieGroup(lowerGroup.ID.Value));
			dieGroup = lowerGroup.Save(dieGroup);
			changed = true;
			dictionary.Add(lowerGroup, dieGroup);
		}
		foreach (LowerToolGroupViewModel deletedLowerGroup in _deletedLowerGroups)
		{
			int? iD = deletedLowerGroup.ID;
			if (iD.HasValue)
			{
				int valueOrDefault = iD.GetValueOrDefault();
				_toolStorage.DeleteDieGroup(valueOrDefault);
			}
		}
		return dictionary;
	}

	private void SaveAliasMultiTools()
	{
		foreach (AliasMultiToolViewModel allAliasMultiTool in AllAliasMultiTools)
		{
			allAliasMultiTool.Save(_toolStorage);
		}
	}

	private void SaveAliasPieces()
	{
		foreach (AliasPieceViewModel allAliasPiece in AllAliasPieces)
		{
			allAliasPiece.Save(_toolStorage);
		}
	}

	private bool CanSaveAliasPieces()
	{
		return AllAliasPieces.All((AliasPieceViewModel x) => x.CanSave());
	}

	private bool CanSaveToolProfiles()
	{
		if (UpperAdapters.All((UpperAdapterViewModel x) => x.CanSave(MessageLogGlobal)) && LowerAdapters.All((LowerAdapterViewModel x) => x.CanSave(MessageLogGlobal)) && UpperTools.All((UpperToolViewModel x) => x.CanSave(MessageLogGlobal)) && LowerTools.All((LowerToolViewModel x) => x.CanSave(MessageLogGlobal)))
		{
			return LowerToolsExtensions.All((LowerToolExtensionViewModel x) => x.CanSave(MessageLogGlobal));
		}
		return false;
	}

	private void SaveMultiTools()
	{
		foreach (MultiToolViewModel allMultiTool in AllMultiTools)
		{
			IMultiToolProfile multiTool = (allMultiTool.ID.HasValue ? _toolStorage.GetMultiToolProfile(allMultiTool.ID.Value) : null);
			allMultiTool.Save(multiTool);
			changed = true;
		}
		foreach (MultiToolViewModel deletedMultiTool in _deletedMultiTools)
		{
			int? num = deletedMultiTool?.ID;
			if (num.HasValue)
			{
				int valueOrDefault = num.GetValueOrDefault();
				_toolStorage.DeleteMultiToolProfile(valueOrDefault);
			}
		}
	}

	private void SaveToolLists(IBendMachine bendMachine)
	{
		foreach (ToolListViewModel allToolList in AllToolLists)
		{
			IToolListAvailable toolListAvailable = (allToolList.ID.HasValue ? _toolStorage.GetToolList(allToolList.ID.Value) : null);
			if (toolListAvailable == null || allToolList.IsChanged)
			{
				allToolList.Save(toolListAvailable);
				changed = true;
			}
		}
		foreach (ToolListViewModel deletedToolList in _deletedToolLists)
		{
			if (deletedToolList.ID.HasValue)
			{
				_toolStorage.DeleteToolList(deletedToolList.ID.Value);
			}
		}
		List<int> second = (from x in AllToolLists
			where x.UsedByMachine
			select x.ID.Value).Order().ToList();
		if (!bendMachine.ToolLists.Select((IToolListAvailable x) => x.Id).Order().ToList()
			.SequenceEqual(second))
		{
			bendMachine.SetToolLists(from x in AllToolLists
				where x.UsedByMachine
				select x.ID.Value);
			changedMachine = true;
		}
	}

	private void SavaeMappings(IBendMachine bendMachine)
	{
		IPpMappings ppMappings = bendMachine.PpMappings;
		ppMappings.ToolMapping.ToolIdMappings.Clear();
		foreach (MappingViewModel.MappingVm multiToolMapping in MultiToolMappings)
		{
			ppMappings.ToolMapping.ToolIdMappings.Add(multiToolMapping.Profile.Id, multiToolMapping.PpName);
		}
		foreach (MappingViewModel.MappingVm toolMapping in ToolMappings)
		{
			ppMappings.ToolMapping.ToolIdMappings.Add(toolMapping.Profile.Id, toolMapping.PpName);
		}
		ppMappings.ToolMapping.MountTypeIdMappings.Clear();
		foreach (MappingViewModel.MappingVm mountIdMapping in MountIdMappings)
		{
			ppMappings.ToolMapping.MountTypeIdMappings.Add(mountIdMapping.Profile.Id, (mountIdMapping.PpName, mountIdMapping.Profile.Desc));
		}
	}

	private void ToolListSelectionChanged()
	{
		foreach (LowerToolPieceViewModel lowerPiece in LowerPieces)
		{
			lowerPiece.CurrentToolList = _selectedToolList;
		}
		foreach (UpperToolPieceViewModel upperPiece in UpperPieces)
		{
			upperPiece.CurrentToolList = _selectedToolList;
		}
		foreach (LowerToolExtensionPieceViewModel lowerToolExtentionPiece in LowerToolExtentionPieces)
		{
			lowerToolExtentionPiece.CurrentToolList = _selectedToolList;
		}
	}

	public int GenerateFakeId()
	{
		return _lastFakeId--;
	}
}
