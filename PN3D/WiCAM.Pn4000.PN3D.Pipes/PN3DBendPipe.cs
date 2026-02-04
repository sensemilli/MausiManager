using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Interfaces;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.BendSimulation;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.BendServices;
using WiCAM.Pn4000.Contracts.BendSimulation;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.PN3D.BendSimulation.PP;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Popup.Model;
using WiCAM.Pn4000.PN3D.Popup.UI.Models;
using WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;
using WiCAM.Pn4000.PN3D.Popup.UI.Views;
using WiCAM.Pn4000.PN3D.Popup.View;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ToolCalculationMediator;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Pipes;

public class PN3DBendPipe : IPN3DBendPipe
{
	private readonly ITranslator _translator;

	private readonly IFactorio _factorio;

	private readonly IScreen3DMain _screen3DMain;

	private readonly IAutoMode _autoMode;

	private readonly IConfigProvider _configProvider;

	private readonly IPnPathService _pathService;

	private readonly IMachineHelper _machineHelper;

	private readonly IPpManager _ppManager;

	private readonly IMainWindowDataProvider _mainWindowDataProvider;

	private readonly IGlobals _globals;

	private readonly IMainWindowBlock _mainWindowBlock;

	private readonly IBendSimulationFactory _bendSimulationFactory;

	private readonly IToolCalculations _toolCalculations;

	private readonly IDoEvents _doEvents;

	private readonly IUndo3dService _undo3dService;

	public PN3DBendPipe(ITranslator translator, IFactorio factorio, IScreen3DMain screen3DMain, IAutoMode autoMode, IConfigProvider configProvider, IMachineHelper machineHelper, IPpManager ppManager, IPnPathService pathService, IMainWindowDataProvider mainWindowDataProvider, IGlobals globals, IMainWindowBlock mainWindowBlock, IBendSimulationFactory bendSimulationFactory, IToolCalculations toolCalculations, IDoEvents doEvents, IUndo3dService undo3dService)
	{
		this._translator = translator;
		this._factorio = factorio;
		this._screen3DMain = screen3DMain;
		this._autoMode = autoMode;
		this._configProvider = configProvider;
		this._machineHelper = machineHelper;
		this._ppManager = ppManager;
		this._pathService = pathService;
		this._mainWindowDataProvider = mainWindowDataProvider;
		this._globals = globals;
		this._mainWindowBlock = mainWindowBlock;
		this._bendSimulationFactory = bendSimulationFactory;
		this._toolCalculations = toolCalculations;
		this._doEvents = doEvents;
		this._undo3dService = undo3dService;
	}

	public void SelectMachineRecentlyUsed(string name, string fullPath, int machineNumber, IDoc3d doc)
	{
		this._mainWindowDataProvider.Ribbon_GetActiveTabID();
		IBendMachine? bendMachine = doc.BendMachine;
		doc.MachinePath = fullPath;
		F2exeReturnCode f2exeReturnCode = this.SetBendMachine(usePreferredTools: true, null, doc);
		IBendMachine bendMachine2 = doc.BendMachine;
		if (bendMachine != bendMachine2)
		{
			this._undo3dService.Save(doc, this._translator.Translate("Undo3d.SetBendMachine", bendMachine2?.MachineNo, bendMachine2?.Name));
		}
		RecentlyUsedRecord rec = new RecentlyUsedRecord
		{
			FileName = name,
			FullPath = fullPath,
			ArchiveID = machineNumber,
			Type = "BMACH"
		};
		if (f2exeReturnCode == F2exeReturnCode.OK || f2exeReturnCode == F2exeReturnCode.ERROR_NOTUNFOLDABLE_GEOMETRY)
		{
			this._mainWindowDataProvider.AddRecentlyUsedRecord(rec);
		}
		else
		{
			this._mainWindowDataProvider.DeleteRecentlyUsedRecord(rec);
		}
	}

	public F2exeReturnCode SelectBendMachine(string machineNumber, IDoc3d doc)
	{
		try
		{
			if (!int.TryParse(machineNumber, out var result))
			{
				return F2exeReturnCode.ERROR_NO_MACHINE_SELECTED;
			}
			return this.SelectBendMachineById(result, doc);
		}
		catch
		{
			return F2exeReturnCode.ERROR_NO_MACHINE_SELECTED;
		}
	}

