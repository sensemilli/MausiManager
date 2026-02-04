using System;
using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.BendTools;
using WiCAM.Pn4000.Contracts.Assembly.Doc;

namespace WiCAM.Pn4000.PN3D.Interfaces;

public interface IDocMetadata
{
	Dictionary<int, string> UserComments { get; }

	double LenUnfoldX { get; }

	double LenUnfoldY { get; }

	bool UseBendAid { get; }

	List<string>? UpperToolNames { get; }

	List<string>? LowerToolNames { get; }

	List<string>? UpperAdapterNames { get; }

	List<string>? LowerAdapterNames { get; }

	string CreationUserName { get; }

	DateTime CreationDate { get; }

	string LastModifiedUserName { get; }

	DateTime LastModified { get; }

	string AssemblyGuid { get; }

	int MacroBlindHole { get; }

	int MacroBolt { get; }

	int MacroBorder { get; }

	int MacroBridgeLance { get; }

	int MacroChamfer { get; }

	int MacroCounterSink { get; }

	int MacroDeepening { get; }

	int MacroDummy { get; }

	int MacroEmbossed { get; }

	int MacroEmbossedCircle { get; }

	int MacroEmbossedCounterSink { get; }

	int MacroEmbossedFreeform { get; }

	int MacroEmbossedLine { get; }

	int MacroEmbossedRectangle { get; }

	int MacroEmbossedRectangleRounded { get; }

	int MacroEmbossedSquare { get; }

	int MacroEmbossedSquareRounded { get; }

	int MacroLance { get; }

	int MacroLouver { get; }

	int MacroManufacturingMacro { get; }

	int MacroPressNut { get; }

	int MacroSimpleHole { get; }

	int MacroThread { get; }

	int MacroTwoSidedCounterSink { get; }

	int BendCount { get; }

	double Thickness { get; }

	GeometryType Dimension { get; }

	double LenX { get; }

	double LenY { get; }

	double LenZ { get; }

	PartType SheetType { get; }

	int DocAssemblyId { get; }

	string AssemblyName { get; set; }

	bool KFactorWarnings { get; }

	bool PnMaterialByUser { get; }

	int? PnMaterialNo { get; }

	int? BendMachineNo { get; }

	string BendMachineDesc { get; }

	int SavedArchivNo { get; }

	string SavedArchivName { get; }

	DocState DocState { get; }

	int? ValidationGeoErrors { get; }

	string PPName { get; }

	int? CutoutsAdded { get; }

	string Comment { get; }
}
