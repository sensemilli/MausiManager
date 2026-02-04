namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SBendToolSetup
{
	public int Id { get; set; }

	public SBendToolSet UpperToolSet { get; set; }

	public SBendToolSet LowerToolSet { get; set; }

	public SBendAdapterSet UpperAdapterSet { get; set; }

	public SBendAdapterSet LowerAdapterSet { get; set; }

	public double StartPosition { get; set; }

	public double EndPosition { get; set; }

	public double SpaceLeft { get; set; }

	public double SpaceRight { get; set; }

	public SBendToolSetup CombinedSetup { get; set; }

	public SBendToolSection CombinedUpperSection { get; set; }

	public SBendToolSection CombinedLowerSection { get; set; }

	public SBendAdapterSection CombinedUpperAdapterSection { get; set; }

	public SBendAdapterSection CombinedLowerAdapterSection { get; set; }

	public SToolStation ToolStation { get; set; }
}
