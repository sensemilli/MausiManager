using WiCAM.Pn4000.Contracts.LogCenterServices.Enum;

namespace WiCAM.Pn4000.Contracts.LogCenterServices;

public interface IErrorPopup
{
	void AddErrorInfo(ErrorLevel errorLevel, string text);
}
