using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace WiCAM.Pn4000.PN3D.Popup.Information;

public partial class ManualAddFunction : Window, IComponentConnector
{
	public string FunctionGroup { get; set; }

	public string FunctionName { get; set; }

	public string Label { get; set; }

	public ManualAddFunction()
	{
		this.InitializeComponent();
		this.grid1.DataContext = this;
	}

	private void button6_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = true;
		base.Close();
	}

	private void textBox3_PreviewTextInput(object sender, TextCompositionEventArgs e)
	{
		e.Handled = new Regex("[^0-9]+").IsMatch(e.Text);
	}
}
