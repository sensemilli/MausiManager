using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Threading;
using WiCAM.Pn4000.Config;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.Doc3d;
using WiCAM.Pn4000.Contracts.Factorys;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiContracts.PnStatusBar;
using WiCAM.Pn4000.GuiContracts.Ribbon;
using WiCAM.Pn4000.JobManager;
using WiCAM.Pn4000.PKernelFlow.Adapters;
using WiCAM.Pn4000.PKernelFlow.WrapCommon;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Extensions;
using WiCAM.Pn4000.PN3D.Interfaces;
using WiCAM.Pn4000.PN3D.Popup.Information;
using WiCAM.Pn4000.pn4.Contracts;
using WiCAM.Pn4000.pn4.pn4FlowCenter.Plugins;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.pn4.pn4UILib;
using WiCAM.Pn4000.pn4.pn4UILib.Popup;
using WiCAM.Pn4000.pn4.pn4UILib.Ribbon;
using WiCAM.Pn4000.pn4.uicontrols.Buttons;
using WiCAM.Pn4000.Screen;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.XEvents;
using WiCAM.Services.ConfigProviders.Contracts;
using WiCAM.Services.ConfigProviders.Contracts.DataType;

namespace WiCAM.Pn4000.pn4.pn4FlowCenter;

public class ExeFlow
{
	private readonly MemoryDisk _memoryDisk = new MemoryDisk();

	private bool _pipeBusy;

	private readonly List<string> _pipeList = new List<string>();

	private WiCAM.Pn4000.pn4.pn4UILib.Popup.Popup _popup;

	private int _popupAnswer;

	private Calculator _calculator;

	private bool _isHelpCursor;

	private readonly bool _isPipeCursor;

	private MainWindow _mainWindow;

	private readonly IPKernelFlowGlobalDataService _pKernelFlowGlobalData;

	private readonly IPnDebug _pnDebug;

	private readonly ILanguageDictionary _languageDictionary;

	private readonly Pn3DKernel _pN3DKernel;

	private readonly UIFeedback _uIFeedback;

	private readonly EventsList _eventsList;

	private readonly IPnPathService _pnPathService;

	private readonly IConfigProvider _configProvider;

//	private IInternalCommand _internalCommand;

	private readonly IFactorio _factorio;

	private readonly PnRibbon _pnRibbon;

	private readonly Toolbars _toolbars;

	private readonly IPnCommandsOther _pnCommandsOther;

	private readonly IScreen2D _screen2D;

//	private readonly IScreen3DMain _screen3D;

	private readonly IModelFactory _modelFactory;

	private readonly ILogCenterService _logCenterService;

	private readonly IPnIconsService _pnIconsService;

	private readonly IRibbonMainWindowConnector _ribbonMainWindowConnector;

	private readonly ISubMenuConnector _subMenuConnector;

	private readonly IPKernelStatusBarModel _pKernelStatusBarModel;

	private readonly ICurrentDocProvider _currentDocProvider;

	private readonly IDoc3dFactory _docFactory;

//	public readonly ISetup2D3D _setup2D3D;

	private readonly IDoEvents _doEvents;

	private bool updateResourceForPopup;

	//public ISetup2D3D setup2D3D;

	private PluginsManager _pluginsManager;

	//private IHardcodePluginManager _hardcodePluginManager;

	private int _lastWidth = 100;

	private int _lastHeight = 100;

	public bool ReloadCenterMenu { get; set; }

	public IPnCommand LastPnCommand { get; set; }

	public string LastPipeFunctionName { get; set; } = string.Empty;

	public PnBrowser PnBrowser { get; set; }

	private IDoc3d CurrentDoc => _currentDocProvider.CurrentDoc;
    //IScreen3DMain screen3D,
    public ExeFlow(IPnPathService pathService, IPKernelFlowGlobalDataService pKernelFlow, ILanguageDictionary languageDictionary, 
		IConfigProvider configProvider, IFactorio factorio, PnRibbon pnRibbon, Toolbars toolbars, 
		IPnCommandsOther pnCommandsOther, ITranslator translator, IScreen2D screen2D,
		IModelFactory modelFactory, ILogCenterService logCenterService, IPnIconsService pnIconsService, 
		IRibbonMainWindowConnector ribbonMainWindowConnector, ISubMenuConnector subMenuConnector, 
		IPnDebug pnDebug, IPKernelStatusBarModel pKernelStatusBarModel, Pn3DKernel pn3DKernel,
		UIFeedback uiFeedback, EventsList eventsList,  ICurrentDocProvider currentDocProvider,
		IDoc3dFactory docFactory, IDoEvents doEvents)
	{
	//	setup2D3D = modelFactory.Resolve<ISetup2D3D>();
		_pnPathService = pathService;
		_configProvider = configProvider;
		_factorio = factorio;
		_pnRibbon = pnRibbon;
		_toolbars = toolbars;
		_pnCommandsOther = pnCommandsOther;
		_screen2D = screen2D;
		_modelFactory = modelFactory;
		_logCenterService = logCenterService;
		_pnIconsService = pnIconsService;
		_ribbonMainWindowConnector = ribbonMainWindowConnector;
		_subMenuConnector = subMenuConnector;
		_pnDebug = pnDebug;
		_pKernelStatusBarModel = pKernelStatusBarModel;
		_pKernelFlowGlobalData = pKernelFlow;
		_languageDictionary = languageDictionary;
		_pN3DKernel = pn3DKernel;
		_uIFeedback = uiFeedback;
		_eventsList = eventsList;
	//	_screen3D = screen3D;
		_currentDocProvider = currentDocProvider;
		_doEvents = doEvents;
		_memoryDisk.Initialize();
		SetXEventsConnection();
		_ribbonMainWindowConnector.OnCallPnCommand += delegate(IPnCommand command)
		{
			CallPnCommand(command);
		};
		_subMenuConnector.OnToKernelMouseWheel += ToKernelMouseWheel;
		_subMenuConnector.OnCallPnCommandSubmenuEdition += CallPnCommandSubmenuEdition;
		translator.ResourcesChangedStrong += ResourcesChangedEventReciver;
		_docFactory = docFactory;
	}

