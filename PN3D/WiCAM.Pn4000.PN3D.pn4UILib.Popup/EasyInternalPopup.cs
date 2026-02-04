using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Shapes;

namespace WiCAM.Pn4000.PN3D.pn4UILib.Popup;

public partial class EasyInternalPopup : Window, IComponentConnector
{
	public Dictionary<object, object> Answer { get; set; }

	public EasyInternalPopup()
	{
		this.InitializeComponent();
		this.Answer = new Dictionary<object, object>();
	}

	internal void AddDoubleEdit(string label, double InitValue)
	{
		Grid grid = new Grid();
		ColumnDefinition value = new ColumnDefinition();
		ColumnDefinition value2 = new ColumnDefinition();
		grid.ColumnDefinitions.Add(value);
		grid.ColumnDefinitions.Add(value2);
		Label label2 = new Label();
		label2.Content = label;
		label2.VerticalAlignment = VerticalAlignment.Center;
		Grid.SetColumn(label2, 0);
		grid.Children.Add(label2);
		TextBox textBox = new TextBox();
		textBox.Text = InitValue.ToString(CultureInfo.InvariantCulture);
		textBox.VerticalAlignment = VerticalAlignment.Center;
		textBox.VerticalContentAlignment = VerticalAlignment.Center;
		textBox.Height = 25.0;
		textBox.PreviewTextInput += Box_PreviewTextInput;
		textBox.TextChanged += Box_TextChanged;
		textBox.LostFocus += Box_LostFocus;
		Grid.SetColumn(textBox, 1);
		grid.Children.Add(textBox);
		this.StackPanelControls.Children.Add(grid);
		this.Answer.Add(textBox, InitValue);
	}

	private void Box_LostFocus(object sender, RoutedEventArgs e)
	{
		TextBox textBox = sender as TextBox;
		if (!double.TryParse(textBox.Text.Trim().Replace(",", "."), NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
		{
			result = (double)this.Answer[textBox];
		}
		textBox.Text = result.ToString(CultureInfo.InvariantCulture);
		this.Answer[textBox] = result;
	}

	private void Box_TextChanged(object sender, TextChangedEventArgs e)
	{
		this.OkButton.IsEnabled = true;
	}

	private void Box_PreviewTextInput(object sender, TextCompositionEventArgs e)
	{
		if ("1234567890.,".IndexOf(e.Text) == -1)
		{
			e.Handled = true;
		}
	}

	internal void AddCheckBox(string label, bool InitValue)
	{
		CheckBox checkBox = new CheckBox();
		checkBox.Content = label;
		checkBox.IsChecked = InitValue;
		checkBox.Margin = new Thickness(5.0);
		checkBox.Click += Box_Click;
		this.StackPanelControls.Children.Add(checkBox);
		this.Answer.Add(checkBox, InitValue);
	}

	internal void AddTextToListBox(string text)
	{
		this.ListBoxControl.Items.Add(text);
	}

	private void Box_Click(object sender, RoutedEventArgs e)
	{
		this.Answer[sender] = (sender as CheckBox).IsChecked.Value;
		this.OkButton.IsEnabled = true;
	}

	internal void AddEmptySpace(double v)
	{
		Rectangle rectangle = new Rectangle();
		rectangle.Width = 10.0;
		rectangle.Height = v;
		this.StackPanelControls.Children.Add(rectangle);
	}

	internal void AddSectionLabel(string v)
	{
		Label label = new Label();
		label.Content = v;
		label.FontWeight = FontWeights.Bold;
		this.StackPanelControls.Children.Add(label);
	}

	private void OkButton_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = true;
	}

	private void CancelButton_Click(object sender, RoutedEventArgs e)
	{
		base.DialogResult = false;
	}

	internal void SetOkModel()
	{
		this.OkButton.IsEnabled = true;
		this.CancelButton.Visibility = Visibility.Collapsed;
	}

	internal void SetListModel()
	{
		this.OkButton.IsEnabled = true;
		this.CancelButton.Visibility = Visibility.Collapsed;
		this.ListBoxControl.Visibility = Visibility.Visible;
		this.StackPanelControls.Visibility = Visibility.Collapsed;
		this.ListBoxControl.MinWidth = 400.0;
		this.ListBoxControl.MinHeight = 200.0;
		this.ListBoxControl.MaxWidth = 800.0;
		this.ListBoxControl.MaxHeight = 600.0;
	}
}
