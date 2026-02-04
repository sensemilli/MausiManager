using BendDataSourceModel.Enums;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SBendAdapter
{
	public int Id { get; set; }

	public int PartId { get; set; }

	public int ProfileId { get; set; }

	public string ProfileName { get; set; }

	public string Name { get; set; }

	public BendAdapterLocationType BendAdapterLocation { get; set; }

	public double Location { get; set; }

	public bool Flipped { get; set; }
}
