using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace WiCAM.Pn4000.GuiWpf.UiBasic;

public class DragDropList : ListView
{
	private readonly DataFormat _format = DataFormats.GetDataFormat("DragDropItemsControl");

	private Point _initialMousePosition;

	private Vector _initialMouseOffset;

	private object _draggedData;

	private DraggedAdorner _draggedAdorner;

	private InsertionAdorner _insertionAdorner;

	private Window _topWindow;

	private ItemsControl _sourceItemsControl;

	private FrameworkElement _sourceItemContainer;

	private ItemsControl _targetItemsControl;

	private FrameworkElement _targetItemContainer;

	private bool _hasVerticalOrientation;

	private int _insertionIndex;

	private bool _isInFirstHalf;

	private double _scrollHorizontalOffset;

	private double _scrollVerticalOffset;

	private double _targetTopMargin;

	private double _targetLeftMargin;

	private static DragDropList _instance;

	public static readonly DependencyProperty IsDragSourceProperty = DependencyProperty.RegisterAttached("IsDragSource", typeof(bool), typeof(DragDropList), new UIPropertyMetadata(false, IsDragSourceChanged));

	public static readonly DependencyProperty IsDropTargetProperty = DependencyProperty.RegisterAttached("IsDropTarget", typeof(bool), typeof(DragDropList), new UIPropertyMetadata(false, IsDropTargetChanged));

	public static readonly DependencyProperty DragDropTemplateProperty = DependencyProperty.RegisterAttached("DragDropTemplate", typeof(DataTemplate), typeof(DragDropList), new UIPropertyMetadata(null));

	public static readonly DependencyProperty OrderChangedProperty = DependencyProperty.Register("OrderChangedCommand", typeof(ICommand), typeof(DragDropList));

	public static readonly DependencyProperty DragSourceCanDragItemProperty = DependencyProperty.Register("DragSourceCanDragItem", typeof(Func<object, bool>), typeof(DragDropList), new PropertyMetadata(null));

	private static DragDropList Instance => _instance ?? (_instance = new DragDropList());

	public ICommand OrderChangedCommand
	{
		get
		{
			return (ICommand)GetValue(OrderChangedProperty);
		}
		set
		{
			SetValue(OrderChangedProperty, value);
		}
	}

	public Func<object, bool> DragSourceCanDragItem
	{
		get
		{
			return (Func<object, bool>)GetValue(DragSourceCanDragItemProperty);
		}
		set
		{
			SetValue(DragSourceCanDragItemProperty, value);
		}
	}

	public static bool GetIsDragSource(DependencyObject obj)
	{
		return (bool)obj.GetValue(IsDragSourceProperty);
	}

	public static void SetIsDragSource(DependencyObject obj, bool value)
	{
		obj.SetValue(IsDragSourceProperty, value);
	}

	public static bool GetIsDropTarget(DependencyObject obj)
	{
		return (bool)obj.GetValue(IsDropTargetProperty);
	}

	public static void SetIsDropTarget(DependencyObject obj, bool value)
	{
		obj.SetValue(IsDropTargetProperty, value);
	}

	public static DataTemplate GetDragDropTemplate(DependencyObject obj)
	{
		return (DataTemplate)obj.GetValue(DragDropTemplateProperty);
	}

	public static void SetDragDropTemplate(DependencyObject obj, DataTemplate value)
	{
		obj.SetValue(DragDropTemplateProperty, value);
	}

	private static void IsDragSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
	{
		if (obj is ItemsControl itemsControl)
		{
			if (object.Equals(e.NewValue, true))
			{
				itemsControl.PreviewMouseLeftButtonDown += Instance.DragSource_PreviewMouseLeftButtonDown;
				itemsControl.PreviewMouseLeftButtonUp += Instance.DragSource_PreviewMouseLeftButtonUp;
				itemsControl.PreviewMouseMove += Instance.DragSource_PreviewMouseMove;
			}
			else
			{
				itemsControl.PreviewMouseLeftButtonDown -= Instance.DragSource_PreviewMouseLeftButtonDown;
				itemsControl.PreviewMouseLeftButtonUp -= Instance.DragSource_PreviewMouseLeftButtonUp;
				itemsControl.PreviewMouseMove -= Instance.DragSource_PreviewMouseMove;
			}
		}
	}

