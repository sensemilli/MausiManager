using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace WiCAM.Pn4000.JobManager;

public partial class PartRejectWindow : Window, IDialogView, IView, IComponentConnector
{
	public PartRejectWindow()
	{
		InitializeComponent();
	}

	private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
	{
		Regex regex = new Regex("[^0-9]+");
		e.Handled = regex.IsMatch(e.Text);
	}

    [SpecialName]
    bool? IDialogView.DialogResult()
    {
        return this.DialogResult;
    }


    [SpecialName]
    void IDialogView.DialogResult(bool? value)
    {
        this.DialogResult = value;
    }

    bool? IDialogView.ShowDialog()
	{
		return ShowDialog();
	}

	void IDialogView.Close()
	{
		Close();
	}

    [SpecialName]
    object IView.DataContext()
    {
        return this.DataContext;
    }

    [SpecialName]
    void IView.DataContext(object value)
    {
        this.DataContext = value;
    }
}