	public F2exeReturnCode SelectBendMachineById(int id, IDoc3d doc)
	{
		try
		{
			string machine3DFolder = this._pathService.GetMachine3DFolder(id);
			if (doc.MachinePath != machine3DFolder)
			{
				doc.MachinePath = machine3DFolder;
				return this.SetBendMachine(usePreferredTools: true, null, doc);
			}
			return F2exeReturnCode.OK;
		}
		catch
		{
			return F2exeReturnCode.ERROR_NO_MACHINE_SELECTED;
		}
	}

	public F2exeReturnCode SetFingers(IDoc3d doc)
	{
		F2exeReturnCode f2exeReturnCode = this.CheckDocProgress(doc, DocState.ToolsCalculated, showErrorMessage: true);
		if (f2exeReturnCode != 0)
		{
			return f2exeReturnCode;
		}
		if (doc.SafeModeUnfold)
		{
			doc.MessageDisplay.ShowTranslatedErrorMessage("l_popup.WarningSafeMode");
			return F2exeReturnCode.ERROR_SAFEMODEUNFOLD;
		}
		doc.ResetFingers();
		if (doc.IsUpdateDocNeeded())
		{
			doc.UpdateDoc();
		}
		F2exeReturnCode f2exeReturnCode2 = F2exeReturnCode.OK;
		doc.CalculateFingers();
		this._undo3dService.Save(doc, this._translator.Translate("Undo3d.FingerCalculation"));
		if (f2exeReturnCode2 == F2exeReturnCode.OK && !doc.HasAllFingers)
		{
			return F2exeReturnCode.ERROR_NO_FINGERS_SELECTED;
		}
		return f2exeReturnCode2;
	}

	public F2exeReturnCode CalculateSimulation(IDoc3d doc, bool calculateFingerPos, bool backToStart = true)
	{
		F2exeReturnCode f2exeReturnCode = this.CheckDocProgress(doc, DocState.ToolsCalculated, showErrorMessage: true);
		if (f2exeReturnCode != 0)
		{
			return f2exeReturnCode;
		}
		if (doc.HasModel)
		{
			doc.HasBendStepsCalculated = true;
			doc.HasSimulationCalculated = true;
		}
		if (doc.CombinedBendDescriptors.Any((ICombinedBendDescriptorInternal cbd) => cbd.FingerPositioningMode == FingerPositioningMode.Auto && cbd.FingerStability == FingerStability.Unstable))
		{
			return F2exeReturnCode.WARN_FINGERS_UNSTABLE;
		}
		if (!doc.HasBendStepsCalculated)
		{
			return F2exeReturnCode.ERROR_NO_STEPS_CALCULATED;
		}
		return F2exeReturnCode.OK;
	}

	public ISimulationThread CreateNewValidationSim(IDoc3d doc)
	{
		Model bendModel3D = doc.BendModel3D;
		IBendMachine bendMachine = doc.BendMachine;
		IToolsAndBends tab = doc.ToolsAndBends;
		double machineCenter = (bendMachine?.ToolConfig.LowerBeamXStart + bendMachine?.ToolConfig.LowerBeamXEnd).GetValueOrDefault() * 0.5;
		SimParams creationParameters = new SimParams
		{
			BendMachine = bendMachine,
			Bends = doc.CombinedBendDescriptors.Select((ICombinedBendDescriptorInternal cbd) => new SimBendInfo(cbd, tab.BendPositions.FirstOrDefault((IBendPositioning x) => x.Order == cbd.Order))
			{
				MachineCenter = machineCenter
			}).Cast<ISimulationBendInfo>().ToList(),
			Material = doc.Material,
			PartModel = bendModel3D,
			SimulationStepFilter = new SimStepFilterAll(),
			IgnoreCollisionsCompletly = false,
			Thickness = doc.Thickness,
			ToolSetups = tab.ToolSetups
		};
		return this._bendSimulationFactory.CreateSimulation(creationParameters);
	}

