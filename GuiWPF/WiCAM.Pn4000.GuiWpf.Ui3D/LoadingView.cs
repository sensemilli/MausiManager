using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace WiCAM.Pn4000.GuiWpf.Ui3D;

public partial class LoadingView : UserControl, IComponentConnector
{
	public LoadingView()
	{
		InitializeComponent();
	}

	private void FrameworkElement_OnLoaded(object sender, RoutedEventArgs e)
	{
		((Storyboard)TryFindResource("TimeAnimation")).Begin();
	}
}
