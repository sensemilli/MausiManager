using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using WiCAM.Pn4000.Common;

namespace WiCAM.Pn4000.JobManager;

public class DataGridManager<T> where T : class, INotifyPropertyChanged, new()
{
	private readonly DataGrid _grid;

	private readonly List<CppConfigurationLineInfo> _configuration;

	private readonly Action<List<T>> _selectionChangedHandler;

	private readonly Action<T> _htmlViewHandler;

	private readonly List<PropertyInfo> _itemProperties;

	private readonly CustomAttributeHelper<T, TranslationKeyAttribute> _attributeHelper;

	private MetricToInchConverter _converter;

	private readonly CustomAttributeHelper<T, InchConversionAttribute> _inchAttributeHelper;

	private readonly DataTemplate _jobStatusTemplate;

	public ResourceDictionary GridResources { get; set; }

	public Style DataGridRightAlignStyle { get; set; }

	public List<T> SelectedItems { get; set; }

	public DataGridManager(DataGrid grid, List<CppConfigurationLineInfo> configuration, Style rightAlignStyle, Action<List<T>> selectionChangedHandler, Action<T> htmlViewHandler, DataTemplate template)
	{
		_grid = grid;
		_itemProperties = new List<PropertyInfo>(typeof(T).GetProperties());
		_jobStatusTemplate = template;
		_selectionChangedHandler = selectionChangedHandler;
		_htmlViewHandler = htmlViewHandler;
		_configuration = configuration;
		_inchAttributeHelper = new CustomAttributeHelper<T, InchConversionAttribute>();
		_converter = new MetricToInchConverter();
		DataGridRightAlignStyle = rightAlignStyle;
		_grid = grid;
		_grid.AutoGenerateColumns = false;
		_grid.Background = Brushes.White;
		_grid.SelectionChanged += DataGrid_SelectionChanged;
		_grid.PreviewMouseDoubleClick += DataGrid_PreviewMouseDoubleClick;
		_grid.PreviewMouseLeftButtonUp += DataGrid_PreviewMouseLeftButtonUp;
		_attributeHelper = new CustomAttributeHelper<T, TranslationKeyAttribute>();
		SelectedItems = new List<T>();
		if (GridResources == null)
		{
			GridResources = new ResourceDictionary();
		}
		BuildReference();
		Initialize();
	}

	public void SelectFirst()
	{
		_grid.SelectedIndex = 0;
	}

	public void Select(T item)
	{
		for (int i = 0; i < _grid.Items.Count; i++)
		{
			if (_grid.Items[i] == item)
			{
				_grid.SelectedIndex = i;
				_grid.ScrollIntoView(item);
				break;
			}
		}
	}

	private void BuildReference()
	{
		foreach (CppConfigurationLineInfo cpp in _configuration)
		{
			PropertyInfo propertyInfo = _itemProperties.Find((PropertyInfo x) => x.Name == cpp.Key);
			if (propertyInfo != null)
			{
				cpp.Property = propertyInfo;
				cpp.PropertyName = propertyInfo.Name;
				GridResources.Add(propertyInfo.Name, cpp.Caption);
			}
		}
		_grid.Resources.MergedDictionaries.Add(GridResources);
	}

