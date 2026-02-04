using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using WiCAM.Pn4000.BendModel.Serialization.SerializationStructure;
using WiCAM.Pn4000.BendModel.Serialization.SerializationStructure.FingerStops;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SPnBndDoc : SDocBase
{
	public SPnBndFile DiskFile { get; set; }

	public bool UseDINUnfold { get; set; }

	public SMaterial Material { get; set; }

	public SModel InputModel { get; set; }

	public SModel EntryModel { get; set; }

	[JsonProperty(NullValueHandling = NullValueHandling.Ignore)]
	public SBndMachineExpert BendMachine { get; set; }

	public SBendToolExpert Tools { get; set; }

	public SToolsAndBends? ToolsAndBends { get; set; }

	public SPreferredProfileStore PreferredProfileStore { get; set; }

	[JsonProperty(ItemTypeNameHandling = TypeNameHandling.Objects)]
	public List<SBendDescriptor> BendDescriptors { get; set; } = new List<SBendDescriptor>();

	public List<SCombinedBendDescriptor> CombinedBendDescriptors { get; set; } = new List<SCombinedBendDescriptor>();

	public bool FreezeCombinedBendDescriptors { get; set; }

	public double Thickness { get; set; }

	public int VisibleFaceGroupId { get; set; }

	public int VisibleFaceGroupSide { get; set; }

	public bool IsReconstructed { get; set; }

	public int AmountInAssembly { get; set; }

	public string Comment { get; set; }

	public string NamePP { get; set; }

	public List<string> NamesPp { get; set; } = new List<string>();

	public int? NumberPp { get; set; }

	public Dictionary<int, string> UserCommentsInt { get; set; } = new Dictionary<int, string>();

	[JsonProperty(ItemTypeNameHandling = TypeNameHandling.Objects)]
	public Dictionary<int, SStopPosition> FingerStopPositionData { get; set; }

	[JsonProperty(ItemTypeNameHandling = TypeNameHandling.Objects)]
	public Dictionary<int, SStopFaceDescriptor> FingerStopFaceDescriptorData { get; set; }

	public List<SSimulationInstance> SimulationInstancesAdditionalParts { get; set; }

	public bool PnMaterialByUser { get; set; }

	public bool KFactorWarningsAcceptedByUser { get; set; }

	public string AssemblyGuid { get; set; }

	public string CreationUserName { get; set; }

	public string LastModifiedUserName { get; set; }

	public DateTime CreationDate { get; set; }

	public DateTime LastModified { get; set; }

	public string Classification { get; set; }

	public string DrawingNumber { get; set; }

	public SPnBndDoc()
	{
		base.Version = 2;
	}

	public new static SPnBndDoc Convert(IDoc3d doc)
	{
		throw new Exception("try to Convert doc to old serialize version 1. Please Convert to current version");
	}

	public override SDocBase GetUpdatedDoc()
	{
		return this;
	}
}
