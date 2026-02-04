using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using WiCAM.Pn4000.PN3D.Popup.UI.ViewModels;

namespace WiCAM.Pn4000.PN3D.Popup.UI.Views;

public partial class BendSequenceEditorView : UserControl, IComponentConnector
{
	public BendSequenceEditorView()
	{
		this.InitializeComponent();
	}

	private void UserControl_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
	{
		((BendSequenceEditorViewModel)base.DataContext)?.LoadList();
	}
}