	public void Init(MainWindow mainWindow)
	{
		_mainWindow = mainWindow;
	}

	private void ResourcesChangedEventReciver(ITranslator arg1, IResourcesChangedArgs arg2)
	{
		updateResourceForPopup = true;
	}

	private void SetXEventsConnection()
	{
		_uIFeedback.ContextMenuDelegate = ContexMenu_XEventEdition;
		_uIFeedback.CenterMenuDelegate = CenterMenu_XEventEdition;
		_uIFeedback.SetLanguageDelegate = SetLanguage_XEventEdition;
		_uIFeedback.SetMachineDelegate = SetMachineDelegate_XEventEdition;
		_uIFeedback.ActiveProgramPartDelegate = ActiveProgramPart_XEventEdition;
		_uIFeedback.HelpCursorDelegate = HelpCursor_XEventEdition;
		_uIFeedback.CalculatorDelegate = Calculator_XEventEdition;
		_uIFeedback.SetWindowTextDelegate = SetWindowsText_XEventEdition;
		_uIFeedback.TextDialogDelegate = TextDialog_XEventEdition;
		_uIFeedback.SubMenuShowDelegate = SubMenuShow_XEventEdition;
		_uIFeedback.SubMenuHideDelegate = SubMenuHide_XEventEdition;
		_uIFeedback.RightMenuShowDelegate = RightMenuShow_XEventEdition;
		_uIFeedback.PopupShowDelegate = PopupShow_XWindowEdition;
		_uIFeedback.AddLastCommandDelegate = AddLastCommand_XWindowEdition;
		_uIFeedback.SetMouseGeometryInfoDelegate = SetMouseGeometryInfo_XWindowEdition;
		_uIFeedback.InfoPaneClearDelegate = InfoPaneClear_XWindowEdition;
		_uIFeedback.InfoPaneSetDelegate = InfoPaneSet_XWindowEdition;
		_uIFeedback.InfoPaneUpdateDelegate = InfoPaneUpdate_XWindowEdition;
		_uIFeedback.RibbonCommandDelegate = RibbonCommand_XWindowEdition;
		_uIFeedback.AddRecentlyUsedDelegate = AddRecentlyUsed_XWindowEdition;
		_uIFeedback.ExternalAppStartDelegate = ExternalAppStart_XWindowEdition;
		_uIFeedback.PartPaneDelegate = PartPane_XWindowEdition;
		_uIFeedback.BlockUnblockDelegate = BlockUnblock_XWindowEdition;
		_uIFeedback.PNDebugDelegate = PnDebug_XWindowEdition;
		_uIFeedback.IconSwapDelegate = IconSwap_XWindowEdition;
		_uIFeedback.PlugInDelegate = PlugIn_XWindowEdition;
		_uIFeedback.PipeDelegate = Pipe_XWindowEdition;
		_uIFeedback.LocalSet3DPartNameDelegate = LocalSet3DPartName_XWindowEdition;
		_uIFeedback.LocalSetNcFilenameDelegate = LocalSetNcFilename_XWindowEdition;
		_uIFeedback.LocalOpen3DPartDelegate = LocalOpen3DPart_XWindowEdition;
		_uIFeedback.LocalSave3DPartDelegate = LocalSave3DPart_XWindowEdition;
		_uIFeedback.LocalSave3DFileDelegate = Save3DFile;
		_uIFeedback.LocalFile3DInDelegate = LocalFile3DIn_XWindowEdition;
		_uIFeedback.LoadFile3DWithMachineDelegate = LoadFile3DWithMachine_XWindowEdition;
		_uIFeedback.LoadFile3DWithMachineMaterialDelegate = LoadFile3DWithMachineMaterial_XWindowEdition;
		_uIFeedback.LoadFile3DAndRepairDelegate = LoadFile3DAndRepairBends_XWindowEdition;
		_uIFeedback.LoadFile3D5Delegate = LoadFile3D5_XWindowEdition;
		_uIFeedback.LoadFile3D6Delegate = LoadFile3D6_XWindowEdition;
		_uIFeedback.LoadFile3D7Delegate = LoadFile3D7_XWindowEdition;
		_uIFeedback.LocalCallCommandDelegate = LocalCallCommand_XWindowEdition;
		_uIFeedback.LocalSelect3DMachine = LocalSelect3DMachine_XWindowEdition;
		_uIFeedback.LocalSimulationDelegate = LocalSimulationDelegate_XWindowEdition;
		_uIFeedback.LoadPartsXmlToF = LoadPartsXmlToF_XWindowEdition;
		_uIFeedback.LocalExportAsmAsStepeDelegate = LocalExportAsmAsStepeDelegate_XWindowEdition;
		_uIFeedback.LocalOpenPartFrom3DAssemblyDelegate = LocalOpenPartFrom3DAssembly_XWindowEdition;
		_uIFeedback.RibbonGetActiveTabDelegate = RibbonCommandDelegate_XWindowEdition;
		_uIFeedback.PrintScreenToFile = PrintScreenToFile_XWindowEdition;
		_uIFeedback.Get3dModelNameFromDoc = Get3dModelName;
		_uIFeedback.GetPnFontNumber = GetPnFontNumber_XWindowEdition;
	}

