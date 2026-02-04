using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.PKernelFlow.Adapters;
using WiCAM.Pn4000.PKernelFlow.Adapters.Data;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.pn4.pn4Services.CADGEO;
using WiCAM.Pn4000.pn4.pn4UILib.Popup.Information;

namespace WiCAM.Pn4000.pn4.pn4UILib.Popup;

public partial class QuickTable1 : UserControl, IQuickTable, IComponentConnector
{
	private Action<bool> _SortButtonsEnableFunction;

	private ComboBox _filterCombo;

	private int _staticListMode;

	private PnImageSource _imageSource;

	private Action _exitCode2;

	private ILogCenterService _logCenterService;

	private ILanguageDictionary _languageDictionary;

	private IPnColorsService _pnColorsService;

	private bool _isAnySum;

	private bool ToolTipHandlerRegister;

	private bool _isSingleListMode;

	private List<QuickTableColumnInfo> _setupColumnInfo;

	private List<QuickTableRecord> deselectedByFilteringItems = new List<QuickTableRecord>();

	private StackPanel filter_panel1;

	private StackPanel filter_panel2;

	private ComboBox filter_combo1;

	private ComboBox filter_combo2;

	private ComboBox filterCombo;

	private TextBox filter_text;

	private TextBox filter_text1;

	private TextBox filter_text2;

	private int _filtersCount;

	private QuickTableSortList quickTableSortList;

	private bool is_EditButtonVisible;

	private int[] FiltersSetup;

	private DispatcherTimer dispatcherTimer;

	private bool mouseupforignore;

	public Popup MainPopupWindow { get; set; }

	public string FilterString { get; set; }

	public string FilterString1 { get; set; }

	public string FilterString2 { get; set; }

	public ObservableCollection<QuickTableRecord> TableRecords { get; set; } = new ObservableCollection<QuickTableRecord>();

	public void KeyboardFocus()
	{
		Keyboard.Focus(this);
	}

	public QuickTable1()
	{
		this.InitializeComponent();
		base.DataContext = this;
	}

	private bool Filter(object e1)
	{
		QuickTableRecord quickTableRecord = e1 as QuickTableRecord;
		if (!quickTableRecord.IsDataRecord)
		{
			return true;
		}
		if (this.CheckFilter(quickTableRecord, this.FilterString, this._filterCombo) && this.CheckFilter(quickTableRecord, this.FilterString1, this.filter_combo1))
		{
			return this.CheckFilter(quickTableRecord, this.FilterString2, this.filter_combo2);
		}
		return false;
	}

	private bool CheckFilter(QuickTableRecord record, string filterString, ComboBox filterCombo)
	{
		if (filterString == string.Empty)
		{
			return true;
		}
		string[] array = filterString.Split(' ');
		bool[] array2 = new bool[array.GetLength(0)];
		for (int i = 0; i < array.GetLength(0); i++)
		{
			array2[i] = false;
		}
		int num = 0;
		if (filterCombo != null)
		{
			num = filterCombo.SelectedIndex;
		}
		for (int j = 0; j < array.GetLength(0); j++)
		{
			for (int k = 0; k < record.Data.Count; k++)
			{
				if (num != 0 && k != num - 1)
				{
					continue;
				}
				string formatingStringFromData = this.GetFormatingStringFromData(record.Data[k], this._setupColumnInfo[k].FormatModification);
				if (record.Data[k] is double && array[j].Contains('.'))
				{
					if (formatingStringFromData.Length >= array[j].Length && formatingStringFromData.Substring(0, array[j].Length) == array[j])
					{
						return true;
					}
				}
				else if (!string.IsNullOrEmpty(array[j]) && formatingStringFromData.ToLower().Contains(array[j].ToLower()))
				{
					array2[j] = true;
				}
			}
		}
		for (int l = 0; l < array.GetLength(0); l++)
		{
			if (array[l].Trim(' ') != string.Empty && !array2[l])
			{
				return false;
			}
		}
		return true;
	}

	private string GetFormatingStringFromData(object data, int format_modification)
	{
		if (data is string)
		{
			return (string)data;
		}
		if (data is int num)
		{
			return num.ToString();
		}
		if (data is double)
		{
			string stringFormatForColumn = this.GetStringFormatForColumn(3, format_modification);
			return ((double)data).ToString(stringFormatForColumn, CultureInfo.InvariantCulture);
		}
		if (data is DateTime dateTime)
		{
			return dateTime.ToString("dd.MM.yyyy");
		}
		if (data is TimeSpan timeSpan)
		{
			return timeSpan.ToString();
		}
		return string.Empty;
	}

