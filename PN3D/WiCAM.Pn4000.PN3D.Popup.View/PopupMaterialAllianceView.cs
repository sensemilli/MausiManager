using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Markup;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.Popup;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.PN3D.Popup.View;

public partial class PopupMaterialAllianceView : PopupBase, IComponentConnector
{
	public PopupMaterialAllianceView(ILogCenterService logCenterService, IConfigProvider configProvider, IShowPopupService popupService, IPKernelFlowGlobalDataService globalDataService)
		: base(logCenterService, configProvider, popupService, globalDataService, "MaterialAlliance")
	{
		this.InitializeComponent();
	}


}
