using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.Assembly.Doc;
using WiCAM.Pn4000.Contracts.PnPathServices;
using WiCAM.Pn4000.PN3D.Interfaces;

namespace WiCAM.Pn4000.PN3D.Assembly;

public class DocMetadata : IDocMetadata
{
	public Dictionary<int, string> UserComments { get; set; }

	public double LenUnfoldX { get; set; }

	public double LenUnfoldY { get; set; }

	public bool UseBendAid { get; set; }

	public List<string>? UpperToolNames { get; set; }

	public List<string>? LowerToolNames { get; set; }

	public List<string>? UpperAdapterNames { get; set; }

	public List<string>? LowerAdapterNames { get; set; }

	public string CreationUserName { get; set; }

	public DateTime CreationDate { get; set; }

	public string LastModifiedUserName { get; set; }

	public DateTime LastModified { get; set; }

	public string AssemblyGuid { get; set; }

	public int MacroBlindHole { get; set; }

	public int MacroConicBlindHole { get; set; }

	public int MacroSphericalBlindHole { get; set; }

	public int MacroBolt { get; set; }

	public int MacroBorder { get; set; }

	public int MacroBridgeLance { get; set; }

	public int MacroChamfer { get; set; }

	public int MacroCounterSink { get; set; }

	public int MacroStepDrilling { get; set; }

	public int MacroDeepening { get; set; }

	public int MacroDummy { get; set; }

	public int MacroEmbossed { get; set; }

	public int MacroEmbossedCircle { get; set; }

	public int MacroEmbossedCounterSink { get; set; }

	public int MacroEmbossedFreeform { get; set; }

	public int MacroEmbossedLine { get; set; }

	public int MacroEmbossedRectangle { get; set; }

	public int MacroEmbossedRectangleRounded { get; set; }

	public int MacroEmbossedSquare { get; set; }

	public int MacroEmbossedSquareRounded { get; set; }

	public int MacroLance { get; set; }

	public int MacroLouver { get; set; }

	public int MacroManufacturingMacro { get; set; }

	public int MacroPressNut { get; set; }

	public int MacroSimpleHole { get; set; }

	public int MacroThread { get; set; }

	public int MacroTwoSidedCounterSink { get; set; }

	public int BendCount { get; set; }

	public double Thickness { get; set; }

	public GeometryType Dimension { get; set; }

	public double LenX { get; set; }

	public double LenY { get; set; }

	public double LenZ { get; set; }

	public PartType SheetType { get; set; }

	public int DocAssemblyId { get; set; }

	public string AssemblyName { get; set; }

	public bool KFactorWarnings { get; set; }

	public bool PnMaterialByUser { get; set; }

	public int? PnMaterialNo { get; set; }

	public int? BendMachineNo { get; set; }

	public string BendMachineDesc { get; set; }

	public int SavedArchivNo { get; set; }

	public string SavedArchivName { get; set; }

	public DocState DocState { get; set; }

	public int? ValidationGeoErrors { get; set; }

	public string PPName { get; set; }

	public int? CutoutsAdded { get; set; }

	public string Comment { get; set; }

	public void CopyFromMetaData(IDocMetadata data)
	{
		this.BendCount = data.BendCount;
		this.Thickness = data.Thickness;
		this.Dimension = data.Dimension;
		this.LenX = data.LenX;
		this.LenY = data.LenY;
		this.LenZ = data.LenZ;
		this.SheetType = data.SheetType;
		this.DocAssemblyId = data.DocAssemblyId;
		this.BendMachineDesc = data.BendMachineDesc;
		this.SavedArchivNo = data.SavedArchivNo;
		this.SavedArchivName = data.SavedArchivName;
		this.DocState = data.DocState;
		this.ValidationGeoErrors = data.ValidationGeoErrors;
		this.PPName = data.PPName;
		this.CutoutsAdded = data.CutoutsAdded;
		this.Comment = data.Comment;
		this.KFactorWarnings = data.KFactorWarnings;
		this.PnMaterialByUser = data.PnMaterialByUser;
		this.PnMaterialNo = data.PnMaterialNo;
		this.BendMachineNo = data.BendMachineNo;
		this.UserComments = data.UserComments.ToDictionary((KeyValuePair<int, string> x) => x.Key, (KeyValuePair<int, string> x) => x.Value);
		this.LenUnfoldX = data.LenUnfoldX;
		this.LenUnfoldY = data.LenUnfoldY;
		this.UseBendAid = data.UseBendAid;
		this.UpperToolNames = data.UpperToolNames?.ToList();
		this.LowerToolNames = data.LowerToolNames?.ToList();
		this.UpperAdapterNames = data.UpperAdapterNames?.ToList();
		this.LowerAdapterNames = data.LowerAdapterNames?.ToList();
		this.CreationUserName = data.CreationUserName;
		this.CreationDate = data.CreationDate;
		this.LastModifiedUserName = data.LastModifiedUserName;
		this.LastModified = data.LastModified;
		this.AssemblyGuid = data.AssemblyGuid;
		this.MacroBlindHole = data.MacroBlindHole;
		this.MacroBolt = data.MacroBolt;
		this.MacroBorder = data.MacroBorder;
		this.MacroBridgeLance = data.MacroBridgeLance;
		this.MacroChamfer = data.MacroChamfer;
		this.MacroCounterSink = data.MacroCounterSink;
		this.MacroDeepening = data.MacroDeepening;
		this.MacroDummy = data.MacroDummy;
		this.MacroEmbossed = data.MacroEmbossed;
		this.MacroEmbossedCircle = data.MacroEmbossedCircle;
		this.MacroEmbossedCounterSink = data.MacroEmbossedCounterSink;
		this.MacroEmbossedFreeform = data.MacroEmbossedFreeform;
		this.MacroEmbossedLine = data.MacroEmbossedLine;
		this.MacroEmbossedRectangle = data.MacroEmbossedRectangle;
		this.MacroEmbossedRectangleRounded = data.MacroEmbossedRectangleRounded;
		this.MacroEmbossedSquare = data.MacroEmbossedSquare;
		this.MacroEmbossedSquareRounded = data.MacroEmbossedSquareRounded;
		this.MacroLance = data.MacroLance;
		this.MacroLouver = data.MacroLouver;
		this.MacroManufacturingMacro = data.MacroManufacturingMacro;
		this.MacroPressNut = data.MacroPressNut;
		this.MacroSimpleHole = data.MacroSimpleHole;
		this.MacroThread = data.MacroThread;
		this.MacroTwoSidedCounterSink = data.MacroTwoSidedCounterSink;
	}

	public void Save(IPnPathService pathService)
	{
		using FileStream utf8Json = new FileStream(Path.Combine(pathService.FolderCad3d2Pn, this.DocAssemblyId + ".c3do.info"), FileMode.Create);
		JsonSerializer.Serialize(utf8Json, this);
	}

	public static DocMetadata Load(IPnPathService pathService, int id)
	{
		string path = Path.Combine(pathService.FolderCad3d2Pn, id + ".c3do.info");
		if (File.Exists(path))
		{
			return JsonSerializer.Deserialize<DocMetadata>(File.ReadAllText(path));
		}
		return null;
	}
}
