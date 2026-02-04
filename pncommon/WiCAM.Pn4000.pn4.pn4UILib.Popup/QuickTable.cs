using System;
using System.CodeDom.Compiler;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using WiCAM.Pn4000.Contracts.LogCenterServices;
using WiCAM.Pn4000.PKernelFlow.Adapters;
using WiCAM.Pn4000.PKernelFlow.Adapters.Data;
using WiCAM.Pn4000.pn4.pn4Services;
using WiCAM.Pn4000.pn4.pn4Services.CADGEO;

namespace WiCAM.Pn4000.pn4.pn4UILib.Popup;

public partial class QuickTable : UserControl, IComponentConnector
{
	public delegate void ExitCode2();

	public class ColumnInfo
	{
		public Label SumBox;

		public int Type;

		public string Label;

		public bool Sum;

		public string Name;

		public GridViewColumn Gridviewcolumn;

		public ColumnInfo(string code, ILogCenterService logCenterService)
		{
			int num = code.IndexOf('[');
			int num2 = code.IndexOf(']');
			if (num < 0 || num2 < 0 || num2 - num < 2)
			{
				this.Type = 1;
				this.Sum = false;
				this.Label = code;
				return;
			}
			this.Sum = code[num + 1] == '+';
			this.Name = code.Substring(0, num);
			this.Label = this.Name;
			try
			{
				this.Type = Convert.ToInt32($"{code[num2 - 1]}");
			}
			catch (Exception e)
			{
				logCenterService.CatchRaport(e);
			}
		}

		internal void SetSumInfo(string p)
		{
			if (this.SumBox != null)
			{
				this.SumBox.Content = p;
				this.SumBox.Background = new SolidColorBrush(Colors.LightGray);
			}
		}
	}

	private ILanguageDictionary _languageDictionary;

	private IPnColorsService _pnColorsService;

	private ILogCenterService _logCenterService;

	public Window MainPopupWindow;

	private GridViewColumnHeader _lastHeaderClicked;

	private ListSortDirection _lastDirection;

	private PnImageSource _imageSource;

	private PopupKernelDataInterface _popupInterface;

	private bool _isSingleListMode;

	private int _staticListMode;

	public List<PopupLine> PublicLines = new List<PopupLine>();

	public int PublicListMode;

	private ComboBox _filterCombo;

	private bool _updateFilters;

	private bool _hookAdd;

	private object _topObject;

	private List<object> _itemsNotSelectable;

	private Action<bool> SortButtonsEnableFunction;

	public string FilterString = string.Empty;

	private List<object> _remebemerFilterSelected = new List<object>();

	private Dictionary<ToolTip, string> _tooltipHtext;

	private List<ColumnInfo> _ciL;

	private ObservableCollection<List<object>> _inputDb;

	private ExitCode2 _onExitCode2;

	public QuickTable(ILanguageDictionary languageDictionary, IPnColorsService pnColorsService, ILogCenterService logCenterService)
	{
		this._languageDictionary = languageDictionary;
		this._pnColorsService = pnColorsService;
		this._logCenterService = logCenterService;
		this.InitializeComponent();
		EventManager.RegisterClassHandler(typeof(FrameworkElement), FrameworkElement.ToolTipOpeningEvent, new ToolTipEventHandler(ToolTipHandler));
	}

	public void SetServiceProvider()
	{
	}

	public void SetInterface(PopupKernelDataInterface popupInterface)
	{
		this._popupInterface = popupInterface;
	}

	private void Sort(string sortBy, ListSortDirection direction)
	{
		ICollectionView defaultView = CollectionViewSource.GetDefaultView(((CollectionContainer)((CompositeCollection)this.list1.ItemsSource)[0]).Collection);
		SortDescription item = new SortDescription(sortBy, direction);
		this.RemoveSameSortBy(defaultView.SortDescriptions, sortBy);
		defaultView.SortDescriptions.Insert(0, item);
		if (defaultView.SortDescriptions.Count == 5)
		{
			defaultView.SortDescriptions.RemoveAt(4);
		}
		defaultView.Refresh();
	}

	private void RemoveSameSortBy(SortDescriptionCollection sortDescriptionCollection, string sortBy)
	{
		foreach (SortDescription item in sortDescriptionCollection)
		{
			if (item.PropertyName == sortBy)
			{
				sortDescriptionCollection.Remove(item);
				break;
			}
		}
	}

	private void list1_Click(object sender, RoutedEventArgs e)
	{
		if (e.OriginalSource is GridViewColumnHeader { Role: not GridViewColumnHeaderRole.Padding } gridViewColumnHeader)
		{
			ListSortDirection listSortDirection = ((gridViewColumnHeader == this._lastHeaderClicked && this._lastDirection == ListSortDirection.Ascending) ? ListSortDirection.Descending : ListSortDirection.Ascending);
			this._lastHeaderClicked = gridViewColumnHeader;
			this._lastDirection = listSortDirection;
			GridView obj = (GridView)this.list1.View;
			_ = gridViewColumnHeader.Column.Header;
			int num = obj.Columns.IndexOf(gridViewColumnHeader.Column);
			if (num >= 0)
			{
				this.Sort($"[{num}]", listSortDirection);
			}
		}
	}

