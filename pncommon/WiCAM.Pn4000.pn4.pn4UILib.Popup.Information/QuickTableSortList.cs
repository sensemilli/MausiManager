using System.Collections.Generic;
using System.ComponentModel;

namespace WiCAM.Pn4000.pn4.pn4UILib.Popup.Information;

public class QuickTableSortList
{
	public List<QuickTableSortRecord> List = new List<QuickTableSortRecord>();

	public void Add(ListSortDirection direction, QuickTableColumnInfo columnInfo, int idx)
	{
		QuickTableSortRecord quickTableSortRecord = new QuickTableSortRecord();
		quickTableSortRecord.direction = direction;
		quickTableSortRecord.columnInfo = columnInfo;
		quickTableSortRecord.idx = idx;
		this.RemoveSameSortBy(columnInfo);
		this.List.Insert(0, quickTableSortRecord);
		if (this.List.Count == 5)
		{
			this.List.RemoveAt(4);
		}
	}

	private void RemoveSameSortBy(QuickTableColumnInfo columnInfo)
	{
		foreach (QuickTableSortRecord item in this.List)
		{
			if (item.columnInfo == columnInfo)
			{
				this.List.Remove(item);
				break;
			}
		}
	}
}
