using System;
using System.Collections.Generic;
using System.Linq;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Holder;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;
using WiCAM.Pn4000.Contracts.BendDataBase;
using WiCAM.Pn4000.Contracts.Tools;
using WiCAM.Pn4000.PN3D.CommonBend;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Tool.Interfaces;
using WiCAM.Pn4000.PN3D.Unfold;

namespace WiCAM.Pn4000.PN3D.Tool;

public class BendToolExpert : IToolExpert
{
	private readonly IGlobals _globals;

	private IDoc3d _doc;

	public BendToolExpert(IGlobals globals)
	{
		this._globals = globals;
	}

	public void Init(IDoc3d doc)
	{
		this._doc = doc;
	}

	public IPreferredProfile GetSuggestedTools(ICombinedBendDescriptor cbd)
	{
		ToolSelectionType toolSelectionType;
		IPreferredProfile preferredProfileForFaceGroup = this._doc.PreferredProfileStore.GetPreferredProfileForFaceGroup(cbd[0].BendParams.EntryFaceGroup.ID, cbd[0].BendParams.UnfoldFaceGroup.SubBendIndex.GetValueOrDefault(), out toolSelectionType);
		if (toolSelectionType == ToolSelectionType.SuggestedTools || toolSelectionType == ToolSelectionType.UserSelectedTools)
		{
			return preferredProfileForFaceGroup;
		}
		return null;
	}

	public IPreferredProfile GetPreferredTools(ICombinedBendDescriptor cbd)
	{
		ToolSelectionType toolSelectionType;
		IPreferredProfile preferredProfileForFaceGroup = this._doc.PreferredProfileStore.GetPreferredProfileForFaceGroup(cbd[0].BendParams.EntryFaceGroup.ID, cbd[0].BendParams.UnfoldFaceGroup.SubBendIndex.GetValueOrDefault(), out toolSelectionType);
		if (toolSelectionType == ToolSelectionType.PreferredTools || toolSelectionType == ToolSelectionType.UserSelectedTools)
		{
			return preferredProfileForFaceGroup;
		}
		return null;
	}

	public IPreferredProfile GetToolGroupsForBend(ICombinedBendDescriptorInternal cbd)
	{
		throw new NotImplementedException();
	}

	public IPunchGroup GetPunchGroupForFaceGroup(ICombinedBendDescriptorInternal cbd, out NoToolsFoundReason reason)
	{
		this.AutoCalcProfilesForCommonBend(cbd, out var upperProfiles, out var _, out var _, out var _, out reason);
		if (upperProfiles.Count == 0)
		{
			return null;
		}
		IPunchProfile pp = upperProfiles.First();
		return this._doc.BendMachine.ToolConfig.UpperGroups.First((IPunchGroup x) => x.ID == pp.GroupID);
	}

	public IDieGroup GetDieGroupForFaceGroup(ICombinedBendDescriptorInternal cbd, out NoToolsFoundReason reason)
	{
		this.AutoCalcProfilesForCommonBend(cbd, out var _, out var lowerProfiles, out var _, out var _, out reason);
		if (lowerProfiles.Count == 0)
		{
			return null;
		}
		IDieProfile dp = lowerProfiles.First();
		return this._doc.BendMachine.ToolConfig.LowerGroups.First((IDieGroup x) => x.ID == dp.GroupID);
	}

	public double GetHemSplitAngle(ICombinedBendDescriptor cbd)
	{
		return 2.5307274153917776;
	}

