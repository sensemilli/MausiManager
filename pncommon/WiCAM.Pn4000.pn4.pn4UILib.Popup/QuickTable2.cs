using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using Telerik.Windows.Controls;
using Telerik.Windows.Controls.GridView;
using Telerik.Windows.Data;
using WiCAM.Pn4000.Contracts.DependencyInjection;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.PKernelFlow.Adapters;
using WiCAM.Pn4000.PKernelFlow.Adapters.Data;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.pn4.pn4Services.CADGEO;
using WiCAM.Pn4000.pn4.pn4UILib.Popup.Information;

namespace WiCAM.Pn4000.pn4.pn4UILib.Popup;

public partial class QuickTable2 : UserControl, IQuickTable, IComponentConnector
{
	private ILogCenterService _logCenterService;

	private ILanguageDictionary _languageDictionary;

	private IPnColorsService _pnColorsService;

	private bool _is_EditButtonVisible;

	private bool _isSingleListMode;

	private int _staticListMode;

	private PnImageSource _imageSource;

	private Action _exitCode2;

	private Action<bool> _SortButtonsEnableFunction;

	public bool ToolTipHandlerRegister;

	private List<QuickTableColumnInfo> _setupColumnInfo;

	private bool _isAnySum;

	private List<QuickTableRecord> deselectedByFilteringItems = new List<QuickTableRecord>();

	public string FilterString { get; set; }

	public string FilterString1 { get; set; }

	public string FilterString2 { get; set; }

	public Popup MainPopupWindow { get; set; }

	private RadGridView dataGrid => this.DataViewContainter.GridView;

	public RadObservableCollection<QuickTableRecord> TableRecords { get; set; } = new RadObservableCollection<QuickTableRecord>();

	public QuickTable2()
	{
		this.InitializeComponent();
		base.DataContext = this;
	}

	public void KeyboardFocusRightFilter()
	{
		this.KeyboardFocus();
	}

	public void PageDown()
	{
		QuickTable2.GetVisualChild<ScrollViewer>(this.dataGrid)?.PageDown();
	}

	public void PageUp()
	{
		QuickTable2.GetVisualChild<ScrollViewer>(this.dataGrid)?.PageUp();
	}