	private string Get3dModelName()
	{
		return _currentDocProvider?.CurrentDoc?.DiskFile?.Header?.ModelName;
	}

	private int Save3DFile(string path, int archiveNumber)
	{
		_logCenterService.Debug("Saving 3D file: " + path + "archivnumber   " + archiveNumber);
        return (int)(_pN3DKernel?.Pn3DRootPipe?.PN3DDocPipe?.SaveP3DFileWithLoop(path, archiveNumber, CurrentDoc)).Value;
	}

	private void PrintScreenToFile_XWindowEdition(string fileName, bool transparentBackground)
	{
		_screen2D.CaptureBitmap(fileName);
	}

	private void LoadPartsXmlToF_XWindowEdition()
	{
		_pN3DKernel.Pn3DRootPipe.LoadPartsXmlToF();
	}

	private void SetMachineDelegate_XEventEdition(int machine)
	{
		if (_pKernelFlowGlobalData.PnMachine != machine)
		{
			_pKernelFlowGlobalData.PnMachine = machine;
		//	ConfigProviderManager.UpdatePnMachine(_pnPathService, _configProvider, machine);
		}
	}

	private int LocalSimulationDelegate_XWindowEdition(int collisioncheck)
	{
		return (int)_pN3DKernel.Pn3DRootPipe.PN3DDocPipe.Simulation(collisioncheck, CurrentDoc);
	}

	public int LocalSelect3DMachine_XWindowEdition(int machineId)
	{
		return (int)_pN3DKernel.Pn3DRootPipe.PN3DBendPipe.SelectBendMachine(machineId.ToString(), _currentDocProvider.CurrentDoc);
	}

	private void LocalSet3DPartName_XWindowEdition(string param)
	{
		_pN3DKernel.Pn3DRootPipe.PN3DDocPipe.SetDocNameExternally(param, CurrentDoc);
	}

	private int LocalSetNcFilename_XWindowEdition(string param, int addTimestamp)
	{
		return (int)_pN3DKernel.Pn3DRootPipe.PN3DDocPipe.SetDocNcFilenameExternally(param, (SetNcTimestampTypes)addTimestamp, CurrentDoc);
	}

	public int LocalOpen3DPart_XWindowEdition(string param)
	{
		IDoc3d newDoc;
		return (int)_pN3DKernel.Pn3DRootPipe.PN3DDocPipe.OpenFileP3DExternal(param, out newDoc);
	}

	private void LocalExportAsmAsStepeDelegate_XWindowEdition(string folder)
	{
		_pN3DKernel.Pn3DRootPipe.PN3DDocPipe.ExportAsmAsStepExternal(folder);
	}

	private void LocalSave3DPart_XWindowEdition(string param)
	{
		_pN3DKernel.Pn3DRootPipe.PN3DDocPipe.SaveDocP3D(param, CurrentDoc);
	}

	private int LocalFile3DIn_XWindowEdition(int viewer, string param)
	{
		return (int)_pnCommandsOther.ImportUniversal(CreateArg("LocalFile3DIn_XWindowEdition"), new ImportArg
		{
			Filename = param,
			CheckLicense = false,
			MoveToCenter = true,
			UseHd = (viewer != 1)
		});
	}

	private IPnCommandArg CreateArg([CallerMemberName] string CommandDesc = null)
	{
		return new PnCommandArg(null, null)
		{
			CommandGroup = -1,
			CommandStr = CommandDesc
		};
	}

	public int LoadFile3DWithMachine_XWindowEdition(int machineNum, string filename)
	{
		return (int)_pnCommandsOther.ImportUniversal(CreateArg("LoadFile3DWithMachine_XWindowEdition"), new ImportArg
		{
			Filename = filename,
			CheckLicense = false,
			MoveToCenter = true,
			UseHd = true,
			MachineNumber = machineNum
		});
	}

	private int LoadFile3DWithMachineMaterial_XWindowEdition(string file, int machineNo, int materialNo, int specialPartSelectionMode)
	{
		return (int)_pnCommandsOther.ImportUniversal(CreateArg("LoadFile3DWithMachineMaterial_XWindowEdition"), new ImportArg
		{
			Filename = file,
			CheckLicense = false,
			MoveToCenter = true,
			UseHd = true,
			MachineNumber = machineNo,
			MaterialNumber = materialNo,
			SpecialPartSelectionMode = (IImportArg.SpecialPartSelectionModes)specialPartSelectionMode
		});
	}

	private int LoadFile3DAndRepairBends_XWindowEdition(string file, int machineNo, int materialNo, int specialPartSelectionMode, int repairIrregularBends)
	{
		return (int)_pnCommandsOther.ImportUniversal(CreateArg("LoadFile3DAndRepairBends_XWindowEdition"), new ImportArg
		{
			Filename = file,
			CheckLicense = false,
			MoveToCenter = true,
			UseHd = true,
			MachineNumber = machineNo,
			MaterialNumber = materialNo,
			ReconstructBends = (IImportArg.ReconstructBendsMode)repairIrregularBends,
			SpecialPartSelectionMode = (IImportArg.SpecialPartSelectionModes)specialPartSelectionMode
		});
	}

	private int LoadFile3D5_XWindowEdition(string file, int machineNo, int materialNo, bool matByForce, int specialPartSelectionMode, int repairIrregularBends)
	{
		return (int)_pnCommandsOther.ImportUniversal(CreateArg("LoadFile3D5_XWindowEdition"), new ImportArg
		{
			Filename = file,
			CheckLicense = false,
			MoveToCenter = true,
			UseHd = true,
			MachineNumber = machineNo,
			MaterialNumber = materialNo,
			MaterialByForce = matByForce,
			ReconstructBends = (IImportArg.ReconstructBendsMode)repairIrregularBends,
			SpecialPartSelectionMode = (IImportArg.SpecialPartSelectionModes)specialPartSelectionMode
		});
	}