	private double CalcBendTableEntryDistance(IBendTableItem bte, int material3dgroup, double thickness, double radius, double angle, double radiusTolerance, out BendTableMatch btm)
	{
		double num = 0.0;
		btm = new BendTableMatch();
		if (bte.Material3DGroupID.HasValue)
		{
			if (material3dgroup != bte.Material3DGroupID.Value)
			{
				btm.Mismatches.Add(ValueMismatch.Material);
			}
		}
		else
		{
			num += 10.0;
		}
		if (bte.Thickness.HasValue)
		{
			if (Math.Abs(bte.Thickness.Value - thickness) > 0.5)
			{
				btm.Mismatches.Add(ValueMismatch.Thickness);
			}
			else
			{
				num += Math.Abs(bte.Thickness.Value - thickness) * 100.0;
			}
		}
		if (bte.Angle.HasValue)
		{
			num += Math.Abs(bte.Angle.Value - Math.Abs(angle)) * 15.0;
		}
		if (bte.R.HasValue)
		{
			if (Math.Abs(bte.R.Value - radius) > global::WiCAM.Pn4000.PN3D.Unfold.Unfold.GetRadiusTolerance(thickness, radius, this._globals.ConfigProvider.InjectOrCreate<General3DConfig>()))
			{
				return double.MaxValue;
			}
			num += Math.Abs(bte.R.Value - radius) * 15.0;
		}
		if (btm.Mismatches.Count > 0)
		{
			return double.MaxValue;
		}
		return num;
	}

	private List<IBendTableItem> GetMatchingBendTableEntries(IBendDescriptor bendDesc, out ValueMismatch vcm)
	{
		int material3dGroup = (this._doc.Material?.MaterialGroupForBendDeduction).Value;
		double thickness = Math.Round(this._doc.Thickness, 5);
		vcm = ValueMismatch.None;
		bool isMachineSpecific;
		List<IBendTableItem> source = this._doc.GetApplicableBendTable(out isMachineSpecific).GetEntries().ToList();
		General3DConfig pnGeneral3DConfig = this._globals.ConfigProvider.InjectOrCreate<General3DConfig>();
		double radius = bendDesc.BendParams.ManualRadius ?? bendDesc.BendParams.OriginalRadius;
		radius = Math.Round(radius, 5);
		double radiusTolerance = Math.Round(global::WiCAM.Pn4000.PN3D.Unfold.Unfold.GetRadiusTolerance(thickness, radius, pnGeneral3DConfig), 5);
		double angle = Math.Round(bendDesc.BendParams.AngleAbs * 180.0 / Math.PI, 5);
		BendTableMatch btm;
		List<(IBendTableItem, double, BendTableMatch)> list = (from x in source
			select (bte: x, cost: this.CalcBendTableEntryDistance(x, material3dGroup, thickness, radius, angle, radiusTolerance, out btm), btm: btm) into x
			orderby x.cost
			select x).ToList();
		List<IBendTableItem> list2 = (from x in list
			where x.Item2 < double.MaxValue
			select x.Item1).ToList();
		if (list2.Count == 0 && source.Any())
		{
			if (list.Where<(IBendTableItem, double, BendTableMatch)>(((IBendTableItem bte, double cost, BendTableMatch btm) x) => x.btm.Mismatches.Contains(ValueMismatch.Material)).Count() == list.Count)
			{
				vcm = ValueMismatch.Material;
				return list2;
			}
			if (list.Count<(IBendTableItem, double, BendTableMatch)>(((IBendTableItem bte, double cost, BendTableMatch btm) x) => x.btm.Mismatches.Contains(ValueMismatch.Thickness)) == list.Count)
			{
				vcm = ValueMismatch.Thickness;
				return list2;
			}
			if (list.Any<(IBendTableItem, double, BendTableMatch)>(((IBendTableItem bte, double cost, BendTableMatch btm) x) => x.btm.Mismatches.Count == 1 && x.btm.Mismatches.Contains(ValueMismatch.Radius)))
			{
				vcm = ValueMismatch.Radius;
				return list2;
			}
			vcm = ValueMismatch.Unknown;
		}
		return list2;
	}

	private double CalcBendOpeningAngle(ICombinedBendDescriptor cbd)
	{
		return 180.0 - cbd.StopProductAngleAbs;
	}

