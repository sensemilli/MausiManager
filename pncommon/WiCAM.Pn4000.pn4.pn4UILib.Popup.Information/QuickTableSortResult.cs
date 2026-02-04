using System;
using System.Collections;

namespace WiCAM.Pn4000.pn4.pn4UILib.Popup.Information;

public class QuickTableSortResult : IComparer
{
	private QuickTableSortList sortingList;

	public QuickTableSortResult(QuickTableSortList sortingList)
	{
		this.sortingList = sortingList;
	}

	public int Compare(object x, object y)
	{
		QuickTableRecord quickTableRecord = x as QuickTableRecord;
		QuickTableRecord quickTableRecord2 = y as QuickTableRecord;
		if (quickTableRecord.IsDataRecord != quickTableRecord2.IsDataRecord)
		{
			if (!quickTableRecord.IsDataRecord)
			{
				return 1;
			}
			if (!quickTableRecord2.IsDataRecord)
			{
				return -1;
			}
		}
		foreach (QuickTableSortRecord item in this.sortingList.List)
		{
			int num = 0;
			num = ((item.direction != 0) ? this.CompareWithType(quickTableRecord2.Data[item.idx], quickTableRecord.Data[item.idx], item.columnInfo) : this.CompareWithType(quickTableRecord.Data[item.idx], quickTableRecord2.Data[item.idx], item.columnInfo));
			if (num != 0)
			{
				return num;
			}
		}
		return 0;
	}

	public int CompareWithType(object xo, object yo, QuickTableColumnInfo columnInfo)
	{
		return columnInfo.Type switch
		{
			1 => string.Compare((string)xo, (string)yo), 
			2 => ((int)xo).CompareTo((int)yo), 
			3 => ((double)xo).CompareTo((double)yo), 
			4 => DateTime.Compare((DateTime)xo, (DateTime)yo), 
			5 => TimeSpan.Compare((TimeSpan)xo, (TimeSpan)yo), 
			_ => 0, 
		};
	}
}
