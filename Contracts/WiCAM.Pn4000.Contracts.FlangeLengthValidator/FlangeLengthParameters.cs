namespace WiCAM.Pn4000.Contracts.FlangeLengthValidator;

public record FlangeLengthParameters
{
	public double? TrapezoidLengthRelative { get; set; }

	public double? BendingAxisAngleRad { get; set; }

	public double BendingAxisOffset { get; set; } = 0.01;

	public double BendingAxisOffsetThreshold { get; set; } = 0.01;

	public double? MinSearchDistanceNeighbor0 { get; set; }

	public double MaxSearchDistanceNeighbor0 { get; set; } = double.MaxValue;

	public double? MinSearchDistanceNeighbor1 { get; set; }

	public double MaxSearchDistanceNeighbor1 { get; set; } = double.MaxValue;

	public bool Global { get; set; }

	public bool HoleIntervalsInBendingZones { get; set; } = true;

	public MacroClassificationStrategy ClassifyMacros { get; set; }

	public bool DisplayOutputRegion { get; set; }

	public bool DisplayInputRegion { get; set; }

	public bool DisplayMacros { get; set; }

	public string? Folder2DWritePath { get; set; }
}
