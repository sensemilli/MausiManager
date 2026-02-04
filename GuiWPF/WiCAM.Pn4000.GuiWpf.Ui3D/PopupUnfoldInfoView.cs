using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Markup;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.pn4.pn4FlowCenter;
using WiCAM.Pn4000.Popup;
using WiCAM.Pn4000.Popup.Enums;
using WiCAM.Services.ConfigProviders.Contracts;

namespace WiCAM.Pn4000.GuiWpf.Ui3D;

public partial class PopupUnfoldInfoView : PopupBase, IComponentConnector
{
	private PopupUnfoldInfoViewModel _vm;

	public PopupUnfoldInfoView(PopupUnfoldInfoViewModel viewModel, IPKernelFlowGlobalDataService pKernelFlowGlobalDataService, ILogCenterService logCenterService, IConfigProvider configProvider, IShowPopupService popupService)
		: base(logCenterService, configProvider, popupService, pKernelFlowGlobalDataService, "UnfoldInfo")
	{
		_vm = viewModel;
		base.DataContext = viewModel;
		base.Owner = pKernelFlowGlobalDataService.MainWindow;
		OnClosingAction = (Action<EPopupCloseReason>)Delegate.Combine(OnClosingAction, new Action<EPopupCloseReason>(viewModel.ViewCloseAction));
	}

	public void Init(IDoc3d doc)
	{
		_vm.Init(doc);
		InitializeComponent();
	}

	private void Content_Rendered(object sender, EventArgs e)
	{
		((PopupUnfoldInfoViewModel)base.DataContext)?.LoadGeometryInBackground();
	}


}