	public F2exeReturnCode ValidateSimulation(bool visible, IDoc3d doc, bool AutoContinueIfOk)
	{
		this._doEvents.DoEvents(null);
		if (doc.SafeModeUnfold)
		{
			doc.MessageDisplay.ShowTranslatedErrorMessage("l_popup.WarningSafeMode");
			return F2exeReturnCode.ERROR_SAFEMODEUNFOLD;
		}
		if (doc.IsUpdateDocNeeded())
		{
			doc.UpdateDoc();
		}
		IMessageLogDoc messageDisplay = doc.MessageDisplay;
		double safetyDistance = this._configProvider.InjectOrCreate<SimValidationConfig>().SafetyDistance;
		F2exeReturnCode result = this.CheckDocProgress(doc, DocState.ToolsCalculated, showErrorMessage: true);
		if (result != 0)
		{
			return result;
		}
		bool flag = visible;
		string message;
		bool maxForceOk;
		bool toolOverlapOk;
		bool toolSavetyDistOk;
		bool toolsFitInMachine;
		bool flag2 = doc.ValidateTools(out message, out maxForceOk, out toolOverlapOk, out toolSavetyDistOk, out toolsFitInMachine);
		(F2exeReturnCode returnCode, string message) tuple = this._toolCalculations.ValidateSimulation(doc);
		F2exeReturnCode f2exeReturnCode = tuple.returnCode;
		string text = tuple.message;
		ISimulationThread sim = this.CreateNewValidationSim(doc);
		ISimulationThread bendSimulation = doc.BendSimulation;
		double? num = bendSimulation?.CurrentStep;
		if (flag && !sim.IsMultiSim)
		{
			doc.BendSimulation = sim;
		}
		else
		{
			doc.BendSimulation?.Pause();
		}
		SimulationConfig simulationConfig = this._configProvider?.InjectOrCreate<SimulationConfig>();
		sim.SetSettings(CheckCollisions: true, CheckCollisionsKeyFrames: false, SheetHandlingVisible: false, 1.0, 10000, ValidationMode: true, PauseOnCollision: false, simulationConfig.IgnoreClampCollisions, simulationConfig.IgnoreOverbendCollisions, simulationConfig.IgnoreOpeningCollisions, simulationConfig.IgnoreClosingCollisions, simulationConfig.IgnoreLiftingAidCollisions);
		PopupValidationPendingModel popupValidationPendingModel = new PopupValidationPendingModel();
		PopupValidationPendingViewModel popupValidationPendingViewModel = null;
		PopupValidationPending popupValidationPending = null;
		List<ICombinedBendDescriptor> list = this.ValidateSimulationToolSafetyWarningBends(safetyDistance, sim, doc);
		if (list.Count > 0)
		{
			text = text + Environment.NewLine + this._translator.Translate("BendView.ToolVerification.SafetyDistanceWarning", safetyDistance, string.Join(", ", from x in list
				orderby x.Order
				select x.Order + 1));
			if (f2exeReturnCode == F2exeReturnCode.OK)
			{
				f2exeReturnCode = F2exeReturnCode.ERROR_STEPS_NOT_VALIDATED;
			}
		}
		sim.GotoStep(0.0, visible);
		sim.StopEvent += popupValidationPendingModel.StopEvent;
		if (flag)
		{
			popupValidationPendingViewModel = new PopupValidationPendingViewModel(this._translator, string.IsNullOrWhiteSpace(text), text.Trim(), AutoContinueIfOk);
			sim.TickEvent += popupValidationPendingViewModel.SimulationTick;
			popupValidationPending = new PopupValidationPending(popupValidationPendingViewModel);
			PopupValidationPending popupValidationPending2 = popupValidationPending;
			popupValidationPending2.Aborting = (Action)Delegate.Combine(popupValidationPending2.Aborting, (Action)delegate
			{
				result = F2exeReturnCode.CANCEL_BY_USER;
				sim.Pause();
			});
			sim.StopEvent += popupValidationPendingViewModel.StopEvent;
			sim.StopEvent += popupValidationPending.StopEvent;
			sim.PauseEvent += popupValidationPending.PauseEvent;
		}
		sim.Play();
		if (flag)
		{
			popupValidationPending.ShowDialog();
			sim.StopEvent -= popupValidationPending.StopEvent;
			sim.StopEvent -= popupValidationPendingViewModel.StopEvent;
			sim.PauseEvent -= popupValidationPending.PauseEvent;
			sim.TickEvent -= popupValidationPendingViewModel.SimulationTick;
		}
		else
		{
			while (!popupValidationPendingModel.simulationDone)
			{
				if (this._autoMode.HasGui)
				{
					this.RenderFrame(this._screen3DMain.Screen3D);
				}
				this._doEvents.DoEvents(100);
			}
		}
		sim.StopEvent -= popupValidationPendingModel.StopEvent;
		List<(double, double, bool)> list2 = sim.GetCollisionIntervals().ToList();
		if (flag && num.HasValue)
		{
			doc.BendSimulation = bendSimulation;
			bendSimulation.GotoStep(num.Value);
			doc.Factorio.Resolve<IBendSelection>().SetCurrentBendBySimulation(bendSimulation.State.ActiveStep.BendInfo.Order);
		}
		if (result != 0)
		{
			return result;
		}
		if (list2.Count > 0)
		{
			messageDisplay.LogErrorMessage(this._translator.Translate("BendView.BendPipe.SimulationValidationCollisions", list2.Count));
			return F2exeReturnCode.ERROR_STEPS_NOT_VALIDATED;
		}
		if (f2exeReturnCode != 0)
		{
			return f2exeReturnCode;
		}
		if (!flag)
		{
			messageDisplay.ShowTranslatedInformationMessage("BendView.BendPipe.SimulationValidationOK");
		}
		if (flag2)
		{
			return F2exeReturnCode.OK;
		}
		if (!toolOverlapOk)
		{
			return F2exeReturnCode.WARN_TOOLS_DOES_NOT_OVERLAP_WITH_BEND_ENOUGH;
		}
		if (!toolSavetyDistOk)
		{
			return F2exeReturnCode.WARN_TOOLS_NO_SAFETY_DISTANCE;
		}
		if (!toolsFitInMachine)
		{
			return F2exeReturnCode.WARN_TOOLS_DOES_NOT_FIT_IN_MACHINE;
		}
		if (!maxForceOk)
		{
			return F2exeReturnCode.WARN_TOOLS_MAX_FORCE_EXCEEDED;
		}
		return F2exeReturnCode.ERROR_UNDEFINED;
	}