	public void SetFilter(string org_string, string org_string1, string org_string2)
	{
		this.FilterString = org_string.Trim();
		this.FilterString1 = org_string1.Trim();
		this.FilterString2 = org_string2.Trim();
		this.SetButtonsEnabel();
		if (this.dataGrid == null)
		{
			return;
		}
		ListCollectionView listCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(this.dataGrid.ItemsSource);
		if (listCollectionView == null)
		{
			return;
		}
		if (string.IsNullOrEmpty(this.FilterString) && string.IsNullOrEmpty(this.FilterString1) && string.IsNullOrEmpty(this.FilterString2))
		{
			listCollectionView.Filter = null;
		}
		else
		{
			listCollectionView.Filter = Filter;
		}
		listCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(this.dataGrid.ItemsSource);
		List<QuickTableRecord> list = new List<QuickTableRecord>();
		foreach (QuickTableRecord deselectedByFilteringItem in this.deselectedByFilteringItems)
		{
			if (!listCollectionView.Contains(deselectedByFilteringItem))
			{
				continue;
			}
			if (this.dataGrid.SelectionMode == DataGridSelectionMode.Single)
			{
				DataGrid dataGrid = this.dataGrid;
				if (dataGrid.SelectedItem == null)
				{
					object obj = (dataGrid.SelectedItem = deselectedByFilteringItem);
				}
			}
			else
			{
				this.dataGrid.SelectedItems.Add(deselectedByFilteringItem);
			}
			list.Add(deselectedByFilteringItem);
		}
		foreach (QuickTableRecord item in list)
		{
			this.deselectedByFilteringItems.Remove(item);
		}
		this.CalculateSums();
	}

	public static double NaturalBrightness(Color color)
	{
		return 0.21 * (double)(int)color.R + 0.72 * (double)(int)color.G + 0.07 * (double)(int)color.B;
	}

	public void SetControls(StackPanel filter_panel1, StackPanel filter_panel2, ComboBox filter_combo1, ComboBox filter_combo2, TextBox filter_text, TextBox filter_text1, TextBox filter_text2)
	{
		this.filter_panel1 = filter_panel1;
		this.filter_panel2 = filter_panel2;
		this.filter_combo1 = filter_combo1;
		this.filter_combo2 = filter_combo2;
		this.filter_text = filter_text;
		this.filter_text1 = filter_text1;
		this.filter_text2 = filter_text2;
	}

