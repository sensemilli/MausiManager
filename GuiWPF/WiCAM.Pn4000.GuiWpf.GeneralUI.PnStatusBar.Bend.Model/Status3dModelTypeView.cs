using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.Bend.Model;

internal partial class Status3dModelTypeView : UserControl, IStatus3dModelTypeView, IComponentConnector
{
	public Status3dModelTypeView(IStatus3dModelInfoViewModel vm)
	{
		base.DataContext = vm;
		InitializeComponent();
	}

	private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (base.DataContext is IStatus3dModelInfoViewModel status3dModelInfoViewModel)
		{
			status3dModelInfoViewModel.ToggleBillboards();
		}
	}

	
}
