using System.Windows.Controls;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.TabUnfold;

public partial class ValidationView : UserControl, IComponentConnector
{
	public ValidationView()
	{
		InitializeComponent();
	}

	private void Selector_OnSelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		((ValidationViewModel)base.DataContext).ValidationSelectedChanged(originUser: true);
	}
}
