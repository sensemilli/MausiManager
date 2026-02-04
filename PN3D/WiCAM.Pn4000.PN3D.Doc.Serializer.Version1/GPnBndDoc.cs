using WiCAM.Pn4000.BendModel.Serialization.SerializationStructure;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class GPnBndDoc
{
	public SModel InputModel { get; set; }

	public SModel EntryModel { get; set; }

	public double Thickness { get; set; }
}
