using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;

namespace WiCAM.Pn4000.pn4.pn4UILib;

public partial class DragDropWindow : Window, IComponentConnector
{
	private Window _mainWindow;

	public const int WS_EX_TRANSPARENT = 32;

	public const int GWL_EXSTYLE = -20;

	public DragDropWindow(Window mainWindow)
	{
		InitializeComponent();
		_mainWindow = mainWindow;
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		label1.Content = string.Empty;
	}

	[DllImport("user32.dll")]
	public static extern int GetWindowLong(nint hwnd, int index);

	[DllImport("user32.dll")]
	public static extern int SetWindowLong(nint hwnd, int index, int newStyle);

	protected override void OnSourceInitialized(EventArgs e)
	{
		base.OnSourceInitialized(e);
		nint handle = new WindowInteropHelper(this).Handle;
		int windowLong = GetWindowLong(handle, -20);
		SetWindowLong(handle, -20, windowLong | 0x20);
	}

	internal void SetImage(ImageSource imageSource)
	{
		image1.Width = base.Width;
		image1.Height = base.Height;
		image1.BeginInit();
		image1.Source = imageSource;
		image1.EndInit();
	}

	internal void SetText(string text)
	{
		label1.Content = text;
	}

	public void SetDropWindowLocation()
	{
		System.Drawing.Point mousePosition = Control.MousePosition;
		System.Windows.Point m = GetM11(mousePosition.X, mousePosition.Y);
		double num = m.X - base.Width / 2.0;
		double num2 = m.Y - base.Height / 2.0;
		if (base.Left != num)
		{
			base.Left = num;
		}
		if (base.Top != num2)
		{
			base.Top = num2;
		}
	}

	private System.Windows.Point GetM11(int x, int y)
	{
		Matrix transformToDevice = PresentationSource.FromVisual(_mainWindow).CompositionTarget.TransformToDevice;
		double m = transformToDevice.M11;
		double m2 = transformToDevice.M22;
		return new System.Windows.Point((double)x / m, (double)y / m2);
	}
}
