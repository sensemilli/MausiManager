using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Markup;

namespace WiCAM.Pn4000.JobManager;

public partial class PlateStornoWindow : Window, IDialogView, IView, IComponentConnector
{
	public PlateStornoWindow()
	{
		InitializeComponent();
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
