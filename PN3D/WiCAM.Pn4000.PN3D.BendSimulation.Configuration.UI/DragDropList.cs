using System;
using System.Collections;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace WiCAM.Pn4000.PN3D.BendSimulation.Configuration.UI;

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

	private static DragDropList Instance => DragDropList._instance ?? (DragDropList._instance = new DragDropList());

	public ICommand OrderChangedCommand
	{
		get
		{
			return (ICommand)base.GetValue(DragDropList.OrderChangedProperty);
		}
		set
		{
			base.SetValue(DragDropList.OrderChangedProperty, value);
		}
	}

	public Func<object, bool> DragSourceCanDragItem
	{
		get
		{
			return (Func<object, bool>)base.GetValue(DragDropList.DragSourceCanDragItemProperty);
		}
		set
		{
			base.SetValue(DragDropList.DragSourceCanDragItemProperty, value);
		}
	}

	public static bool GetIsDragSource(DependencyObject obj)
	{
		return (bool)obj.GetValue(DragDropList.IsDragSourceProperty);
	}

	public static void SetIsDragSource(DependencyObject obj, bool value)
	{
		obj.SetValue(DragDropList.IsDragSourceProperty, value);
	}

	public static bool GetIsDropTarget(DependencyObject obj)
	{
		return (bool)obj.GetValue(DragDropList.IsDropTargetProperty);
	}

	public static void SetIsDropTarget(DependencyObject obj, bool value)
	{
		obj.SetValue(DragDropList.IsDropTargetProperty, value);
	}

	public static DataTemplate GetDragDropTemplate(DependencyObject obj)
	{
		return (DataTemplate)obj.GetValue(DragDropList.DragDropTemplateProperty);
	}

	public static void SetDragDropTemplate(DependencyObject obj, DataTemplate value)
	{
		obj.SetValue(DragDropList.DragDropTemplateProperty, value);
	}

	private static void IsDragSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
	{
		if (obj is ItemsControl itemsControl)
		{
			if (object.Equals(e.NewValue, true))
			{
				itemsControl.PreviewMouseLeftButtonDown += DragDropList.Instance.DragSource_PreviewMouseLeftButtonDown;
				itemsControl.PreviewMouseLeftButtonUp += DragDropList.Instance.DragSource_PreviewMouseLeftButtonUp;
				itemsControl.PreviewMouseMove += DragDropList.Instance.DragSource_PreviewMouseMove;
			}
			else
			{
				itemsControl.PreviewMouseLeftButtonDown -= DragDropList.Instance.DragSource_PreviewMouseLeftButtonDown;
				itemsControl.PreviewMouseLeftButtonUp -= DragDropList.Instance.DragSource_PreviewMouseLeftButtonUp;
				itemsControl.PreviewMouseMove -= DragDropList.Instance.DragSource_PreviewMouseMove;
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
				itemsControl.PreviewDrop += DragDropList.Instance.DropTarget_PreviewDrop;
				itemsControl.PreviewDragEnter += DragDropList.Instance.DropTarget_PreviewDragEnter;
				itemsControl.PreviewDragOver += DragDropList.Instance.DropTarget_PreviewDragOver;
				itemsControl.PreviewDragLeave += DragDropList.Instance.DropTarget_PreviewDragLeave;
			}
			else
			{
				itemsControl.AllowDrop = false;
				itemsControl.PreviewDrop -= DragDropList.Instance.DropTarget_PreviewDrop;
				itemsControl.PreviewDragEnter -= DragDropList.Instance.DropTarget_PreviewDragEnter;
				itemsControl.PreviewDragOver -= DragDropList.Instance.DropTarget_PreviewDragOver;
				itemsControl.PreviewDragLeave -= DragDropList.Instance.DropTarget_PreviewDragLeave;
			}
		}
	}

	private void DragSource_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		this._sourceItemsControl = (ItemsControl)sender;
		Visual element = e.OriginalSource as Visual;
		this._topWindow = Window.GetWindow(this._sourceItemsControl);
		this._initialMousePosition = e.GetPosition(this._topWindow);
		this._sourceItemContainer = this._sourceItemsControl.ContainerFromElement(element) as FrameworkElement;
		if (this._sourceItemContainer != null)
		{
			object dataContext = this._sourceItemContainer.DataContext;
			DragDropList dragDropList = (DragDropList)sender;
			if (dragDropList.DragSourceCanDragItem == null || dragDropList.DragSourceCanDragItem(dataContext))
			{
				this._draggedData = dataContext;
			}
		}
	}

	private void DragSource_PreviewMouseMove(object sender, MouseEventArgs e)
	{
		if (this._draggedData != null && DragDropList.IsMovementBigEnough(this._initialMousePosition, e.GetPosition(this._topWindow)))
		{
			this._initialMouseOffset = this._initialMousePosition - this._sourceItemContainer.TranslatePoint(new Point(0.0, 0.0), this._topWindow);
			DataObject data = new DataObject(this._format.Name, this._draggedData);
			bool allowDrop = this._topWindow.AllowDrop;
			this._topWindow.AllowDrop = true;
			this._topWindow.DragEnter += TopWindow_DragEnter;
			this._topWindow.DragOver += TopWindow_DragOver;
			this._topWindow.DragLeave += TopWindow_DragLeave;
			DragDrop.DoDragDrop((DependencyObject)sender, data, DragDropEffects.Move);
			this.RemoveDraggedAdorner();
			this._topWindow.AllowDrop = allowDrop;
			this._topWindow.DragEnter -= TopWindow_DragEnter;
			this._topWindow.DragOver -= TopWindow_DragOver;
			this._topWindow.DragLeave -= TopWindow_DragLeave;
			this._draggedData = null;
		}
	}

	private void DragSource_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		this._draggedData = null;
	}

	private void DropTarget_PreviewDragEnter(object sender, DragEventArgs e)
	{
		this._targetItemsControl = (ItemsControl)sender;
		object data = e.Data.GetData(this._format.Name);
		this.DecideDropTarget(e);
		if (data != null)
		{
			this.ShowDraggedAdorner(e.GetPosition(this._topWindow));
			this.CreateInsertionAdorner();
		}
		e.Handled = true;
	}

	private void DropTarget_PreviewDragOver(object sender, DragEventArgs e)
	{
		object data = e.Data.GetData(this._format.Name);
		this.DecideDropTarget(e);
		if (data != null)
		{
			this.ShowDraggedAdorner(e.GetPosition(this._topWindow));
			this.UpdateInsertionAdornerPosition();
		}
		e.Handled = true;
	}

	private void DropTarget_PreviewDrop(object sender, DragEventArgs e)
	{
		object data = e.Data.GetData(this._format.Name);
		int num = -1;
		if (data != null)
		{
			if ((e.Effects & DragDropEffects.Move) != 0)
			{
				num = DragDropList.RemoveItemFromItemsControl(this._sourceItemsControl, data);
			}
			if (num != -1 && this._sourceItemsControl == this._targetItemsControl && num < this._insertionIndex)
			{
				this._insertionIndex--;
			}
			this.InsertItemInItemsControl(this._targetItemsControl, data, this._insertionIndex);
			this.RemoveDraggedAdorner();
			this.RemoveInsertionAdorner();
		}
		e.Handled = true;
	}

	private void DropTarget_PreviewDragLeave(object sender, DragEventArgs e)
	{
		if (e.Data.GetData(this._format.Name) != null)
		{
			this.RemoveInsertionAdorner();
		}
		e.Handled = true;
	}

	private void DecideDropTarget(DragEventArgs e)
	{
		int count = this._targetItemsControl.Items.Count;
		if (this.IsDropDataTypeAllowed(e.Data.GetData(this._format.Name)))
		{
			if (count > 0)
			{
				this._hasVerticalOrientation = DragDropList.HasVerticalOrientation(this._targetItemsControl.ItemContainerGenerator.ContainerFromIndex(0) as FrameworkElement);
				this._targetItemContainer = this._targetItemsControl.ContainerFromElement((DependencyObject)e.OriginalSource) as FrameworkElement;
				if (this._targetItemContainer != null)
				{
					this._isInFirstHalf = DragDropList.IsInFirstHalf(clickedPoint: e.GetPosition(this._targetItemContainer), container: this._targetItemContainer, hasVerticalOrientation: this._hasVerticalOrientation);
					this._insertionIndex = this._targetItemsControl.ItemContainerGenerator.IndexFromContainer(this._targetItemContainer);
					if (!this._isInFirstHalf)
					{
						this._insertionIndex++;
					}
				}
				else
				{
					this._targetItemContainer = this._targetItemsControl.ItemContainerGenerator.ContainerFromIndex(count - 1) as FrameworkElement;
					this._isInFirstHalf = false;
					this._insertionIndex = count;
				}
			}
			else
			{
				this._targetItemContainer = null;
				this._insertionIndex = 0;
			}
		}
		else
		{
			this._targetItemContainer = null;
			this._insertionIndex = -1;
			e.Effects = DragDropEffects.None;
		}
	}

	private bool IsDropDataTypeAllowed(object draggedItem)
	{
		IEnumerable itemsSource = this._targetItemsControl.ItemsSource;
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
		this.ShowDraggedAdorner(e.GetPosition(this._topWindow));
		e.Effects = DragDropEffects.None;
		e.Handled = true;
	}

	private void TopWindow_DragOver(object sender, DragEventArgs e)
	{
		this.ShowDraggedAdorner(e.GetPosition(this._topWindow));
		e.Effects = DragDropEffects.None;
		e.Handled = true;
	}

	private void TopWindow_DragLeave(object sender, DragEventArgs e)
	{
		this.RemoveDraggedAdorner();
		e.Handled = true;
	}

	private void ShowDraggedAdorner(Point currentPosition)
	{
		if (this._draggedAdorner == null)
		{
			this._draggedAdorner = new DraggedAdorner(adornerLayer: AdornerLayer.GetAdornerLayer(this._sourceItemsControl), dragDropData: this._draggedData, dragDropTemplate: DragDropList.GetDragDropTemplate(this._sourceItemsControl), adornedElement: this._sourceItemContainer);
		}
		double left = currentPosition.X - this._initialMousePosition.X + this._initialMouseOffset.X;
		double top = currentPosition.Y - this._initialMousePosition.Y + this._initialMouseOffset.Y;
		this._draggedAdorner.SetPosition(left, top);
	}

	private void RemoveDraggedAdorner()
	{
		if (this._draggedAdorner != null)
		{
			this._draggedAdorner.Detach();
			this._draggedAdorner = null;
		}
	}

	private void CreateInsertionAdorner()
	{
		if (this._targetItemContainer != null)
		{
			this._insertionAdorner = new InsertionAdorner(adornerLayer: AdornerLayer.GetAdornerLayer(this._targetItemContainer), isSeparatorHorizontal: this._hasVerticalOrientation, isInFirstHalf: this._isInFirstHalf, adornedElement: this._targetItemContainer);
		}
	}

	private void UpdateInsertionAdornerPosition()
	{
		if (this._insertionAdorner != null)
		{
			this._insertionAdorner.IsInFirstHalf = this._isInFirstHalf;
			this._insertionAdorner.InvalidateVisual();
		}
	}

	private void RemoveInsertionAdorner()
	{
		if (this._insertionAdorner != null)
		{
			this._insertionAdorner.Detach();
			this._insertionAdorner = null;
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
