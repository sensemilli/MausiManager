using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.PartsReader;
using WiCAM.Pn4000.PartsReader.Contracts;
using WiCAM.Pn4000.PartsReader.DataClasses;
using WiCAM.Pn4000.PKernelFlow.Adapters;
using WiCAM.Pn4000.PKernelFlow.Adapters.Data;
using WiCAM.Pn4000.PN3D.Interfaces;
using WiCAM.Pn4000.ScreenD3D.Controls;
using WiCAM.Pn4000.ScreenD3D.Extensions;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Pipes;

public class PN3DRootPipe : IPnCommandRegister
{
	private Dictionary<(int group, string command), Action<IPnCommandArg>> _pipeList;

	private Dictionary<(int group, string command), Func<IPnCommandArg, F2exeReturnCode>> _dynamicCommands = new Dictionary<(int, string), Func<IPnCommandArg, F2exeReturnCode>>();

	private F2exeReturnCode _answer;

	public readonly IPnCommandsBend RibbonCommandsBend;

	public readonly IPnCommandsScreen RibbonCommandsScreen;

	public readonly IPnCommandsUnfold RibbonCommandsUnfold;

	public readonly IPnCommandsOther RibbonCommandsOther;

	private readonly IScreen3DMain _screen3DMain;

	private readonly IAutoMode _autoMode;

	private readonly IConfigProvider _configProvider;

	private readonly IPnPathService _pathService;

	private readonly ICurrentDocProvider _currentDocProvider;

	private readonly ILogCenterService _logCenterService;

	private readonly IDoEvents _doEvents;

	private string _lastAutoloopName;

	private IMessageLogGlobal _logGlobal { get; }

	public IPN3DBendPipe PN3DBendPipe { get; }

	public IPN3DDocPipe PN3DDocPipe { get; }

	public event Action<IPnCommand> OnCommandCalledBegin;

	public event Action<IPnCommand> OnCommandCalledEnded;

