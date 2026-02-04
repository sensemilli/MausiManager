using System.Collections.Generic;
using WiCAM.Pn4000.BendModel.Base;

namespace WiCAM.Pn4000.Contracts.PnCommands;

public class ImportArg : IImportArg
{
	public string Filename { get; set; }

	public bool UseHd { get; set; } = true;

	public IImportArg.SpecialPartSelectionModes SpecialPartSelectionMode { get; set; }

	public bool CheckLicense { get; set; } = true;

	public bool MoveToCenter { get; set; } = true;

	public bool LoadLastAssembly { get; set; }

	public bool OpenSingleParts { get; set; } = true;

	public int? OpenPartId { get; set; }

	public bool NoPopups { get; set; }

	public int? MachineNumber { get; set; }

	public int? MaterialNumber { get; set; }

	public bool MaterialByForce { get; set; } = true;

	public bool CloseAllDocs { get; set; }

	public IImportArg.ReconstructBendsMode? ReconstructBends { get; set; }

	public int? ParallelCount { get; set; }

	public List<Vector3d> ASidePointsOutline { get; set; } = new List<Vector3d>();

	public List<Vector3d> ASidePointsHoles { get; set; } = new List<Vector3d>();

	public bool UseOppositeViewingSide { get; set; }
}
