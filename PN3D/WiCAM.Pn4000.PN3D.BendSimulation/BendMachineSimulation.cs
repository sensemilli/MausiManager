using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Interfaces;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Dies;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Punches;
using WiCAM.Pn4000.BendModel;
using WiCAM.Pn4000.BendModel.Base.Enum;
using WiCAM.Pn4000.BendModel.Base.Motions;
using WiCAM.Pn4000.BendTable;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.BendPP;
using WiCAM.Pn4000.Contracts.BendSimulation;
using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.BendSimulation;

public class BendMachineSimulation : IBendMachineSimulation, IBendMachineSimulationBasic
{
	protected IGlobals _globals;

	private readonly IPnPathService _pathService;

	private readonly IConfigProvider _configProvider;

	private readonly IAutoMode _autoMode;

	private readonly IMaterialManager _materials;

	protected IDoc3d _doc;

	protected Dictionary<ICombinedBendDescriptorInternal, IPreferredProfile> _preferredProfilesDict;

	public BendMachine BendMachine { get; set; }

	public IBendTable BendTable { get; private set; }

	public IMachineHelper MachineHelper { get; }

	public IPostProcessor PostProcessor { get; private set; }

	public event Action<ISimulationThread, ISimulationThread> SimulationChangedEvent;

	public BendMachineSimulation(IMachineHelper machineHelper, IGlobals globals, IAutoMode autoMode, IMaterialManager materials, IPnPathService pathService, IConfigProvider configProvider)
	{
		this.MachineHelper = machineHelper;
		this._globals = globals;
		this._autoMode = autoMode;
		this._materials = materials;
		this._pathService = pathService;
		this._configProvider = configProvider;
	}

	public IBendMachineSimulation Init(string path, Dictionary<ICombinedBendDescriptorInternal, IPreferredProfile> preferredProfilesDict, IDoc3d doc, out bool loadingError)
	{
		loadingError = false;
		this._doc = doc;
		this._preferredProfilesDict = preferredProfilesDict;
		this.BendMachine = this.MachineHelper.Deserialize(path);
		if (!string.IsNullOrEmpty(this.BendMachine.PressBrakeInfo.PP))
		{
			this.PostProcessor = doc.PostProcessor;
		}
		_ = this._doc?.Material;
		General3DConfig general3DConfig = this._configProvider.InjectOrCreate<General3DConfig>();
		string text = "";
		text = ((this.BendMachine.PressBrakeInfo.UseGlobalBendTable != 0) ? this.BendMachine.MachinePath : Path.Combine(Directory.GetParent(this.BendMachine.MachinePath).FullName, "globals"));
		this.BendTable = new BendTableDataBase(this.BendMachine.UnfoldConfig?.DefaultKFactor ?? general3DConfig.P3D_Default_KFactor);
		BendTableReader bendTableReader = new BendTableReader();
		if (!bendTableReader.ReadFile(Path.Combine(new string[2] { text, "BENDTABLE.json" }), this.BendTable) && !bendTableReader.ReadFile(Path.Combine(new string[2] { text, "BENDTABLE.txt" }), this.BendTable))
		{
			loadingError = true;
		}
		this._globals.PKernelFlowGlobalData.ActiveBendMachineID = this.BendMachine.Number;
		this.CheckConfig(doc.MessageDisplay);
		return this;
	}

	public void RestorePrefferedTools()
	{
		if (this._preferredProfilesDict == null)
		{
			return;
		}
		foreach (KeyValuePair<ICombinedBendDescriptorInternal, IPreferredProfile> item in this._preferredProfilesDict)
		{
			foreach (IBendDescriptor item2 in item.Key.Enumerable)
			{
				this._doc.PreferredProfileStore.AssignPreferredProfileToCommonBend(item.Value, item2.BendParams.EntryFaceGroup.ID, item2.BendParams.UnfoldFaceGroup.SubBendIndex.GetValueOrDefault(), ToolSelectionType.PreferredTools);
			}
		}
	}

	public virtual void CalculateBendSteps(Model part, List<IBendPositioning> bendPositionings, bool calculateFingerPos, bool backToStart = true, bool toolConfigActive = false)
	{
		throw new NotImplementedException();
	}

	public virtual void CalculateBendSteps(bool calculateFingerPos, bool backToStart = true, bool toolConfigActive = false)
	{
	}