	public void Setup(IFactorio factorio, List<PopupLine> lines, PnImageSource imageSource, Action exitCode2, int staticListMode, ComboBox filterCombo, bool updateFilters, Action<bool> updateEnableSortingButtons, int filtersCount, bool is_EditButtonVisible, string popupName)
	{
		this.is_EditButtonVisible = is_EditButtonVisible;
		this.quickTableSortList = new QuickTableSortList();
		this.deselectedByFilteringItems = new List<QuickTableRecord>();
		((ListCollectionView)CollectionViewSource.GetDefaultView(this.dataGrid.ItemsSource)).CustomSort = null;
		this.filterCombo = filterCombo;
		this._filtersCount = filtersCount;
		this.mouseupforignore = true;
		if (!this.ToolTipHandlerRegister)
		{
			EventManager.RegisterClassHandler(typeof(FrameworkElement), FrameworkElement.ToolTipOpeningEvent, new ToolTipEventHandler(ToolTipHandler));
			this.ToolTipHandlerRegister = true;
		}
		this._isAnySum = false;
		this._isSingleListMode = false;
		List<QuickTableRecord> list = new List<QuickTableRecord>();
		List<QuickTableRecord> list2 = new List<QuickTableRecord>();
		this._logCenterService = factorio.Resolve<ILogCenterService>();
		this._languageDictionary = factorio.Resolve<ILanguageDictionary>();
		this._pnColorsService = factorio.Resolve<IPnColorsService>();
		this._SortButtonsEnableFunction = updateEnableSortingButtons;
		this.SetButtonsEnabel(v: false);
		this._filterCombo = filterCombo;
		this._staticListMode = staticListMode;
		this._imageSource = imageSource;
		this._exitCode2 = exitCode2;
		foreach (PopupLine line in lines)
		{
			line.text = line.text.Replace("\\v", "\v");
			line.ntext = line.ntext.Replace("\\v", "\v");
		}
		string[] array = lines[0].text.Split('\v');
		this._setupColumnInfo = new List<QuickTableColumnInfo>();
		this.dataGrid.Columns.Clear();
		int num = 0;
		for (int i = 0; i < array.Length; i++)
		{
			QuickTableColumnInfo quickTableColumnInfo = new QuickTableColumnInfo(array[i], this._logCenterService);
			quickTableColumnInfo.Column = new DataGridTextColumn
			{
				Header = quickTableColumnInfo.Label
			};
			quickTableColumnInfo.Column.Binding = new Binding($"Data[{num}]");
			quickTableColumnInfo.Column.Binding.StringFormat = this.GetStringFormatForColumn(quickTableColumnInfo.Type, quickTableColumnInfo.FormatModification);
			quickTableColumnInfo.Column.ElementStyle = this.HorizontalContentAlignmentGet(quickTableColumnInfo.Type);
			this.dataGrid.Columns.Add(quickTableColumnInfo.Column);
			this._setupColumnInfo.Add(quickTableColumnInfo);
			if (filterCombo != null && updateFilters)
			{
				filterCombo.Items.Add(quickTableColumnInfo.Label);
			}
			num++;
		}
		this.SetupFilters(lines[0].ntext);
		if (lines.Count == 1)
		{
			this._isSingleListMode = true;
			this.dataGrid.SelectionMode = DataGridSelectionMode.Single;
		}
		for (int j = 1; j < lines.Count; j++)
		{
			if (j == 1)
			{
				this._isSingleListMode = lines[j].typ == 6;
				if (this._isSingleListMode)
				{
					this.dataGrid.SelectionMode = DataGridSelectionMode.Single;
				}
				else
				{
					this.dataGrid.SelectionMode = DataGridSelectionMode.Extended;
				}
			}
			QuickTableRecord quickTableRecord = new QuickTableRecord();
			object helpTextOrImageKey = this.GetHelpTextOrImageKey(lines[j]);
			if (helpTextOrImageKey != null)
			{
				quickTableRecord.RecordToolTip = new ToolTip();
				quickTableRecord.RecordToolTip.Content = helpTextOrImageKey;
			}
			quickTableRecord.Id1 = lines[j].id1;
			quickTableRecord.Inte1 = lines[j].inte1;
			if (lines[j].sel == -1)
			{
				quickTableRecord.IsSelectable = false;
			}
			if (lines[j].real4 != 0.0)
			{
				Color wpfColor = this._pnColorsService.GetWpfColor((int)lines[j].real4);
				quickTableRecord.ConstBrush = new SolidColorBrush(wpfColor);
				if (QuickTable1.NaturalBrightness(wpfColor) < 128.0)
				{
					quickTableRecord.FontBrush = new SolidColorBrush(Colors.White);
				}
			}
			string[] array2 = lines[j].text.Split('\v');
			int num2 = 0;
			string[] array3 = array2;
			foreach (string s in array3)
			{
				if (num2 < this._setupColumnInfo.Count)
				{
					try
					{
						quickTableRecord.Data.Add(this.PopupString2Object(s, this._setupColumnInfo[num2]));
					}
					catch (Exception e)
					{
						this._logCenterService.CatchRaport(e);
					}
				}
				num2++;
			}
			list.Add(quickTableRecord);
			if (lines[j].sel == 1)
			{
				list2.Add(quickTableRecord);
			}
		}
		if (this.IsAnySum())
		{
			QuickTableRecord quickTableRecord2 = new QuickTableRecord();
			for (int l = 0; l < num; l++)
			{
				quickTableRecord2.Data.Add(null);
			}
			quickTableRecord2.IsDataRecord = false;
			list.Add(quickTableRecord2);
			this._isAnySum = true;
		}
		this.TableRecords.Clear();
		foreach (QuickTableRecord item in list)
		{
			this.TableRecords.Add(item);
		}
		if (this._isSingleListMode)
		{
			if (list2.Count > 0)
			{
				this.dataGrid.SelectedItem = list2[0];
			}
		}
		else
		{
			this.dataGrid.SelectManyItems(list2, clear: true);
		}
		int id = lines[0].id4;
		if (id != 0)
		{
			ListSortDirection listSortDirection = ListSortDirection.Ascending;
			if (id < 0)
			{
				listSortDirection = ListSortDirection.Descending;
			}
			DataGridColumn column = this.dataGrid.Columns[Math.Abs(id) - 1];
			IComparer comparer = null;
			column.SortDirection = listSortDirection;
			ListCollectionView obj = (ListCollectionView)CollectionViewSource.GetDefaultView(this.dataGrid.ItemsSource);
			QuickTableColumnInfo quickTableColumnInfo2 = this._setupColumnInfo.Where((QuickTableColumnInfo a) => a.Column == column).First();
			this.quickTableSortList.Add(listSortDirection, quickTableColumnInfo2, this._setupColumnInfo.IndexOf(quickTableColumnInfo2));
			obj.CustomSort = new QuickTableSortResult(this.quickTableSortList);
		}
		this.CalculateSums();
		if (this.dispatcherTimer != null)
		{
			this.dispatcherTimer.Stop();
		}
		this.dispatcherTimer = new DispatcherTimer();
		this.dispatcherTimer.Tick += dispatcherTimer_Tick;
		this.dispatcherTimer.Interval = new TimeSpan(3333333L);
		this.dispatcherTimer.Start();
	}

