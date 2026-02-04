using System;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.TabBend.PPaperSettings;

public partial class PpScreenshotSettingsView : UserControl, IComponentConnector
{
	private readonly PpScreenshotSettingsViewModel _viewModel;

	public PpScreenshotSettingsView(PpScreenshotSettingsViewModel vm)
	{
		_viewModel = vm;
		base.DataContext = _viewModel;
	}

	public void Init(Action<PpScreenshotSettingsViewModel> closeAction)
	{
		_viewModel.Init(closeAction);
		InitializeComponent();
	}
}
