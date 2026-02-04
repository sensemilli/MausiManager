using System.Windows.Markup;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.Contracts.ManualCameraStateView;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.Popup;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.BendSimulation.PP;

public partial class PPManualSetupView : PopupBase, IPPManualSetupView, IComponentConnector
{
	public PPManualSetupView(ILogCenterService logCenterService, IConfigProvider configProvider, IShowPopupService popupService, IPKernelFlowGlobalDataService globalDataService)
		: base(logCenterService, configProvider, popupService, globalDataService, "PP Manual Setup")
	{
		InitializeComponent();
	}
}
