using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Telerik.Windows.Data;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendModel.GeometryTools;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendServices;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.ToolCalculation;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiContracts.EventArgs;
using WiCAM.Pn4000.GuiContracts.Implementations;
using WiCAM.Pn4000.GuiContracts.Popups;
using WiCAM.Pn4000.GuiWpf.GeneralUI.Docking;
using WiCAM.Pn4000.GuiWpf.Services;
using WiCAM.Pn4000.PN3D.BendSimulation.BendPropertyPanelUI;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.ToolCalculation.AcbFunctions;
using WiCAM.Pn4000.ToolCalculation.Interfaces;
using WiCAM.Pn4000.ToolCalculationGuiWpf.EditTools.Entities;
using WiCAM.Pn4000.ToolCalculationGuiWpf.EditTools.SubViews;
using WiCAM.Pn4000.ToolCalculationMediator;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditTools;

public class EditToolsViewModel : SubViewModelBase, IEditToolsViewModel, ISubViewModel, IPopupViewModel
{
	private enum CommandType
	{
		Default,
		Undo,
		Redo
	}

	public class SensordDiskVm
	{
		public string TypeName { get; }

		public SensordDiskVm(string typeName)
		{
			TypeName = typeName;
		}
	}

	public class UndoVm
	{
		private readonly IUndo3dSave _save;

		private readonly IUndo3dDocService _undo3dDocService;

		private readonly ITranslator _translator;

		public string Desc => _translator.Translate("Undo3d.HistoryTextFormat", _save.Timestamp, _save.DescriptionLastAction);

		public string Tooltip => _translator.Translate("Undo3d.HistoryTooltipFormat", _save.Timestamp, _save.DescriptionLastAction);

		public ICommand Cmd { get; }

		public UndoVm(IUndo3dSave save, IUndo3dDocService undo3dDocService, ITranslator translator)
		{
			_save = save;
			_undo3dDocService = undo3dDocService;
			_translator = translator;
			Cmd = new RelayCommand(Command);
		}

		private void Command()
		{
			_undo3dDocService.Goto(_save);
		}
	}

	private readonly Stack<ISToolsAndBends> _undoStack = new Stack<ISToolsAndBends>();

	private readonly Stack<ISToolsAndBends> _redoStack = new Stack<ISToolsAndBends>();

	private readonly IEditToolsSelection _toolsSelection;

	private readonly IBendSelection _bendSelection;

	private readonly IToolOperator _toolOperator;

	private readonly IToolFactory _toolFactory;

	private readonly IMessageLogGlobal _messageLogGlobal;

	private readonly ICurrentCalculation _currentToolCalculation;

	private readonly IFactorio _factorio;

	private readonly IToolCalculator _toolCalculator;

	private readonly IUnitConverter _unitConverter;

	private readonly IShortcutSettingsCommon _shortcutSettingsCommon;

	private readonly ShortcutEditTools _shortcutEditTools;

	private readonly IToolsAndBendsSerializer _toolsAndBendsSerializer;

	private readonly IToolCalculations _toolCalculations;

	private readonly IToolSetupMemory _toolSetupMemory;

	private readonly IDoc3d _doc;

	private readonly IAcbCalculator _acbCalculator;

	private EditToolSelectionModes _editToolSelectionMode;

	private bool _isVisible;

	private double _moveDistance = 100.0;

	private double? _adjustSectionLength;

	private double? _globalPosition;

	private object? _subElement;

	private readonly IDockingService _dockingService;

	private readonly IConfigProvider _configProvider;

	private readonly IUndo3dDocService _undo3dDocService;

	private readonly ITranslator _translator;

	private readonly ILogCenterService _logCenterService;

	private ToolSetupsViewModel.ToolSetupsNewDummySetup _dummySetupsNew = new ToolSetupsViewModel.ToolSetupsNewDummySetup();

	private IToolSetups? _selectedSetup;

	private SensordDiskVm? _selectedSensorDisk;

	private Visibility _sensorDiskVisibility = Visibility.Collapsed;

	private static string? _tempSerializedToolSetup;

	private bool _isClosing;

	private bool _disposedValue;

	private bool _refreshingAcb;

	public EditToolSelectionModes EditToolSelectionMode
	{
		get
		{
			return _editToolSelectionMode;
		}
		set
		{
			if (_editToolSelectionMode != value)
			{
				_editToolSelectionMode = value;
				NotifyPropertyChanged("IsSelectionModeToolPiece");
				NotifyPropertyChanged("IsSelectionModeToolSection");
				NotifyPropertyChanged("IsSelectionModeCluster");
				_toolsSelection.SelectionMode = _editToolSelectionMode;
			}
		}
	}

	public bool IsVisible
	{
		get
		{
			return _isVisible;
		}
		private set
		{
			if (_isVisible != value)
			{
				_isVisible = value;
				NotifyPropertyChanged("IsVisible");
			}
		}
	}

	public bool IsSelectionModeToolPiece
	{
		get
		{
			return EditToolSelectionMode == EditToolSelectionModes.ToolPiece;
		}
		set
		{
			if (value)
			{
				EditToolSelectionMode = EditToolSelectionModes.ToolPiece;
			}
		}
	}

	public bool IsSelectionModeToolSection
	{
		get
		{
			return EditToolSelectionMode == EditToolSelectionModes.ToolSection;
		}
		set
		{
			if (value)
			{
				EditToolSelectionMode = EditToolSelectionModes.ToolSection;
			}
		}
	}

	public bool IsSelectionModeCluster
	{
		get
		{
			return EditToolSelectionMode == EditToolSelectionModes.Cluster;
		}
		set
		{
			if (value)
			{
				EditToolSelectionMode = EditToolSelectionModes.Cluster;
			}
		}
	}

	public ICommand CmdChangeToolProfile { get; }

	public ICommand CmdAddToolPiece { get; }

	public ICommand<IToolPieceProfile> CmdAddToolPieceLeft { get; }

	public ICommand<IToolPieceProfile> CmdAddToolPieceRight { get; }

	public ICommand<IAdapterProfile> CmdAddAdapterToSection { get; }

	public ICommand<IToolProfile> CmdAddExtensionToSection { get; }

	public ICommand CmdAddAdapter { get; }

	public ICommand CmdNewSection { get; }

	public ICommand CmdSectionReplace { get; }

	public ICommand CmdDeleteTools { get; }

	public ICommand CmdCenterTools { get; }

	public ICommand CmdMoveLeft { get; }

	public ICommand CmdMoveRight { get; }

	public ICommand CmdMoveUp { get; }

	public ICommand CmdMoveDown { get; }

	public ICommand<double> CmdMoveByDistance { get; }

	public ICommand<double> CmdMoveByDistanceAndMerge { get; }

	public ICommand<int> CmdMoveToIndexInSection { get; }

	public ICommand CmdMoveBendLeft { get; }

	public ICommand CmdMoveBendRight { get; }

	public ICommand<double> CmdMoveBendByDistance { get; }

	public double MoveDistance
	{
		get
		{
			return _moveDistance;
		}
		set
		{
			if (_moveDistance != value)
			{
				_moveDistance = value;
				NotifyPropertyChanged("MoveDistanceUi");
			}
		}
	}

