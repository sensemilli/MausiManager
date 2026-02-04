using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;
using WiCAM.Pn4000.PN3D.BendSimulation.Configuration.Views;
using WiCAM.Pn4000.PN3D.Unfold.UI.ViewModel;

namespace WiCAM.Pn4000.PN3D.Unfold.UI.View;

public partial class SplitAndCombineBendView : ScreenControlBaseView, IComponentConnector, IStyleConnector
{
	public SplitAndCombineBendView()
	{
		this.InitializeComponent();
	}

	private void UIElement_OnMouseEnter(object sender, MouseEventArgs e)
	{
		((SplitAndCombineBendViewModel)base.DataContext)?.MouseEnterCommand();
	}

	private void UIElement_OnMouseLeave(object sender, MouseEventArgs e)
	{
		((SplitAndCombineBendViewModel)base.DataContext)?.MouseLeaveCommand();
	}

	private void Bend_OnMouseEnter(object sender, MouseEventArgs e)
	{
		if (sender is FrameworkElement { DataContext: BendFaceViewModel dataContext } && base.DataContext is SplitAndCombineBendViewModel splitAndCombineBendViewModel)
		{
			splitAndCombineBendViewModel.HoverBend(dataContext);
		}
	}

	private void Bend_OnMouseLeave(object sender, MouseEventArgs e)
	{
		if (sender is FrameworkElement { DataContext: BendFaceViewModel dataContext } && base.DataContext is SplitAndCombineBendViewModel splitAndCombineBendViewModel)
		{
			splitAndCombineBendViewModel.DeHoverBend(dataContext);
		}
	}

	
}
