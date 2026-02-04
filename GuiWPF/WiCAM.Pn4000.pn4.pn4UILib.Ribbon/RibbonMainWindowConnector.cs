using System;
using System.Windows.Controls;
using WiCAM.Pn4000.Contracts.PnCommands;
using WiCAM.Pn4000.GuiContracts.Ribbon;

namespace WiCAM.Pn4000.pn4.pn4UILib.Ribbon;

public class RibbonMainWindowConnector : IRibbonMainWindowConnector
{
	public event Action<int, object, string, int, Image, Label> OnFillMenu;

	public event Action OnDynamicSetup;

	public event Action OnUpdateLastMenu;

	public event Action<IPnRibbonNode> OnSetup;

	public event Action OnChangeLanguage;

	public event Action OnResetAllPreviewNeeds;

	public event Action<int> OnSetMainScreenLayout;

	public event Action<IPnCommand> OnCallPnCommand;

	public event Action<EKernelType> OnScreenTypeSetup;

	public event Action<int, IRFileRecord> OnAddQatButtonToToolbar;

	public void FillMenu(int menutype, object list, string datatype, int recordMax, Image previewViewer, Label previewDescription)
	{
		this.OnFillMenu?.Invoke(menutype, list, datatype, recordMax, previewViewer, previewDescription);
	}

	public void DynamicSetup()
	{
		this.OnDynamicSetup?.Invoke();
	}

	public void UpdateLastMenu()
	{
		this.OnUpdateLastMenu?.Invoke();
	}

	public void Setup(IPnRibbonNode node)
	{
		this.OnSetup?.Invoke(node);
	}

	public void ChangeLanguage()
	{
		this.OnChangeLanguage?.Invoke();
	}

	public void ResetAllPreviewNeeds()
	{
		this.OnResetAllPreviewNeeds?.Invoke();
	}

	public void SetMainScreenLayout(int i)
	{
		this.OnSetMainScreenLayout?.Invoke(i);
	}

	public void CallPnCommand(IPnCommand nodeCommand)
	{
		this.OnCallPnCommand?.Invoke(nodeCommand);
	}

	public void ScreenTypeSetup(EKernelType screenType)
	{
		this.OnScreenTypeSetup?.Invoke(screenType);
	}

	public void AddQatButtonToToolbar(int i, IRFileRecord rfileRecord)
	{
		this.OnAddQatButtonToToolbar?.Invoke(i, rfileRecord);
	}
}