	private void SetupFilters(string ntext)
	{
		this.FiltersSetup = new int[this._filtersCount];
		for (int i = 0; i < this._filtersCount; i++)
		{
			this.FiltersSetup[i] = 0;
		}
		if (this._filtersCount == 0)
		{
			return;
		}
		if (this._filtersCount > 1)
		{
			this.filter_combo2.Items.Clear();
			foreach (string item in (IEnumerable)this.filterCombo.Items)
			{
				this.filter_combo2.Items.Add(item);
			}
		}
		if (this._filtersCount > 2)
		{
			this.filter_combo1.Items.Clear();
			foreach (string item2 in (IEnumerable)this.filterCombo.Items)
			{
				this.filter_combo1.Items.Add(item2);
			}
		}
		string[] array = ntext.Trim().Split('\v');
		if (array.Length == 1 && array[0] == string.Empty)
		{
			array = new string[0];
		}
		int num = array.Length;
		if (num > this._filtersCount)
		{
			num = this._filtersCount;
		}
		for (int j = 0; j < num; j++)
		{
			if (!int.TryParse(array[j], out this.FiltersSetup[j]))
			{
				this.FiltersSetup[j] = 0;
			}
		}
		switch (this.FiltersSetup.Length)
		{
		case 1:
			this.filterCombo.SelectedIndex = this.FiltersSetup[0];
			break;
		case 2:
			this.filter_combo2.SelectedIndex = this.FiltersSetup[0];
			this.filterCombo.SelectedIndex = this.FiltersSetup[1];
			break;
		case 3:
			this.filter_combo1.SelectedIndex = this.FiltersSetup[0];
			this.filter_combo2.SelectedIndex = this.FiltersSetup[1];
			this.filterCombo.SelectedIndex = this.FiltersSetup[2];
			break;
		}
	}

	public void KeyboardFocusRightFilter()
	{
		if (this._filtersCount < 2)
		{
			Keyboard.Focus(this.filter_text);
		}
		else if (this._filtersCount == 2)
		{
			Keyboard.Focus(this.filter_text2);
		}
		else
		{
			Keyboard.Focus(this.filter_text1);
		}
	}

	private void dispatcherTimer_Tick(object sender, EventArgs e)
	{
		this.dispatcherTimer.Stop();
		this.ScrollToSelected();
	}

	public void SendAnswer(string ignore, string ignore1, string ignore2)
	{
		this.SendAnswer(0, ignore, ignore1, ignore2);
	}

	public void SendAnswer(int startIdx, object ignore, object ignore1, object ignore2)
	{
		int num = 1;
		foreach (QuickTableRecord item in (IEnumerable)this.dataGrid.Items)
		{
			if (!item.IsDataRecord)
			{
				continue;
			}
			int idx = num + startIdx;
			int value = 2;
			if (this.Filter(item))
			{
				value = (this.dataGrid.SelectedItems.Contains(item) ? 1 : 0);
			}
			PopupAdapter.Popup_Line_IPOSEL_set(idx, value);
			PopupAdapter.Popup_Line_IPOPZ_set(idx, item.Inte1);
			PopupAdapter.Popup_Line_IPOPID_set(idx, item.Id1);
			StringBuilder stringBuilder = new StringBuilder(600);
			for (int i = 0; i < item.Data.Count; i++)
			{
				if (i > 0 && i < item.Data.Count)
				{
					stringBuilder.Append('\v');
				}
				stringBuilder.Append(this.GetFormatingStringFromData(item.Data[i], this._setupColumnInfo[i].FormatModification));
			}
			PopupAdapter.Popup_Line_POPBEM_set(idx, stringBuilder.ToString());
			num++;
		}
	}

