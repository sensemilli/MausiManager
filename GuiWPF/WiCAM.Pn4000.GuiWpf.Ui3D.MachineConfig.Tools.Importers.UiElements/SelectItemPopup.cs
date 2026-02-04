using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig.Tools.Importers.UiElements;

public partial class SelectItemPopup : Window, IComponentConnector
{
	private readonly List<SelectableItem> _allItems;

	public List<SelectableItem> SelectedItems => GridView.SelectedItems.Cast<SelectableItem>().ToList();

	public SelectItemPopup(List<SelectableItem> items)
	{
		InitializeComponent();
		_allItems = items;
		GridView.ItemsSource = _allItems;
	}

	private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
	{
		string q = SearchBox.Text?.ToLower() ?? "";
		GridView.ItemsSource = _allItems.Where((SelectableItem i) => i.FilePath.ToLower().Contains(q)).ToList();
	}

	private void OkButton_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = true;
		Close();
	}

	private void CancelButton_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = false;
		Close();
	}
}
