namespace WiCAM.Pn4000.Contracts.FlangeLengthValidator;

public class AtomicBendLength
{
	public double Start { get; set; }

	public double End { get; set; }

	public double StartWithOffset { get; set; }

	public double EndWithOffset { get; set; }

	public int FaceGroupId { get; set; }

	public FlangeLength? Neighbor0FlangeLength { get; set; }

	public FlangeLength? Neighbor1FlangeLength { get; set; }
}
