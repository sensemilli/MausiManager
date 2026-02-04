using WiCAM.Pn4000.Contracts.Assembly.Doc;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SBendDescriptor
{
	public int ID { get; set; }

	public BendingType Type { get; set; }

	public SBendParametersBase BendParams { get; set; }
}