	private int LoadFile3D6_XWindowEdition(string file, int machineNo, int materialNo, bool matByForce, int specialPartSelectionMode, int repairIrregularBends, bool emptyIfFail, bool closeOldDocs)
	{
		try
		{
			_logCenterService.Debug($"Loading 3D file: {file} (Machine:{machineNo}, Material:{materialNo})");

			IDoc3d currentDoc = _currentDocProvider.CurrentDoc;
			int result = (int)_pnCommandsOther.ImportUniversal(CreateArg("LoadFile3D6_XWindowEdition"), new ImportArg
			{
				Filename = file,
				CheckLicense = false,
				MoveToCenter = true,
				UseHd = true,
				MachineNumber = machineNo,
				MaterialNumber = materialNo,
				MaterialByForce = matByForce,
				CloseAllDocs = closeOldDocs,
				ReconstructBends = (IImportArg.ReconstructBendsMode)repairIrregularBends,
				SpecialPartSelectionMode = (IImportArg.SpecialPartSelectionModes)specialPartSelectionMode
			});
			_logCenterService.Debug($"Import result: {result}");

			if (emptyIfFail && _currentDocProvider.CurrentDoc == currentDoc)
			{
				_logCenterService.Debug("Import failed, creating empty document");

				try
				{
					IDoc3d doc3d = _docFactory.CreateDoc(file);
					doc3d.DiskFile.Header.ImportedFilename = file;
					doc3d.DiskFile.Header.ModelName = new FileInfo(file).Name;
					_currentDocProvider.CurrentDoc = doc3d;
					try
					{
						doc3d.DiskFile.Header.ModelName = new FileInfo(file).Name;
					}
					catch (Exception)
					{
						_logCenterService.Debug($"Failed to delete temp file ");

					}
					_currentDocProvider.CurrentDoc = doc3d;
					string[] files = Directory.GetFiles(_pnPathService.FolderCad3d2Pn);
					foreach (string path in files)
					{
						try
						{
							File.Delete(path);
						}
						catch (Exception)
						{
						}
					}
				}
				catch (Exception ex)
				{
					_logCenterService.Debug($"Failed to create empty document: {ex.Message}");
				}
			}
				return result;
		 }
    catch (Exception ex)
    {
        _logCenterService.Debug($"LoadFile3D6 failed: {ex.Message}");
        _logCenterService.CatchRaport(ex);
        throw;
    }
		}
		

	private int LoadFile3D7_XWindowEdition(string file, int machineNo, int materialNo, bool matByForce, int specialPartSelectionMode, int repairIrregularBends, bool emptyIfFail, bool closeOldDocs, bool oppositeViewingSide)
	{
		_ = _currentDocProvider.CurrentDoc;
		int result = (int)_pnCommandsOther.ImportUniversal(CreateArg("LoadFile3D7_XWindowEdition"), new ImportArg
		{
			Filename = file,
			CheckLicense = false,
			MoveToCenter = true,
			UseHd = true,
			MachineNumber = machineNo,
			MaterialNumber = materialNo,
			MaterialByForce = matByForce,
			CloseAllDocs = closeOldDocs,
			ReconstructBends = (IImportArg.ReconstructBendsMode)repairIrregularBends,
			SpecialPartSelectionMode = (IImportArg.SpecialPartSelectionModes)specialPartSelectionMode,
			UseOppositeViewingSide = oppositeViewingSide
		});
		if (emptyIfFail && _currentDocProvider.CurrentDoc == _factorio.Resolve<IDoc3dFactory>().EmptyDoc)
		{
			IDoc3d doc3d = _docFactory.CreateDoc(file);
			doc3d.DiskFile.Header.ImportedFilename = file;
			try
			{
				doc3d.DiskFile.Header.ModelName = new FileInfo(file).Name;
			}
			catch (Exception)
			{
			}
			_currentDocProvider.CurrentDoc = doc3d;
			string[] files = Directory.GetFiles(_pnPathService.FolderCad3d2Pn);
			foreach (string path in files)
			{
				try
				{
					File.Delete(path);
				}
				catch (Exception)
				{
				}
			}
		}
		return result;
	}

	public int LocalCallCommand_XWindowEdition(int p1, string p2)
	{
        /*	if (_internalCommand == null)
            {
                _internalCommand = _modelFactory.Resolve<IInternalCommand>();
            }
            return _internalCommand.Call(new PnCommand(p1, p2));
        */
        return 0;
    }

	private void LocalOpenPartFrom3DAssembly_XWindowEdition(int id)
	{
		//_mainWindow?.Ribbon_Activate3DTab();
		_pnCommandsOther.Disassembly(CreateArg("LocalOpenPartFrom3DAssembly_XWindowEdition"), id);
	}

	private void Pipe_XWindowEdition(int type, int group, string name)
	{
	//	if (!_mainWindow.MultitaskingManager.IsInit)
	//	{
		//	return;
	//	}
		switch (type)
		{
		case 1:
			_pKernelFlowGlobalData.LastPipeFunctionName = name;
			_pipeList.Add($"{group} {name}");
			UpdatePipeListStatus();
			break;
		case 0:
		{
			int num = _pipeList.IndexOf($"{group} {name}");
			if (num >= 0)
			{
				_pipeList.RemoveAt(num);
			}
			UpdatePipeListStatus();
			break;
		}
		}
	}