	public double MoveDistanceUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_moveDistance, 1);
		}
		set
		{
			double num = _unitConverter.Length.FromUi(value);
			if (num != _moveDistance)
			{
				_moveDistance = num;
				NotifyPropertyChanged("MoveDistanceUi");
				GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
				generalUserSettingsConfig.MoveDistance3d = num;
				_configProvider.Push(generalUserSettingsConfig);
				_configProvider.Save<GeneralUserSettingsConfig>();
			}
		}
	}

	public ICommand CmdPrettify { get; }

	public ICommand CmdAssignBendsToLocalSections { get; }

	public ICommand CmdAssignAndMoveBendsToSections { get; }

	public ICommand CmdNewSectionForBends { get; }

	public ICommand CmdAdjustSectionLengthLeft { get; }

	public ICommand CmdAdjustSectionLengthRight { get; }

	public ICommand CmdAdjustSectionLengthCenter { get; }

	private double? OriginalSectionLength { get; set; }

	public double? AdjustSectionLength
	{
		get
		{
			return _adjustSectionLength;
		}
		set
		{
			if (value != _adjustSectionLength)
			{
				_adjustSectionLength = value;
				NotifyPropertyChanged("AdjustSectionLengthUi");
			}
		}
	}

	public double? AdjustSectionLengthUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_adjustSectionLength, 1);
		}
		set
		{
			double? num = (value.HasValue ? new double?(_unitConverter.Length.FromUi(value.Value)) : null);
			if (num != _adjustSectionLength)
			{
				_adjustSectionLength = num;
				NotifyPropertyChanged("AdjustSectionLengthUi");
			}
		}
	}

	public double? GlobalPosition
	{
		get
		{
			return _globalPosition;
		}
		set
		{
			if (value != _globalPosition)
			{
				_globalPosition = value;
				NotifyPropertyChanged("GlobalPositionUi");
			}
		}
	}

	public double? GlobalPositionUi
	{
		get
		{
			return _unitConverter.Length.ToUi(_globalPosition, 2);
		}
		set
		{
			double? num = (value.HasValue ? new double?(_unitConverter.Length.FromUi(value.Value)) : null);
			if (num != _globalPosition)
			{
				_globalPosition = num;
				NotifyPropertyChanged("GlobalPositionUi");
				if (_globalPosition.HasValue)
				{
					MoveToGlobalPosition(_globalPosition.Value);
				}
			}
		}
	}

	public string? ToolName => string.Join("; ", (from x in _toolsSelection.SelectionAsPieces.SelectMany((IToolPiece x) => x.PieceProfile.MultiToolProfile.ToolProfiles.Select((IToolProfile p) => p.Name)).Distinct()
		where !string.IsNullOrEmpty(x)
		select x).DefaultIfEmpty(""));

	public ICommand<IDictionary<IToolSection, double>> CmdExtendSectionsLeft { get; set; }

	public ICommand<IDictionary<IToolSection, double>> CmdExtendSectionsRight { get; set; }

	public ICommand CmdFlipTools { get; }

	public RadObservableCollection<ToolSetupsViewModel> ToolSetups { get; } = new RadObservableCollection<ToolSetupsViewModel>();

	public IToolSetups? SelectedSetup
	{
		get
		{
			return _selectedSetup;
		}
		set
		{
			if (_selectedSetup != value)
			{
				_selectedSetup = value;
				if (value == _dummySetupsNew)
				{
					IToolSetups toolSetups = _toolFactory.CreateToolSetup(_toolsSelection.ToolsAndBends, Tools);
					toolSetups.Desc = "manuelle Aufspannung";
					ToolSetups.Add(new ToolSetupsViewModel(toolSetups));
					_selectedSetup = toolSetups;
				}
				_bendSelection.CurrentBend = null;
				_toolsSelection.CurrentSetups = _selectedSetup;
				NotifyPropertyChanged("SelectedSetup");
			}
		}
	}

	public ICommand CmdDeleteToolSetup { get; }

	public ICommand CmdExtractToolPieces { get; }

	public ICommand CmdMergeToolSections { get; }

	public ICommand CmdSaveToolSetups { get; }

	public ICommand CmdLoadToolSetups { get; }

	public ICommand CmdUndo { get; }

	public ICommand CmdRedo { get; }

	public ICommand CmdCloseOk { get; }

	public ICommand CmdCloseCancel { get; }

	public RadObservableCollection<UndoVm> UndoHistory { get; } = new RadObservableCollection<UndoVm>();

	public RadObservableCollection<UndoVm> RedoHistory { get; } = new RadObservableCollection<UndoVm>();

	public bool IsUndoAvailable => _undoStack.Any();

	public bool IsRedoAvailable => _redoStack.Any();

	private IPnBndDoc Doc => _toolsSelection.CurrentDoc;

	public IBendMachineTools Tools => Doc.BendMachine?.ToolConfig;

	public double? GapLeftUi
	{
		get
		{
			if (_toolsSelection.SelectionMode == EditToolSelectionModes.ToolSection)
			{
				List<IToolSection> source = _toolsSelection.SelectedSections.ToList();
				Stack<double?> stack = new Stack<double?>();
				foreach (IGrouping<(bool, double), IToolSection> item in from x in source
					group x by (IsUpperSection: x.IsUpperSection, Math.Round(x.OffsetWorld.Z)))
				{
					IToolSection sec = item.OrderBy((IToolSection x) => x.XMin).First();
					stack.Push(_toolOperator.CalcGapLeft(sec));
				}
				if (stack.Select((double? x) => (!x.HasValue) ? null : new double?(Math.Round(x.Value, 4))).Distinct().Count() == 1)
				{
					return _unitConverter.Length.ToUi(stack.First(), 1);
				}
			}
			return null;
		}
		set
		{
			if (_toolsSelection.SelectionMode == EditToolSelectionModes.ToolSection && value.HasValue)
			{
				double val = _unitConverter.Length.FromUi(value.Value);
				DoActionAsCalculation(delegate
				{
					foreach (IGrouping<(bool, double), IToolSection> item in from x in _toolsSelection.SelectedSections.ToList()
						group x by (IsUpperSection: x.IsUpperSection, Math.Round(x.OffsetWorld.Z)))
					{
						IToolSection sec = item.OrderBy((IToolSection x) => x.XMin).First();
						double? num = val - _toolOperator.CalcGapLeft(sec);
						if (num.HasValue)
						{
							foreach (IToolSection item2 in item)
							{
								item2.OffsetLocalX += num.Value;
							}
						}
					}
				});
			}
			NotifyPropertyChanged("GapLeftUi");
		}
	}

	public double? GapRightUi
	{
		get
		{
			if (_toolsSelection.SelectionMode == EditToolSelectionModes.ToolSection)
			{
				List<IToolSection> source = _toolsSelection.SelectedSections.ToList();
				Stack<double?> stack = new Stack<double?>();
				foreach (IGrouping<(bool, double), IToolSection> item in from x in source
					group x by (IsUpperSection: x.IsUpperSection, Math.Round(x.OffsetWorld.Z)))
				{
					IToolSection sec = item.OrderByDescending((IToolSection x) => x.XMax).First();
					stack.Push(_toolOperator.CalcGapRight(sec));
				}
				if (stack.Select((double? x) => (!x.HasValue) ? null : new double?(Math.Round(x.Value, 4))).Distinct().Count() == 1)
				{
					return _unitConverter.Length.ToUi(stack.First(), 1);
				}
			}
			return null;
		}
		set
		{
			if (_toolsSelection.SelectionMode == EditToolSelectionModes.ToolSection && value.HasValue)
			{
				double val = _unitConverter.Length.FromUi(value.Value);
				DoActionAsCalculation(delegate
				{
					foreach (IGrouping<(bool, double), IToolSection> item in from x in _toolsSelection.SelectedSections.ToList()
						group x by (IsUpperSection: x.IsUpperSection, Math.Round(x.OffsetWorld.Z)))
					{
						IToolSection sec = item.OrderByDescending((IToolSection x) => x.XMax).First();
						double? num = val - _toolOperator.CalcGapRight(sec);
						if (num.HasValue)
						{
							foreach (IToolSection item2 in item)
							{
								item2.OffsetLocalX -= num.Value;
							}
						}
					}
				});
			}
			NotifyPropertyChanged("GapRightUi");
		}
	}

	public RadObservableCollection<SensordDiskVm> AllAvailableSensorDisks { get; } = new RadObservableCollection<SensordDiskVm>();

	public SensordDiskVm? SelectedSensorDisk
	{
		get
		{
			return _selectedSensorDisk;
		}
		set
		{
			if (_selectedSensorDisk == value)
			{
				return;
			}
			_selectedSensorDisk = value;
			IAcbActivator acbActivator = _factorio.Resolve<IAcbActivator>();
			if (!_refreshingAcb)
			{
				if (_selectedSensorDisk != null)
				{
					List<IAcbPunchPiece> list = _toolsSelection.SelectionAsPieces.OfType<IAcbPunchPiece>().ToList();
					Model unfoldModel = _doc.UnfoldModel3D.CopyStructure();
					List<ISensorDisk> source = (_doc.BendMachine?.ToolConfig?.SensorDisks).EmptyIfNull().ToList();
					foreach (IAcbPunchPiece tool in list)
					{
						tool.SensorDisks = source.FirstOrDefault((ISensorDisk x) => x.TypeName == _selectedSensorDisk.TypeName && tool.PieceProfile.MultiToolProfile.ToolProfiles.Contains(x.PunchProfile));
						acbActivator.TryActivate(tool, _doc.ToolsAndBends, unfoldModel, _doc.Material);
					}
				}
				_toolsSelection.RefreshData();
				_doc.RecalculateSimulation();
			}
			NotifyPropertyChanged("SelectedSensorDisk");
		}
	}

	public Visibility SensorDiskVisibility
	{
		get
		{
			return _sensorDiskVisibility;
		}
		set
		{
			if (_sensorDiskVisibility != value)
			{
				_sensorDiskVisibility = value;
				NotifyPropertyChanged("SensorDiskVisibility");
			}
		}
	}

	public string UnitLength => _unitConverter.Length.Unit;

	public EditToolsViewModel(IEditToolsSelection toolsSelection, IBendSelection bendSelection, IToolOperator toolOperator, IToolFactory toolFactory, IMessageLogGlobal messageLogGlobal, ICurrentCalculation currentToolCalculation, IScopedFactorio factorio, IToolCalculator toolCalculator, IUnitConverter unitConverter, IShortcutSettingsCommon shortcutSettingsCommon, ShortcutEditTools shortcutEditTools, IToolsAndBendsSerializer toolsAndBendsSerializer, IToolCalculations toolCalculations, IToolSetupMemory toolSetupMemory, IDoc3d doc, IAcbCalculator acbCalculator, IDockingService dockingService, IConfigProvider configProvider, IUndo3dDocService undo3dDocService, ITranslator translator, ILogCenterService logCenterService)
	{
		_toolsSelection = toolsSelection;
		_bendSelection = bendSelection;
		_toolOperator = toolOperator;
		_toolFactory = toolFactory;
		_messageLogGlobal = messageLogGlobal;
		_currentToolCalculation = currentToolCalculation;
		_factorio = factorio;
		_toolCalculator = toolCalculator;
		_unitConverter = unitConverter;
		_shortcutSettingsCommon = shortcutSettingsCommon;
		_shortcutEditTools = shortcutEditTools;
		_toolsAndBendsSerializer = toolsAndBendsSerializer;
		_toolCalculations = toolCalculations;
		_toolSetupMemory = toolSetupMemory;
		_doc = doc;
		_acbCalculator = acbCalculator;
		_dockingService = dockingService;
		_configProvider = configProvider;
		_undo3dDocService = undo3dDocService;
		_translator = translator;
		_logCenterService = logCenterService;
		CmdCenterTools = AddCommand(CenterTools);
		CmdMoveLeft = AddCommand(MoveLeft);
		CmdMoveRight = AddCommand(MoveRight);
		CmdMoveUp = AddCommand(MoveUp);
		CmdMoveDown = AddCommand(MoveDown);
		CmdMoveByDistance = AddCommand<double>(MoveByDistance);
		CmdMoveByDistanceAndMerge = AddCommand<double>(MoveByDistanceAndMerge);
		CmdMoveToIndexInSection = AddCommand<int>(MoveInSectionToIndex);
		CmdMoveBendLeft = AddCommand(MoveBendLeft);
		CmdMoveBendRight = AddCommand(MoveBendRight);
		CmdMoveBendByDistance = AddCommand<double>(MoveBendByDistance);
		CmdDeleteTools = AddCommand(DeleteTools);
		CmdNewSectionForBends = AddCommand(NewSectionForBends);
		CmdPrettify = AddCommand(Prettify);
		CmdAssignBendsToLocalSections = AddCommand(AssignBendsToLocalSections);
		CmdAssignAndMoveBendsToSections = AddCommand(AssignAndMoveBendsToSections);
		CmdFlipTools = AddCommand(FlipTools);
		CmdAdjustSectionLengthCenter = AddCommand(AdjustSectionLengthCenter, CanAdjustLength);
		CmdAdjustSectionLengthLeft = AddCommand(AdjustSectionLengthLeft, CanAdjustLength);
		CmdAdjustSectionLengthRight = AddCommand(AdjustSectionLengthRight, CanAdjustLength);
		CmdExtendSectionsLeft = AddCommand<IDictionary<IToolSection, double>>(ExtendSectionLeft);
		CmdExtendSectionsRight = AddCommand<IDictionary<IToolSection, double>>(ExtendSectionRight);
		CmdAddToolPiece = AddCommand(StartSubToolPiece);
		CmdAddAdapter = AddCommand(StartSubAdapter);
		CmdNewSection = AddCommand(StartSubToolSection);
		CmdSectionReplace = AddCommand(StartSubToolSectionReplacement);
		CmdAddToolPieceLeft = AddCommand<IToolPieceProfile>(AddToolPieceLeft);
		CmdAddToolPieceRight = AddCommand<IToolPieceProfile>(AddToolPieceRight);
		CmdAddAdapterToSection = AddCommand<IAdapterProfile>(AddAdapterToSection);
		CmdAddExtensionToSection = AddCommand<IToolProfile>(AddExtensionToSection);
		CmdDeleteToolSetup = AddCommand(DeleteToolSetup);
		CmdExtractToolPieces = AddCommand(ExtractToolPieces);
		CmdMergeToolSections = AddCommand(MergeToolSections);
		CmdSaveToolSetups = AddCommand(SaveToolSetups);
		CmdLoadToolSetups = AddCommand(LoadToolSetups);
		CmdUndo = AddCommand(Undo, CanUndo, CommandType.Undo);
		CmdRedo = AddCommand(Redo, CanRedo, CommandType.Redo);
		CmdCloseOk = new RelayCommand(CloseOk);
		CmdCloseCancel = new RelayCommand(CloseCancel);
		_toolsSelection.SelectionChanged += RefreshSelection;
		_toolsSelection.DataRefreshed += RefreshData;
		_toolsSelection.NewSetups += _toolsSelection_NewSetupsBackground;
		_bendSelection.SelectionChanged += BendSelection_SelectionChanged;
		_editToolSelectionMode = _toolsSelection.SelectionMode;
		_toolsSelection_NewSetupsUi();
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		MoveDistance = generalUserSettingsConfig.MoveDistance3d;
		_undo3dDocService.SavesChanged += Undo3dDocService_SavesChanged;
		Undo3dDocService_SavesChanged();
	}

	~EditToolsViewModel()
	{
		Dispose(disposing: false);
	}

	public void Dispose()
	{
		Dispose(disposing: true);
	}

	protected virtual void Dispose(bool disposing)
	{
		IsVisible = false;
		if (!_disposedValue)
		{
			if (disposing)
			{
				_toolsSelection.SelectionChanged -= RefreshSelection;
				_toolsSelection.DataRefreshed -= RefreshData;
				_toolsSelection.NewSetups -= _toolsSelection_NewSetupsBackground;
				_bendSelection.SelectionChanged -= BendSelection_SelectionChanged;
			}
			_disposedValue = true;
		}
	}

	private void BendSelection_SelectionChanged()
	{
		RefreshData();
	}

	public void Activate()
	{
		IsVisible = true;
		_isClosing = false;
		RefreshSelection();
	}

	private void CloseOk()
	{
		Close();
	}

	private void CloseCancel()
	{
		Close();
	}

	private void Undo3dDocService_SavesChanged()
	{
		Application.Current.Dispatcher.BeginInvoke((Action)delegate
		{
			UndoHistory.Clear();
			RedoHistory.Clear();
			UndoHistory.AddRange(from x in _undo3dDocService.GetSavesUndo()
				select new UndoVm(x, _undo3dDocService, _translator));
			RedoHistory.AddRange(from x in _undo3dDocService.GetSavesRedo()
				select new UndoVm(x, _undo3dDocService, _translator));
		});
	}

	private void ShowError(string message)
	{
		_messageLogGlobal.ShowErrorMessage(message);
	}

	private ICommand AddCommand(Action<IToolCalculationOption> action, CommandType type = CommandType.Default)
	{
		return AddCommand(action, null, type);
	}

	private ICommand AddCommand(Action<IToolCalculationOption> action, Func<bool>? canExecute, CommandType type = CommandType.Default)
	{
		return AddCommand(delegate(IToolCalculationOption x, object _)
		{
			action(x);
		}, canExecute, type, action.Method.Name);
	}

	private ICommand<T> AddCommand<T>(Action<IToolCalculationOption, T> action, CommandType type = CommandType.Default)
	{
		return AddCommand(action, null, type);
	}

	private ICommand<T> AddCommand<T>(Action<IToolCalculationOption, T> action, Func<bool>? canExecute, CommandType type = CommandType.Default, string? actionName = null)
	{
		return new RelayCommand<T>(delegate(T param)
		{
			string desc = actionName ?? action.Method.Name;
			if (_doc.BendMachine == null)
			{
				_doc.MessageDisplay.ShowTranslatedErrorMessage("EditToolsViewModel.NoMachineSelected");
			}
			else
			{
				IToolCalculationOption option = _toolCalculations.CreateDefaultOptions(Doc);
				if (!_currentToolCalculation.TryStartNewCalculation(option?.CalculationArg))
				{
					_doc.MessageDisplay.ShowWarningMessage(_translator.Translate("EditToolsViewModel.CommandAlreadyRunning"), null, notificationStyle: true);
				}
				else
				{
					option.CalculationArg.SetStatus("Executing Command " + desc);
					Stopwatch sw = null;
					ILogStep log = option.CalculationArg.DebugInfo?.Log;
					if (log != null)
					{
						log.DescShort = "Command " + desc;
						sw = new Stopwatch();
						sw.Start();
					}
					Task.Run(delegate
					{
						ISToolsAndBends item = _toolsAndBendsSerializer.Convert(_toolsSelection.ToolsAndBends);
						switch (type)
						{
						case CommandType.Undo:
							_redoStack.Push(item);
							break;
						case CommandType.Redo:
							_undoStack.Push(item);
							break;
						default:
							_undoStack.Push(item);
							_redoStack.Clear();
							break;
						}
						action(option, param);
					}).ContinueWith(delegate(Task t)
					{
						if (log != null)
						{
							log.WriteLine($"Command ended in {sw.ElapsedMilliseconds} ms");
							if (t.IsFaulted)
							{
								log.WriteLine("Error during " + desc + Environment.NewLine + t.Exception.Message, t.Exception);
							}
						}
						if (t.IsFaulted)
						{
							ShowError("Error during " + desc + Environment.NewLine + t.Exception.Message);
							_logCenterService.CatchRaport(t.Exception);
						}
						_ = t.IsCompleted;
						_toolsSelection.RefreshData();
						Doc.RecalculateSimulation();
						RefreshSelection();
						_currentToolCalculation.EndCalculation(option.CalculationArg);
					}, TaskScheduler.FromCurrentSynchronizationContext());
				}
			}
		}, (T _) => _currentToolCalculation.CurrentCalculationOption == null && (canExecute?.Invoke() ?? true));
	}

	private void RefreshSelection()
	{
		Application.Current.Dispatcher.Invoke(delegate
		{
			NotifyPropertyChanged("GapLeftUi");
			NotifyPropertyChanged("GapRightUi");
			NotifyPropertyChanged("ToolName");
			RefreshSectionLength();
			RefreshGlobalPosition();
			RefreshAcb();
			CommandManager.InvalidateRequerySuggested();
		});
	}

	private void RefreshData()
	{
		_selectedSetup = _toolsSelection.CurrentSetups;
		NotifyPropertyChanged("SelectedSetup");
		CommandManager.InvalidateRequerySuggested();
	}

	private void _toolsSelection_NewSetupsBackground()
	{
		Application.Current.Dispatcher.Invoke(_toolsSelection_NewSetupsUi);
	}

	private void _toolsSelection_NewSetupsUi()
	{
		ToolSetups.Clear();
		ToolSetups.Add(new ToolSetupsViewModel(_dummySetupsNew));
		List<IToolSetups> list = _toolsSelection.ToolsAndBends?.ToolSetups;
		if (list != null && list.Count > 0)
		{
			ToolSetups.AddRange(list.Select((IToolSetups x) => new ToolSetupsViewModel(x)));
		}
	}

	private void RefreshSectionLength()
	{
		IEnumerable<double> enumerable;
		switch (_toolsSelection.SelectionMode)
		{
		case EditToolSelectionModes.ToolPiece:
			enumerable = _toolsSelection.SelectedPieces.Select((IToolPiece x) => x.Length);
			break;
		case EditToolSelectionModes.ToolSection:
			enumerable = _toolsSelection.SelectedSections.Select((IToolSection x) => x.Length);
			break;
		default:
			AdjustSectionLength = null;
			OriginalSectionLength = null;
			return;
		}
		double? num = null;
		foreach (double item in enumerable)
		{
			if (num.HasValue)
			{
				if (Math.Abs(num.Value - item) > 1E-06)
				{
					AdjustSectionLength = null;
					OriginalSectionLength = null;
					return;
				}
			}
			else
			{
				num = item;
			}
		}
		AdjustSectionLength = num;
		OriginalSectionLength = num;
	}

	private void RefreshGlobalPosition()
	{
		IEnumerable<double> enumerable;
		switch (_toolsSelection.SelectionMode)
		{
		case EditToolSelectionModes.ToolPiece:
			enumerable = _toolsSelection.SelectedPieces.Select((IToolPiece x) => x.OffsetWorld.X);
			break;
		case EditToolSelectionModes.ToolSection:
			enumerable = _toolsSelection.SelectedSections.Select((IToolSection x) => x.OffsetWorld.X);
			break;
		case EditToolSelectionModes.Cluster:
			enumerable = _toolsSelection.SelectedClusters.Select((IToolCluster x) => x.XMin ?? x.OffsetWorld.X);
			break;
		default:
			GlobalPosition = null;
			return;
		}
		double? globalPosition = null;
		foreach (double item in enumerable)
		{
			if (globalPosition.HasValue)
			{
				if (Math.Abs(globalPosition.Value - item) > 1E-06)
				{
					GlobalPosition = null;
					return;
				}
			}
			else
			{
				globalPosition = item;
			}
		}
		GlobalPosition = globalPosition;
	}

	private void RefreshAcb()
	{
		_refreshingAcb = true;
		List<IToolPiece> list = _toolsSelection.SelectionAsPieces.ToList();
		if (list.Count > 0 && list.All((IToolPiece x) => x.PieceProfile.SpecialToolType.HasFlag(SpecialToolTypes.Acb) || x.PieceProfile.SpecialToolType.HasFlag(SpecialToolTypes.AcbWireless)))
		{
			List<IAcbPunchPiece> source = list.OfType<IAcbPunchPiece>().ToList();
			List<ISensorDisk> list2 = (from x in source
				select x.SensorDisks into x
				where x != null
				select x).Distinct().ToList();
			HashSet<IMultiToolProfile> acbToolProfiles = source.Select((IAcbPunchPiece x) => x.PieceProfile.MultiToolProfile).ToHashSet();
			ISensorDisk selected = null;
			if (list2.Count == 1)
			{
				selected = list2.First();
			}
			List<ISensorDisk> source2 = (_doc.BendMachine?.ToolConfig?.SensorDisks.Where((ISensorDisk x) => x.PunchProfile != null)).EmptyIfNull().ToList();
			IOrderedEnumerable<string> source3 = from x in (from x in source2
					where acbToolProfiles.Contains(x.PunchProfile.MultiToolProfile)
					group x by x.PunchProfile).Aggregate(source2.Select((ISensorDisk x) => x.TypeName).Distinct().ToList(), (List<string> last, IGrouping<IPunchProfile, ISensorDisk> x) => last.Intersect(x.Select((ISensorDisk y) => y.TypeName).Distinct().ToList()).ToList())
				orderby x
				select x;
			AllAvailableSensorDisks.Clear();
			AllAvailableSensorDisks.Add(new SensordDiskVm("-"));
			AllAvailableSensorDisks.AddRange(source3.Select((string x) => new SensordDiskVm(x)));
			SelectedSensorDisk = ((list2.Count == 1) ? AllAvailableSensorDisks.FirstOrDefault((SensordDiskVm x) => x.TypeName == selected.TypeName) : null);
			SensorDiskVisibility = Visibility.Visible;
		}
		else
		{
			SensorDiskVisibility = Visibility.Collapsed;
		}
		_refreshingAcb = false;
	}

	private void SaveUndo(string msgKey, params object[] parameters)
	{
		_undo3dDocService.Save(_translator.Translate(msgKey, parameters));
	}

	private void CenterTools(IToolCalculationOption option)
	{
		_toolOperator.CenterTools(_toolsSelection.CurrentSetups);
		SaveUndo("Undo3d.CenterTools");
	}

	private void MoveLeft(IToolCalculationOption option)
	{
		Move(0.0 - MoveDistance, option);
		SaveUndo("Undo3d.Move", 0.0 - MoveDistance);
	}

	private void MoveRight(IToolCalculationOption option)
	{
		Move(MoveDistance, option);
		SaveUndo("Undo3d.Move", MoveDistance);
	}

	private void MoveUp(IToolCalculationOption option)
	{
		MoveVertical(up: true, option);
	}

	private void MoveDown(IToolCalculationOption option)
	{
		MoveVertical(up: false, option);
	}

	private void MoveByDistance(IToolCalculationOption option, double distance)
	{
		Move(distance, option);
		SaveUndo("Undo3d.Move", distance);
	}

	private bool MoveUpCanExecute()
	{
		return MoveVerticalCanExecute(up: true);
	}

	private bool MoveDownCanExecute()
	{
		return MoveVerticalCanExecute(up: false);
	}

	private void MoveByDistanceAndMerge(IToolCalculationOption option, double distance)
	{
		Move(distance, option);
		MergeToolSectionsInCurrentSetup(option);
		SaveUndo("Undo3d.Move", distance);
	}

	private void Move(double distance, IToolCalculationOption option)
	{
		if (!_toolsSelection.SelectionAsPieces.Any())
		{
			return;
		}
		IAcbActivator acbActivator = _factorio.Resolve<IAcbActivator>();
		switch (_toolsSelection.SelectionMode)
		{
		case EditToolSelectionModes.ToolPiece:
			foreach (IToolSection item in _toolFactory.SplitSections(_toolsSelection.SelectedPieces).ToList())
			{
				item.OffsetLocalX += distance;
			}
			break;
		case EditToolSelectionModes.ToolSection:
			foreach (IToolSection selectedSection in _toolsSelection.SelectedSections)
			{
				selectedSection.OffsetLocalX += distance;
			}
			break;
		case EditToolSelectionModes.Cluster:
			foreach (IToolCluster selectedCluster in _toolsSelection.SelectedClusters)
			{
				selectedCluster.OffsetLocalX += distance;
			}
			break;
		}
		foreach (IAcbPunchPiece item2 in _toolsSelection.SelectionAsPieces.OfType<IAcbPunchPiece>())
		{
			acbActivator.TryActivate(item2, _doc.ToolsAndBends, _doc.UnfoldModel3D, _doc.Material);
		}
	}

	private void MoveInSectionToIndex(IToolCalculationOption option, int newIndex)
	{
		MoveToIndex(newIndex, option);
		SaveUndo("Undo3d.Swap", newIndex);
	}

	private void MoveToIndex(int newIndex, IToolCalculationOption option)
	{
		List<IToolPiece> list = _toolsSelection.SelectionAsPieces.ToList();
		if (list.Count != 1)
		{
			return;
		}
		IToolSection toolSection = list.First().ToolSection;
		double num = list.Sum((IToolPiece x) => x.Length);
		double num2 = toolSection.Pieces[newIndex].OffsetLocal.X - list.Min((IToolPiece x) => x.OffsetLocal.X);
		int num3 = toolSection.Pieces.IndexOf(list.First());
		if (newIndex > num3)
		{
			num2 -= num;
			num2 += toolSection.Pieces[newIndex].Length;
		}
		for (int i = 0; i < toolSection.Pieces.Count; i++)
		{
			IToolPiece toolPiece = toolSection.Pieces[i];
			Vector3d offsetLocal = toolPiece.OffsetLocal;
			if (list.Contains(toolPiece))
			{
				offsetLocal.X += num2;
			}
			else if (newIndex < num3 && i < num3 && i >= newIndex)
			{
				offsetLocal.X += num;
			}
			else if (newIndex > num3 && i > num3 && i <= newIndex)
			{
				offsetLocal.X -= num;
			}
			toolPiece.OffsetLocal = offsetLocal;
		}
		toolSection.Pieces.Sort((IToolPiece x, IToolPiece y) => x.OffsetLocal.X.CompareTo(y.OffsetLocal.X));
	}

	private void MoveToGlobalPosition(double position)
	{
		switch (_toolsSelection.SelectionMode)
		{
		default:
			return;
		case EditToolSelectionModes.ToolPiece:
			foreach (IToolSection item in _toolFactory.SplitSections(_toolsSelection.SelectedPieces).ToList())
			{
				Vector3d offsetWorld3 = item.OffsetWorld;
				offsetWorld3.X = position;
				item.OffsetWorld = offsetWorld3;
			}
			break;
		case EditToolSelectionModes.ToolSection:
			foreach (IToolSection selectedSection in _toolsSelection.SelectedSections)
			{
				Vector3d offsetWorld2 = selectedSection.OffsetWorld;
				offsetWorld2.X = position;
				selectedSection.OffsetWorld = offsetWorld2;
			}
			break;
		case EditToolSelectionModes.Cluster:
			foreach (IToolCluster selectedCluster in _toolsSelection.SelectedClusters)
			{
				double num = selectedCluster.XMin ?? selectedCluster.OffsetWorld.X;
				double num2 = position - num;
				Vector3d offsetWorld = selectedCluster.OffsetWorld;
				offsetWorld.X += num2;
				selectedCluster.OffsetWorld = offsetWorld;
			}
			break;
		}
		IAcbActivator acbActivator = _factorio.Resolve<IAcbActivator>();
		foreach (IAcbPunchPiece item2 in _toolsSelection.SelectionAsPieces.OfType<IAcbPunchPiece>())
		{
			acbActivator.TryActivate(item2, _doc.ToolsAndBends, _doc.UnfoldModel3D, _doc.Material);
		}
		_bendSelection.RefreshSimulation();
	}

	private bool MoveVerticalCanExecute(bool up)
	{
		return true;
	}

	private void MoveVertical(bool up, IToolCalculationOption option)
	{
		if (!_toolsSelection.SelectionAsPieces.Any())
		{
			return;
		}
		IAcbActivator acbActivator = _factorio.Resolve<IAcbActivator>();
		HashSet<IToolSection> hashSet = new HashSet<IToolSection>();
		switch (_toolsSelection.SelectionMode)
		{
		default:
			return;
		case EditToolSelectionModes.ToolPiece:
			hashSet.AddRange(_toolFactory.SplitSections(_toolsSelection.SelectedPieces).ToList());
			break;
		case EditToolSelectionModes.ToolSection:
			hashSet.AddRange(_toolsSelection.SelectedSections);
			break;
		case EditToolSelectionModes.Cluster:
			return;
		}
		MoveVertical(hashSet.Where((IToolSection x) => x.IsUpperSection).ToHashSet(), sectionsUpper: true, up);
		MoveVertical(hashSet.Where((IToolSection x) => !x.IsUpperSection).ToHashSet(), sectionsUpper: false, up);
		foreach (IAcbPunchPiece item in _toolsSelection.SelectionAsPieces.OfType<IAcbPunchPiece>())
		{
			acbActivator.TryActivate(item, _doc.ToolsAndBends, _doc.UnfoldModel3D, _doc.Material);
		}
	}

	private void MoveVertical(HashSet<IToolSection> sections, bool sectionsUpper, bool moveUp)
	{
		if (sections.Count <= 0)
		{
			return;
		}
		List<double> list = new List<double> { 0.0 };
		foreach (IToolSection allSection in sections.First().Cluster.Root.AllSections)
		{
			if (allSection.IsUpperSection != sectionsUpper || sections.Contains(allSection))
			{
				continue;
			}
			double z2 = allSection.OffsetWorld.Z;
			list.Add(z2);
			foreach (IToolProfile toolProfile in allSection.MultiToolProfile.ToolProfiles)
			{
				if (toolProfile.IsAdapter)
				{
					list.Add(z2 + (sectionsUpper ? (0.0 - toolProfile.WorkingHeight) : toolProfile.WorkingHeight));
				}
			}
		}
		List<double> list2 = new List<double>();
		foreach (double item in list)
		{
			bool flag = true;
			foreach (double item2 in list2)
			{
				if (Math.Abs(item2 - item) < 1E-06)
				{
					flag = false;
					break;
				}
			}
			if (flag)
			{
				list2.Add(item);
			}
		}
		list = list2.Order().ToList();
		foreach (IToolSection section in sections)
		{
			double offsetGlobal = section.OffsetWorld.Z;
			Vector3d offsetLocal = section.OffsetLocal;
			double num = (moveUp ? list.FirstOrDefault((double z) => z > offsetGlobal + 1E-06, double.NaN) : list.LastOrDefault((double z) => z < offsetGlobal - 1E-06, double.NaN));
			if (!double.IsNaN(num))
			{
				offsetLocal.Z += num - offsetGlobal;
				section.OffsetLocal = offsetLocal;
			}
		}
	}

	private void MoveBendLeft(IToolCalculationOption option)
	{
		MoveBendByDistance(option, 0.0 - MoveDistance);
	}

	private void MoveBendRight(IToolCalculationOption option)
	{
		MoveBendByDistance(option, MoveDistance);
	}

	private void MoveBendByDistance(IToolCalculationOption option, double distance)
	{
		IBendPositioning currentBend = _bendSelection.CurrentBend;
		if (currentBend != null)
		{
			Vector3d offset = currentBend.Offset;
			offset.X = currentBend.Offset.X + distance;
			currentBend.Offset = offset;
			_toolOperator.AssignBendToLocalSections(currentBend, Doc.BendMachine.ToolConfig.ProfileSelector);
			_factorio.Resolve<IAcbActivator>().TryActivate(currentBend.Anchor.Root, currentBend, _doc.UnfoldModel3D, _doc.Material);
			SaveUndo("Undo3d.MoveBendByDistance", distance);
		}
	}

	private void DeleteTools(IToolCalculationOption option)
	{
		switch (_toolsSelection.SelectionMode)
		{
		case EditToolSelectionModes.ToolPiece:
		{
			foreach (IToolPiece selectedPiece in _toolsSelection.SelectedPieces)
			{
				if (selectedPiece is IAcbPunchPiece key)
				{
					foreach (IBendPositioning bendPosition in _doc.ToolsAndBends.BendPositions)
					{
						bendPosition.AcbStatus.Remove(key);
					}
				}
				IToolSection toolSection = selectedPiece.ToolSection;
				if (toolSection.Pieces.Count == 1)
				{
					_toolOperator.RemoveSectionAndMovePossibleChildren(toolSection);
				}
				else
				{
					_toolFactory.RemoveToolPiece(selectedPiece);
				}
			}
			IAcbActivator acbActivator = _factorio.Resolve<IAcbActivator>();
			foreach (IAcbPunchPiece item in _toolsSelection.SelectedPieces.SelectMany((IToolPiece x) => x.ToolSection.Pieces.OfType<IAcbPunchPiece>()))
			{
				acbActivator.TryActivate(item, _doc.ToolsAndBends, _doc.UnfoldModel3D, _doc.Material);
			}
			break;
		}
		case EditToolSelectionModes.ToolSection:
			foreach (IToolSection selectedSection in _toolsSelection.SelectedSections)
			{
				foreach (IAcbPunchPiece item2 in selectedSection.Pieces.OfType<IAcbPunchPiece>())
				{
					foreach (IBendPositioning bendPosition2 in _doc.ToolsAndBends.BendPositions)
					{
						bendPosition2.AcbStatus.Remove(item2);
					}
				}
				_toolOperator.RemoveSectionAndMovePossibleChildren(selectedSection);
			}
			break;
		case EditToolSelectionModes.Cluster:
			foreach (IToolCluster selectedCluster in _toolsSelection.SelectedClusters)
			{
				foreach (IAcbPunchPiece item3 in _toolsSelection.SelectedClusters.SelectMany((IToolCluster x) => x.AllChildren).SelectMany((IToolCluster x) => x.Sections).SelectMany((IToolSection x) => x.Pieces)
					.OfType<IAcbPunchPiece>())
				{
					foreach (IBendPositioning bendPosition3 in _doc.ToolsAndBends.BendPositions)
					{
						bendPosition3.AcbStatus.Remove(item3);
					}
				}
				_toolFactory.RemoveCluster(selectedCluster, _toolsSelection.ToolsAndBends);
			}
			break;
		}
		foreach (IBendPositioning bendPosition4 in _bendSelection.ToolsAndBends.BendPositions)
		{
			_toolOperator.AssignBendZ(bendPosition4);
		}
		_toolsSelection.UnselectAll();
		SaveUndo("Undo3d.DeleteTools");
	}

	private void NewSectionForBends(IToolCalculationOption option)
	{
		try
		{
			List<IBendPositioning> list = _bendSelection.SelectedBends.ToList();
			if (list.Count > 0 && Tools != null)
			{
				EnsureCurrentSetup();
				IToolSetups currentSetups = _toolsSelection.CurrentSetups;
				bool num = !currentSetups.Children.Any();
				_toolCalculator.CreateStationsForBends(list, currentSetups, Tools, _toolsSelection.ToolsAndBends, Doc, option, option.CalculationArg.DebugInfo?.CurrentLog, addAdapters: false);
				if (num)
				{
					_toolOperator.CenterTools(currentSetups);
				}
			}
		}
		catch (Exception)
		{
		}
	}

	private void Prettify(IToolCalculationOption option)
	{
		_toolOperator.Prettify(_toolsSelection.ToolsAndBends, snapHeights: true, assignBends: true, Doc.BendMachine.ToolConfig.ProfileSelector);
	}

	private void EnsureCurrentSetup()
	{
		if (_toolsSelection.CurrentSetups == null)
		{
			Application.Current.Dispatcher.Invoke(delegate
			{
				SelectedSetup = _dummySetupsNew;
			});
		}
	}

	private void FlipTools(IToolCalculationOption option)
	{
		foreach (IToolPiece selectionAsPiece in _toolsSelection.SelectionAsPieces)
		{
			selectionAsPiece.Flipped = !selectionAsPiece.Flipped;
		}
		SaveUndo("Undo3d.FlipTools");
	}

	private void ExtendSectionLeft(IToolCalculationOption option, IDictionary<IToolSection, double> lengths)
	{
		ExtendSection(option, lengths, extendLeft: true);
		SaveUndo("Undo3d.ExtendSectionLeft");
	}

	private void ExtendSectionRight(IToolCalculationOption option, IDictionary<IToolSection, double> lengths)
	{
		ExtendSection(option, lengths, extendLeft: false);
		SaveUndo("Undo3d.ExtendSectionRight");
	}

	private void ExtendSection(IToolCalculationOption option, IDictionary<IToolSection, double> lengths, bool extendLeft)
	{
		if (_toolsSelection.SelectionMode != EditToolSelectionModes.ToolSection)
		{
			return;
		}
		foreach (IToolSection selectedSection in _toolsSelection.SelectedSections)
		{
			if (lengths.TryGetValue(selectedSection, out var value))
			{
				AdjustLengthForSingleSection(option, selectedSection, value, extendLeft ? 1 : 0);
			}
		}
		List<IAcbPunchPiece> pieces = _toolsSelection.SelectedSections.SelectMany((IToolSection s) => s.Pieces).OfType<IAcbPunchPiece>().ToList();
		_acbCalculator.AutoSelectAcbDisks(_bendSelection.ToolsAndBends.BendPositions.ToList(), pieces, _doc.UnfoldModel3D, _doc.Material, _doc.BendMachine.ToolConfig);
		RefreshAcb();
	}

	private bool CanAdjustLength()
	{
		if (_toolsSelection.SelectionMode == EditToolSelectionModes.ToolSection && OriginalSectionLength.HasValue && AdjustSectionLength.HasValue)
		{
			return Math.Abs(OriginalSectionLength.Value - AdjustSectionLength.Value) > 1E-06;
		}
		return false;
	}

	private void AdjustSectionLengthCenter(IToolCalculationOption option)
	{
		AdjustSectionLengthFactor(option, 0.5);
	}

	private void AdjustSectionLengthLeft(IToolCalculationOption option)
	{
		AdjustSectionLengthFactor(option, 1.0);
	}

	private void AdjustSectionLengthRight(IToolCalculationOption option)
	{
		AdjustSectionLengthFactor(option, 0.0);
	}

	private void AdjustSectionLengthFactor(IToolCalculationOption option, double factorMove)
	{
		if (!CanAdjustLength() || !AdjustSectionLength.HasValue)
		{
			return;
		}
		double value = AdjustSectionLength.Value;
		foreach (IToolSection selectedSection in _toolsSelection.SelectedSections)
		{
			AdjustLengthForSingleSection(option, selectedSection, value, factorMove);
		}
		SaveUndo("Undo3d.AdjustSectionLength");
	}

	private void AdjustLengthForSingleSection(IToolCalculationOption option, IToolSection section, double length, double factorMove)
	{
		double length2 = section.Length;
		double offsetLocalX = section.OffsetLocalX;
		_toolOperator.SetSectionLength(section, length, Doc.ToolManager, option);
		double length3 = section.Length;
		section.OffsetLocalX = offsetLocalX - factorMove * (length3 - length2);
		_toolOperator.OrderPiecesInSection(section, option.DefaultSortingStrategy);
	}

	private bool ValidateUndoRedoToolsAndBends(IToolsAndBends tab)
	{
		HashSet<(int, int?, int?)> hashSet = _doc.BendDescriptors.Select((IBendDescriptor x) => x.BendParams.UnfoldFaceGroup.ToFaceGroupIdentifier()).ToHashSet();
		foreach (var item in tab.BendPositions.SelectMany((IBendPositioning b) => b.Bend.FaceGroupIdentifiers))
		{
			if (!hashSet.Contains(item))
			{
				return false;
			}
		}
		return true;
	}

	private void Undo(IToolCalculationOption option)
	{
		Application.Current.Dispatcher.Invoke(delegate
		{
			_undo3dDocService.Undo();
		});
	}

	private bool CanUndo()
	{
		return _undo3dDocService.GetSavesUndo().Any();
	}

	private bool CanRedo()
	{
		return _undo3dDocService.GetSavesRedo().Any();
	}

	private void Redo(IToolCalculationOption option)
	{
		Application.Current.Dispatcher.Invoke(delegate
		{
			_undo3dDocService.Redo();
		});
	}

	private void AddToolPieceLeft(IToolCalculationOption option, IToolPieceProfile newProfile)
	{
		IToolPiece toolPiece = null;
		switch (_toolsSelection.SelectionMode)
		{
		case EditToolSelectionModes.ToolPiece:
		{
			IToolPiece toolPiece2 = _toolsSelection.SelectedPieces.First();
			toolPiece = _toolFactory.CreateToolPiece(newProfile, toolPiece2, 0);
			toolPiece2.ToolSection.OffsetLocalX -= newProfile.Length;
			break;
		}
		case EditToolSelectionModes.ToolSection:
		{
			IToolSection toolSection = _toolsSelection.SelectedSections.First();
			toolPiece = _toolFactory.CreateToolPiece(newProfile, toolSection.Pieces.First(), 0);
			toolSection.OffsetLocalX -= newProfile.Length;
			break;
		}
		}
		IAcbPunchPiece acbPiece = toolPiece as IAcbPunchPiece;
		if (acbPiece != null)
		{
			List<IBendPositioning> bends = _doc.ToolsAndBends.BendPositions.Where((IBendPositioning x) => x.Anchor.Root == acbPiece.ToolSection.Cluster.Root).ToList();
			AutoSelectAcbDisks(bends, acbPiece);
		}
		IAcbActivator acbActivator = _factorio.Resolve<IAcbActivator>();
		foreach (IToolPiece piece in toolPiece.ToolSection.Pieces)
		{
			if (piece != toolPiece && piece is IAcbPunchPiece acbTool)
			{
				acbActivator.TryActivate(acbTool, _doc.ToolsAndBends, _doc.UnfoldModel3D, _doc.Material);
			}
		}
		SaveUndo("Undo3d.AddToolPiece");
	}

	private void AddToolPieceRight(IToolCalculationOption option, IToolPieceProfile newProfile)
	{
		IToolPiece toolPiece = null;
		switch (_toolsSelection.SelectionMode)
		{
		case EditToolSelectionModes.ToolPiece:
		{
			IToolPiece refPiece = _toolsSelection.SelectedPieces.First();
			toolPiece = _toolFactory.CreateToolPiece(newProfile, refPiece, 1);
			break;
		}
		case EditToolSelectionModes.ToolSection:
		{
			IToolSection toolSection = _toolsSelection.SelectedSections.First();
			toolPiece = _toolFactory.CreateToolPiece(newProfile, toolSection.Pieces.Last(), 1);
			break;
		}
		}
		IAcbPunchPiece acbPiece = toolPiece as IAcbPunchPiece;
		if (acbPiece != null)
		{
			List<IBendPositioning> bends = _doc.ToolsAndBends.BendPositions.Where((IBendPositioning x) => x.Anchor.Root == acbPiece.ToolSection.Cluster.Root).ToList();
			AutoSelectAcbDisks(bends, acbPiece);
		}
		IAcbActivator acbActivator = _factorio.Resolve<IAcbActivator>();
		foreach (IToolPiece piece in toolPiece.ToolSection.Pieces)
		{
			if (piece != toolPiece && piece is IAcbPunchPiece acbTool)
			{
				acbActivator.TryActivate(acbTool, _doc.ToolsAndBends, _doc.UnfoldModel3D, _doc.Material);
			}
		}
		SaveUndo("Undo3d.AddToolPiece");
	}

	private void AutoSelectAcbDisks(List<IBendPositioning> bends, IAcbPunchPiece pice)
	{
		_acbCalculator.AutoSelectAcbDisks(bends, pice, _doc.UnfoldModel3D, _doc.Material, _doc.BendMachine?.ToolConfig);
	}

	private void AddAdapterToSection(IToolCalculationOption option, IAdapterProfile profile)
	{
		IToolSection toolSection = _toolsSelection.SelectedSections.FirstOrDefault();
		if (_toolsSelection.SelectionMode != EditToolSelectionModes.ToolSection || toolSection == null)
		{
			return;
		}
		_toolOperator.AddAdapters(toolSection, profile, Doc.ToolManager, option);
		foreach (IBendPositioning bendPosition in _bendSelection.ToolsAndBends.BendPositions)
		{
			_toolOperator.AssignBendZ(bendPosition);
		}
	}

	private void AddExtensionToSection(IToolCalculationOption option, IToolProfile profile)
	{
		IToolSection toolSection = _toolsSelection.SelectedSections.FirstOrDefault();
		if (_toolsSelection.SelectionMode != EditToolSelectionModes.ToolSection || toolSection == null)
		{
			return;
		}
		_toolOperator.AddExtensions(toolSection, profile, Doc.ToolManager, option);
		foreach (IBendPositioning bendPosition in _bendSelection.ToolsAndBends.BendPositions)
		{
			_toolOperator.AssignBendZ(bendPosition);
		}
	}

	private void AssignBendsToLocalSections(IToolCalculationOption option)
	{
		foreach (IBendPositioning item in _bendSelection.SelectedBends.ToList())
		{
			_toolOperator.AssignBendToLocalSections(item, Doc.BendMachine.ToolConfig.ProfileSelector);
		}
	}

	private void AssignAndMoveBendsToSections(IToolCalculationOption option)
	{
		foreach (IBendPositioning item in _bendSelection.SelectedBends.ToList())
		{
			IToolSetups toolSetups = item.Anchor?.Root;
			if (toolSetups != null)
			{
				_toolCalculator.AssignBendToBestSections(item, toolSetups, Doc.BendMachine.ToolConfig.ProfileSelector, Doc, _toolsSelection.ToolsAndBends, option);
			}
		}
		SaveUndo("Undo3d.AssignAndMoveBendsToSections");
	}

	public void DoCmdChangeToolSetups(IBendPositioning bend, IToolSetups root, IToolCluster? station)
	{
		AddCommand(delegate(IToolCalculationOption option)
		{
			_toolCalculator.AssignBendToBestSections(bend, root, Doc.BendMachine.ToolConfig.ProfileSelector, Doc, _toolsSelection.ToolsAndBends, option);
		}).Execute(null);
	}

	private void StartSubToolPiece(IToolCalculationOption option)
	{
		if (_toolsSelection.SelectionMode == EditToolSelectionModes.ToolPiece && _toolsSelection.SelectedPieces.Count() == 1)
		{
			IToolPiece oldPiece = _toolsSelection.SelectedPieces.First();
			Application.Current.Dispatcher.Invoke(delegate
			{
				ISubToolPieceViewModel subToolPieceViewModel = _factorio.Resolve<ISubToolPieceViewModel>();
				subToolPieceViewModel.Close += CloseSubToolPiece;
				subToolPieceViewModel.Init(_toolsSelection.CurrentDoc, oldPiece);
				_dockingService.Hide(_subElement);
				_subElement = subToolPieceViewModel;
				_dockingService.Show(subToolPieceViewModel);
			});
		}
	}

	private void CloseSubToolPiece(IToolPieceProfile? newPiece)
	{
		_dockingService.Hide(_subElement);
		_subElement = null;
		if (newPiece != null)
		{
			DoActionAsCalculation(delegate
			{
				AddToolPiece(newPiece);
			});
		}
	}

	private void AddToolPiece(IToolPieceProfile newProfile)
	{
		IToolPiece refPiece = _toolsSelection.SelectedPieces.First();
		_toolFactory.CreateToolPiece(newProfile, refPiece, 1);
	}

	private void StartSubToolSection(IToolCalculationOption option)
	{
		IToolSection oldSection = _toolsSelection.SelectedSections.FirstOrDefault();
		Application.Current.Dispatcher.Invoke(delegate
		{
			ISubToolSectionViewModel subToolSectionViewModel = _factorio.Resolve<ISubToolSectionViewModel>();
			subToolSectionViewModel.Close += CloseSubToolSection;
			int materialGroupForBendDeduction = _toolsSelection.CurrentDoc.Material.MaterialGroupForBendDeduction;
			double thickness = _toolsSelection.CurrentDoc.Thickness;
			if (oldSection != null)
			{
				subToolSectionViewModel.Init(Doc, oldSection, Tools, _toolsSelection.ToolsAndBends, materialGroupForBendDeduction, thickness);
			}
			else
			{
				EnsureCurrentSetup();
				subToolSectionViewModel.Init(Doc, _toolsSelection.CurrentSetups, Tools, _toolsSelection.ToolsAndBends, materialGroupForBendDeduction, thickness);
			}
			_dockingService.Hide(_subElement);
			_subElement = subToolSectionViewModel;
			_dockingService.Show(subToolSectionViewModel);
		});
	}

	private void StartSubToolSectionReplacement(IToolCalculationOption option)
	{
		Application.Current.Dispatcher.Invoke(delegate
		{
			ISubToolSectionReplaceViewModel subToolSectionReplaceViewModel = _factorio.Resolve<ISubToolSectionReplaceViewModel>();
			subToolSectionReplaceViewModel.Close += CloseSubToolSection;
			int materialGroupForBendDeduction = _toolsSelection.CurrentDoc.Material.MaterialGroupForBendDeduction;
			double thickness = _toolsSelection.CurrentDoc.Thickness;
			subToolSectionReplaceViewModel.Init(Tools, _toolsSelection.ToolsAndBends, materialGroupForBendDeduction, thickness, Doc);
			_dockingService.Hide(_subElement);
			_subElement = subToolSectionReplaceViewModel;
			_dockingService.Show(subToolSectionReplaceViewModel);
		});
	}

	private void CloseSubToolSection()
	{
		_dockingService.Hide(_subElement);
		_subElement = null;
		DoActionAsCalculation(delegate
		{
		});
	}

	private void DoActionAsCalculation(Action<IToolCalculationOption> action)
	{
		IToolCalculationOption obj = _toolCalculations.CreateDefaultOptions(Doc);
		action(obj);
		_doc.RecalculateSimulation();
		RefreshSelection();
	}

	private void StartSubAdapter(IToolCalculationOption option)
	{
		IToolSection oldSection = _toolsSelection.SelectedSections.FirstOrDefault();
		Application.Current.Dispatcher.Invoke(delegate
		{
			ISubAdapterSectionViewModel subAdapterSectionViewModel = _factorio.Resolve<ISubAdapterSectionViewModel>();
			subAdapterSectionViewModel.Close += CloseSubToolSection;
			int materialGroupForBendDeduction = _toolsSelection.CurrentDoc.Material.MaterialGroupForBendDeduction;
			double thickness = _toolsSelection.CurrentDoc.Thickness;
			if (oldSection != null)
			{
				subAdapterSectionViewModel.Init(oldSection, Tools, _toolsSelection.ToolsAndBends, materialGroupForBendDeduction, thickness);
			}
			else
			{
				EnsureCurrentSetup();
				subAdapterSectionViewModel.Init(_toolsSelection.CurrentSetups, Tools, _toolsSelection.ToolsAndBends, materialGroupForBendDeduction, thickness);
			}
			_dockingService.Hide(_subElement);
			_subElement = subAdapterSectionViewModel;
			_dockingService.Show(subAdapterSectionViewModel);
		});
	}

	private void DeleteToolSetup(IToolCalculationOption option)
	{
		IToolSetups setup = SelectedSetup;
		if (setup == null)
		{
			return;
		}
		ToolSetupsViewModel newSelected = ToolSetups.FirstOrDefault((ToolSetupsViewModel x) => x.Setup != null && !(x.Setup is ToolSetupsViewModel.ToolSetupsNewDummySetup) && x.Setup != setup);
		if (newSelected == null)
		{
			return;
		}
		_selectedSetup = null;
		ToolSetupsViewModel oldOne = ToolSetups.FirstOrDefault((ToolSetupsViewModel x) => x.Setup == setup);
		Application.Current.Dispatcher.Invoke(delegate
		{
			if (oldOne != null)
			{
				ToolSetups.Remove(oldOne);
			}
			NotifyPropertyChanged("SelectedSetup");
			SelectedSetup = newSelected?.Setup;
		});
		_toolFactory.RemoveToolSetup(_toolsSelection.ToolsAndBends, setup);
	}

	private void ExtractToolPieces(IToolCalculationOption option)
	{
		if (_toolsSelection.SelectionMode == EditToolSelectionModes.ToolPiece)
		{
			_toolFactory.SplitSections(_toolsSelection.SelectedPieces);
		}
	}

	private void MergeToolSections(IToolCalculationOption option)
	{
		switch (_toolsSelection.SelectionMode)
		{
		case EditToolSelectionModes.ToolSection:
		{
			IEnumerable<IToolSection> sections = _toolFactory.MergeSections(_toolsSelection.SelectedSections);
			_toolsSelection.UnselectAll();
			_toolsSelection.SetSelection(sections, isSelected: true);
			break;
		}
		case EditToolSelectionModes.Cluster:
		{
			IEnumerable<IToolSection> source = _toolFactory.MergeSections(_toolsSelection.SelectedClusters.SelectMany((IToolCluster x) => x.Sections));
			_toolsSelection.UnselectAll();
			_toolsSelection.SetSelection(source.Select((IToolSection x) => x.Cluster), isSelected: true);
			break;
		}
		case EditToolSelectionModes.ToolPiece:
			break;
		}
	}

	private void MergeToolSectionsInCurrentSetup(IToolCalculationOption option)
	{
		List<IToolPiece> list = _toolsSelection.SelectionAsPieces.ToList();
		_toolFactory.MergeSections(_toolsSelection.CurrentSetups.AllSections);
		switch (_toolsSelection.SelectionMode)
		{
		case EditToolSelectionModes.ToolPiece:
			_toolsSelection.UnselectAll();
			_toolsSelection.SetSelection(list, isSelected: true);
			break;
		case EditToolSelectionModes.ToolSection:
			_toolsSelection.UnselectAll();
			_toolsSelection.SetSelection(list.Select((IToolPiece x) => x.ToolSection), isSelected: true);
			break;
		case EditToolSelectionModes.Cluster:
			_toolsSelection.UnselectAll();
			_toolsSelection.SetSelection(list.Select((IToolPiece x) => x.ToolSection.Cluster), isSelected: true);
			break;
		}
	}

	private void SaveToolSetups(IToolCalculationOption option)
	{
		IToolSetups currentSetups = _toolsSelection.CurrentSetups;
		if (currentSetups != null && Tools != null)
		{
			_tempSerializedToolSetup = _toolsAndBendsSerializer.Convert(_toolsAndBendsSerializer.Convert(currentSetups));
			_toolSetupMemory.Save(option.Doc.BendMachine.MachineNo, _tempSerializedToolSetup, option.Doc.SavedFileName);
		}
	}

	private void LoadToolSetups(IToolCalculationOption option)
	{
		if (Tools == null)
		{
			return;
		}
		string text = null;
		string text2 = null;
		if (option.Doc.BendMachine != null)
		{
			IToolSetupMemory.Memory memory = _toolSetupMemory.Load(option.Doc.BendMachine.MachineNo);
			if (memory != null)
			{
				if (text == null)
				{
					text = memory.Setup;
				}
				text2 = memory.SavedName;
			}
		}
		if (text == null || Tools == null)
		{
			return;
		}
		IToolsAndBends tab = _toolsSelection.ToolsAndBends;
		IToolSetups dupplicate = _toolsAndBendsSerializer.ConvertBack(_toolsAndBendsSerializer.ConvertBack(text), Tools);
		dupplicate.Number = tab.ToolSetups.Select((IToolSetups x) => x.Number).DefaultIfEmpty().Max() + 1;
		tab.ToolSetups.Add(dupplicate);
		dupplicate.Desc = text2 ?? ("Copy of " + dupplicate.Desc);
		Application.Current.Dispatcher.Invoke(delegate
		{
			ToolSetups.Add(new ToolSetupsViewModel(dupplicate));
		});
		_toolsSelection.CurrentSetups = dupplicate;
		if (!dupplicate.HasTools() || tab.ToolSetups.Count((IToolSetups x) => x.HasTools()) != 1)
		{
			return;
		}
		AcbActivator acbActivator = new AcbActivator();
		foreach (IBendPositioning bendPosition in tab.BendPositions)
		{
			_toolCalculator.AssignBendToBestSections(bendPosition, dupplicate, Doc.BendMachine.ToolConfig.ProfileSelector, Doc, tab, option);
			acbActivator.TryActivate(bendPosition.Anchor.Root, bendPosition, _doc.UnfoldModel3D, _doc.Material);
		}
		Application.Current.Dispatcher.BeginInvoke((Action)delegate
		{
			_bendSelection.CurrentBend = tab.BendPositions.FirstOrDefault();
		});
	}

	public void CommitChanges()
	{
		Close();
	}

	public void Cancel()
	{
		Close();
	}

	public override bool Close()
	{
		IsVisible = false;
		if (!_isClosing)
		{
			_isClosing = true;
			return base.Close();
		}
		return true;
	}

	public override void KeyUp(object sender, IPnInputEventArgs e)
	{
		base.KeyUp(sender, e);
		if (_shortcutSettingsCommon.Cancel.IsShortcut(e))
		{
			Cancel();
			e.Handle();
		}
		else if (_shortcutSettingsCommon.Commit.IsShortcut(e))
		{
			CommitChanges();
			e.Handle();
		}
		else if (_shortcutEditTools.SelectionModePiece.IsShortcut(e))
		{
			IsSelectionModeToolPiece = true;
			e.Handle();
		}
		else if (_shortcutEditTools.SelectionModeSection.IsShortcut(e))
		{
			IsSelectionModeToolSection = true;
			e.Handle();
		}
		else if (_shortcutEditTools.SelectionModeGroup.IsShortcut(e))
		{
			IsSelectionModeCluster = true;
			e.Handle();
		}
		else if (_shortcutEditTools.Flip.IsShortcut(e))
		{
			CmdFlipTools.Execute(null);
			e.Handle();
		}
		else if (_shortcutEditTools.RemovePieces.IsShortcut(e))
		{
			CmdDeleteTools.Execute(null);
			e.Handle();
		}
		else if (_shortcutEditTools.MoveRight.IsShortcut(e))
		{
			CmdMoveRight.Execute(null);
			e.Handle();
		}
		else if (_shortcutEditTools.MoveLeft.IsShortcut(e))
		{
			CmdMoveLeft.Execute(null);
			e.Handle();
		}
		else if (_shortcutEditTools.CenterTools.IsShortcut(e))
		{
			CmdCenterTools.Execute(null);
			e.Handle();
		}
	}
}
