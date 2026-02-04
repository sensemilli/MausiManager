using WiCAM.Pn4000.PN3D.Tool;

namespace WiCAM.Pn4000.PN3D.CommonBend;

public class NoToolsFoundReason
{
	public NoToolsFoundReasonType ReasonType;

	public double BendAngleDeg;

	public double OriginalRadius;

	public string Material;

	public ValueMismatch Vm;

	public NoToolsFoundReason(NoToolsFoundReasonType reasonType, double bendAngleDeg, double originalRadius, string material, ValueMismatch vm)
	{
		this.ReasonType = reasonType;
		this.BendAngleDeg = bendAngleDeg;
		this.OriginalRadius = originalRadius;
		this.Material = material;
		this.Vm = vm;
	}

	public NoToolsFoundReason()
	{
		this.ReasonType = NoToolsFoundReasonType.NoReason;
	}

	public NoToolsFoundReason(NoToolsFoundReasonType reasonType)
	{
		this.ReasonType = reasonType;
	}
}
