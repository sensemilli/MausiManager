using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Markup;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration.Views;
using WiCAM.Pn4000.PN3D.Unfold.UI.ViewModel;

namespace WiCAM.Pn4000.PN3D.Unfold.UI.View;

public partial class ContextMenuStepBendWithoutMachineView : ScreenControlBaseView, IComponentConnector
{
	public ContextMenuStepBendWithoutMachineView()
	{
		this.InitializeComponent();
	}

	private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
	{
		((ContextMenuStepBendWithoutMachineViewModel)base.DataContext)?.MouseEnterCommand();
	}

	private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
	{
		((ContextMenuStepBendWithoutMachineViewModel)base.DataContext)?.MouseLeaveCommand();
	}

	
}
