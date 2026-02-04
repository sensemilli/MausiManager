using System;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.GuiContracts.Ribbon;
using WiCAM.Pn4000.pn4.Contracts;

namespace WiCAM.Pn4000.pn4.pn4UILib;

internal class SubMenuConnector : ISubMenuConnector
{
	public event Action<int> OnToKernelMouseWheel;

	public event Action<IPnCommand> OnCallPnCommandSubmenuEdition;

	public event Action<IMFileExpert> OnSetSubMenu;

	public event Action OnShowWithLocationCheck;

	public event Action OnHide;

	public void ToKernelMouseWheel(int delta)
	{
		this.OnToKernelMouseWheel?.Invoke(delta);
	}

	public void CallPnCommandSubmenuEdition(IPnCommand cmd)
	{
		this.OnCallPnCommandSubmenuEdition?.Invoke(cmd);
	}

	public void SetSubMenu(IMFileExpert expert)
	{
		this.OnSetSubMenu?.Invoke(expert);
	}

	public void ShowWithLocationCheck()
	{
		this.OnShowWithLocationCheck?.Invoke();
	}

	public void Hide()
	{
		this.OnHide?.Invoke();
	}
}