	private static void IsDropTargetChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
	{
		if (obj is ItemsControl itemsControl)
		{
			if (object.Equals(e.NewValue, true))
			{
				itemsControl.AllowDrop = true;
				itemsControl.PreviewDrop += Instance.DropTarget_PreviewDrop;
				itemsControl.PreviewDragEnter += Instance.DropTarget_PreviewDragEnter;
				itemsControl.PreviewDragOver += Instance.DropTarget_PreviewDragOver;
				itemsControl.PreviewDragLeave += Instance.DropTarget_PreviewDragLeave;
			}
			else
			{
				itemsControl.AllowDrop = false;
				itemsControl.PreviewDrop -= Instance.DropTarget_PreviewDrop;
				itemsControl.PreviewDragEnter -= Instance.DropTarget_PreviewDragEnter;
				itemsControl.PreviewDragOver -= Instance.DropTarget_PreviewDragOver;
				itemsControl.PreviewDragLeave -= Instance.DropTarget_PreviewDragLeave;
			}
		}
	}

	private void DragSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		_sourceItemsControl = (ItemsControl)sender;
		Visual element = e.OriginalSource as Visual;
		_topWindow = Window.GetWindow(_sourceItemsControl);
		_initialMousePosition = e.GetPosition(_topWindow);
		_sourceItemContainer = _sourceItemsControl.ContainerFromElement(element) as FrameworkElement;
		if (_sourceItemContainer != null)
		{
			object dataContext = _sourceItemContainer.DataContext;
			DragDropList dragDropList = (DragDropList)sender;
			if (dragDropList.DragSourceCanDragItem == null || dragDropList.DragSourceCanDragItem(dataContext))
			{
				_draggedData = dataContext;
			}
		}
	}

	private void DragSource_PreviewMouseMove(object sender, MouseEventArgs e)
	{
		if (_draggedData != null && IsMovementBigEnough(_initialMousePosition, e.GetPosition(_topWindow)))
		{
			_initialMouseOffset = _initialMousePosition - _sourceItemContainer.TranslatePoint(new Point(0.0, 0.0), _topWindow);
			DataObject data = new DataObject(_format.Name, _draggedData);
			bool allowDrop = _topWindow.AllowDrop;
			_topWindow.AllowDrop = true;
			_topWindow.DragEnter += TopWindow_DragEnter;
			_topWindow.DragOver += TopWindow_DragOver;
			_topWindow.DragLeave += TopWindow_DragLeave;
			DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Move);
			RemoveDraggedAdorner();
			_topWindow.AllowDrop = allowDrop;
			_topWindow.DragEnter -= TopWindow_DragEnter;
			_topWindow.DragOver -= TopWindow_DragOver;
			_topWindow.DragLeave -= TopWindow_DragLeave;
			_draggedData = null;
		}
	}

	private void DragSource_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		_draggedData = null;
	}

	private void DropTarget_PreviewDragEnter(object sender, DragEventArgs e)
	{
		_targetItemsControl = (ItemsControl)sender;
		object data = e.Data.GetData(_format.Name);
		DecideDropTarget(e);
		if (data != null)
		{
			Point position = e.GetPosition(_topWindow);
			ShowDraggedAdorner(position);
			CreateInsertionAdorner();
		}
		e.Handled = true;
	}

	private void DropTarget_PreviewDragOver(object sender, DragEventArgs e)
	{
		object data = e.Data.GetData(_format.Name);
		DecideDropTarget(e);
		if (data != null)
		{
			Point position = e.GetPosition(_topWindow);
			ShowDraggedAdorner(position);
			UpdateInsertionAdornerPosition();
		}
		e.Handled = true;
	}

	private void DropTarget_PreviewDrop(object sender, DragEventArgs e)
	{
		object data = e.Data.GetData(_format.Name);
		int num = -1;
		if (data != null)
		{
			if ((e.Effects & DragDropEffects.Move) != 0)
			{
				num = RemoveItemFromItemsControl(_sourceItemsControl, data);
			}
			if (num != -1 && _sourceItemsControl == _targetItemsControl && num < _insertionIndex)
			{
				_insertionIndex--;
			}
			InsertItemInItemsControl(_targetItemsControl, data, _insertionIndex);
			RemoveDraggedAdorner();
			RemoveInsertionAdorner();
		}
		e.Handled = true;
	}

	private void DropTarget_PreviewDragLeave(object sender, DragEventArgs e)
	{
		if (e.Data.GetData(_format.Name) != null)
		{
			RemoveInsertionAdorner();
		}
		e.Handled = true;
	}

	private void DecideDropTarget(DragEventArgs e)
	{
		int count = _targetItemsControl.Items.Count;
		object data = e.Data.GetData(_format.Name);
		if (IsDropDataTypeAllowed(data))
		{
			if (count > 0)
			{
				_hasVerticalOrientation = HasVerticalOrientation(_targetItemsControl.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement);
				_targetItemContainer = _targetItemsControl.ContainerFromElement((DependencyObject)e.OriginalSource) as FrameworkElement;
				if (_targetItemContainer != null)
				{
					Point position = e.GetPosition(_targetItemContainer);
					_isInFirstHalf = IsInFirstHalf(_targetItemContainer, position, _hasVerticalOrientation);
					_insertionIndex = _targetItemsControl.ItemContainerGenerator.IndexFromContainer(_targetItemContainer);
					if (!_isInFirstHalf)
					{
						_insertionIndex++;
					}
				}
				else
				{
					_targetItemContainer = _targetItemsControl.ItemContainerGenerator.ContainerFromIndex(count - 1) as FrameworkElement;
					_isInFirstHalf = false;
					_insertionIndex = count;
				}
			}
			else
			{
				_targetItemContainer = null;
				_insertionIndex = 0;
			}
		}
		else
		{
			_targetItemContainer = null;
			_insertionIndex = -1;
			e.Effects = DragDropEffects.None;
		}
	}

	private bool IsDropDataTypeAllowed(object draggedItem)
	{
		IEnumerable itemsSource = _targetItemsControl.ItemsSource;
		if (draggedItem != null)
		{
			if (itemsSource != null)
			{
				Type type = draggedItem.GetType();
				Type type2 = itemsSource.GetType();
				Type @interface = type2.GetInterface("IList`1");
				if (@interface != null)
				{
					return @interface.GetGenericArguments()[0].IsAssignableFrom(type);
				}
				if (typeof(IList).IsAssignableFrom(type2))
				{
					return true;
				}
				return false;
			}
			return true;
		}
		return false;
	}

	private void TopWindow_DragEnter(object sender, DragEventArgs e)
	{
		ShowDraggedAdorner(e.GetPosition(_topWindow));
		e.Effects = DragDropEffects.None;
		e.Handled = true;
	}

	private void TopWindow_DragOver(object sender, DragEventArgs e)
	{
		ShowDraggedAdorner(e.GetPosition(_topWindow));
		e.Effects = DragDropEffects.None;
		e.Handled = true;
	}

	private void TopWindow_DragLeave(object sender, DragEventArgs e)
	{
		RemoveDraggedAdorner();
		e.Handled = true;
	}

	private void ShowDraggedAdorner(Point currentPosition)
	{
		if (_draggedAdorner == null)
		{
			AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_sourceItemsControl);
			_draggedAdorner = new DraggedAdorner(_draggedData, GetDragDropTemplate(_sourceItemsControl), _sourceItemContainer, adornerLayer);
		}
		double left = currentPosition.X - _initialMousePosition.X + _initialMouseOffset.X;
		double top = currentPosition.Y - _initialMousePosition.Y + _initialMouseOffset.Y;
		_draggedAdorner.SetPosition(left, top);
	}

	private void RemoveDraggedAdorner()
	{
		if (_draggedAdorner != null)
		{
			_draggedAdorner.Detach();
			_draggedAdorner = null;
		}
	}

	private void CreateInsertionAdorner()
	{
		if (_targetItemContainer != null)
		{
			AdornerLayer adornerLayer = AdornerLayer.GetAdornerLayer(_targetItemContainer);
			_insertionAdorner = new InsertionAdorner(_hasVerticalOrientation, _isInFirstHalf, _targetItemContainer, adornerLayer);
		}
	}

	private void UpdateInsertionAdornerPosition()
	{
		if (_insertionAdorner != null)
		{
			_insertionAdorner.IsInFirstHalf = _isInFirstHalf;
			_insertionAdorner.InvalidateVisual();
		}
	}

	private void RemoveInsertionAdorner()
	{
		if (_insertionAdorner != null)
		{
			_insertionAdorner.Detach();
			_insertionAdorner = null;
		}
	}

	private static bool HasVerticalOrientation(FrameworkElement itemContainer)
	{
		bool result = true;
		if (itemContainer != null)
		{
			Panel panel = VisualTreeHelper.GetParent(itemContainer) as Panel;
			if (panel is StackPanel stackPanel)
			{
				result = stackPanel.Orientation == Orientation.Vertical;
			}
			else if (panel is WrapPanel wrapPanel)
			{
				result = wrapPanel.Orientation == Orientation.Vertical;
			}
		}
		return result;
	}

	private void InsertItemInItemsControl(ItemsControl itemsControl, object itemToInsert, int insertionIndex)
	{
		if (itemToInsert == null)
		{
			return;
		}
		if (itemsControl.ItemsSource == null)
		{
			if (!itemsControl.Items.Contains(itemToInsert))
			{
				itemsControl.Items.Insert(insertionIndex, itemToInsert);
			}
			return;
		}
		if (itemsControl.ItemsSource is IList)
		{
			if (!((IList)itemsControl.ItemsSource).Contains(itemToInsert))
			{
				((IList)itemsControl.ItemsSource).Insert(insertionIndex, itemToInsert);
				((DragDropList)itemsControl).OrderChangedCommand?.Execute(new DragDropEventArgs(itemsControl, itemToInsert, insertionIndex));
			}
			return;
		}
		Type type = itemsControl.ItemsSource.GetType();
		if (type.GetInterface("IList`1") != null)
		{
			type.GetMethod("Insert").Invoke(itemsControl.ItemsSource, new object[2] { insertionIndex, itemToInsert });
		}
	}

	private static int RemoveItemFromItemsControl(ItemsControl itemsControl, object itemToRemove)
	{
		int num = -1;
		if (itemToRemove != null)
		{
			num = itemsControl.Items.IndexOf(itemToRemove);
			if (num != -1)
			{
				if (itemsControl.ItemsSource == null)
				{
					if (num >= 0 && num < itemsControl.Items.Count)
					{
						itemsControl.Items.RemoveAt(num);
					}
				}
				else if (itemsControl.ItemsSource is IList)
				{
					IList list = (IList)itemsControl.ItemsSource;
					if (num >= 0 && num < list.Count)
					{
						list.RemoveAt(num);
					}
				}
				else
				{
					Type type = itemsControl.ItemsSource.GetType();
					if (type.GetInterface("IList`1") != null)
					{
						type.GetMethod("RemoveAt").Invoke(itemsControl.ItemsSource, new object[1] { num });
					}
				}
			}
		}
		return num;
	}

	private static bool IsInFirstHalf(FrameworkElement container, Point clickedPoint, bool hasVerticalOrientation)
	{
		if (hasVerticalOrientation)
		{
			return clickedPoint.Y < container.ActualHeight / 2.0;
		}
		return clickedPoint.X < container.ActualWidth / 2.0;
	}

	private static bool IsMovementBigEnough(Point initialMousePosition, Point currentPosition)
	{
		if (!(Math.Abs(currentPosition.X - initialMousePosition.X) >= SystemParameters.MinimumHorizontalDragDistance))
		{
			return Math.Abs(currentPosition.Y - initialMousePosition.Y) >= SystemParameters.MinimumVerticalDragDistance;
		}
		return true;
	}
}
