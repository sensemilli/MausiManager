using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.Enums;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Machine.Tool;
using WiCAM.Pn4000.Common;
using WiCAM.Pn4000.PN3D.Popup.ViewModel;

namespace WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

public class ToolSettingsViewModel : ViewModelBase
{
	private ToolSettings _toolSettings;

	public ChangedConfigType Changed { get; set; }

	public BendingProcess BendProcessForLargeRadiusBends
	{
		get
		{
			return this._toolSettings.BendProcessForLargeRadiusBends;
		}
		set
		{
			this._toolSettings.BendProcessForLargeRadiusBends = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("BendProcessForLargeRadiusBends");
		}
	}

	public BendingProcess BendProcessFor90DegreeBends
	{
		get
		{
			return this._toolSettings.BendProcessFor90DegreeBends;
		}
		set
		{
			this._toolSettings.BendProcessFor90DegreeBends = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("BendProcessFor90DegreeBends");
		}
	}

	public BendingProcess BendProcessForOtherBends
	{
		get
		{
			return this._toolSettings.BendProcessForOtherBends;
		}
		set
		{
			this._toolSettings.BendProcessForOtherBends = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("BendProcessForOtherBends");
		}
	}

	public bool AlwaysUseBendProcessForOtherBendsIfPartHasSharpBends
	{
		get
		{
			return this._toolSettings.AlwaysUseBendProcessForOtherBendsIfPartHasSharpBends;
		}
		set
		{
			this._toolSettings.AlwaysUseBendProcessForOtherBendsIfPartHasSharpBends = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("AlwaysUseBendProcessForOtherBendsIfPartHasSharpBends");
		}
	}

	public bool IgnorePunchMarks
	{
		get
		{
			return this._toolSettings.IgnorePunchMarks;
		}
		set
		{
			this._toolSettings.IgnorePunchMarks = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("IgnorePunchMarks");
		}
	}

	public bool IgnoreNonParallelEdges
	{
		get
		{
			return this._toolSettings.IgnoreNonParallelEdges;
		}
		set
		{
			this._toolSettings.IgnoreNonParallelEdges = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("IgnoreNonParallelEdges");
		}
	}

	public Side PunchDieAlignment
	{
		get
		{
			return this._toolSettings.PunchDieAlignment;
		}
		set
		{
			this._toolSettings.PunchDieAlignment = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("PunchDieAlignment");
		}
	}

	public bool SynchronizePunchWithDie
	{
		get
		{
			return this._toolSettings.SynchronizePunchWithDie;
		}
		set
		{
			this._toolSettings.SynchronizePunchWithDie = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("SynchronizePunchWithDie");
		}
	}

	public Side UpperAdapterAlignment
	{
		get
		{
			return this._toolSettings.UpperAdapterAlignment;
		}
		set
		{
			this._toolSettings.UpperAdapterAlignment = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("UpperAdapterAlignment");
		}
	}

	public Side LowerAdapterAlignment
	{
		get
		{
			return this._toolSettings.LowerAdapterAlignment;
		}
		set
		{
			this._toolSettings.LowerAdapterAlignment = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("LowerAdapterAlignment");
		}
	}

	public Side ToolSetupAlignment
	{
		get
		{
			return this._toolSettings.ToolSetupAlignment;
		}
		set
		{
			this._toolSettings.ToolSetupAlignment = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("ToolSetupAlignment");
		}
	}

	public double GapBetweenToolSetups
	{
		get
		{
			return this._toolSettings.GapBetweenToolSetups;
		}
		set
		{
			this._toolSettings.GapBetweenToolSetups = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("GapBetweenToolSetups");
		}
	}

	public double SecurityOffsetBetweenSetups
	{
		get
		{
			return this._toolSettings.SecurityOffsetBetweenSetups;
		}
		set
		{
			this._toolSettings.SecurityOffsetBetweenSetups = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("SecurityOffsetBetweenSetups");
		}
	}

	public double PrefPunchOverlength
	{
		get
		{
			return this._toolSettings.PrefPunchOverlength;
		}
		set
		{
			this._toolSettings.PrefPunchOverlength = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("PrefPunchOverlength");
		}
	}

	public double PrefDieOverlength
	{
		get
		{
			return this._toolSettings.PrefDieOverlength;
		}
		set
		{
			this._toolSettings.PrefDieOverlength = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("PrefDieOverlength");
		}
	}

	public bool UseSameProfileForAllBends
	{
		get
		{
			return this._toolSettings.UseSameProfileForAllBends;
		}
		set
		{
			this._toolSettings.UseSameProfileForAllBends = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("UseSameProfileForAllBends");
		}
	}

	public bool LastHopeAllowDifferentWorkingHeights
	{
		get
		{
			return this._toolSettings.LastHopeAllowDifferentWorkingHeights;
		}
		set
		{
			this._toolSettings.LastHopeAllowDifferentWorkingHeights = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("LastHopeAllowDifferentWorkingHeights");
		}
	}

	public bool OnlySelectAvailableTools
	{
		get
		{
			return this._toolSettings.OnlySelectAvailableTools;
		}
		set
		{
			this._toolSettings.OnlySelectAvailableTools = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("OnlySelectAvailableTools");
		}
	}