	public PN3DRootPipe(IPN3DBendPipe bendPipe, IPN3DDocPipe docPipe, IPnCommandsBend ribbonCommandsBend, IPnCommandsUnfold ribbonCommandsUnfold, IPnCommandsScreen ribbonCommandsScreen, IPnCommandsOther ribbonCommandsOther, IMessageLogGlobal logGlobal, IScreen3DMain screen3DMain, IAutoMode autoMode, IConfigProvider configProvider, IPnPathService pathService, ICurrentDocProvider currentDocProvider, ILogCenterService logCenterService, IDoEvents doEvents)
	{
		this._logGlobal = logGlobal;
		this.PN3DBendPipe = bendPipe;
		this.PN3DDocPipe = docPipe;
		this.RibbonCommandsBend = ribbonCommandsBend;
		this.RibbonCommandsUnfold = ribbonCommandsUnfold;
		this.RibbonCommandsScreen = ribbonCommandsScreen;
		this.RibbonCommandsOther = ribbonCommandsOther;
		this._screen3DMain = screen3DMain;
		this._autoMode = autoMode;
		this._configProvider = configProvider;
		this._pathService = pathService;
		this._currentDocProvider = currentDocProvider;
		this._logCenterService = logCenterService;
		this._doEvents = doEvents;
		this._pipeList = new Dictionary<(int, string), Action<IPnCommandArg>>
		{
			{
				(81, "V3D_J3CSHA"),
				this.RibbonCommandsScreen.V3D_J3CSHA
			},
			{
				(81, "V3D_J3FO"),
				this.RibbonCommandsScreen.V3D_J3FO
			},
			{
				(81, "V3D_J3BLCH"),
				this.RibbonCommandsScreen.V3D_J3BLCH
			},
			{
				(81, "V3D_WIRESMODE"),
				this.RibbonCommandsScreen.V3D_WIRESMODE
			},
			{
				(81, "V3D_TRA3D100"),
				this.RibbonCommandsScreen.V3D_TRA3D100
			},
			{
				(81, "V3D_TRA3D75"),
				this.RibbonCommandsScreen.V3D_TRA3D75
			},
			{
				(81, "V3D_TRA3D50"),
				this.RibbonCommandsScreen.V3D_TRA3D50
			},
			{
				(81, "V3D_TRA3D25"),
				this.RibbonCommandsScreen.V3D_TRA3D25
			},
			{
				(81, "V3D_ROT3DFREE"),
				this.RibbonCommandsScreen.V3D_ROT3DFREE
			},
			{
				(81, "V3D_ROT3DX"),
				this.RibbonCommandsScreen.V3D_ROT3DX
			},
			{
				(81, "V3D_ROT3DY"),
				this.RibbonCommandsScreen.V3D_ROT3DY
			},
			{
				(81, "V3D_ROT3DZ"),
				this.RibbonCommandsScreen.V3D_ROT3DZ
			},
			{
				(81, "P3DIMPORT"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsOther.Import(arg);
				}
			},
			{
				(81, "P3DIMPORTFLAT"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsOther.ImportFlat(arg);
				}
			},
			{
				(81, "P3DIMPORTUNFOLD"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsOther.ImportUnfold(arg);
				}
			},
			{
				(81, "P3DOPENVIEW"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsOther.OpenView(arg);
				}
			},
			{
				(81, "P3DDISASSEMBLY"),
				delegate(IPnCommandArg arg)
				{
					this.RibbonCommandsOther.Disassembly(arg, null);
				}
			},
			{
				(81, "P3DMAT"),
				this.RibbonCommandsUnfold.SetMaterial
			},
			{
				(81, "P3DUNFOLD"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsUnfold.UnfoldWithMessage(arg);
				}
			},
			{
				(81, "P3DDISSTEPEXP"),
				this.RibbonCommandsOther.DisStepExp
			},
			{
				(81, "P3DEXPORTGEOCAD"),
				this.RibbonCommandsOther.ExportCadGeo
			},
			{
				(81, "P3DCLOSEDOCUMENT"),
				this.RibbonCommandsOther.CloseDocument
			},
			{
				(81, "P3DUNDO"),
				this.RibbonCommandsOther.Undo
			},
			{
				(81, "P3DREDO"),
				this.RibbonCommandsOther.Redo
			},
			{
				(91, "V3D_CNT_INP3D"),
				this.RibbonCommandsUnfold.ViewInputModel
			},
			{
				(91, "V3D_CNT_ENT3D"),
				this.RibbonCommandsUnfold.ViewEntryModel
			},
			{
				(91, "V3D_CNT3D"),
				this.RibbonCommandsUnfold.ViewModifiedModel
			},
			{
				(91, "V3D_CNT_UNF3D"),
				this.RibbonCommandsUnfold.ViewUnfoldModel
			},
			{
				(81, "BNDMACHINE"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.SelectBendMachine(arg);
				}
			},
			{
				(81, "BNDMACHINEUNFOLD"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.SelectBendMachine(arg);
				}
			},
			{
				(81, "SETBNDMACHINE"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.SetBendMachine(arg);
				}
			},
			{
				(81, "BNDBENDORDER"),
				this.RibbonCommandsBend.CalculateBendOrder
			},
			{
				(81, "BNDTOOLS"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.SetTools(arg);
				}
			},
			{
				(81, "BNDVALIDATEFLANGELENGTH"),
				this.RibbonCommandsBend.ValidateFlangeLength
			},
			{
				(81, "BNDTOOLS3D"),
				this.RibbonCommandsBend.EditTools3d
			},
			{
				(81, "BNDASSIGNBENDS"),
				this.RibbonCommandsBend.AssignBendsToSections
			},
			{
				(81, "BNDFINGERS"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.SetFingers(arg);
				}
			},
			{
				(81, "BNDSIMULATION"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.CalculateSimulation(arg);
				}
			},
			{
				(81, "BNDVALIDATESIM"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.ValidateSimulation(arg);
				}
			},
			{
				(81, "BNDSIM"),
				delegate(IPnCommandArg arg)
				{
					this.RibbonCommandsBend.PlaySimulation(arg);
				}
			},
			{
				(81, "BNDAUTO"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.AutoBend(arg);
				}
			},
			{
				(81, "BNDMACHINECFG"),
				this.RibbonCommandsBend.BendMachineConfig
			},
			{
				(81, "BNDPP"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.BendPp(arg, createNc: true, createReport: false, "PDF");
				}
			},
			{
				(81, "BNDPPNO"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.BendPpNo(arg);
				}
			},
			{
				(81, "BNDREPORTCONVERT"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.BendReportProductionPaperConvert(arg, "PDF");
				}
			},
			{
				(81, "BNDREPORT"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.BendReportProductionPaper(arg, "PDF");
				}
			},
			{
				(81, "BNDREPORTPDF"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.BendReportProductionPaper(arg, "PDF");
				}
			},
			{
				(81, "BNDREPORTXLSX"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.BendReportProductionPaper(arg, "XLSX");
				}
			},
			{
				(81, "BNDREPORTDOCX"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.BendReportProductionPaper(arg, "DOCX");
				}
			},
			{
				(81, "BNDREPORTRTF"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.BendReportProductionPaper(arg, "RTF");
				}
			},
			{
				(81, "BNDREPORTCSV"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.BendReportProductionPaper(arg, "CSV");
				}
			},
			{
				(81, "BNDREPORTPPTX"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.BendReportProductionPaper(arg, "PPTX");
				}
			},
			{
				(81, "BNDREPORTSET"),
				this.RibbonCommandsBend.BendProductionPaperSettings
			},
			{
				(81, "BNDPRVAR"),
				this.RibbonCommandsBend.BendProgrammableVariables
			},
			{
				(81, "BNDPPSEND"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.BendPPSend(arg, nc: true, report: true);
				}
			},
			{
				(81, "BNDPPSENDREPORTONLY"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.BendPPSend(arg, nc: false, report: true);
				}
			},
			{
				(81, "BNDPPSENDNCONLY"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.BendPPSend(arg, nc: true, report: false);
				}
			},
			{
				(81, "BNDEXPORTSIMGLB"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.ExportGLB(arg);
				}
			},
			{
				(81, "V3D_SIM_PLAY"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.SimulationPlay(arg);
				}
			},
			{
				(81, "V3D_SIM_PAUSE"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.SimulationPause(arg);
				}
			},
			{
				(81, "V3D_SIM_NEXT"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.SimulationNext(arg);
				}
			},
			{
				(81, "V3D_SIM_END"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.SimulationEnd(arg);
				}
			},
			{
				(81, "V3D_SIM_PREVIOUS"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.SimulationPrevious(arg);
				}
			},
			{
				(81, "V3D_SIM_START"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsBend.SimulationStart(arg);
				}
			},
			{
				(81, "P3DPIPETTETOPCOLOR"),
				this.RibbonCommandsUnfold.PipetteFaceColorForVisibleFace
			},
			{
				(81, "P3DRECONSTRUCT"),
				this.RibbonCommandsUnfold.ReconstructFromFaceSelectFace
			},
			{
				(81, "P3DRECONSTRUCTIRREGULARBENDS"),
				this.RibbonCommandsUnfold.ReconstructIrregularBends
			},
			{
				(81, "P3DRECONSTRUCTBENDSEXPERIMENTAL"),
				this.RibbonCommandsUnfold.ReconstructBendsExperimental
			},
			{
				(81, "P3DGEOVALIDATE"),
				this.RibbonCommandsUnfold.ValidateGeometry
			},
			{
				(81, "P3DGEOVALRESET"),
				this.RibbonCommandsUnfold.ValidateGeometryReset
			},
			{
				(81, "P3DUNFOLDF"),
				this.RibbonCommandsUnfold.UnfoldFromFaceSelectFace
			},
			{
				(81, "P3DUNFOLDT"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsUnfold.UnfoldTube(arg);
				}
			},
			{
				(81, "P3DOPEN"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsOther.Open(arg);
				}
			},
			{
				(81, "P3DDELETE"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsOther.Delete(arg);
				}
			},
			{
				(81, "P3DSAVE"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsOther.Save(arg);
				}
			},
			{
				(81, "P3DANALASSEM"),
				this.RibbonCommandsOther.AnalyzeAssembly
			},
			{
				(81, "P3DCLEAN"),
				this.RibbonCommandsOther.Clean
			},
			{
				(81, "P3DBENDDB"),
				this.RibbonCommandsBend.ShowBendParameter
			},
			{
				(81, "P3DMATALLI"),
				this.RibbonCommandsOther.MaterialAlliance
			},
			{
				(83, "P3DUNFOLDCFG"),
				this.RibbonCommandsOther.UnfoldConfig
			},
			{
				(81, "P3DINFO"),
				this.RibbonCommandsOther.ShowInfo
			},
			{
				(81, "P3DLEGEND"),
				this.RibbonCommandsOther.ShowLegend
			},
			{
				(81, "P3DTR2D"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsUnfold.Transfer2D(arg);
				}
			},
			{
				(81, "P3DTR2DMODIFIED"),
				delegate(IPnCommandArg arg)
				{
					this.RibbonCommandsUnfold.Transfer2DFromModifiedFace(arg, removeProjectionHoles: true);
				}
			},
			{
				(81, "P3DTR2DMODIFIEDWITHHOLES"),
				delegate(IPnCommandArg arg)
				{
					this.RibbonCommandsUnfold.Transfer2DFromModifiedFace(arg, removeProjectionHoles: false);
				}
			},
			{
				(81, "DISTANCE3D"),
				this.RibbonCommandsScreen.Distance3D
			},
			{
				(81, "POINT3D"),
				this.RibbonCommandsScreen.SelectPoint3D
			},
			{
				(81, "ANGLE3D"),
				this.RibbonCommandsScreen.Angle3D
			},
			{
				(81, "NODE3DINFO"),
				this.RibbonCommandsOther.Node3DInfo
			},
			{
				(81, "VIEW3DINFO"),
				this.RibbonCommandsOther.View3DInfo
			},
			{
				(81, "DEVIMPORT"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsOther.DevImport(arg);
				}
			},
			{
				(81, "DEVIMPORTLOWTESS"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsOther.DevImportLowTess(arg);
				}
			},
			{
				(81, "DEVSAVE3D"),
				delegate(IPnCommandArg arg)
				{
					this._answer = this.RibbonCommandsOther.DevSave(arg);
				}
			},
			{
				(81, "P3DPRINT"),
				this.RibbonCommandsOther.Print
			},
			{
				(81, "OPNSTART"),
				this.RibbonCommandsOther.OldPnStart
			},
			{
				(81, "OPNSTARTFOR3D"),
				this.RibbonCommandsOther.OldPnStart3D
			}
		};
	}

