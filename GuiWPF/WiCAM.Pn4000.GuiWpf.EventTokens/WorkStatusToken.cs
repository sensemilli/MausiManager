using System;
using WiCAM.Pn4000.Contracts.EventTokens;

namespace WiCAM.Pn4000.GuiWpf.EventTokens;

public class WorkStatusToken : IWorkStatusToken
{
	public event Action<int> OnStatusChanged;

	public void RaiseStatusChange(int status)
	{
		this.OnStatusChanged?.Invoke(status);
	}
}