	public void ScrollReset()
	{
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

	public void SetControls(StackPanel filter_panel1, StackPanel filter_panel2, ComboBox filter_combo1, ComboBox filter_combo2, TextBox filter_text, TextBox filter_text1, TextBox filter_text2)
	{
	}

	public void SetFilter(string empty1, string empty2, string empty3)
	{
	}

	public void Setup(IFactorio factorio, List<PopupLine> lines, PnImageSource imageSource, Action exitCode2, int staticListMode, ComboBox filter_combo, bool updateFilters, Action<bool> updateEnableSortingButtons, int filtersCount, bool is_EditButtonVisible, string popupName)
	{
		if (this.dataGrid != null)
		{
			this.dataGrid.SelectionChanged -= dataGrid_SelectionChanged;
			this.dataGrid.Filtered -= dataGrid_Filtered;
			this.dataGrid.MouseLeftButtonDown -= dataGrid_MouseLeftButtonDown;
			this.dataGrid.Grouped -= dataGrid_Grouped;
			this.dataGrid.MouseDoubleClick -= dataGrid_MouseDoubleClick;
		}
		this.DataViewContainter.GridView = new RadGridView
		{
			AutoGenerateColumns = false,
			ShowColumnFooters = true,
			IsReadOnly = true,
			FilteringDropDownStaysOpen = false,
			ShouldCloseFilteringPopupOnKeyboardFocusChanged = true,
			GroupRenderMode = GroupRenderMode.Flat,
			RightFrozenColumnsSplitterVisibility = Visibility.Visible
		};
		this.dataGrid.SelectionChanged += dataGrid_SelectionChanged;
		this.dataGrid.Filtered += dataGrid_Filtered;
		this.dataGrid.MouseLeftButtonDown += dataGrid_MouseLeftButtonDown;
		this.dataGrid.Grouped += dataGrid_Grouped;
		this.dataGrid.MouseDoubleClick += dataGrid_MouseDoubleClick;
		this._is_EditButtonVisible = is_EditButtonVisible;
		this._isSingleListMode = false;
		this._staticListMode = staticListMode;
		this._imageSource = imageSource;
		this._exitCode2 = exitCode2;
		this._SortButtonsEnableFunction = updateEnableSortingButtons;
		this.SetButtonsEnabel(v: false);
		this._logCenterService = factorio.Resolve<ILogCenterService>();
		this._languageDictionary = factorio.Resolve<ILanguageDictionary>();
		this._pnColorsService = factorio.Resolve<IPnColorsService>();
		if (!this.ToolTipHandlerRegister)
		{
			EventManager.RegisterClassHandler(typeof(FrameworkElement), FrameworkElement.ToolTipOpeningEvent, new ToolTipEventHandler(ToolTipHandler));
			this.ToolTipHandlerRegister = true;
		}
		foreach (PopupLine line in lines)
		{
			line.text = line.text.Replace("\\v", "\v");
			line.ntext = line.ntext.Replace("\\v", "\v");
		}
		this.dataGrid.FrozenColumnCount = 0;
		this.dataGrid.RightFrozenColumnCount = 0;
		this.TableRecords.Clear();
		this.dataGrid.ItemsSource = this.TableRecords;
		this.deselectedByFilteringItems.Clear();
		string[] array = lines[0].text.Split('\v');
		this._setupColumnInfo = new List<QuickTableColumnInfo>();
		for (int i = 0; i < array.Length; i++)
		{
			QuickTableColumnInfo quickTableColumnInfo = new QuickTableColumnInfo(array[i], this._logCenterService);
			quickTableColumnInfo.RadColumn = new GridViewDataColumn
			{
				Header = quickTableColumnInfo.Label
			};
			quickTableColumnInfo.RadColumn.DataMemberBinding = new Binding($"Data[{i}]");
			quickTableColumnInfo.RadColumn.DataFormatString = this.GetStringFormatForColumn(quickTableColumnInfo.Type, quickTableColumnInfo.FormatModification);
			quickTableColumnInfo.RadColumn.DataType = quickTableColumnInfo.GetColumnType();
			quickTableColumnInfo.RadColumn.TextAlignment = this.HorizontalContentAlignmentGet(quickTableColumnInfo.Type);
			quickTableColumnInfo.RadColumn.FooterTextAlignment = this.HorizontalContentAlignmentGet(quickTableColumnInfo.Type);
			this.dataGrid.Columns.Add(quickTableColumnInfo.RadColumn);
			if (quickTableColumnInfo.Type == 3)
			{
				quickTableColumnInfo.Round = this.GetColumnRound(quickTableColumnInfo.FormatModification);
			}
			this._setupColumnInfo.Add(quickTableColumnInfo);
		}
		if (lines.Count == 1)
		{
			this._isSingleListMode = true;
			this.dataGrid.SelectionMode = SelectionMode.Single;
		}
		List<QuickTableRecord> list = new List<QuickTableRecord>();
		List<QuickTableRecord> list2 = new List<QuickTableRecord>();
		for (int j = 1; j < lines.Count; j++)
		{
			if (j == 1)
			{
				this._isSingleListMode = lines[j].typ == 6;
				if (this._isSingleListMode)
				{
					this.dataGrid.SelectionMode = SelectionMode.Single;
				}
				else
				{
					this.dataGrid.SelectionMode = SelectionMode.Extended;
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
				quickTableRecord.IsColored = true;
				Color wpfColor = this._pnColorsService.GetWpfColor((int)lines[j].real4);
				quickTableRecord.ConstBrush = new SolidColorBrush(wpfColor);
				if (QuickTable2.NaturalBrightness(wpfColor) < 128.0)
				{
					quickTableRecord.FontBrush = new SolidColorBrush(Colors.White);
				}
			}
			string[] array2 = lines[j].text.Split('\v');
			int k = 0;
			string[] array3 = array2;
			foreach (string s in array3)
			{
				if (k < this._setupColumnInfo.Count)
				{
					try
					{
						quickTableRecord.Data.Add(this.PopupString2Object(s, this._setupColumnInfo[k]));
					}
					catch (Exception e)
					{
						this._logCenterService.CatchRaport(e);
					}
				}
				k++;
			}
			for (; k < this._setupColumnInfo.Count; k++)
			{
				quickTableRecord.Data.Add(this.PopupString2Object(string.Empty, this._setupColumnInfo[k]));
			}
			list.Add(quickTableRecord);
			if (lines[j].sel == 1)
			{
				list2.Add(quickTableRecord);
			}
		}
		this._isAnySum = this.IsAnySum();
		this.dataGrid.ShowColumnFooters = this._isAnySum;
		this.TableRecords.AddRange(list);
		if (this._isSingleListMode)
		{
			if (list2.Count > 0)
			{
				this.dataGrid.SelectedItems.Add(list2[0]);
			}
		}
		else
		{
			this.dataGrid.SelectedItems.AddRange(list2);
		}
		this.DataViewContainter.StorageId = "QuickTable2_" + popupName;
		this.DataViewContainter.SetCurrentLayoutAsDefault();
		this.DataViewContainter.ColumnsSettingsLoad();
		this.dataGrid.UpdateLayout();
		this.CalculateSums();
	}

	public void SendAnswer(string ignore, string ignore1, string ignore2)
	{
		this.SendAnswer(0, ignore, ignore1, ignore2);
	}

	public void SendAnswer(int startIdx, object ignore, object ignore1, object ignore2)
	{
		int num = 1;
		foreach (QuickTableRecord tableRecord in this.TableRecords)
		{
			int idx = num + startIdx;
			int value = 2;
			if (this.dataGrid.Items.Contains(tableRecord))
			{
				value = (this.dataGrid.SelectedItems.Contains(tableRecord) ? 1 : 0);
			}
			this.deselectedByFilteringItems.Contains(tableRecord);
			PopupAdapter.Popup_Line_IPOSEL_set(idx, value);
			PopupAdapter.Popup_Line_IPOPZ_set(idx, tableRecord.Inte1);
			PopupAdapter.Popup_Line_IPOPID_set(idx, tableRecord.Id1);
			StringBuilder stringBuilder = new StringBuilder(600);
			for (int i = 0; i < tableRecord.Data.Count; i++)
			{
				if (i > 0 && i < tableRecord.Data.Count)
				{
					stringBuilder.Append('\v');
				}
				stringBuilder.Append(this.GetFormatingStringFromData(tableRecord.Data[i], this._setupColumnInfo[i].FormatModification));
			}
			PopupAdapter.Popup_Line_POPBEM_set(idx, stringBuilder.ToString());
			num++;
		}
		this.DataViewContainter.CheckAutosave();
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
		if (data is TimeSpan)
		{
			return this.TimeSpanToDecimalMinutesString((TimeSpan)data);
		}
		return string.Empty;
	}

	private string TimeSpanToDecimalMinutesString(TimeSpan timeSpan)
	{
		return (timeSpan.TotalSeconds / 60.0).ToString(CultureInfo.InvariantCulture);
	}

	private int GetColumnRound(int formatModification)
	{
		if (formatModification > 0)
		{
			return formatModification;
		}
		if (this._languageDictionary.GetInchMode() == 1)
		{
			return 4;
		}
		return 2;
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

	private TextAlignment HorizontalContentAlignmentGet(int type)
	{
		if (type == 1)
		{
			return TextAlignment.Left;
		}
		return TextAlignment.Right;
	}

	public static double NaturalBrightness(Color color)
	{
		return 0.21 * (double)(int)color.R + 0.72 * (double)(int)color.G + 0.07 * (double)(int)color.B;
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
		foreach (QuickTableRecord item in this.dataGrid.Items)
		{
			if (flag && !this.dataGrid.SelectedItems.Contains(item))
			{
				continue;
			}
			for (int j = 0; j < item.Data.Count; j++)
			{
				if (!this._setupColumnInfo[j].Sum)
				{
					continue;
				}
				if (item.Data[j] is string)
				{
					if (list[j] == null)
					{
						list[j] = 1f;
					}
					else
					{
						list[j] = (float)list[j] + 1f;
					}
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
		for (int k = 0; k < this._setupColumnInfo.Count; k++)
		{
			if (list[k] is double)
			{
				double num = (double)list[k];
				this.dataGrid.Columns[k].Footer = num.ToString(this._setupColumnInfo[k].RadColumn.DataFormatString);
			}
			else if (list[k] is DateTime)
			{
				DateTime dateTime = (DateTime)list[k];
				this.dataGrid.Columns[k].Footer = dateTime.ToString(this._setupColumnInfo[k].RadColumn.DataFormatString);
			}
			else if (list[k] is float)
			{
				this.dataGrid.Columns[k].Footer = (Application.Current.TryFindResource("l_popup.Count") as string) + list[k].ToString();
			}
			else
			{
				this.dataGrid.Columns[k].Footer = list[k];
			}
		}
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
			result = Math.Round((double)(object)this.PopupString2Double(s), qtc.Round);
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

	private void SetButtonsEnabel()
	{
		this.SetButtonsEnabel(this.dataGrid.SelectedItems.Count > 0 && this.dataGrid.FilterDescriptors.Count == 0 && this.dataGrid.GroupDescriptors.Count == 0);
	}

	private void SetButtonsEnabel(bool v)
	{
		this._SortButtonsEnableFunction?.Invoke(v);
	}

	private void dataGrid_SelectionChanged(object sender, SelectionChangeEventArgs e)
	{
		if (!this._is_EditButtonVisible)
		{
			foreach (object removedItem in e.RemovedItems)
			{
				if (!this.dataGrid.Items.Contains(removedItem))
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

	private void dataGrid_Grouped(object sender, GridViewGroupedEventArgs e)
	{
		this.SetButtonsEnabel();
	}

	private void dataGrid_Filtered(object sender, GridViewFilteredEventArgs e)
	{
		List<QuickTableRecord> list = new List<QuickTableRecord>();
		foreach (QuickTableRecord deselectedByFilteringItem in this.deselectedByFilteringItems)
		{
			if (!this.dataGrid.Items.Contains(deselectedByFilteringItem))
			{
				continue;
			}
			if (this._isSingleListMode)
			{
				RadGridView radGridView = this.dataGrid;
				if (radGridView.SelectedItem == null)
				{
					object obj = (radGridView.SelectedItem = deselectedByFilteringItem);
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
		this.SetButtonsEnabel();
	}

	private void dataGrid_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
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

	public void KeyboardFocus()
	{
		try
		{
			base.Focus();
			Keyboard.Focus(this);
			if (this.DataViewContainter != null)
			{
				this.DataViewContainter.Focus();
				Keyboard.Focus(this.DataViewContainter);
				if (this.DataViewContainter.GridView != null)
				{
					this.DataViewContainter.GridView.Focus();
					Keyboard.Focus(this.DataViewContainter.GridView);
				}
			}
		}
		catch
		{
		}
	}

	private void dataGrid_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton == MouseButtonState.Pressed && this._staticListMode == 0 && this.CheckTreeForListViewItem((DependencyObject)e.OriginalSource) && this.dataGrid.SelectedItems.Count != 0)
		{
			this._exitCode2?.Invoke();
		}
	}

	private bool CheckTreeForListViewItem(DependencyObject obj)
	{
		if (obj is GridViewCell)
		{
			return true;
		}
		DependencyObject parent = VisualTreeHelper.GetParent(obj);
		if (parent == null)
		{
			return false;
		}
		if (parent is RadGridView)
		{
			return false;
		}
		return this.CheckTreeForListViewItem(parent);
	}

	private void ToolTipHandler(object sender, ToolTipEventArgs e)
	{
		if (!(sender is GridViewRow))
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

	public void MoveSelectedOneDown()
	{
		List<QuickTableRecord> dataAtCurrentOrder = this.GetDataAtCurrentOrder();
		List<Tuple<int, QuickTableRecord>> sortedSelected = this.GetSortedSelected(dataAtCurrentOrder, sorttop: false);
		if (sortedSelected[0].Item1 == this.dataGrid.Items.Count - 1)
		{
			return;
		}
		foreach (Tuple<int, QuickTableRecord> item2 in sortedSelected)
		{
			int item = item2.Item1;
			dataAtCurrentOrder.RemoveAt(item);
			dataAtCurrentOrder.Insert(item + 1, item2.Item2);
		}
		this.UpdateWithCurrent(dataAtCurrentOrder, sortedSelected);
	}

	public void MoveSelectedOneUp()
	{
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

	public void MoveSelectedToEnd()
	{
		List<QuickTableRecord> dataAtCurrentOrder = this.GetDataAtCurrentOrder();
		List<Tuple<int, QuickTableRecord>> sortedSelected = this.GetSortedSelected(dataAtCurrentOrder, sorttop: false);
		int item = sortedSelected[0].Item1;
		if (item == this.dataGrid.Items.Count - 1)
		{
			return;
		}
		foreach (Tuple<int, QuickTableRecord> item3 in sortedSelected)
		{
			int item2 = item3.Item1;
			dataAtCurrentOrder.RemoveAt(item2);
			dataAtCurrentOrder.Insert(item2 + this.dataGrid.Items.Count - item - 1, item3.Item2);
		}
		this.UpdateWithCurrent(dataAtCurrentOrder, sortedSelected);
	}

	public void MoveSelectedToStart()
	{
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

	private List<QuickTableRecord> GetDataAtCurrentOrder()
	{
		List<QuickTableRecord> list = new List<QuickTableRecord>();
		list.AddRange(this.dataGrid.Items);
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
		this.dataGrid.SortDescriptors.Clear();
		this.TableRecords.Clear();
		this.TableRecords.AddRange(current);
		this.dataGrid.SelectedItems.Clear();
		List<QuickTableRecord> list = new List<QuickTableRecord>();
		foreach (Tuple<int, QuickTableRecord> item in selected)
		{
			list.Add(item.Item2);
		}
		this.dataGrid.SelectedItems.AddRange(list);
		this.ScrollToSelected();
	}

	private void ScrollToSelected()
	{
		if (this.dataGrid.SelectedItems.Count != 0)
		{
			ScrollViewer visualChild = QuickTable2.GetVisualChild<ScrollViewer>(this.dataGrid);
			if (visualChild != null)
			{
				int num = this.TableRecords.IndexOf((QuickTableRecord)this.dataGrid.SelectedItems[0]);
				int count = this.TableRecords.Count;
				visualChild.ScrollToVerticalOffset(visualChild.ScrollableHeight / (double)count * (double)num);
			}
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
				val = QuickTable2.GetVisualChild<T>(visual);
			}
			if (val != null)
			{
				break;
			}
		}
		return val;
	}
}