	public void LoadPartsXmlToF()
	{
		List<DisassemblyPart> list = (new global::WiCAM.Pn4000.PartsReader.PartsReader().DeserializeAssembly(Path.Combine(this._pathService.FolderCad3d2Pn, "Parts.XML"))?.DisassemblyParts ?? new List<DisassemblyPart>()).Where((DisassemblyPart x) => x.PartHistory != DisassemblyPartHistory.Deleted && x.PartInfo.PurchasedPart == 0).ToList();
		AsmComAdapter.AsmComCount = list.Count;
		bool p3D_UseAssemblyName = this._configProvider.InjectOrCreate<General3DConfig>().P3D_UseAssemblyName;
		int num = 0;
		foreach (DisassemblyPart item in list)
		{
			AsmComAdapter.SetElement(num, new AsmComEntity
			{
				PartName = (p3D_UseAssemblyName ? ((IDisassemblyPart)item).ModifiedAssemblyName : ((IDisassemblyPart)item).ModifiedGeometryName),
				Iasarc = 0,
				LengthX = (float)(((IDisassemblyPart)item).LenX * 1.0),
				LengthY = (float)(((IDisassemblyPart)item).LenY * 1.0),
				LengthZ = (float)(((IDisassemblyPart)item).LenZ * 1.0),
				PartId = (float)((double)((IDisassemblyPart)item).ID + 0.2),
				InstanceNumber = (float)((double)((IDisassemblyPart)item).InstanceNumber * 1.0),
				Zasrot = 0f,
				Iasanz = 1,
				PartType = ((IDisassemblyPart)item).PartInfo.PartType.ToString(),
				Thickness = (float)((IDisassemblyPart)item).Thickness
			});
			num++;
		}
	}

