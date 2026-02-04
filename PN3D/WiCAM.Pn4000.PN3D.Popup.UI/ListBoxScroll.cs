using System.Windows.Controls;

namespace WiCAM.Pn4000.PN3D.Popup.UI;

public class ListBoxScroll : ListView
{
	public ListBoxScroll()
	{
		base.SelectionChanged += ListBoxScroll_SelectionChanged;
	}

	private void ListBoxScroll_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		base.ScrollIntoView(base.SelectedItem);
	}
}