	public void Setup(List<PopupLine> lines, PnImageSource imageSource, ExitCode2 onExitCode2, int staticListMode, ComboBox filterCombo, bool updateFilters, Action<bool> SortButtonsEnableFunction)
	{
		this.SortButtonsEnableFunction = SortButtonsEnableFunction;
		this.SetButtonsEnabel(v: false);
		this._remebemerFilterSelected.Clear();
		this._updateFilters = updateFilters;
		bool flag = false;
		this._filterCombo = filterCombo;
		foreach (PopupLine line in lines)
		{
			line.text = line.text.Replace("\\v", "\v");
		}
		this._staticListMode = staticListMode;
		if (this.list1.SelectionMode != 0)
		{
			this.list1.SelectedItems.Clear();
		}
		else
		{
			this.list1.SelectedItem = null;
		}
		this.list1.ItemsSource = null;
		if (this._itemsNotSelectable == null)
		{
			this._itemsNotSelectable = new List<object>();
		}
		else
		{
			this._itemsNotSelectable.Clear();
		}
		this._imageSource = imageSource;
		this._onExitCode2 = onExitCode2;
		this._tooltipHtext = new Dictionary<ToolTip, string>();
		GridView gridView = (GridView)this.list1.View;
		gridView.Columns.Clear();
		this._ciL = new List<ColumnInfo>();
		string[] array = lines[0].text.Split('\v');
		int num = 0;
		TextBlock textBlock = new TextBlock
		{
			Margin = new Thickness(6.0, 4.0, 6.0, 4.0),
			FontWeight = FontWeights.Bold
		};
		int[] array2 = new int[array.Length];
		string[] array3 = array;
		for (int i = 0; i < array3.Length; i++)
		{
			ColumnInfo columnInfo = new ColumnInfo(array3[i], this._logCenterService);
			columnInfo.Gridviewcolumn = new GridViewColumn
			{
				Header = columnInfo.Label,
				CellTemplate = this.GetGridColumnTemplate(num, columnInfo.Type),
				Width = 50.0
			};
			if (filterCombo != null && updateFilters)
			{
				filterCombo.Items.Add(columnInfo.Label);
			}
			Style style = new Style(typeof(GridViewColumnHeader));
			style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left));
			switch (columnInfo.Type)
			{
			case 1:
				style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Left));
				break;
			case 2:
				style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Right));
				break;
			case 3:
				style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Right));
				break;
			case 4:
				style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Right));
				break;
			case 5:
				style.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Right));
				break;
			}
			if (staticListMode > 1)
			{
				style.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Collapsed));
			}
			columnInfo.Gridviewcolumn.HeaderContainerStyle = style;
			((INotifyPropertyChanged)columnInfo.Gridviewcolumn).PropertyChanged += gridcolumn_PropertyChanged;
			this._ciL.Add(columnInfo);
			gridView.Columns.Add(columnInfo.Gridviewcolumn);
			textBlock.Text = (string)columnInfo.Gridviewcolumn.Header;
			textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
			if ((int)textBlock.DesiredSize.Width + 2 > array2[num])
			{
				array2[num] = (int)textBlock.DesiredSize.Width + 2;
			}
			num++;
		}
		this._inputDb = new ObservableCollection<List<object>>();
		int num2 = -1;
		for (int j = 1; j < lines.Count; j++)
		{
			if (j == 1)
			{
				this._isSingleListMode = lines[j].typ == 6;
				if (this._isSingleListMode)
				{
					this.list1.SelectionMode = SelectionMode.Single;
					this.list1.SetValue(ListBoxSelector.EnabledProperty, false);
				}
				else
				{
					this.list1.SelectionMode = SelectionMode.Extended;
					this.list1.SelectedItems.Clear();
					this.list1.SetValue(ListBoxSelector.EnabledProperty, false);
					this.list1.SetValue(ListBoxSelector.EnabledProperty, true);
				}
			}
			string[] array4 = lines[j].text.Split('\v');
			int num3 = 0;
			List<object> list = new List<object>();
			array3 = array4;
			foreach (string text in array3)
			{
				try
				{
					if (num3 < this._ciL.Count)
					{
						switch (this._ciL[num3].Type)
						{
						case 1:
							list.Add(text);
							break;
						case 2:
							if (text == string.Empty)
							{
								list.Add(null);
								break;
							}
							try
							{
								list.Add(Convert.ToInt32(text));
							}
							catch
							{
								list.Add(0);
							}
							break;
						case 3:
							if (text == string.Empty)
							{
								list.Add(null);
								break;
							}
							try
							{
								list.Add(Convert.ToDouble(text, CultureInfo.InvariantCulture));
							}
							catch
							{
								list.Add(0.0);
							}
							break;
						case 4:
							list.Add(this.Datafromstring(text));
							break;
						case 5:
							list.Add(this.Timefromstring(text));
							break;
						}
						if (j < 100)
						{
							textBlock.Text = text;
							textBlock.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
							if ((int)textBlock.DesiredSize.Width + 2 > array2[num3])
							{
								array2[num3] = (int)textBlock.DesiredSize.Width + 2;
							}
						}
					}
				}
				catch (Exception e)
				{
					this._logCenterService.CatchRaport(e);
				}
				num3++;
			}
			num2 = num3;
			if (lines[j].htext != string.Empty)
			{
				ToolTip toolTip = new ToolTip();
				this._tooltipHtext.Add(toolTip, lines[j].htext);
				list.Add(toolTip);
			}
			else if (lines[j].id3 > 0)
			{
				ToolTip toolTip2 = new ToolTip();
				string pophlp = this._languageDictionary.GetPophlp(lines[j].id3);
				if (pophlp != null)
				{
					this._tooltipHtext.Add(toolTip2, pophlp);
					list.Add(toolTip2);
				}
				else
				{
					list.Add(null);
				}
			}
			else
			{
				list.Add(null);
			}
			if (lines[j].sel == -1)
			{
				if (lines[j].real4 != 0.0)
				{
					list.Add(this._pnColorsService.GetWpfBrush((int)lines[j].real4));
					list.Add(this._pnColorsService.GetWpfBrush((int)lines[j].real4));
				}
				else
				{
					list.Add(new SolidColorBrush(Colors.LightGray));
					list.Add(new SolidColorBrush(Colors.LightGray));
				}
				list.Add(FontWeights.Thin);
				list.Add(FontWeights.Thin);
				this._itemsNotSelectable.Add(list);
			}
			else
			{
				if (lines[j].real4 != 0.0)
				{
					list.Add(this._pnColorsService.GetWpfBrush((int)lines[j].real4));
				}
				else
				{
					list.Add(null);
				}
				list.Add(new SolidColorBrush(Colors.LightBlue));
				list.Add(FontWeights.Normal);
				list.Add(FontWeights.Bold);
			}
			if (lines[j].sel == -1)
			{
				list.Add(new SolidColorBrush(Colors.Gray));
				list.Add(false);
			}
			else
			{
				list.Add(SystemColors.WindowTextBrush);
				list.Add(true);
			}
			list.Add(Visibility.Visible);
			list.Add(lines[j].inte1);
			list.Add(lines[j].id1);
			if (lines[j].sel == 1)
			{
				if (this._isSingleListMode)
				{
					this.list1.SelectedItem = list;
				}
				else
				{
					this.list1.SelectedItems.Add(list);
				}
				if (!flag)
				{
					flag = true;
					this._topObject = list;
				}
			}
			this._inputDb.Add(list);
		}
		Style style2 = new Style(typeof(ListViewItem));
		style2.Setters.Add(new Setter(ToolTipService.ShowOnDisabledProperty, true));
		style2.Setters.Add(new Setter(FrameworkElement.ToolTipProperty, new Binding($"[{num2}]")));
		style2.Setters.Add(new Setter(Control.HorizontalContentAlignmentProperty, HorizontalAlignment.Stretch));
		style2.Setters.Add(new Setter(Control.BackgroundProperty, new Binding($"[{num2 + 1}]")));
		style2.Setters.Add(new Setter(Control.FontWeightProperty, new Binding($"[{num2 + 3}]")));
		style2.Setters.Add(new Setter(Control.ForegroundProperty, new Binding($"[{num2 + 5}]")));
		style2.Setters.Add(new Setter(UIElement.IsEnabledProperty, new Binding($"[{num2 + 6}]")));
		style2.Setters.Add(new Setter(UIElement.VisibilityProperty, new Binding($"[{num2 + 7}]")));
		MultiTrigger multiTrigger = new MultiTrigger();
		multiTrigger.Conditions.Add(new Condition(ListBoxItem.IsSelectedProperty, true));
		multiTrigger.Setters.Add(new Setter(Control.BackgroundProperty, new Binding($"[{num2 + 2}]")));
		multiTrigger.Setters.Add(new Setter(Control.FontWeightProperty, new Binding($"[{num2 + 4}]")));
		style2.Triggers.Add(multiTrigger);
		if (staticListMode > 0)
		{
			style2.Setters.Add(new Setter(UIElement.FocusableProperty, false));
			multiTrigger = new MultiTrigger();
			multiTrigger.Conditions.Add(new Condition(UIElement.IsMouseOverProperty, true));
			multiTrigger.Setters.Add(new Setter(Control.BackgroundProperty, null));
			multiTrigger.Setters.Add(new Setter(Control.BorderBrushProperty, null));
			style2.Triggers.Add(multiTrigger);
		}
		this.list1.ItemContainerStyle = style2;
		Style style3 = new Style(typeof(GridViewColumnHeader));
		if (staticListMode > 1)
		{
			style3.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Collapsed));
		}
		else
		{
			style3.Setters.Add(new Setter(UIElement.VisibilityProperty, Visibility.Visible));
		}
		gridView.ColumnHeaderContainerStyle = style3;
		CompositeCollection compositeCollection = new CompositeCollection();
		CollectionContainer collectionContainer = new CollectionContainer();
		collectionContainer.Collection = this._inputDb;
		compositeCollection.Add(collectionContainer);
		this.list1.ItemsSource = compositeCollection;
		for (int k = 0; k < gridView.Columns.Count; k++)
		{
			gridView.Columns[k].Width = array2[k];
		}
		this.CalculateColumnsSum();
		CollectionViewSource.GetDefaultView(((CollectionContainer)((CompositeCollection)this.list1.ItemsSource)[0]).Collection).Filter = MyFilter;
	}

	public bool MyFilter(object item)
	{
		return this.MyFilter(item, this.FilterString);
	}

	private bool MyFilter(object item, string filter)
	{
		if (filter == null)
		{
			return true;
		}
		if (filter == string.Empty)
		{
			return true;
		}
		List<object> list = (List<object>)item;
		string[] array = filter.Split(' ');
		bool[] array2 = new bool[array.GetLength(0)];
		for (int i = 0; i < array.GetLength(0); i++)
		{
			array2[i] = false;
		}
		int num = 0;
		if (this._filterCombo != null)
		{
			num = this._filterCombo.SelectedIndex;
		}
		for (int j = 0; j < list.Count - 6; j++)
		{
			if ((num != 0 && j != num - 1) || list[j] == null)
			{
				continue;
			}
			string text = list[j].ToString();
			if (list[j] is double)
			{
				text = ((this._languageDictionary.GetInchMode() != 1) ? ((double)list[j]).ToString("0.00", CultureInfo.InvariantCulture) : ((double)list[j]).ToString("0.0000", CultureInfo.InvariantCulture));
			}
			if (list[j] is DateTime)
			{
				text = ((DateTime)list[j]).ToString("dd.MM.yyyy");
			}
			for (int k = 0; k < array.GetLength(0); k++)
			{
				string text2 = array[k].Trim(' ');
				if (text2 != string.Empty && text.ToLower().Contains(text2.ToLower()))
				{
					array2[k] = true;
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

	public void SetFilter(string p)
	{
		CompositeCollection compositeCollection = (CompositeCollection)this.list1.ItemsSource;
		if (compositeCollection == null)
		{
			return;
		}
		ICollectionView defaultView = CollectionViewSource.GetDefaultView(((CollectionContainer)compositeCollection[0]).Collection);
		if (this._isSingleListMode)
		{
			this.FilterString = p;
			defaultView.Refresh();
			this.CalculateColumnsSum();
			return;
		}
		List<object> list = new List<object>();
		foreach (object item in this._remebemerFilterSelected)
		{
			if (this.list1.Items.Contains(item) && !this.list1.SelectedItems.Contains(item))
			{
				list.Add(item);
			}
		}
		foreach (object item2 in list)
		{
			this._remebemerFilterSelected.Remove(item2);
		}
		foreach (object selectedItem in this.list1.SelectedItems)
		{
			if (!this._remebemerFilterSelected.Contains(selectedItem))
			{
				this._remebemerFilterSelected.Add(selectedItem);
			}
		}
		this.FilterString = p;
		defaultView.Refresh();
		List<object> list2 = new List<object>();
		foreach (object item3 in this._remebemerFilterSelected)
		{
			if (this.list1.Items.Contains(item3))
			{
				list2.Add(item3);
			}
		}
		this.list1.SelectItems(list2);
		this.CalculateColumnsSum();
	}

	private bool IsAnySum()
	{
		foreach (ColumnInfo item in this._ciL)
		{
			if (item.Sum)
			{
				return true;
			}
		}
		return false;
	}

	private void CalculateColumnsSum()
	{
		CompositeCollection compositeCollection = (CompositeCollection)this.list1.ItemsSource;
		if (compositeCollection.Count == 0)
		{
			return;
		}
		bool flag = this.IsAnySum();
		List<object> list = new List<object>();
		for (int i = 0; i < this._ciL.Count; i++)
		{
			list.Add(null);
		}
		bool flag2 = this.list1.SelectedItems.Count == 0;
		bool flag3 = false;
		if (compositeCollection.Count == 2)
		{
			_ = compositeCollection[1];
			_ = this._isSingleListMode;
			flag3 = true;
		}
		int num = 0;
		foreach (object item in (IEnumerable)this.list1.Items)
		{
			bool flag4 = flag3 && num == this.list1.Items.Count - 1;
			num++;
			if (!this.MyFilter(item) || (!flag2 && !this.list1.SelectedItems.Contains(item)) || flag4)
			{
				continue;
			}
			List<object> list2 = (List<object>)item;
			int num2 = 0;
			foreach (ColumnInfo item2 in this._ciL)
			{
				if (item2.Sum)
				{
					if (list2[num2] is TimeSpan)
					{
						if (list[num2] == null)
						{
							list[num2] = (TimeSpan)list2[num2];
						}
						else
						{
							list[num2] = (TimeSpan)list[num2] + (TimeSpan)list2[num2];
						}
					}
					if (list2[num2] is double)
					{
						if (list[num2] == null)
						{
							list[num2] = (double)list2[num2];
						}
						else
						{
							list[num2] = (double)list[num2] + (double)list2[num2];
						}
					}
					if (list2[num2] is int)
					{
						if (list[num2] == null)
						{
							list[num2] = (int)list2[num2];
						}
						else
						{
							list[num2] = (int)list[num2] + (int)list2[num2];
						}
					}
				}
				num2++;
			}
		}
		list.Add(null);
		list.Add(new SolidColorBrush(Colors.LightGray));
		list.Add(new SolidColorBrush(Colors.LightGray));
		list.Add(FontWeights.Bold);
		list.Add(FontWeights.Bold);
		if (flag)
		{
			if (compositeCollection.Count == 1)
			{
				compositeCollection.Add(list);
			}
			else
			{
				compositeCollection[1] = list;
			}
		}
	}

	private DataTemplate GetGridColumnTemplate(int count, int type)
	{
		DataTemplate dataTemplate = new DataTemplate();
		FrameworkElementFactory frameworkElementFactory = new FrameworkElementFactory(typeof(TextBlock));
		Binding binding = new Binding($"[{count}]");
		switch (type)
		{
		case 1:
			frameworkElementFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Left);
			break;
		case 2:
			frameworkElementFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
			break;
		case 3:
			frameworkElementFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
			if (this._languageDictionary.GetInchMode() == 1)
			{
				binding.StringFormat = "0.0000";
			}
			else
			{
				binding.StringFormat = "0.00";
			}
			break;
		case 4:
			frameworkElementFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Left);
			binding.StringFormat = "dd.MM.yyyy";
			break;
		case 5:
			frameworkElementFactory.SetValue(TextBlock.TextAlignmentProperty, TextAlignment.Right);
			break;
		}
		frameworkElementFactory.SetBinding(TextBlock.TextProperty, binding);
		dataTemplate.VisualTree = frameworkElementFactory;
		return dataTemplate;
	}

	private void gridcolumn_PropertyChanged(object sender, PropertyChangedEventArgs e)
	{
		if (!(e.PropertyName == "ActualWidth"))
		{
			return;
		}
		foreach (ColumnInfo item in this._ciL)
		{
			if (item.Gridviewcolumn == sender)
			{
				if (item.SumBox != null)
				{
					item.SumBox.Width = item.Gridviewcolumn.ActualWidth;
				}
				break;
			}
		}
	}

	private TimeSpan Timefromstring(string str)
	{
		try
		{
			int num = (int)(Convert.ToDouble(str, CultureInfo.InvariantCulture) * 60.0);
			int num2 = num / 3600;
			int num3 = num - num2 * 3600;
			int num4 = num3 / 60;
			return new TimeSpan(num2, num4, num3 - num4 * 60);
		}
		catch (Exception e)
		{
			this._logCenterService.CatchRaport(e);
		}
		return default(TimeSpan);
	}

	private DateTime Datafromstring(string str)
	{
		int num = str.IndexOf('.');
		int num2 = str.IndexOf('.', num + 1);
		if (num > 0 && num2 > num)
		{
			try
			{
				return new DateTime(Convert.ToInt32(str.Substring(num2 + 1)), Convert.ToInt32(str.Substring(num + 1, num2 - num - 1)), Convert.ToInt32(str.Substring(0, num)));
			}
			catch (Exception e)
			{
				this._logCenterService.CatchRaport(e);
			}
		}
		return default(DateTime);
	}

	private void UserControl_SizeChanged(object sender, SizeChangedEventArgs e)
	{
	}

	private void list1_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (this._itemsNotSelectable != null)
		{
			foreach (object item in this._itemsNotSelectable)
			{
				if (this.list1.SelectedItems.Contains(item))
				{
					this.list1.SelectedItems.Remove(item);
				}
			}
		}
		this.CalculateColumnsSum();
		this.SetButtonsEnabel(this.list1.SelectedItem != null || this.list1.SelectedItems.Count > 0);
	}

	private void SetButtonsEnabel(bool v)
	{
		this.SortButtonsEnableFunction?.Invoke(v);
	}

	private void ToolTipHandler(object sender, ToolTipEventArgs e)
	{
		if (this._tooltipHtext == null || !(e.Source is FrameworkElement frameworkElement) || !(frameworkElement.ToolTip is ToolTip))
		{
			return;
		}
		ToolTip toolTip = (ToolTip)frameworkElement.ToolTip;
		if (!this._tooltipHtext.ContainsKey(toolTip))
		{
			return;
		}
		if (this._tooltipHtext[toolTip][0] == '#')
		{
			if (this._tooltipHtext[toolTip].Length >= 2)
			{
				toolTip.Content = this._tooltipHtext[toolTip].Substring(1);
			}
			this._tooltipHtext.Remove(toolTip);
		}
		else if (File.Exists(this._tooltipHtext[toolTip]))
		{
			Image image = new Image();
			image.SnapsToDevicePixels = true;
			image.BeginInit();
			image.Source = this._imageSource.GetImageSource(this._tooltipHtext[toolTip], 200, 200, 0);
			image.EndInit();
			toolTip.Content = image;
		}
		else
		{
			toolTip.Content = this._tooltipHtext[toolTip];
		}
	}

	private void list1_MouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (e.LeftButton != MouseButtonState.Pressed || this._staticListMode != 0 || !this.CheckTreeForListViewItem((DependencyObject)e.OriginalSource))
		{
			return;
		}
		CompositeCollection compositeCollection = (CompositeCollection)this.list1.ItemsSource;
		if (compositeCollection.Count == 2)
		{
			object obj = compositeCollection[1];
			if (this._isSingleListMode)
			{
				if (obj != null && this.list1.SelectedItem == obj)
				{
					return;
				}
			}
			else if (this.list1.SelectedItems.Count == 0 || this.list1.SelectedItems.Contains(obj))
			{
				return;
			}
		}
		if (this._onExitCode2 != null)
		{
			this._onExitCode2();
		}
	}

	private bool CheckTreeForListViewItem(DependencyObject obj)
	{
		if (obj is ListViewItem)
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

	public void SendAnswer(string filter)
	{
		this.SendAnswer(0, filter);
	}

	public void SendAnswer(int startLn, string filter)
	{
		int num = 1;
		foreach (object item in (IEnumerable)this.list1.Items)
		{
			if (num <= this._inputDb.Count)
			{
				List<object> list = (List<object>)item;
				int idx = num + startLn;
				int value = 2;
				if (this.MyFilter(list, filter))
				{
					value = ((this.list1.SelectedItems.Contains(item) || this._remebemerFilterSelected.Contains(item)) ? 1 : 0);
				}
				PopupAdapter.Popup_Line_IPOSEL_set(idx, value);
				PopupAdapter.Popup_Line_IPOPZ_set(idx, (int)list[list.Count - 2]);
				PopupAdapter.Popup_Line_IPOPID_set(idx, (int)list[list.Count - 1]);
				StringBuilder stringBuilder = new StringBuilder(600);
				for (int i = 0; i < list.Count - 8; i++)
				{
					if (i > 0 && i < list.Count - 9)
					{
						stringBuilder.Append('\v');
					}
					if (list[i] is double)
					{
						if (this._languageDictionary.GetInchMode() == 1)
						{
							stringBuilder.Append($"{list[i]:0.0000}".Replace(',', '.'));
						}
						else
						{
							stringBuilder.Append($"{list[i]:0.00}".Replace(',', '.'));
						}
					}
					else if (list[i] is DateTime)
					{
						stringBuilder.AppendFormat("{0:dd.MM.yyyy}", list[i]);
					}
					else if (list[i] != null && !(list[i] is ToolTip))
					{
						stringBuilder.Append(list[i].ToString());
					}
				}
				PopupAdapter.Popup_Line_POPBEM_set(idx, stringBuilder.ToString());
			}
			num++;
		}
	}

	internal void PageDown()
	{
		((ScrollViewer)QuickTable.GetScrollViewer(this.list1)).PageDown();
	}

	internal void PageUp()
	{
		((ScrollViewer)QuickTable.GetScrollViewer(this.list1)).PageUp();
	}

	public static DependencyObject GetScrollViewer(DependencyObject o)
	{
		if (o is ScrollViewer)
		{
			return o;
		}
		for (int i = 0; i < VisualTreeHelper.GetChildrenCount(o); i++)
		{
			DependencyObject scrollViewer = QuickTable.GetScrollViewer(VisualTreeHelper.GetChild(o, i));
			if (scrollViewer != null)
			{
				return scrollViewer;
			}
		}
		return null;
	}

	public void ScrollReset()
	{
		ScrollViewer scrollViewer = (ScrollViewer)QuickTable.GetScrollViewer(this.list1);
		if (scrollViewer != null)
		{
			scrollViewer.ScrollToVerticalOffset(0.0);
			scrollViewer.ScrollToHorizontalOffset(0.0);
		}
	}

	private void UserControl_LayoutUpdated(object sender, EventArgs e)
	{
	}

	private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		this.ScrollReset();
	}

	private void list1_MouseDown(object sender, MouseButtonEventArgs e)
	{
	}

	private void list1_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		if (!this.CheckTreeForListViewItem((DependencyObject)e.OriginalSource))
		{
			if (this._isSingleListMode)
			{
				this.list1.SelectedItem = null;
			}
			else
			{
				this.list1.SelectedItems.Clear();
			}
		}
	}

	private void list1_Loaded(object sender, RoutedEventArgs e)
	{
	}

	private void list1_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key != Key.C || Keyboard.Modifiers != ModifierKeys.Control)
		{
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		if (this.list1.SelectionMode != 0)
		{
			foreach (List<object> selectedItem in this.list1.SelectedItems)
			{
				if (selectedItem.Count > 0)
				{
					stringBuilder.AppendLine(selectedItem[0].ToString());
				}
			}
		}
		else
		{
			List<object> list2 = this.list1.SelectedItem as List<object>;
			if (list2.Count > 0)
			{
				stringBuilder.AppendLine(list2[0].ToString());
			}
		}
		Clipboard.Clear();
		Clipboard.SetText(stringBuilder.ToString());
	}

	private void list1_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		if (this._topObject != null)
		{
			this.list1.ScrollIntoView(this._topObject);
			this._topObject = null;
		}
	}

	internal void MoveSelectedToStart()
	{
		if (this.list1.SelectionMode == SelectionMode.Single)
		{
			this.SingleMoveSelectToStart();
		}
		else
		{
			if (this.list1.SelectedItems.Count == 0)
			{
				return;
			}
			ObservableCollection<List<object>> dataAtCurrentOrder = this.GetDataAtCurrentOrder();
			List<Tuple<int, object>> sortedSelected = this.GetSortedSelected(dataAtCurrentOrder, sorttop: true);
			int item = sortedSelected[0].Item1;
			if (item == 0)
			{
				return;
			}
			foreach (Tuple<int, object> item3 in sortedSelected)
			{
				int item2 = item3.Item1;
				dataAtCurrentOrder.RemoveAt(item2);
				dataAtCurrentOrder.Insert(item2 - item, item3.Item2 as List<object>);
			}
			this.UpdateWithCurrent(dataAtCurrentOrder, sortedSelected);
		}
	}

	private void SingleMoveSelectToStart()
	{
		if (this.list1.SelectedItem != null)
		{
			ObservableCollection<List<object>> dataAtCurrentOrder = this.GetDataAtCurrentOrder();
			object selectedItem = this.list1.SelectedItem;
			int num = dataAtCurrentOrder.IndexOf(this.list1.SelectedItem as List<object>);
			if (num != 0)
			{
				dataAtCurrentOrder.RemoveAt(num);
				dataAtCurrentOrder.Insert(0, selectedItem as List<object>);
				this.UpdateWithCurrentForSingleSelection(dataAtCurrentOrder, selectedItem);
			}
		}
	}

	private ObservableCollection<List<object>> GetDataAtCurrentOrder()
	{
		CollectionViewSource.GetDefaultView(((CollectionContainer)((CompositeCollection)this.list1.ItemsSource)[0]).Collection);
		ObservableCollection<List<object>> observableCollection = new ObservableCollection<List<object>>();
		for (int i = 0; i < this._inputDb.Count; i++)
		{
			observableCollection.Add(null);
		}
		foreach (List<object> item in this._inputDb)
		{
			observableCollection[this.list1.Items.IndexOf(item)] = item as List<object>;
		}
		return observableCollection;
	}

	private List<Tuple<int, object>> GetSortedSelected(ObservableCollection<List<object>> current, bool sorttop)
	{
		List<Tuple<int, object>> list = new List<Tuple<int, object>>();
		foreach (List<object> selectedItem in this.list1.SelectedItems)
		{
			list.Add(new Tuple<int, object>(current.IndexOf(selectedItem), selectedItem));
		}
		if (sorttop)
		{
			list.Sort(delegate(Tuple<int, object> x, Tuple<int, object> y)
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
			list.Sort(delegate(Tuple<int, object> x, Tuple<int, object> y)
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

	private void UpdateWithCurrent(ObservableCollection<List<object>> current, List<Tuple<int, object>> selected)
	{
		CompositeCollection compositeCollection = (CompositeCollection)this.list1.ItemsSource;
		CollectionContainer collectionContainer = new CollectionContainer();
		collectionContainer.Collection = (this._inputDb = current);
		compositeCollection.Clear();
		compositeCollection.Add(collectionContainer);
		this.list1.ItemsSource = compositeCollection;
		foreach (Tuple<int, object> item in selected)
		{
			this.list1.SelectedItems.Add(item.Item2 as List<object>);
		}
		this.list1.ScrollIntoView(selected[0].Item2);
	}

	private void UpdateWithCurrentForSingleSelection(ObservableCollection<List<object>> current, object selected)
	{
		CompositeCollection compositeCollection = (CompositeCollection)this.list1.ItemsSource;
		CollectionContainer collectionContainer = new CollectionContainer();
		collectionContainer.Collection = (this._inputDb = current);
		compositeCollection.Clear();
		compositeCollection.Add(collectionContainer);
		this.list1.ItemsSource = compositeCollection;
		this.list1.SelectedItem = selected;
		this.list1.ScrollIntoView(selected);
	}

	internal void MoveSelectedOneUp()
	{
		if (this.list1.SelectionMode == SelectionMode.Single)
		{
			this.SingleMoveSelectOneUp();
		}
		else
		{
			if (this.list1.SelectedItems.Count == 0)
			{
				return;
			}
			ObservableCollection<List<object>> dataAtCurrentOrder = this.GetDataAtCurrentOrder();
			List<Tuple<int, object>> sortedSelected = this.GetSortedSelected(dataAtCurrentOrder, sorttop: true);
			if (sortedSelected[0].Item1 == 0)
			{
				return;
			}
			foreach (Tuple<int, object> item2 in sortedSelected)
			{
				int item = item2.Item1;
				dataAtCurrentOrder.RemoveAt(item);
				dataAtCurrentOrder.Insert(item - 1, item2.Item2 as List<object>);
			}
			this.UpdateWithCurrent(dataAtCurrentOrder, sortedSelected);
		}
	}

	private void SingleMoveSelectOneUp()
	{
		if (this.list1.SelectedItem != null)
		{
			ObservableCollection<List<object>> dataAtCurrentOrder = this.GetDataAtCurrentOrder();
			object selectedItem = this.list1.SelectedItem;
			int num = dataAtCurrentOrder.IndexOf(selectedItem as List<object>);
			if (num != 0)
			{
				dataAtCurrentOrder.RemoveAt(num);
				dataAtCurrentOrder.Insert(num - 1, selectedItem as List<object>);
				this.UpdateWithCurrentForSingleSelection(dataAtCurrentOrder, selectedItem);
			}
		}
	}

	internal void MoveSelectedOneDown()
	{
		if (this.list1.SelectionMode == SelectionMode.Single)
		{
			this.SingleMoveSelectOneDown();
		}
		else
		{
			if (this.list1.SelectedItems.Count == 0)
			{
				return;
			}
			ObservableCollection<List<object>> dataAtCurrentOrder = this.GetDataAtCurrentOrder();
			List<Tuple<int, object>> sortedSelected = this.GetSortedSelected(dataAtCurrentOrder, sorttop: false);
			int item = sortedSelected[0].Item1;
			int num = 0;
			if (this.IsAnySum())
			{
				num = 1;
			}
			if (item == this.list1.Items.Count - 1 - num)
			{
				return;
			}
			foreach (Tuple<int, object> item3 in sortedSelected)
			{
				int item2 = item3.Item1;
				dataAtCurrentOrder.RemoveAt(item2);
				dataAtCurrentOrder.Insert(item2 + 1, item3.Item2 as List<object>);
			}
			this.UpdateWithCurrent(dataAtCurrentOrder, sortedSelected);
		}
	}

	private void SingleMoveSelectOneDown()
	{
		if (this.list1.SelectedItem != null)
		{
			object selectedItem = this.list1.SelectedItem;
			ObservableCollection<List<object>> dataAtCurrentOrder = this.GetDataAtCurrentOrder();
			int num = dataAtCurrentOrder.IndexOf(this.list1.SelectedItem as List<object>);
			int num2 = 0;
			if (this.IsAnySum())
			{
				num2 = 1;
			}
			if (num != this.list1.Items.Count - 1 - num2)
			{
				dataAtCurrentOrder.RemoveAt(num);
				dataAtCurrentOrder.Insert(num + 1, selectedItem as List<object>);
				this.UpdateWithCurrentForSingleSelection(dataAtCurrentOrder, selectedItem);
			}
		}
	}

	internal void MoveSelectedToEnd()
	{
		if (this.list1.SelectionMode == SelectionMode.Single)
		{
			this.SingleMoveSelectToEnd();
		}
		else
		{
			if (this.list1.SelectedItems.Count == 0)
			{
				return;
			}
			ObservableCollection<List<object>> dataAtCurrentOrder = this.GetDataAtCurrentOrder();
			List<Tuple<int, object>> sortedSelected = this.GetSortedSelected(dataAtCurrentOrder, sorttop: false);
			int item = sortedSelected[0].Item1;
			int num = 0;
			if (this.IsAnySum())
			{
				num = 1;
			}
			if (item == this.list1.Items.Count - 1 - num)
			{
				return;
			}
			foreach (Tuple<int, object> item3 in sortedSelected)
			{
				int item2 = item3.Item1;
				dataAtCurrentOrder.RemoveAt(item2);
				dataAtCurrentOrder.Insert(item2 + this.list1.Items.Count - num - item - 1, item3.Item2 as List<object>);
			}
			this.UpdateWithCurrent(dataAtCurrentOrder, sortedSelected);
		}
	}

	private void SingleMoveSelectToEnd()
	{
		if (this.list1.SelectedItem != null)
		{
			ObservableCollection<List<object>> dataAtCurrentOrder = this.GetDataAtCurrentOrder();
			object selectedItem = this.list1.SelectedItem;
			int num = dataAtCurrentOrder.IndexOf(this.list1.SelectedItem as List<object>);
			int num2 = 0;
			if (this.IsAnySum())
			{
				num2 = 1;
			}
			if (num != this.list1.Items.Count - 1 - num2)
			{
				dataAtCurrentOrder.RemoveAt(num);
				dataAtCurrentOrder.Insert(this.list1.Items.Count - num2 - 1, selectedItem as List<object>);
				this.UpdateWithCurrentForSingleSelection(dataAtCurrentOrder, selectedItem);
			}
		}
	}

	
}
