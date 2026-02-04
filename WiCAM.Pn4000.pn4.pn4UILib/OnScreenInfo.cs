using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using WiCAM.Pn4000.pn4.Interfaces;
using WiCAM.Pn4000.Screen;

namespace WiCAM.Pn4000.pn4.pn4UILib;

public partial class OnScreenInfo : Window, IScreenAddOn, IComponentConnector
{
	private readonly IScreen2D _screen2D;

	private bool _isIn;

	private Window _mainWindow;

	public OnScreenInfo(IScreen2D screen2D)
	{
		_screen2D = screen2D;
	}

	public OnScreenInfo Init(Window parent)
	{
		_mainWindow = parent;
		InitializeComponent();
		return this;
	}

	public void CalculateLocation()
	{
		if (!_isIn)
		{
			_isIn = true;
			if (!base.IsVisible && DataPanel.Children.Count > 0)
			{
				Show();
			}
			Tuple<int, int, int, int> cornersAtScreen = _screen2D.GetCornersAtScreen();
			if (PresentationSource.FromVisual(_mainWindow) != null)
			{
				base.Left = cornersAtScreen.Item1 + 10;
				base.Top = cornersAtScreen.Item2 + 10;
				_isIn = false;
			}
		}
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		WindowsServices.SetWindowExTransparent(new WindowInteropHelper(this).Handle);
	}

	internal void UpdateString(int id, string v)
	{
		if (DataPanel.Children.Count <= id)
		{
			AddString(v);
			return;
		}
		(DataPanel.Children[id] as TextBlock).Text = v;
		DataPanel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		base.Width = DataPanel.DesiredSize.Width;
		base.Height = DataPanel.DesiredSize.Height;
	}

	internal void AddString(string v)
	{
		TextBlock textBlock = new TextBlock();
		textBlock.Text = v;
		textBlock.Padding = new Thickness(5.0);
		textBlock.FontSize = 14.0;
		textBlock.Foreground = new SolidColorBrush(Colors.Black);
		textBlock.HorizontalAlignment = HorizontalAlignment.Left;
		textBlock.VerticalAlignment = VerticalAlignment.Center;
		DataPanel.Children.Add(textBlock);
		base.Visibility = Visibility.Visible;
		DataPanel.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
		base.Width = DataPanel.DesiredSize.Width;
		base.Height = DataPanel.DesiredSize.Height;
	}

	internal void ClearStrings()
	{
		DataPanel.Children.Clear();
		base.Visibility = Visibility.Hidden;
	}
}
