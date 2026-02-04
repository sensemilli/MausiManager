namespace WiCAM.Pn4000.Contracts.Tools;

public class GeometryVerifierDieParameters
{
	public double? VWidth { get; init; }

	public double? VAngleRad { get; init; }

	public double? VAngleDeg { get; init; }

	public double? CornerRadius { get; init; }

	public VWidthTypes? VWidthType { get; init; }

	public double? StoneFactor { get; init; }

	public bool IsFoldTool { get; init; }
}
