using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend.Model;

internal partial class Status3dModelComponentsView : UserControl, IStatus3dModelComponentsView, IComponentConnector
{
	public Status3dModelComponentsView(IStatus3dModelInfoViewModel vm)
	{
		base.DataContext = vm;
		InitializeComponent();
	}

	
}