	private void PlugIn_XWindowEdition(string pluginName, string pluginFunction)
	{
	//	if (_hardcodePluginManager == null)
	//	{
	//		_hardcodePluginManager = _modelFactory.Resolve<IHardcodePluginManager>();
	//	}
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		if (_pluginsManager == null)
		{
			_pluginsManager = new PluginsManager(_logCenterService, _memoryDisk, generalUserSettingsConfig.UseOnlineHelp);
		}
	//	if (!_hardcodePluginManager.CheckAndExecuteHardcodePlugin(pluginName, pluginFunction, _memoryDisk))
	//	{
		///	_pluginsManager.ModifyHelpLocation(generalUserSettingsConfig.UseOnlineHelp);
		//	_pluginsManager.OnPlugIn(pluginName, pluginFunction, _memoryDisk);
	//	}
	}

	private void IconSwap_XWindowEdition(string key, string newicon)
	{
		_pnIconsService.SwapIcons(key, newicon);
		_pnRibbon.SwapIcons(key, newicon);
	}

	private void PnDebug_XWindowEdition(int code, string text)
	{
		switch (code)
		{
		case 0:
			_pN3DKernel.logCenterService.Debug(text);
			break;
		case 1:
			_pnDebug.ShowPlus();
			break;
		case 2:
			_pnDebug?.HidePlus();
			break;
		case 3:
			_pnDebug.Clear();
			break;
		}
	}

	private void BlockUnblock_XWindowEdition(string code)
	{
		//_mainWindow.BlockUnblockItems.Command(code);
	}

	private int PartPane_XWindowEdition(string param)
	{
		if (param == "PARTPANE_UPDATEBEGIN")
		{
			User32Wrap.LockWindowUpdate(_mainWindow.Handle);
			return 0;
		}
		if (param == "PARTPANE_UPDATEEND")
		{
			_mainWindow.Dispatcher.Invoke(delegate
			{
				User32Wrap.LockWindowUpdate(IntPtr.Zero);
			}, DispatcherPriority.SystemIdle);
			return 0;
		}
		return 0;//Convert.ToInt32(_mainWindow.PartPanePanel.OnPartPane(param));
	}

	private int ExternalAppStart_XWindowEdition(string path)
	{
		if (path.Contains("dxf2pn") || path.Contains("dwg2pn"))
		{
			if (path.Trim().EndsWith("dialog"))
			{
				_factorio.Resolve<IAcad2PNWpf>().ConfigDialog();
			}
			else
			{
				_factorio.Resolve<IAcad2PNLauncher>().CommandLineStart(path);
			}
			return 1;
		}
		if (path.Contains("dstvconf"))
		{
			_factorio.Resolve<IDstvConfigLauncher>().Launch();
			return 1;
		}
		if (path.Contains("pnfilebr"))
		{
			InternalPnBrowser(path);
			return 1;
		}
		_doEvents.DoEvents();
		PnExternalCall.Start(path, _logCenterService);
		return 1;
	}

	private void AddRecentlyUsed_XWindowEdition(int archive, string partName, string path, string type)
	{
		//_mainWindow.PnRecentlyUsedMaster.AddRecentlyUsedRecord(archive, partName, path, type);
	}

	private void RibbonCommand_XWindowEdition(int type, int id)
	{
		if (type < 100)
		{
			_pnRibbon.ExternalAppearanceModification(type, id);
		}
		if (type == 100)
		{
			_toolbars.IsSpecjalRibbontab = true;
			_pnRibbon.OnRibbonSubTabShow(id);
		}
		if (type == 101)
		{
			_toolbars.IsSpecjalRibbontab = false;
			_pnRibbon.OnRibbonSubTabHide();
		}
	}

	private int RibbonCommandDelegate_XWindowEdition()
	{
		return _pnRibbon.GetActiveTabId();
	}

	private void InfoPaneUpdate_XWindowEdition()
	{
		if (!_configProvider.InjectOrCreate<GeneralUserSettingsConfig>().InformationPanels)
		{
			return;
		}
		_pKernelStatusBarModel.InfoPaneUpdate();
		try
		{
			if (File.Exists("PNSTATUS"))
			{
				using (StreamWriter streamWriter = File.AppendText("PNSTATUS"))
				{
					streamWriter.WriteLine("INFOPANE_UPDATE");
					return;
				}
			}
		}
		catch (Exception e)
		{
			_logCenterService.CatchRaport(e);
		}
	}

	private void InfoPaneSet_XWindowEdition(int id1, int id2, string text)
	{
		if (_configProvider.InjectOrCreate<GeneralUserSettingsConfig>().InformationPanels)
		{
			string text2 = $"INFOPANE_SET {id1} {id2} {text}";
			_pKernelStatusBarModel.InfoPaneSet(id1, id2, text);
			if (text2.Substring(0, 16) == "INFOPANE_SET 1 0")
			{
				SetScreenToolTip(text2.Substring(13));
			}
		}
	}

	private void InfoPaneClear_XWindowEdition(int id1, int id2)
	{
		if (_configProvider.InjectOrCreate<GeneralUserSettingsConfig>().InformationPanels)
		{
			SetScreenToolTip(string.Empty);
			_pKernelStatusBarModel.InfoPaneClear(id1, id2);
		}
	}

	private void SetMouseGeometryInfo_XWindowEdition(double x, double y)
	{
		if (_configProvider.InjectOrCreate<GeneralUserSettingsConfig>().GadgetCoordinates)
		{
			//_mainWindow.CoordinatesWindow?.Set(string.Format(CultureInfo.InvariantCulture, "{0:0.000} {1:0.000}", x, y));
		}
	}

	private void AddLastCommand_XWindowEdition()
	{
		_toolbars.AddLastButton(LastPnCommand);
	}