	public F2exeReturnCode Call(IPnCommand command)
	{
		IMessageDisplay messageDisplay = this._logGlobal.WithContext(this._currentDocProvider.CurrentDoc);
		GeneralSystemComponentsAdapter.GetCurrentAutoloopName(out var fileName);
		if (this._lastAutoloopName != fileName)
		{
			this._lastAutoloopName = fileName;
			if (!string.IsNullOrEmpty(fileName))
			{
				messageDisplay.LogDebug("Begin Autoloop: " + fileName);
			}
		}
		if (this._autoMode.HasGui)
		{
			this._answer = this.PN3DDocPipe.Activate3DTab();
			if (this._answer != 0)
			{
				messageDisplay.LogDebug("Skip Command: " + command.Command + " because of activating 3D Tab failed.");
				this._doEvents.DoEvents(null);
				return this._answer;
			}
		}
		string text;
		if (command.Command.Contains("("))
		{
			text = Regex.Match(command.Command, "(?<=\\()(.*)(?=\\))").Value;
			command.Command = command.Command.Substring(0, command.Command.IndexOf('('));
		}
		else
		{
			text = string.Empty;
		}
		this.OnCommandCalledBegin?.Invoke(command);
		this._answer = F2exeReturnCode.Undefined;
		PnCommandArg pnCommandArg = new PnCommandArg(this._currentDocProvider.CurrentDoc, this._currentDocProvider.CurrentFactorio);
		pnCommandArg.CommandStr = command.Command;
		pnCommandArg.CommandGroup = command.Group;
		pnCommandArg.CommandParam = text;
		try
		{
			Dictionary<(int group, string command), Action<IPnCommandArg>> pipeList = this._pipeList;
			if (pipeList != null && pipeList.TryGetValue((command.Group, command.Command), out var value))
			{
				value(pnCommandArg);
				CommandManager.InvalidateRequerySuggested();
			}
			else if (!this.PipeCallByPrefix(command))
			{
				this._answer = this.ExecuteDynamicCommands(pnCommandArg) ?? F2exeReturnCode.Undefined;
			}
		}
		catch (Exception e)
		{
			this._logCenterService.RaportUnhandledException(e);
			this._logGlobal.ShowTranslatedErrorMessage("RootPipe.CommandException", command.Group, command.Command);
			messageDisplay.LogDebug("CommandException: " + command.Command + $"({text}) {27}");
			this._doEvents.DoEvents(null);
			return F2exeReturnCode.ERROR_FATAL;
		}
		this.OnCommandCalledEnded?.Invoke(command);
		if (this._answer == F2exeReturnCode.Undefined)
		{
			this._answer = F2exeReturnCode.OK;
		}
		this._doEvents.DoEvents(null);
		return this._answer;
	}

	private bool PipeCallByPrefix(IPnCommand command)
	{
		if (command.Command.StartsWith("V3D_"))
		{
			this._screen3DMain.ScreenD3D.ZoomExtend();
			return true;
		}
		return false;
	}

	private F2exeReturnCode? ExecuteDynamicCommands(IPnCommandArg arg)
	{
		if (this._dynamicCommands.TryGetValue((arg.CommandGroup, arg.CommandStr), out var value))
		{
			return value(arg);
		}
		return null;
	}

	public void RegisterDynamicCommand(int group, string cmdStr, bool overwrite, Func<IPnCommandArg, F2exeReturnCode> cmd)
	{
		if (overwrite)
		{
			this._dynamicCommands[(group, cmdStr)] = cmd;
		}
		else
		{
			this._dynamicCommands.Add((group, cmdStr), cmd);
		}
	}

	public void RegisterDynamicCommand(int group, string cmdStr, bool overwrite, Action<IPnCommandArg> cmd)
	{
		this.RegisterDynamicCommand(group, cmdStr, overwrite, delegate(IPnCommandArg arg)
		{
			cmd(arg);
			return F2exeReturnCode.Undefined;
		});
	}
}
