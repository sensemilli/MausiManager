using System.Runtime.CompilerServices;
using System.Windows.Controls;
using System.Windows.Markup;

namespace WiCAM.Pn4000.JobManager;

public partial class FilterControl : UserControl, IView, IComponentConnector
{
	public FilterControl()
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
