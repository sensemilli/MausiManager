using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace WiCAM.Pn4000.pn4.pn4UILib.Popup;

public sealed class ListBoxSelector
{
	private sealed class AutoScroller
	{
		private readonly DispatcherTimer _autoScroll = new DispatcherTimer();

		private readonly ItemsControl _itemsControl;

		private readonly ScrollViewer _scrollViewer;

		private readonly ScrollContentPresenter _scrollContent;

		private bool _isEnabled;

		private Point _offset;

		private Point _mouse;

		public bool IsEnabled
		{
			get
			{
				return this._isEnabled;
			}
			set
			{
				if (this._isEnabled != value)
				{
					this._isEnabled = value;
					this._autoScroll.IsEnabled = false;
					this._offset = default(Point);
				}
			}
		}

		public event EventHandler<OffsetChangedEventArgs> OffsetChanged;

		public AutoScroller(ItemsControl itemsControl)
		{
			if (itemsControl == null)
			{
				throw new ArgumentNullException("itemsControl");
			}
			this._itemsControl = itemsControl;
			this._scrollViewer = ListBoxSelector.FindChild<ScrollViewer>(itemsControl);
			this._scrollViewer.ScrollChanged += OnScrollChanged;
			this._scrollContent = ListBoxSelector.FindChild<ScrollContentPresenter>(this._scrollViewer);
			this._autoScroll.Tick += delegate
			{
				this.PreformScroll();
			};
			this._autoScroll.Interval = TimeSpan.FromMilliseconds(AutoScroller.GetRepeatRate(), 0L);
		}

		public Point TranslatePoint(Point point)
		{
			return new Point(point.X - this._offset.X, point.Y - this._offset.Y);
		}

		public void UnRegister()
		{
			this._scrollViewer.ScrollChanged -= OnScrollChanged;
		}

		public void Update(Point mouse)
		{
			this._mouse = mouse;
			if (!this._autoScroll.IsEnabled)
			{
				this.PreformScroll();
			}
		}

		private static int GetRepeatRate()
		{
			return 400 - (int)((double)SystemParameters.KeyboardSpeed * 11.838709677419354);
		}

		private double CalculateOffset(int startIndex, int endIndex)
		{
			double num = 0.0;
			for (int i = startIndex; i != endIndex; i++)
			{
				if (this._itemsControl.ItemContainerGenerator.ContainerFromIndex(i) is FrameworkElement frameworkElement)
				{
					num = num + frameworkElement.ActualHeight + (frameworkElement.Margin.Top + frameworkElement.Margin.Bottom);
				}
			}
			return num;
		}

		private void OnScrollChanged(object sender, ScrollChangedEventArgs e)
		{
			if (this.IsEnabled)
			{
				double horizontalChange = e.HorizontalChange;
				double num = e.VerticalChange;
				if (this._scrollViewer.CanContentScroll)
				{
					num = ((!(e.VerticalChange < 0.0)) ? this.CalculateOffset((int)(e.VerticalOffset - e.VerticalChange), (int)e.VerticalOffset) : (0.0 - this.CalculateOffset((int)e.VerticalOffset, (int)(e.VerticalOffset - e.VerticalChange))));
				}
				this._offset.X += horizontalChange;
				this._offset.Y += num;
				this.OffsetChanged?.Invoke(this, new OffsetChangedEventArgs(horizontalChange, num));
			}
		}

		private void PreformScroll()
		{
			bool isEnabled = false;
			if (this._mouse.X > this._scrollContent.ActualWidth)
			{
				this._scrollViewer.LineRight();
				isEnabled = true;
			}
			else if (this._mouse.X < 0.0)
			{
				this._scrollViewer.LineLeft();
				isEnabled = true;
			}
			if (this._mouse.Y > this._scrollContent.ActualHeight)
			{
				this._scrollViewer.LineDown();
				isEnabled = true;
			}
			else if (this._mouse.Y < 0.0)
			{
				this._scrollViewer.LineUp();
				isEnabled = true;
			}
			this._autoScroll.IsEnabled = isEnabled;
		}
	}

	private sealed class ItemsControlSelector
	{
		private readonly ItemsControl _itemsControl;

		private Rect _previousArea;

		public ItemsControlSelector(ItemsControl itemsControl)
		{
			if (itemsControl == null)
			{
				throw new ArgumentNullException("itemsControl");
			}
			this._itemsControl = itemsControl;
		}

		public void Reset()
		{
			this._previousArea = default(Rect);
		}

		public void Scroll(double x, double y)
		{
			this._previousArea.Offset(0.0 - x, 0.0 - y);
		}

