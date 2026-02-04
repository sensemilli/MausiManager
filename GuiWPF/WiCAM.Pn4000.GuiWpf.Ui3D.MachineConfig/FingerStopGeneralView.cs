using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.Ui3D.MachineConfig;

public partial class FingerStopGeneralView : UserControl, IComponentConnector
{
	public FingerStopGeneralView()
	{
		InitializeComponent();
	}

	private void ToggleButton1_OnChecked(object sender, RoutedEventArgs e)
	{
		GridPrefCornerStopChoice.IsEnabled = true;
	}

	private void ToggleButtonCorners0_2_OnChecked(object sender, RoutedEventArgs e)
	{
		GridPrefCornerStopChoice.IsEnabled = false;
	}
}
