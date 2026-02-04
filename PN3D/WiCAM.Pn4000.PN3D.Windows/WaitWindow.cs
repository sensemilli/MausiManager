using System.Windows;
using System.Windows.Markup;
using System.Windows.Media.Animation;

namespace WiCAM.Pn4000.PN3D.Windows;

public partial class WaitWindow : Window, IComponentConnector
{
	public WaitWindow()
	{
		this.InitializeComponent();
	}

	private void Window_Loaded(object sender, RoutedEventArgs e)
	{
		((Storyboard)base.TryFindResource("TimeAnimation")).Begin();
	}
}