	public void CheckConfig(IMessageDisplay messageDisplay)
	{
		try
		{
			bool flag = false;
			bool flag2 = false;
			bool flag3 = false;
			StringBuilder stringBuilder = new StringBuilder();
			foreach (IGrouping<AxisType, Axis> item in from v in this.BendMachine.Velocities
				select v.Axis into x
				group x by x.Type into g
				where g.Count() > 1
				select g)
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder3 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(28, 2, stringBuilder2);
				handler.AppendLiteral("Axis of Type ");
				handler.AppendFormatted(item.Key);
				handler.AppendLiteral(" exists ");
				handler.AppendFormatted(item.Count());
				handler.AppendLiteral(" times.");
				stringBuilder3.AppendLine(ref handler);
			}
			foreach (Axis item2 in this.BendMachine.Velocities.Select((AxisVelocity v) => v.Axis))
			{
				if (item2.Speed <= 0.0 || item2.Speeds.Any((SpeedItem x) => x.Speed <= 0.0))
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder4 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(26, 1, stringBuilder2);
					handler.AppendLiteral("Axis ");
					handler.AppendFormatted(item2.Type);
					handler.AppendLiteral(" has invalid speed(s)");
					stringBuilder4.AppendLine(ref handler);
				}
			}
			List<DieProfile> list = this.BendMachine.Dies.DieProfiles.Where((DieProfile x) => x.VWidthType == VWidthTypes.Undefined).ToList();
			if (list.Count > 0)
			{
				flag3 = true;
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder5 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(53, 2, stringBuilder2);
				handler.AppendLiteral("Die Profile ");
				handler.AppendFormatted(list.First().Name);
				handler.AppendLiteral(" and ");
				handler.AppendFormatted(list.Count - 1);
				handler.AppendLiteral(" others have no VWidth Type defined!");
				stringBuilder5.AppendLine(ref handler);
			}
			foreach (IGrouping<int, DiePart> item3 in (from x in this.BendMachine.Dies.DieParts
				group x by x.ID into g
				where g.Count() > 1
				select g).ToList())
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder6 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(27, 2, stringBuilder2);
				handler.AppendLiteral("Die Part ID ");
				handler.AppendFormatted(item3.Key);
				handler.AppendLiteral(" exists ");
				handler.AppendFormatted(item3.Count());
				handler.AppendLiteral(" times.");
				stringBuilder6.AppendLine(ref handler);
				flag = true;
			}
			foreach (IGrouping<int, PunchPart> item4 in (from x in this.BendMachine.Punches.PunchParts
				group x by x.ID into g
				where g.Count() > 1
				select g).ToList())
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder7 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(29, 2, stringBuilder2);
				handler.AppendLiteral("Punch Part ID ");
				handler.AppendFormatted(item4.Key);
				handler.AppendLiteral(" exists ");
				handler.AppendFormatted(item4.Count());
				handler.AppendLiteral(" times.");
				stringBuilder7.AppendLine(ref handler);
				flag = true;
			}
			foreach (IGrouping<int, DieProfile> item5 in (from x in this.BendMachine.Dies.DieProfiles
				group x by x.ID into g
				where g.Count() > 1
				select g).ToList())
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder8 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(30, 2, stringBuilder2);
				handler.AppendLiteral("Die Profile ID ");
				handler.AppendFormatted(item5.Key);
				handler.AppendLiteral(" exists ");
				handler.AppendFormatted(item5.Count());
				handler.AppendLiteral(" times.");
				stringBuilder8.AppendLine(ref handler);
			}
			foreach (IGrouping<int, PunchProfile> item6 in (from x in this.BendMachine.Punches.PunchProfiles
				group x by x.ID into g
				where g.Count() > 1
				select g).ToList())
			{
				StringBuilder stringBuilder2 = stringBuilder;
				StringBuilder stringBuilder9 = stringBuilder2;
				StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(32, 2, stringBuilder2);
				handler.AppendLiteral("Punch Profile ID ");
				handler.AppendFormatted(item6.Key);
				handler.AppendLiteral(" exists ");
				handler.AppendFormatted(item6.Count());
				handler.AppendLiteral(" times.");
				stringBuilder9.AppendLine(ref handler);
			}
			Dictionary<int, List<PunchProfile>> dictionary = (from x in this.BendMachine.Punches.PunchProfiles
				group x by x.ID).ToDictionary((IGrouping<int, PunchProfile> x) => x.Key, (IGrouping<int, PunchProfile> x) => x.ToList());
			Dictionary<int, List<DieProfile>> dictionary2 = (from x in this.BendMachine.Dies.DieProfiles
				group x by x.ID).ToDictionary((IGrouping<int, DieProfile> x) => x.Key, (IGrouping<int, DieProfile> x) => x.ToList());
			foreach (PunchPart punchPart2 in this.BendMachine.Punches.PunchParts)
			{
				if (!dictionary.ContainsKey(punchPart2.ProfileID))
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder10 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(53, 2, stringBuilder2);
					handler.AppendLiteral("Punch Tool id ");
					handler.AppendFormatted(punchPart2.ID);
					handler.AppendLiteral(" refers to profil ");
					handler.AppendFormatted(punchPart2.ProfileID);
					handler.AppendLiteral(" that doesn't exists.");
					stringBuilder10.AppendLine(ref handler);
					flag2 = true;
				}
			}
			foreach (DiePart diePart2 in this.BendMachine.Dies.DieParts)
			{
				if (!dictionary2.ContainsKey(diePart2.ProfileID))
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder11 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(51, 2, stringBuilder2);
					handler.AppendLiteral("Die Tool id ");
					handler.AppendFormatted(diePart2.ID);
					handler.AppendLiteral(" refers to profil ");
					handler.AppendFormatted(diePart2.ProfileID);
					handler.AppendLiteral(" that doesn't exists.");
					stringBuilder11.AppendLine(ref handler);
					flag2 = true;
				}
			}
			if (stringBuilder.Length == 0)
			{
				foreach (IGrouping<string, DieProfile> item7 in (from x in this.BendMachine.Dies.DieProfiles
					group x by x.Name into g
					where g.Count() > 1
					select g).ToList())
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder12 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(32, 2, stringBuilder2);
					handler.AppendLiteral("Die Profile Name ");
					handler.AppendFormatted(item7.Key);
					handler.AppendLiteral(" exists ");
					handler.AppendFormatted(item7.Count());
					handler.AppendLiteral(" times.");
					stringBuilder12.AppendLine(ref handler);
				}
				foreach (IGrouping<string, PunchProfile> item8 in (from x in this.BendMachine.Punches.PunchProfiles
					group x by x.Name into g
					where g.Count() > 1
					select g).ToList())
				{
					StringBuilder stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder13 = stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler = new StringBuilder.AppendInterpolatedStringHandler(34, 2, stringBuilder2);
					handler.AppendLiteral("Punch Profile Name ");
					handler.AppendFormatted(item8.Key);
					handler.AppendLiteral(" exists ");
					handler.AppendFormatted(item8.Count());
					handler.AppendLiteral(" times.");
					stringBuilder13.AppendLine(ref handler);
				}
			}
			if (stringBuilder.Length <= 0)
			{
				return;
			}
			messageDisplay.ShowWarningMessage("Corrupt Machine Config detected:" + Environment.NewLine + stringBuilder.ToString());
			if (!this._autoMode.PopupsEnabled)
			{
				return;
			}
			if (flag2 && MessageBox.Show($"Some Parts refer to Profils that don't exist.{Environment.NewLine}Do you want to delete all those Parts?{Environment.NewLine}{Environment.NewLine}If you agree with changes and want them to be persistent open machine config and safe.", "Cleanup", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
			{
				PunchPart[] array = this.BendMachine.Punches.PunchParts.ToArray();
				foreach (PunchPart punchPart in array)
				{
					if (!dictionary.ContainsKey(punchPart.ProfileID))
					{
						this.BendMachine.Punches.PunchParts.Remove(punchPart);
					}
				}
				DiePart[] array2 = this.BendMachine.Dies.DieParts.ToArray();
				foreach (DiePart diePart in array2)
				{
					if (!dictionary2.ContainsKey(diePart.ProfileID))
					{
						this.BendMachine.Dies.DieParts.Remove(diePart);
					}
				}
			}
			if (flag && MessageBox.Show($"Some Parts share the same ID.{Environment.NewLine}Do you want to generate new IDs for these Parts except one?{Environment.NewLine}{Environment.NewLine}If you agree with changes and want them to be persistent open machine config and safe.", "Change IDs", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) == MessageBoxResult.Yes)
			{
				int num = this.BendMachine.Dies.DieParts.Max((DiePart x) => x.ID) + 1;
				int num2 = this.BendMachine.Punches.PunchParts.Max((PunchPart x) => x.ID) + 1;
				foreach (IGrouping<int, DiePart> item9 in (from x in this.BendMachine.Dies.DieParts
					group x by x.ID into g
					where g.Count() > 1
					select g).ToList())
				{
					foreach (DiePart item10 in item9.Skip(1))
					{
						item10.ID = num++;
					}
				}
				foreach (IGrouping<int, PunchPart> item11 in (from x in this.BendMachine.Punches.PunchParts
					group x by x.ID into g
					where g.Count() > 1
					select g).ToList())
				{
					foreach (PunchPart item12 in item11.Skip(1))
					{
						item12.ID = num2++;
					}
				}
			}
			if (!flag3 || MessageBox.Show($"Some dies without VWidth Type.{Environment.NewLine}Do you want to apply BendMachine.PressBrakeInfo.Manufacturer Settings to all dies without VWidth Type?{Environment.NewLine}{Environment.NewLine}If you agree with changes and want them to be persistent open machine config and safe.", "Change VWidthType", MessageBoxButton.YesNo, MessageBoxImage.Question, MessageBoxResult.No) != MessageBoxResult.Yes)
			{
				return;
			}
			VWidthTypes vWidthType = VWidthTypes.BTrumpf;
			if (this.BendMachine.PressBrakeInfo.Manufacturer == "LVD" || this.BendMachine.PressBrakeInfo.Manufacturer == "Delem")
			{
				vWidthType = VWidthTypes.ALvdDelem;
			}
			foreach (DieProfile item13 in list)
			{
				item13.VWidthType = vWidthType;
			}
		}
		catch (Exception value)
		{
			Console.WriteLine(value);
			throw;
		}
	}
}
