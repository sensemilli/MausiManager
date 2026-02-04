using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;

namespace WiCAM.Pn4000.PN3D.Doc.Serializer.Version1;

public class SToolPresence
{
	public ToolPresenceType PresenceType { get; set; }

	public double Length { get; set; }

	public double Location { get; set; } = double.NaN;
}
