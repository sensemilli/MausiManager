using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;
using WiCAM.Pn4000.GuiContracts.PnStatusBar;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Fortran;

public partial class PnStatusBarPKernelControl : UserControl, IComponentConnector
{
	public PnStatusBarPKernelControl(IPKernelStatusBarViewModel viewModel, int idx)
	{
		IPKernelStatusBarVisualData dataContext = viewModel.VisualData[idx];
		base.DataContext = dataContext;
		InitializeComponent();
	}

	
}