	private void RightMenuShow_XEventEdition(int refno, int count)
	{
		SubMenuHide_XEventEdition();
		_toolbars.SetRightMenu(MFileExpert.CreateByPfs842(refno, count));
	}

	private void SubMenuHide_XEventEdition()
	{
		_pnRibbon.OnSubMenuHide();
	//	_mainWindow.HideStartWindow();
		_subMenuConnector.Hide();
	}

	private void SubMenuShow_XEventEdition(int refno, int count)
	{
		MFileExpert mFileExpert = MFileExpert.CreateByPfs842(refno, count);
		_pnRibbon.OnSubMenuShow(mFileExpert);
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		if (generalUserSettingsConfig.SumMenuWindowMode == 2 || (generalUserSettingsConfig.SumMenuWindowMode == 1 && _pnRibbon.PulldownMode))
		{
			_subMenuConnector.SetSubMenu(mFileExpert);
			_subMenuConnector.ShowWithLocationCheck();
		}
	}

	private void ContexMenu_XEventEdition(int x, int y, MFileExpert mfile)
	{
		if (!_screen2D.isWF())
		{
			MessageBox.Show("Context menu for screen is implemented only for WF screen");
			return;
		}
	//	ScreenContextMenuWFStyle screenContextMenuWFStyle = new ScreenContextMenuWFStyle(mfile, _mainWindow);
		//_screen2D.ShowScreenContextMenu(screenContextMenuWFStyle.GetContextMenu(), x, y);
	}

	public string TextDialog_XEventEdition(string label, string text)
	{
		ITextInput textInput = _modelFactory.Resolve<ITextInput>().Init(text, label);
		//textInput.Owner(_mainWindow);
		textInput.Show();
		textInput.StartLikeModalMode();
		return textInput.Result;
	}

	private void SetWindowsText_XEventEdition(string text)
	{
		if (text.Trim() == string.Empty)
		{
			text = "Sense3D";
		}
	//	_mainWindow.SetApplicationTitleNameByKernelCall(text);
		if (_configProvider.InjectOrCreate<GeneralUserSettingsConfig>().InitialMultiTask)
		{
	//		_mainWindow.MultitaskingManager.SetTitle(_mainWindow.Title);
		}
	}

	private float Calculator_XEventEdition(string text, float value)
	{
		if (_calculator == null)
		{
			_calculator = _modelFactory.Resolve<Calculator>();
			_calculator.Owner = _mainWindow;
		}
		_calculator.Init(value.ToString(CultureInfo.InvariantCulture), text);
		return (float)Convert.ToDouble(_calculator.CalcRetValue.Replace(',', '.'), CultureInfo.InvariantCulture);
	}

	private void HelpCursor_XEventEdition(int mode)
	{
		_isHelpCursor = mode != 0;
		SetCursor();
	}

	private void ActiveProgramPart_XEventEdition(int part)
	{
		if (_languageDictionary.GetViewer() == 1)
		{
			part = 3;
		}
	//	_mainWindow.ActivateProgramPart.ActivateByPKernelSetActiveProgramPart(part);
		_modelFactory.Resolve<QuickLunchManager>().ResetSettings();
	}

	private void SetLanguage_XEventEdition(int language)
	{
		bool num = _languageDictionary.ChangeActiveLanguage();
	//	_mainWindow.StyleManager.Update();
		if (num)
		{
			_toolbars.ChangeLanguage();
			_pnRibbon.ChangeLanguage();
			_ribbonMainWindowConnector.ChangeLanguage();
			_modelFactory.Resolve<QuickLunchManager>().ResetSettings();
			_mainWindow.Language = XmlLanguage.GetLanguage(Thread.CurrentThread.CurrentUICulture.IetfLanguageTag);
		}
	}

	private void CenterMenu_XEventEdition(int x, int y, string filename)
	{
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
	//	_mainWindow.CenterMenuHelper.OnCenterMenuShow(x, y, filename, ReloadCenterMenu);
		ReloadCenterMenu = false;
	}

	public void Start()
	{
		SetConfig();
		WINDOW.window_set_iconsl(0);
		//GeneralSystemComponentsAdapter.StartPKernel();
	//	Application.Current.Shutdown();
	}

	internal void SetConfig()
	{
		Console.WriteLine("tab3d?   " + _mainWindow.MainViewModel.IsTab3D);
		GeneralUserSettingsConfig generalUserSettingsConfig = _configProvider.InjectOrCreate<GeneralUserSettingsConfig>();
		if (_mainWindow.MainViewModel.IsTab3D)
		{
			CfgColor color = generalUserSettingsConfig.BackgroundColor1;
			CfgColor color2 = generalUserSettingsConfig.BackgroundColor2;
			bool flag = generalUserSettingsConfig.PnOneColor;
			if (generalUserSettingsConfig.SpecialBackgroundColorFor3D)
			{
				color = generalUserSettingsConfig.BackgroundColor1_3d;
				color2 = generalUserSettingsConfig.BackgroundColor2_3d;
				flag = generalUserSettingsConfig.PnOneColor_3d;
			}
			if (flag)
			{
		//		_screen3D.Screen3D.SetBackground(color.ToWpfColor(), color2.ToWpfColor());
			}
			else
			{
		//		_screen3D.Screen3D.SetBackground(color.ToWpfColor());
			}
		}
		else
		{
			_screen2D.SetUserCfg(generalUserSettingsConfig.PnOneColor, generalUserSettingsConfig.BackgroundColor1.R, generalUserSettingsConfig.BackgroundColor1.G, generalUserSettingsConfig.BackgroundColor1.B, generalUserSettingsConfig.BackgroundColor2.R, generalUserSettingsConfig.BackgroundColor2.G, generalUserSettingsConfig.BackgroundColor2.B, generalUserSettingsConfig.MonospaceFont, generalUserSettingsConfig.PnDrawThick, generalUserSettingsConfig.PnGeoFontIndex);
			_eventsList.ConfigureNotify();
		}
	}

