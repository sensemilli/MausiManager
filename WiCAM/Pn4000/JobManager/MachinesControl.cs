using System;
using System.CodeDom.Compiler;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WiCAM.Pn4000.JobManager;

public partial class MachinesControl : UserControl, IView, IComponentConnector
{
	public MachinesControl()
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
