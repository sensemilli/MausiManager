using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Markup;

namespace WiCAM.Pn4000.JobManager;

public partial class JobProductionStrategySelect : Window, IDialogView, IView, IComponentConnector
{
	public JobProductionStrategySelect()
	{
		InitializeComponent();
	}

	private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
	{
		if (e.Key == Key.Return)
		{
			e.Handled = true;
		}
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
