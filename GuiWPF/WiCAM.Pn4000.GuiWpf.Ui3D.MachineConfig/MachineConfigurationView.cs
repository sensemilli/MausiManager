using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using WiCAM.Pn4000.Contracts.MachineBend;
using WiCAM.Pn4000.GuiContracts;
using WiCAM.Pn4000.PN3D.Doc;
using WiCAM.Pn4000.PN3D.Interfaces;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public partial class MachineConfigurationView : UserControl, IComponentConnector
{
	private readonly IMainWindowBlock _mainWindowBlock;

	private readonly ICurrentDocProvider _currentDocProvider;

	private PopupMachineConfigViewModel Vm { get; }

	public MachineConfigurationView(PopupMachineConfigViewModel viewModel, IMainWindowBlock mainWindowBlock, ICurrentDocProvider currentDocProvider)
	{
		_mainWindowBlock = mainWindowBlock;
		_currentDocProvider = currentDocProvider;
		Vm = viewModel;
	}

	public void Init(IDoc3d doc, IBendMachine bendMachine, Action<PopupMachineConfigViewModel> closeAction)
	{
		Vm.Init(doc, bendMachine, closeAction);
		InitializeComponent();
		base.DataContext = Vm;
	}

	private void UserControl_Loaded(object sender, RoutedEventArgs e)
	{
		BendTableTabView.Visibility = Visibility.Visible;
		MachineTabView.Visibility = Visibility.Visible;
		TabControlTools.Visibility = Visibility.Visible;
		FingerStopsTabView.Visibility = Visibility.Visible;
		MappingTabView.Visibility = Visibility.Visible;
		BendSequenceTabView.Visibility = Visibility.Visible;
		PostProcessorTabView.Visibility = Visibility.Visible;
		if (base.DataContext != null)
		{
			((PopupMachineConfigViewModel)base.DataContext).SelectedTab = BendTableTab;
		}
	}

	private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
	}

	public void SetActive(bool active)
	{
		if (active && Vm.LowerToolsViewModel.RememberBlockState)
		{
			Vm.LowerToolsViewModel.RememberBlockState = false;
			if (!_mainWindowBlock.BlockUI_IsBlock(_currentDocProvider.CurrentDoc))
			{
				_mainWindowBlock.BlockUI_Block(_currentDocProvider.CurrentDoc);
			}
		}
	}


}
