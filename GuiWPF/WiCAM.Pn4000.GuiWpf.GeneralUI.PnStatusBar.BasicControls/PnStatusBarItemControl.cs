using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;

namespace WiCAM.Pn4000.GuiWpf.GeneralUI.PnStatusBar.BasicControls;

public partial class PnStatusBarItemControl : UserControl, IComponentConnector
{
	public static readonly DependencyProperty LabelProperty = DependencyProperty.Register("Label", typeof(string), typeof(PnStatusBarItemControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

	public static readonly DependencyProperty InfoProperty = DependencyProperty.Register("Info", typeof(string), typeof(PnStatusBarItemControl), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

	public static readonly DependencyProperty FillBrushProperty = DependencyProperty.Register("FillBrush", typeof(Brush), typeof(PnStatusBarItemControl), new FrameworkPropertyMetadata(WhiteBrush, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));

	private static Brush WhiteBrush { get; set; } = new SolidColorBrush(Colors.White);

	public string Label
	{
		get
		{
			return (string)GetValue(LabelProperty);
		}
		set
		{
			SetValue(LabelProperty, value);
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

	public PnStatusBarItemControl()
	{
		InitializeComponent();
	}
}
