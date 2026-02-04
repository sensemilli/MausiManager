using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;

namespace WiCAM.Pn4000.GuiWpf.TabBend.EditFingers;

public partial class EditFingersView : UserControl, IComponentConnector
{
	public EditFingersView()
	{
		InitializeComponent();
	}

	private void UIElement_OnKeyUp(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return && sender is TextBox target)
		{
			BindingOperations.GetBindingExpression(target, TextBox.TextProperty)?.UpdateSource();
		}
	}

	private void UIElement_OnGotFocus(object sender, RoutedEventArgs e)
	{
		if (sender is TextBox textBox)
		{
			textBox.SelectAll();
		}
	}
}
