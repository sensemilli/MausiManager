using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Ribbon;
using System.Windows.Markup;

namespace WiCAM.Pn4000.JobManager;

public partial class MainWindow : RibbonWindow, IDialogView, IView, IComponentConnector
{
	public MainWindow()
	{
		InitializeComponent();
	}

	private void Ribbon_SelectionChanged(object sender, SelectionChangedEventArgs e)
	{
		if (ribbonTab1.IsSelected)
		{
			gridMain.Visibility = Visibility.Visible;
			gridSettings.Visibility = Visibility.Hidden;
		}
		else if (ribbonTab2.IsSelected)
		{
			gridMain.Visibility = Visibility.Visible;
			gridSettings.Visibility = Visibility.Hidden;
		}
		else if (ribbonTab3.IsSelected)
		{
			gridMain.Visibility = Visibility.Hidden;
			gridSettings.Visibility = Visibility.Visible;
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
