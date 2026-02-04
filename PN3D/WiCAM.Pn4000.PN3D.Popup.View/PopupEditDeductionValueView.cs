using System.Windows.Input;
using System.Windows.Markup;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.Popup;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Popup.View;

public partial class PopupEditDeductionValueView : PopupBase, IComponentConnector
{
	public PopupEditDeductionValueView(ILogCenterService logCenterService, IConfigProvider configProvider, IShowPopupService popupService, IPKernelFlowGlobalDataService globalDataService)
		: base(logCenterService, configProvider, popupService, globalDataService, "Edit-Deduction-Value")
	{
		this.InitializeComponent();
	}

	private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		base.DragMove();
	}
}
