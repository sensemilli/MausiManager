using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace pncommon.WiCAM.Pn4000.Helpers.ObservableCollectionHelper;

public static class OcHelper
{
	public static IEnumerable<T> Swap<T>(this IList<T> list, int indexA, int indexB)
	{
		T item = list[indexA];
		T item2 = list[indexB];
		list.Remove(item);
		list.Remove(item2);
		if (indexA < indexB)
		{
			list.Insert(indexA, item2);
			list.Insert(indexB, item);
		}
		else
		{
			list.Insert(indexB, item);
			list.Insert(indexA, item2);
		}
		return list;
	}

	public static ObservableCollection<T> ToObservableCollection<T>(this IEnumerable<T> enumerable)
	{
		return new ObservableCollection<T>(enumerable);
	}
}
