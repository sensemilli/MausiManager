using System.Text;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Dies;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Holder;
using BendDataBase.WiCAM.Pn4000.pn4.Sym3D.BendSimulation.PnConfig.Tools.Punches;
using WiCAM.Pn4000.BendModel;

namespace WiCAM.Pn4000.PN3D.CommonBend;

public class ToolInfo
{
	public bool PunchToolFlipped { get; set; }

	public PunchProfile PunchProfile { get; set; }

	public HolderProfile UpperHolderProfile { get; set; }

	public Model PunchModel { get; set; }

	public DieProfile DieProfile { get; set; }

	public HolderProfile LowerHolderProfile { get; set; }

	public HemProfile FrontHemProfile { get; set; }

	public HemProfile BackHemProfile { get; set; }

	public Model DieModel { get; set; }

	public override string ToString()
	{
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append("UpperHolder: ").Append(this.UpperHolderProfile?.Name).Append(", WH:")
			.Append(this.UpperHolderProfile?.WorkingHeight)
			.Append(" / ");
		stringBuilder.Append("Punch: ").Append(this.PunchProfile.Name).Append(", Flipped:")
			.Append(this.PunchToolFlipped)
			.Append(", WH:")
			.Append(this.PunchProfile.WorkingHeight)
			.Append(" / ");
		stringBuilder.Append("LowerHolder: ").Append(this.LowerHolderProfile?.Name).Append(", WH:")
			.Append(this.LowerHolderProfile?.WorkingHeight)
			.Append(" / ");
		if (this.FrontHemProfile != null)
		{
			stringBuilder.Append("FrontHemPart: ").Append(this.FrontHemProfile?.Name).Append(", WH:")
				.Append(this.FrontHemProfile?.WorkingHeight)
				.Append(" / ");
		}
		if (this.BackHemProfile != null)
		{
			stringBuilder.Append("BackHemPart: ").Append(this.BackHemProfile?.Name).Append(", WH:")
				.Append(this.BackHemProfile?.WorkingHeight)
				.Append(" / ");
		}
		stringBuilder.Append("Die: ").Append(this.DieProfile.Name).Append(", WH:")
			.Append(this.DieProfile.WorkingHeight);
		return stringBuilder.ToString();
	}

	public ToolInfo ShallowCopy()
	{
		return (ToolInfo)base.MemberwiseClone();
	}
}