		public void UpdateSelection(Rect area)
		{
			for (int i = 0; i < this._itemsControl.Items.Count; i++)
			{
				if (this._itemsControl.ItemContainerGenerator.ContainerFromIndex(i) is FrameworkElement frameworkElement)
				{
					Point point = frameworkElement.TranslatePoint(new Point(0.0, 0.0), this._itemsControl);
					Rect rect = new Rect(point.X, point.Y, frameworkElement.ActualWidth, frameworkElement.ActualHeight);
					if (rect.IntersectsWith(area))
					{
						Selector.SetIsSelected(frameworkElement, isSelected: true);
					}
					else if (rect.IntersectsWith(this._previousArea))
					{
						Selector.SetIsSelected(frameworkElement, isSelected: false);
					}
				}
			}
			this._previousArea = area;
		}
	}

	private sealed class OffsetChangedEventArgs : EventArgs
	{
		private readonly double _horizontal;

		private readonly double _vertical;

		public double HorizontalChange => this._horizontal;

		public double VerticalChange => this._vertical;

		internal OffsetChangedEventArgs(double horizontal, double vertical)
		{
			this._horizontal = horizontal;
			this._vertical = vertical;
		}
	}

	private sealed class SelectionAdorner : Adorner
	{
		private Rect _selectionRect;

		public Rect SelectionArea
		{
			get
			{
				return this._selectionRect;
			}
			set
			{
				this._selectionRect = value;
				base.InvalidateVisual();
			}
		}

		public SelectionAdorner(UIElement parent)
			: base(parent)
		{
			base.IsHitTestVisible = false;
			base.IsEnabledChanged += delegate
			{
				base.InvalidateVisual();
			};
		}

		protected override void OnRender(DrawingContext drawingContext)
		{
			base.OnRender(drawingContext);
			if (base.IsEnabled)
			{
				drawingContext.PushGuidelineSet(new GuidelineSet(new double[2]
				{
					this.SelectionArea.Left + 0.5,
					this.SelectionArea.Right + 0.5
				}, new double[2]
				{
					this.SelectionArea.Top + 0.5,
					this.SelectionArea.Bottom + 0.5
				}));
				Brush brush = SystemColors.HighlightBrush.Clone();
				brush.Opacity = 0.4;
				drawingContext.DrawRectangle(brush, new Pen(SystemColors.HighlightBrush, 1.0), this.SelectionArea);
			}
		}
	}