	public void PageDown()
	{
		QuickTable1.GetVisualChild<ScrollViewer>(this.dataGrid)?.PageDown();
	}

	public void PageUp()
	{
		QuickTable1.GetVisualChild<ScrollViewer>(this.dataGrid)?.PageUp();
	}

	private List<QuickTableRecord> GetDataAtCurrentOrder()
	{
		ListCollectionView listCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(this.dataGrid.ItemsSource);
		List<QuickTableRecord> list = new List<QuickTableRecord>();
		for (int i = 0; i < listCollectionView.Count; i++)
		{
			list.Add(null);
		}
		foreach (QuickTableRecord tableRecord in this.TableRecords)
		{
			list[listCollectionView.IndexOf(tableRecord)] = tableRecord;
		}
		listCollectionView.CustomSort = null;
		return list;
	}

	private List<Tuple<int, QuickTableRecord>> GetSortedSelected(List<QuickTableRecord> current, bool sorttop)
	{
		List<Tuple<int, QuickTableRecord>> list = new List<Tuple<int, QuickTableRecord>>();
		foreach (QuickTableRecord selectedItem in this.dataGrid.SelectedItems)
		{
			list.Add(new Tuple<int, QuickTableRecord>(current.IndexOf(selectedItem), selectedItem));
		}
		if (sorttop)
		{
			list.Sort(delegate(Tuple<int, QuickTableRecord> x, Tuple<int, QuickTableRecord> y)
			{
				if (x.Item1 == y.Item1)
				{
					return 0;
				}
				return (x.Item1 > y.Item1) ? 1 : (-1);
			});
		}
		else
		{
			list.Sort(delegate(Tuple<int, QuickTableRecord> x, Tuple<int, QuickTableRecord> y)
			{
				if (x.Item1 == y.Item1)
				{
					return 0;
				}
				return (x.Item1 < y.Item1) ? 1 : (-1);
			});
		}
		return list;
	}

	private void UpdateWithCurrent(List<QuickTableRecord> current, List<Tuple<int, QuickTableRecord>> selected)
	{
		this.TableRecords.Clear();
		foreach (QuickTableRecord item in current)
		{
			this.TableRecords.Add(item);
		}
		this.dataGrid.SelectedItems.Clear();
		foreach (Tuple<int, QuickTableRecord> item2 in selected)
		{
			this.dataGrid.SelectedItems.Add(item2.Item2);
		}
		this.ScrollToSelected();
	}

	private void ScrollToSelected()
	{
		if (this.dataGrid.SelectedItems.Count != 0)
		{
			ScrollViewer visualChild = QuickTable1.GetVisualChild<ScrollViewer>(this.dataGrid);
			if (visualChild != null)
			{
				int num = this.TableRecords.IndexOf((QuickTableRecord)this.dataGrid.SelectedItems[0]);
				int count = this.TableRecords.Count;
				visualChild.ScrollToVerticalOffset(visualChild.ScrollableHeight / (double)count * (double)(num + 1));
			}
		}
	}

	public void MoveSelectedToStart()
	{
		if (this.dataGrid.SelectedItems.Count == 0)
		{
			return;
		}
		List<QuickTableRecord> dataAtCurrentOrder = this.GetDataAtCurrentOrder();
		List<Tuple<int, QuickTableRecord>> sortedSelected = this.GetSortedSelected(dataAtCurrentOrder, sorttop: true);
		int item = sortedSelected[0].Item1;
		if (item == 0)
		{
			return;
		}
		foreach (Tuple<int, QuickTableRecord> item3 in sortedSelected)
		{
			int item2 = item3.Item1;
			dataAtCurrentOrder.RemoveAt(item2);
			dataAtCurrentOrder.Insert(item2 - item, item3.Item2);
		}
		this.UpdateWithCurrent(dataAtCurrentOrder, sortedSelected);
	}

	public void MoveSelectedOneUp()
	{
		if (this.dataGrid.SelectedItems.Count == 0)
		{
			return;
		}
		List<QuickTableRecord> dataAtCurrentOrder = this.GetDataAtCurrentOrder();
		List<Tuple<int, QuickTableRecord>> sortedSelected = this.GetSortedSelected(dataAtCurrentOrder, sorttop: true);
		if (sortedSelected[0].Item1 == 0)
		{
			return;
		}
		foreach (Tuple<int, QuickTableRecord> item2 in sortedSelected)
		{
			int item = item2.Item1;
			dataAtCurrentOrder.RemoveAt(item);
			dataAtCurrentOrder.Insert(item - 1, item2.Item2);
		}
		this.UpdateWithCurrent(dataAtCurrentOrder, sortedSelected);
	}

