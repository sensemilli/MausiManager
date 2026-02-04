using System;

namespace WiCAM.Pn4000.Contracts.EventTokens;

public interface IWorkStatusToken
{
	event Action<int> OnStatusChanged;

	void RaiseStatusChange(int status);
}
