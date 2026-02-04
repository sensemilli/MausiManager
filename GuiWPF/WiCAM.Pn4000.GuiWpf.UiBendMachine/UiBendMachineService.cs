using System;
using System.Collections.Generic;
using System.Linq;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Interfaces;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.GuiWpf.PnCommand;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;
using WiCAM.Pn4000.PN3D;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Pipes;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.ScreenD3D.Controls;

namespace WiCAM.Pn4000.GuiWpf.UiBendMachine;

internal class UiBendMachineService : IUiBendMachineService
{
	private readonly IMachineHelper _machineHelper;

	private readonly IFactorio _factorio;

	private readonly IAutoMode _autoMode;

	private readonly IScreen3DMain _screen3DMain;

	private readonly IPN3DBendPipe _bendPipe;

	private readonly IMainWindowDataProvider _mainWindowDataProvider;

	private readonly IGlobals _globals;

	private readonly IPnPathService _pathService;

	private readonly ITranslator _translator;

	private readonly IMainWindowBlock _mainWindowBlock;

	private readonly IMachineBendFactory _machineBendFactory;

	private readonly IDoEvents _doEvents;

	public UiBendMachineService(IMachineHelper machineHelper, IFactorio factorio, IAutoMode autoMode, IScreen3DMain screen3DMain, IPN3DBendPipe bendPipe, IMainWindowDataProvider mainWindowDataProvider, IGlobals globals, IPnPathService pathService, ITranslator translator, IMainWindowBlock mainWindowBlock, IMachineBendFactory machineBendFactory, IDoEvents doEvents)
	{
		_machineHelper = machineHelper;
		_factorio = factorio;
		_autoMode = autoMode;
		_screen3DMain = screen3DMain;
		_bendPipe = bendPipe;
		_mainWindowDataProvider = mainWindowDataProvider;
		_globals = globals;
		_pathService = pathService;
		_translator = translator;
		_mainWindowBlock = mainWindowBlock;
		_machineBendFactory = machineBendFactory;
		_doEvents = doEvents;
	}

	public void BendMachineConfig(IPnCommandArg arg)
	{
		IDoc3d doc = arg.doc();
		if (doc.MachinePath != null)
		{
			if (!_mainWindowBlock.BlockUI_IsBlock(doc))
			{
				_mainWindowBlock.BlockUI_Block(doc);
			}
			MachineConfigurationView machineConfigurationView = _factorio.Resolve<MachineConfigurationView>();
			machineConfigurationView.Init(doc, doc.BendMachine, delegate(PopupMachineConfigViewModel sender)
			{
				_mainWindowDataProvider.SetViewForConfig(null);
				sender.Dispose();
				GC.Collect();
				GC.WaitForPendingFinalizers();
				_mainWindowBlock.BlockUI_Unblock(doc);
			});
			_mainWindowDataProvider.SetViewForConfig(machineConfigurationView);
		}
	}

	public F2exeReturnCode SelectBendMachineFunc(IPnCommandArg arg)
	{
		Screen3D screen3d = _screen3DMain.Screen3D;
		if (!_autoMode.PopupsEnabled)
		{
			return _bendPipe.SelectBendMachine(arg.CommandParam, arg.doc());
		}
		IDoc3d doc = arg.doc();
		screen3d.IgnoreMouseMove(ignore: true);
		doc.MachineFullyLoaded = false;
		List<BendMachine> machines = new List<BendMachine>();
		List<IBendMachineSummary> machineSummaries = GetMachineSummaries(doc.MessageDisplay).ToList();
		if (!_mainWindowBlock.BlockUI_IsBlock(doc))
		{
			_mainWindowBlock.BlockUI_Block(doc);
		}
		F2exeReturnCode returnCode = F2exeReturnCode.OK;
		ISelectMachineViewModel selectMachineViewModel = null;
		bool waitForView = true;
		selectMachineViewModel = _factorio.Resolve<ISelectMachineViewModel>().Init(doc, machines, machineSummaries, delegate(ISelectMachineViewModel sender)
		{
			_mainWindowDataProvider.SetViewForConfig(null);
			Dictionary<ICombinedBendDescriptorInternal, IPreferredProfile> preferedToolsForCombinedBends = sender.GetPreferedToolsForCombinedBends();
			returnCode = _bendPipe.SetBendMachine(sender.UseDefaultTools, preferedToolsForCombinedBends, doc);
			if (returnCode == F2exeReturnCode.OK)
			{
				IBendMachineSummary selectedMachine = selectMachineViewModel.GetSelectedMachine();
				_mainWindowDataProvider.AddRecentlyUsedRecord(new RecentlyUsedRecord
				{
					FileName = selectedMachine.Name,
					FullPath = selectedMachine.MachinePath,
					ArchiveID = selectedMachine.MachineNo,
					Type = "BMACH"
				});
			}
			sender.Dispose();
			GC.Collect();
			GC.WaitForPendingFinalizers();
			screen3d.IgnoreMouseMove(ignore: false);
			waitForView = false;
		});
		ISelectMachineView viewForConfig = _factorio.Resolve<ISelectMachineView>().Init(selectMachineViewModel);
		_mainWindowDataProvider.SetViewForConfig(viewForConfig);
		_mainWindowBlock.CloseWait(doc);
		while (waitForView)
		{
			_doEvents.DoEvents(20);
		}
		return returnCode;
	}

	private IEnumerable<IBendMachineSummary> GetMachineSummaries(IMessageDisplay messageDisplay)
	{
		return _machineBendFactory.GetMachineSummaries();
	}
}