	private IBendTableItem AutoCalcProfilesForRegularBend(ICombinedBendDescriptor cbd, out List<IPunchProfile> upperProfiles, out List<IDieProfile> lowerProfiles, out List<HolderProfile> upperHolderProfiles, out List<HolderProfile> lowerHolderProfiles, out NoToolsFoundReason reason)
	{
		IBendTableItem result = null;
		string material = this._doc.Material?.Name;
		ValueMismatch vcm;
		List<IBendTableItem> matchingBendTableEntries = this.GetMatchingBendTableEntries(cbd[0], out vcm);
		upperProfiles = new List<IPunchProfile>();
		lowerProfiles = new List<IDieProfile>();
		upperHolderProfiles = new List<HolderProfile>();
		lowerHolderProfiles = new List<HolderProfile>();
		if (matchingBendTableEntries.Count == 0)
		{
			reason = new NoToolsFoundReason(NoToolsFoundReasonType.NoBendTableEntry, cbd.StopProductAngleAbs, cbd[0].BendParams.OriginalRadius, material, vcm);
			return null;
		}
		List<IPunchProfile> list = null;
		list = this.PrefilterPunchesForRegularBend(cbd, this._doc.BendMachine.ToolConfig.UpperTools);
		if (list.Count == 0)
		{
			reason = new NoToolsFoundReason(NoToolsFoundReasonType.NoUpperToolWithRightVAngle, cbd.StopProductAngleAbs, cbd[0].BendParams.OriginalRadius, material, vcm);
			return result;
		}
		if (list.Count == 0)
		{
			reason = new NoToolsFoundReason(NoToolsFoundReasonType.NoUpperToolWithRightWorkingHeight, cbd.StopProductAngleAbs, cbd[0].BendParams.OriginalRadius, material, vcm);
			return result;
		}
		List<IDieProfile> list2 = this.PrefilterDiesForRegularBend(cbd, this._doc.BendMachine.ToolConfig.LowerTools);
		if (list2.Count == 0)
		{
			reason = new NoToolsFoundReason(NoToolsFoundReasonType.NoLowerToolWithRightVAngle, cbd.StopProductAngleAbs, cbd[0].BendParams.OriginalRadius, material, vcm);
			return result;
		}
		foreach (IBendTableItem bte in matchingBendTableEntries)
		{
			result = bte;
			if (bte.PunchRadius.HasValue)
			{
				double minRadiusDist = list.Select((IPunchProfile x) => Math.Abs(x.Radius.Value - bte.PunchRadius.Value)).Min();
				IPunchProfile pp = list.First((IPunchProfile x) => Math.Abs(x.Radius.Value - bte.PunchRadius.Value) == minRadiusDist);
				upperProfiles = (from x in list
					where x.GroupID == pp.GroupID
					orderby x.Priority
					select x).ToList();
			}
			if (bte.VWidth.HasValue)
			{
				double minVWidthDist = list2.Select((IDieProfile x) => Math.Abs(x.VWidth.Value - bte.VWidth.Value)).Min();
				IDieProfile dp = list2.First((IDieProfile x) => Math.Abs(x.VWidth.Value - bte.VWidth.Value) == minVWidthDist);
				lowerProfiles = (from x in list2
					where x.GroupID == dp.GroupID && x.VWidth >= bte.VWidth.Value
					orderby x.Priority
					select x).ToList();
			}
			if (upperProfiles.Count != 0 && lowerProfiles.Count != 0)
			{
				break;
			}
		}
		reason = new NoToolsFoundReason();
		return result;
	}

	private List<IPunchProfile> PrefilterPunchesForRegularBend(ICombinedBendDescriptor cbd, IEnumerable<IPunchProfile> candidates)
	{
		if (cbd.BendType == CombinedBendType.Bend)
		{
			return candidates.Where((IPunchProfile x) => x.AngleDeg < this.CalcBendOpeningAngle(cbd)).ToList();
		}
		return new List<IPunchProfile>();
	}

	private List<IDieProfile> PrefilterDiesForRegularBend(ICombinedBendDescriptor cbd, IEnumerable<IDieProfile> candidates)
	{
		if (cbd.BendType == CombinedBendType.Bend)
		{
			return candidates.Where((IDieProfile x) => x.VAngleDeg <= this.CalcBendOpeningAngle(cbd)).ToList();
		}
		return new List<IDieProfile>();
	}

