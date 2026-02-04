using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace WiCAM.Pn4000.PN3D.Popup.View;

public partial class PopupCheckConfigChangedView : Window, IComponentConnector
{
	public PopupCheckConfigChangedView()
	{
		this.InitializeComponent();
	}

	private void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
	{
		base.DragMove();
	}
}
