using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.TabViewer3D;

public partial class Viewer3dView : UserControl, IComponentConnector, IStyleConnector
{
	public Viewer3dView(IViewer3dViewModel vm)
	{
		base.DataContext = vm;
		InitializeComponent();
	}

	private void TreeViewSelectedItemChanged(object sender, RoutedEventArgs e)
	{
		if (sender is TreeViewItem { Header: HierarchicNode header } treeViewItem && header.SubNodes.Count == 0)
		{
			treeViewItem.BringIntoView();
		}
	}

	private void TreeViewItemMouseEnter(object sender, MouseEventArgs e)
	{
		if (sender is FrameworkElement { DataContext: HierarchicNode dataContext } && base.DataContext is IViewer3dViewModel viewer3dViewModel)
		{
			viewer3dViewModel.HoverModel(dataContext.Model);
		}
	}

	private void TreeViewItemMouseLeave(object sender, MouseEventArgs e)
	{
		if (sender is FrameworkElement { DataContext: HierarchicNode dataContext } && base.DataContext is IViewer3dViewModel viewer3dViewModel)
		{
			viewer3dViewModel.DeHoverModel(dataContext.Model);
		}
	}
}
