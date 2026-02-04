using WiCAM.Pn4000.Contracts.Assembly.Doc;

namespace WiCAM.Pn4000.Contracts.Common;

public interface IMessageLogGlobal : IMessageDisplay
{
	IMessageDisplay WithContext(IPnBndDoc? doc)
	{
		return (IMessageDisplay)(((object)doc?.MessageDisplay) ?? ((object)this));
	}
}
