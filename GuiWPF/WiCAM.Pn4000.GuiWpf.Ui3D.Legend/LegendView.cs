using System;
using System.Windows.Markup;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.Popup;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.Legend;

public partial class LegendView : PopupBase, IComponentConnector
{
	private ILegendViewModel _vm;

	public void Init(ILegendViewModel vm)
	{
		_vm = vm;
		base.DataContext = vm;
		InitializeComponent();
	}

	public LegendView(ILogCenterService logCenterService, IConfigProvider configProvider, IShowPopupService popupService, IPKernelFlowGlobalDataService globalDataService)
		: base(logCenterService, configProvider, popupService, globalDataService, "Legende3D")
	{
	}

	protected override void OnClosed(EventArgs e)
	{
		_vm.Save();
		base.OnClosed(e);
	}
}