	public bool OnlySelectMappedTools
	{
		get
		{
			return this._toolSettings.OnlySelectMappedTools;
		}
		set
		{
			this._toolSettings.OnlySelectMappedTools = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("OnlySelectMappedTools");
		}
	}

	public bool OnlySelectPredefinedToolCombinations
	{
		get
		{
			return this._toolSettings.OnlySelectPredefinedToolCombinations;
		}
		set
		{
			this._toolSettings.OnlySelectPredefinedToolCombinations = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("OnlySelectPredefinedToolCombinations");
		}
	}

	public int CheckForCollisionsDuringToolSelection
	{
		get
		{
			return this._toolSettings.CheckForCollisionsDuringToolSelection;
		}
		set
		{
			this._toolSettings.CheckForCollisionsDuringToolSelection = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("CheckForCollisionsDuringToolSelection");
		}
	}

	public int UseRealToolGeometry
	{
		get
		{
			return this._toolSettings.UseRealToolGeometry;
		}
		set
		{
			this._toolSettings.UseRealToolGeometry = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("UseRealToolGeometry");
		}
	}

	public bool OptimizeNumberOfToolSetups
	{
		get
		{
			return this._toolSettings.OptimizeNumberOfToolSetups;
		}
		set
		{
			this._toolSettings.OptimizeNumberOfToolSetups = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("OptimizeNumberOfToolSetups");
		}
	}

	public bool UsePreferredToolProfiles
	{
		get
		{
			return this._toolSettings.UsePreferredToolProfiles;
		}
		set
		{
			this._toolSettings.UsePreferredToolProfiles = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("UsePreferredToolProfiles");
		}
	}

	public bool SelectPreferredToolProfilesForMostDifficultBend
	{
		get
		{
			return this._toolSettings.SelectPreferredToolProfilesForMostDifficultBend;
		}
		set
		{
			this._toolSettings.SelectPreferredToolProfilesForMostDifficultBend = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("SelectPreferredToolProfilesForMostDifficultBend");
		}
	}

	public bool UseStoredToolTechnology
	{
		get
		{
			return this._toolSettings.UseStoredToolTechnology;
		}
		set
		{
			this._toolSettings.UseStoredToolTechnology = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("UseStoredToolTechnology");
		}
	}

	public bool UseToolStations
	{
		get
		{
			return this._toolSettings.UseToolStations;
		}
		set
		{
			this._toolSettings.UseToolStations = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("UseToolStations");
		}
	}

	public bool CheckOverlapPunchWithBend
	{
		get
		{
			return this._toolSettings.CheckOverlapPunchWithBend;
		}
		set
		{
			this._toolSettings.CheckOverlapPunchWithBend = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("CheckOverlapPunchWithBend");
		}
	}

	public bool FillGaps
	{
		get
		{
			return this._toolSettings.FillGaps;
		}
		set
		{
			this._toolSettings.FillGaps = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("FillGaps");
		}
	}

	public double PreferredBendMargin
	{
		get
		{
			return this._toolSettings.PreferredBendMargin;
		}
		set
		{
			this._toolSettings.PreferredBendMargin = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("PreferredBendMargin");
		}
	}

	public double PreferredToolCollisionMargin
	{
		get
		{
			return this._toolSettings.PreferredToolCollisionMargin;
		}
		set
		{
			this._toolSettings.PreferredToolCollisionMargin = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("PreferredToolCollisionMargin");
		}
	}

	public double MaxBendReductionPercentage
	{
		get
		{
			return this._toolSettings.MaxBendReductionPercentage;
		}
		set
		{
			this._toolSettings.MaxBendReductionPercentage = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("MaxBendReductionPercentage");
		}
	}

	public double MaxBendReductionMM
	{
		get
		{
			return this._toolSettings.MaxBendReductionMM;
		}
		set
		{
			this._toolSettings.MaxBendReductionMM = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("MaxBendReductionMM");
		}
	}

	public double ToolStationOptimizationParam1
	{
		get
		{
			return this._toolSettings.ToolStationOptimizationParam1;
		}
		set
		{
			this._toolSettings.ToolStationOptimizationParam1 = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("ToolStationOptimizationParam1");
		}
	}

	public bool AllowHeelToolsInSetup
	{
		get
		{
			return this._toolSettings.AllowHeelToolsInSetup;
		}
		set
		{
			this._toolSettings.AllowHeelToolsInSetup = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("AllowHeelToolsInSetup");
		}
	}

	public bool AllowFlipTools
	{
		get
		{
			return this._toolSettings.AllowFlipTools;
		}
		set
		{
			this._toolSettings.AllowFlipTools = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("AllowFlipTools");
		}
	}

	public double MaxGapToolsToBend
	{
		get
		{
			return this._toolSettings.MaxGapToolsToBend;
		}
		set
		{
			this._toolSettings.MaxGapToolsToBend = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("MaxGapToolsToBend");
		}
	}

	public double? MaxGapLegCoverageToBend
	{
		get
		{
			return this._toolSettings.MaxGapLegCoverageToBend;
		}
		set
		{
			this._toolSettings.MaxGapLegCoverageToBend = value;
			this.Changed = ChangedConfigType.Tools;
			base.NotifyPropertyChanged("MaxGapLegCoverageToBend");
		}
	}

	public ToolSettingsViewModel(ToolSettings toolSettings)
	{
		this._toolSettings = toolSettings;
	}
}
