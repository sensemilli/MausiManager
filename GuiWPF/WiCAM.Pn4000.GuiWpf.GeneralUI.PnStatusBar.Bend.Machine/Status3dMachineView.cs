using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend.Machine;

internal partial class Status3dMachineView : UserControl, IStatus3dMachineView, IComponentConnector
{
	public Status3dMachineView(IStatus3dMachineViewModel vm)
	{
		base.DataContext = vm;
		InitializeComponent();
	}

}
