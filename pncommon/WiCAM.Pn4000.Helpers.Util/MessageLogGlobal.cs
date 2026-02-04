using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Services.Loggers.Contracts;

namespace WiCAM.Pn4000.Helpers.Util;

public class MessageLogGlobal : MessageDisplay, IMessageLogGlobal, IMessageDisplay
{
	public MessageLogGlobal(IWiLogger logger, ITranslator translator, IFactorio factorio)
		: base(logger, translator, factorio)
	{
	}
}
