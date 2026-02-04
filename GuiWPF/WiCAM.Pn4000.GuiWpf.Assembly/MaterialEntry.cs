using System.Collections.Generic;

namespace WiCAM.Pn4000.GuiWpf.Assembly;

public class MaterialEntry
{
	private MaterialWicam _matWicam;

	public string MatImport { get; set; }

	public MaterialWicam MatWicam
	{
		get
		{
			return _matWicam;
		}
		set
		{
			if (_matWicam != value)
			{
				_matWicam = value;
				AssignMaterialToParts();
			}
		}
	}

	public int PartsCount => Parts.Count;

	public List<DisassemblyPartViewModel> Parts { get; set; }

	public void AssignMaterialToParts()
	{
		int id = MatWicam.Id;
		if (id < 0)
		{
			return;
		}
		foreach (DisassemblyPartViewModel part in Parts)
		{
			part.MaterialId = id;
			part.PnMaterialByUser = true;
		}
	}
}