	public void MoveSelectedOneDown()
	{
		if (this.dataGrid.SelectedItems.Count == 0)
		{
			return;
		}
		List<QuickTableRecord> dataAtCurrentOrder = this.GetDataAtCurrentOrder();
		List<Tuple<int, QuickTableRecord>> sortedSelected = this.GetSortedSelected(dataAtCurrentOrder, sorttop: false);
		int item = sortedSelected[0].Item1;
		int num = 0;
		if (this.IsAnySum())
		{
			num = 1;
		}
		if (item == this.dataGrid.Items.Count - 1 - num)
		{
			return;
		}
		foreach (Tuple<int, QuickTableRecord> item3 in sortedSelected)
		{
			int item2 = item3.Item1;
			dataAtCurrentOrder.RemoveAt(item2);
			dataAtCurrentOrder.Insert(item2 + 1, item3.Item2);
		}
		this.UpdateWithCurrent(dataAtCurrentOrder, sortedSelected);
	}

	public void MoveSelectedToEnd()
	{
		if (this.dataGrid.SelectedItems.Count == 0)
		{
			return;
		}
		List<QuickTableRecord> dataAtCurrentOrder = this.GetDataAtCurrentOrder();
		List<Tuple<int, QuickTableRecord>> sortedSelected = this.GetSortedSelected(dataAtCurrentOrder, sorttop: false);
		int item = sortedSelected[0].Item1;
		int num = 0;
		if (this.IsAnySum())
		{
			num = 1;
		}
		if (item == this.dataGrid.Items.Count - 1 - num)
		{
			return;
		}
		foreach (Tuple<int, QuickTableRecord> item3 in sortedSelected)
		{
			int item2 = item3.Item1;
			dataAtCurrentOrder.RemoveAt(item2);
			dataAtCurrentOrder.Insert(item2 + this.dataGrid.Items.Count - num - item - 1, item3.Item2);
		}
		this.UpdateWithCurrent(dataAtCurrentOrder, sortedSelected);
	}

	private void SetButtonsEnabel()
	{
		this.SetButtonsEnabel(this.dataGrid.SelectedItems.Count > 0 && string.IsNullOrEmpty(this.FilterString));
	}

	private void SetButtonsEnabel(bool v)
	{
		this._SortButtonsEnableFunction?.Invoke(v);
	}

	private object PopupString2Object(string s, QuickTableColumnInfo qtc)
	{
		object result = null;
		switch (qtc.Type)
		{
		case 1:
			result = s;
			break;
		case 2:
			result = this.PopupString2Int(s);
			break;
		case 3:
			result = this.PopupString2Double(s);
			break;
		case 4:
			result = this.PopupString2DateTime(s);
			break;
		case 5:
			result = this.PopupString2TimeSpan(s);
			break;
		}
		return result;
	}

	private TimeSpan PopupString2TimeSpan(string str)
	{
		try
		{
			int num = (int)(Convert.ToDouble(str, CultureInfo.InvariantCulture) * 60.0);
			int num2 = num / 3600;
			int num3 = num - num2 * 3600;
			int num4 = num3 / 60;
			return new TimeSpan(num2, num4, num3 - num4 * 60);
		}
		catch (Exception)
		{
		}
		return default(TimeSpan);
	}

	private DateTime PopupString2DateTime(string str)
	{
		int num = str.IndexOf('.');
		int num2 = str.IndexOf('.', num + 1);
		if (num > 0 && num2 > num)
		{
			try
			{
				return new DateTime(Convert.ToInt32(str.Substring(num2 + 1)), Convert.ToInt32(str.Substring(num + 1, num2 - num - 1)), Convert.ToInt32(str.Substring(0, num)));
			}
			catch (Exception)
			{
			}
		}
		return default(DateTime);
	}

	private double PopupString2Double(string str)
	{
		double result = 0.0;
		double.TryParse(str, NumberStyles.Any, CultureInfo.InvariantCulture, out result);
		return result;
	}

	private int PopupString2Int(string str)
	{
		int result = 0;
		int.TryParse(str, out result);
		return result;
	}

