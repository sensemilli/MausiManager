using System.Windows.Markup;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.Popup;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.DeveloperTests;

public partial class PopupTest2 : PopupBase, IComponentConnector
{
	public PopupTest2(ILogCenterService logCenterService, IConfigProvider configProvider, IShowPopupService popupService, IPKernelFlowGlobalDataService globalDataService, string PopupName)
		: base(logCenterService, configProvider, popupService, globalDataService, PopupName)
	{
		this.InitializeComponent();
	}
}