	public static readonly DependencyProperty EnabledProperty = DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(ListBoxSelector), new UIPropertyMetadata(false, IsEnabledChangedCallback));

	private static readonly Dictionary<ListBox, ListBoxSelector> AttachedControls = new Dictionary<ListBox, ListBoxSelector>();

	private readonly ListBox _listBox;

	private ScrollContentPresenter _scrollContent;

	private SelectionAdorner _selectionRect;

	private AutoScroller _autoScroller;

	private ItemsControlSelector _selector;

	private bool _mouseCaptured;

	private Point _start;

	private Point _end;

	public static bool GetIt = false;

	private MouseButtonEventArgs _eMem;

	private ListBoxSelector(ListBox listBox)
	{
		this._listBox = listBox;
		if (this._listBox.IsLoaded)
		{
			this.Register();
		}
		else
		{
			this._listBox.Loaded += OnListBoxLoaded;
		}
	}

	public static bool GetEnabled(DependencyObject obj)
	{
		return (bool)obj.GetValue(ListBoxSelector.EnabledProperty);
	}

	public static void SetEnabled(DependencyObject obj, bool value)
	{
		obj.SetValue(ListBoxSelector.EnabledProperty, value);
	}

	private static void IsEnabledChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
	{
		ListBoxSelector.GetIt = false;
		if (!(d is ListBox listBox))
		{
			return;
		}
		ListBoxSelector value;
		if ((bool)e.NewValue)
		{
			if (listBox.SelectionMode == SelectionMode.Single)
			{
				listBox.SelectionMode = SelectionMode.Extended;
			}
			ListBoxSelector.AttachedControls.Add(listBox, new ListBoxSelector(listBox));
		}
		else if (ListBoxSelector.AttachedControls.TryGetValue(listBox, out value))
		{
			ListBoxSelector.AttachedControls.Remove(listBox);
			value.UnRegister();
		}
	}

	private static T FindChild<T>(DependencyObject reference) where T : class
	{
		Queue<DependencyObject> queue = new Queue<DependencyObject>();
		queue.Enqueue(reference);
		while (queue.Count > 0)
		{
			DependencyObject dependencyObject = queue.Dequeue();
			if (dependencyObject is T result)
			{
				return result;
			}
			for (int i = 0; i < VisualTreeHelper.GetChildrenCount(dependencyObject); i++)
			{
				queue.Enqueue(VisualTreeHelper.GetChild(dependencyObject, i));
			}
		}
		return null;
	}

	private bool Register()
	{
		this._scrollContent = ListBoxSelector.FindChild<ScrollContentPresenter>(this._listBox);
		if (this._scrollContent != null)
		{
			this._autoScroller = new AutoScroller(this._listBox);
			this._autoScroller.OffsetChanged += OnOffsetChanged;
			this._selectionRect = new SelectionAdorner(this._scrollContent);
			this._scrollContent.AdornerLayer.Add(this._selectionRect);
			this._selector = new ItemsControlSelector(this._listBox);
			this._listBox.PreviewMouseLeftButtonDown += OnPreviewMouseLeftButtonDown;
			this._listBox.PreviewMouseLeftButtonUp += OnPreviewMouseLeftButtonUp;
			this._listBox.MouseLeftButtonUp += OnMouseLeftButtonUp;
			this._listBox.MouseMove += OnMouseMove;
		}
		return this._scrollContent != null;
	}

	private void UnRegister()
	{
		this.StopSelection();
		this._listBox.PreviewMouseLeftButtonDown -= OnPreviewMouseLeftButtonDown;
		this._listBox.MouseLeftButtonUp -= OnMouseLeftButtonUp;
		this._listBox.MouseMove -= OnMouseMove;
		this._listBox.PreviewMouseLeftButtonUp -= OnPreviewMouseLeftButtonUp;
		this._autoScroller.UnRegister();
	}

	private void OnListBoxLoaded(object sender, EventArgs e)
	{
		if (this.Register())
		{
			this._listBox.Loaded -= OnListBoxLoaded;
		}
	}

	private void OnOffsetChanged(object sender, OffsetChangedEventArgs e)
	{
		this._selector.Scroll(e.HorizontalChange, e.VerticalChange);
		this.UpdateSelection();
	}

	private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		ListBoxSelector.GetIt = false;
		if (this._mouseCaptured)
		{
			this._mouseCaptured = false;
			this._scrollContent.ReleaseMouseCapture();
			this.StopSelection();
		}
	}

	private void OnMouseMove(object sender, MouseEventArgs e)
	{
		if (ListBoxSelector.GetIt)
		{
			ListBoxSelector.GetIt = false;
			Point position = e.GetPosition(this._scrollContent);
			if (position.X >= 0.0 && position.X < this._scrollContent.ActualWidth && position.Y >= 0.0 && position.Y < this._scrollContent.ActualHeight)
			{
				this._mouseCaptured = this.TryCaptureMouse(this._eMem);
				if (this._mouseCaptured)
				{
					this.StartSelection(position);
				}
			}
		}
		if (this._mouseCaptured)
		{
			this._end = e.GetPosition(this._scrollContent);
			this._autoScroller.Update(this._end);
			this.UpdateSelection();
		}
	}

	private void OnPreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		this._eMem = e;
		ListBoxSelector.GetIt = true;
	}

	private void OnPreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
	{
		ListBoxSelector.GetIt = false;
	}

	private bool TryCaptureMouse(MouseButtonEventArgs e)
	{
		Point position = e.GetPosition(this._scrollContent);
		if (this._scrollContent.InputHitTest(position) is UIElement uIElement)
		{
			MouseButtonEventArgs mouseButtonEventArgs = new MouseButtonEventArgs(e.MouseDevice, e.Timestamp, MouseButton.Left, e.StylusDevice);
			mouseButtonEventArgs.RoutedEvent = Mouse.MouseDownEvent;
			mouseButtonEventArgs.Source = e.Source;
			uIElement.RaiseEvent(mouseButtonEventArgs);
			if (Mouse.Captured != this._listBox)
			{
				return false;
			}
		}
		return this._scrollContent.CaptureMouse();
	}

	private void StopSelection()
	{
		ListBoxSelector.GetIt = false;
		this._selectionRect.IsEnabled = false;
		this._autoScroller.IsEnabled = false;
	}

	private void StartSelection(Point location)
	{
		this._listBox.Focus();
		this._start = location;
		this._end = location;
		if ((Keyboard.Modifiers & ModifierKeys.Control) == 0 && (Keyboard.Modifiers & ModifierKeys.Shift) == 0)
		{
			this._listBox.SelectedItems.Clear();
		}
		this._selector.Reset();
		this.UpdateSelection();
		this._selectionRect.IsEnabled = true;
		this._autoScroller.IsEnabled = true;
	}

	private void UpdateSelection()
	{
		Point point = this._autoScroller.TranslatePoint(this._start);
		Rect selectionArea = new Rect(Math.Min(point.X, this._end.X), Math.Min(point.Y, this._end.Y), Math.Abs(this._end.X - point.X), Math.Abs(this._end.Y - point.Y));
		this._selectionRect.SelectionArea = selectionArea;
		Point point2 = this._scrollContent.TranslatePoint(selectionArea.TopLeft, this._listBox);
		Point point3 = this._scrollContent.TranslatePoint(selectionArea.BottomRight, this._listBox);
		this._selector.UpdateSelection(new Rect(point2, point3));
	}
}
