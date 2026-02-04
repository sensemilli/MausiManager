using System.Collections;
using System.Reflection;
using System.Windows.Controls.Primitives;

namespace WiCAM.Pn4000.pn4.pn4UILib.Popup;

public static class MultiSelectorHelper
{
	private static readonly PropertyInfo _piIsUpdatingSelectedItems;

	private static readonly MethodInfo _miBeginUpdateSelectedItems;

	private static readonly MethodInfo _miEndUpdateSelectedItems;

	static MultiSelectorHelper()
	{
		MultiSelectorHelper._piIsUpdatingSelectedItems = typeof(MultiSelector).GetProperty("IsUpdatingSelectedItems", BindingFlags.Instance | BindingFlags.NonPublic);
		MultiSelectorHelper._miBeginUpdateSelectedItems = typeof(MultiSelector).GetMethod("BeginUpdateSelectedItems", BindingFlags.Instance | BindingFlags.NonPublic);
		MultiSelectorHelper._miEndUpdateSelectedItems = typeof(MultiSelector).GetMethod("EndUpdateSelectedItems", BindingFlags.Instance | BindingFlags.NonPublic);
	}

	public static void SelectManyItems(this MultiSelector control, IEnumerable itemsToBeSelected, bool clear)
	{
		control.Dispatcher.Invoke(delegate
		{
			if (!(bool)MultiSelectorHelper._piIsUpdatingSelectedItems.GetValue(control, null))
			{
				MultiSelectorHelper._miBeginUpdateSelectedItems.Invoke(control, null);
				try
				{
					if (clear)
					{
						control.SelectedItems.Clear();
					}
					foreach (object item in itemsToBeSelected)
					{
						control.SelectedItems.Add(item);
					}
				}
				finally
				{
					MultiSelectorHelper._miEndUpdateSelectedItems.Invoke(control, null);
				}
			}
		});
	}
}
