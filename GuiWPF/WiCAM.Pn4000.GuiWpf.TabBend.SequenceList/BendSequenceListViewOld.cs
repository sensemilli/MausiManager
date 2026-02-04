using System;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.TabBend.SequenceList;

public partial class BendSequenceListViewOld : UserControl, IComponentConnector, IStyleConnector
{
	public BendSequenceListViewOld()
	{
		InitializeComponent();
	}

	private void Bd_OnMouseEnter(object sender, MouseEventArgs e)
	{
		if (sender is Border { DataContext: BendSequenceListItemViewModel dataContext } && base.DataContext is BendSequenceListViewModelOld bendSequenceListViewModelOld)
		{
			bendSequenceListViewModelOld.HoverBend(dataContext);
		}
	}

	private void Bd_OnMouseLeave(object sender, MouseEventArgs e)
	{
		if (sender is Border { DataContext: BendSequenceListItemViewModel dataContext } && base.DataContext is BendSequenceListViewModelOld bendSequenceListViewModelOld)
		{
			bendSequenceListViewModelOld.DeHoverBend(dataContext);
		}
	}

	private void Bd_OnMouseRightButtonUp(object sender, MouseButtonEventArgs e)
	{
	}

	public void SetMaxHeight(double maxHeight)
	{
		BendList.MaxHeight = Math.Max(maxHeight, 30.0);
	}
}
