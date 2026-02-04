using WiCAM.Pn4000.Contracts.Common;

namespace WiCAM.Pn4000.Contracts.BendPP;

public interface IPPError
{
	string ToString(ITranslator translator);

	string? GetAdditionalInformation(ITranslator translator)
	{
		return null;
	}
}
