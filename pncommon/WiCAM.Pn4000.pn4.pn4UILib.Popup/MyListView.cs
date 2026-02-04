using System.Collections;
using System.Windows.Controls;

namespace WiCAM.Pn4000.pn4.pn4UILib.Popup;

public class MyListView : ListView
{
	public void SelectItems(IEnumerable items)
	{
		base.SetSelectedItems(items);
	}
}
