using WiCAM.Pn4000.Contracts.Common;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Services.Loggers.Contracts;

namespace WiCAM.Pn4000.Helpers.Util;

public class MessageLogDoc : MessageDisplay, IMessageLogDoc, IMessageDisplay
{
	public MessageLogDoc(IWiLogger logger, ITranslator translator, IFactorio factorio)
		: base(logger, translator, factorio)
	{
	}
}
