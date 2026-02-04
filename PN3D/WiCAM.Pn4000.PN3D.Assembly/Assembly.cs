using System;
using System.Collections.Generic;
using System.Linq;
using WiCAM.Pn4000.BendModel.Base;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Config.DataStructures;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.PN3D.Unfold;

namespace WiCAM.Pn4000.PN3D.Assembly;

public class Assembly
{
	public enum EnumLoadingStatus
	{
		Initialized = 0,
		SpatialRunning = 1,
		SpatialFinished = 2,
		AssemblyStructureRead = 3,
		LowTesselationLoaded = 4,
		AnalyzedHd = 5
	}

	private EnumLoadingStatus _loadingStatus = EnumLoadingStatus.SpatialRunning;

	public int MajorVersion { get; set; }

	public int MinorVersion { get; set; }

	public string RootPartName { get; set; }

	public string FilenameImport { get; set; }

	public string Guid { get; set; }

	public List<DisassemblyPart> DisassemblyParts { get; set; } = new List<DisassemblyPart>();

	public DisassemblyPartNode RootNode { get; set; }

	public EnumLoadingStatus LoadingStatus
	{
		get
		{
			return this._loadingStatus;
		}
		set
		{
			this._loadingStatus = value;
			this.OnStatusChanged?.Invoke(value);
		}
	}

	public F2exeReturnCode ProcessCode { get; set; }

	public int? LastOpenedPartId { get; set; }

	public event Action<EnumLoadingStatus> OnStatusChanged;

	public event Action<SpatialImportProgress> OnSpatialProgress;

	public Assembly()
	{
		this.Guid = global::System.Guid.NewGuid().ToString();
	}

	public void SetDefaultMaterialsCalcPositions(IImportArg importSetting, IImportMaterialMapper importMaterialMapper, int matIdFallback, General3DConfig general3DConfig)
	{
		if (this.LoadingStatus >= EnumLoadingStatus.AssemblyStructureRead)
		{
			return;
		}
		foreach (DisassemblyPart disassemblyPart in this.DisassemblyParts)
		{
			int num = importSetting.MaterialNumber ?? (-1);
			if (!string.IsNullOrEmpty(disassemblyPart.OriginalMaterialName) && (num < 0 || !importSetting.MaterialByForce))
			{
				int materialId = importMaterialMapper.GetMaterialId(disassemblyPart.OriginalMaterialName);
				if (materialId >= 0)
				{
					num = materialId;
				}
			}
			if (num < 0)
			{
				num = matIdFallback;
				disassemblyPart.PnMaterialByUser = false;
			}
			else
			{
				disassemblyPart.PnMaterialByUser = true;
			}
			disassemblyPart.PnMaterialID = num;
			int? machineNo = importSetting.MachineNumber;
			if (!machineNo.HasValue && general3DConfig.P3D_UseDefaultMachine)
			{
				machineNo = general3DConfig.P3D_Default_MachineId;
			}
			disassemblyPart.MachineNo = machineNo;
		}
		foreach (DisassemblyPartNode item in new DisassemblyPartNode[1] { this.RootNode }.SelectAndManyRecursive((DisassemblyPartNode x) => x.Children))
		{
			if (item.Part != null)
			{
				DisassemblyPart part = item.Part;
				if (part.Matrixes == null)
				{
					List<Matrix4d> list2 = (part.Matrixes = new List<Matrix4d>());
				}
				item.Part.Matrixes.Add(item.WorldMatrix);
				part = item.Part;
				if (part.Instances == null)
				{
					List<string> list4 = (part.Instances = new List<string>());
				}
				item.Part.Instances.Add(item.Path());
			}
		}
	}

	public T GetAutoChoseSinglePart<T>(IImportArg importSetting, IEnumerable<T> ListParts, Func<T, DisassemblyPart> convert)
	{
		if (importSetting.OpenSingleParts)
		{
			if (ListParts.Count() == 1)
			{
				return ListParts.First();
			}
			if (importSetting.SpecialPartSelectionMode == IImportArg.SpecialPartSelectionModes.Flat)
			{
				List<(T, DisassemblyPart)> list = (from x in ListParts
					select (obj: x, part: convert(x)) into x
					where !x.part.IsAdditionalPart && !x.part.PartInfo.PartType.HasFlag(PartType.SmallPart)
					where x.part.PartInfo.PartType.HasFlag(PartType.FlatSheetMetal)
					select x).ToList();
				if (list.Count == 1)
				{
					return list.First().Item1;
				}
			}
			else if (importSetting.SpecialPartSelectionMode == IImportArg.SpecialPartSelectionModes.NotFlat)
			{
				List<(T, DisassemblyPart)> list2 = (from x in ListParts
					select (obj: x, part: convert(x)) into x
					where !x.part.IsAdditionalPart && !x.part.PartInfo.PartType.HasFlag(PartType.SmallPart)
					where x.part.PartInfo.PartType.HasFlag(PartType.UnfoldableSheetMetal) || x.part.PartInfo.PartType.HasFlag(PartType.Unknown)
					select x).ToList();
				if (list2.Count == 1)
				{
					return list2.First().Item1;
				}
			}
		}
		return default(T);
	}

	public void RaiseOnSpatialProgress(SpatialImportProgress model)
	{
		this.OnSpatialProgress?.Invoke(model);
	}
}
