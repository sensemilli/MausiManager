using System.Collections;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Markup;
using System.Windows.Media;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.BasicControls;

public partial class PnStatusBarControl : UserControl, IComponentConnector
{
	public static readonly DependencyProperty ToolTipVisibilityProperty = DependencyProperty.Register("ToolTipVisibility", typeof(bool), typeof(PnStatusBarControl), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

	public static readonly DependencyProperty ItemsProperty = DependencyProperty.Register("Items", typeof(ObservableCollection<FrameworkElement>), typeof(PnStatusBarControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

	public static readonly DependencyProperty InfoProperty = DependencyProperty.Register("Info", typeof(string), typeof(PnStatusBarControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

	public static readonly DependencyProperty FillBrushProperty = DependencyProperty.Register("FillBrush", typeof(Brush), typeof(PnStatusBarControl), new FrameworkPropertyMetadata(WhiteBrush, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

	public static readonly DependencyProperty TextBrushProperty = DependencyProperty.Register("TextBrush", typeof(Brush), typeof(PnStatusBarControl), new FrameworkPropertyMetadata(BlackBrush, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

	public static readonly DependencyProperty ItemsPlacementProperty = DependencyProperty.Register("ItemsPlacement", typeof(PlacementMode), typeof(PnStatusBarControl), new FrameworkPropertyMetadata(PlacementMode.Top, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

	public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register("ItemsSource", typeof(IEnumerable), typeof(PnStatusBarControl), new PropertyMetadata(OnItemsSourcePropertyChanged));

	public static readonly DependencyProperty ItemsTemplateProperty = DependencyProperty.Register("ItemsTemplate", typeof(DataTemplate), typeof(PnStatusBarControl));

	private static Brush WhiteBrush { get; set; } = new SolidColorBrush(Colors.White);

	private static Brush BlackBrush { get; set; } = new SolidColorBrush(Colors.Black);

	public bool ToolTipVisibility
	{
		get
		{
			return (bool)GetValue(ToolTipVisibilityProperty);
		}
		set
		{
			SetValue(ToolTipVisibilityProperty, value);
		}
	}

	public ObservableCollection<FrameworkElement> Items
	{
		get
		{
			return (ObservableCollection<FrameworkElement>)GetValue(ItemsProperty);
		}
		set
		{
			SetValue(ItemsProperty, value);
		}
	}

	public string Info
	{
		get
		{
			return (string)GetValue(InfoProperty);
		}
		set
		{
			SetValue(InfoProperty, value);
		}
	}

	public Brush FillBrush
	{
		get
		{
			return (Brush)GetValue(FillBrushProperty);
		}
		set
		{
			SetValue(FillBrushProperty, value);
		}
	}

	public Brush TextBrush
	{
		get
		{
			return (Brush)GetValue(TextBrushProperty);
		}
		set
		{
			SetValue(TextBrushProperty, value);
		}
	}

	public PlacementMode ItemsPlacement
	{
		get
		{
			return (PlacementMode)GetValue(ItemsPlacementProperty);
		}
		set
		{
			SetValue(ItemsPlacementProperty, value);
		}
	}

	public IEnumerable ItemsSource
	{
		get
		{
			return (IEnumerable)GetValue(ItemsSourceProperty);
		}
		set
		{
			SetValue(ItemsSourceProperty, value);
		}
	}

	public DataTemplate ItemsTemplate
	{
		get
		{
			return (DataTemplate)GetValue(ItemsTemplateProperty);
		}
		set
		{
			SetValue(ItemsTemplateProperty, value);
		}
	}

	public PnStatusBarControl()
	{
		Items = new ObservableCollection<FrameworkElement>();
		Items.CollectionChanged += Items_CollectionChanged;
		InitializeComponent();
	}

	private void Items_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
	{
		ToolTipVisibility = Items.Count > 0;
	}

	private static void OnItemsSourcePropertyChanged(DependencyObject sender, DependencyPropertyChangedEventArgs e)
	{
		if (sender is PnStatusBarControl pnStatusBarControl)
		{
			pnStatusBarControl.OnItemsSourceChanged((IEnumerable)e.OldValue, (IEnumerable)e.NewValue);
		}
	}

	private void OnItemsSourceChanged(IEnumerable oldValue, IEnumerable newValue)
	{
		if (oldValue is INotifyCollectionChanged notifyCollectionChanged)
		{
			notifyCollectionChanged.CollectionChanged -= newValueINotifyCollectionChanged_CollectionChanged;
		}
		if (newValue is INotifyCollectionChanged notifyCollectionChanged2)
		{
			notifyCollectionChanged2.CollectionChanged += newValueINotifyCollectionChanged_CollectionChanged;
			UpdateSubItems();
		}
	}

	private void newValueINotifyCollectionChanged_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
	{
		UpdateSubItems();
	}

	private void UpdateSubItems()
	{
		if (ItemsTemplate == null || !ItemsTemplate.HasContent)
		{
			return;
		}
		Items.Clear();
		foreach (object item in ItemsSource)
		{
			FrameworkElement frameworkElement = (FrameworkElement)ItemsTemplate.LoadContent();
			frameworkElement.DataContext = item;
			Items.Add(frameworkElement);
		}
	}
}