	private void Initialize(bool isIsSelectedVisible = false)
	{
		CustomAttributeHelper<T, BrowsableAttribute> customAttributeHelper = new CustomAttributeHelper<T, BrowsableAttribute>();
		DataGridColumnManager columnsManager = new DataGridColumnManager(DataGridRightAlignStyle);
		if (EnumerableHelper.IsNullOrEmpty(_configuration))
		{
			return;
		}
		if (isIsSelectedVisible)
		{
			CppConfigurationLineInfo cppConfigurationLineInfo = new CppConfigurationLineInfo();
			string text2 = (cppConfigurationLineInfo.Caption = "IsSelected");
			string text4 = (cppConfigurationLineInfo.PropertyName = text2);
			string key = (cppConfigurationLineInfo.PnKey = text4);
			cppConfigurationLineInfo.Key = key;
			cppConfigurationLineInfo.Property = _itemProperties.ToList().Find((PropertyInfo x) => x.Name == "IsSelected");
			cppConfigurationLineInfo.Visibility = 1;
			DataGridBoundColumn dataGridBoundColumn = CreateGridColumn(columnsManager, cppConfigurationLineInfo);
			dataGridBoundColumn.MinWidth = 40.0;
			dataGridBoundColumn.Width = new DataGridLength(40.0, DataGridLengthUnitType.Pixel);
		}
		foreach (CppConfigurationLineInfo item2 in _configuration)
		{
			bool flag = true;
			if (!(item2.Property != null))
			{
				continue;
			}
			BrowsableAttribute browsableAttribute = customAttributeHelper.FindAttribute(item2.Property);
			if (browsableAttribute != null)
			{
				flag = browsableAttribute.Browsable;
			}
			if (!flag && item2.Visibility > 0)
			{
				flag = true;
			}
			if (item2.PropertyName == "JOB_PROGRESS")
			{
				TextBlock textBlock = new TextBlock();
				textBlock.SetResourceReference(TextBlock.TextProperty, item2.PropertyName);
				DataGridTemplateColumn item = new DataGridTemplateColumn
				{
					CellTemplate = _jobStatusTemplate,
					Header = textBlock,
					Width = new DataGridLength(item2.Width, DataGridLengthUnitType.Pixel)
				};
				_grid.Columns.Add(item);
				continue;
			}
			DataGridBoundColumn dataGridBoundColumn2 = CreateGridColumn(columnsManager, item2);
			if (_inchAttributeHelper.FindAttribute(item2.PropertyName) != null && SystemConfiguration.UseInch)
			{
				if (_converter == null)
				{
					_converter = new MetricToInchConverter();
				}
				if (dataGridBoundColumn2.Binding is Binding binding)
				{
					binding.Converter = _converter;
				}
			}
			if (!flag)
			{
				dataGridBoundColumn2.Visibility = Visibility.Hidden;
			}
		}
	}

	private DataGridBoundColumn CreateGridColumn(DataGridColumnManager columnsManager, CppConfigurationLineInfo cpp)
	{
		DataGridBoundColumn dataGridBoundColumn = columnsManager.CreateColumn(cpp);
		dataGridBoundColumn.Width = new DataGridLength(cpp.Width, DataGridLengthUnitType.Pixel);
		if (cpp.Visibility == 0)
		{
			dataGridBoundColumn.Visibility = Visibility.Hidden;
		}
		cpp.BoundColumn = dataGridBoundColumn;
		_grid.Columns.Add(dataGridBoundColumn);
		return dataGridBoundColumn;
	}

	private void DataGrid_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
	{
		if (_grid.SelectedItems.Count != 0 && _grid.SelectedItem != null && !(_grid.SelectedItem.GetType() != typeof(T)))
		{
			IInputElement inputElement = _grid.InputHitTest(e.GetPosition(_grid));
			if (inputElement != null && WpfVisualHelper.FindVisualParent<DataGridRow>(inputElement as UIElement) != null)
			{
				_htmlViewHandler?.Invoke(_grid.SelectedItem as T);
			}
		}
	}

	private void DataGrid_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (_grid.SelectedItems.Count == 0 || _grid.SelectedItem == null || _grid.SelectedItem.GetType() != typeof(T))
		{
			return;
		}
		SelectedItems.Clear();
		foreach (T selectedItem in _grid.SelectedItems)
		{
			SelectedItems.Add(selectedItem);
		}
		_selectionChangedHandler?.Invoke(SelectedItems);
	}

	private void DataGrid_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		_selectionChangedHandler?.Invoke(SelectedItems);
	}
}
