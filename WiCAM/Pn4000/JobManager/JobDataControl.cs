using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WiCAM.Pn4000.JobManager;

public partial class JobDataControl : UserControl, IView, IComponentConnector
{
	public JobDataControl()
	{
		InitializeComponent();
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
