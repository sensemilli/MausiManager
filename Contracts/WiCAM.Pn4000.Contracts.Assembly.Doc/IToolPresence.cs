using WiCAM.Pn4000.Contracts.Assembly.Doc.Enums;

namespace WiCAM.Pn4000.Contracts.Assembly.Doc;

public interface IToolPresence
{
	ToolPresenceType PresenceType { get; set; }

	double Length { get; set; }

	double Location { get; set; }

	double LocationEnd { get; }
}
