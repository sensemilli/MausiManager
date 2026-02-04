using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using WiCAM.Pn4000.BendModel.Writer.Writer;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendOrderCaclulation;
using WiCAM.Pn4000.Contracts.BendSimulation;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.Doc3d;
using WiCAM.Pn4000.Contracts.FlangeLengthValidator;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.ToolCalculation;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.GuiContracts.Ribbon;
using WiCAM.Pn4000.GuiWpf.TabBend.PPaperSettings;
using WiCAM.Pn4000.GuiWpf.TabBend.PpRenameProgramNumber;
using WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;
using WiCAM.Pn4000.GuiWpf.UiBendMachine;
using WiCAM.Pn4000.PN3D.BendSimulation.PP;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Pipes;
using WiCAM.Pn4000.PN3D.Popup.Information;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ToolCalculationMediator;

namespace WiCAM.Pn4000.GuiWpf.PnCommand;

public class PnCommandsBend(IPnCommandBasics _cmd, IMainWindowDataProvider _mainWindowDataProvider, PN3DBendPipe _bendPipe,
	MainWindowViewModel _mainWindowViewModel, IFactorio _factorio, IAutoMode _autoMode, ITranslator _translator, IPpManager _ppManager, 
	IRibbon _ribbon, IScreen3DMain _screen3D, IUiBendMachineService _uiBendMachineService, IToolCalculations _toolCalculations,
	IMainWindowBlock _windowBlock, IDoEvents _doEvents, IBendOrderCalculator _bendOrderCalculator) : IPnCommandsBend
{
	private IScreen3D Screen3D => _screen3D.Screen3D;

	public F2exeReturnCode SimulationPlay(IPnCommandArg arg)
	{
		return _cmd.DoCmd(arg, (IPnCommandArg arg) => PnBndDocBendHandler.SimulationPlay(arg.doc()), showWaitCursor: false, blockRibbon: false, mainThread: true, "SimulationPlay", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public F2exeReturnCode SimulationPause(IPnCommandArg arg)
	{
		return _cmd.DoCmd(arg, (IPnCommandArg arg) => PnBndDocBendHandler.SimulationPause(arg.doc()), showWaitCursor: false, blockRibbon: false, mainThread: true, "SimulationPause", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public F2exeReturnCode SimulationNext(IPnCommandArg arg)
	{
		return _cmd.DoCmd(arg, (IPnCommandArg arg) => PnBndDocBendHandler.SimulationNext(arg.doc()), showWaitCursor: false, blockRibbon: false, mainThread: true, "SimulationNext", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public F2exeReturnCode SimulationEnd(IPnCommandArg arg)
	{
		return _cmd.DoCmd(arg, (IPnCommandArg arg) => PnBndDocBendHandler.SimulationEnd(arg.doc()), showWaitCursor: false, blockRibbon: false, mainThread: true, "SimulationEnd", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public F2exeReturnCode SimulationPrevious(IPnCommandArg arg)
	{
		return _cmd.DoCmd(arg, (IPnCommandArg arg) => PnBndDocBendHandler.SimulationPrevious(arg.doc()), showWaitCursor: false, blockRibbon: false, mainThread: true, "SimulationPrevious", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public F2exeReturnCode SimulationStart(IPnCommandArg arg)
	{
		return _cmd.DoCmd(arg, (IPnCommandArg arg) => PnBndDocBendHandler.SimulationStart(arg.doc()), showWaitCursor: false, blockRibbon: false, mainThread: true, "SimulationStart", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public F2exeReturnCode SelectBendMachine(IPnCommandArg arg)
	{
		return _cmd.DoCmd(arg, (Func<IPnCommandArg, F2exeReturnCode>)_uiBendMachineService.SelectBendMachineFunc, showWaitCursor: false, blockRibbon: false, mainThread: true, "SelectBendMachine", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public F2exeReturnCode SetBendMachine(IPnCommandArg arg)
	{
		return _cmd.DoCmd(arg, (IPnCommandArg arg) => _bendPipe.SetBendMachine(usePreferredTools: true, null, arg.doc()), showWaitCursor: false, blockRibbon: false, mainThread: true, "SetBendMachine", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public F2exeReturnCode SetTools(IPnCommandArg arg, int optionId)
	{
		if (optionId == -2)
		{
			if (_autoMode.PopupsEnabled)
			{
				_ribbon.Ribbon_SetActiveTab(898);
				_mainWindowViewModel.BendViewModel.ToolCalcEditProfile();
			}
			return F2exeReturnCode.OK;
		}
		IToolCalculationOption option = _toolCalculations.CreateDefaultOptions(arg.Doc, optionId);
		arg.CalculationArg = option?.CalculationArg;
		return _cmd.DoCmd(arg, (IPnCommandArg arg) => _toolCalculations.SetToolsDefault(arg.Doc, option), showWaitCursor: true, blockRibbon: true, mainThread: false, "SetTools", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public void ValidateFlangeLength(IPnCommandArg arg)
	{
		IFlangeLengthValidator flangeLengthValidator = _factorio.Resolve<IFlangeLengthValidator>();
		arg.CalculationArg = new CalculationArg(canCancel: true);
		_cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			flangeLengthValidator.Validate(arg.Doc, arg.CalculationArg);
		}, showWaitCursor: true, blockRibbon: false, mainThread: false, "ValidateFlangeLength", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public void CalculateBendOrder(IPnCommandArg arg)
	{
		arg.CalculationArg = new CalculationArg(canCancel: true);
		_cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			_bendOrderCalculator.ChangeBendOrder(arg.Doc, arg.CalculationArg);
		}, showWaitCursor: true, blockRibbon: true, mainThread: false, "CalculateBendOrder", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public F2exeReturnCode SetTools(IPnCommandArg arg)
	{
		IToolCalculationOption option = _toolCalculations.CreateDefaultOptions(arg.Doc);
		arg.CalculationArg = option?.CalculationArg;
		return _cmd.DoCmd(arg, (IPnCommandArg arg) => _toolCalculations.SetToolsDefault(arg.Doc, option), showWaitCursor: true, blockRibbon: false, mainThread: false, "SetTools", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public void EditTools3d(IPnCommandArg arg)
	{
		_cmd.DoCmd(arg, delegate
		{
			_ribbon.Ribbon_SetActiveTab(898);
			_mainWindowViewModel.BendViewModel.EditTools3D();
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "EditTools3d", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public void AssignBendsToSections(IPnCommandArg arg)
	{
		_cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			_toolCalculations.AssignBendsToSections(arg.Doc);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "AssignBendsToSections", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public F2exeReturnCode SetFingers(IPnCommandArg arg)
	{
		return _cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			arg.doc().ResetFingers();
			return _bendPipe.SetFingers(arg.doc());
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "SetFingers", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public F2exeReturnCode CalculateSimulation(IPnCommandArg arg)
	{
		return _cmd.DoCmd(arg, (IPnCommandArg arg) => _bendPipe.CalculateSimulation(arg.doc(), calculateFingerPos: false), showWaitCursor: true, blockRibbon: true, mainThread: true, "CalculateSimulation", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public F2exeReturnCode ValidateSimulation(IPnCommandArg arg)
	{
		return _cmd.DoCmd(arg, (IPnCommandArg arg) => _bendPipe.ValidateSimulation(_autoMode.PopupsEnabled, arg.doc(), AutoContinueIfOk: false), showWaitCursor: true, blockRibbon: true, mainThread: true, "ValidateSimulation", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public void PlaySimulation(IPnCommandArg arg)
	{
		_cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			_ribbon.Ribbon_SetActiveTab(898);
			_doEvents.DoEvents(10);
			ISimulationThread bendSimulation = arg.doc().BendSimulation;
			if (bendSimulation != null)
			{
				bendSimulation.GotoStep(0.0);
				bendSimulation.Play();
				_doEvents.DoEvents(10);
				while (bendSimulation.IsRunning)
				{
					_doEvents.DoEvents(10);
				}
			}
		}, showWaitCursor: false, blockRibbon: false, mainThread: true, "PlaySimulation", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public F2exeReturnCode AutoBend(IPnCommandArg arg)
	{
		IToolCalculationOption option = _toolCalculations.CreateDefaultOptions(arg.Doc);
		arg.CalculationArg = option?.CalculationArg ?? new CalculationArg(canCancel: true);
		_ribbon.Ribbon_SetActiveTab(898);
		F2exeReturnCode result = _cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			arg.CalculationArg.SetProgress(0.17);
			ILogStep log = arg.CalculationArg.DebugInfo?.CreateSubLog("AutoBend");
			if (!arg.Doc.FreezeCombinedBendDescriptors)
			{
				IBendMachine? bendMachine = arg.Doc.BendMachine;
				if (bendMachine == null || bendMachine.ToolCalculationSettings.BndAutoCalcBendOrder != false)
				{
					_bendOrderCalculator.ChangeBendOrder(arg.Doc, arg.CalculationArg);
					_doEvents.DoEvents();
				}
			}
			if (arg.CalculationArg.CancellationToken.IsCancellationRequested)
			{
				arg.CalculationArg.DebugInfo?.CloseSubLog(log);
				return F2exeReturnCode.CANCEL_BY_USER;
			}
			arg.CalculationArg.SetProgress(0.5);
			F2exeReturnCode f2exeReturnCode = _toolCalculations.SetToolsDefault(arg.Doc, option);
			if (f2exeReturnCode == F2exeReturnCode.OK && arg.Doc.HasToolSetups)
			{
				if (arg.CalculationArg.CancellationToken.IsCancellationRequested)
				{
					arg.CalculationArg.DebugInfo?.CloseSubLog(log);
					return F2exeReturnCode.CANCEL_BY_USER;
				}
				_doEvents.DoEvents();
				arg.CalculationArg.SetStatus(_translator.Translate("CommandInfo.FingerCalculation"), update: false);
				arg.CalculationArg.SetProgress(0.83);
				ILogStep log2 = arg.CalculationArg.DebugInfo?.CreateSubLog(arg.CalculationArg.Status);
				arg.Doc.ResetFingers();
				f2exeReturnCode = _bendPipe.SetFingers(arg.doc());
				arg.CalculationArg.DebugInfo?.CloseSubLog(log2);
			}
			arg.CalculationArg.DebugInfo?.CloseSubLog(log);
			return f2exeReturnCode;
		}, showWaitCursor: true, blockRibbon: true, mainThread: false, "AutoBend", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
		arg.Doc.RecalculateSimulation();
		return result;
	}

	public F2exeReturnCode BendPp(IPnCommandArg arg, bool createNc, bool createReport, string reportFormat)
	{
		return _cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			F2exeReturnCode result = _ppManager.CreateNC(arg.CommandParam, arg.doc(), createNc, createReport, reportFormat);
			_mainWindowViewModel.RefreshScreenActiveTab();
			return result;
		}, showWaitCursor: true, blockRibbon: false, mainThread: true, "BendPp", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public F2exeReturnCode BendPpNo(IPnCommandArg arg)
	{
		return _cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			if (arg.Doc.BendMachine == null)
			{
				return F2exeReturnCode.ERROR_NO_MACHINE_SELECTED;
			}
			if (_autoMode.PopupsEnabled)
			{
				PpRenameProgramNumberViewModel ppRenameProgramNumberViewModel = new PpRenameProgramNumberViewModel();
				ppRenameProgramNumberViewModel.Init(arg.doc());
				PpRenameProgramNumberView content = new PpRenameProgramNumberView
				{
					DataContext = ppRenameProgramNumberViewModel
				};
				Window win = new Window();
				win.Width = 400.0;
				win.Height = 300.0;
				win.Content = content;
				win.Owner = _mainWindowViewModel.MainWindow;
				bool closed = false;
				win.Closed += delegate
				{
					closed = true;
				};
				ppRenameProgramNumberViewModel.OnResult += delegate
				{
					win.Close();
				};
				win.ShowDialog();
				while (ppRenameProgramNumberViewModel.ResultCode == F2exeReturnCode.Undefined && !closed)
				{
					_doEvents.DoEvents(100);
				}
				win.Close();
				if (ppRenameProgramNumberViewModel.ResultCode == F2exeReturnCode.Undefined)
				{
					return F2exeReturnCode.CANCEL_BY_USER;
				}
				return ppRenameProgramNumberViewModel.ResultCode;
			}
			return _ppManager.GeneratePpName(arg.doc());
		}, showWaitCursor: false, blockRibbon: true, mainThread: true, "BendPpNo", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public void BendProgrammableVariables(IPnCommandArg arg)
	{
		_cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			_bendPipe.BendProgrammableVariables(arg.doc(), _mainWindowDataProvider);
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "BendProgrammableVariables", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public F2exeReturnCode BendReportProductionPaperConvert(IPnCommandArg arg, string format)
	{
		return _cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			List<Exception> errors;
			int num = _ppManager.CreateReportConvert(arg.doc(), format, out errors);
			if (errors.Count > 0 || num == 0)
			{
				arg.Doc.MessageDisplay.ShowWarningMessage(_translator.Translate("l_popup.PPGeneration.ReportFailed"), null, notificationStyle: true);
			}
			return (num <= 0 || errors.Count != 0) ? F2exeReturnCode.ERROR_FATAL : F2exeReturnCode.OK;
		}, showWaitCursor: false, blockRibbon: true, mainThread: true, "BendReportProductionPaperConvert", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public F2exeReturnCode BendReportProductionPaper(IPnCommandArg arg, string format)
	{
		return _cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			F2exeReturnCode result = _ppManager.CreateReport(arg.doc(), format);
			_mainWindowViewModel.RefreshScreenActiveTab();
			return result;
		}, showWaitCursor: false, blockRibbon: true, mainThread: true, "BendReportProductionPaper", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public void BendProductionPaperSettings(IPnCommandArg arg)
	{
		Screen3D.IgnoreMouseMove(ignore: true);
		_cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			IPnBndDoc doc = arg.Doc;
			if (!_windowBlock.BlockUI_IsBlock(doc))
			{
				_windowBlock.BlockUI_Block(doc);
			}
			PpScreenshotSettingsView ppScreenshotSettingsView = _factorio.Resolve<PpScreenshotSettingsView>();
			ppScreenshotSettingsView.Init(delegate(PpScreenshotSettingsViewModel sender)
			{
				_mainWindowDataProvider.SetViewForConfig(null);
				sender.Dispose();
				GC.Collect();
				GC.WaitForPendingFinalizers();
				_windowBlock.BlockUI_Unblock(doc);
			});
			_mainWindowDataProvider.SetViewForConfig(ppScreenshotSettingsView);
		}, showWaitCursor: true, blockRibbon: false, mainThread: true, "BendProductionPaperSettings", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
		Screen3D.IgnoreMouseMove(ignore: false);
	}

	public F2exeReturnCode BendPPSend(IPnCommandArg arg, bool nc, bool report)
	{
		return _cmd.DoCmd(arg, (IPnCommandArg arg) => _ppManager.SendPPFiles(arg.doc(), nc, report), showWaitCursor: true, blockRibbon: true, mainThread: true, "BendPPSend", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public F2exeReturnCode ExportGLB(IPnCommandArg arg)
	{
		return _cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			GltfWriter gltfWriter = new GltfWriter(arg.Doc.BendSimulation, arg.Doc);
			string filename = Path.Combine(arg.Doc.BendTempFolder, "simulation.glb");
			Directory.CreateDirectory(arg.Doc.BendTempFolder);
			gltfWriter.WriteGltf(filename);
			return F2exeReturnCode.OK;
		}, showWaitCursor: true, blockRibbon: true, mainThread: true, "ExportGLB", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}

	public void BendMachineConfig(IPnCommandArg arg)
	{
		Screen3D.IgnoreMouseMove(ignore: true);
		_cmd.DoCmd(arg, (Action<IPnCommandArg>)_uiBendMachineService.BendMachineConfig, showWaitCursor: true, blockRibbon: false, mainThread: true, "BendMachineConfig", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
		Screen3D.IgnoreMouseMove(ignore: false);
	}

	public void ShowBendParameter(IPnCommandArg arg)
	{
		_cmd.DoCmd(arg, delegate(IPnCommandArg arg)
		{
			IScopedFactorio scopedFactorio = arg.ScopedFactorio;
			IPnBndDoc doc = arg.Doc;
			_windowBlock.BlockUI_Block(doc);
			PopupBendParameterViewModel popupBendParameterViewModel = scopedFactorio.Resolve<PopupBendParameterViewModel>();
			popupBendParameterViewModel.Init(arg.doc(), arg.doc().BendMachineConfig, delegate
			{
				_mainWindowDataProvider.SetViewForConfig(null);
				_windowBlock.BlockUI_Unblock(doc);
			});
			PopupBendParameterView popupBendParameterView = scopedFactorio.Resolve<PopupBendParameterView>();
			popupBendParameterView.DataContext = popupBendParameterViewModel;
			SubPopupForPopup subPopup = scopedFactorio.Resolve<SubPopupForPopup>().Init(_mainWindowViewModel.MainWindow);
			popupBendParameterViewModel.SubPopup = subPopup;
			_mainWindowDataProvider.SetViewForConfig(popupBendParameterView);
		}, showWaitCursor: true, blockRibbon: false, mainThread: true, "ShowBendParameter", "D:\\Agents\\BuildAgent1\\work\\7fea391e578f71f5\\u\\net\\WiCAM.Pn4000.GuiWpf\\PnCommand\\PnCommandsBend.cs");
	}
}