	private int GetPnFontNumber_XWindowEdition()
	{
		return _configProvider.InjectOrCreate<GeneralUserSettingsConfig>().PnGeoFontIndex + 1;
	}

	internal void CallPnCommand(SimplifiedPnCommand pncommand)
	{
		CallPnCommand(new PnCommand(pncommand.Group, pncommand.Command));
	}

	public int CallPnCommand(IPnCommand command)
	{
		try
		{
			_logCenterService.Debug($"Executing command - Group: {command.Group}, Command: {command.Command}");
			if (command.Command == "PNENDE")
			{
			//	MainWindow._MainWin.RBFBCloseButton_Click();
				return 0;
			}
            if (_mainWindow.Is2DAlternativeScreen())
			{
				_logCenterService.Debug("Command ignored - 2D alternative screen active");
				return 0;
			}
		/*	if (_mainWindow.LikeModalMode.IsMode() && _popup != null && _popup.IsVisible)
			{

				_mainWindow.LikeModalMode.SetMode(_mainWindow.LikeModalMode.OnEndLikeModalMode);
				_popup.IsEnabled = false;
				_popup.ExitCode = 0;
				_popup.Close();
                _logCenterService.Debug("Command ignored - Modal popup active");
				return 0;
			}*/
			if (command.Group == 25 && command.AddValue2 > 0)
			{
				_eventsList.Submenu(command.AddValue2);
				return 0;
			}
		//	if (_mainWindow.CenterMenuHelper.AddCommandIfVisible(command))
			//{
				//return 0;
//			}
			LastPnCommand = command;
			_logCenterService.Debug($"Last command updated to: {command.Group}:{command.Command}");
			if (command.Group == 81)
			{
                MainWindow._MainWin.ExeFlow.LocalCallCommand_XWindowEdition(81, command.Command);
                _logCenterService.Debug("Internal command handler initialized");

            }
            if (command.Group == 20)
            {
                MainWindow._MainWin.ExeFlow.LocalCallCommand_XWindowEdition(81, command.Command);
                _logCenterService.Debug("Internal command handler initialized");

            }
            if (command.Group == 91)
			{
			/*	if (_internalCommand == null)
				{
					_internalCommand = _modelFactory.Resolve<IInternalCommand>();
					_logCenterService.Debug("Internal command handler initialized");
				}
				int result = _internalCommand.Call(command);
			
				_logCenterService.Debug($"Internal command result: {result}");

				if (command.Command.Length > 4 && command.Command.Substring(0, 4) == "V3D_")
				{
					return result;
				}
			*/
				_toolbars.AddLastButton(LastPnCommand);
				return 0;
			}
         /*   if (command.Group == 1)
            {
                MainWindow._MainWin.ExeFlow.LocalCallCommand_XWindowEdition(1, command.Command);
                _logCenterService.Debug("Internal command handler initialized");
                return 0;
           }
		 */
            if (command.Group == 1 && command.Command == "HELP02")
			{
				_modelFactory.Resolve<QuickLunchManager>().ResetSettings();
                MainWindow._MainWin.ExeFlow.LocalCallCommand_XWindowEdition(1, command.Command);
                _logCenterService.Debug("Internal command handler initialized");
                return 0;
			}
			if (command.Group != 85 && command.Group != 81 && command.Group != 80 && _pnRibbon.Is3Dscreen && command.Command != "PNENDE")
			{
				_pnRibbon.ActivateFirstVisibleTab();
			}
			_eventsList.Command(command.Group, command.Command);
			return 0;
		}

		catch (Exception ex)
		{
			_logCenterService.CatchRaport(ex);
			throw;
		}
	}

	public void HelpForFunction(string command)
	{
	//	_mainWindow.MainViewModel.DeactivateHelpMode();
		try
		{
			_memoryDisk.Clear();
			string item = $"{command}|{_languageDictionary.CurrentLanguage}";
			_memoryDisk.Add(item);
			PlugIn_XWindowEdition("database", "HelpFindHtmlpage");
            if (_memoryDisk.Count == 1 && _memoryDisk[0] != string.Empty)
            {
                string path = string.Format(
                    "{0}\\u\\pn\\bin\\pnhelp {2} {1} \"\" \"\" {3} {2}",
                    _pnPathService.PNDRIVE,
                    _languageDictionary.CurrentLanguage,
                    command,
                    _memoryDisk[0].Replace('/', '\\')
                );
                ExternalAppStart_XWindowEdition(path);
            }
        }
		catch (Exception e)
		{
			_logCenterService.CatchRaport(e);
		}
	}

	internal void CallRecenlty(List<string> memoryDisk)
	{
		string text = memoryDisk[3];
		switch (text)
		{
		case "1":
			if (!memoryDisk[1].EndsWith(".prt.1"))
			{
				goto default;
			}
			goto case "PN3D";
		default:
			if (!(text != "DXF") || !(text != "DWG") || !Import3DTypes.GetFormatInfo("test." + text).HasValue)
			{
				break;
			}
			goto case "PN3D";
		case "PN3D":
		case "STL":
		case "SMX":
			_factorio.Resolve<IPnCommandsOther>().ImportUniversal(CreateArg("CallRecenlty"), new ImportArg
			{
				Filename = memoryDisk[2].Trim() + memoryDisk[1],
				CheckLicense = false,
				MoveToCenter = true,
				UseHd = (_pnRibbon.GetActiveTabId() != 897)
			});
			_pN3DKernel.Pn3DRootPipe.PN3DDocPipe.Activate3DTab();
			return;
		}
		if (text == "C3MO" || text == "C3DO")
		{
			_factorio.Resolve<IPnCommandsOther>().ImportUniversal(CreateArg("CallRecenlty"), new ImportArg
			{
				Filename = memoryDisk[2].Trim() + memoryDisk[1],
				UseHd = (_pnRibbon.GetActiveTabId() != 897)
			});
		}
		else
		{
			_memoryDisk.CopyStringListToMemoryDisk(memoryDisk);
			_eventsList.Command(5, "RECENTLY");
		}
	}