	public List<ICombinedBendDescriptor> ValidateSimulationToolSafetyWarningBends(double safetyDistance, ISimulationThread sim, IDoc3d doc)
	{
		List<ICombinedBendDescriptor> list = new List<ICombinedBendDescriptor>();
		foreach (ICombinedBendDescriptorInternal cbd in doc.CombinedBendDescriptors)
		{
			ISimulationStep simulationStep = sim.SimulationsSteps.Where((ISimulationStep x) => x.BendInfo.Order == cbd.Order).ToList().FirstOrDefault((ISimulationStep x) => x is IStepMinToolGap);
			if (simulationStep == null)
			{
				continue;
			}
			sim.GotoStep(sim.SimulationsSteps.IndexOf(simulationStep), visible: false);
			if (safetyDistance > 0.0)
			{
				Model upperToolSystemTools = sim.State.MachineConfig.Geometry.UpperToolSystemTools;
				Matrix4d transform = upperToolSystemTools.Transform;
				upperToolSystemTools.Transform *= Matrix4d.Translation(0.0, 0.0, 0.0 - safetyDistance);
				if (new ModelCollision
				{
					ModelA = upperToolSystemTools,
					ModelB = sim.State.MachineConfig.Geometry.LowerToolSystemTools,
					TreatTouchingFacesAsCollision = true,
					BreakAtFirstCollision = true
				}.UpdateAnyCollisions())
				{
					list.Add(cbd);
				}
				upperToolSystemTools.Transform = transform;
			}
		}
		return list;
	}

	private void RenderFrame(Screen3D screen3d)
	{
		if (screen3d != null)
		{
			EventWaitHandle wait = new EventWaitHandle(initialState: false, EventResetMode.ManualReset);
			screen3d.ScreenD3D.Render(skipQueuedFrames: true, delegate
			{
				wait.Set();
			});
			wait.WaitOne();
			this._doEvents.DoEvents(null);
		}
		else
		{
			this._doEvents.DoEvents(null);
		}
	}

	public F2exeReturnCode AutoBend(IDoc3d doc)
	{
		throw new NotImplementedException();
	}

	public F2exeReturnCode SetBendMachine(bool usePreferredTools, Dictionary<ICombinedBendDescriptorInternal, IPreferredProfile> preferredProfiles, IDoc3d doc)
	{
		if (string.IsNullOrEmpty(doc.MachinePath))
		{
			return F2exeReturnCode.ERROR_NO_MACHINE_SELECTED;
		}
		this._mainWindowBlock.InitWait(doc);
		_ = doc.BendMachine;
		this._mainWindowBlock.CloseWait(doc);
		return F2exeReturnCode.OK;
	}

	public void CheckCalcSimulation(IDoc3d doc, Action actionBefore = null, Action actionAfter = null)
	{
		if (doc.HasModel && doc.HasToolSetups && doc.HasFingers && !doc.HasSimulationCalculated)
		{
			actionBefore?.Invoke();
			this.CalculateSimulation(doc, calculateFingerPos: false);
			actionAfter?.Invoke();
		}
	}

