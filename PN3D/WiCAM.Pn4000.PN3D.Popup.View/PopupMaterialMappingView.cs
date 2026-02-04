using System.Windows;
using System.Windows.Markup;

namespace WiCAM.Pn4000.PN3D.Popup.View;

public partial class PopupMaterialMappingView : Window, IComponentConnector
{
	public bool CancelByUser { get; private set; } = true;

	public PopupMaterialMappingView()
	{
		this.InitializeComponent();
	}

	private void Cancel_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = false;
		base.Close();
	}

	private void Continue_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = true;
		this.CancelByUser = false;
		base.Close();
	}
}