	private string GetStringFormatForColumn(int type, int format_modification)
	{
		string result = string.Empty;
		switch (type)
		{
		case 3:
			result = ((format_modification <= 0) ? ((this._languageDictionary.GetInchMode() != 1) ? "0.00" : "0.0000") : ("0." + string.Empty.PadRight(format_modification, '0')));
			break;
		case 4:
			result = "dd.MM.yyyy";
			break;
		}
		return result;
	}

	private Style HorizontalContentAlignmentGet(int type)
	{
		Style style = new Style();
		Setter item = null;
		switch (type)
		{
		case 1:
			item = new Setter(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Left);
			break;
		case 2:
			item = new Setter(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Right);
			break;
		case 3:
			item = new Setter(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Right);
			break;
		case 4:
			item = new Setter(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Right);
			break;
		case 5:
			item = new Setter(FrameworkElement.HorizontalAlignmentProperty, HorizontalAlignment.Right);
			break;
		}
		style.Setters.Add(item);
		item = new Setter(FrameworkElement.MarginProperty, new Thickness(3.0));
		style.Setters.Add(item);
		return style;
	}

	private bool IsAnySum()
	{
		foreach (QuickTableColumnInfo item in this._setupColumnInfo)
		{
			if (item.Sum)
			{
				return true;
			}
		}
		return false;
	}

	private void CalculateSums()
	{
		if (!this._isAnySum || this.TableRecords.Count == 0)
		{
			return;
		}
		bool flag = this.dataGrid.SelectedItems != null && this.dataGrid.SelectedItems.Count > 0;
		List<object> list = new List<object>();
		for (int i = 0; i < this._setupColumnInfo.Count; i++)
		{
			list.Add(null);
		}
		foreach (QuickTableRecord item in (IEnumerable)this.dataGrid.Items)
		{
			if (!item.IsDataRecord || (flag && !this.dataGrid.SelectedItems.Contains(item)))
			{
				continue;
			}
			for (int j = 0; j < item.Data.Count; j++)
			{
				if (!this._setupColumnInfo[j].Sum)
				{
					continue;
				}
				if (item.Data[j] is TimeSpan)
				{
					if (list[j] == null)
					{
						list[j] = (TimeSpan)item.Data[j];
					}
					else
					{
						list[j] = (TimeSpan)list[j] + (TimeSpan)item.Data[j];
					}
				}
				if (item.Data[j] is double)
				{
					if (list[j] == null)
					{
						list[j] = (double)item.Data[j];
					}
					else
					{
						list[j] = (double)list[j] + (double)item.Data[j];
					}
				}
				if (item.Data[j] is int)
				{
					if (list[j] == null)
					{
						list[j] = (int)item.Data[j];
					}
					else
					{
						list[j] = (int)list[j] + (int)item.Data[j];
					}
				}
			}
		}
		QuickTableRecord quickTableRecord2 = this.TableRecords[this.TableRecords.Count - 1];
		for (int k = 0; k < this._setupColumnInfo.Count; k++)
		{
			quickTableRecord2.Data[k] = list[k];
		}
	}