	public void BendProgrammableVariables(IDoc3d doc, IMainWindowDataProvider mainWindowDataProvider)
	{
		if (!this._autoMode.PopupsEnabled || this.CheckDocProgress(doc, DocState.FingerCalculated, showErrorMessage: true) != 0)
		{
			return;
		}
		this._mainWindowBlock.BlockUI_Block(doc);
		ProgrammableVariablesModel model = new ProgrammableVariablesModel(doc.BendMachineConfig.BendMachine, doc.PpModel);
		ProgrammableVariablesViewModel dataContext = new ProgrammableVariablesViewModel(model, this._configProvider, delegate
		{
			if (model.isViewOkResult)
			{
				model.GetDataBack();
			}
			mainWindowDataProvider.SetViewForConfig(null);
			this._mainWindowBlock.BlockUI_Unblock(doc);
		});
		ProgrammableVariablesView viewForConfig = new ProgrammableVariablesView
		{
			DataContext = dataContext
		};
		mainWindowDataProvider.SetViewForConfig(viewForConfig);
	}

	public F2exeReturnCode CheckDocProgress(IDoc3d doc, DocState requiredState, bool showErrorMessage)
	{
		ITranslator translator = this._translator;
		if (doc.State < DocState.BendModelCreated)
		{
			if (showErrorMessage)
			{
				string message = translator.Translate("BendView.BendPipe.SelectEntryModel");
				string caption = translator.Translate("BendView.BendPipe.NoEntryModel");
				doc.MessageDisplay.ShowErrorMessage(message, caption);
			}
			return F2exeReturnCode.ERROR_NOTUNFOLDABLE_GEOMETRY;
		}
		if (requiredState <= DocState.BendModelCreated)
		{
			return F2exeReturnCode.OK;
		}
		if (doc.EntryModel3D.PartInfo.PartType.HasFlag(PartType.FlatSheetMetal))
		{
			string message2 = translator.Translate("BendView.BendPipe.SelectModelWithBends");
			string caption2 = translator.Translate("BendView.BendPipe.NoBends");
			doc.MessageDisplay.ShowErrorMessage(message2, caption2);
			return F2exeReturnCode.OK_FLAT;
		}
		if (doc.State < DocState.MachineLoaded)
		{
			if (showErrorMessage)
			{
				string message3 = translator.Translate("BendView.BendPipe.SelectMachine");
				string caption3 = translator.Translate("BendView.BendPipe.NoMachineSelected");
				doc.MessageDisplay.ShowErrorMessage(message3, caption3);
			}
			return F2exeReturnCode.ERROR_NO_MACHINE_SELECTED;
		}
		if (requiredState <= DocState.MachineLoaded)
		{
			return F2exeReturnCode.OK;
		}
		if (!doc.HasToolSetups)
		{
			if (showErrorMessage)
			{
				string message4 = translator.Translate("BendView.BendPipe.CalculateTools");
				string caption4 = translator.Translate("BendView.BendPipe.NoToolsCalculated");
				doc.MessageDisplay.ShowErrorMessage(message4, caption4);
			}
			return F2exeReturnCode.ERROR_NO_TOOLS_SELECTED;
		}
		if (requiredState <= DocState.ToolsCalculated)
		{
			return F2exeReturnCode.OK;
		}
		if (!doc.HasAllFingers)
		{
			if (showErrorMessage)
			{
				string message5 = translator.Translate("BendView.BendPipe.CalculateFingers");
				string caption5 = translator.Translate("BendView.BendPipe.NoFingersCalculated");
				doc.MessageDisplay.ShowErrorMessage(message5, caption5);
			}
			return F2exeReturnCode.ERROR_NO_STEPS_CALCULATED;
		}
		return F2exeReturnCode.OK;
	}

	public static void DeleteMachine(BendMachine machine)
	{
	}

	public void SetBendMachineForVisualization(IDoc3d doc, IGlobals globals, IMainWindowDataProvider mainWindowDataProvider)
	{
		if (doc?.BendMachineConfig != null && doc.BendMachine?.Geometry != null && !doc.MachineFullyLoaded)
		{
			this._mainWindowBlock.InitWait();
			this.CheckCalcSimulation(doc);
			this._mainWindowBlock.CloseWait();
		}
	}
}
