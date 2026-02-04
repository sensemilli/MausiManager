using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Markup;

namespace WiCAM.Pn4000.pn4.pn4UILib;

public partial class PartPaneWindow : Window, IComponentConnector
{
	private const int GWL_STYLE = -16;

	private const int WS_SYSMENU = 524288;

	[DllImport("user32.dll", SetLastError = true)]
	private static extern int GetWindowLong(nint hWnd, int nIndex);

	[DllImport("user32.dll")]
	private static extern int SetWindowLong(nint hWnd, int nIndex, int dwNewLong);

	public PartPaneWindow()
	{
		InitializeComponent();
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		nint handle = new WindowInteropHelper(this).Handle;
		SetWindowLong(handle, -16, GetWindowLong(handle, -16) & -524289);
	}

	private void Window_Closing(object sender, CancelEventArgs e)
	{
		e.Cancel = true;
	}
}