	public void OnDropAction(int x, int y, int flag)
	{
		_eventsList.Drop(x, y, flag);
	}

	private void CallPnCommandSubmenuEdition(IPnCommand command)
	{
		_eventsList.Submenu(command.Group);
	}

	public void KernelScreenSize(int width, int height)
	{
		_lastWidth = width;
		_lastHeight = height;
		if (!_mainWindow.MainViewModel.IsTab3D)
		{
			_eventsList.Size(width, height);
		}
	}

	public void KernelScreenSizeResend()
	{
		_eventsList.Size(_lastWidth, _lastHeight);
	}

	public void KernelScreenMouseMove(int x, int y)
	{
		if (!_mainWindow.MainViewModel.IsTab3D)
		{
			_eventsList.MouseMove(x, y);
		}
	}

	public void KernelScreenMouseDown(int x, int y, int button)
	{
		if (!_mainWindow.MainViewModel.IsTab3D)
		{
			_eventsList.MouseDown(button, x, y);
		}
	}

	public void KernelScreenMouseUp(int x, int y, int button)
	{
		if (!_mainWindow.MainViewModel.IsTab3D)
		{
			_eventsList.MouseUp(button, x, y);
		}
	}

	public void SupportShortCutKey(int p)
	{
		//_mainWindow.Shortcuts.Call(p);
	}

	public void ToKernelKeyDown(string val)
	{
		_eventsList.KeyDown(val);
	}

	public void ToKernelMouseWheel(int delta)
	{
		if (!_mainWindow.MainViewModel.IsTab3D && !_mainWindow.Is2DAlternativeScreen())
		{
			_eventsList.Wheel(delta);
		}
	}

	private void SetScreenToolTip(string txt)
	{
	}

	private void SetCursor()
	{
		if (!_isHelpCursor && !_isPipeCursor)
		{
			_screen2D.SetCursor(Screen2DCusror.Cross);
			_mainWindow.Cursor = Cursors.Arrow;
		}
		else if (_isPipeCursor)
		{
			_screen2D.SetCursor(Screen2DCusror.Wait);
			_mainWindow.Cursor = Cursors.Wait;
		}
		else
		{
			_screen2D.SetCursor(Screen2DCusror.Help);
			_mainWindow.Cursor = Cursors.Help;
		}
	}

	private void UpdatePipeListStatus()
	{
try
    {
		bool flag = _pipeList.Count > 0;
		if (flag != _pipeBusy)
		{
			_pipeBusy = flag;
			if (_pipeBusy)
			{
			///	_mainWindow.MultitaskingManager.SetStatus(MultiTaskStatus.Busy, _pipeList[_pipeList.Count - 1]);
                string currentPipe = _pipeList[_pipeList.Count - 1];
				_logCenterService.Debug($"Pipe busy: {currentPipe}");

			}
			else
			{
				                _logCenterService.Debug("Pipe free");

			//	_mainWindow.MultitaskingManager.SetStatus(MultiTaskStatus.Free, string.Empty);
			}
		}
 }
    catch (Exception ex)
    {
        _logCenterService.Debug($"UpdatePipeListStatus failed: {ex.Message}");
        _logCenterService.CatchRaport(ex);
    }
	}

	public void InternalPnBrowser(string p)
	{
		if (PnBrowser == null)
		{
			PnBrowser pnBrowser2 = (PnBrowser = new PnBrowser(_logCenterService));
		}
		PnBrowser.Action(p, _mainWindow);
	}

	private void OnPopupAnswer(int exitcode)
	{
		_popupAnswer = exitcode;
	}

	private int PopupShow_XWindowEdition()
	{
		User32Wrap.PumpMesseges();
	//	MultiTaskStatus status = _mainWindow.MultitaskingManager.SetStatus(MultiTaskStatus.NeedInteration);
		if (_popup == null || updateResourceForPopup)
		{
			_popup = _factorio.Resolve<WiCAM.Pn4000.pn4.pn4UILib.Popup.Popup>();
			_popup.Init(OnPopupAnswer, "pn3.poppos");
			updateResourceForPopup = false;
		}
		GC.Collect();
		GC.WaitForPendingFinalizers();
		GC.Collect();
		_popup.Setup();
		while (_mainWindow.Visibility != 0)
		{
			Thread.Sleep(100);
			User32Wrap.PumpMesseges();
		}
		if (_popup.ListMode)
		{
			User32Wrap.PumpMesseges();
		}
		if (_configProvider.InjectOrCreate<GeneralUserSettingsConfig>().PopupCloseOutsidePopup)
		{
			_popup.StartLikeModalMode();
		}
		else
		{
			_popup.ShowDialog();
		}
		Keyboard.Focus(_mainWindow);
		//_mainWindow.MultitaskingManager.SetStatus(status);
		return _popupAnswer;
	}

	public string Translate(string msgKey, params object[] parameters)
	{
		throw new NotImplementedException();
	}

	public string TranslateFallback(string msgKey, string fallbackResult, params object[] parameters)
	{
		throw new NotImplementedException();
	}
}
