using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.Contracts.PnCommands;

public interface IImportArg
{
	public enum SpecialPartSelectionModes
	{
		None = 0,
		Flat = 1,
		NotFlat = 2
	}

	public enum ReconstructBendsMode
	{
		DoNotReconstruct = 0,
		ReconstructIfNecessary = 1,
		AlwaysReconstruct = 2
	}

	string Filename { get; set; }

	bool UseHd { get; set; }

	SpecialPartSelectionModes SpecialPartSelectionMode { get; set; }

	bool CheckLicense { get; set; }

	bool MoveToCenter { get; set; }

	bool LoadLastAssembly { get; set; }

	bool OpenSingleParts { get; set; }

	int? OpenPartId { get; set; }

	bool NoPopups { get; set; }

	int? MachineNumber { get; set; }

	int? MaterialNumber { get; set; }

	bool MaterialByForce { get; set; }

	int? ParallelCount { get; set; }

	ReconstructBendsMode? ReconstructBends { get; set; }

	List<Vector3d> ASidePointsOutline { get; set; }

	List<Vector3d> ASidePointsHoles { get; set; }

	bool UseOppositeViewingSide { get; set; }

	bool CloseAllDocs { get; set; }
}