	private IBendTableItem AutoCalcProfilesForCommonBend(ICombinedBendDescriptor cbd, out List<IPunchProfile> upperProfiles, out List<IDieProfile> lowerProfiles, out List<HolderProfile> upperHolderProfiles, out List<HolderProfile> lowerHolderProfiles, out NoToolsFoundReason reason)
	{
		if (cbd.StopProductAngleAbs > this.GetHemSplitAngle(cbd))
		{
			return this.AutoCalcProfilesForHemBend(cbd, out upperProfiles, out lowerProfiles, out upperHolderProfiles, out lowerHolderProfiles, out reason);
		}
		return this.AutoCalcProfilesForRegularBend(cbd, out upperProfiles, out lowerProfiles, out upperHolderProfiles, out lowerHolderProfiles, out reason);
	}

	private void GetPreferredToolsForCommonBend(ICombinedBendDescriptor cbd, out List<IPunchProfile> upperProfiles, out List<IDieProfile> lowerProfiles, out List<HolderProfile> upperHolderProfiles, out List<HolderProfile> lowerHolderProfiles)
	{
		throw new NotImplementedException();
	}

	private void GetUserForcedProfilesForCommonBend(ICombinedBendDescriptor cbd, out List<IPunchProfile> upperProfiles, out List<IDieProfile> lowerProfiles, out List<HolderProfile> upperHolderProfiles, out List<HolderProfile> lowerHolderProfiles)
	{
		upperProfiles = new List<IPunchProfile>();
		lowerProfiles = new List<IDieProfile>();
		upperHolderProfiles = new List<HolderProfile>();
		lowerHolderProfiles = new List<HolderProfile>();
		if (cbd.UserForcedUpperToolProfile.HasValue)
		{
			IPunchProfile item = this._doc.BendMachine.ToolConfig.UpperTools.FirstOrDefault((IPunchProfile x) => x.ID == cbd.UserForcedUpperToolProfile.Value);
			upperProfiles.Add(item);
		}
		if (cbd.UserForcedLowerToolProfile.HasValue)
		{
			IDieProfile item2 = this._doc.BendMachine.ToolConfig.LowerTools.FirstOrDefault((IDieProfile x) => x.ID == cbd.UserForcedLowerToolProfile.Value);
			lowerProfiles.Add(item2);
		}
	}

	private IBendTableItem AutoCalcProfilesForHemBend(ICombinedBendDescriptor cbd, out List<IPunchProfile> upperProfiles, out List<IDieProfile> lowerProfiles, out List<HolderProfile> upperHolderProfiles, out List<HolderProfile> lowerHolderProfiles, out NoToolsFoundReason reason)
	{
		IBendTableItem result = null;
		string material = this._doc.Material?.Name;
		upperProfiles = new List<IPunchProfile>();
		lowerProfiles = new List<IDieProfile>();
		upperHolderProfiles = new List<HolderProfile>();
		lowerHolderProfiles = new List<HolderProfile>();
		ValueMismatch vm = ValueMismatch.None;
		List<IPunchProfile> list = null;
		list = this._doc.BendMachine.ToolConfig.UpperTools.Where((IPunchProfile x) => x.WorkingHeight != 0.0).ToList();
		if (list.Count == 0)
		{
			reason = new NoToolsFoundReason(NoToolsFoundReasonType.NoUpperToolForHemming, cbd.StopProductAngleAbs, cbd[0].BendParams.OriginalRadius, material, vm);
			return result;
		}
		List<IDieProfile> list2 = null;
		list2 = this._doc.BendMachine.ToolConfig.LowerTools.Where((IDieProfile x) => x.WorkingHeight != 0.0).ToList();
		if (list2.Count == 0)
		{
			reason = new NoToolsFoundReason(NoToolsFoundReasonType.NoLowerToolForHemming, cbd.StopProductAngleAbs, cbd[0].BendParams.OriginalRadius, material, vm);
			return result;
		}
		if (list2.Count == 0)
		{
			reason = new NoToolsFoundReason(NoToolsFoundReasonType.NoLowerToolWithRightVAngle, cbd.StopProductAngleAbs, cbd[0].BendParams.OriginalRadius, material, vm);
			return result;
		}
		upperProfiles = list;
		lowerProfiles = list2;
		reason = null;
		return null;
	}
}