	private void dataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		ListCollectionView listCollectionView = (ListCollectionView)CollectionViewSource.GetDefaultView(this.dataGrid.ItemsSource);
		if (!this.is_EditButtonVisible)
		{
			foreach (object removedItem in e.RemovedItems)
			{
				if (!listCollectionView.Contains(removedItem))
				{
					this.deselectedByFilteringItems.Add(removedItem as QuickTableRecord);
				}
			}
		}
		List<QuickTableRecord> list = new List<QuickTableRecord>();
		foreach (QuickTableRecord selectedItem in this.dataGrid.SelectedItems)
		{
			if (!selectedItem.IsForEnabled)
			{
				list.Add(selectedItem);
			}
		}
		foreach (QuickTableRecord item in list)
		{
			this.dataGrid.SelectedItems.Remove(item);
		}
		this.CalculateSums();
		this.SetButtonsEnabel();
	}

	private void dataGrid_Sorting(object sender, DataGridSortingEventArgs e)
	{
		DataGridColumn column = e.Column;
		IComparer comparer = null;
		e.Handled = true;
		ListSortDirection listSortDirection = ((column.SortDirection == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending);
		column.SortDirection = listSortDirection;
		ListCollectionView obj = (ListCollectionView)CollectionViewSource.GetDefaultView(this.dataGrid.ItemsSource);
		QuickTableColumnInfo quickTableColumnInfo = this._setupColumnInfo.Where((QuickTableColumnInfo a) => a.Column == column).First();
		this.quickTableSortList.Add(listSortDirection, quickTableColumnInfo, this._setupColumnInfo.IndexOf(quickTableColumnInfo));
		obj.CustomSort = new QuickTableSortResult(this.quickTableSortList);
		this.CalculateSums();
	}

	private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		this.ScrollReset();
	}

	public void ScrollReset()
	{
		ScrollViewer visualChild = QuickTable1.GetVisualChild<ScrollViewer>(this.dataGrid);
		if (visualChild != null)
		{
			visualChild.ScrollToVerticalOffset(0.0);
			visualChild.ScrollToHorizontalOffset(0.0);
		}
	}

	private static T GetVisualChild<T>(DependencyObject parent) where T : Visual
	{
		T val = null;
		int childrenCount = VisualTreeHelper.GetChildrenCount(parent);
		for (int i = 0; i < childrenCount; i++)
		{
			Visual visual = (Visual)VisualTreeHelper.GetChild(parent, i);
			val = visual as T;
			if (val == null)
			{
				val = QuickTable1.GetVisualChild<T>(visual);
			}
			if (val != null)
			{
				break;
			}
		}
		return val;
	}

	private string GetHelpTextOrImageKey(PopupLine popupLine)
	{
		string text = string.Empty;
		if (popupLine.htext != string.Empty)
		{
			text = popupLine.htext;
		}
		else if (popupLine.id3 > 0)
		{
			text = this._languageDictionary.GetPophlp(popupLine.id3);
		}
		if (text == null || text.Length < 3)
		{
			return null;
		}
		if (text[0] == '#')
		{
			return popupLine.htext.Substring(1);
		}
		if (this.ImageFileExist(popupLine.htext))
		{
			return $"IMG:{popupLine.htext}";
		}
		return popupLine.htext;
	}

	private bool ImageFileExist(string key)
	{
		string[] array = key.Split(' ');
		if (array.Length == 2 && array[1].Contains("="))
		{
			return File.Exists(array[0]);
		}
		return File.Exists(key);
	}

	private void ToolTipHandler(object sender, ToolTipEventArgs e)
	{
		if (!(sender is DataGridRow))
		{
			return;
		}
		FrameworkElement frameworkElement = e.Source as FrameworkElement;
		if (!(frameworkElement.ToolTip is ToolTip))
		{
			return;
		}
		ToolTip toolTip = (ToolTip)frameworkElement.ToolTip;
		if (toolTip.Content is string)
		{
			string text = toolTip.Content as string;
			if (text.Length > 4 && text.Substring(0, 4) == "IMG:")
			{
				string key = text.Substring(4);
				Image image = new Image();
				image.SnapsToDevicePixels = true;
				image.BeginInit();
				image.Source = this._imageSource.GetImageSource(key, 200, 200, 0);
				image.EndInit();
				toolTip.Content = image;
			}
		}
	}

	private bool CheckTreeForListViewItem(DependencyObject obj)
	{
		if (obj is DataGridCell)
		{
			return true;
		}
		DependencyObject parent = VisualTreeHelper.GetParent(obj);
		if (parent == null)
		{
			return false;
		}
		if (parent is ListView)
		{
			return false;
		}
		return this.CheckTreeForListViewItem(parent);
	}

	private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed && this._staticListMode == 0 && this.CheckTreeForListViewItem((DependencyObject)e.OriginalSource) && this.dataGrid.SelectedItems.Count != 0)
		{
			this._exitCode2?.Invoke();
		}
	}

	private void dataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (!this.CheckTreeForListViewItem((DependencyObject)e.OriginalSource))
		{
			if (this._isSingleListMode)
			{
				this.dataGrid.SelectedItem = null;
			}
			else
			{
				this.dataGrid.SelectedItems.Clear();
			}
		}
	}

	private void dataGrid_PreviewMouseUp(object sender, MouseButtonEventArgs e)
	{
		if (this.mouseupforignore)
		{
			this.mouseupforignore = false;
			e.Handled = true;
		}
	}

	private void dataGrid_PreviewMouseDown(object sender, MouseButtonEventArgs e)
	{
		this.mouseupforignore = false;
	}

	public void SetVisibility(Visibility v)
	{
		base.Visibility = v;
	}

	public void SetFocusable(bool v)
	{
		base.Focusable = v;
	}

	public Visibility CheckVisibility()
	{
		return base.Visibility;
	}
}
