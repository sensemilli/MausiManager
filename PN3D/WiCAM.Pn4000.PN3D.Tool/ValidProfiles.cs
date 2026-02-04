using System.Collections.Generic;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Dies;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Holder;
using WiCAM.Pn4000.BendTable.Contracts;
using WiCAM.Pn4000.Contracts.Tools;

namespace WiCAM.Pn4000.PN3D.Tool;

public class ValidProfiles
{
	public List<HemProfile> FrontHemProfile { get; set; }

	public List<HemProfile> BackHemProfile { get; set; }

	public List<HolderProfile> UpperHolderProfile { get; set; }

	public List<HolderProfile> LowerHolderProfile { get; set; }

	public List<IPunchProfile> Punches { get; set; }

	public List<IDieProfile> Dies { get; set; }

	public IBendTableEntity BendTableEntry { get; set; }
}
